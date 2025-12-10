using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using BaSyx.Models.AdminShell;
using Range = BaSyx.Models.AdminShell.Range;

namespace AasSharpClient.Models;

public sealed class CapabilityDescriptionSubmodel : Submodel
{
    public const string DefaultIdShort = "OfferedCapabilitiyDescription";

    public SubmodelElementCollection CapabilitySet { get; private set; }

    public CapabilityDescriptionSubmodel(
        string? submodelIdentifier = null,
        string idShort = DefaultIdShort,
        Reference? semanticId = null,
        Reference? capabilitySetSemanticId = null)
        : base(idShort, new Identifier(submodelIdentifier ?? Guid.NewGuid().ToString()))
    {
        Kind = ModelingKind.Instance;
        SemanticId = semanticId ?? CapabilityDescriptionSemantics.Submodel;

        CapabilitySet = CapabilityDescriptionElementFactory.CreateEmptyCollection(
            "CapabilitySet",
            capabilitySetSemanticId ?? CapabilityDescriptionSemantics.CapabilitySet);

        SubmodelElements.Add(CapabilitySet);
    }

    public static CapabilityDescriptionSubmodel CreateWithIdentifier(string submodelIdentifier) => new(submodelIdentifier);

    public async Task<string> ToJsonAsync(CancellationToken cancellationToken = default)
    {
        var serialized = await SubmodelSerialization.SerializeAsync(this, cancellationToken);
        return CapabilityDescriptionJsonNormalizer.Normalize(serialized);
    }

    public void Apply(CapabilityDescriptionTemplate template)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (string.IsNullOrWhiteSpace(template.Identifier))
        {
            throw new ArgumentException("Template identifier must be provided.", nameof(template));
        }

        IdShort = string.IsNullOrWhiteSpace(template.SubmodelIdShort)
            ? IdShort
            : template.SubmodelIdShort!;
        Id = new Identifier(template.Identifier!);
        SemanticId = template.SemanticId ?? SemanticId;

