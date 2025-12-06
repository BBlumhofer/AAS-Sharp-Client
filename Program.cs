using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;
using BaSyx.Clients.AdminShell.Http;
using System.Linq;

namespace AasSharpClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            
            logger.LogInformation("=== AAS Sharp Client Demo - Alle Submodelle ===");
            logger.LogInformation("BaSyx Server: http://localhost:8080");

            try
            {
                var aasRepoUri = new Uri("http://localhost:8080");
                var submodelRepoUri = new Uri("http://localhost:8080");

                var aasRepoClient = new AssetAdministrationShellRepositoryHttpClient(aasRepoUri);
                var submodelRepoClient = new SubmodelRepositoryHttpClient(submodelRepoUri);

                // 1. Alle Submodelle erstellen
                logger.LogInformation("\n1. Erstelle alle Submodelle...");
                
                var submodels = new System.Collections.Generic.List<Submodel>();
                
                // MachineSchedule
                string smMachineScheduleId = $"https://smartfactory.de/submodels/machineSchedule/{Guid.NewGuid()}";
                var machineSchedule = new MachineScheduleSubmodel(smMachineScheduleId);
                submodels.Add(machineSchedule);
                logger.LogInformation($"   ✅ MachineSchedule: {smMachineScheduleId}");
                
                // BillOfMaterial
                string smBomId = $"https://smartfactory.de/submodels/billOfMaterial/{Guid.NewGuid()}";
                var billOfMaterial = new BillOfMaterialSubmodel();
                billOfMaterial.Id = new Identifier(smBomId);
                submodels.Add(billOfMaterial);
                logger.LogInformation($"   ✅ BillOfMaterial: {smBomId}");
                
                // CapabilityDescription
                string smCapabilityId = $"https://smartfactory.de/submodels/capability/{Guid.NewGuid()}";
                var capability = new CapabilityDescriptionSubmodel(smCapabilityId);
                submodels.Add(capability);
                logger.LogInformation($"   ✅ CapabilityDescription: {smCapabilityId}");
                
                // Nameplate
                string smNameplateId = $"https://smartfactory.de/submodels/nameplate/{Guid.NewGuid()}";
                var nameplate = new NameplateSubmodel(smNameplateId);
                submodels.Add(nameplate);
                logger.LogInformation($"   ✅ Nameplate: {smNameplateId}");
                
                // ProductIdentification
                string smProductIdId = $"https://smartfactory.de/submodels/productId/{Guid.NewGuid()}";
                var productId = new ProductIdentificationSubmodel(smProductIdId);
                submodels.Add(productId);
                logger.LogInformation($"   ✅ ProductIdentification: {smProductIdId}");
                
                // ProductionPlan
                string smProductionPlanId = $"https://smartfactory.de/submodels/productionPlan/{Guid.NewGuid()}";
                var productionPlan = new ProductionPlan(false, 100);
                productionPlan.Id = new Identifier(smProductionPlanId);
                submodels.Add(productionPlan);
                logger.LogInformation($"   ✅ ProductionPlan: {smProductionPlanId}");
                
                // Skills
                string smSkillsId = $"https://smartfactory.de/submodels/skills/{Guid.NewGuid()}";
                var skills = new SkillsSubmodel(smSkillsId);
                submodels.Add(skills);
                logger.LogInformation($"   ✅ Skills: {smSkillsId}");

                // 2. Asset Administration Shell erstellen
                string shellId = $"https://smartfactory.de/aas/{Guid.NewGuid()}";
                string assetId = $"https://smartfactory.de/assets/{Guid.NewGuid()}";
                
                logger.LogInformation($"\n2. Erstelle AAS: {shellId}");
                
                var shell = new AssetAdministrationShell("SmartFactoryAAS", new Identifier(shellId))
                {
                    Description = new LangStringSet
                    {
                        new LangString("de", "Asset Administration Shell für Smart Factory Demo mit allen Submodellen"),
                        new LangString("en", "Asset Administration Shell for Smart Factory Demo with all Submodels")
                    },
                    AssetInformation = new AssetInformation()
                    {
                        AssetKind = AssetKind.Instance,
                        GlobalAssetId = new Identifier(assetId)  
                    }
                };

                // 3. Alle Submodelle zur Shell hinzufügen
                logger.LogInformation("\n3. Füge alle Submodelle zur Shell hinzu...");
                foreach (var submodel in submodels)
                {
                    shell.Submodels.Add(submodel);
                    logger.LogInformation($"   ✅ {submodel.IdShort} hinzugefügt");
                }

                // 4. ZUERST: Alle Submodelle hochladen (BEVOR die Shell hochgeladen wird!)
                logger.LogInformation("\n4. Lade alle Submodelle hoch (VOR der Shell)...");
                int successCount = 0;
                int failCount = 0;
                
                foreach (var submodel in submodels)
                {
                    var smResult = await submodelRepoClient.CreateSubmodelAsync(submodel);
                    
                    if (smResult.Success)
                    {
                        logger.LogInformation($"   ✅ {submodel.IdShort} erfolgreich hochgeladen");
                        successCount++;
                    }
                    else
                    {
                        logger.LogError($"   ❌ Fehler bei {submodel.IdShort}: {smResult.Messages}");
                        failCount++;
                    }
                }
                
                logger.LogInformation($"\n   Zusammenfassung: {successCount} erfolgreich, {failCount} fehlgeschlagen");

                if (failCount > 0)
                {
                    logger.LogWarning("   ⚠️ Einige Submodelle konnten nicht hochgeladen werden!");
                }

                // 4a. Shell als JSON speichern zur Inspektion
                logger.LogInformation("\n4a. Speichere AAS als JSON-Datei zur Inspektion...");
                string shellJsonPath = "shell_with_all_submodels.json";
                
                var jsonOptions = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                string shellJson = JsonSerializer.Serialize(shell, jsonOptions);
                await File.WriteAllTextAsync(shellJsonPath, shellJson);
                logger.LogInformation($"   ✅ AAS gespeichert in: {shellJsonPath}");

                // 5. DANACH: Shell hochladen (die Submodelle existieren bereits auf dem Server)
                logger.LogInformation("\n5. Lade AAS hoch (Submodelle sind bereits auf dem Server)...");
                var shellResult = await aasRepoClient.CreateAssetAdministrationShellAsync(shell);
                
                if (shellResult.Success)
                {
                    logger.LogInformation("   ✅ AAS erfolgreich hochgeladen.");
                }
                else
                {
                    logger.LogError($"   ❌ Fehler beim Hochladen der AAS: {shellResult.Messages}");
                    return;
                }

                // 6. Shell mit Submodels zurücklesen
                logger.LogInformation("\n6. Lese AAS mit Submodels vom Server zurück...");
                var retrievedShellResult = await aasRepoClient.RetrieveAssetAdministrationShellAsync(new Identifier(shellId));
                
                if (retrievedShellResult.Success && retrievedShellResult.Entity != null)
                {
                    var retrievedShell = retrievedShellResult.Entity;
                    logger.LogInformation($"   ✅ AAS zurückgelesen: {retrievedShell.IdShort}");
                    logger.LogInformation($"      ID: {retrievedShell.Id.Id}");
                    logger.LogInformation($"      Asset: {retrievedShell.AssetInformation?.GlobalAssetId}");
                    logger.LogInformation($"      Anzahl Submodels: {retrievedShell.Submodels?.Count ?? 0}");
                    
                    if (retrievedShell.Submodels != null && retrievedShell.Submodels.Count > 0)
                    {
                        logger.LogInformation("      Submodels:");
                        foreach (var sm in retrievedShell.Submodels)
                        {
                            logger.LogInformation($"        - {sm.IdShort} (ID: {sm.Id?.Id})");
                        }
                    }
                }
                else
                {
                    logger.LogError($"   ❌ Fehler beim Abrufen der AAS: {retrievedShellResult.Messages}");
                }

                // 7. Beispiel: Ein Submodel zurücklesen (MachineSchedule)
                logger.LogInformation("\n7. Lese MachineSchedule Submodel zurück...");
                var retrievedSmResult = await submodelRepoClient.RetrieveSubmodelAsync(new Identifier(smMachineScheduleId));
                
                if (retrievedSmResult.Success && retrievedSmResult.Entity != null)
                {
                    var retrievedSm = retrievedSmResult.Entity;
                    logger.LogInformation($"   ✅ Submodel zurückgelesen: {retrievedSm.IdShort}");
                    logger.LogInformation($"      ID: {retrievedSm.Id.Id}");
                    
                    if (retrievedSm.SemanticId?.Keys != null && retrievedSm.SemanticId.Keys.Count() > 0)
                    {
                        logger.LogInformation($"      Semantic ID: {retrievedSm.SemanticId.Keys.FirstOrDefault()?.Value}");
                    }
                    
                    logger.LogInformation($"      Anzahl SubmodelElements: {retrievedSm.SubmodelElements?.Count ?? 0}");
                    
                    if (retrievedSm.SubmodelElements != null && retrievedSm.SubmodelElements.Count > 0)
                    {
                        logger.LogInformation("      SubmodelElements:");
                        foreach (var element in retrievedSm.SubmodelElements)
                        {
                            logger.LogInformation($"        - {element.IdShort} ({element.ModelType})");
                        }
                    }
                }
                else
                {
                    logger.LogError($"   ❌ Fehler beim Abrufen des Submodels: {retrievedSmResult.Messages}");
                }

                logger.LogInformation("\n=== Demo erfolgreich abgeschlossen ===");
                logger.LogInformation($"Es wurden {submodels.Count} verschiedene Submodell-Typen demonstriert:");
                foreach (var sm in submodels)
                {
                    logger.LogInformation($"  - {sm.IdShort}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"❌ Fehler: {ex.Message}");
                logger.LogError($"Stack Trace: {ex.StackTrace}");
            }
        }
    }
}
