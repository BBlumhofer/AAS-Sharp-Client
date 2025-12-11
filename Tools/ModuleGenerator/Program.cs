using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleGenerator
{
    internal static class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Behavior:
            // - No args: process all *_config.json in Tools/ModuleGenerator/Examples
            // - First arg is a directory: process all *_config.json in that directory
            // - Otherwise: treat first arg as single config file

            string repoRoot = Directory.GetCurrentDirectory();
            string defaultExamples = Path.Combine(repoRoot, "Configs");
            // Default output: $PWD/generated (relative to current working directory)
            string defaultOutput = Path.Combine(Directory.GetCurrentDirectory(), "generated");

            if (args.Length == 0)
            {
                // process whole Examples folder
                if (!Directory.Exists(defaultExamples))
                {
                    Console.Error.WriteLine($"Configs folder not found: {defaultExamples}");
                    return 2;
                }

                Directory.CreateDirectory(defaultOutput);
                var configs = Directory.GetFiles(defaultExamples, "*_config.json");
                if (configs.Length == 0)
                {
                    Console.WriteLine("No config files found in Examples folder.");
                    return 0;
                }

                int failures = 0;
                foreach (var cfg in configs.OrderBy(p => p))
                {
                    Console.WriteLine($"Processing {cfg}");
                    try
                    {
                        var outPath = await ModuleGenerator.GenerateAsync(cfg, defaultOutput);
                        Console.WriteLine($"Generated: {outPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error processing {cfg}: {ex.Message}");
                        failures++;
                    }
                }

                return failures == 0 ? 0 : 3;
            }

            // args provided
            var first = args[0];
            var outputFolder = args.Length > 1 ? args[1] : defaultOutput;

            if (Directory.Exists(first))
            {
                Directory.CreateDirectory(outputFolder);
                var configs = Directory.GetFiles(first, "*_config.json");
                if (configs.Length == 0)
                {
                    Console.WriteLine("No config files found in provided directory.");
                    return 0;
                }

                int failures = 0;
                foreach (var cfg in configs.OrderBy(p => p))
                {
                    Console.WriteLine($"Processing {cfg}");
                    try
                    {
                        var outPath = await ModuleGenerator.GenerateAsync(cfg, outputFolder);
                        Console.WriteLine($"Generated: {outPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error processing {cfg}: {ex.Message}");
                        failures++;
                    }
                }

                return failures == 0 ? 0 : 3;
            }

            // single file path
            var configPath = first;
            if (!File.Exists(configPath))
            {
                Console.Error.WriteLine($"Config not found: {configPath}");
                return 2;
            }

            Directory.CreateDirectory(outputFolder);

            try
            {
                var result = await ModuleGenerator.GenerateAsync(configPath, outputFolder);
                Console.WriteLine($"Generated: {result}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 3;
            }
        }
    }
}
