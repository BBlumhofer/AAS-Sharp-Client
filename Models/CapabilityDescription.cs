using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaSyx.Models.AdminShell;
using Range = BaSyx.Models.AdminShell.Range;

namespace AasSharpClient.Models;

public sealed class CapabilityDescriptionSubmodel : Submodel
{
    private static readonly Reference SemanticReference = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://smartfactory.de/semantics/submodel/CapabilityDescription#1/0"));

    private static readonly Reference CapabilitySetSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/OfferedCapabilitiyDescription/CapabilitySet#1/0"));

    private static readonly Reference CapabilityContainerSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/OfferedCapabilitiyDescription/CapabilitySet/FullyAutomatedAssemblyContainer#1/0"));

    private static readonly Reference CapabilityRelationsSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/CapabilityRelations/1/0"));

    private static readonly Reference PropertySetSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/OfferedCapabilitiyDescription/CapabilitySet/FullyAutomatedAssemblyContainer/PropertySet#1/0"));

    public SubmodelElementCollection CapabilitySet { get; }

    public CapabilityDescriptionSubmodel(string? submodelIdentifier = null)
        : base("OfferedCapabilitiyDescription", new Identifier(submodelIdentifier ?? Guid.NewGuid().ToString()))
    {
        Kind = ModelingKind.Instance;
        SemanticId = SemanticReference;

        CapabilitySet = new SubmodelElementCollection("CapabilitySet")
        {
            SemanticId = CapabilitySetSemantic
        };

        SubmodelElements.Add(CapabilitySet);
    }

    public static CapabilityDescriptionSubmodel CreateWithIdentifier(string submodelIdentifier) => new(submodelIdentifier);

    public Task<string> ToJsonAsync(CancellationToken cancellationToken = default) =>
        SubmodelSerialization.SerializeAsync(this, cancellationToken);

    /// <summary>
    /// Returns all Capability elements contained in the CapabilitySet.
    /// </summary>
    public IEnumerable<Capability> GetCapabilities()
    {
        // Search containers inside CapabilitySet for Capability elements
        foreach (var container in CapabilitySet.OfType<SubmodelElementCollection>())
        {
            foreach (var cap in container.OfType<Capability>())
            {
                yield return cap;
            }
        }
    }

    /// <summary>
    /// Returns the idShort of all capabilities found in the CapabilitySet.
    /// </summary>
    public IEnumerable<string> GetCapabilityNames()
    {
        return GetCapabilities().Select(c => c.IdShort ?? string.Empty).Where(s => !string.IsNullOrEmpty(s));
    }

