using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using AasSharpClient.Extensions;
using AasSharpClient.Models.Remote;
using AasSharpClient.Models;

Console.WriteLine("AAS-Sharp-Client Sample: BaSyx IHttpClientFactory integration example");

var services = new ServiceCollection();
services.AddLogging(cfg => cfg.AddConsole());

// Configure a named HttpClient for BaSyx clients. Adjust timeouts/auth as needed.
services.AddHttpClient("basyx", (sp, client) =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
// Add a simple Polly retry policy for transient errors
.AddPolicyHandler(HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

// Register the BaSyx factory + remote sync service via the library-provided helper
services.AddBaSyxClientServices("basyx");

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();

// Resolve the sync service from DI
var sync = provider.GetRequiredService<IRemoteScheduleSyncService>();

// Create a lightweight MachineScheduleSubmodel to send
var schedule = new MachineScheduleSubmodel();

if (args.Length == 0)
{
    logger.LogInformation("No endpoint provided. Usage: SampleClient <submodel-endpoint-URL>");
    return;
}

if (!Uri.TryCreate(args[0], UriKind.Absolute, out var endpoint))
{
    logger.LogError("Invalid endpoint URL: {0}", args[0]);
    return;
}

try
{
    logger.LogInformation("Syncing to {0}", endpoint);
    await sync.SyncToAsync(schedule, endpoint);
    logger.LogInformation("Sync completed successfully.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Sync failed");
}
