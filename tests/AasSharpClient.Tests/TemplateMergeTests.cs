using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AasSharpClient.Tests
{
    public class TemplateMergeTests
    {
        [Fact]
        public async Task Generate_With_Template_Merges_And_Deserializes()
        {
            var configPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Tools", "ModuleGenerator", "Examples", "P18_config.json"));
            var outFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Tools", "ModuleGenerator", "generated"));

            Directory.CreateDirectory(outFolder);
            var outPath = await ModuleGenerator.ModuleGenerator.GenerateAsync(configPath, outFolder);

            Assert.True(File.Exists(outPath));

            var json = File.ReadAllText(outPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Ensure assetAdministrationShells present
            Assert.True(root.TryGetProperty("assetAdministrationShells", out var aasArr));
            Assert.Equal(JsonValueKind.Array, aasArr.ValueKind);

            // Ensure submodels array present
            Assert.True(root.TryGetProperty("submodels", out var submodels));
            Assert.Equal(JsonValueKind.Array, submodels.ValueKind);

            // Check that at least one submodel has a semanticId key containing 'Skills' or 'CapabilityDescription'
            var foundSkillsOrCap = false;
            foreach (var sm in submodels.EnumerateArray())
            {
                if (sm.TryGetProperty("semanticId", out var sem) && sem.ValueKind == JsonValueKind.Object)
                {
                    var txt = sem.ToString();
                    if (txt.Contains("Skills") || txt.Contains("CapabilityDescription"))
                    {
                        foundSkillsOrCap = true;
                        break;
                    }
                }
            }

            Assert.True(foundSkillsOrCap, "Generated file should include Skills or CapabilityDescription semanticId in submodels");
        }
    }
}
