using System.Threading.Tasks;
using BaSyx.Models.AdminShell;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AasSharpClient.Models;

public sealed record AssetLocationData(
    string Address,
    string CurrentArea,
    double X,
    double Y,
    double Theta,
    int Floor);

public sealed class AssetLocationSubmodel : Submodel
{
    public const string DefaultIdShort = "AssetLocation";

    public AssetLocationSubmodel(string? submodelIdentifier = null, string idShort = DefaultIdShort)
        : base(idShort, new Identifier(submodelIdentifier ?? System.Guid.NewGuid().ToString()))
    {
        Kind = ModelingKind.Instance;
        // semantic id can be set externally if desired
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

        SubmodelElements.Clear();

        // Address as a single Property
        var addr = SubmodelElementFactory.CreateProperty("Address", data.Address, null, "xs:string");
        WithoutKind(addr);
        SubmodelElements.Add(addr);

        // Current area
        var area = SubmodelElementFactory.CreateProperty("CurrentArea", data.CurrentArea, null, "xs:string");
        WithoutKind(area);
        SubmodelElements.Add(area);

        // Position as a SubmodelElementCollection with X,Y,Theta and Floor
        var pos = new SubmodelElementCollection("Position");
        WithoutKind(pos);

        var px = SubmodelElementFactory.CreateProperty("X", data.X.ToString(System.Globalization.CultureInfo.InvariantCulture), null, "xs:double");
        WithoutKind(px);
        pos.Add(px);

        var py = SubmodelElementFactory.CreateProperty("Y", data.Y.ToString(System.Globalization.CultureInfo.InvariantCulture), null, "xs:double");
        WithoutKind(py);
        pos.Add(py);

        var pth = SubmodelElementFactory.CreateProperty("Theta", data.Theta.ToString(System.Globalization.CultureInfo.InvariantCulture), null, "xs:double");
        WithoutKind(pth);
        pos.Add(pth);

        var floor = SubmodelElementFactory.CreateProperty("Floor", data.Floor.ToString(), null, "xs:integer");
        WithoutKind(floor);
        pos.Add(floor);

        SubmodelElements.Add(pos);
    }

    private static void WithoutKind(ISubmodelElement? element)
    {
        if (element is SubmodelElement submodelElement)
        {
            submodelElement.Kind = default;
        }
    }
}
