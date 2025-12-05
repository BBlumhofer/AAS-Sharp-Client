using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models;

public sealed class BillOfMaterialSubmodel : Submodel
{
    private static readonly Reference SemanticReference = ReferenceFactory.External(
        (KeyType.Submodel, "http://example.com/id/type/submodel/BOM/1/1"),
        (KeyType.Submodel, "https://smartfactory.de/semantics/submodel/Truck/BillOfMaterial#1/0"));

    // Parameterless constructor: programmatic, empty BOM (use AddElement/AddSubElement)
    public BillOfMaterialSubmodel()
        : base("BillOfMaterial", new Identifier(Guid.NewGuid().ToString()))
    {
        Kind = ModelingKind.Instance;
        SemanticId = SemanticReference;
    }

    // Create an empty, programmatic submodel but with a specific identifier
    public static BillOfMaterialSubmodel CreateWithIdentifier(string submodelIdentifier)
    {
        return new BillOfMaterialSubmodel(submodelIdentifier);
    }

    // Internal ctor used to set a known identifier
    private BillOfMaterialSubmodel(string submodelIdentifier)
        : base("BillOfMaterial", new Identifier(submodelIdentifier))
    {
        Kind = ModelingKind.Instance;
        SemanticId = SemanticReference;
    }

    public Task<string> ToJsonAsync(CancellationToken cancellationToken = default) => SubmodelSerialization.SerializeAsync(this, cancellationToken);

    /// <summary>
    /// Programmatically add a top-level BOM element.
    /// </summary>
    public BillOfMaterialElement AddElement(string idShort, string assetShellId, int quantity, string? name = null)
        => AddElement(idShort, assetShellId, quantity.ToString(), name);

    public BillOfMaterialElement AddElement(string idShort, string assetShellId, string quantity, string? name = null)
    {
        var entity = CreateEntityFromParams(idShort, assetShellId, name ?? idShort, quantity);
        SubmodelElements.Add(entity);
        return new BillOfMaterialElement(entity, this);
    }

    private Entity CreateEntityFromParams(string idShort, string assetShellId, string name, string quantity)
    {
        var entity = new Entity(idShort)
        {
            EntityType = EntityType.SelfManagedEntity
        };

        entity.Add(SubmodelElementFactory.CreateProperty("Id", assetShellId, null, "xs:string"));
        entity.Add(SubmodelElementFactory.CreateProperty("Name", name, null, "xs:string"));
        entity.Add(SubmodelElementFactory.CreateProperty("Quantity", quantity, null, "xs:string"));
        entity.Add(CreateUrlReference(assetShellId));

        return entity;
    }

    // CreateEntity(BillOfMaterialItem) removed — build programmatically via AddElement/AddSubElement

    internal static ReferenceElement CreateUrlReference(string shellId)
    {
        return new ReferenceElement("URL")
        {
            Value = new ReferenceElementValue(ReferenceFactory.External((KeyType.AssetAdministrationShell, shellId)))
        };
    }
}

/// <summary>
/// Small helper wrapper that exposes an API to add nested BOM elements to an Entity instance.
/// </summary>
public sealed class BillOfMaterialElement
{
    private readonly Entity _entity;
    private readonly BillOfMaterialSubmodel _submodel;

    internal BillOfMaterialElement(Entity entity, BillOfMaterialSubmodel submodel)
    {
        _entity = entity;
        _submodel = submodel;
    }

    public BillOfMaterialElement AddSubElement(string idShort, string assetShellId, int quantity, string? name = null)
        => AddSubElement(idShort, assetShellId, quantity.ToString(), name);

    public BillOfMaterialElement AddSubElement(string idShort, string assetShellId, string quantity, string? name = null)
    {
        var child = new Entity(idShort)
        {
            EntityType = EntityType.SelfManagedEntity
        };

        child.Add(SubmodelElementFactory.CreateProperty("Id", assetShellId, null, "xs:string"));
        child.Add(SubmodelElementFactory.CreateProperty("Name", name ?? idShort, null, "xs:string"));
        child.Add(SubmodelElementFactory.CreateProperty("Quantity", quantity, null, "xs:string"));
        child.Add(BillOfMaterialSubmodel.CreateUrlReference(assetShellId));

        _entity.Add(child);
        return new BillOfMaterialElement(child, _submodel);
    }
}

// Template data removed — build BOM programmatically via AddElement/AddSubElement
