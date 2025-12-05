using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models;

public sealed class NameplateSubmodel : Submodel
{
    private static readonly Reference SemanticReference = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://smartfactory.de/semantics/submodel/Island/Nameplate/#3/0"));

    private static readonly Reference ManufacturerUriSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://smartfactory.de/Productionsinsel_KUBA"));

    private static readonly Reference ManufacturerNameSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0173-1#02-AAO677#002"));

    private static readonly Reference ManufacturerProductDesignationSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0112/2///61987#ABA567#009"));

    private static readonly Reference AddressInformationSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0173-1#02-AAQ832#005"));

    private static readonly Reference StreetSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0173-1#02-AAO128#002"));

    private static readonly Reference ZipcodeSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0173-1#02-AAO129#002"));

    private static readonly Reference CityTownSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0173-1#02-AAO132#002"));

    private static readonly Reference StateCountySemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0173-1#02-AAO133#002"));

    private static readonly Reference NationalCodeSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0173-1#02-AAO134#002"));

    private static readonly Reference PhoneSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0173-1#02-AAQ833#005"));

    private static readonly Reference TelephoneNumberSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0173-1#02-AAO136#002"));

    private static readonly Reference TypeOfTelephoneSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0173-1#02-AAO137#003"));

    private static readonly Reference EmailSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0173-1#02-AAQ836#005"));

    private static readonly Reference EmailAddressSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0173-1#02-AAO198#002"));

    private static readonly Reference TypeOfEmailSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0173-1#02-AAO199#003"));

    private static readonly Reference OrderCodeSemantic = ReferenceFactory.External(
        (KeyType.GlobalReference, "0112/2///61987#ABA950#008"));

    public NameplateSubmodel(string? submodelIdentifier = null)
        : base("Nameplate", new Identifier(submodelIdentifier ?? Guid.NewGuid().ToString()))
    {
        Kind = ModelingKind.Instance;
        SemanticId = SemanticReference;
    }

    public static NameplateSubmodel CreateWithIdentifier(string submodelIdentifier) => new(submodelIdentifier);

    public Task<string> ToJsonAsync(CancellationToken cancellationToken = default) =>
        SubmodelSerialization.SerializeAsync(this, cancellationToken);

    public void Apply(NameplateData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        Description = CloneLangStrings(data.Description);
        Administration = new AdministrativeInformation
        {
            Version = data.Administration.Version,
            Revision = data.Administration.Revision,
            TemplateId = new Identifier(data.Administration.TemplateId)
        };

        SubmodelElements.Clear();
        BuildElements(data);
    }

    private void BuildElements(NameplateData data)
    {
        SubmodelElements.Add(CreateManufacturerUriProperty(data));
        SubmodelElements.Add(CreateMultiLanguageProperty("ManufacturerName", ManufacturerNameSemantic, data.ManufacturerName, "PARAMETER"));
        SubmodelElements.Add(CreateManufacturerProductDesignation(data));
        SubmodelElements.Add(CreateAddressInformation(data.Address));
        SubmodelElements.Add(CreateOrderCodeProperty(data.OrderCodeOfManufacturer));
    }

    private Property CreateManufacturerUriProperty(NameplateData data)
    {
        var property = SubmodelElementFactory.CreateProperty("URIOfTheManufacturer", data.ManufacturerUri, ManufacturerUriSemantic, "xs:anyURI");
        property.Description = CloneLangStrings(data.ManufacturerUriDescription);
        property.Qualifiers = new List<IQualifier>
        {
            new Qualifier
            {
                Type = "Multiplicity",
                Value = "One",
                ValueType = new DataType(DataObjectType.String)
            }
        };
        return property;
    }

    private MultiLanguageProperty CreateManufacturerProductDesignation(NameplateData data)
    {
        var property = CreateMultiLanguageProperty("ManufacturerProductDesignation", ManufacturerProductDesignationSemantic, data.ManufacturerProductDesignation);
        property.SupplementalSemanticIds = new[]
        {
            ReferenceFactory.External((KeyType.GlobalReference, "0173-1#02-AAW338#003"))
        };

        return property;
    }

