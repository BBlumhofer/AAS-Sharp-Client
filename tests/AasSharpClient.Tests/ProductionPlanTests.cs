using AasSharpClient.Models;
using AasSharpClient.Tools;
using BaSyx.Models.AdminShell;
using BaSyx.Models.Extensions;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

// alias to avoid conflict with System.Action delegate
using ActionModel = AasSharpClient.Models.Action;

namespace AasSharpClient.Tests
{
    public class ProductionPlanTests
    {
        [Fact]
        public async Task TestSMSKills()
        {
            //Create Production Plan Submodel matching the new ExpectedProductionPlan.json

            // Step0001 - Assembly
            var skill_refence_list_1 = new List<(object Key, string Value)>
            {
                (ModelReferenceEnum.Submodel, "https://example.com/ids/sm/4510_5181_3022_5180"),
                (ModelReferenceEnum.SubmodelElementCollection, "Skills"),
                (ModelReferenceEnum.SubmodelElementCollection, "Skill_0001")
            };
            SkillReference skill_reference_1 = new SkillReference(skill_refence_list_1);

            var input_param_dict_1 = new Dictionary<string, string>
            {
                { "ResourceId1", "self,BillOfMaterial/Truck/Semitrailer/Id" },
                { "ResourceId2", "self,BillOfMaterial/Truck/Semitrailer_Truck/Id" },
                { "ProductID", "self,ProductIdentification/Identifier" }
            };
            InputParameters input_params_1 = new InputParameters(input_param_dict_1);
            var final_result_dict_1 = new Dictionary<string, object>
            {
                { "EndTime", "2023/12/06 12:02:20" },
                { "StartTime", "2023/12/06 12:01:40" }
            };
            FinalResultData final_result_data_1 = new FinalResultData(final_result_dict_1);
            ActionModel action_1 = new ActionModel("Action001", "AssembleProduct", ActionStatusEnum.OPEN, input_params_1, final_result_data_1, skill_reference_1, "TSN-Hochzeitsmodul");

            var input_param_dict_1_2 = new Dictionary<string, string>();
            InputParameters input_params_1_2 = new InputParameters(input_param_dict_1_2);
            var final_result_dict_1_2 = new Dictionary<string, object>
            {
                { "EndTime", "2023/12/06 12:02:20" },
                { "StartTime", "2023/12/06 12:01:40" }
            };
            FinalResultData final_result_data_1_2 = new FinalResultData(final_result_dict_1_2);
            ActionModel action_1_2 = new ActionModel("Action0002", "LoadCarrierFromAssemblyStationToAxis", ActionStatusEnum.OPEN, input_params_1_2, final_result_data_1_2, skill_reference_1, "TSN-Hochzeitsmodul");

            var input_param_dict_1_3 = new Dictionary<string, string>();
            InputParameters input_params_1_3 = new InputParameters(input_param_dict_1_3);
            var final_result_dict_1_3 = new Dictionary<string, object>
            {
                { "EndTime", "2023/12/06 12:02:20" },
                { "StartTime", "2023/12/06 12:01:40" }
            };
            FinalResultData final_result_data_1_3 = new FinalResultData(final_result_dict_1_3);
            ActionModel action_1_3 = new ActionModel("Action0003", "LoadProductToTrack", ActionStatusEnum.OPEN, input_params_1_3, final_result_data_1_3, skill_reference_1, "TSN-Hochzeitsmodul");

            SchedulingContainer scheduling_1 = new SchedulingContainer("2025-12-03 00:05:35", "2025-12-03 00:06:55", "00:00:00", "00:01:20");
            Step step_1 = new Step("Step0001", "Assembly", StepStatusEnum.OPEN, new[] { action_1, action_1_2, action_1_3 }, "P13", scheduling_1, "SmartFactory-KL", "_PHUKET");

            // Step0002 - Unload
            var input_param_dict_2 = new Dictionary<string, string>
            {
                { "ProductType", "self,ProductIdentification/ProductName" },
                { "ProductID", "self,ProductIdentification/Identifier" },
                { "WeighProduct", "False" }
            };
            InputParameters input_params_2 = new InputParameters(input_param_dict_2);
            FinalResultData final_result_data_2 = new FinalResultData();
            ActionModel action_2 = new ActionModel("Action001", "RetrieveFromPortLogisticToAMR", ActionStatusEnum.OPEN, input_params_2, final_result_data_2, skill_reference_1, "StorageModule");

            SchedulingContainer scheduling_2 = new SchedulingContainer("2025-12-03 00:07:05", "2025-12-03 00:07:55", "00:00:00", "00:00:50");
            Step step_2 = new Step("Step0002", "Unload", StepStatusEnum.OPEN, action_2, "P24", scheduling_2, "SmartFactory-KL", "_PHUKET");

            // Step0003 - LabelPrint
            var input_param_dict_3 = new Dictionary<string, string>
            {
                { "ShellURL", "https://shell-view.smartfactory.de" },
                { "AAS_ID", "self,ProductIdentification/Identifier" }
            };
            InputParameters input_params_3 = new InputParameters(input_param_dict_3);
            var final_result_dict_3 = new Dictionary<string, object>
            {
                { "EndTime", "2023/12/06 12:03:33" },
                { "StartTime", "2023/12/06 12:02:54" }
            };
            FinalResultData final_result_data_3 = new FinalResultData(final_result_dict_3);
            ActionModel action_3 = new ActionModel("Action001", "AASLabelPrint", ActionStatusEnum.OPEN, input_params_3, final_result_data_3, skill_reference_1, "LabelPrinter");

            SchedulingContainer scheduling_3 = new SchedulingContainer("2025-12-03 00:08:05", "2025-12-03 00:08:10", "00:00:00", "00:00:05");
            Step step_3 = new Step("Step0003", "LabelPrint", StepStatusEnum.OPEN, action_3, "P12", scheduling_3, "SmartFactory-KL", "_PHUKET");

            QuantityInformation quantityInformation = new QuantityInformation(1);
            ProductionPlan production_plan_sm = new ProductionPlan(false, 1, step_1);
            production_plan_sm.append_step(step_2);
            production_plan_sm.append_step(step_3);
            // Serialize submodel using BaSyx serializer helpers (we'll use System.Text.Json for comparison)
            var output = await production_plan_sm.ToJsonAsync();

            // save actual JSON to repo-local TestOutputs to avoid writing outside workspace
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            var outDir = Path.Combine(projectRoot, "TestOutputs", "ProductionPlan");
            Directory.CreateDirectory(outDir);
            var outPath = Path.Combine(outDir, "ActualProductionPlan.json");
            await File.WriteAllTextAsync(outPath, output);

            var expected = await File.ReadAllTextAsync("TestData/ExpectedProductionPlan.json");

            // Strict recursive comparison: for every element in expected JSON ensure a matching
            // element exists in the actual JSON with the same primitive value and semanticId.
            var expectedDoc = JsonDocument.Parse(expected);
            var actualDoc = JsonDocument.Parse(output);

            JsonElement? FindByIdShort(JsonElement root, string idShort, System.Collections.Generic.List<string> expectedAncestors)
            {
                // Traverse actual JSON and track ancestor idShorts to disambiguate repeated idShorts
                JsonElement? result = null;

                bool Rec(JsonElement node, System.Collections.Generic.List<string> currentAncestors)
                {
                    if (node.ValueKind == JsonValueKind.Object)
                    {
                        node.TryGetProperty("idShort", out var idProp);
                        var nodeId = idProp.ValueKind == JsonValueKind.String ? idProp.GetString() : null;
                        if (nodeId != null && nodeId == idShort)
                        {
                            // compare ancestor chains (exclude the leaf)
                            // expectedAncestors contains the chain leading to this expected element
                            if (expectedAncestors == null || expectedAncestors.Count == 0)
                            {
                                result = node;
                                return true;
                            }
                            // match last N ancestors
                            var exp = expectedAncestors;
                            // currentAncestors corresponds to the path to this node
                            if (currentAncestors.Count >= exp.Count)
                            {
                                // compare suffix of currentAncestors with exp
                                var start = currentAncestors.Count - exp.Count;
                                bool ok = true;
                                for (int i = 0; i < exp.Count; i++)
                                {
                                    if (!string.Equals(currentAncestors[start + i], exp[i], System.StringComparison.Ordinal)) { ok = false; break; }
                                }
                                if (ok) { result = node; return true; }
                            }
                        }

                        // when recursing into children, append this node's idShort (if any)
                        var nextAnc = new System.Collections.Generic.List<string>(currentAncestors);
                        if (nodeId != null) nextAnc.Add(nodeId);
                        foreach (var prop in node.EnumerateObject())
                        {
                            if (Rec(prop.Value, new System.Collections.Generic.List<string>(nextAnc))) return true;
                        }
                    }
                    else if (node.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in node.EnumerateArray())
                        {
                            if (Rec(item, new System.Collections.Generic.List<string>(currentAncestors))) return true;
                        }
                    }
                    return false;
                }

                // start traversal with empty ancestor chain
                Rec(root, new System.Collections.Generic.List<string>());
                return result;
            }

