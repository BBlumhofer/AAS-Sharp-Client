using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using AasSharpClient.Models;
using ModuleGenerator;

namespace AasSharpClient.Tests
{
    public class ModuleGeneratorTests
    {
        [Fact]
        public async Task Generate_P18_Config_Generates_Deserializable_Json()
        {
            var configPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Tools", "ModuleGenerator", "Examples", "P18_config.json"));
            var outFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Tools", "ModuleGenerator", "generated"));

            Directory.CreateDirectory(outFolder);
            var outPath = await ModuleGenerator.ModuleGenerator.GenerateAsync(configPath, outFolder);

            Assert.True(File.Exists(outPath));

            var generated = File.ReadAllText(outPath);
            using var doc = JsonDocument.Parse(generated);
            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("submodels", out var subsElement));
            Assert.Equal(System.Text.Json.JsonValueKind.Array, subsElement.ValueKind);

            // try deserializing first submodel element arrays to ensure converters work
            // reuse existing loader options
            var firstSubmodel = subsElement[0];
            Assert.True(firstSubmodel.TryGetProperty("submodelElements", out var elements));
            Assert.Equal(System.Text.Json.JsonValueKind.Array, elements.ValueKind);

            var elementJson = elements[0].GetRawText();
            var sme = JsonSerializer.Deserialize<BaSyx.Models.AdminShell.ISubmodelElement>(elementJson, BasyxJsonLoader.Options);
            Assert.NotNull(sme);
        }
    }
}
