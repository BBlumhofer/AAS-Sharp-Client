using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;
using Xunit;

namespace AasSharpClient.Tests;

public class MachineScheduleTests
{
    [Fact]
    public async Task MachineScheduleTemplateMatchesJson()
    {
        var data = new MachineScheduleData(
            "https://template.smartfactory.de/sm/MachineSchedule",
            null,
            false,
            new List<ISubmodelElement>());

        var submodel = MachineScheduleSubmodel.CreateWithIdentifier(data.SubmodelIdentifier);
        submodel.Apply(data);

        var actual = await submodel.ToJsonAsync();
        var expected = await File.ReadAllTextAsync("TestData/Test_SM_MachineSchedule.json");

        TestHelpers.AssertJsonEqual(expected, actual);
    }
}