        CapabilitySet = CapabilityDescriptionElementFactory.CreateCapabilitySet(template.CapabilitySet);
        SubmodelElements.Clear();
        SubmodelElements.Add(CapabilitySet);
    }

    public SubmodelElementCollection AddCapabilityContainer(CapabilityContainerDefinition definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        var container = CapabilityDescriptionElementFactory.CreateCapabilityContainer(definition);
        CapabilitySet.Add(container);
        return container;
    }

    public IEnumerable<Capability> GetCapabilities()
    {
        foreach (var container in CapabilitySet.OfType<SubmodelElementCollection>())
        {
            foreach (var capability in container.OfType<Capability>())
            {
                yield return capability;
            }
        }
    }

    public IEnumerable<string> GetCapabilityNames()
    {
        return GetCapabilities()
            .Select(c => c.IdShort ?? string.Empty)
            .Where(name => !string.IsNullOrWhiteSpace(name));
    }

    public SubmodelElementCollection? FindCapabilityContainer(string idShort)
    {
        if (string.IsNullOrWhiteSpace(idShort))
        {
            return null;
        }

        return CapabilitySet
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(c => string.Equals(c.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
    }
}

internal static class CapabilityDescriptionElementFactory
{
    public static SubmodelElementCollection CreateEmptyCollection(string idShort, Reference? semanticId)
    {
        var collection = new SubmodelElementCollection(idShort);
        if (semanticId is not null)
        {
            collection.SemanticId = semanticId;
        }

        WithoutKind(collection);
        return collection;
    }

    public static SubmodelElementCollection CreateCapabilitySet(CapabilitySetDefinition definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        var capabilitySet = new SubmodelElementCollection(definition.IdShort)
        {
            SemanticId = definition.SemanticId ?? CapabilityDescriptionSemantics.CapabilitySet,
            Description = CloneLangStrings(definition.Description)
        };
        WithoutKind(capabilitySet);

        ApplyQualifiers(definition.Qualifiers, qualifiers => capabilitySet.Qualifiers = qualifiers);

        var containers = definition.CapabilityContainers ?? Array.Empty<CapabilityContainerDefinition>();
        foreach (var containerDefinition in containers)
        {
            capabilitySet.Add(CreateCapabilityContainer(containerDefinition));
        }

        return capabilitySet;
    }

    public static SubmodelElementCollection CreateCapabilityContainer(CapabilityContainerDefinition definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        var container = new SubmodelElementCollection(definition.IdShort)
        {
            SemanticId = definition.SemanticId ?? CapabilityDescriptionSemantics.CapabilityContainer,
            Description = CloneLangStrings(definition.Description)
        };
        WithoutKind(container);

        ApplyQualifiers(definition.Qualifiers, qualifiers => container.Qualifiers = qualifiers);
        container.Add(CreateCapabilityElement(definition.Capability));

        if (definition.Comment is not null)
        {
            container.Add(CreateMultiLanguageProperty(definition.Comment));
        }

        if (definition.Relations is not null)
        {
            container.Add(CreateRelations(definition.Relations));
        }

        if (definition.PropertySet is not null)
        {
            container.Add(CreatePropertySet(definition.PropertySet));
        }

        if (definition.AdditionalElements != null)
        {
            foreach (var element in definition.AdditionalElements)
            {
                container.Add(element);
            }
        }

        return container;
    }

    private static Capability CreateCapabilityElement(CapabilityElementDefinition definition)
    {
        var capability = new Capability(definition.IdShort)
        {
            SemanticId = definition.SemanticId ?? CapabilityDescriptionSemantics.Capability
        };

        ApplyQualifiers(definition.Qualifiers, qualifiers => capability.Qualifiers = qualifiers);
        WithoutKind(capability);
        return capability;
    }

    private static MultiLanguageProperty CreateMultiLanguageProperty(MultiLanguagePropertyDefinition definition)
    {
        var property = new MultiLanguageProperty(definition.IdShort)
        {
            Description = CloneLangStrings(definition.Description),
            Value = new MultiLanguagePropertyValue(CloneLangStrings(definition.Value) ?? new LangStringSet())
        };

        if (definition.SemanticId is not null)
        {
            property.SemanticId = definition.SemanticId;
        }

        ApplyQualifiers(definition.Qualifiers, qualifiers => property.Qualifiers = qualifiers);
        WithoutKind(property);
        return property;
    }

    private static SubmodelElementCollection CreateRelations(CapabilityRelationsDefinition definition)
    {
        var relations = new SubmodelElementCollection(definition.IdShort)
        {
            SemanticId = definition.SemanticId ?? CapabilityDescriptionSemantics.CapabilityRelations,
            Description = CloneLangStrings(definition.Description)
        };
        WithoutKind(relations);

        ApplyQualifiers(definition.Qualifiers, qualifiers => relations.Qualifiers = qualifiers);

        var relationships = definition.Relationships ?? Array.Empty<RelationshipElementDefinition>();
        foreach (var relationship in relationships)
        {
            relations.Add(CreateRelationshipElement(relationship));
        }

        if (definition.AdditionalCollections != null)
        {
            foreach (var extra in definition.AdditionalCollections)
            {
                relations.Add(CreateSimpleCollection(extra));
            }
        }

        if (definition.ConstraintSet is not null)
        {
            relations.Add(CreateConstraintSet(definition.ConstraintSet));
        }

        return relations;
    }

    private static SubmodelElementCollection CreateConstraintSet(CapabilityConstraintSetDefinition definition)
    {
        var constraintSet = new SubmodelElementCollection(definition.IdShort)
        {
            SemanticId = definition.SemanticId ?? CapabilityDescriptionSemantics.ConstraintSet
        };
        WithoutKind(constraintSet);

        var containers = definition.ConstraintContainers ?? Array.Empty<PropertyConstraintContainerDefinition>();
        foreach (var container in containers)
        {
            constraintSet.Add(CreatePropertyConstraintContainer(container));
        }

        return constraintSet;
    }

    private static SubmodelElementCollection CreatePropertyConstraintContainer(PropertyConstraintContainerDefinition definition)
    {
        var container = new SubmodelElementCollection(definition.IdShort)
        {
            SemanticId = definition.SemanticId ?? CapabilityDescriptionSemantics.PropertyConstraintContainer
        };
        WithoutKind(container);

        container.Add(CreateProperty(definition.ConditionalType, true));
        container.Add(CreateProperty(definition.ConstraintType, true));
        container.Add(CreateCustomConstraint(definition.CustomConstraint));

        if (definition.PropertyRelations is not null && definition.PropertyRelations.Count > 0)
        {
            var relations = new SubmodelElementCollection(definition.PropertyRelationsIdShort ?? "ConstraintPropertyRelations");
            WithoutKind(relations);

            var relationsSemantic = definition.PropertyRelationsSemanticId ?? definition.SemanticId;
            if (relationsSemantic is not null)
            {
                relations.SemanticId = relationsSemantic;
            }

            foreach (var relationDefinition in definition.PropertyRelations)
            {
                relations.Add(CreateRelationshipElement(relationDefinition));
            }

            container.Add(relations);
        }

        return container;
    }

    private static SubmodelElementCollection CreateCustomConstraint(CustomConstraintDefinition definition)
    {
        var customConstraint = new SubmodelElementCollection(definition.IdShort)
        {
            SemanticId = definition.SemanticId ?? CapabilityDescriptionSemantics.CustomConstraint
        };
        WithoutKind(customConstraint);

        var properties = definition.Properties ?? Array.Empty<PropertyValueDefinition>();
        foreach (var propertyDefinition in properties)
        {
            customConstraint.Add(CreateProperty(propertyDefinition));
        }

        return customConstraint;
    }

    private static SubmodelElementCollection CreatePropertySet(CapabilityPropertySetDefinition definition)
    {
        var propertySet = new SubmodelElementCollection(definition.IdShort)
        {
            SemanticId = definition.SemanticId ?? CapabilityDescriptionSemantics.PropertySet,
            Description = CloneLangStrings(definition.Description)
        };
        WithoutKind(propertySet);

        ApplyQualifiers(definition.Qualifiers, qualifiers => propertySet.Qualifiers = qualifiers);

        var containers = definition.Containers ?? Array.Empty<CapabilityPropertyContainerDefinition>();
        foreach (var containerDefinition in containers)
        {
            propertySet.Add(CreatePropertyContainer(containerDefinition));
        }

        return propertySet;
    }

    private static SubmodelElementCollection CreatePropertyContainer(CapabilityPropertyContainerDefinition definition)
    {
        var container = new SubmodelElementCollection(definition.IdShort)
        {
            SemanticId = definition.SemanticId ?? CapabilityDescriptionSemantics.PropertyContainer
        };
        WithoutKind(container);

        ApplyQualifiers(definition.Qualifiers, qualifiers => container.Qualifiers = qualifiers);

        if (definition.Comment is not null)
        {
            container.Add(CreateMultiLanguageProperty(definition.Comment));
        }

        switch (definition)
        {
            case RangePropertyContainerDefinition rangeDefinition:
                container.Add(CreateRange(rangeDefinition));
                break;
            case PropertyValueContainerDefinition propertyDefinition:
                container.Add(CreateFixedProperty(propertyDefinition));
                break;
            case PropertyListContainerDefinition listDefinition:
                container.Add(CreatePropertyList(listDefinition));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(definition), definition.GetType(), "Unsupported property container definition");
        }

        return container;
    }

    private static Range CreateRange(RangePropertyContainerDefinition definition)
    {
        var dataType = ResolveDataType(definition.ValueType);
        var range = new Range(definition.PropertyIdShort, dataType)
        {
            Value = new RangeValue(
                new ElementValue<string>(definition.MinValue),
                new ElementValue<string>(definition.MaxValue))
        };

        if (definition.PropertySemanticId is not null)
        {
            range.SemanticId = definition.PropertySemanticId;
        }
        else
        {
            range.SemanticId = null;
        }

        WithoutKind(range);
        return range;
    }

    private static Property CreateFixedProperty(PropertyValueContainerDefinition definition)
    {
        var property = SubmodelElementFactory.CreateProperty(
            definition.PropertyIdShort,
            definition.Value,
            definition.PropertySemanticId,
            definition.ValueType);

        if (definition.PropertySemanticId is null)
        {
            property.SemanticId = null;
        }

        WithoutKind(property);
        return property;
    }

    private static SubmodelElementList CreatePropertyList(PropertyListContainerDefinition definition)
    {
        var list = new SubmodelElementList(definition.ListIdShort)
        {
            OrderRelevant = definition.OrderRelevant,
            TypeValueListElement = ResolveModelType(definition.TypeValueListElement),
            ValueTypeListElement = ResolveDataType(definition.ValueTypeListElement)
        };

        if (definition.ListSemanticId is not null)
        {
            list.SemanticId = definition.ListSemanticId;
        }
        else
        {
            list.SemanticId = null;
        }
        WithoutKind(list);

        var entries = definition.Entries ?? Array.Empty<PropertyValueDefinition>();
        foreach (var entry in entries)
        {
            var property = SubmodelElementFactory.CreateProperty(
                entry.IdShort ?? definition.ListIdShort,
                entry.Value,
                entry.SemanticId,
                entry.ValueType);

            if (entry.SemanticId is null && property is Property propertyElement)
            {
                propertyElement.SemanticId = null;
            }

            if (entry.IdShort == null && property is Property propertyElementNoId)
            {
                propertyElementNoId.IdShort = null;
            }

            WithoutKind(property as ISubmodelElement);
            list.Add(property);
        }

        return list;
    }

    private static SubmodelElementCollection CreateSimpleCollection(SimpleSubmodelElementCollectionDefinition definition)
    {
        var collection = new SubmodelElementCollection(definition.IdShort);
        if (definition.SemanticId is not null)
        {
            collection.SemanticId = definition.SemanticId;
        }

        WithoutKind(collection);
        return collection;
    }

    private static RelationshipElement CreateRelationshipElement(RelationshipElementDefinition definition)
    {
        var relationship = new RelationshipElement(definition.IdShort)
        {
            Category = definition.Category,
            Description = CloneLangStrings(definition.Description),
            Value = new RelationshipElementValue(definition.First, definition.Second)
        };

        if (definition.SemanticId is not null)
        {
            relationship.SemanticId = definition.SemanticId;
        }

        ApplyQualifiers(definition.Qualifiers, qualifiers => relationship.Qualifiers = qualifiers);
        WithoutKind(relationship);
        return relationship;
    }

    private static Property CreateProperty(PropertyValueDefinition definition, bool ensureIdShort = false)
    {
        if (ensureIdShort && string.IsNullOrWhiteSpace(definition.IdShort))
        {
            throw new ArgumentException("Property definition requires an idShort.");
        }

        var property = SubmodelElementFactory.CreateProperty(
            definition.IdShort ?? string.Empty,
            definition.Value,
            definition.SemanticId,
            definition.ValueType);

        WithoutKind(property);
        return property;
    }

    private static void WithoutKind(ISubmodelElement? element)
    {
        if (element is SubmodelElement submodelElement)
        {
            submodelElement.Kind = default;
        }
    }

    private static void ApplyQualifiers(IReadOnlyList<IQualifier>? qualifiers, Action<ICollection<IQualifier>> assign)
    {
        if (qualifiers is { Count: > 0 })
        {
            assign(new List<IQualifier>(qualifiers));
        }
    }

    private static LangStringSet? CloneLangStrings(LangStringSet? source)
    {
        if (source == null)
        {
            return null;
        }

        var clone = new List<LangString>();
        foreach (var entry in source)
        {
            clone.Add(new LangString(entry.Language, entry.Text));
        }

        return new LangStringSet(clone);
    }

    private static DataType ResolveDataType(string? valueType)
    {
        if (string.IsNullOrWhiteSpace(valueType))
        {
            return new DataType(DataObjectType.String);
        }

        return DataObjectType.TryParse(valueType, out var parsed)
            ? new DataType(parsed)
            : new DataType(DataObjectType.String);
    }

    private static ModelType ResolveModelType(string? modelType)
    {
        if (string.IsNullOrWhiteSpace(modelType))
        {
            return ModelType.Property;
        }

        return modelType.Equals("ReferenceElement", StringComparison.OrdinalIgnoreCase)
            ? ModelType.ReferenceElement
            : modelType.Equals("SubmodelElementCollection", StringComparison.OrdinalIgnoreCase)
                ? ModelType.SubmodelElementCollection
                : modelType.Equals("Capability", StringComparison.OrdinalIgnoreCase)
                    ? ModelType.Capability
                    : ModelType.Property;
    }
}

