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

    [Fact]
    public void ComposedOf_Decomposes_To_AtomicCapabilities()
    {
        const string submodelId = "https://example.org/submodels/test_composed";

        // Build three capability containers: parent + two atomic children
        var parentContainer = new CapabilityContainerDefinition(
            "DockRetrieveHandoverContainer",
            new CapabilityElementDefinition("DockRetrieveHandoverCapability"));

        var child1 = new CapabilityContainerDefinition(
            "RetrieveContainer",
            new CapabilityElementDefinition("Retrieve"));

        var child2 = new CapabilityContainerDefinition(
            "DockingContainer",
            new CapabilityElementDefinition("Docking"));

        var capabilitySet = new CapabilitySetDefinition(
            "CapabilitySet",
            new List<CapabilityContainerDefinition> { parentContainer, child1, child2 });

        // References to the capability elements inside the submodel
        var parentRef = ReferenceFactory.Model(
            (KeyType.Submodel, submodelId),
            (KeyType.SubmodelElementCollection, "CapabilitySet"),
            (KeyType.SubmodelElementCollection, "DockRetrieveHandoverContainer"),
            (KeyType.Capability, "DockRetrieveHandoverCapability"));

        var child1Ref = ReferenceFactory.Model(
            (KeyType.Submodel, submodelId),
            (KeyType.SubmodelElementCollection, "CapabilitySet"),
            (KeyType.SubmodelElementCollection, "RetrieveContainer"),
            (KeyType.Capability, "Retrieve"));

        var child2Ref = ReferenceFactory.Model(
            (KeyType.Submodel, submodelId),
            (KeyType.SubmodelElementCollection, "CapabilitySet"),
            (KeyType.SubmodelElementCollection, "DockingContainer"),
            (KeyType.Capability, "Docking"));

        var composedOf = new CapabilityComposedOfSetDefinition(
            "ComposedOfSet",
            new List<RelationshipElementDefinition>
            {
                new("Composed_Retrieve", parentRef, child1Ref),
                new("Composed_Docking", parentRef, child2Ref)
            });

        var relations = new CapabilityRelationsDefinition(
            "CapabilityRelations",
            new List<RelationshipElementDefinition>(),
            ComposedOfSet: composedOf);

        // attach relations to parent container definition
        var parentWithRelations = new CapabilityContainerDefinition(
            parentContainer.IdShort,
            new CapabilityElementDefinition("DockRetrieveHandoverCapability"),
            Relations: relations);

        var fullSet = new CapabilitySetDefinition(
            "CapabilitySet",
            new List<CapabilityContainerDefinition> { parentWithRelations, child1, child2 });

        var template = new CapabilityDescriptionTemplate(submodelId, fullSet, "TestSubmodel");

        var submodel = new CapabilityDescriptionSubmodel(template.Identifier);
        submodel.Apply(template);

        var container = submodel.FindCapabilityContainer("DockRetrieveHandoverContainer");
        Assert.NotNull(container);

        var capContainer = CapabilityContainer.FromSubmodelElement(container!);
        var composed = capContainer.Relations?.ComposedOf;
        Assert.NotNull(composed);

        var children = composed!.Select(rel => rel.Value.Second.Keys?.Last().Value).Where(v => !string.IsNullOrWhiteSpace(v)).ToList();

        Assert.Contains("Retrieve", children);
        Assert.Contains("Docking", children);
    }
}
