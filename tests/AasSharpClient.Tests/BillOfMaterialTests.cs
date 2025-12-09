using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AasSharpClient.Models;
using Xunit;

namespace AasSharpClient.Tests;

public class BillOfMaterialTests
{
    [Fact]
    public async Task BillOfMaterialTemplateMatchesJson()
    {
        var expected = await File.ReadAllTextAsync("TestData/Test_SM_BoM_Truck.json");
        using var doc = JsonDocument.Parse(expected);
        var templateId = doc.RootElement.GetProperty("id").GetString();
        var submodel = BillOfMaterialSubmodel.CreateWithIdentifier(
            templateId ?? Guid.NewGuid().ToString(),
            new[] { BillOfMaterialSubmodel.DefaultSupplementalSemanticReference });

        // Build the same structure as the template programmatically
        var truck = submodel.AddElement(
            "Truck",
            "https://smartfactory.de/shells/5ee202c4-cbef-42c7-b5bd-020dc1b0ca07",
            1,
            "Truck");

        var semitrailer = truck.AddSubElement(
            "Semitrailer",
            "https://smartfactory.de/shells/fe7f1e29-7fd2-44e9-9e5a-5d95852d3f83",
            1,
            "Semitrailer");

        semitrailer.AddSubElement(
            "Semitrailer_Chassis",
            "https://smartfactory.de/shells/ea68adec-e8a5-48a1-bbec-752ee6a7560d",
            1,
            "Semitrailer_Chassis");

        semitrailer.AddSubElement(
            "Trailer_Body_Blue",
            "https://smartfactory.de/shells/a1a14718-84c1-4578-9214-56bf166d12a0",
            1,
            "Trailer_Body_Blue");

        var semitrailerTruck = truck.AddSubElement(
            "Semitrailer_Truck",
            "https://smartfactory.de/shells/e6a4cbf2-89a5-485e-b926-9c98a334be72",
            1,
            "Semitrailer_Truck");

        semitrailerTruck.AddSubElement(
            "Cab_Chassis",
            "https://smartfactory.de/shells/e6dedf92-1468-4b1b-80e9-095ded9eb4b0",
            1,
            "Cab_Chassis");

        semitrailerTruck.AddSubElement(
            "Cab_A_Blue",
            "https://smartfactory.de/shells/ae2ccf5d-2a59-4713-b446-291acfd923b7",
            1,
            "Cab_A_Blue");

        var actual = await submodel.ToJsonAsync();

        TestHelpers.AssertJsonEqual(expected, actual);
    }
}
