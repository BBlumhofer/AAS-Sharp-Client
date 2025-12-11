using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;

namespace ModuleGenerator
{
    public static class ModuleGenerator
    {
        public static async Task<string> GenerateAsync(string configPath, string outputFolder)
        {
            var json = await File.ReadAllTextAsync(configPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var config = JsonSerializer.Deserialize<ModuleConfig>(json, options) ?? throw new InvalidOperationException("Invalid config");

            // Create shell
            var shellId = config.Id ?? Guid.NewGuid().ToString();
            var shell = new AssetAdministrationShell(shellId, new Identifier(shellId))
            {
                AssetInformation = new AssetInformation()
                {
                    AssetKind = AssetKind.Instance,
                    GlobalAssetId = new Identifier($"https://smartfactory.de/asset/{shellId}")
                }
            };

            // Skills submodel: use domain Models API to populate
            var smSkillsId = $"https://smartfactory.de/submodels/skills/{Guid.NewGuid()}";
            var skills = new SkillsSubmodel(smSkillsId);

            // Build SkillsData from config and apply
            var skillDef = new SkillDefinition(
                IdShort: "Skill_0001",
                Name: config.Skill ?? "UnnamedSkill",
                Endpoint: string.Empty,
                RequiredParameters: new List<SkillParameterDefinition>
                {
                    new SkillParameterDefinition("ProductId", "xs:string", "*")
                },
                Triggers: Array.Empty<SkillTriggerDefinition>());

            var skillsData = new SkillsData(
                SubmodelIdentifier: smSkillsId,
                Skills: new[] { skillDef },
                SecurityRequirementsReference: new Reference(new Key(KeyType.GlobalReference, "https://example.org/security")) { Type = ReferenceType.ExternalReference },
                EndpointMetadata: new EndpointMetadataData(Array.Empty<EndpointMetadataPropertyDefinition>(), new Reference(new Key(KeyType.GlobalReference, "https://example.org/securityList")) { Type = ReferenceType.ExternalReference }, Array.Empty<SecuritySchemeDefinition>()),
                SkillMetadata: new SkillMetadataData("","", "", Array.Empty<StateDefinition>(), Array.Empty<SkillMetadataTriggerDefinition>()));

            skills.Apply(skillsData);

            shell.Submodels.Add(skills);

            // CapabilityDescription submodel: construct template definition and apply
            var smCapabilityId = $"https://smartfactory.de/submodels/capability/{Guid.NewGuid()}";
            var capabilitySubmodel = new CapabilityDescriptionSubmodel(smCapabilityId);

            var propertyContainers = new List<CapabilityPropertyContainerDefinition>();
            // keep a map from property name -> created container idShort so constraints can reference them
            var propertyNameToContainerIdShort = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (config.Capability?.PropertyContainers != null)
            {
                foreach (var kv in config.Capability.PropertyContainers)
                {
                    var name = kv.Key;
                    var entry = kv.Value;
                    if (entry.Min != null && entry.Max != null)
                    {
                        var idShort = name + "Range";
                        propertyContainers.Add(new RangePropertyContainerDefinition(
                            IdShort: idShort,
                            PropertyIdShort: name,
                            MinValue: entry.Min.ToString() ?? string.Empty,
                            MaxValue: entry.Max.ToString() ?? string.Empty,
                            ValueType: "xs:double"));
                        propertyNameToContainerIdShort[name] = idShort;
                    }
                    else if (entry.Value != null)
                    {
                        var idShort = name + "Fixed";
                        propertyContainers.Add(new PropertyValueContainerDefinition(
                            IdShort: idShort,
                            PropertyIdShort: name,
                            Value: entry.Value.ToString() ?? string.Empty,
                            ValueType: "xs:string"));
                        propertyNameToContainerIdShort[name] = idShort;
                    }
                }
            }

            var propertySet = new CapabilityPropertySetDefinition("PropertySet", propertyContainers);

            // Map capability constraints from config into PropertyConstraintContainerDefinition items
            var constraintContainers = new List<PropertyConstraintContainerDefinition>();
            if (config.Capability?.Constraints != null)
            {
                var idx = 0;
                foreach (var c in config.Capability.Constraints)
                {
                    idx++;
                    var idShort = string.IsNullOrWhiteSpace(c.ConstraintName) ? $"Constraint_{idx}" : c.ConstraintName!;

                    var conditional = new PropertyValueDefinition("ConditionalType", c.ConditionalType ?? string.Empty, "xs:string");
                    var constraintType = new PropertyValueDefinition("ConstraintType", c.ConstraintType ?? string.Empty, "xs:string");
                    // populate custom constraint properties (include constraint name and optional related property value)
                    var customProps = new List<PropertyValueDefinition>
                    {
                        new PropertyValueDefinition("ConstraintName", c.ConstraintName ?? idShort, "xs:string")
                    };

                    if (!string.IsNullOrWhiteSpace(c.RelatedProperty))
                    {
                        // try to include the actual configured value from PropertyContainers if available
                        if (config.Capability.PropertyContainers != null && config.Capability.PropertyContainers.TryGetValue(c.RelatedProperty, out var relatedEntry) && relatedEntry.Value != null)
                        {
                            customProps.Add(new PropertyValueDefinition(c.RelatedProperty, relatedEntry.Value.ToString() ?? string.Empty, "xs:string"));
                        }
                        else
                        {
                            customProps.Add(new PropertyValueDefinition(c.RelatedProperty, string.Empty, "xs:string"));
                        }
                    }

                    var custom = new CustomConstraintDefinition("CustomConstraint", customProps);

                    var propConstraint = new PropertyConstraintContainerDefinition(
                        IdShort: idShort,
                        ConditionalType: conditional,
                        ConstraintType: constraintType,
                        CustomConstraint: custom);

                    // Optionally add a relation to a related property if provided
                    if (!string.IsNullOrWhiteSpace(c.RelatedProperty))
                    {
                        // Build a model reference path to the CustomConstraint inside this capability container
                        var capabilityContainerIdShort = (config.Capability?.Name ?? "Capability") + "Container";
                        var firstKeys = new List<IKey>
                        {
                            new Key(KeyType.Submodel, smCapabilityId),
                            new Key(KeyType.SubmodelElementCollection, "CapabilitySet"),
                            new Key(KeyType.SubmodelElementCollection, capabilityContainerIdShort),
                            new Key(KeyType.SubmodelElementCollection, "CapabilityRelations"),
                            new Key(KeyType.SubmodelElementCollection, "ConstraintSet"),
                            new Key(KeyType.SubmodelElementCollection, idShort),
                            new Key(KeyType.SubmodelElementCollection, "CustomConstraint")
                        };
                        var firstRef = new Reference(firstKeys) { Type = ReferenceType.ModelReference };

                        // Build a model reference path to the related property inside the PropertySet
                        var relatedContainerId = propertyNameToContainerIdShort.TryGetValue(c.RelatedProperty!, out var containerId) ? containerId : (c.RelatedProperty! + "Fixed");
                        var secondKeys = new List<IKey>
                        {
                            new Key(KeyType.Submodel, smCapabilityId),
                            new Key(KeyType.SubmodelElementCollection, "CapabilitySet"),
                            new Key(KeyType.SubmodelElementCollection, capabilityContainerIdShort),
                            new Key(KeyType.SubmodelElementCollection, "PropertySet"),
                            new Key(KeyType.SubmodelElementCollection, relatedContainerId),
                            new Key(KeyType.Property, c.RelatedProperty!)
                        };
                        var secondRef = new Reference(secondKeys) { Type = ReferenceType.ModelReference };

                        var rel = new RelationshipElementDefinition("RelatedProperty", firstRef, secondRef);
                        propConstraint = propConstraint with { PropertyRelations = new[] { rel } };
                    }

                    constraintContainers.Add(propConstraint);
                }
            }

            CapabilityConstraintSetDefinition? constraintSet = null;
            if (constraintContainers.Count > 0)
            {
                constraintSet = new CapabilityConstraintSetDefinition("ConstraintSet", constraintContainers);
            }

            var relationsDef = new CapabilityRelationsDefinition("Relations", Array.Empty<RelationshipElementDefinition>(), constraintSet);

            var capabilityContainer = new CapabilityContainerDefinition(
                IdShort: (config.Capability?.Name ?? "Capability") + "Container",
                Capability: new CapabilityElementDefinition(config.Capability?.Name ?? "Capability"),
                Relations: relationsDef,
                PropertySet: propertySet);

            var capabilitySet = new CapabilitySetDefinition("CapabilitySet", new[] { capabilityContainer });
            var template = new CapabilityDescriptionTemplate(smCapabilityId, capabilitySet);
            capabilitySubmodel.Apply(template);

            shell.Submodels.Add(capabilitySubmodel);

            // AssetLocation submodel: address shared across modules, position may come from config or use defaults
            var smAssetId = $"https://smartfactory.de/submodels/assetlocation/{Guid.NewGuid()}";
            var assetLocation = new AasSharpClient.Models.AssetLocationSubmodel(smAssetId);

            // config may optionally contain location info
            var loc = config.AssetLocation ?? new AssetLocationConfig();
            var addressDefault = "Trippstadter Str. 122, 67663 Kaiserslautern";
            var assetData = new AasSharpClient.Models.AssetLocationData(
                Address: addressDefault,
                CurrentArea: loc.CurrentArea ?? loc.Area ?? "ProductionHallA",
                X: loc.X ?? 0.0,
                Y: loc.Y ?? 0.0,
                Theta: loc.Theta ?? 0.0,
                Floor: loc.Floor ?? loc.Level ?? 1);

            assetLocation.Apply(assetData);
            shell.Submodels.Add(assetLocation);

            // Serialize individual generated submodels to JSON strings (these will be parsed fresh when inserting into template)
            var skillsJson = await skills.ToJsonAsync();
            var capJson = await capabilitySubmodel.ToJsonAsync();
            var assetJson = await assetLocation.ToJsonAsync();

            // Always use template.json (next to config) as basis and merge generated Skills/Capability
            var configDir = Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory();
            var templatePath = Path.Combine(configDir, "template.json");
            if (!File.Exists(templatePath))
            {
                // try parent folder (e.g. Tools/ModuleGenerator/template.json)
                var parentCandidate = Path.GetFullPath(Path.Combine(configDir, "..", "template.json"));
                if (File.Exists(parentCandidate)) templatePath = parentCandidate;
                else
                {
                    var repoCandidate = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Tools", "ModuleGenerator", "template.json"));
                    if (File.Exists(repoCandidate)) templatePath = repoCandidate;
                }
            }
            if (!File.Exists(templatePath)) throw new FileNotFoundException($"template.json not found (looked in config dir and Tools/ModuleGenerator)");

            var templateText = await File.ReadAllTextAsync(templatePath);
            var root = JsonNode.Parse(templateText) as JsonObject ?? new JsonObject();

            // Update AAS metadata (collect template submodel ids to remove and remember original AAS submodel refs)
            var toRemoveIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            JsonArray? originalAasSubRefs = null;
            if (root["assetAdministrationShells"] is JsonArray aasArray && aasArray.Count > 0)
            {
                if (aasArray[0] is JsonObject aasObj)
                {
                    aasObj["id"] = shellId;
                    aasObj["idShort"] = shellId;

                    if (aasObj["assetInformation"] is JsonObject assetInfo)
                    {
                        assetInfo["globalAssetId"] = $"https://smartfactory.de/asset/{shellId}";
                        if (assetInfo["specificAssetIds"] is JsonArray specific && specific.Count > 0)
                        {
                            if (specific[0] is JsonObject first)
                            {
                                first["value"] = shellId;
                            }
                        }
                    }

                    // save original AAS submodel refs for later update
                    if (aasObj["submodels"] is JsonArray aasSubRefs)
                    {
                        originalAasSubRefs = aasSubRefs;
                    }

                    // Prepare to update submodel references: find ids of template submodels that correspond to Skills/Capability
                    if (root["submodels"] is JsonArray templateSubmodels)
                    {
                        foreach (var item in templateSubmodels)
                        {
                            if (item is JsonObject obj && obj.TryGetPropertyValue("semanticId", out var sem) && sem is JsonObject semObj)
                            {
                                if (semObj.TryGetPropertyValue("keys", out var keys) && keys is JsonArray keysArr && keysArr.Count > 0)
                                {
                                    var firstKey = keysArr[0] as JsonObject;
                                    if (firstKey != null && firstKey.TryGetPropertyValue("value", out var val))
                                    {
                                        var sval = val?.ToString() ?? string.Empty;
                                        if (string.Equals(sval, "https://smartfactory.de/semantics/submodel/Skills#1/0", StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(sval, "https://admin-shell.io/idta/CapabilityDescription/1/0/Submodel", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (obj.TryGetPropertyValue("id", out var idNode) && idNode != null)
                                            {
                                                toRemoveIds.Add(idNode.ToString() ?? string.Empty);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Replace/remove template submodels that match Skills/Capability semantics, then append generated ones
            if (root["submodels"] is JsonArray templateArr)
            {
                var filtered = new JsonArray();
                    foreach (var item in templateArr)
                {
                    if (item is JsonObject obj)
                    {
                        var keep = true;
                        if (obj.TryGetPropertyValue("id", out var idNode) && idNode != null && toRemoveIds.Contains(idNode.ToString() ?? string.Empty))
                        {
                            keep = false;
                        }

                        if (keep)
                        {
                            // clone the preserved template item to avoid parent conflict
                            var clone = JsonNode.Parse(obj.ToJsonString() ?? string.Empty);
                            if (clone != null) filtered.Add(clone);
                        }
                    }
                    else
                    {
                        var cloneItem = JsonNode.Parse(item?.ToJsonString() ?? string.Empty);
                        if (cloneItem != null) filtered.Add(cloneItem);
                    }
                }

                // append generated submodels (parse from original JSON strings to avoid parent conflicts)
                var renameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    // when preserving template items, give them unique ids per shell to avoid duplicates in aggregated environments
                    for (int i = 0; i < filtered.Count; i++)
                    {
                        if (filtered[i] is JsonObject fobj && fobj.TryGetPropertyValue("id", out var idNode) && idNode != null)
                        {
                            var oldId = idNode.ToString() ?? string.Empty;
                            var newId = oldId + "-" + shellId;
                            fobj["id"] = newId;
                            renameMap[oldId] = newId;
                        }
                    }

                    filtered.Add(JsonNode.Parse(skillsJson)!);
                    filtered.Add(JsonNode.Parse(capJson)!);
                    filtered.Add(JsonNode.Parse(assetJson)!);
                    root["submodels"] = filtered;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error while merging generated submodels into template: " + ex);
                    throw;
                }

                // Now update the AAS' submodel references: rebuild from original refs, skip removed ids and apply rename map,
                // then append references to the generated submodels
                if (root["assetAdministrationShells"] is JsonArray finalAasArray && finalAasArray.Count > 0 && originalAasSubRefs != null)
                {
                    if (finalAasArray[0] is JsonObject finalAasObj)
                    {
                        var newRefs = new JsonArray();
                        foreach (var r in originalAasSubRefs)
                        {
                            if (r is JsonObject rObj && rObj.TryGetPropertyValue("keys", out var keys) && keys is JsonArray kArr && kArr.Count > 0)
                            {
                                var k0 = kArr[0] as JsonObject;
                                if (k0 != null && k0.TryGetPropertyValue("value", out var v) && v != null)
                                {
                                    var sval = v.ToString() ?? string.Empty;
                                    if (toRemoveIds.Contains(sval))
                                    {
                                        continue; // skip
                                    }
                                    // clone and replace value if renamed
                                    var clonedRef = JsonNode.Parse(r?.ToJsonString() ?? string.Empty) as JsonObject;
                                    if (clonedRef != null && clonedRef.TryGetPropertyValue("keys", out var clonedKeys) && clonedKeys is JsonArray ckArr && ckArr.Count > 0)
                                    {
                                        var ck0 = ckArr[0] as JsonObject;
                                        if (ck0 != null && ck0.TryGetPropertyValue("value", out var cv) && cv != null)
                                        {
                                            var cvs = cv.ToString() ?? string.Empty;
                                            if (renameMap.TryGetValue(cvs, out var newVal))
                                            {
                                                ck0["value"] = newVal;
                                            }
                                        }
                                    }
                                    newRefs.Add(clonedRef);
                                }
                                else
                                {
                                    // no first key value -> keep clone
                                    var clonedRef = JsonNode.Parse(r?.ToJsonString() ?? string.Empty);
                                    if (clonedRef != null) newRefs.Add(clonedRef);
                                }
                            }
                            else
                            {
                                var clonedRef = JsonNode.Parse(r?.ToJsonString() ?? string.Empty);
                                if (clonedRef != null) newRefs.Add(clonedRef);
                            }
                        }

                        // append references for generated submodels
                        foreach (var smId in new[] { smSkillsId, smCapabilityId, smAssetId })
                        {
                            var refObj = new JsonObject
                            {
                                ["keys"] = new JsonArray(new JsonObject
                                {
                                    ["type"] = "Submodel",
                                    ["value"] = smId
                                }),
                                ["type"] = "ModelReference"
                            };
                            newRefs.Add(refObj);
                        }

                        finalAasObj["submodels"] = newRefs;
                    }
                }
            }

            Directory.CreateDirectory(outputFolder);
            var outPath = Path.Combine(outputFolder, shellId + ".json");
            await File.WriteAllTextAsync(outPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

            return outPath;
        }
    }

    // Config types (minimal)
    public class ModuleConfig
    {
        public string? Id { get; set; }
        public string? Skill { get; set; }
        public CapabilityConfig? Capability { get; set; }
        public AssetLocationConfig? AssetLocation { get; set; }
    }

    public class CapabilityConfig
    {
        public string? Name { get; set; }
        public string? SkillReference { get; set; }
        public Dictionary<string, PropertyContainerConfig>? PropertyContainers { get; set; }
        public ConstraintConfig[]? Constraints { get; set; }
    }

    public class PropertyContainerConfig
    {
        public double? Min { get; set; }
        public double? Max { get; set; }
        public object? Value { get; set; }
    }

    public class ConstraintConfig
    {
        public string? ConstraintType { get; set; }
        public string? ConditionalType { get; set; }
        public string? ConstraintName { get; set; }
        public string? RelatedProperty { get; set; }
    }

    public class AssetLocationConfig
    {
        // support both possible JSON property names used in examples
        public string? CurrentArea { get; set; }
        public string? Area { get; set; }

        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Theta { get; set; }

        public int? Floor { get; set; }
        public int? Level { get; set; }
}

    }
