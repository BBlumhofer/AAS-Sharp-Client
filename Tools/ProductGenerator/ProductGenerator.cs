using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;

public static class ProductGenerator
{
    public static async Task<string> GenerateAsync(string configPath, string outputFolder)
    {
        // resolve config path variants (support repo-root invocation)
        string FindRepoRoot()
        {
            var dir = Directory.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(dir))
            {
                if (File.Exists(Path.Combine(dir, "AAS Sharp Client.slnx")) || File.Exists(Path.Combine(dir, "AAS Sharp Client.csproj"))) return dir;
                var parent = Directory.GetParent(dir);
                if (parent == null) break;
                dir = parent.FullName;
            }
            return Directory.GetCurrentDirectory();
        }

        var repoRoot = FindRepoRoot();
        var possibleConfigPaths = new[] {
            configPath,
            Path.Combine(repoRoot, configPath),
            Path.Combine(repoRoot, "Tools", "ProductGenerator", Path.GetFileName(configPath)),
            Path.Combine(repoRoot, "Tools", "ProductGenerator", "configs", Path.GetFileName(configPath))
        };
        string? resolvedConfig = null;
        foreach (var p in possibleConfigPaths)
        {
            if (File.Exists(p)) { resolvedConfig = p; break; }
        }
        if (resolvedConfig == null) throw new FileNotFoundException("Config not found", configPath);
        var cfgText = await File.ReadAllTextAsync(resolvedConfig);
        var cfg = JsonNode.Parse(cfgText) as JsonObject ?? throw new Exception("Invalid config JSON");

        var shellId = cfg["Id"]?.ToString() ?? throw new Exception("Config missing Id");
        // derive short id from last segment
        var idShort = shellId.Split('/', StringSplitOptions.RemoveEmptyEntries)[^1];

        // load template
        // resolve template location (allow running from repo root)
        var possibleTemplatePaths = new[] {
            Path.Combine(repoRoot, "Tools", "ProductGenerator", "Template", "Cab_A_Blue-environment.json"),
            Path.Combine(repoRoot, "Tools", "ProductGenerator", "Cab_A_Blue-environment.json"),
            Path.Combine("Tools", "ProductGenerator", "Template", "Cab_A_Blue-environment.json"),
            Path.Combine("Template", "Cab_A_Blue-environment.json")
        };
        string? resolvedTemplate = null;
        foreach (var p in possibleTemplatePaths)
        {
            if (File.Exists(p)) { resolvedTemplate = p; break; }
        }
        if (resolvedTemplate == null) throw new FileNotFoundException("Template not found", "Tools/ProductGenerator/Template/Cab_A_Blue-environment.json");
        var templateText = await File.ReadAllTextAsync(resolvedTemplate);
        var root = JsonNode.Parse(templateText) as JsonObject ?? throw new Exception("Invalid template JSON");

        // find capability submodel index
        JsonObject? capabilitySubmodel = null;
        int capIndex = -1;
        if (root["submodels"] is JsonArray subs)
        {
            for (int i = 0; i < subs.Count; i++)
            {
                if (subs[i] is JsonObject s)
                {
                    // check idShort or semanticId for CapabilityDescription
                    if (s.TryGetPropertyValue("idShort", out var idShortNode) && idShortNode?.ToString() == "RequiredCapabilityDescription") { capabilitySubmodel = s; capIndex = i; break; }
                    if (s.TryGetPropertyValue("semanticId", out var sem) && sem is JsonObject semObj)
                    {
                        if (semObj.TryGetPropertyValue("keys", out var keys) && keys is JsonArray kArr && kArr.Count>0)
                        {
                            var first = kArr[0] as JsonObject;
                            if (first != null && first.TryGetPropertyValue("value", out var fv) && fv != null && fv.ToString().Contains("CapabilityDescription"))
                            {
                                capabilitySubmodel = s; capIndex = i; break;
                            }
                        }
                    }
                }
            }
        }

