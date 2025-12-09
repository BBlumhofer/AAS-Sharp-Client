using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;
using Xunit;

namespace AasSharpClient.Tests;

public class CapabilityDescriptionTests
{
    [Fact]
    public async Task CapabilityDescriptionTemplateMatchesJson()
    {
        var data = BuildCapabilityDescriptionData();
        var submodel = CapabilityDescriptionSubmodel.CreateWithIdentifier(data.SubmodelIdentifier);
        submodel.Apply(data);

        var actual = await submodel.ToJsonAsync();
        var expected = await File.ReadAllTextAsync("TestData/Test_SM_Capabilities.json");

        TestHelpers.AssertJsonEqual(expected, actual);
    }

    public static CapabilityDescriptionData BuildCapabilityDescriptionData()
    {
        var propertyDefinitions = new List<PropertyContainerDefinition>
        {
            new(
                "PropertyContainer01",
                "https://admin-shell.io/idta/CapabilityDescription/PropertyContainer/1/0",
                TestHelpers.Lang(("en", "blank")),
                PropertyContainerType.Range,
                RangeIdShort: "HeightOfTheProduct",
                RangeMin: "0",
                RangeMax: "1000"),
            new(
                "PropertyContainer02",
                "https://admin-shell.io/idta/CapabilityDescription/PropertyContainer/1/0",
                TestHelpers.Lang(("en", "blank")),
                PropertyContainerType.Range,
                RangeIdShort: "WidthOfTheProduct",
                RangeMin: "0",
                RangeMax: "1000"),
            new(
                "PropertyContainer03",
                "https://admin-shell.io/idta/CapabilityDescription/PropertyContainer/1/0",
                TestHelpers.Lang(("en", "blank")),
                PropertyContainerType.Range,
                RangeIdShort: "DepthOfProduct",
                RangeMin: "0",
                RangeMax: "1000"),
            new(
                "PropertyContainer04",
                "https://admin-shell.io/idta/CapabilityDescription/PropertyContainer/1/0",
                TestHelpers.Lang(("en", "blank")),
                PropertyContainerType.Range,
                RangeIdShort: "WeightOfProduct",
                RangeMin: "0",
                RangeMax: "500"),
            new(
                "PropertyContainer05",
                "https://admin-shell.io/idta/CapabilityDescription/PropertyContainer/1/0",
                TestHelpers.Lang(("en", "blank")),
                PropertyContainerType.Property,
                PropertyIdShort: "JoiningTechnique",
                PropertyValue: "Positive locking connection - detachable",
                PropertyValueType: "xs:string"),
            new(
                "PropertyContainer06",
                "https://admin-shell.io/idta/CapabilityDescription/PropertyContainer/1/0",
                TestHelpers.Lang(("en", "blank")),
                PropertyContainerType.Property,
                PropertyIdShort: "ManufacturableShapeOfProduct",
                PropertyValue: "Cube",
                PropertyValueType: "xs:string"),
            new(
                "PropertyContainer07",
                "https://admin-shell.io/idta/CapabilityDescription/PropertyContainer/1/0",
                TestHelpers.Lang(("en", "blank")),
                PropertyContainerType.List,
                ListIdShort: "ManufacturableMaterialOfProduct",
                ListValues: new List<string> { "Metal", "NonMetal", "Composites" })
        };

        var firstReference = ReferenceFactory.Model(
            (KeyType.Submodel, "https://smartfactory.de/submodels/0fb7f0f5-2eb4-43d1-b7a0-8415d34a41e9"),
            (KeyType.SubmodelElementCollection, "CapabilitySet"),
            (KeyType.SubmodelElementCollection, "FullyAutomatedAssemblyContainer"));

        var secondReference = ReferenceFactory.Model(
            (KeyType.Submodel, "https://smartfactory.de/submodels/1bf3f6ae-6b95-4f2a-8bd2-c7a80eee4608"),
            (KeyType.SubmodelElementCollection, "SkillSet"),
            (KeyType.SubmodelElementCollection, "Skill_0001"));

        return new CapabilityDescriptionData(
            "https://smartfactory.de/submodels/0fb7f0f5-2eb4-43d1-b7a0-8415d34a41e9",
            TestHelpers.Lang(("en", "Container for one specific capability and its relations and attributes.")),
            TestHelpers.Lang(("en", "Human readable description of the capability.")),
            TestHelpers.Lang(("en", "Capability to fully-automate an assembly of different parts.")),
            firstReference,
            secondReference,
            propertyDefinitions);
    }
}