internal static class CapabilityDescriptionJsonNormalizer
{
    public static string Normalize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return json;
        }

        var root = JsonNode.Parse(json);
        if (root is null)
        {
            return json;
        }

        NormalizeNode(root, true);
        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private static void NormalizeNode(JsonNode? node, bool isRoot)
    {
        switch (node)
        {
            case JsonObject obj:
                PruneObject(obj, isRoot);
                foreach (var property in obj.ToList())
                {
                    NormalizeNode(property.Value, false);
                }

                break;
            case JsonArray array:
                foreach (var item in array)
                {
                    NormalizeNode(item, false);
                }

                break;
        }
    }

    private static void PruneObject(JsonObject obj, bool isRoot)
    {
        if (!isRoot && obj.ContainsKey("modelType"))
        {
            obj.Remove("kind");
        }

        RemoveEmptyArray(obj, "supplementalSemanticIds");
        RemoveEmptyArray(obj, "embeddedDataSpecifications");
        RemoveEmptyArray(obj, "qualifiers");
        RemoveEmptyArray(obj, "description");
        RemoveEmptyArray(obj, "displayName");

        RemoveNull(obj, "conceptDescription");
        RemoveNull(obj, "category");
        RemoveNull(obj, "administration");

        if (obj.TryGetPropertyValue("idShort", out var idShortNode))
        {
            if (idShortNode is null)
            {
                obj.Remove("idShort");
            }
            else if (idShortNode is JsonValue idShortValue &&
                     idShortValue.TryGetValue<string>(out var idShort) &&
                     string.IsNullOrEmpty(idShort))
            {
                obj.Remove("idShort");
            }
        }

        if (obj.TryGetPropertyValue("semanticId", out var semanticNode) && semanticNode is JsonObject semanticObj)
        {
            semanticObj.Remove("referredSemanticId");
            if (semanticObj.TryGetPropertyValue("keys", out var keysNode) &&
                keysNode is JsonArray keysArray &&
                keysArray.Count == 0)
            {
                obj.Remove("semanticId");
            }
        }
    }

    private static void RemoveEmptyArray(JsonObject obj, string propertyName)
    {
        if (obj.TryGetPropertyValue(propertyName, out var arrayNode) && arrayNode is JsonArray array && array.Count == 0)
        {
            obj.Remove(propertyName);
        }
    }

    private static void RemoveNull(JsonObject obj, string propertyName)
    {
        if (obj.TryGetPropertyValue(propertyName, out var valueNode) && valueNode == null)
        {
            obj.Remove(propertyName);
        }
    }
}

