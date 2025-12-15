using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;
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

    [Fact]
    public void P102CapabilityContainerIncludesRealizedByRelation()
    {
        var container = LoadP102CapabilityContainer();

        var relation = container.SkillRelation ?? container.RealizedBy.FirstOrDefault();

        Assert.NotNull(relation);
        Assert.Contains("RealizedBy", relation!.IdShort ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        var secondReference = relation.Value?.Second;
        Assert.NotNull(secondReference);
        Assert.Contains(secondReference!.Keys, key => string.Equals(key.Value, "Skill_0001", StringComparison.OrdinalIgnoreCase));
    }

    private static CapabilityContainer LoadP102CapabilityContainer()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));
        var submodelPath = Path.Combine(repoRoot, "environment", "playground-v3", "aas", "P102.json");

        using var document = JsonDocument.Parse(File.ReadAllText(submodelPath));
        var capabilitySubmodel = document.RootElement
            .GetProperty("submodels")
            .EnumerateArray()
            .First(element => HasSemanticId(element, "https://admin-shell.io/idta/CapabilityDescription/1/0/Submodel"));

        var capabilitySet = capabilitySubmodel
            .GetProperty("submodelElements")
            .EnumerateArray()
            .First(element => string.Equals(element.GetProperty("idShort").GetString(), "CapabilitySet", StringComparison.OrdinalIgnoreCase));

        var containerElement = capabilitySet
            .GetProperty("value")
            .EnumerateArray()
            .First(element => string.Equals(element.GetProperty("idShort").GetString(), "AssembleContainer", StringComparison.OrdinalIgnoreCase));

        var submodelElement = BasyxJsonLoader.DeserializeElement(containerElement.GetRawText()) as SubmodelElementCollection;
        if (submodelElement == null)
        {
            throw new InvalidOperationException("Failed to deserialize P102 CapabilityContainer.");
        }

        return new CapabilityContainer(submodelElement);
    }

    private static bool HasSemanticId(JsonElement element, string expectedValue)
    {
        if (!element.TryGetProperty("semanticId", out var semanticId))
        {
            return false;
        }

        if (!semanticId.TryGetProperty("keys", out var keys) || keys.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var key in keys.EnumerateArray())
        {
            if (key.TryGetProperty("value", out var valueNode) && string.Equals(valueNode.GetString(), expectedValue, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
