using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models;

public sealed class SkillsSubmodel : Submodel
{
    private static readonly Reference SemanticReference = ReferenceFactory.Model(
        (KeyType.Submodel, "https://smartfactory.de/semantics/submodel/Skills#1/0"));

    private static readonly Reference EndpointMetadataSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://admin-shell.io/idta/AssetInterfacesDescription/1/0/EndpointMetadata"));

    private static readonly Reference SecurityListSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://www.w3.org/2019/wot/td#hasSecurityConfiguration"));

    private static readonly Reference SecurityDefinitionsSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://www.w3.org/2019/wot/td#definesSecurityScheme"));

    public SubmodelElementCollection SkillSet { get; }
    public SubmodelElementCollection EndpointMetadata { get; }
    public SubmodelElementCollection SkillMetadata { get; }

    public SkillsSubmodel(string? submodelIdentifier = null)
        : base("Skills", new Identifier(submodelIdentifier ?? Guid.NewGuid().ToString()))
    {
        Kind = ModelingKind.Instance;
        SemanticId = SemanticReference;

        SkillSet = new SubmodelElementCollection("SkillSet");
        EndpointMetadata = new SubmodelElementCollection("EndpointMetadata")
        {
            SemanticId = EndpointMetadataSemantic,
            Qualifiers = new List<IQualifier> { Cardinality("One") }
        };

        SkillMetadata = new SubmodelElementCollection("SkillMetadata")
        {
            SemanticId = EndpointMetadataSemantic,
            Qualifiers = new List<IQualifier> { Cardinality("One") }
        };

        SubmodelElements.Add(SkillSet);
        SubmodelElements.Add(EndpointMetadata);
        SubmodelElements.Add(SkillMetadata);
    }

    public static SkillsSubmodel CreateWithIdentifier(string submodelIdentifier) => new(submodelIdentifier);

    public Task<string> ToJsonAsync(CancellationToken cancellationToken = default)
    {
        return SubmodelSerialization.SerializeAsync(this, cancellationToken);
    }

    // Read/query helpers
    public IEnumerable<SubmodelElementCollection> GetSkills()
    {
        return SkillSet.OfType<SubmodelElementCollection>();
    }

    public IEnumerable<string> GetSkillNames()
    {
        foreach (var skill in GetSkills())
        {
            var nameProp = skill.OfType<Property>().FirstOrDefault(p => string.Equals(p.IdShort, "Name", StringComparison.OrdinalIgnoreCase));
            if (nameProp?.Value?.Value != null)
            {
                yield return nameProp.Value.Value.ToString() ?? string.Empty;
            }
        }
    }