internal static class CapabilityDescriptionSemantics
{
    public static Reference Submodel { get; } = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/1/0/Submodel"));

    public static Reference CapabilitySet { get; } = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/CapabilitySet/1/0"));

    public static Reference CapabilityContainer { get; } = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/CapabilityContainer/1/0"));

    public static Reference Capability { get; } = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://wiki.eclipse.org/BaSyx_/_Documentation_/_Submodels_/_Capability#Capability"));

    public static Reference CapabilityRelations { get; } = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/CapabilityRelations/1/0"));

    public static Reference PropertySet { get; } = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/PropertySet/1/0"));

    public static Reference PropertyContainer { get; } = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/PropertyContainer/1/0"));

    public static Reference ConstraintSet { get; } = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/ConstraintSet/1/0"));

    public static Reference PropertyConstraintContainer { get; } = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/PropertyConstraintContainer/1/0"));

    public static Reference CustomConstraint { get; } = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/CustomConstraint/1/0"));
}

public static class CapabilityDescriptionQualifiers
{
    public static IReadOnlyList<IQualifier> Multiplicity(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Multiplicity value must not be empty.", nameof(value));
        }

        return new List<IQualifier>
        {
            new Qualifier
            {
                Type = "Multiplicity",
                Value = value,
                ValueType = new DataType(DataObjectType.String)
            }
        };
    }
}

