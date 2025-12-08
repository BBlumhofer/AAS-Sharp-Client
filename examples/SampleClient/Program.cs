using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using AasSharpClient.Extensions;
using AasSharpClient.Models.Remote;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;
using BaSyx.Clients.AdminShell.Http;

Console.WriteLine("=== AAS-Sharp-Client Server-Anbindung Test (Testserver: localhost:8080) ===");
Console.WriteLine();

var services = new ServiceCollection();
services.AddLogging(cfg => cfg.AddConsole().SetMinimumLevel(LogLevel.Information));

// Configure HTTP client for BaSyx with timeout and retry policy
services.AddHttpClient("basyx", (sp, client) =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.BaseAddress = new Uri("http://localhost:8080/");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
});

// Register BaSyx client services
services.AddBaSyxClientServices("basyx");

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Starte Test-Ablauf mit Server auf localhost:8080...");

    string basyxHost = "http://localhost:8080";
    
    // Clients initialisieren
    var aasRepoClient = new AssetAdministrationShellRepositoryHttpClient(new Uri($"{basyxHost}/shells"));
    var submodelRepoClient = new SubmodelRepositoryHttpClient(new Uri($"{basyxHost}/submodels"));

    // 1. Shell erstellen und hochladen
    string shellId = $"https://smartfactory.de/shells/{Guid.NewGuid()}";
    logger.LogInformation($"1. Erstelle und lade Shell hoch: {shellId}");
    
    var assetInfo = new AssetInformation 
    { 
        AssetKind = AssetKind.Instance, 
        GlobalAssetId = $"urn:asset:{Guid.NewGuid()}" 
    };
    var shell = new AssetAdministrationShell("Shell2", new Identifier(shellId));
    shell.AssetInformation = assetInfo;
    
    var shellResult = await aasRepoClient.CreateAssetAdministrationShellAsync(shell);
    if (shellResult.Success)
    {
        logger.LogInformation("   ✅ Shell erfolgreich hochgeladen.");
    }
    else
    {
        logger.LogError($"   ❌ Fehler beim Hochladen der Shell: {shellResult.Messages}");
        return;
    }

    // 2. Submodel erstellen und hochladen
    string smId = $"https://smartfactory.de/submodels/{Guid.NewGuid()}";
    logger.LogInformation($"2. Erstelle und lade Submodel hoch: {smId}");
    
    var machineSchedule = new MachineScheduleSubmodel(smId);
    var smResult = await submodelRepoClient.CreateSubmodelAsync(machineSchedule);
    
    if (smResult.Success)
    {
        logger.LogInformation("   ✅ Submodel erfolgreich hochgeladen.");
    }
    else
    {
        logger.LogError($"   ❌ Fehler beim Hochladen des Submodels: {smResult.Messages}");
        return;
    }

    // 3. Submodel zurücklesen
    logger.LogInformation("3. Lese Submodel vom Server zurück...");
    var retrieveResult = await submodelRepoClient.RetrieveSubmodelAsync(new Identifier(smId));
    
    if (retrieveResult.Success && retrieveResult.Entity != null)
    {
        logger.LogInformation("   ✅ Submodel erfolgreich gelesen.");
        
        // 4. Werte manipulieren (Schedule hinzufügen)
        logger.LogInformation("4. Manipuliere Werte (Füge Schedule hinzu)...");
        
        var container = new SchedulingContainer(
            DateTime.UtcNow.AddHours(1).ToString("o"),
            DateTime.UtcNow.AddHours(2).ToString("o"),
            "00:10:00",
            "01:50:00");
            
        // Wir nutzen unser lokales Wrapper-Objekt, um die Änderung vorzunehmen
        machineSchedule.AddOrUpdateSchedule(container);
        logger.LogInformation($"   Schedule Eintrag hinzugefügt. Anzahl lokal: {machineSchedule.GetSchedules().Count}");
        
        // 5. Update auf den Server pushen
        logger.LogInformation("5. Aktualisiere Submodel auf dem Server...");
        
        var updateResult = await submodelRepoClient.UpdateSubmodelAsync(new Identifier(smId), machineSchedule);
        if (updateResult.Success)
        {
            logger.LogInformation("   ✅ Update erfolgreich.");
        }
        else
        {
            logger.LogError($"   ❌ Update fehlgeschlagen: {updateResult.Messages}");
        }
        
        // 6. Verifikation: Erneut lesen
        logger.LogInformation("6. Verifikation: Lese erneut vom Server...");
        
        // Wir nutzen SyncFromSubmodelRepositoryAsync um direkt in ein neues Objekt zu laden
        var verifySchedule = new MachineScheduleSubmodel(smId);
        await verifySchedule.SyncFromSubmodelRepositoryAsync(new Uri($"{basyxHost}/submodels"), new Identifier(smId));
        
        var count = verifySchedule.GetSchedules().Count;
        logger.LogInformation($"   ✅ Gelesen. Anzahl Schedules: {count}");
        
        if (count == 1)
        {
            logger.LogInformation("🎉 Test-Ablauf erfolgreich abgeschlossen!");
        }
        else
        {
            logger.LogWarning($"⚠️ Unerwartete Anzahl an Schedules: {count}");
        }
    }
    else
    {
        logger.LogError($"   ❌ Fehler beim Lesen des Submodels: {retrieveResult.Messages}");
    }
    
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Genereller Fehler bei der Server-Anbindung: {Message}", ex.Message);
    Console.WriteLine($"Detaillierte Fehlermeldung: {ex}");
}
finally
{
    Console.WriteLine("\nDrücken Sie eine Taste, um zu beenden...");
    Console.ReadKey();
}