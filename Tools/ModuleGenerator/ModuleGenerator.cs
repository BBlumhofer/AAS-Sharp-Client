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
        private const string SmartFactoryCapabilityBase = "https://smartfactory.de/aas/submodel/CapabilityDescription";
        private const string SmartFactoryCapabilitySubmodelSemantic = SmartFactoryCapabilityBase + "#1/0";
        private const string SmartFactoryStorageConfigurationSemantic =
            "https://smartfactory.de/semantics/submodel/CarrierManagement/StorageConfiguration#1/0";
        private const string SmartFactoryCapabilitySetSemantic = SmartFactoryCapabilityBase + "/CapabilitySet#1/0";
        private const string SmartFactoryCapabilityContainerSemantic = SmartFactoryCapabilityBase + "/CapabilitySet/CapabilityContainer#1/0";
        private const string SmartFactoryCapabilitySemantic = SmartFactoryCapabilityBase + "/Capability#1/0";
        private const string SmartFactoryCapabilityRelationsSemantic = SmartFactoryCapabilityBase + "/CapabilitySet/CapabilityContainer/CapabilityRelations#1/0";
        private const string SmartFactoryPropertySetSemantic = SmartFactoryCapabilityBase + "/CapabilitySet/CapabilityContainer/PropertySet#1/0";
        private const string SmartFactoryConstraintSetSemantic = SmartFactoryCapabilityRelationsSemantic + "/ConstraintSet#1/0";
        private const string SmartFactoryPropertyConstraintContainerSemantic = SmartFactoryConstraintSetSemantic + "/PropertyConstraintContainer#1/0";
        private const string SmartFactoryTransitionConstraintContainerSemantic = SmartFactoryConstraintSetSemantic + "/TransitionConstraintContainer#1/0";
        private const string SmartFactoryTransitionConditionTypeSemantic = SmartFactoryTransitionConstraintContainerSemantic + "/TransitionConditionType#1/0";
        private const string SmartFactoryCustomConstraintSemantic = SmartFactoryPropertyConstraintContainerSemantic + "/CustomConstraint#1/0";
        private const string SmartFactoryCapabilityRelationsRealizedBySemantic =
            "https://admin-shell.io/idta/CapabilityDescription/CapabilityRelations/RealizedBy/1/0";

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

            var capabilityConfigs = config.Capabilities;
            if (capabilityConfigs == null || capabilityConfigs.Length == 0)
            {
                capabilityConfigs = config.Capability == null ? Array.Empty<CapabilityConfig>() : new[] { config.Capability };
            }

            // Skills submodel: use domain Models API to populate
            var smSkillsId = $"https://smartfactory.de/submodels/skills/{Guid.NewGuid()}";
            var skills = new SkillsSubmodel(smSkillsId);

            var skillNames = new List<string>();
            foreach (var capability in capabilityConfigs)
            {
                if (!string.IsNullOrWhiteSpace(capability.SkillReference))
                {
                    skillNames.Add(capability.SkillReference!);
                }
            }

            if (skillNames.Count == 0)
            {
                skillNames.Add(config.Skill ?? "UnnamedSkill");
            }

            var uniqueSkills = skillNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var skillDefinitions = new List<SkillDefinition>();
            var skillNameToIdShort = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < uniqueSkills.Count; i++)
            {
                var idShort = $"Skill_{i + 1:0000}";
                var name = uniqueSkills[i];
                skillDefinitions.Add(new SkillDefinition(
                    IdShort: idShort,
                    Name: name,
                    Endpoint: string.Empty,
                    RequiredParameters: new List<SkillParameterDefinition>
                    {
                        new SkillParameterDefinition("ProductId", "xs:string", "*")
                    },
                    Triggers: Array.Empty<SkillTriggerDefinition>()));
                skillNameToIdShort[name] = idShort;
            }

            var skillsData = new SkillsData(
                SubmodelIdentifier: smSkillsId,
                Skills: skillDefinitions,
                SecurityRequirementsReference: new Reference(new Key(KeyType.GlobalReference, "https://example.org/security")) { Type = ReferenceType.ExternalReference },
                EndpointMetadata: new EndpointMetadataData(Array.Empty<EndpointMetadataPropertyDefinition>(), new Reference(new Key(KeyType.GlobalReference, "https://example.org/securityList")) { Type = ReferenceType.ExternalReference }, Array.Empty<SecuritySchemeDefinition>()),
                SkillMetadata: new SkillMetadataData("", "", "", Array.Empty<StateDefinition>(), Array.Empty<SkillMetadataTriggerDefinition>()));

            skills.Apply(skillsData);

            shell.Submodels.Add(skills);

            // CapabilityDescription submodel: construct template definition and apply
            var smCapabilityId = $"https://smartfactory.de/submodels/capability/{Guid.NewGuid()}";
            var capabilitySubmodel = new CapabilityDescriptionSubmodel(smCapabilityId);

            var capabilityContainers = new List<CapabilityContainerDefinition>();

            foreach (var capability in capabilityConfigs)
            {
                var capabilityName = capability.Name ?? "Capability";
                var capabilityContainerIdShort = capabilityName + "Container";
                const string relationsIdShort = "CapabilityRelations";

                var propertyContainers = new List<CapabilityPropertyContainerDefinition>();
                // keep a map from property name -> container idShort and element key type for constraint references
                var propertyReferenceInfo = new Dictionary<string, PropertyReferenceInfo>(StringComparer.OrdinalIgnoreCase);
                if (capability.PropertyContainers != null)
                {
                    foreach (var kv in capability.PropertyContainers)
                    {
                        var name = kv.Key;
                        var entry = kv.Value;
                        var propertyContainerSemantic = CreateExternalReference(
                            $"{SmartFactoryCapabilityBase}/CapabilitySet/CapabilityContainer/PropertySet/PropertyContainer/{name}#1/0");
                        if (entry.Min != null && entry.Max != null)
                        {
                            var idShort = name + "Range";
                            propertyContainers.Add(new RangePropertyContainerDefinition(
                                IdShort: idShort,
                                PropertyIdShort: name,
                                MinValue: entry.Min.ToString() ?? string.Empty,
                                MaxValue: entry.Max.ToString() ?? string.Empty,
                                ValueType: "xs:double",
                                SemanticId: propertyContainerSemantic));
                            propertyReferenceInfo[name] = new PropertyReferenceInfo(idShort, KeyType.Range, name);
                        }
                        else if (TryGetListValues(entry.Value, out var listValues, out var listValueType))
                        {
                            var idShort = name + "List";
                            var entries = listValues
                                .Select(value => new PropertyValueDefinition(null, value, listValueType))
                                .ToList();
                            propertyContainers.Add(new PropertyListContainerDefinition(
                                IdShort: idShort,
                                ListIdShort: name,
                                Entries: entries,
                                ValueTypeListElement: listValueType,
                                SemanticId: propertyContainerSemantic));
                            propertyReferenceInfo[name] = new PropertyReferenceInfo(idShort, KeyType.SubmodelElementList, name);
                        }
                        else if (entry.Value != null)
                        {
                            var idShort = name;
                            propertyContainers.Add(new PropertyValueContainerDefinition(
                                IdShort: idShort,
                                PropertyIdShort: name,
                                Value: ConvertValueToString(entry.Value),
                                ValueType: GetValueType(entry.Value),
                                SemanticId: propertyContainerSemantic));
                            propertyReferenceInfo[name] = new PropertyReferenceInfo(idShort, KeyType.Property, name);
                        }
                    }
                }

                var propertySet = new CapabilityPropertySetDefinition(
                    "PropertySet",
                    propertyContainers,
                    SemanticId: CreateExternalReference(SmartFactoryPropertySetSemantic));

                // Map capability constraints from config into PropertyConstraintContainerDefinition items
                var constraintContainers = new List<PropertyConstraintContainerDefinition>();
                var propertyConstraints = capability.PropertyConstraints ?? capability.Constraints;
                if (propertyConstraints != null)
                {
                    var idx = 0;
                    foreach (var c in propertyConstraints)
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
                            if (capability.PropertyContainers != null && capability.PropertyContainers.TryGetValue(c.RelatedProperty, out var relatedEntry) && relatedEntry.Value != null)
                            {
                                customProps.Add(new PropertyValueDefinition(c.RelatedProperty, relatedEntry.Value.ToString() ?? string.Empty, "xs:string"));
                            }
                            else
                            {
                                customProps.Add(new PropertyValueDefinition(c.RelatedProperty, string.Empty, "xs:string"));
                            }
                        }

                        var custom = new CustomConstraintDefinition(
                            "CustomConstraint",
                            customProps,
                            SemanticId: CreateExternalReference(SmartFactoryCustomConstraintSemantic));

                        var propConstraint = new PropertyConstraintContainerDefinition(
                            IdShort: idShort,
                            ConditionalType: conditional,
                            ConstraintType: constraintType,
                            CustomConstraint: custom,
                            SemanticId: CreateExternalReference(SmartFactoryPropertyConstraintContainerSemantic),
                            PropertyRelationsSemanticId: CreateExternalReference(SmartFactoryPropertyConstraintContainerSemantic));

                        // Optionally add a relation to a related property if provided
                        if (!string.IsNullOrWhiteSpace(c.RelatedProperty))
                        {
                            // Build a model reference path to the CustomConstraint inside this capability container
                            var firstKeys = new List<IKey>
                            {
                                new Key(KeyType.Submodel, smCapabilityId),
                                new Key(KeyType.SubmodelElementCollection, "CapabilitySet"),
                                new Key(KeyType.SubmodelElementCollection, capabilityContainerIdShort),
                                new Key(KeyType.SubmodelElementCollection, relationsIdShort),
                                new Key(KeyType.SubmodelElementCollection, "ConstraintSet"),
                                new Key(KeyType.SubmodelElementCollection, idShort),
                                new Key(KeyType.SubmodelElementCollection, "CustomConstraint")
                            };
                            var firstRef = new Reference(firstKeys) { Type = ReferenceType.ModelReference };

                            // Build a model reference path to the related property inside the PropertySet
                            var relatedInfo = propertyReferenceInfo.TryGetValue(c.RelatedProperty!, out var info)
                                ? info
                                : new PropertyReferenceInfo(c.RelatedProperty!, KeyType.Property, c.RelatedProperty!);
                            var secondKeys = new List<IKey>
                            {
                                new Key(KeyType.Submodel, smCapabilityId),
                                new Key(KeyType.SubmodelElementCollection, "CapabilitySet"),
                                new Key(KeyType.SubmodelElementCollection, capabilityContainerIdShort),
                                new Key(KeyType.SubmodelElementCollection, "PropertySet"),
                                new Key(KeyType.SubmodelElementCollection, relatedInfo.ContainerIdShort),
                                new Key(relatedInfo.ElementKeyType, relatedInfo.ElementIdShort)
                            };
                            var secondRef = new Reference(secondKeys) { Type = ReferenceType.ModelReference };

                            var rel = new RelationshipElementDefinition("RelatedProperty", firstRef, secondRef);
                            propConstraint = propConstraint with { PropertyRelations = new[] { rel } };
                        }

                        constraintContainers.Add(propConstraint);
                    }
                }

                var transitionConstraintContainers = new List<TransitionConstraintContainerDefinition>();
                if (capability.TransitionConstraints != null)
                {
                    var idx = 0;
                    foreach (var c in capability.TransitionConstraints)
                    {
                        idx++;
                        var idShort = $"TransitionConstraintContainer{idx:000}";
                        var condition = new PropertyValueDefinition(
                            "TransitionConditionType",
                            c.ConditionalType ?? string.Empty,
                            "xs:string",
                            SemanticId: CreateExternalReference(SmartFactoryTransitionConditionTypeSemantic));
                        var constraintName = new PropertyValueDefinition(
                            "ConstraintName",
                            c.ConstraintName ?? string.Empty,
                            "xs:string");
                        transitionConstraintContainers.Add(new TransitionConstraintContainerDefinition(
                            idShort,
                            condition,
                            constraintName,
                            SemanticId: CreateExternalReference(SmartFactoryTransitionConstraintContainerSemantic)));
                    }
                }

                CapabilityConstraintSetDefinition? constraintSet = null;
                if (constraintContainers.Count > 0 || transitionConstraintContainers.Count > 0)
                {
                    constraintSet = new CapabilityConstraintSetDefinition(
                        "ConstraintSet",
                        constraintContainers,
                        transitionConstraintContainers,
                        SemanticId: CreateExternalReference(SmartFactoryConstraintSetSemantic));
                }

                var relationships = new List<RelationshipElementDefinition>();
                var capabilityReference = CreateCapabilityReference(smCapabilityId, capabilityName);

                var skillName = capability.SkillReference;
                if (string.IsNullOrWhiteSpace(skillName) && uniqueSkills.Count == 1)
                {
                    skillName = uniqueSkills[0];
                }

                if (!string.IsNullOrWhiteSpace(skillName) && skillNameToIdShort.TryGetValue(skillName, out var skillIdShort))
                {
                    var skillReference = CreateSkillReference(smSkillsId, skillIdShort);
                    relationships.Add(new RelationshipElementDefinition(
                        "CapabilityRealizedBy_001",
                        capabilityReference,
                        skillReference,
                        SemanticId: CreateExternalReference(SmartFactoryCapabilityRelationsRealizedBySemantic)));
                }

                CapabilityGeneralizedBySetDefinition? generalizedBySet = null;
                if (capability.GeneralizedBy != null && capability.GeneralizedBy.Length > 0)
                {
                    var generalizedRelations = new List<RelationshipElementDefinition>();
                    for (var i = 0; i < capability.GeneralizedBy.Length; i++)
                    {
                        var target = capability.GeneralizedBy[i];
                        if (string.IsNullOrWhiteSpace(target))
                        {
                            continue;
                        }

                        var targetRef = CreateCapabilityReference(smCapabilityId, target);
                        generalizedRelations.Add(new RelationshipElementDefinition($"CapabilityGeneralizedBy_{i + 1:000}", capabilityReference, targetRef));
                    }

                    if (generalizedRelations.Count > 0)
                    {
                        generalizedBySet = new CapabilityGeneralizedBySetDefinition("GeneralizedBySet", generalizedRelations);
                    }
                }

                CapabilityComposedOfSetDefinition? composedOfSet = null;
                if (capability.ComposedOf != null && capability.ComposedOf.Length > 0)
                {
                    var composedRelations = new List<RelationshipElementDefinition>();
                    for (var i = 0; i < capability.ComposedOf.Length; i++)
                    {
                        var target = capability.ComposedOf[i];
                        if (string.IsNullOrWhiteSpace(target))
                        {
                            continue;
                        }

                        var targetRef = CreateCapabilityReference(smCapabilityId, target);
                        composedRelations.Add(new RelationshipElementDefinition($"CapabilityComposedOf_{i + 1:000}", capabilityReference, targetRef));
                    }

                    if (composedRelations.Count > 0)
                    {
                        composedOfSet = new CapabilityComposedOfSetDefinition("ComposedOfSet", composedRelations);
                    }
                }

                var relationsDef = new CapabilityRelationsDefinition(
                    IdShort: relationsIdShort,
                    Relationships: relationships,
                    ConstraintSet: constraintSet,
                    GeneralizedBySet: generalizedBySet,
                    ComposedOfSet: composedOfSet,
                    SemanticId: CreateExternalReference(SmartFactoryCapabilityRelationsSemantic));

                var capabilityContainer = new CapabilityContainerDefinition(
                    IdShort: capabilityContainerIdShort,
                    Capability: new CapabilityElementDefinition(
                        capabilityName,
                        SemanticId: CreateExternalReference(SmartFactoryCapabilitySemantic)),
                    Relations: relationsDef,
                    PropertySet: propertySet,
                    SemanticId: CreateExternalReference(SmartFactoryCapabilityContainerSemantic));

                capabilityContainers.Add(capabilityContainer);
            }

            var capabilitySet = new CapabilitySetDefinition(
                "CapabilitySet",
                capabilityContainers,
                SemanticId: CreateExternalReference(SmartFactoryCapabilitySetSemantic));
            var template = new CapabilityDescriptionTemplate(
                smCapabilityId,
                capabilitySet,
                SemanticId: CreateExternalReference(SmartFactoryCapabilitySubmodelSemantic));
            capabilitySubmodel.Apply(template);

            shell.Submodels.Add(capabilitySubmodel);

            // AssetLocation submodel: address shared across modules, position may come from config or use defaults
            var smAssetId = $"https://smartfactory.de/submodels/assetlocation/{Guid.NewGuid()}";
            var assetSubmodel = new AasSharpClient.Models.AssetLocationSubmodel(smAssetId);

            // config may optionally contain location info
            var loc = config.AssetLocation ?? new AssetLocationConfig();
            var addressDefault = "Trippstadter Str. 122, 67663 Kaiserslautern";
            var assetData = new AasSharpClient.Models.AssetLocationData(
                Address: addressDefault,
                Parent: loc.Parent ?? loc.Area ?? "ProductionHallA",
                X: loc.X ?? 0.0,
                Y: loc.Y ?? 0.0,
                Theta: loc.Theta ?? 0.0);

            assetSubmodel.Apply(assetData);
            shell.Submodels.Add(assetSubmodel);

            StorageConfigurationSubmodel? storageSubmodel = null;
            string? smStorageId = null;
            if (config.StorageConfiguration != null)
            {
                smStorageId = $"https://smartfactory.de/submodels/storageconfiguration/{Guid.NewGuid()}";
                storageSubmodel = new StorageConfigurationSubmodel(smStorageId);
                var storageData = BuildStorageConfigurationData(config.StorageConfiguration, smStorageId);
                storageSubmodel.Apply(storageData);
                shell.Submodels.Add(storageSubmodel);
            }

            // Serialize individual generated submodels to JSON strings (these will be parsed fresh when inserting into template)
            var skillsJson = await skills.ToJsonAsync();
            var capJson = await capabilitySubmodel.ToJsonAsync();
            var assetJson = await assetSubmodel.ToJsonAsync();
            var storageJson = storageSubmodel != null ? await storageSubmodel.ToJsonAsync() : null;

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
                                            || string.Equals(sval, "https://admin-shell.io/idta/CapabilityDescription/1/0/Submodel", StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(sval, SmartFactoryCapabilitySubmodelSemantic, StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(sval, SmartFactoryStorageConfigurationSemantic, StringComparison.OrdinalIgnoreCase))
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
                            if (clone != null)
                            {
                                RemoveReferredSemanticId(clone);
                                filtered.Add(clone);
                            }
                        }
                    }
                    else
                    {
                        var cloneItem = JsonNode.Parse(item?.ToJsonString() ?? string.Empty);
                        if (cloneItem != null)
                        {
                            RemoveReferredSemanticId(cloneItem);
                            filtered.Add(cloneItem);
                        }
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

                    var skillsNode = JsonNode.Parse(skillsJson)!;
                    RemoveReferredSemanticId(skillsNode);
                    filtered.Add(skillsNode);

                    var capNode = JsonNode.Parse(capJson)!;
                    RemoveReferredSemanticId(capNode);
                    filtered.Add(capNode);

                    var assetNode = JsonNode.Parse(assetJson)!;
                    RemoveReferredSemanticId(assetNode);
                    filtered.Add(assetNode);

                    if (!string.IsNullOrWhiteSpace(storageJson))
                    {
                        var storageNode = JsonNode.Parse(storageJson)!;
                        RemoveReferredSemanticId(storageNode);
                        filtered.Add(storageNode);
                    }
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
                        var generatedIds = new List<string> { smSkillsId, smCapabilityId, smAssetId };
                        if (!string.IsNullOrWhiteSpace(smStorageId))
                        {
                            generatedIds.Add(smStorageId);
                        }

                        foreach (var smId in generatedIds)
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

        private static StorageConfigurationData BuildStorageConfigurationData(StorageConfigurationConfig config, string submodelId)
        {
            var storages = new List<StorageConfigurationStorageData>();
            var storageConfigs = ExpandStorageConfigs(config);
            var storageIndex = 0;

            foreach (var storage in storageConfigs)
            {
                storageIndex++;
                var effectiveStorage = MergeStorageWithDefaults(storage, config.StorageDefaults);
                var idShort = string.IsNullOrWhiteSpace(effectiveStorage.IdShort)
                    ? $"Storage_{storageIndex:000}"
                    : effectiveStorage.IdShort!;
                var storageId = string.IsNullOrWhiteSpace(effectiveStorage.StorageId)
                    ? $"storage-{storageIndex:000}"
                    : effectiveStorage.StorageId!;
                var name = string.IsNullOrWhiteSpace(effectiveStorage.Name) ? storageId : effectiveStorage.Name!;
                var totalSlots = effectiveStorage.TotalSlots ?? Math.Max(effectiveStorage.Slots?.Length ?? 1, 1);
                var costFunction = string.IsNullOrWhiteSpace(effectiveStorage.CostFunctionType)
                    ? "exponential"
                    : effectiveStorage.CostFunctionType!.Trim();

                var baseCost = effectiveStorage.BaseCost ?? 1.0;
                var maxCost = effectiveStorage.MaxCost ?? Math.Max(baseCost, 100.0);
                var alpha = effectiveStorage.Alpha ?? (string.Equals(costFunction, "exponential", StringComparison.OrdinalIgnoreCase) ? 1.0 : null);
                var lowCost = effectiveStorage.LowCost ?? (string.Equals(costFunction, "step", StringComparison.OrdinalIgnoreCase) ? 1.0 : null);
                var highCost = effectiveStorage.HighCost ?? (string.Equals(costFunction, "step", StringComparison.OrdinalIgnoreCase) ? 2.0 : null);
                var stepThreshold = effectiveStorage.StepThreshold ?? (string.Equals(costFunction, "step", StringComparison.OrdinalIgnoreCase) ? 0.5 : null);

                var slots = new List<StorageConfigurationSlotData>();
                var slotConfigs = effectiveStorage.Slots;
                if (slotConfigs != null && slotConfigs.Length > 0)
                {
                    if (slotConfigs.Length == 1 && totalSlots > 1)
                    {
                        var templateSlot = slotConfigs[0];
                        for (var slotIndex = 1; slotIndex <= totalSlots; slotIndex++)
                        {
                            var slotIdShort = $"Slot_{slotIndex:00}";
                            var slotId = string.IsNullOrWhiteSpace(templateSlot.SlotId)
                                ? $"slot-{storageIndex:000}-{slotIndex:00}"
                                : $"{templateSlot.SlotId}-{slotIndex:00}";
                            var reward = templateSlot.AffinityReward ?? 1.0;
                            var penalty = templateSlot.AffinityPenalty ?? 0.5;

                            slots.Add(new StorageConfigurationSlotData(
                                slotIdShort,
                                slotId,
                                templateSlot.PreferredType,
                                reward,
                                penalty));
                        }
                    }
                    else
                    {
                        var slotIndex = 0;
                        foreach (var slot in slotConfigs)
                        {
                            slotIndex++;
                            var slotIdShort = string.IsNullOrWhiteSpace(slot.IdShort)
                                ? $"Slot_{slotIndex:00}"
                                : slot.IdShort!;
                            var slotId = string.IsNullOrWhiteSpace(slot.SlotId)
                                ? $"slot-{storageIndex:000}-{slotIndex:00}"
                                : slot.SlotId!;
                            var reward = slot.AffinityReward ?? 1.0;
                            var penalty = slot.AffinityPenalty ?? 0.5;

                            slots.Add(new StorageConfigurationSlotData(
                                slotIdShort,
                                slotId,
                                slot.PreferredType,
                                reward,
                                penalty));
                        }
                    }
                }

                storages.Add(new StorageConfigurationStorageData(
                    idShort,
                    storageId,
                    name,
                    totalSlots,
                    costFunction,
                    baseCost,
                    maxCost,
                    alpha,
                    lowCost,
                    highCost,
                    stepThreshold,
                    slots));
            }

            var demandConfig = config.DemandConfig ?? new DemandConfigConfig();
            var projectionConfig = config.ProjectionConfig ?? new ProjectionConfigConfig();

            var demandData = new StorageConfigurationDemandConfigData(
                demandConfig.DemandBonusBase ?? 4.0,
                demandConfig.DemandBonusMax ?? 5.0,
                demandConfig.DemandWeightPotential ?? 0.0,
                demandConfig.DemandWeightPlanned ?? 0.3,
                demandConfig.DemandWeightImminent ?? 0.7,
                demandConfig.DemandWeightExecuting ?? 1.0,
                demandConfig.UrgencyEnabled ?? false);

            var projectionData = new StorageConfigurationProjectionConfigData(
                projectionConfig.WeightNoAgent ?? 0.0,
                projectionConfig.WeightStepOpen ?? 0.3,
                projectionConfig.WeightStepPlanned ?? 0.7,
                projectionConfig.WeightStepExecuting ?? 1.0,
                projectionConfig.MaxStepsAhead ?? 3);

            return new StorageConfigurationData(
                submodelId,
                storages,
                demandData,
                projectionData);
        }

        private static IReadOnlyList<StorageConfig> ExpandStorageConfigs(StorageConfigurationConfig config)
        {
            var storageConfigs = config.Storages ?? Array.Empty<StorageConfig>();
            if (storageConfigs.Length > 0)
            {
                return storageConfigs;
            }

            var count = config.StorageCount ?? 0;
            if (count <= 0)
            {
                return Array.Empty<StorageConfig>();
            }

            var list = new List<StorageConfig>(count);
            for (var i = 0; i < count; i++)
            {
                list.Add(config.StorageDefaults ?? new StorageConfig());
            }

            return list;
        }

        private static StorageConfig MergeStorageWithDefaults(StorageConfig storage, StorageConfig? defaults)
        {
            if (defaults == null)
            {
                return storage;
            }

            var slots = storage.Slots != null && storage.Slots.Length > 0 ? storage.Slots : defaults.Slots;

            return new StorageConfig
            {
                IdShort = storage.IdShort ?? defaults.IdShort,
                StorageId = storage.StorageId ?? defaults.StorageId,
                Name = storage.Name ?? defaults.Name,
                TotalSlots = storage.TotalSlots ?? defaults.TotalSlots,
                CostFunctionType = storage.CostFunctionType ?? defaults.CostFunctionType,
                BaseCost = storage.BaseCost ?? defaults.BaseCost,
                Alpha = storage.Alpha ?? defaults.Alpha,
                LowCost = storage.LowCost ?? defaults.LowCost,
                HighCost = storage.HighCost ?? defaults.HighCost,
                StepThreshold = storage.StepThreshold ?? defaults.StepThreshold,
                MaxCost = storage.MaxCost ?? defaults.MaxCost,
                Slots = slots
            };
        }

        private static Reference CreateCapabilityReference(string submodelId, string capabilityName)
        {
            var keys = new List<IKey>
            {
                new Key(KeyType.Submodel, submodelId),
                new Key(KeyType.SubmodelElementCollection, "CapabilitySet"),
                new Key(KeyType.SubmodelElementCollection, capabilityName + "Container"),
                new Key(KeyType.Capability, capabilityName)
            };
            return new Reference(keys) { Type = ReferenceType.ModelReference };
        }

        private static Reference CreateSkillReference(string skillsSubmodelId, string skillIdShort)
        {
            var keys = new List<IKey>
            {
                new Key(KeyType.Submodel, skillsSubmodelId),
                new Key(KeyType.SubmodelElementCollection, "SkillSet"),
                new Key(KeyType.SubmodelElementCollection, skillIdShort)
            };
            return new Reference(keys) { Type = ReferenceType.ModelReference };
        }

        private static Reference CreateExternalReference(string globalReference)
        {
            var keys = new List<IKey>
            {
                new Key(KeyType.GlobalReference, globalReference)
            };
            return new Reference(keys) { Type = ReferenceType.ExternalReference };
        }

        private static void RemoveReferredSemanticId(JsonNode? node)
        {
            if (node == null) return;

            if (node is JsonObject obj)
            {
                if (obj.ContainsKey("referredSemanticId"))
                {
                    obj.Remove("referredSemanticId");
                }

                foreach (var kvp in obj)
                {
                    RemoveReferredSemanticId(kvp.Value);
                }
            }
            else if (node is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    RemoveReferredSemanticId(item);
                }
            }
        }

        private static bool TryGetListValues(object? value, out List<string> values, out string valueType)
        {
            values = new List<string>();
            valueType = "xs:string";
            if (value is null)
            {
                return false;
            }

            if (value is JsonElement element && element.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in element.EnumerateArray())
                {
                    values.Add(ConvertJsonElementToString(entry));
                }

                if (element.GetArrayLength() > 0)
                {
                    valueType = GetValueType(element.EnumerateArray().First());
                }

                return true;
            }

            return false;
        }

        private static string ConvertValueToString(object value)
        {
            if (value is JsonElement element)
            {
                return ConvertJsonElementToString(element);
            }

            return value.ToString() ?? string.Empty;
        }

        private static string ConvertJsonElementToString(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Number => element.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => string.Empty,
                _ => element.ToString()
            };
        }

        private static string GetValueType(object value)
        {
            if (value is JsonElement element)
            {
                return GetValueType(element);
            }

            return value switch
            {
                bool => "xs:boolean",
                sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal => "xs:double",
                _ => "xs:string"
            };
        }

        private static string GetValueType(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.True => "xs:boolean",
                JsonValueKind.False => "xs:boolean",
                JsonValueKind.Number => "xs:double",
                _ => "xs:string"
            };
        }
    }

    // Config types (minimal)
    public class ModuleConfig
    {
        public string? Id { get; set; }
        public string? Skill { get; set; }
        public CapabilityConfig? Capability { get; set; }
        public CapabilityConfig[]? Capabilities { get; set; }
        public AssetLocationConfig? AssetLocation { get; set; }
        public StorageConfigurationConfig? StorageConfiguration { get; set; }
    }

    public class CapabilityConfig
    {
        public string? Name { get; set; }
        public string? SkillReference { get; set; }
        public Dictionary<string, PropertyContainerConfig>? PropertyContainers { get; set; }
        public ConstraintConfig[]? Constraints { get; set; }
        public ConstraintConfig[]? PropertyConstraints { get; set; }
        public ConstraintConfig[]? TransitionConstraints { get; set; }
        public string[]? ComposedOf { get; set; }
        public string[]? GeneralizedBy { get; set; }
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
        public string? Parent { get; set; }
        public string? Area { get; set; }

        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Theta { get; set; }

        public int? Level { get; set; }
    }

    public class StorageConfigurationConfig
    {
        public int? StorageCount { get; set; }
        public StorageConfig? StorageDefaults { get; set; }
        public StorageConfig[]? Storages { get; set; }
        public DemandConfigConfig? DemandConfig { get; set; }
        public ProjectionConfigConfig? ProjectionConfig { get; set; }
    }

    public class StorageConfig
    {
        public string? IdShort { get; set; }
        public string? StorageId { get; set; }
        public string? Name { get; set; }
        public int? TotalSlots { get; set; }
        public string? CostFunctionType { get; set; }
        public double? BaseCost { get; set; }
        public double? Alpha { get; set; }
        public double? LowCost { get; set; }
        public double? HighCost { get; set; }
        public double? StepThreshold { get; set; }
        public double? MaxCost { get; set; }
        public SlotConfig[]? Slots { get; set; }
    }

    public class SlotConfig
    {
        public string? IdShort { get; set; }
        public string? SlotId { get; set; }
        public string? PreferredType { get; set; }
        public double? AffinityReward { get; set; }
        public double? AffinityPenalty { get; set; }
    }

    public class DemandConfigConfig
    {
        public double? DemandBonusBase { get; set; }
        public double? DemandBonusMax { get; set; }
        public double? DemandWeightPotential { get; set; }
        public double? DemandWeightPlanned { get; set; }
        public double? DemandWeightImminent { get; set; }
        public double? DemandWeightExecuting { get; set; }
        public bool? UrgencyEnabled { get; set; }
    }

    public class ProjectionConfigConfig
    {
        public double? WeightNoAgent { get; set; }
        public double? WeightStepOpen { get; set; }
        public double? WeightStepPlanned { get; set; }
        public double? WeightStepExecuting { get; set; }
        public int? MaxStepsAhead { get; set; }
    }

    internal sealed record PropertyReferenceInfo(string ContainerIdShort, KeyType ElementKeyType, string ElementIdShort);

    }
