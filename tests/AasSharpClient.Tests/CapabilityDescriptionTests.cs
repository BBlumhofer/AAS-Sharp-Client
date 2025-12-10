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
    public async Task OfferedCapabilityTemplateMatchesJson()
    {
        var template = BuildOfferedCapabilityTemplate();
        var submodel = new CapabilityDescriptionSubmodel(template.Identifier);
        submodel.Apply(template);

        var actual = await submodel.ToJsonAsync();
        var expected = await File.ReadAllTextAsync("TestData/Test_SM_Capabilities.json");

        TestHelpers.AssertJsonEqual(expected, actual);
    }

    [Fact]
    public async Task ConstraintCapabilityTemplateMatchesJson()
    {
        var template = BuildConstraintCapabilityTemplate();
        var submodel = new CapabilityDescriptionSubmodel(template.Identifier);
        submodel.Apply(template);

        var actual = await submodel.ToJsonAsync();
        var expected = await File.ReadAllTextAsync("TestData/Test_SM_Constraint_Cap.json");

        TestHelpers.AssertJsonEqual(expected, actual);
    }

    internal static CapabilityDescriptionTemplate BuildOfferedCapabilityTemplate()
    {
        var submodelId = "https://smartfactory.de/submodels/0fb7f0f5-2eb4-43d1-b7a0-8415d34a41e9";
        var propertyContainerSemantic = ReferenceFactory.External(
            (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/PropertyContainer/1/0"));

        static MultiLanguagePropertyDefinition CreateBlankComment() => new(
            "Comment",
            TestHelpers.Lang(("en", "blank")),
            Qualifiers: CapabilityDescriptionQualifiers.Multiplicity("ZeroToOne"));

        var propertyContainers = new List<CapabilityPropertyContainerDefinition>
        {
            new RangePropertyContainerDefinition(
                "PropertyContainer01",
                "HeightOfTheProduct",
                "0",
                "1000",
                SemanticId: propertyContainerSemantic,
                Comment: CreateBlankComment(),
                Qualifiers: CapabilityDescriptionQualifiers.Multiplicity("OneToMany")),
            new RangePropertyContainerDefinition(
                "PropertyContainer02",
                "WidthOfTheProduct",
                "0",
                "1000",
                SemanticId: propertyContainerSemantic,
                Comment: CreateBlankComment(),
                Qualifiers: CapabilityDescriptionQualifiers.Multiplicity("OneToMany")),
            new RangePropertyContainerDefinition(
                "PropertyContainer03",
                "DepthOfProduct",
                "0",
                "1000",
                SemanticId: propertyContainerSemantic,
                Comment: CreateBlankComment(),
                Qualifiers: CapabilityDescriptionQualifiers.Multiplicity("OneToMany")),
            new RangePropertyContainerDefinition(
                "PropertyContainer04",
                "WeightOfProduct",
                "0",
                "500",
                SemanticId: propertyContainerSemantic,
                Comment: CreateBlankComment(),
                Qualifiers: CapabilityDescriptionQualifiers.Multiplicity("OneToMany")),
            new PropertyValueContainerDefinition(
                "PropertyContainer05",
                "JoiningTechnique",
                "Positive locking connection - detachable",
                SemanticId: propertyContainerSemantic,
                Comment: CreateBlankComment(),
                Qualifiers: CapabilityDescriptionQualifiers.Multiplicity("OneToMany")),
            new PropertyValueContainerDefinition(
                "PropertyContainer06",
                "ManufacturableShapeOfProduct",
                "Cube",
                SemanticId: propertyContainerSemantic,
                Comment: CreateBlankComment(),
                Qualifiers: CapabilityDescriptionQualifiers.Multiplicity("OneToMany")),
            new PropertyListContainerDefinition(
                "PropertyContainer07",
                "ManufacturableMaterialOfProduct",
                new List<PropertyValueDefinition>
                {
                    new(null, "Metal"),
                    new(null, "NonMetal"),
                    new(null, "Composites")
                },
                SemanticId: propertyContainerSemantic,
                Comment: CreateBlankComment(),
                Qualifiers: CapabilityDescriptionQualifiers.Multiplicity("OneToMany"))
        };

        var capabilityReference = ReferenceFactory.External(
            (KeyType.GlobalReference, "https://wiki.eclipse.org/BaSyx_/_Documentation_/_Submodels_/_Capability#Capability"));

        var relations = new CapabilityRelationsDefinition(
            "CapabilityRelations",
            new List<RelationshipElementDefinition>
            {
                new(
                    "CapabilityRealizedBy",
                    ReferenceFactory.Model(
                        (KeyType.Submodel, submodelId),
                        (KeyType.SubmodelElementCollection, "CapabilitySet"),
                        (KeyType.SubmodelElementCollection, "FullyAutomatedAssemblyContainer")),
                    ReferenceFactory.Model(
                        (KeyType.Submodel, "https://smartfactory.de/submodels/1bf3f6ae-6b95-4f2a-8bd2-c7a80eee4608"),
                        (KeyType.SubmodelElementCollection, "SkillSet"),
                        (KeyType.SubmodelElementCollection, "Skill_0001")),
                    Category: "PARAMETER",
                    Description: TestHelpers.Lang(
                        ("en-US", "Relationship between two elements"),
                        ("de", "Beziehung zwischen zwei Elementen")),
                    Qualifiers: CapabilityDescriptionQualifiers.Multiplicity("OneToMany"))
            },
            SemanticId: ReferenceFactory.External(
                (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/CapabilityRelations/1/0")));

        var propertySet = new CapabilityPropertySetDefinition(
            "PropertySet",
            propertyContainers,
            SemanticId: ReferenceFactory.External(
                (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/PropertySet/1/0")),
            Qualifiers: CapabilityDescriptionQualifiers.Multiplicity("ZeroToOne"));

        var container = new CapabilityContainerDefinition(
            "FullyAutomatedAssemblyContainer",
            new CapabilityElementDefinition(
                "FullyAutomatedAssembly",
                capabilityReference,
                CapabilityDescriptionQualifiers.Multiplicity("One")),
            SemanticId: ReferenceFactory.External(
                (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/CapabilityContainer/1/0")),
            Description: TestHelpers.Lang(
                ("en", "Container for one specific capability and its relations and attributes.")),
            Qualifiers: CapabilityDescriptionQualifiers.Multiplicity("OneToMany"),
            Comment: new MultiLanguagePropertyDefinition(
                "Comment",
                TestHelpers.Lang(
                    ("en", "Capability to fully-automate an assembly of different parts.")),
                Description: TestHelpers.Lang(
                    ("en", "Human readable description of the capability.")),
                Qualifiers: CapabilityDescriptionQualifiers.Multiplicity("ZeroToOne")),
            Relations: relations,
            PropertySet: propertySet);

        var capabilitySet = new CapabilitySetDefinition(
            "CapabilitySet",
            new List<CapabilityContainerDefinition> { container },
            ReferenceFactory.External(
                (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/CapabilitySet/1/0")));

        return new CapabilityDescriptionTemplate(
            submodelId,
            capabilitySet,
            CapabilityDescriptionSubmodel.DefaultIdShort,
            ReferenceFactory.External(
                (KeyType.GlobalReference, "https://admin-shell.io/idta/CapabilityDescription/1/0/Submodel")));
    }

    internal static CapabilityDescriptionTemplate BuildConstraintCapabilityTemplate()
    {
        const string submodelId = "https://smartfactory.de/submodels/5107ce40-b886-4b80-b986-a180e1424500";

        var capabilitySetSemantic = ReferenceFactory.External(
            (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/CapabilityDescription/CapabilitySet#1/0"));
        var containerSemantic = ReferenceFactory.External(
            (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/CapabilityDescription/CapabilitySet/CapabilityContainer#1/0"));
        var capabilitySemantic = ReferenceFactory.External(
            (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/CapabilityDescription/Capability#1/0"));
        var relationsSemantic = ReferenceFactory.External(
            (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/CapabilityDescription/CapabilitySet/CapabilityContainer/CapabilityRelations#1/0"));
        var constraintSetSemantic = ReferenceFactory.External(
            (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/CapabilityDescription/CapabilitySet/CapabilityContainer/CapabilityRelations#1/0/ConstraintSet#1/0"));
        var propertyConstraintSemantic = ReferenceFactory.External(
            (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/CapabilityDescription/CapabilitySet/CapabilityContainer/CapabilityRelations#1/0/ConstraintSet#1/0/PropertyConstraintContainer#1/0"));
        var customConstraintSemantic = ReferenceFactory.External(
            (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/CapabilityDescription/CapabilitySet/CapabilityContainer/CapabilityRelations#1/0/ConstraintSet#1/0/PropertyConstraintContainer#1/0/CustomConstraint#1/0/ConstraintValue#1/0"));
        var propertyRelationsSemantic = ReferenceFactory.External(
            (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/CapabilityDescription/CapabilitySet/CapabilityContainer/CapabilityRelations#1/0/ConstraintSet#1/0/PropertyConstraintContainer#1/0"));
        var propertySetSemantic = ReferenceFactory.External(
            (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/CapabilityDescription/CapabilitySet/CapabilityContainer/PropertySet#1/0"));
        var propertyContainerSemantic = ReferenceFactory.External(
            (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/CapabilityDescription/CapabilitySet/CapabilityContainer/PropertySet/PropertyContainer/InitialProductID#1/0"));

        var constraintContainer = new PropertyConstraintContainerDefinition(
            "PropertyConstraintContainer000",
            new PropertyValueDefinition("PropertyConditionalType", "Pre"),
            new PropertyValueDefinition("ConstraintType", "CustomConstraint"),
            new CustomConstraintDefinition(
                "CustomConstraint",
                new List<PropertyValueDefinition>
                {
                    new("ConstraintName", "StorageConstraint", SemanticId: ReferenceFactory.External(
                        (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/CapabilityDescription/CapabilitySet/CapabilityContainer/CapabilityRelations#1/0/ConstraintSet#1/0/PropertyConstraintContainer#1/0/CustomConstraint#1/0/ConstraintValue#1/0/SotrageConstraint#1/0"))),
                    new("ProductID", "https://smartfactory.de/aas/sample_cab_a_blue", SemanticId: ReferenceFactory.External(
                        (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/CapabilityDescription/CapabilitySet/CapabilityContainer/CapabilityRelations#1/0/ConstraintSet#1/0/PropertyConstraintContainer#1/0/CustomConstraint#1/0/ConstraintValue#1/0/SotrageConstraint#1/0/ProductID#1/0"))),
                    new("ProductType", "Cab_A_Blue", SemanticId: ReferenceFactory.External(
                        (KeyType.GlobalReference, "https://smartfactory.de/aas/submodel/CapabilityDescription/CapabilitySet/CapabilityContainer/CapabilityRelations#1/0/ConstraintSet#1/0/PropertyConstraintContainer#1/0/CustomConstraint#1/0/ConstraintValue#1/0/SotrageConstraint#1/0/ProductType#1/0")))
                },
                customConstraintSemantic),
            new List<RelationshipElementDefinition>
            {
                new(
                    "ConstraintHasProperty_001",
                    ReferenceFactory.Model(
                        (KeyType.Submodel, "TestCapabilityDescription"),
                        (KeyType.SubmodelElementCollection, "CapabilitySet"),
                        (KeyType.SubmodelElementCollection, "AssembleContainer"),
                        (KeyType.SubmodelElementCollection, "CapabilityRelations"),
                        (KeyType.SubmodelElementCollection, "ConstraintSet"),
                        (KeyType.SubmodelElementCollection, "PropertyConstraintContainer000"),
                        (KeyType.SubmodelElementCollection, "CustomConstraint")),
                    ReferenceFactory.Model(
                        (KeyType.Submodel, "TestCapabilityDescription"),
                        (KeyType.SubmodelElementCollection, "CapabilitySet"),
                        (KeyType.SubmodelElementCollection, "AssembleContainer"),
                        (KeyType.SubmodelElementCollection, "PropertySet"),
                        (KeyType.SubmodelElementCollection, "PropertyContainer001"),
                        (KeyType.SubmodelElementList, "InitialProductID")),
                    SemanticId: propertyRelationsSemantic)
            },
            propertyConstraintSemantic,
            "ConstraintPropertyRelations",
            propertyRelationsSemantic);

        var relations = new CapabilityRelationsDefinition(
            "CapabilityRelations",
            new List<RelationshipElementDefinition>
            {
                new(
                    "CapabilityRealizedBy_001",
                    ReferenceFactory.Model(
                        (KeyType.Submodel, "TestCapabilityDescription"),
                        (KeyType.SubmodelElementCollection, "CapabilitySet"),
                        (KeyType.SubmodelElementCollection, "AssembleContainer"),
                        (KeyType.Capability, "Assemble")),
                    ReferenceFactory.Model(
                        (KeyType.Submodel, "https://smartfactory.de/submodells/03zurpeqwr"),
                        (KeyType.SubmodelElementCollection, "SkillSet"),
                        (KeyType.SubmodelElementCollection, "Skill_001")))
            },
            new CapabilityConstraintSetDefinition(
                "ConstraintSet",
                new List<PropertyConstraintContainerDefinition> { constraintContainer },
                constraintSetSemantic),
            new List<SimpleSubmodelElementCollectionDefinition>
            {
                new("GeneralizedBySet"),
                new("ComposedOfSet")
            },
            relationsSemantic);

        var propertySet = new CapabilityPropertySetDefinition(
            "PropertySet",
            new List<CapabilityPropertyContainerDefinition>
            {
                new PropertyListContainerDefinition(
                    "PropertyContainer001",
                    "InitialProductID",
                    new List<PropertyValueDefinition>
                    {
                        new(null, "https://smartfactory.de/aas/sample_cab_a_blue"),
                        new(null, "https://smartfactory.de/aas/sample_cab_chassis")
                    },
                    SemanticId: propertyContainerSemantic,
                    ListSemanticId: null)
            },
            propertySetSemantic);

        var container = new CapabilityContainerDefinition(
            "AssembleContainer",
            new CapabilityElementDefinition("Assemble", capabilitySemantic),
            containerSemantic,
            Relations: relations,
            PropertySet: propertySet);

        var capabilitySet = new CapabilitySetDefinition(
            "CapabilitySet",
            new List<CapabilityContainerDefinition> { container },
            capabilitySetSemantic);

        return new CapabilityDescriptionTemplate(
            submodelId,
            capabilitySet,
            "RequiredCapabilityDescription",
            ReferenceFactory.External(
                (KeyType.GlobalReference, "https://smartfactory.de/semantics/submodel/CapabilityDescription#1/0")));
    }
}