public sealed record CapabilityDescriptionTemplate(
    string Identifier,
    CapabilitySetDefinition CapabilitySet,
    string? SubmodelIdShort = CapabilityDescriptionSubmodel.DefaultIdShort,
    Reference? SemanticId = null);

public sealed record CapabilitySetDefinition(
    string IdShort,
    IReadOnlyList<CapabilityContainerDefinition> CapabilityContainers,
    Reference? SemanticId = null,
    LangStringSet? Description = null,
    IReadOnlyList<IQualifier>? Qualifiers = null);

public sealed record CapabilityContainerDefinition(
    string IdShort,
    CapabilityElementDefinition Capability,
    Reference? SemanticId = null,
    LangStringSet? Description = null,
    IReadOnlyList<IQualifier>? Qualifiers = null,
    MultiLanguagePropertyDefinition? Comment = null,
    CapabilityRelationsDefinition? Relations = null,
    CapabilityPropertySetDefinition? PropertySet = null,
    IReadOnlyList<ISubmodelElement>? AdditionalElements = null);

public sealed record CapabilityElementDefinition(
    string IdShort,
    Reference? SemanticId = null,
    IReadOnlyList<IQualifier>? Qualifiers = null);

public sealed record MultiLanguagePropertyDefinition(
    string IdShort,
    LangStringSet Value,
    LangStringSet? Description = null,
    IReadOnlyList<IQualifier>? Qualifiers = null,
    Reference? SemanticId = null);

