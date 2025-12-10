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
        var template = CapabilityDescriptionTests.BuildOfferedCapabilityTemplate();
        var submodel = new CapabilityDescriptionSubmodel(template.Identifier);
        submodel.Apply(template);

        var names = submodel.GetCapabilityNames().ToList();
        Assert.Contains("FullyAutomatedAssembly", names);
    }

    [Fact]
    public void FindCapabilityContainer_Finds_ByIdShort()
    {
        var template = CapabilityDescriptionTests.BuildOfferedCapabilityTemplate();
        var submodel = new CapabilityDescriptionSubmodel(template.Identifier);
        submodel.Apply(template);

        var container = submodel.FindCapabilityContainer("FullyAutomatedAssemblyContainer");
        Assert.NotNull(container);
        Assert.Equal("FullyAutomatedAssemblyContainer", container!.IdShort);
    }
}
