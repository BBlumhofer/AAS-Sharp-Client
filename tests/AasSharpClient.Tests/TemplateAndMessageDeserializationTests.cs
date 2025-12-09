using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BaSyx.Models.AdminShell;
using Xunit;
using Xunit.Abstractions;
using AasSharpClient.Models;
using AasSharpClient.Models.Messages;

namespace AasSharpClient.Tests
{
    public class TemplateAndMessageDeserializationTests
    {
        private readonly ITestOutputHelper _output;

        public TemplateAndMessageDeserializationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Deserialize_Assemble_Template_WithBaSyx() => AssertCanDeserializeSubmodel("templates/Assemble.json");

        [Fact]
        public void Deserialize_SMC_Scheduling_Template_WithBaSyx() => AssertCanDeserializeSubmodel("templates/Test_SMC_scheduling.json");

        [Fact]
        public void Deserialize_BoM_Holon_Template_WithBaSyx() => AssertCanDeserializeSubmodel("templates/Test_SM_BoM_Holon.json");

        [Fact]
        public void Deserialize_BoM_Truck_Template_WithBaSyx() => AssertCanDeserializeSubmodel("templates/Test_SM_BoM_Truck.json");

        [Fact]
        public void Deserialize_CapabilityDescription_Template_WithBaSyx() => AssertCanDeserializeSubmodel("templates/Test_SM_CapabilityDescription.json");

        [Fact]
        public void Deserialize_MachineSchedule_Template_WithBaSyx() => AssertCanDeserializeSubmodel("templates/Test_SM_MachineSchedule.json");

        [Fact]
        public void Deserialize_Nameplate_Template_WithBaSyx() => AssertCanDeserializeSubmodel("templates/Test_SM_Nameplate.json");

        [Fact]
        public void Deserialize_ProductIdentification_Template_WithBaSyx() => AssertCanDeserializeSubmodel("templates/Test_SM_ProductIdentification.json");

        [Fact]
        public void Deserialize_Skills_Template_WithBaSyx() => AssertCanDeserializeSubmodel("templates/Test_SM_Skills.json");

        [Fact]
        public void Deserialize_Testing_Process_Chain_Template_WithBaSyx() => AssertCanDeserializeSubmodel("templates/Testing_Process_Chain.json");

        [Fact]
        public void Deserialize_Sample_Skills_Template_WithBaSyx() => AssertCanDeserializeSubmodel("examples/SampleClient/skills_actual.json");

        [Fact]
        public void Deserialize_TestData_BoM_Truck_Template_WithBaSyx() => AssertCanDeserializeSubmodel("tests/AasSharpClient.Tests/TestData/Test_SM_BoM_Truck.json");

        [Fact]
        public void Deserialize_TestData_Capabilities_Template_WithBaSyx() => AssertCanDeserializeSubmodel("tests/AasSharpClient.Tests/TestData/Test_SM_Capabilities.json");

        [Fact]
        public void Deserialize_TestData_MachineSchedule_Template_WithBaSyx() => AssertCanDeserializeSubmodel("tests/AasSharpClient.Tests/TestData/Test_SM_MachineSchedule.json");

        [Fact]
        public void Deserialize_TestData_Nameplate_Template_WithBaSyx() => AssertCanDeserializeSubmodel("tests/AasSharpClient.Tests/TestData/Test_SM_Nameplate.json");

        [Fact]
        public void Deserialize_TestData_ProductIdentification_Template_WithBaSyx() => AssertCanDeserializeSubmodel("tests/AasSharpClient.Tests/TestData/Test_SM_ProductIdentification.json");

        [Fact]
        public void Deserialize_TestData_Skills_Template_WithBaSyx() => AssertCanDeserializeSubmodel("tests/AasSharpClient.Tests/TestData/Test_SM_Skills.json");

        [Fact]
        public void Deserialize_LogMessage_Roundtrip_WithBaSyx()
        {
            var message = new LogMessage(LogMessage.LogLevel.Info, "Example log", "ExecutionAgent", "Running");
            AssertRoundtrip(new[] { message }, "LogMessage");
        }

        [Fact]
        public void Deserialize_InventoryMessage_Roundtrip_WithBaSyx()
        {
            var message = new InventoryMessage(new List<StorageUnit>());
            AssertRoundtrip(new[] { message }, "InventoryMessage");
        }