            void CompareSemanticId(JsonElement expectedSid, JsonElement actualSid)
            {
                // expected and actual should have 'type' and 'keys' array
                Assert.True(actualSid.ValueKind == JsonValueKind.Object, "semanticId missing or wrong kind in actual");
                if (expectedSid.TryGetProperty("type", out var et))
                {
                    Assert.True(actualSid.TryGetProperty("type", out var at), "semanticId.type missing in actual");
                    Assert.Equal(et.GetString(), at.GetString());
                }
                if (expectedSid.TryGetProperty("keys", out var ek) && ek.ValueKind == JsonValueKind.Array)
                {
                    Assert.True(actualSid.TryGetProperty("keys", out var ak) && ak.ValueKind == JsonValueKind.Array, "semanticId.keys missing in actual");
                    var ekeys = ek.EnumerateArray().ToArray();
                    var akeys = ak.EnumerateArray().ToArray();
                    Assert.Equal(ekeys.Length, akeys.Length);
                    for (int i = 0; i < ekeys.Length; i++)
                    {
                        var ekey = ekeys[i];
                        var akey = akeys[i];
                        if (ekey.TryGetProperty("type", out var etk))
                        {
                            Assert.True(akey.TryGetProperty("type", out var atk));
                            Assert.Equal(etk.GetString(), atk.GetString());
                        }
                        if (ekey.TryGetProperty("value", out var evk))
                        {
                            Assert.True(akey.TryGetProperty("value", out var avk));
                            Assert.Equal(evk.GetString(), avk.GetString());
                        }
                    }
                }
            }

