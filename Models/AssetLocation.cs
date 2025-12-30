using System.Threading.Tasks;
using BaSyx.Models.AdminShell;
using System.Text.Json;
using System.Text.Json.Nodes;
using BaSyx.Models.Extensions;

namespace AasSharpClient.Models;

public sealed record AssetLocationData(
    string Address,
    string Parent,
    double X,
    double Y,
    double Theta);

/// <summary>
/// Strongly-typed representation of an AssetLocation SMC. Inherits from <see cref="SubmodelElementCollection"/>
/// so it integrates with the BaSyx model types and the existing JsonLoader serialization pipeline.
/// </summary>
public sealed class AssetLocation : SubmodelElementCollection
{
    public const string DefaultIdShort = "AssetLocation";

    public Property<string> Address { get; }
    public Property<string> Parent { get; }
    public SubmodelElementCollection Position { get; }
    public Property<string> X { get; }
    public Property<string> Y { get; }
    public Property<string> Theta { get; }

    public AssetLocation(string idShort = DefaultIdShort) : base(idShort)
    {
        // create BaSyx children like Action.cs does
        Address = SubmodelElementFactory.CreateStringProperty("Address", string.Empty);
        Parent = SubmodelElementFactory.CreateStringProperty("Parent", string.Empty);

        X = SubmodelElementFactory.CreateStringProperty("X", string.Empty);
        Y = SubmodelElementFactory.CreateStringProperty("Y", string.Empty);
        Theta = SubmodelElementFactory.CreateStringProperty("Theta", string.Empty);

        Position = new SubmodelElementCollection("Position");

        // ensure Position contains the coordinate properties
        Position.Add(X);
        Position.Add(Y);
        Position.Add(Theta);

        // Add main children to this collection
        Add(Address);
        Add(Parent);
        Add(Position);
    }

    public void Apply(AssetLocationData data)
    {
        if (data == null) return;

        IdShort = DefaultIdShort;

        Address.Value = new BaSyx.Models.AdminShell.PropertyValue<string>(data.Address);
        Parent.Value = new BaSyx.Models.AdminShell.PropertyValue<string>(data.Parent);

        X.Value = new BaSyx.Models.AdminShell.PropertyValue<string>(data.X.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Y.Value = new BaSyx.Models.AdminShell.PropertyValue<string>(data.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Theta.Value = new BaSyx.Models.AdminShell.PropertyValue<string>(data.Theta.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public static AssetLocation FromCollection(SubmodelElementCollection? coll)
    {
        var result = new AssetLocation();
        if (coll == null) return result;

        if (!string.IsNullOrWhiteSpace(coll.IdShort)) result.IdShort = coll.IdShort;

        // Populate known properties if present
        foreach (var el in coll)
        {
            if (el is BaSyx.Models.AdminShell.Property p)
            {
                var key = p.IdShort ?? string.Empty;
                switch (key)
                {
                    case "Address":
                        result.Address.Value = new BaSyx.Models.AdminShell.PropertyValue<string>(AasSharpClient.Models.Helpers.AasValueUnwrap.UnwrapToString(p.Value) ?? string.Empty);
                        break;
                    case "Parent":
                        result.Parent.Value = new BaSyx.Models.AdminShell.PropertyValue<string>(AasSharpClient.Models.Helpers.AasValueUnwrap.UnwrapToString(p.Value) ?? string.Empty);
                        break;
                    case "X":
                        result.X.Value = new BaSyx.Models.AdminShell.PropertyValue<string>(AasSharpClient.Models.Helpers.AasValueUnwrap.UnwrapToString(p.Value) ?? string.Empty);
                        break;
                    case "Y":
                        result.Y.Value = new BaSyx.Models.AdminShell.PropertyValue<string>(AasSharpClient.Models.Helpers.AasValueUnwrap.UnwrapToString(p.Value) ?? string.Empty);
                        break;
                    case "Theta":
                        result.Theta.Value = new BaSyx.Models.AdminShell.PropertyValue<string>(AasSharpClient.Models.Helpers.AasValueUnwrap.UnwrapToString(p.Value) ?? string.Empty);
                        break;
                    default:
                        // for unknown properties, add as child to the top-level collection
                        result.Add(el);
                        break;
                }
            }
            else if (el is SubmodelElementCollection childColl && string.Equals(childColl.IdShort, "Position", StringComparison.OrdinalIgnoreCase))
            {
                // extract coordinate props from nested Position collection
                foreach (var posEl in childColl)
                {
                    if (posEl is BaSyx.Models.AdminShell.Property pp)
                    {
                        var key = pp.IdShort ?? string.Empty;
                        switch (key)
                        {
                            case "X":
                                result.X.Value = new BaSyx.Models.AdminShell.PropertyValue<string>(AasSharpClient.Models.Helpers.AasValueUnwrap.UnwrapToString(pp.Value) ?? string.Empty);
                                break;
                            case "Y":
                                result.Y.Value = new BaSyx.Models.AdminShell.PropertyValue<string>(AasSharpClient.Models.Helpers.AasValueUnwrap.UnwrapToString(pp.Value) ?? string.Empty);
                                break;
                            case "Theta":
                                result.Theta.Value = new BaSyx.Models.AdminShell.PropertyValue<string>(AasSharpClient.Models.Helpers.AasValueUnwrap.UnwrapToString(pp.Value) ?? string.Empty);
                                break;
                            default:
                                result.Position.Add(posEl);
                                break;
                        }
                    }
                    else
                    {
                        result.Position.Add(posEl);
                    }
                }
            }
            else
            {
                // other elements => attach to top-level
                result.Add(el);
            }
        }

        return result;
    }
}

/// <summary>
/// Compatibility wrapper for components expecting a full <see cref="Submodel"/> instance.
/// Keeps the previous API surface used by other parts of the codebase (e.g. LoadSubmodelNodeBase).
/// Internally it populates the <see cref="Submodel.SubmodelElements"/> collection from an <see cref="AssetLocation"/> instance.
/// </summary>
public sealed class AssetLocationSubmodel : Submodel
{
    public const string DefaultIdShort = AssetLocation.DefaultIdShort;
    private static readonly Reference SemanticReference = ReferenceFactory.External(
        (KeyType.GlobalReference, "https://smartfactory.de/submodels/AssetLocation1#0"));

    public AssetLocationSubmodel(string? submodelIdentifier = null, string idShort = DefaultIdShort)
        : base(idShort, new Identifier(submodelIdentifier ?? System.Guid.NewGuid().ToString()))
    {
        Kind = ModelingKind.Instance;
        SemanticId = SemanticReference;
    }

    public async Task<string> ToJsonAsync(System.Threading.CancellationToken cancellationToken = default)
    {
        var serialized = await SubmodelSerialization.SerializeAsync(this, cancellationToken);
        return serialized;
    }

    public void Apply(AssetLocationData data)
    {
        if (data == null) return;

        IdShort = DefaultIdShort;

        // build a typed AssetLocation and copy its elements into this Submodel
        var typed = new AssetLocation();
        typed.Apply(data);

        // clear existing and copy
        SubmodelElements.Clear();
        foreach (var el in typed)
        {
            if (el != null)
            {
                SubmodelElements.Add(el);
            }
        }
    }

    public static AssetLocationSubmodel FromCollection(SubmodelElementCollection? coll)
    {
        var sm = new AssetLocationSubmodel();
        if (coll == null) return sm;

        foreach (var el in coll)
        {
            if (el != null)
            {
                sm.SubmodelElements.Add(el);
            }
        }

        return sm;
    }
}