public sealed record CapabilityRelationsDefinition(
    string IdShort,
    IReadOnlyList<RelationshipElementDefinition> Relationships,
    CapabilityConstraintSetDefinition? ConstraintSet = null,
    IReadOnlyList<SimpleSubmodelElementCollectionDefinition>? AdditionalCollections = null,
    Reference? SemanticId = null,
    LangStringSet? Description = null,
    IReadOnlyList<IQualifier>? Qualifiers = null);

public sealed record CapabilityConstraintSetDefinition(
    string IdShort,
    IReadOnlyList<PropertyConstraintContainerDefinition> ConstraintContainers,
    Reference? SemanticId = null);

public sealed record PropertyConstraintContainerDefinition(
    string IdShort,
    PropertyValueDefinition ConditionalType,
    PropertyValueDefinition ConstraintType,
    CustomConstraintDefinition CustomConstraint,
    IReadOnlyList<RelationshipElementDefinition>? PropertyRelations = null,
    Reference? SemanticId = null,
    string? PropertyRelationsIdShort = null,
    Reference? PropertyRelationsSemanticId = null);

public sealed record CustomConstraintDefinition(
    string IdShort,
    IReadOnlyList<PropertyValueDefinition> Properties,
    Reference? SemanticId = null);

public sealed record CapabilityPropertySetDefinition(
    string IdShort,
    IReadOnlyList<CapabilityPropertyContainerDefinition> Containers,
    Reference? SemanticId = null,
    LangStringSet? Description = null,
    IReadOnlyList<IQualifier>? Qualifiers = null);

public abstract record CapabilityPropertyContainerDefinition(
    string IdShort,
    Reference? SemanticId = null,
    MultiLanguagePropertyDefinition? Comment = null,
    IReadOnlyList<IQualifier>? Qualifiers = null);

public sealed record RangePropertyContainerDefinition(
    string IdShort,
    string PropertyIdShort,
    string MinValue,
    string MaxValue,
    string ValueType = "xs:double",
    Reference? SemanticId = null,
    MultiLanguagePropertyDefinition? Comment = null,
    IReadOnlyList<IQualifier>? Qualifiers = null,
    Reference? PropertySemanticId = null)
    : CapabilityPropertyContainerDefinition(IdShort, SemanticId, Comment, Qualifiers);

public sealed record PropertyValueContainerDefinition(
    string IdShort,
    string PropertyIdShort,
    string Value,
    string ValueType = "xs:string",
    Reference? SemanticId = null,
    MultiLanguagePropertyDefinition? Comment = null,
    IReadOnlyList<IQualifier>? Qualifiers = null,
    Reference? PropertySemanticId = null)
    : CapabilityPropertyContainerDefinition(IdShort, SemanticId, Comment, Qualifiers);

public sealed record PropertyListContainerDefinition(
    string IdShort,
    string ListIdShort,
    IReadOnlyList<PropertyValueDefinition> Entries,
    Reference? SemanticId = null,
    MultiLanguagePropertyDefinition? Comment = null,
    IReadOnlyList<IQualifier>? Qualifiers = null,
    bool OrderRelevant = true,
    string TypeValueListElement = "Property",
    string ValueTypeListElement = "xs:string",
    Reference? ListSemanticId = null)
    : CapabilityPropertyContainerDefinition(IdShort, SemanticId, Comment, Qualifiers);

public sealed record PropertyValueDefinition(
    string? IdShort,
    string Value,
    string ValueType = "xs:string",
    Reference? SemanticId = null);

public sealed record SimpleSubmodelElementCollectionDefinition(
    string IdShort,
    Reference? SemanticId = null);

public sealed record RelationshipElementDefinition(
    string IdShort,
    Reference First,
    Reference Second,
    string? Category = null,
    LangStringSet? Description = null,
    IReadOnlyList<IQualifier>? Qualifiers = null,
    Reference? SemanticId = null);