        // Build capability submodel using CapabilityDescription types and apply template
        var capabilityContainers = new List<CapabilityContainerDefinition>();

        if (cfg["Capabilities"] is JsonArray caps)
        {
            foreach (var c in caps)
            {
                if (c is JsonObject co)
                {
                    var name = co["Name"]?.ToString() ?? "Capability";

                    // Build property containers for this capability
                    var propertyContainers = new List<CapabilityPropertyContainerDefinition>();
                    if (co["PropertyContainers"] is JsonObject pcs)
                    {
                        foreach (var kv in pcs)
                        {
                            var key = kv.Key;
                            var val = kv.Value?.ToString() ?? string.Empty;
                            if (string.Equals(key, "ProductId", StringComparison.OrdinalIgnoreCase)) continue;

                            // choose value type heuristically
                            string valueType = "xs:string";
                            if (double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
                            {
                                valueType = "xs:double";
                            }
                            else if (val.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || val.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            {
                                valueType = "xs:anyURI";
                            }

                            var propContainer = new PropertyValueContainerDefinition(key + "Container", key, val, valueType);
                            propertyContainers.Add(propContainer);
                        }
                    }

                    CapabilityPropertySetDefinition? propSetDef = null;
                    if (propertyContainers.Count > 0)
                    {
                        propSetDef = new CapabilityPropertySetDefinition("PropertySet", propertyContainers);
                    }

                    var capElem = new CapabilityElementDefinition(name);
                    var containerDef = new CapabilityContainerDefinition(name + "Container", capElem, null, null, null, null, null, propSetDef, null);
                    capabilityContainers.Add(containerDef);
                }
            }
        }

        var capSetDef = new CapabilitySetDefinition("CapabilitySet", capabilityContainers);
        // Use plain submodels/ UUID path (no 'capability' segment) to match repo conventions
        var templateIdentifier = $"https://smartfactory.de/submodels/{Guid.NewGuid()}-{idShort}";
        // Provide the desired submodel idShort here so Apply(...) does not overwrite it
        var desiredIdShort = "RequiredCapabilityDescription";
        var capTemplate = new CapabilityDescriptionTemplate(templateIdentifier, capSetDef, desiredIdShort, null);

        var capSubmodel = new CapabilityDescriptionSubmodel(null, "RequiredCapabilities");
        capSubmodel.Apply(capTemplate);
        var serializedSubmodel = await capSubmodel.ToJsonAsync();
        var newCapNode = JsonNode.Parse(serializedSubmodel) as JsonObject ?? throw new Exception("failed to parse serialized capability submodel");

        // rename existing template submodel ids to include shell short id to avoid duplicates and update AAS refs
        var renameMap = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        if (root["submodels"] is JsonArray existingSubs)
        {
            for (int i = 0; i < existingSubs.Count; i++)
            {
                if (existingSubs[i] is JsonObject s && s.TryGetPropertyValue("id", out var idNode) && idNode != null)
                {
                    var oldId = idNode.ToString() ?? string.Empty;
                    var newId = oldId + "-" + idShort;
                    s["id"] = newId;
                    renameMap[oldId] = newId;
                }
            }
        }

        // Replace any occurrences of the old submodel ids inside the whole template JSON
        void ReplaceIdsInNode(JsonNode? node)
        {
            if (node is JsonObject obj)
            {
                var propList = obj.ToList();
                foreach (var property in propList)
                {
                    var k = property.Key;
                    var child = obj[k];
                    if (child is JsonValue v)
                    {
                        if (v.TryGetValue<string>(out var s) && s != null && renameMap.TryGetValue(s, out var replacement))
                        {
                            obj[k] = replacement;
                        }
                    }
                    else
                    {
                        ReplaceIdsInNode(child);
                    }
                }
            }
            else if (node is JsonArray arr)
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    var el = arr[i];
                    if (el is JsonValue vv)
                    {
                        if (vv.TryGetValue<string>(out var s2) && s2 != null && renameMap.TryGetValue(s2, out var repl2))
                        {
                            arr[i] = repl2;
                        }
                    }
                    else
                    {
                        ReplaceIdsInNode(el);
                    }
                }
            }
        }

