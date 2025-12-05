using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models;

public sealed class ProductIdentificationSubmodel : Submodel
{
    private static readonly Reference SemanticReference = ReferenceFactory.External(
        (KeyType.Submodel, "https://smartfactory.de/semantics/submodel/ProductIdentification#1/0"));

    private static readonly Reference IdentifierSemantic = ReferenceFactory.External(
        (KeyType.ConceptDescription, "0112/2///61360_4#ACB025#001"));

    private static readonly Reference ProductNameSemantic = ReferenceFactory.External(
        (KeyType.ConceptDescription, "0112/2///61360_4#ACB024#002"));

    private static readonly Reference ProductFamilySemantic = ReferenceFactory.External(
        (KeyType.ConceptDescription, "0112/2///61360_4#ACB006#001"));

    private static readonly Reference CountryOfOriginSemantic = ReferenceFactory.External(
        (KeyType.ConceptDescription, "0173-1#02-AAO841#001"));

    private static readonly Reference OrderNumberSemantic = ReferenceFactory.External(
        (KeyType.ConceptDescription, "0173-1#02-AAO663#003"));

    private static readonly Reference OrderTimestampSemantic = ReferenceFactory.External(
        (KeyType.ConceptDescription, "0173-1#02-AAO663#003"));

    private static readonly Reference BrandSemantic = ReferenceFactory.External(
        (KeyType.ConceptDescription, "0173-1#02-AAO742#002"));

    private static readonly Reference EffectiveDateSemantic = ReferenceFactory.External(
        (KeyType.ConceptDescription, "0112/2///61360_4#ACB027#001"));

    public ProductIdentificationSubmodel(string? submodelIdentifier = null)
        : base("ProductIdentification", new Identifier(submodelIdentifier ?? Guid.NewGuid().ToString()))
    {
        Kind = ModelingKind.Instance;
        SemanticId = SemanticReference;
    }

    public static ProductIdentificationSubmodel CreateWithIdentifier(string submodelIdentifier) => new(submodelIdentifier);

    public Task<string> ToJsonAsync(CancellationToken cancellationToken = default) => SubmodelSerialization.SerializeAsync(this, cancellationToken);

    public void Apply(ProductIdentificationData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        SubmodelElements.Clear();
        SubmodelElements.Add(SubmodelElementFactory.CreateStringProperty("Identifier", data.Identifier, IdentifierSemantic));
        SubmodelElements.Add(SubmodelElementFactory.CreateStringProperty("ProductName", data.ProductName, ProductNameSemantic));
        SubmodelElements.Add(SubmodelElementFactory.CreateStringProperty("ProductFamilyName", data.ProductFamilyName, ProductFamilySemantic));
        SubmodelElements.Add(SubmodelElementFactory.CreateStringProperty("ProductCountryOfOrigin", data.ProductCountryOfOrigin, CountryOfOriginSemantic));
        SubmodelElements.Add(SubmodelElementFactory.CreateStringProperty("OrderNumber", data.OrderNumber, OrderNumberSemantic));
        SubmodelElements.Add(SubmodelElementFactory.CreateProperty("OrderTimestamp", data.OrderTimestamp, OrderTimestampSemantic, "xs:integer"));
        SubmodelElements.Add(SubmodelElementFactory.CreateStringProperty("Brand", data.Brand, BrandSemantic));
        SubmodelElements.Add(SubmodelElementFactory.CreateStringProperty("EffectiveDate", data.EffectiveDate, EffectiveDateSemantic));
        SubmodelElements.Add(CreateCargoHazardReference(data.CargoHazardReferenceKeys));
    }

    public string? GetIdentifier() => GetStringPropertyValue("Identifier");
    public void SetIdentifier(string value) => SetStringPropertyValue("Identifier", value, IdentifierSemantic);

    public string? GetProductName() => GetStringPropertyValue("ProductName");
    public void SetProductName(string value) => SetStringPropertyValue("ProductName", value, ProductNameSemantic);

    public string? GetProductFamilyName() => GetStringPropertyValue("ProductFamilyName");
    public void SetProductFamilyName(string value) => SetStringPropertyValue("ProductFamilyName", value, ProductFamilySemantic);

    public string? GetProductCountryOfOrigin() => GetStringPropertyValue("ProductCountryOfOrigin");
    public void SetProductCountryOfOrigin(string value) => SetStringPropertyValue("ProductCountryOfOrigin", value, CountryOfOriginSemantic);

    public string? GetOrderNumber() => GetStringPropertyValue("OrderNumber");
    public void SetOrderNumber(string value) => SetStringPropertyValue("OrderNumber", value, OrderNumberSemantic);

    public string? GetOrderTimestamp() => GetPropertyValue("OrderTimestamp");
    public void SetOrderTimestamp(string value) => SetPropertyValue("OrderTimestamp", value, OrderTimestampSemantic, "xs:integer");

