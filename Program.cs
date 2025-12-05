using AasSharpClient.Models;
using System.Collections.Generic;

// alias to avoid conflict with System.Action delegate
using ActionModel = AasSharpClient.Models.Action;

var defaultSkillReference = new SkillReference(new List<(object Key, string Value)>
{
	(ModelReferenceEnum.Submodel, "https://example.com/ids/sm/4510_5181_3022_5180"),
	(ModelReferenceEnum.SubmodelElementCollection, "Skills"),
	(ModelReferenceEnum.SubmodelElementCollection, "Skill_0001")
});

var inputParameters = new InputParameters(new Dictionary<string, string>
{
	{ "RetrieveByProductType", "true" },
	{ "ProductType", "Semitrailer_Chassis" },
	{ "ProductID", string.Empty }
});

var finalResultData = new FinalResultData(new Dictionary<string, object>
{
	{ "StartTime", "2025/12/02 15:59:49" },
	{ "EndTime", "2025/12/02 16:00:29" },
	{ "SuccessfulExecutionsCount", 9 }
});

var action = new ActionModel(
	"Action0001",
	"RetrieveToPortLogistic",
	ActionStatusEnum.PLANNED,
	inputParameters,
	finalResultData,
	defaultSkillReference,
	"StorageModule");

var scheduling = new SchedulingContainer(
	"2025-12-02 15:58:45",
	"2025-12-02 15:59:25",
	"00:00:00",
	"00:00:40");

var step = new Step(
	"Step0001",
	"Load",
	StepStatusEnum.PLANNED,
	action,
	"P24",
	scheduling,
	"SmartFactory-KL",
	"_PHUKET");

var productionPlan = new ProductionPlan(false, 1, step);
var json = await productionPlan.ToJsonAsync();
Console.WriteLine(json);

/*
// Beispiel: DI-Registrierung für IHttpClientFactory + BaSyx-Factory + RemoteScheduleSyncService
// Entferne die Kommentarzeichen und passe ggf. Timeout/Handler an, wenn du echte Integration testen willst.
// Benötigte Namespaces: Microsoft.Extensions.DependencyInjection, AasSharpClient.Extensions, AasSharpClient.Models.Remote

// var services = new ServiceCollection();
// // Konfiguriere benannten HttpClient (auth/retry/timeout hier hinzufügen)
// services.AddHttpClient("basyx", client =>
// {
//     client.Timeout = TimeSpan.FromSeconds(30);
//     // client.BaseAddress = new Uri("https://your-basyx-server/"); // optional
// })
// // Beispiel: addiere eine Polly-Policy-Handler (falls Paket installiert)
// // .AddPolicyHandler(Policy<HttpResponseMessage>
// //     .Handle<HttpRequestException>()
// //     .OrResult(r => !r.IsSuccessStatusCode)
// //     .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

// // Registriere die BaSyx-Factory, die IHttpClientFactory verwendet
// services.AddBaSyxSubmodelClientFactory("basyx");
// // Registriere den RemoteSync-Service
// services.AddScoped<IRemoteScheduleSyncService, RemoteScheduleSyncService>();

// var provider = services.BuildServiceProvider();
// var sync = provider.GetRequiredService<IRemoteScheduleSyncService>();
// // Beispielaufruf (sofern dein Testserver erreichbar ist):
// // await sync.SyncToAsync(myMachineSchedule, new Uri("https://your-testserver/api/submodels/mysm"), CancellationToken.None);

*/
