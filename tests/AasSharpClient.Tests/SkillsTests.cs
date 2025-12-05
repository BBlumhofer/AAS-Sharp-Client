using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;
using Xunit;

namespace AasSharpClient.Tests;

public class SkillsTests
{
    [Fact]
    public async Task SkillsTemplateMatchesJson()
    {
        var data = BuildSkillsData();
        var submodel = SkillsSubmodel.CreateWithIdentifier(data.SubmodelIdentifier);
        submodel.Apply(data);

        var actual = await submodel.ToJsonAsync();
        var expected = await File.ReadAllTextAsync("TestData/Test_SM_Skills.json");

        TestHelpers.AssertJsonEqual(expected, actual);
    }

    private static SkillsData BuildSkillsData()
    {
        var skills = new List<SkillDefinition>
        {
            new("Skill_0001", "Store", "opc.tcp://172.17.57.3:4845/ns=6;s=Module.SkillSet.Store", new List<SkillParameterDefinition>
            {
                new("ID", "xs:string", string.Empty),
                new("Port", "xs:int", "0")
            },
            StandardSkillTriggers()),
            new("Skill_0002", "Retrieve", "opc.tcp:/172.17.57.3:4845/ns=6;s=Module.SkillSet.Retrieve", new List<SkillParameterDefinition>
            {
                new("Port", "xs:int", "0"),
                new("ID", "xs:string", string.Empty)
            },
            StandardSkillTriggers()),
            new("Skill_0003", "Arrange", "opc.tcp://172.17.57.3:4845/ns=6;s=Module.SkillSet.Arrange", new List<SkillParameterDefinition>
            {
                new("BlockPortAfterExecution", "xs:boolean", "true"),
                new("Port", "xs:int", "0"),
                new("ID", "xs:string", string.Empty)
            },
            StandardSkillTriggers()),
            new("Skill_0004", "ReleaseBlockedPort", "opc.tcp://172.17.57.3:4845/ns=6;s=Module.SkillSet.ReleaseBlockedPort", new List<SkillParameterDefinition>
            {
                new("Port", "xs:int", "0"),
                new("ID", "xs:string", string.Empty)
            },
            StandardSkillTriggers())
        };

        var endpointProperties = new List<EndpointMetadataPropertyDefinition>
        {
            new("base", "https://www.w3.org/2019/wot/td#base", "One", "xs:anyURI", "http://172.17.57.3:4845"),
            new("scada", "https://www.w3.org/2019/wot/td#base", "One", "xs:anyURI", "http://172.17.57.3:8000"),
            new("contentType", "https://www.w3.org/2019/wot/hypermedia#forContentType", "One", "xs:string", "application/json")
        };

        var securityListReference = ReferenceFactory.Model(
            (KeyType.Submodel, "https://smartfactory.de/sm/e70c0aa2-2a58-43a9-aa3d-cd81bcbedd3b"),
            (KeyType.SubmodelElementCollection, "Skills"),
            (KeyType.SubmodelElementCollection, "EndpointMetadata"),
            (KeyType.SubmodelElementCollection, "securityDefinitions"),
            (KeyType.SubmodelElementCollection, "basic_sc"));

        var securitySchemes = new List<SecuritySchemeDefinition>
        {
            new(
                "nosec_sc",
                "https://www.w3.org/2019/wot/security#NoSecurityScheme",
                "ZeroToOne",
                new List<SecuritySchemeProperty>
                {
                    new("scheme", "https://www.w3.org/2019/wot/security#SecurityScheme", "One", "xs:string", "nosec")
                }),
            new(
                "auto_sc",
                "https://www.w3.org/2019/wot/security#AutoSecurityScheme",
                "ZeroToOne",
                new List<SecuritySchemeProperty>
                {
                    new("scheme", "https://www.w3.org/2019/wot/security#SecurityScheme", "One", "xs:string", "auto"),
                    new("proxy", "https://www.w3.org/2019/wot/security#proxy", "ZeroToOne", "xs:string", null)
                }),
            new(
                "basic_sc",
                "https://www.w3.org/2019/wot/security#BasicSecurityScheme",
                "ZeroToOne",
                new List<SecuritySchemeProperty>
                {
                    new("scheme", "https://www.w3.org/2019/wot/security#SecurityScheme", "One", "xs:string", "basic"),
                    new("name", "https://www.w3.org/2019/wot/security#name", "ZeroToOne", "xs:string", null),
                    new("in", "https://www.w3.org/2019/wot/security#in", "ZeroToOne", "xs:string", null),
                    new("proxy", "https://www.w3.org/2019/wot/security#proxy", "ZeroToOne", "xs:string", null)
                }),
            new(
                "combo_sc",
                "https://www.w3.org/2019/wot/security#ComboSecurityScheme",
                "ZeroToOne",
                new List<SecuritySchemeProperty>
                {
                    new("scheme", "https://www.w3.org/2019/wot/security#SecurityScheme", "One", "xs:string", "combo"),
                    new("proxy", "https://www.w3.org/2019/wot/security#proxy", "ZeroToOne", "xs:string", null)
                },
                new List<SecuritySchemeListDefinition>
                {
                    new("oneOf", "https://www.w3.org/2019/wot/security#oneOf", "One", ModelType.SubmodelElement, null, null),
                    new("allOf", "https://www.w3.org/2019/wot/security#allOf", "One", ModelType.SubmodelElement, null, null)
                }),
            new(
                "apikey_sc",
                "https://www.w3.org/2019/wot/security#APIKeySecurityScheme",
                "ZeroToOne",
                new List<SecuritySchemeProperty>
                {
                    new("scheme", "https://www.w3.org/2019/wot/security#SecurityScheme", "One", "xs:string", "apikey"),
                    new("name", "https://www.w3.org/2019/wot/security#name", "ZeroToOne", "xs:string", null),
                    new("in", "https://www.w3.org/2019/wot/security#in", "ZeroToOne", "xs:string", null),
                    new("proxy", "https://www.w3.org/2019/wot/security#proxy", "ZeroToOne", "xs:string", null)
                }),
            new(
                "psk_sc",
                "https://www.w3.org/2019/wot/security#PSKSecurityScheme",
                "ZeroToOne",
                new List<SecuritySchemeProperty>
                {
                    new("scheme", "https://www.w3.org/2019/wot/security#SecurityScheme", "One", "xs:string", "psk"),
                    new("identity", "https://www.w3.org/2019/wot/security#identity", "ZeroToOne", "xs:string", null),
                    new("proxy", "https://www.w3.org/2019/wot/security#proxy", "ZeroToOne", "xs:string", null)
                }),
            new(
                "digest_sc",
                "https://www.w3.org/2019/wot/security#DigestSecurityScheme",
                "ZeroToOne",
                new List<SecuritySchemeProperty>
                {
                    new("scheme", "https://www.w3.org/2019/wot/security#SecurityScheme", "One", "xs:string", "digest"),
                    new("name", "https://www.w3.org/2019/wot/security#name", "ZeroToOne", "xs:string", null),
                    new("in", "https://www.w3.org/2019/wot/security#in", "ZeroToOne", "xs:string", null),
                    new("qop", "https://www.w3.org/2019/wot/security#qop", "ZeroToOne", "xs:string", null),
                    new("proxy", "https://www.w3.org/2019/wot/security#proxy", "ZeroToOne", "xs:string", null)
                }),
            new(
                "bearer_sc",
                "https://www.w3.org/2019/wot/security#BearerSecurityScheme",
                "ZeroToOne",
                new List<SecuritySchemeProperty>
                {
                    new("scheme", "https://www.w3.org/2019/wot/security#SecurityScheme", "One", "xs:string", "bearer"),
                    new("name", "https://www.w3.org/2019/wot/security#name", "ZeroToOne", "xs:string", null),
                    new("in", "https://www.w3.org/2019/wot/security#in", "ZeroToOne", "xs:string", null),
                    new("authorization", "https://www.w3.org/2019/wot/security#authorization", "ZeroToOne", "xs:string", null),
                    new("alg", "https://www.w3.org/2019/wot/security#alg", "ZeroToOne", "xs:string", null),
                    new("format", "https://www.w3.org/2019/wot/security#format", "ZeroToOne", "xs:string", null),
                    new("proxy", "https://www.w3.org/2019/wot/security#proxy", "ZeroToOne", "xs:string", null)
                }),
            new(
                "oauth2_sc",
                "https://www.w3.org/2019/wot/security#OAuth2SecurityScheme",
                "ZeroToOne",
                new List<SecuritySchemeProperty>
                {
                    new("scheme", "https://www.w3.org/2019/wot/security#SecurityScheme", "One", "xs:string", "oauth2"),
                    new("token", "https://www.w3.org/2019/wot/security#token", "ZeroToOne", "xs:anyURI", string.Empty),
                    new("refresh", "https://www.w3.org/2019/wot/security#refresh", "ZeroToOne", "xs:anyURI", string.Empty),
                    new("authorization", "https://www.w3.org/2019/wot/security#authorization", "ZeroToOne", "xs:anyURI", string.Empty),
                    new("scopes", "https://www.w3.org/2019/wot/security#scopes", "ZeroToOne", "xs:string", string.Empty),
                    new("flow", "https://www.w3.org/2019/wot/security#flow", "ZeroToOne", "xs:string", "code"),
                    new("proxy", "https://www.w3.org/2019/wot/security#proxy", "ZeroToOne", "xs:anyURI", string.Empty)
                })
        };

        var endpointMetadata = new EndpointMetadataData(endpointProperties, securityListReference, securitySchemes);

        var securityRequirementsReference = ReferenceFactory.Model(
            (KeyType.Submodel, "https://smartfactory.de/submodels/a868c959-baad-406c-a5ef-c087a491def8"),
            (KeyType.SubmodelElementCollection, "Skills"),
            (KeyType.SubmodelElementCollection, "EndpointMetadata"),
            (KeyType.SubmodelElementCollection, "securityDefinitions"),
            (KeyType.SubmodelElementCollection, "basic_sc"));

        var states = new List<StateDefinition>
        {
            new("Halted", string.Empty),
            new("Ready", string.Empty),
            new("Running", string.Empty),
            new("Suspended", string.Empty),
            new("Completed", string.Empty),
            new("Starting", null),
            new("Halting", null)
        };

        var skillMetadataTriggers = new List<SkillMetadataTriggerDefinition>
        {
            new("Start"),
            new("Suspend"),
            new("Halt"),
            new("Reset")
        };

        var skillMetadata = new SkillMetadataData("SkillModel_V4", "opc.tcp", "SmartFactoryKL V4", states, skillMetadataTriggers);

        return new SkillsData(
            "https://smartfactory.de/submodels/b6f0d706-88b7-4ab7-b17b-bdbc7a2ad261",
            skills,
            securityRequirementsReference,
            endpointMetadata,
            skillMetadata);
    }

    private static IReadOnlyList<SkillTriggerDefinition> StandardSkillTriggers()
    {
        return new List<SkillTriggerDefinition>
        {
            new("Start"),
            new("Reset"),
            new("Suspend"),
            new("Halt")
        };
    }
}