            void CompareExpectedAgainstActual(JsonElement expectedElem, System.Collections.Generic.List<string> ancestors)
            {
                if (expectedElem.ValueKind != JsonValueKind.Object) return;
                if (!expectedElem.TryGetProperty("idShort", out var idProp) || idProp.ValueKind != JsonValueKind.String) return;
                var id = idProp.GetString();
                Assert.False(string.IsNullOrEmpty(id));

                var actualElemNullable = FindByIdShort(actualDoc.RootElement, id!, ancestors);
                Assert.True(actualElemNullable.HasValue, $"Missing element in actual JSON: {id}");
                var actualElem = actualElemNullable.Value;

                // If expected has semanticId, compare
                if (expectedElem.TryGetProperty("semanticId", out var expectedSid))
                {
                    Assert.True(actualElem.TryGetProperty("semanticId", out var actualSid), $"Missing semanticId for {id} in actual JSON");
                    CompareSemanticId(expectedSid, actualSid);
                }

                // If expected has primitive value, assert equality (trim)
                if (expectedElem.TryGetProperty("value", out var expectedVal))
                {
                    if (expectedVal.ValueKind == JsonValueKind.String || expectedVal.ValueKind == JsonValueKind.Number || expectedVal.ValueKind == JsonValueKind.True || expectedVal.ValueKind == JsonValueKind.False)
                    {
                        Assert.True(actualElem.TryGetProperty("value", out var actualVal), $"Missing value for {id} in actual JSON");
                        var ev = expectedVal.ToString()?.Trim();
                        var av = actualVal.ToString()?.Trim();
                        Assert.Equal(ev, av);
                        return;
                    }
                    else
                    {
                            // value is array/object -> recursively compare children
                            if (expectedVal.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var child in expectedVal.EnumerateArray())
                                {
                                    var newAnc = new System.Collections.Generic.List<string>(ancestors);
                                    newAnc.Add(id!);
                                    CompareExpectedAgainstActual(child, newAnc);
                                }
                            }
                            else if (expectedVal.ValueKind == JsonValueKind.Object)
                            {
                                var newAnc = new System.Collections.Generic.List<string>(ancestors);
                                newAnc.Add(id!);
                                CompareExpectedAgainstActual(expectedVal, newAnc);
                            }
                    }
                }

                // also recurse into other properties to find nested idShorts
                foreach (var prop in expectedElem.EnumerateObject())
                {
                    var newAnc = new System.Collections.Generic.List<string>(ancestors);
                    if (!string.IsNullOrEmpty(id)) newAnc.Add(id!);
                    CompareExpectedAgainstActual(prop.Value, newAnc);
                }
            }

            // Start comparison at top-level submodelElements
            if (expectedDoc.RootElement.TryGetProperty("submodelElements", out var expectedElems) && expectedElems.ValueKind == JsonValueKind.Array)
            {
                foreach (var e in expectedElems.EnumerateArray())
                {
                    CompareExpectedAgainstActual(e, new System.Collections.Generic.List<string>());
                }
            }

            var input = ProductionPlan.Parse(output);
            var steps = input.Steps;
        }
    }
}
