using System.Linq;
using System.Threading.Tasks;
using AasSharpClient.Models;
using Xunit;

namespace AasSharpClient.Tests;

public class CapabilityDescriptionHelpersTests
{
    [Fact]
    public void GetCapabilities_Returns_CapabilityElements()
    {
        var data = CapabilityDescriptionTests.BuildCapabilityDescriptionData();
        var submodel = CapabilityDescriptionSubmodel.CreateWithIdentifier(data.SubmodelIdentifier);
        submodel.Apply(data);

        var names = submodel.GetCapabilityNames().ToList();
        Assert.Contains("FullyAutomatedAssembly", names);
    }

    [Fact]
    public void FindCapabilityContainer_Finds_ByIdShort()
    {
        var data = CapabilityDescriptionTests.BuildCapabilityDescriptionData();
        var submodel = CapabilityDescriptionSubmodel.CreateWithIdentifier(data.SubmodelIdentifier);
        submodel.Apply(data);

        var container = submodel.FindCapabilityContainer("FullyAutomatedAssemblyContainer");
        Assert.NotNull(container);
        Assert.Equal("FullyAutomatedAssemblyContainer", container!.IdShort);
    }
}
