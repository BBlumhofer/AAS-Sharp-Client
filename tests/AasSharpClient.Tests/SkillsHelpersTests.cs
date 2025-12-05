using System.Collections.Generic;
using System.Linq;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;
using Xunit;

namespace AasSharpClient.Tests;

public class SkillsHelpersTests
{
    [Fact]
    public void GetSkillNames_ReturnsNames_FromAppliedData()
    {
        var skills = new List<SkillDefinition>
        {
            new SkillDefinition("Skill_01", "PickAndPlace", "http://example/1", new List<SkillParameterDefinition>(), new List<SkillTriggerDefinition>())
        };

        var endpointMetadata = new EndpointMetadataData(new List<EndpointMetadataPropertyDefinition>(), ReferenceFactory.External((KeyType.GlobalReference, "EMPTY")), new List<SecuritySchemeDefinition>());
        var skillMetadata = new SkillMetadataData(string.Empty, string.Empty, string.Empty, new List<StateDefinition>(), new List<SkillMetadataTriggerDefinition>());

        var data = new SkillsData("urn:sm:skills", skills, ReferenceFactory.External((KeyType.GlobalReference, "EMPTY")), endpointMetadata, skillMetadata);
        var submodel = SkillsSubmodel.CreateWithIdentifier(data.SubmodelIdentifier);
        submodel.Apply(data);

        var names = submodel.GetSkillNames().ToList();
        Assert.Contains("PickAndPlace", names);
    }

    [Fact]
    public void FindSkillById_Returns_Container()
    {
        var skills = new List<SkillDefinition>
        {
            new SkillDefinition("Skill_42", "Weld", "http://example/weld", new List<SkillParameterDefinition>(), new List<SkillTriggerDefinition>())
        };

        var endpointMetadata = new EndpointMetadataData(new List<EndpointMetadataPropertyDefinition>(), ReferenceFactory.External((KeyType.GlobalReference, "EMPTY")), new List<SecuritySchemeDefinition>());
        var skillMetadata = new SkillMetadataData(string.Empty, string.Empty, string.Empty, new List<StateDefinition>(), new List<SkillMetadataTriggerDefinition>());

        var data = new SkillsData("urn:sm:skills2", skills, ReferenceFactory.External((KeyType.GlobalReference, "EMPTY")), endpointMetadata, skillMetadata);
        var submodel = SkillsSubmodel.CreateWithIdentifier(data.SubmodelIdentifier);
        submodel.Apply(data);

        var container = submodel.FindSkillById("Skill_42");
        Assert.NotNull(container);
        Assert.Equal("Skill_42", container!.IdShort);
    }
}
