using System.IO;
using System.Threading.Tasks;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;
using Xunit;

namespace AasSharpClient.Tests;

public class NameplateTests
{
    [Fact]
    public async Task NameplateTemplateMatchesJson()
    {
        var data = BuildNameplateData();
        var submodel = NameplateSubmodel.CreateWithIdentifier(data.SubmodelIdentifier);
        submodel.Apply(data);

        var actual = await submodel.ToJsonAsync();
        var expected = await File.ReadAllTextAsync("TestData/Test_SM_Nameplate.json");

        TestHelpers.AssertJsonEqual(expected, actual);
    }

    private static NameplateData BuildNameplateData()
    {
        var address = new AddressData(
            TestHelpers.Lang(("de", "Trippstadterstra\u00DFe 122")),
            TestHelpers.Lang(("en", "67663")),
            TestHelpers.Lang(("de", "Kaiserslautern")),
            TestHelpers.Lang(("en", "Germany")),
            TestHelpers.Lang(("en", "276")),
            new PhoneData(TestHelpers.Lang(("en", "+49631205753401")), string.Empty),
            new EmailData("info@smartfactory.de", string.Empty));

        return new NameplateData(
            "https://smartfactory.de/submodels/be03fcce-7e76-4758-934d-7dda6c858ff0",
            TestHelpers.Lang(("en", "Contains the nameplate information attached to the product")),
            new NameplateAdministrativeInfo("3", "0", "https://admin-shell.io/IDTA 02006-3-0"),
            "https://smartfactory.de/Productionsinsel_KUBA",
            TestHelpers.Lang(("en", "Note: see also [IRDI] 0112/2///61987#ABN590#001 URI of product instance ")),
            TestHelpers.Lang(("de", "PI_KUBA")),
            TestHelpers.Lang(("en", "\"ABC-123\"")),
            address,
            "FMABC1234");
    }
}