    public string? GetBrand() => GetStringPropertyValue("Brand");
    public void SetBrand(string value) => SetStringPropertyValue("Brand", value, BrandSemantic);

    public string? GetEffectiveDate() => GetStringPropertyValue("EffectiveDate");
    public void SetEffectiveDate(string value) => SetStringPropertyValue("EffectiveDate", value, EffectiveDateSemantic);

    public bool HasCargoHazardReference()
    {
        var reference = GetCargoHazardReference();
        return reference is not null && reference.Keys?.Any() == true;
    }

    public Reference? GetCargoHazardReference()
    {
        return GetCargoHazardReferenceElement()?.Value?.Value as Reference;
    }

    public void SetCargoHazardReference(IReadOnlyList<(KeyType Type, string Value)> referenceKeys)
    {
        if (referenceKeys is null || referenceKeys.Count == 0)
        {
            throw new ArgumentException("At least one reference key is required.", nameof(referenceKeys));
        }

        var referenceElement = GetOrCreateCargoHazardReferenceElement();
        referenceElement.Value = new ReferenceElementValue(ReferenceFactory.Model(referenceKeys.ToArray()));
    }

    public bool CargoHazardReferenceEquals(Reference reference)
    {
        if (reference is null)
        {
            throw new ArgumentNullException(nameof(reference));
        }

        var existing = GetCargoHazardReference();
        return AreReferencesEqual(existing, reference);
    }

    public static ReferenceElement CreateCargoHazardReference(IReadOnlyList<(KeyType Type, string Value)> referenceKeys)
    {
        if (referenceKeys is null || referenceKeys.Count == 0)
        {
            throw new ArgumentException("At least one reference key is required.", nameof(referenceKeys));
        }

        var reference = ReferenceFactory.Model(referenceKeys.ToArray());
        return new ReferenceElement("CargoHazardClass")
        {
            Value = new ReferenceElementValue(reference)
        };
    }

    private string? GetStringPropertyValue(string idShort)
    {
        return SubmodelElements
            .OfType<Property<string>>()
            .FirstOrDefault(p => string.Equals(p.IdShort, idShort, StringComparison.OrdinalIgnoreCase))?
            .Value.Value?.ToString();
    }

    private void SetStringPropertyValue(string idShort, string value, Reference semanticId)
    {
        var property = SubmodelElements
            .OfType<Property<string>>()
            .FirstOrDefault(p => string.Equals(p.IdShort, idShort, StringComparison.OrdinalIgnoreCase));

        if (property is null)
        {
            property = SubmodelElementFactory.CreateStringProperty(idShort, value, semanticId);
            SubmodelElements.Add(property);
            return;
        }

        property.Value = new PropertyValue<string>(value ?? string.Empty);
    }

    private string? GetPropertyValue(string idShort)
    {
        return FindProperty(idShort)?.Value.Value?.ToString();
    }

    private void SetPropertyValue(string idShort, object? value, Reference semanticId, string valueType)
    {
        var existing = FindProperty(idShort);
        if (existing is not null)
        {
            SubmodelElements.Remove(existing);
        }

        var property = SubmodelElementFactory.CreateProperty(idShort, value, semanticId, valueType);
        SubmodelElements.Add(property);
    }

    private Property? FindProperty(string idShort)
    {
        return SubmodelElements
            .OfType<Property>()
            .FirstOrDefault(p => string.Equals(p.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
    }

    private ReferenceElement? GetCargoHazardReferenceElement()
    {
        return SubmodelElements
            .OfType<ReferenceElement>()
            .FirstOrDefault(e => string.Equals(e.IdShort, "CargoHazardClass", StringComparison.OrdinalIgnoreCase));
    }

    private ReferenceElement GetOrCreateCargoHazardReferenceElement()
    {
        var element = GetCargoHazardReferenceElement();
        if (element is not null)
        {
            return element;
        }

        element = new ReferenceElement("CargoHazardClass");
        SubmodelElements.Add(element);
        return element;
    }

    private static bool AreReferencesEqual(Reference? current, Reference target)
    {
        if (current is null)
        {
            return false;
        }

        var currentKeys = current.Keys?.ToList() ?? new List<IKey>();
        var targetKeys = target.Keys?.ToList() ?? new List<IKey>();

        if (currentKeys.Count != targetKeys.Count)
        {
            return false;
        }

        for (int i = 0; i < currentKeys.Count; i++)
        {
            var currentKey = currentKeys[i];
            var targetKey = targetKeys[i];
            if (currentKey.Type != targetKey.Type)
            {
                return false;
            }

            if (!string.Equals(currentKey.Value, targetKey.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}

public sealed record ProductIdentificationData(
    string SubmodelIdentifier,
    string Identifier,
    string ProductName,
    string ProductFamilyName,
    string ProductCountryOfOrigin,
    string OrderNumber,
    string OrderTimestamp,
    string Brand,
    string EffectiveDate,
    IReadOnlyList<(KeyType Type, string Value)> CargoHazardReferenceKeys);