    public SubmodelElementCollection? FindSkillById(string idShort)
    {
        if (string.IsNullOrWhiteSpace(idShort)) return null;
        return SkillSet.OfType<SubmodelElementCollection>().FirstOrDefault(s => string.Equals(s.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
    }

    public void Apply(SkillsData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        SkillSet.Clear();
        foreach (var skill in data.Skills)
        {
            SkillSet.Add(CreateSkill(skill, data.SecurityRequirementsReference));
        }

        EndpointMetadata.Clear();
        PopulateEndpointMetadata(data.EndpointMetadata);

        SkillMetadata.Clear();
        PopulateSkillMetadata(data.SkillMetadata);
    }

    private SubmodelElementCollection CreateSkill(SkillDefinition definition, Reference securityRequirementsReference)
    {
        var skill = new SubmodelElementCollection(definition.IdShort);
        skill.Add(SubmodelElementFactory.CreateProperty("Name", definition.Name, null, "xs:string"));
        skill.Add(CreateSkillInterfaceDescription(definition, securityRequirementsReference));
        return skill;
    }

    private SubmodelElementCollection CreateSkillInterfaceDescription(SkillDefinition definition, Reference securityRequirementsReference)
    {
        var description = new SubmodelElementCollection("SkillInterfaceDescription");
        description.Add(SubmodelElementFactory.CreateProperty("RequiredAccessLevel", definition.RequiredAccessLevel, null, "xs:integer"));
        description.Add(SubmodelElementFactory.CreateProperty("SkillEndpoint", definition.Endpoint, null, "xs:anyURI"));
        description.Add(CreateRequiredInputParameters(definition));
        description.Add(CreateTriggerCollection(definition.Triggers));
        description.Add(CreateSecurityRequirements(securityRequirementsReference));
        return description;
    }

    private SubmodelElementCollection CreateRequiredInputParameters(SkillDefinition definition)
    {
        var parameters = new SubmodelElementCollection("RequiredInputParameters");
        foreach (var parameter in definition.RequiredParameters)
        {
            var property = parameter.Value is null
                ? new Property(parameter.IdShort, ParseDataType(parameter.ValueType))
                : SubmodelElementFactory.CreateProperty(parameter.IdShort, parameter.Value, null, parameter.ValueType);
            parameters.Add(property);
        }

        return parameters;
    }

    private static SubmodelElementCollection CreateTriggerCollection(IReadOnlyList<SkillTriggerDefinition> triggers)
    {
        var triggerCollection = new SubmodelElementCollection("Trigger");
        if (triggers != null)
        {
            foreach (var trigger in triggers)
            {
                triggerCollection.Add(CreateTriggerOperation(trigger.IdShort));
            }
        }

        return triggerCollection;
    }

    private static Operation CreateTriggerOperation(string idShort)
    {
        var operation = new Operation(idShort);
        operation.InputVariables.Add(CreatePlaceholderSubmodelElement("inputVariables"));
        operation.OutputVariables.Add(CreatePlaceholderSubmodelElement("outputVariables"));
        operation.InOutputVariables.Add(CreatePlaceholderSubmodelElement("inoutputVariables"));
        return operation;
    }

    private static Property CreatePlaceholderSubmodelElement(string idShort)
    {
        return new Property(idShort, new DataType(DataObjectType.String));
    }

    private static ReferenceElement CreateSecurityRequirements(Reference reference)
    {
        return new ReferenceElement("SecurityRequirements")
        {
            Value = new ReferenceElementValue(reference)
        };
    }

    private void PopulateEndpointMetadata(EndpointMetadataData endpointMetadata)
    {
        if (endpointMetadata.Properties != null)
        {
            foreach (var property in endpointMetadata.Properties)
            {
                EndpointMetadata.Add(CreateEndpointProperty(property));
            }
        }

        EndpointMetadata.Add(CreateSecurityList(endpointMetadata.SecurityListReference));
        EndpointMetadata.Add(CreateSecurityDefinitions(endpointMetadata.SecuritySchemes));
    }

    private static Property CreateEndpointProperty(EndpointMetadataPropertyDefinition definition)
    {
        var property = CreatePropertyPreservingStringValue(definition.IdShort, definition.ValueType, definition.Value);
        property.SemanticId = ReferenceFactory.External((KeyType.GlobalReference, definition.SemanticUri));
        property.Qualifiers = new List<IQualifier> { Cardinality(definition.Cardinality) };
        return property;
    }

    private static Property CreatePropertyPreservingStringValue(string idShort, string valueType, string value)
    {
        if (!string.Equals(valueType, "xs:anyURI", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(valueType, "anyURI", StringComparison.OrdinalIgnoreCase))
        {
            return (Property)SubmodelElementFactory.CreateProperty(idShort, value, null, valueType);
        }

        var stringProperty = (Property)SubmodelElementFactory.CreateProperty(idShort, value, null);
        stringProperty.ValueType = new DataType(DataObjectType.AnyURI);
        return stringProperty;
    }

    private static SubmodelElementList CreateSecurityList(Reference securityReference)
    {
        var list = new SubmodelElementList("security")
        {
            SemanticId = SecurityListSemantic,
            Qualifiers = new List<IQualifier> { Cardinality("One") },
            TypeValueListElement = ModelType.ReferenceElement
        };

        var referenceElement = new ReferenceElement(string.Empty)
        {
            Value = new ReferenceElementValue(securityReference)
        };
        referenceElement.IdShort = null;
        list.Add(referenceElement);

        return list;
    }

    private static SubmodelElementCollection CreateSecurityDefinitions(IReadOnlyList<SecuritySchemeDefinition> schemes)
    {
        var definitions = new SubmodelElementCollection("securityDefinitions")
        {
            SemanticId = SecurityDefinitionsSemantic,
            Qualifiers = new List<IQualifier> { Cardinality("One") }
        };

        if (schemes != null)
        {
            foreach (var scheme in schemes)
            {
                definitions.Add(CreateSecurityScheme(scheme));
            }
        }

        return definitions;
    }

    private static SubmodelElementCollection CreateSecurityScheme(SecuritySchemeDefinition definition)
    {
        var scheme = new SubmodelElementCollection(definition.IdShort)
        {
            SemanticId = ReferenceFactory.External((KeyType.GlobalReference, definition.SemanticUri)),
            Qualifiers = new List<IQualifier> { Cardinality(definition.Cardinality) }
        };

        SecuritySchemeProperty? proxyProp = null;
        if (definition.Properties != null)
        {
            foreach (var property in definition.Properties)
            {
                if (string.Equals(property.IdShort, "proxy", StringComparison.OrdinalIgnoreCase))
                {
                    proxyProp = property;
                    continue;
                }

                scheme.Add(CreateSecurityProperty(property));
            }
        }

        if (definition.Lists != null)
        {
            foreach (var listDefinition in definition.Lists)
            {
                scheme.Add(CreateSecuritySchemeList(listDefinition));
            }
        }

        if (proxyProp != null)
        {
            scheme.Add(CreateSecurityProperty(proxyProp));
        }
        else if (string.Equals(definition.IdShort, "combo_sc", StringComparison.OrdinalIgnoreCase))
        {
            var forcedProxy = new SecuritySchemeProperty("proxy", "https://www.w3.org/2019/wot/security#proxy", "ZeroToOne", "xs:string", null);
            scheme.Add(CreateSecurityProperty(forcedProxy));
        }

        return scheme;
    }

    private static Property CreateSecurityProperty(SecuritySchemeProperty property)
    {
        Property result;
        if (string.Equals(property.ValueType, "xs:anyURI", StringComparison.OrdinalIgnoreCase)
            || string.Equals(property.ValueType, "anyURI", StringComparison.OrdinalIgnoreCase))
        {
            var val = property.Value ?? string.Empty;
            result = CreatePropertyPreservingStringValue(property.IdShort, property.ValueType, val);
        }
        else if (property.Value is null)
        {
            result = new Property(property.IdShort, ParseDataType(property.ValueType));
        }
        else
        {
            result = SubmodelElementFactory.CreateProperty(property.IdShort, property.Value, null, property.ValueType);
        }

        result.SemanticId = ReferenceFactory.External((KeyType.GlobalReference, property.SemanticUri));
        result.Qualifiers = new List<IQualifier> { Cardinality(property.Cardinality) };
        return result;
    }

    private static SubmodelElementList CreateSecuritySchemeList(SecuritySchemeListDefinition definition)
    {
        var list = new SubmodelElementList(definition.IdShort)
        {
            SemanticId = ReferenceFactory.External((KeyType.GlobalReference, definition.SemanticUri)),
            Qualifiers = new List<IQualifier> { Cardinality(definition.Cardinality) },
            TypeValueListElement = definition.ElementType
        };

        if (definition.ValueType != null)
        {
            list.ValueTypeListElement = definition.ValueType;
        }

        if (definition.ReferenceValues != null)
        {
            foreach (var reference in definition.ReferenceValues)
            {
                var referenceElement = new ReferenceElement(string.Empty)
                {
                    Value = new ReferenceElementValue(reference)
                };
                referenceElement.IdShort = null;
                list.Add(referenceElement);
            }
        }

        return list;
    }

    private void PopulateSkillMetadata(SkillMetadataData metadataData)
    {
        SkillMetadata.Add(CreateStateMachine(metadataData));
        SkillMetadata.Add(SubmodelElementFactory.CreateProperty("InterfaceConcept", metadataData.InterfaceConcept, null, "xs:string"));
        SkillMetadata.Add(SubmodelElementFactory.CreateProperty("InterfaceProtocol", metadataData.InterfaceProtocol, null, "xs:string"));
    }

    private static SubmodelElementCollection CreateStateMachine(SkillMetadataData metadataData)
    {
        var stateMachine = new SubmodelElementCollection("StateMachine");
        stateMachine.Add(SubmodelElementFactory.CreateProperty("TypeOfStateMachine", metadataData.StateMachineType, null, "xs:string"));
        stateMachine.Add(CreateStatesCollection(metadataData.States));
        stateMachine.Add(new SubmodelElementCollection("Transitions"));
        stateMachine.Add(CreateSkillMetadataTriggers(metadataData.Triggers));
        return stateMachine;
    }

    private static SubmodelElementCollection CreateStatesCollection(IReadOnlyList<StateDefinition> states)
    {
        var statesCollection = new SubmodelElementCollection("States");
        if (states != null)
        {
            foreach (var state in states)
            {
                if (state.Value is null)
                {
                    statesCollection.Add(new Property(state.IdShort, new DataType(DataObjectType.String)));
                }
                else
                {
                    statesCollection.Add(SubmodelElementFactory.CreateProperty(state.IdShort, state.Value, null, "xs:string"));
                }
            }
        }

        return statesCollection;
    }

    private static SubmodelElementCollection CreateSkillMetadataTriggers(IReadOnlyList<SkillMetadataTriggerDefinition> triggers)
    {
        var triggersCollection = new SubmodelElementCollection("Trigger");
        if (triggers != null)
        {
            foreach (var trigger in triggers)
            {
                triggersCollection.Add(CreateMetadataTrigger(trigger.IdShort));
            }
        }

        return triggersCollection;
    }

    private static ReferenceElement CreateMetadataTrigger(string idShort)
    {
        return new ReferenceElement(idShort);
    }

    private static DataType ParseDataType(string valueType)
    {
        return DataObjectType.TryParse(valueType, out var dataType)
            ? new DataType(dataType)
            : new DataType(DataObjectType.String);
    }

    private static IQualifier Cardinality(string value) => new Qualifier
    {
        Type = "Cardinality",
        Value = value,
        ValueType = new DataType(DataObjectType.String)
    };
}

public sealed record SkillsData(
    string SubmodelIdentifier,
    IReadOnlyList<SkillDefinition> Skills,
    Reference SecurityRequirementsReference,
    EndpointMetadataData EndpointMetadata,
    SkillMetadataData SkillMetadata);

public sealed record SkillDefinition(
    string IdShort,
    string Name,
    string Endpoint,
    IReadOnlyList<SkillParameterDefinition> RequiredParameters,
    IReadOnlyList<SkillTriggerDefinition> Triggers,
    string RequiredAccessLevel = "2");

public sealed record SkillParameterDefinition(string IdShort, string ValueType, string? Value);

public sealed record EndpointMetadataData(
    IReadOnlyList<EndpointMetadataPropertyDefinition> Properties,
    Reference SecurityListReference,
    IReadOnlyList<SecuritySchemeDefinition> SecuritySchemes);

public sealed record EndpointMetadataPropertyDefinition(
    string IdShort,
    string SemanticUri,
    string Cardinality,
    string ValueType,
    string Value);

public sealed record SkillMetadataData(
    string InterfaceConcept,
    string InterfaceProtocol,
    string StateMachineType,
    IReadOnlyList<StateDefinition> States,
    IReadOnlyList<SkillMetadataTriggerDefinition> Triggers);

public sealed record SkillTriggerDefinition(string IdShort);

public sealed record SkillMetadataTriggerDefinition(string IdShort);

public sealed record SecuritySchemeDefinition(
    string IdShort,
    string SemanticUri,
    string Cardinality,
    IReadOnlyList<SecuritySchemeProperty> Properties,
    IReadOnlyList<SecuritySchemeListDefinition>? Lists = null);

public sealed record SecuritySchemeProperty(
    string IdShort,
    string SemanticUri,
    string Cardinality,
    string ValueType,
    string? Value);

public sealed record SecuritySchemeListDefinition(
    string IdShort,
    string SemanticUri,
    string Cardinality,
    ModelType ElementType,
    DataType? ValueType,
    IReadOnlyList<Reference>? ReferenceValues);

public sealed record StateDefinition(string IdShort, string? Value);