        ReplaceIdsInNode(root);

        // replace capability submodel
        if (capIndex >= 0 && root["submodels"] is JsonArray arr)
        {
            arr[capIndex] = newCapNode;
        }
        else
        {
            // append if not found
            if (root["submodels"] is JsonArray arr2) arr2.Add(newCapNode);
        }

        // update AAS submodel references in assetAdministrationShells: keep only refs to actually present submodels
        if (root["assetAdministrationShells"] is JsonArray aasArr && aasArr.Count > 0 && aasArr[0] is JsonObject aasObj && aasObj.TryGetPropertyValue("submodels", out var smRefs) && smRefs is JsonArray smRefsArr)
        {
            // compute set of submodel ids that exist in the root document
            var existingSubmodelIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (root["submodels"] is JsonArray allSubs)
            {
                foreach (var s in allSubs)
                {
                    if (s is JsonObject so && so.TryGetPropertyValue("id", out var idNode) && idNode != null)
                    {
                        existingSubmodelIds.Add(idNode.ToString() ?? string.Empty);
                    }
                }
            }

            var newRefs = new JsonArray();
            foreach (var r in smRefsArr)
            {
                if (r is JsonObject ro && ro.TryGetPropertyValue("keys", out var keys) && keys is JsonArray kArr && kArr.Count > 0)
                {
                    var k0 = kArr[0] as JsonObject;
                    if (k0 != null && k0.TryGetPropertyValue("value", out var v) && v != null)
                    {
                        var vs = v.ToString() ?? string.Empty;
                        // apply rename map if needed
                        if (renameMap.TryGetValue(vs, out var nv)) vs = nv;
                        // include only if the referenced submodel exists in the document
                        if (existingSubmodelIds.Contains(vs))
                        {
                            // ensure the key value is updated to the possibly renamed id
                            k0["value"] = vs;
                            newRefs.Add(JsonNode.Parse(ro.ToJsonString()));
                        }
                    }
                }
            }

            // ensure our generated capability submodel is referenced
            if (!existingSubmodelIds.Contains(templateIdentifier))
            {
                // if for some reason the generated submodel isn't added to root, add it now
                if (root["submodels"] is JsonArray arr2) arr2.Add(newCapNode);
                existingSubmodelIds.Add(templateIdentifier);
            }

            var capRef = new JsonObject
            {
                ["keys"] = new JsonArray(new JsonObject { ["type"] = "Submodel", ["value"] = templateIdentifier }),
                // make this an ExternalReference and add referredSemanticId pointing to CapabilityDescription semantics
                ["referredSemanticId"] = new JsonObject
                {
                    ["keys"] = new JsonArray(new JsonObject { ["type"] = "GlobalReference", ["value"] = "https://smartfactory.de/semantics/submodel/CapabilityDescription#1/0" }),
                    ["type"] = "ExternalReference"
                },
                ["type"] = "ExternalReference"
            };

            // avoid duplicate entries
            var already = false;
            foreach (var nr in newRefs)
            {
                if (nr is JsonObject nro && nro.TryGetPropertyValue("keys", out var k) && k is JsonArray kar && kar.Count > 0)
                {
                    var first = kar[0] as JsonObject;
                    if (first != null && first.TryGetPropertyValue("value", out var fv) && fv != null && fv.ToString() == templateIdentifier) { already = true; break; }
                }
            }
            if (!already) newRefs.Add(capRef);

            aasObj["submodels"] = newRefs;
        }

        // ensure output folder
        Directory.CreateDirectory(outputFolder);
        var outName = idShort + ".json";
        var outPath = Path.Combine(outputFolder, outName);
        await File.WriteAllTextAsync(outPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        return outPath;
    }
}