    /// <summary>
    /// Finds a capability container by its idShort (e.g. "FullyAutomatedAssemblyContainer").
    /// </summary>
    public SubmodelElementCollection? FindCapabilityContainer(string idShort)
    {
        if (string.IsNullOrWhiteSpace(idShort)) return null;

        return CapabilitySet.OfType<SubmodelElementCollection>()
            .FirstOrDefault(c => string.Equals(c.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
    }

    public void Apply(CapabilityDescriptionData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        CapabilitySet.Clear();
        CapabilitySet.Add(CreateFullyAutomatedAssemblyContainer(data));
    }

    private SubmodelElementCollection CreateFullyAutomatedAssemblyContainer(CapabilityDescriptionData data)
    {
        var container = new SubmodelElementCollection("FullyAutomatedAssemblyContainer")
        {
            SemanticId = CapabilityContainerSemantic,
            Description = CloneLangStrings(data.ContainerDescription),
            Qualifiers = new List<IQualifier> { Multiplicity("OneToMany") }
        };

        container.Add(CreateCapabilityElement());
        container.Add(CreateCapabilityComment(data));
        container.Add(CreateCapabilityRelations(data));
        container.Add(CreatePropertySet(data.PropertyContainers));

        return container;
    }

    private static Capability CreateCapabilityElement()
    {
        return new Capability("FullyAutomatedAssembly")
        {
            SemanticId = ReferenceFactory.External((KeyType.GlobalReference, "https://wiki.eclipse.org/BaSyx_/_Documentation_/_Submodels_/_Capability#Capability")),
            Qualifiers = new List<IQualifier> { Multiplicity("One") }
        };
    }

    private static MultiLanguageProperty CreateCapabilityComment(CapabilityDescriptionData data)
    {
        var property = new MultiLanguageProperty("Comment")
        {
            Description = CloneLangStrings(data.CapabilityCommentDescription),
            Value = new MultiLanguagePropertyValue(CloneLangStrings(data.CapabilityCommentValue)),
            Qualifiers = new List<IQualifier> { Multiplicity("ZeroToOne") }
        };

        return property;
    }

    private SubmodelElementCollection CreateCapabilityRelations(CapabilityDescriptionData data)
    {
        var relations = new SubmodelElementCollection("CapabilityRelations")
        {
            SemanticId = CapabilityRelationsSemantic
        };

        relations.Add(CreateCapabilityRealizedBy(data.CapabilityRealizedByFirst, data.CapabilityRealizedBySecond));
        return relations;
    }

    private static RelationshipElement CreateCapabilityRealizedBy(Reference first, Reference second)
    {
        var relationship = new RelationshipElement("CapabilityRealizedBy")
        {
            Category = "PARAMETER",
            Description = Lang(("en-US", "Relationship between two elements"), ("de", "Beziehung zwischen zwei Elementen")),
            Qualifiers = new List<IQualifier> { Multiplicity("OneToMany") },
            Value = new RelationshipElementValue(first, second)
        };

        return relationship;
    }

    private SubmodelElementCollection CreatePropertySet(IReadOnlyList<PropertyContainerDefinition> definitions)
    {
        var propertySet = new SubmodelElementCollection("PropertySet")
        {
            SemanticId = PropertySetSemantic,
            Qualifiers = new List<IQualifier> { Multiplicity("ZeroToOne") }
        };

        foreach (var definition in definitions)
        {
            propertySet.Add(CreatePropertyContainer(definition));
        }

        return propertySet;
    }

    private SubmodelElementCollection CreatePropertyContainer(PropertyContainerDefinition definition)
    {
        var container = new SubmodelElementCollection(definition.IdShort)
        {
            SemanticId = ReferenceFactory.External((KeyType.GlobalReference, definition.SemanticUri)),
            Qualifiers = new List<IQualifier> { Multiplicity("OneToMany") }
        };

        container.Add(CreateCommentProperty(definition.Comment));

        switch (definition.Kind)
        {
            case PropertyContainerType.Range:
                container.Add(CreateRange(definition.RangeIdShort!, definition.RangeMin!, definition.RangeMax!));
                break;
            case PropertyContainerType.Property:
                container.Add(CreateFixedProperty(definition.PropertyIdShort!, definition.PropertyValue!, definition.PropertyValueType!));
                break;
            case PropertyContainerType.List:
                container.Add(CreateMaterialList(definition.ListIdShort!, definition.ListValues!));
                break;
        }

        return container;
    }

    private static MultiLanguageProperty CreateCommentProperty(LangStringSet comment)
    {
        return new MultiLanguageProperty("Comment")
        {
            Value = new MultiLanguagePropertyValue(CloneLangStrings(comment)),
            Qualifiers = new List<IQualifier> { Multiplicity("ZeroToOne") }
        };
    }

    private static Range CreateRange(string idShort, string min, string max)
    {
        var range = new Range(idShort, new DataType(DataObjectType.Double))
        {
            Value = new RangeValue(new ElementValue<string>(min), new ElementValue<string>(max))
        };

        return range;
    }

    private static Property CreateFixedProperty(string idShort, string value, string valueType)
    {
        return SubmodelElementFactory.CreateProperty(idShort, value, null, valueType);
    }

    private static SubmodelElementList CreateMaterialList(string idShort, IReadOnlyList<string> values)
    {
        var list = new SubmodelElementList(idShort)
        {
            TypeValueListElement = ModelType.Property,
            ValueTypeListElement = new DataType(DataObjectType.String)
        };

        foreach (var value in values)
        {
            var property = SubmodelElementFactory.CreateProperty(idShort, value, null, "xs:string");
            property.IdShort = null;
            list.Add(property);
        }

        return list;
    }

    private static IQualifier Multiplicity(string value) => new Qualifier
    {
        Type = "Multiplicity",
        Value = value,
        ValueType = new DataType(DataObjectType.String)
    };

    private static LangStringSet CloneLangStrings(LangStringSet source)
    {
        return new LangStringSet(source.Select(entry => new LangString(entry.Language, entry.Text)));
    }

    private static LangStringSet Lang(params (string Language, string Text)[] entries)
    {
        return new LangStringSet(entries.Select(entry => new LangString(entry.Language, entry.Text)));
    }
}

public sealed record CapabilityDescriptionData(
    string SubmodelIdentifier,
    LangStringSet ContainerDescription,
    LangStringSet CapabilityCommentDescription,
    LangStringSet CapabilityCommentValue,
    Reference CapabilityRealizedByFirst,
    Reference CapabilityRealizedBySecond,
    IReadOnlyList<PropertyContainerDefinition> PropertyContainers);

public enum PropertyContainerType
{
    Range,
    Property,
    List
}

public sealed record PropertyContainerDefinition(
    string IdShort,
    string SemanticUri,
    LangStringSet Comment,
    PropertyContainerType Kind,
    string? RangeIdShort = null,
    string? RangeMin = null,
    string? RangeMax = null,
    string? PropertyIdShort = null,
    string? PropertyValue = null,
    string? PropertyValueType = null,
    string? ListIdShort = null,
    IReadOnlyList<string>? ListValues = null);
