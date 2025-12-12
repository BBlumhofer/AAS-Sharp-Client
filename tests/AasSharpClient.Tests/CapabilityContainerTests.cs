using System.Linq;
using AasSharpClient.Models;
using Xunit;

namespace AasSharpClient.Tests;

public class CapabilityContainerTests
{
    [Fact]
    public void CapabilityContainerLoadsConstraintsAndPropertiesFromJson()
    {
        var collection = BasyxJsonLoader.LoadCollectionFromFile("Test_CapabilityContainer.json");
        var container = new CapabilityContainer(collection);

        Assert.Equal("Assemble", container.GetCapabilityName());

        Assert.NotEmpty(container.ConstraintDictionary);
        Assert.True(container.ConstraintDictionary.ContainsKey("StorageConstraint"));
        var constraint = container.ConstraintDictionary["StorageConstraint"];
        Assert.Equal("Pre", constraint.ConditionalType?.Value?.Value?.ToString());
        Assert.Equal("CustomConstraint", constraint.ConstraintType?.Value?.Value?.ToString());
        Assert.Equal("StorageConstraint", constraint.CustomConstraint?.GetProperty("ConstraintName")?.Value?.Value?.ToString());

        Assert.NotEmpty(container.PropertyContainerDictionary);
        Assert.True(container.PropertyContainerDictionary.ContainsKey("GripForceRange"));
        var gripForce = container.PropertyContainerDictionary["GripForceRange"];
        Assert.Equal("GripForce", gripForce.Range?.IdShort);
        Assert.Equal("10", gripForce.Range?.Value?.Min?.Value);
        Assert.Equal("50", gripForce.Range?.Value?.Max?.Value);

        Assert.True(container.PropertyContainerDictionary.ContainsKey("ProductIdFixed"));
        var productId = container.PropertyContainerDictionary["ProductIdFixed"];
        Assert.Equal("*", productId.Property?.Value?.Value?.ToString());
    }
}
