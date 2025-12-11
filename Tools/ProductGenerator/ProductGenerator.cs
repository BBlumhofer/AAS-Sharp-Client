using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO;
using System;
using System.Threading.Tasks;

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

        // build capability submodel from config: similar structure but without Constraints, only RealizedBy relation
        var newCap = new JsonObject();
        newCap["modelType"] = "Submodel";
        newCap["kind"] = "Instance";
        newCap["id"] = $"https://smartfactory.de/submodels/capability/{Guid.NewGuid()}-{idShort}";
        newCap["idShort"] = "OfferedCapabilityDescription";
        newCap["semanticId"] = new JsonObject { ["type"] = "ExternalReference", ["keys"] = new JsonArray(new JsonObject { ["type"] = "GlobalReference", ["value"] = "https://admin-shell.io/idta/CapabilityDescription/1/0/Submodel" }) };

        var capabilitySet = new JsonObject();
        capabilitySet["idShort"] = "CapabilitySet";
        capabilitySet["modelType"] = "SubmodelElementCollection";
        capabilitySet["semanticId"] = new JsonObject { ["type"] = "ExternalReference", ["keys"] = new JsonArray(new JsonObject { ["type"] = "GlobalReference", ["value"] = "https://admin-shell.io/idta/CapabilityDescription/CapabilitySet/1/0" }) };

        var capList = new JsonArray();

        if (cfg["Capabilities"] is JsonArray caps)
        {
            foreach (var c in caps)
            {
                if (c is JsonObject co)
                {
                    var name = co["Name"]?.ToString() ?? "Capability";
                    var container = new JsonObject();
                    container["idShort"] = name + "Container";
                    container["modelType"] = "SubmodelElementCollection";
                    container["value"] = new JsonArray();

                    // Capability entry
                    var capEntry = new JsonObject();
                    capEntry["idShort"] = name;
                    capEntry["modelType"] = "Capability";
                    capEntry["semanticId"] = new JsonObject { ["type"] = "ExternalReference", ["keys"] = new JsonArray(new JsonObject { ["type"] = "GlobalReference", ["value"] = "https://wiki.eclipse.org/BaSyx_/_Documentation_/_Submodels_/_Capability#Capability" }) };

                    // No RealizedBy relation: product-side relations are omitted for this generator
                    // add capEntry
                    var capValueArray = container["value"] as JsonArray ?? new JsonArray();
                    capValueArray.Add(capEntry);
                    container["value"] = capValueArray;

                    // Build PropertySet from PropertyContainers (no constraints)
                    var propertySet = new JsonObject { ["idShort"] = "PropertySet", ["modelType"] = "SubmodelElementCollection", ["value"] = new JsonArray() };
                    if (co["PropertyContainers"] is JsonObject pcs)
                    {
                        foreach (var kv in pcs)
                        {
                            var key = kv.Key;
                            var val = kv.Value?.ToString();
                            if (key == "ProductId") continue; // skip ProductId here
                            var propContainer = new JsonObject { ["idShort"] = key + "Container", ["modelType"] = "SubmodelElementCollection", ["value"] = new JsonArray() };
                            // add a simple Property element
                            var prop = new JsonObject { ["idShort"] = key, ["modelType"] = "Property", ["valueType"] = "double", ["value"] = val ?? "" };
                            (propContainer["value"] as JsonArray ?? new JsonArray()).Add(prop);
                            (propertySet["value"] as JsonArray ?? new JsonArray()).Add(propContainer);
                        }
                    }

                    // attach elements to container: capability and propertySet (no relations)
                    var containerVals = container["value"] as JsonArray ?? new JsonArray();
                    containerVals.Add(propertySet);
                    container["value"] = containerVals;

                    capList.Add(container);
                }
            }
        }

        capabilitySet["value"] = capList;
        newCap["submodelElements"] = new JsonArray(capabilitySet);

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

        // replace capability submodel
        if (capIndex >= 0 && root["submodels"] is JsonArray arr)
        {
            arr[capIndex] = newCap;
        }
        else
        {
            // append if not found
            if (root["submodels"] is JsonArray arr2) arr2.Add(newCap);
        }

        // update AAS submodel references in assetAdministrationShells
        if (root["assetAdministrationShells"] is JsonArray aasArr && aasArr.Count>0 && aasArr[0] is JsonObject aasObj && aasObj.TryGetPropertyValue("submodels", out var smRefs) && smRefs is JsonArray smRefsArr)
        {
            var newRefs = new JsonArray();
            foreach (var r in smRefsArr)
            {
                if (r is JsonObject ro && ro.TryGetPropertyValue("keys", out var keys) && keys is JsonArray kArr && kArr.Count>0)
                {
                    var k0 = kArr[0] as JsonObject;
                    if (k0 != null && k0.TryGetPropertyValue("value", out var v) && v!=null)
                    {
                        var vs = v.ToString() ?? string.Empty;
                        if (renameMap.TryGetValue(vs, out var nv)) k0["value"] = nv;
                    }
                }
                newRefs.Add(JsonNode.Parse(r?.ToJsonString() ?? string.Empty));
            }
            // ensure capability ref exists and points to newCap id
            var capRef = new JsonObject { ["keys"] = new JsonArray(new JsonObject { ["type"] = "Submodel", ["value"] = newCap["id"]?.ToString() ?? string.Empty }), ["type"] = "ModelReference" };
            newRefs.Add(capRef);
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
