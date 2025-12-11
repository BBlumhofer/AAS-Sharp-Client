using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using AasSharpClient.Models;
using AasSharpClient.Models.Messages;
using BaSyx.Models.AdminShell;
using Xunit;

namespace AasSharpClient.Tests
{
    public class MessageExamplesTests
    {
        private static string GetOutputFolder()
        {
            var baseDir = AppContext.BaseDirectory; // bin/Debug/net10.0
            var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..")); // project root
            var outDir = Path.Combine(projectRoot, "TestOutputs", "MessageExamples");
            Directory.CreateDirectory(outDir);
            return outDir;
        }

        [Fact]
        public void LogMessage_CreatesExample_AndWritesJson()
        {
            var log = new AasSharpClient.Models.Messages.LogMessage(LogMessage.LogLevel.Info, "Example log", "ExecutionAgent", "Running");

            var json = SubmodelSerialization.SerializeElements(new[] { log });

            using var doc = JsonDocument.Parse(json);
            Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);

            var logCollection = Assert.Single(doc.RootElement.EnumerateArray());
            Assert.Equal("Log", logCollection.GetProperty("idShort").GetString());

            var elementIdShorts = logCollection
                .GetProperty("value")
                .EnumerateArray()
                .Select(e => e.GetProperty("idShort").GetString())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            var expectedIds = new[] { "LogLevel", "Message", "Timestamp", "AgentRole", "AgentState" };
            foreach (var expectedId in expectedIds)
            {
                Assert.Contains(expectedId, elementIdShorts);
            }

            var outFile = Path.Combine(GetOutputFolder(), "LogMessage.json");
            File.WriteAllText(outFile, json);
            Assert.True(File.Exists(outFile));
        }

        [Fact]
        public void InventoryMessage_CreatesExample_AndWritesJson()
        {
            var storage = new StorageUnit { Name = "StorageA" };
            storage.Slots.Add(new Slot
            {
                Index = 1,
                Content = new SlotContent
                {
                    CarrierID = "C001",
                    CarrierType = "Tray",
                    ProductType = "Widget",
                    ProductID = "W-123",
                    IsSlotEmpty = false
                }
            });

            var inv = new InventoryMessage(new List<StorageUnit> { storage });

            var json = SubmodelSerialization.SerializeElements(new[] { inv });
            var outFile = Path.Combine(GetOutputFolder(), "InventoryMessage.json");
            File.WriteAllText(outFile, json);
            Assert.True(File.Exists(outFile));
        }

        [Fact]
        public void InventoryMessage_RoundTrip_RehydratesStorageUnits()
        {
            var storage = new StorageUnit { Name = "ModuleStorage" };
            storage.Slots.Add(new Slot
            {
                Index = 0,
                Content = new SlotContent
                {
                    CarrierID = "WST_B_9",
                    CarrierType = "WST_B",
                    ProductType = "Semitrailer_Chassis",
                    ProductID = "https://smartfactory.de/shells/example",
                    IsSlotEmpty = false
                }
            });

            storage.Slots.Add(new Slot
            {
                Index = 1,
                Content = new SlotContent
                {
                    CarrierID = string.Empty,
                    CarrierType = "i=0",
                    ProductType = "i=0",
                    ProductID = string.Empty,
                    IsSlotEmpty = true
                }
            });

            var original = new InventoryMessage(new List<StorageUnit> { storage });
            var interactionElements = new List<ISubmodelElement> { original };
            var parsedInventory = new InventoryMessage(interactionElements);
            Assert.Single(parsedInventory.StorageUnits);

            var parsedStorage = parsedInventory.StorageUnits[0];
            Assert.Equal("ModuleStorage", parsedStorage.Name);
            Assert.Equal(2, parsedStorage.Slots.Count);

            var slot0 = parsedStorage.Slots[0];
            Assert.Equal(0, slot0.Index);
            Assert.False(slot0.Content.IsSlotEmpty);
            Assert.Equal("WST_B_9", slot0.Content.CarrierID);
            Assert.Equal("https://smartfactory.de/shells/example", slot0.Content.ProductID);

            var slot1 = parsedStorage.Slots[1];
            Assert.Equal(1, slot1.Index);
            Assert.True(slot1.Content.IsSlotEmpty);
        }

        [Fact]
        public void NeighborMessage_CreatesExample_AndWritesJson()
        {
            var neighbors = new[] { "ModuleA", "ModuleB" };
            var neigh = new NeighborMessage(neighbors);

            var json = SubmodelSerialization.SerializeElements(new[] { neigh });
            var outFile = Path.Combine(GetOutputFolder(), "NeighborMessage.json");
            File.WriteAllText(outFile, json);
            Assert.True(File.Exists(outFile));
        }

        [Fact]
        public void StateMessage_CreatesExample_AndWritesJson()
        {
            var state = new StateMessage(isLocked: false, isReady: true, moduleState: "Idle", startupSkillRunning: false);

            var json = SubmodelSerialization.SerializeElements(new[] { state });
            var outFile = Path.Combine(GetOutputFolder(), "StateMessage.json");
            File.WriteAllText(outFile, json);
            Assert.True(File.Exists(outFile));
        }
    }
}
