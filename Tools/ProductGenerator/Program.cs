using System.Text.Json.Nodes;
using System.Text.Json;

namespace ProductGeneratorApp;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        var configPath = args.Length > 0 ? args[0] : Path.Combine("Tools", "ProductGenerator", "configs", "Cab_B_Red_config.json");
        var outputDir = args.Length > 1 ? args[1] : Path.Combine(Directory.GetCurrentDirectory(), "generated");

        try
        {
            var result = await ProductGenerator.GenerateAsync(configPath, outputDir);
            Console.WriteLine("Generated: " + result);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: " + ex);
            return 1;
        }
    }
}
