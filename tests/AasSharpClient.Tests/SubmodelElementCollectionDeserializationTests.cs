using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using BaSyx.Models.AdminShell;
using BaSyx.Models.Extensions;
using Xunit;
using AasSharpClient.Models;
using AasSharpClient.Tests;
using ActionModel = AasSharpClient.Models.Action;

namespace AasSharpClient.Tests
{
    public class SubmodelElementCollectionDeserializationTests
    {
        [Fact]
        public void Deserialize_SubmodelElementCollection_FromFile_WithBaSyxSerializer()
        {
            var coll = BasyxJsonLoader.LoadCollectionFromFile("ActionCollection.json");

            Assert.NotNull(coll);
            Assert.Equal("Action001", coll!.IdShort);
            Assert.Contains(Elements(coll), e => string.Equals(e.IdShort, "ActionTitle", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Deserialize_ActionJson_IntoActionClass_UsingBasyxSerializer()
        {
            var coll = BasyxJsonLoader.LoadCollectionFromFile("ActionCollection.json");

            Assert.NotNull(coll);

            var action = CreateActionFromCollection(coll!);

            Assert.NotNull(action);
            Assert.Equal("Action001", action.IdShort);
            Assert.Equal("Retrieve", action.ActionTitle.Value.Value?.ToString());
            Assert.Equal(ActionStatusEnum.PLANNED, action.State);
            Assert.Equal("CA-Module", action.MachineName.Value.Value?.ToString());
        }

        private static ActionModel CreateActionFromCollection(SubmodelElementCollection coll)
        {
            var title = GetStringProperty(coll, "ActionTitle", "Unknown");
            var statusValue = GetStringProperty(coll, "Status", "planned");
            var machineName = GetStringProperty(coll, "MachineName", string.Empty);

            var status = Enum.TryParse<ActionStatusEnum>(statusValue, true, out var parsedStatus)
                ? parsedStatus
                : ActionStatusEnum.PLANNED;

            var inputParams = BuildInputParameters(GetCollection(coll, "InputParameters"));
            var finalResultData = BuildFinalResultData(GetCollection(coll, "FinalResultData"));
            var skillReference = new SkillReference(Array.Empty<(object Key, string Value)>());

            return new ActionModel(
                idShort: coll.IdShort ?? "Action001",
                actionTitle: title,
                status: status,
                inputParameters: inputParams,
                finalResultData: finalResultData,
                preconditions: null,
                skillReference: skillReference,
                machineName: machineName
            );
        }

        private static SubmodelElementCollection? GetCollection(SubmodelElementCollection coll, string idShort)
        {
            return Elements(coll)
                .OfType<SubmodelElementCollection>()
                .FirstOrDefault(e => string.Equals(e.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
        }

        private static string GetStringProperty(SubmodelElementCollection coll, string idShort, string fallback)
        {
            var property = Elements(coll)
                .FirstOrDefault(e => string.Equals(e.IdShort, idShort, StringComparison.OrdinalIgnoreCase));

            if (property is Property<string> stringProp)
            {
                return stringProp.Value?.Value?.ToString() ?? fallback;
            }

            if (property is IProperty prop && prop.Value?.Value is not null)
            {
                return prop.Value.Value.ToString() ?? fallback;
            }

            return fallback;
        }

        private static InputParameters BuildInputParameters(SubmodelElementCollection? collection)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var element in Elements(collection).OfType<IProperty>())
            {
                object? raw = element.Value?.Value;
                if (raw is IValue inner)
                {
                    raw = inner.Value;
                }

                if (raw != null)
                {
                    dict[element.IdShort] = raw;
                }
            }

            return InputParameters.FromTypedValues(dict);
        }

        private static FinalResultData BuildFinalResultData(SubmodelElementCollection? collection)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var element in Elements(collection).OfType<IProperty>())
            {
                object? raw = element.Value?.Value;
                if (raw is IValue inner)
                {
                    raw = inner.Value;
                }

                if (raw != null)
                {
                    dict[element.IdShort] = raw;
                }
            }

            return new FinalResultData(dict);
        }

        private static IEnumerable<ISubmodelElement> Elements(SubmodelElementCollection? coll)
        {
            if (coll is null)
            {
                return Array.Empty<ISubmodelElement>();
            }

            if (coll.Value is IEnumerable<ISubmodelElement> seq)
            {
                return seq;
            }

            if (coll is IEnumerable<ISubmodelElement> enumerable)
            {
                return enumerable;
            }

            return Array.Empty<ISubmodelElement>();
        }

        private static string ResolveRepoPath(string relative)
        {
            var baseDir = AppContext.BaseDirectory; // bin/Debug/netX.X
            var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
            return Path.Combine(projectRoot, relative);
        }
    }
}