    private SubmodelElementCollection CreateAddressInformation(AddressData address)
    {
        var collection = new SubmodelElementCollection("Addressinformation")
        {
            Category = "VARIABLE",
            SemanticId = AddressInformationSemantic
        };

        collection.Add(CreateMultiLanguageProperty("Street", StreetSemantic, address.Street, "PARAMETER"));
        collection.Add(CreateMultiLanguageProperty("Zipcode", ZipcodeSemantic, address.Zipcode, "VARIABLE"));
        collection.Add(CreateMultiLanguageProperty("CityTown", CityTownSemantic, address.CityTown, "VARIABLE"));
        collection.Add(CreateMultiLanguageProperty("StateCounty", StateCountySemantic, address.StateCounty, "VARIABLE"));
        collection.Add(CreateMultiLanguageProperty("NationalCode", NationalCodeSemantic, address.NationalCode, "VARIABLE"));
        collection.Add(CreatePhoneCollection(address.Phone));
        collection.Add(CreateEmailCollection(address.Email));

        return collection;
    }

    private SubmodelElementCollection CreatePhoneCollection(PhoneData phone)
    {
        var collection = new SubmodelElementCollection("Phone01")
        {
            Category = "VARIABLE",
            SemanticId = PhoneSemantic
        };

        collection.Add(CreateMultiLanguageProperty("TelephoneNumber", TelephoneNumberSemantic, phone.Number, "VARIABLE"));

        var typeProperty = SubmodelElementFactory.CreateProperty("TypeOfTelephone", phone.TypeOfTelephone, TypeOfTelephoneSemantic, "xs:string");
        typeProperty.Category = "VARIABLE";
        collection.Add(typeProperty);

        return collection;
    }

    private SubmodelElementCollection CreateEmailCollection(EmailData email)
    {
        var collection = new SubmodelElementCollection("Email01")
        {
            Category = "VARIABLE",
            SemanticId = EmailSemantic
        };

        var emailAddress = SubmodelElementFactory.CreateProperty("EmailAddress", email.Address, EmailAddressSemantic, "xs:string");
        emailAddress.Category = "VARIABLE";
        collection.Add(emailAddress);

        var typeOfEmail = SubmodelElementFactory.CreateProperty("TypeOfEmailAddress", email.TypeOfEmailAddress, TypeOfEmailSemantic, "xs:string");
        typeOfEmail.Category = "VARIABLE";
        collection.Add(typeOfEmail);

        return collection;
    }

    private Property CreateOrderCodeProperty(string orderCode)
    {
        var property = SubmodelElementFactory.CreateProperty("OrderCodeOfManufacturer", orderCode, OrderCodeSemantic, "xs:string");
        property.SupplementalSemanticIds = new[]
        {
            ReferenceFactory.External((KeyType.GlobalReference, "0173-1#02-AAO227#004"))
        };

        return property;
    }

    private MultiLanguageProperty CreateMultiLanguageProperty(string idShort, Reference semanticId, LangStringSet value, string? category = null)
    {
        var property = new MultiLanguageProperty(idShort)
        {
            SemanticId = semanticId,
            Value = new MultiLanguagePropertyValue(CloneLangStrings(value))
        };

        if (!string.IsNullOrEmpty(category))
        {
            property.Category = category;
        }

        return property;
    }

    private static LangStringSet CloneLangStrings(LangStringSet source)
    {
        return new LangStringSet(source.Select(entry => new LangString(entry.Language, entry.Text)));
    }
}

public sealed record NameplateData(
    string SubmodelIdentifier,
    LangStringSet Description,
    NameplateAdministrativeInfo Administration,
    string ManufacturerUri,
    LangStringSet ManufacturerUriDescription,
    LangStringSet ManufacturerName,
    LangStringSet ManufacturerProductDesignation,
    AddressData Address,
    string OrderCodeOfManufacturer);

public sealed record NameplateAdministrativeInfo(string Version, string Revision, string TemplateId);

public sealed record AddressData(
    LangStringSet Street,
    LangStringSet Zipcode,
    LangStringSet CityTown,
    LangStringSet StateCounty,
    LangStringSet NationalCode,
    PhoneData Phone,
    EmailData Email);

public sealed record PhoneData(LangStringSet Number, string TypeOfTelephone);

public sealed record EmailData(string Address, string TypeOfEmailAddress);