        [Fact]
        public void Deserialize_NeighborMessage_Roundtrip_WithBaSyx()
        {
            var message = new NeighborMessage(new[] { "ModuleA", "ModuleB" });
            AssertRoundtrip(new[] { message }, "NeighborMessage");
        }

        [Fact]
        public void Deserialize_StateMessage_Roundtrip_WithBaSyx()
        {
            var message = new StateMessage(isLocked: false, isReady: true, moduleState: "Idle", startupSkillRunning: false);
            AssertRoundtrip(new[] { message }, "StateMessage");
        }

        private void AssertCanDeserializeSubmodel(string relativePath)
        {
            var json = File.ReadAllText(ResolveRepoPath(relativePath));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("idShort", out _), $"idShort missing in {relativePath}");

            var elements = ExtractElementsArray(root, relativePath);
            var failures = new List<string>();
            int converted = 0;

            foreach (var element in elements)
            {
                var descriptor = DescribeElement(element);
                try
                {
                    var sme = BasyxJsonLoader.DeserializeElement(element);
                    if (sme != null)
                    {
                        converted++;
                    }
                    else
                    {
                        failures.Add($"{relativePath}: {descriptor} -> deserializer returned null");
                    }
                }
                catch (Exception ex)
                {
                    failures.Add($"{relativePath}: {descriptor} -> {ex}");
                }
            }

            _output.WriteLine($"{relativePath}: converted {converted} of {elements.Count} elements");
            foreach (var failure in failures)
            {
                _output.WriteLine(failure);
            }

            Assert.True(elements.Count > 0, $"No submodel elements found in {relativePath}");
            Assert.True(converted > 0, $"No submodel elements could be converted in {relativePath}. First failure: {failures.FirstOrDefault() ?? "none"}");
            Assert.True(failures.Count == 0, $"Failures while converting {relativePath}: {string.Join(" | ", failures.Take(3))}");
        }

        private void AssertRoundtrip(ISubmodelElement[] messages, string label)
        {
            var json = SubmodelSerialization.SerializeElements(messages);
            using var doc = JsonDocument.Parse(json);
            Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);

            var failures = new List<string>();
            var roundTripElements = new List<ISubmodelElement>();
            int index = 0;
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var descriptor = $"{label}[{index}]";
                try
                {
                    var sme = BasyxJsonLoader.DeserializeElement(element);
                    if (sme != null)
                    {
                        roundTripElements.Add(sme);
                    }
                    else
                    {
                        failures.Add($"{descriptor} -> deserializer returned null");
                    }
                }
                catch (Exception ex)
                {
                    failures.Add($"{descriptor} -> {ex.GetType().Name}: {ex.Message}");
                }

                index++;
            }

            foreach (var failure in failures)
            {
                _output.WriteLine(failure);
            }

            Assert.True(failures.Count == 0, $"Roundtrip failed for {label}: {string.Join(" | ", failures)}");
            Assert.Equal(messages.Length, roundTripElements.Count);
            foreach (var original in messages)
            {
                Assert.Contains(roundTripElements, e => string.Equals(e.IdShort, original.IdShort, StringComparison.OrdinalIgnoreCase));
            }
        }

        private static List<JsonElement> ExtractElementsArray(JsonElement root, string relativePath)
        {
            if (root.TryGetProperty("submodelElements", out var elementsNode) && elementsNode.ValueKind == JsonValueKind.Array)
            {
                return elementsNode.EnumerateArray().ToList();
            }

            if (root.TryGetProperty("value", out var valueNode) && valueNode.ValueKind == JsonValueKind.Array)
            {
                return valueNode.EnumerateArray().ToList();
            }

            throw new InvalidOperationException($"No submodelElements/value array in {relativePath}");
        }

        private static string DescribeElement(JsonElement element)
        {
            var idShort = element.TryGetProperty("idShort", out var idShortNode) ? idShortNode.GetString() ?? string.Empty : string.Empty;
            var modelType = element.TryGetProperty("modelType", out var modelNode) ? modelNode.GetString() ?? string.Empty : string.Empty;
            return string.IsNullOrWhiteSpace(idShort) ? modelType : $"{idShort} ({modelType})";
        }

        private static string ResolveRepoPath(string relative)
        {
            var baseDir = AppContext.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
            return Path.Combine(projectRoot, relative);
        }
    }
}
