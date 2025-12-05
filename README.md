# AAS-Sharp-Client

A lightweight C# extension for the Eclipse Basyx .NET SDK that generates and exposes strongly-typed custom classes for Submodels defined in templates. Ziel ist es, aus in Templates abgelegten Submodellen automatisch C#-Klassen inkl. Methoden zu erzeugen und diese nahtlos mit dem `basyx-dotnet` SDK nutzbar zu machen.

## Projektziel
- Erweiterung des `basyx-dotnet` SDK so, dass Submodel-Templates in strongly-typed C#-Klassen abgebildet werden.
- Unterst�tzung von Methoden / Operationen, die in Submodel-Templates definiert sind (z. B. Inputs, Outputs, Ausf�hrungslogik-Aufrufe �ber REST/OPC UA etc.).
- Einfaches Entwickeln, Testen und Deployen von Custom-Submodels f�r Asset Administration Shells (AAS) in .NET-Projekten.

## Hauptfunktionen (geplant)
- Template-Parser: Liest Submodel-Templates (JSON/JSON-LD) und extrahiert Properties, Sub-Submodels und Operationen.
- Code-Generator: Erzeugt C#-POCO-Klassen inkl. Serialisierung/Deserialisierung.
- Methoden-Stub-Generator: Erstellt Methoden-Signaturen f�r Submodel-Operationen und optionale Default-Implementierungen.
- Integrationslayer: Wrapper/Adapter, die generierte Klassen an `basyx-dotnet` Objekte binden (z. B. `Submodel`, `SubmodelElement`), einschlie�lich CRUD und Invoke-Operationen.
- Template-Repository-Schnittstelle: Unterst�tzung f�r lokale Templates, Git-Repos oder registrierte Template-Registries.

## Architektur-�berblick
- `TemplateParser` � Analysiert Template-Dateien und erzeugt ein Zwischenmodell (AST).
- `ModelGenerator` � Erzeugt C#-Quelltext aus dem Zwischenmodell und bietet Optionen (z. B. Namespaces, Async-Support).
- `BasyxAdapter` � Verantwortlich f�r die Abbildung zwischen generierten Klassen und `basyx-dotnet` Laufzeitobjekten.
- `CLI` / `MSBuild`-Task � Optionaler Weg, Klassen beim Build zu generieren oder per Kommandozeile zu erzeugen.

## Beispiel-Workflows
- Entwickler legt ein Submodel-Template (`.jsonld`) in ein Template-Verzeichnis.
- CLI / MSBuild-Task generiert C#-Klassen in Zielnamespace `AasSharpClient.Models`.
- Anwendung instanziiert generierte Klasse und registriert sie mit `BasyxAdapter` an einem laufenden AAS-Server.

## Offene Fragen / Entscheidungs-Punkte (f�r Implementierung durch LLM)
1. Welche Template-Formate m�ssen priorit�r unterst�tzt werden? (`JSON`, `JSON-LD`, `AASX`?)
2. Wie sollen Operationen methodisch abgebildet werden: synchronous methods, async Task, oder beide?
3. Wie werden komplexe Datentypen (Envelopes, DataSpecifications, Referenzen) �bersetzt? Flache POCOs, Nested-Classes oder `JObject`-Fallback?
4. Error-/Exception-Modell: Wie sollen Fehler bei Operation-Invoke, Mapping und Serialisierung gehandhabt werden?
5. Versioning: Wie gehen wir mit Template-�nderungen und generiertem Code um (Fingerprinting, Partial classes, Regeneration-Strategie)?
6. Extensibility: Plugin-Schnittstelle f�r benutzerdefinierte Typen/Mapping (z. B. f�r dom�nenspezifische Datentypen)?
7. Tests: Welche Test-Strategie (Unit, Integration gegen Basysx-Stub/Mock)?
8. Integration mit `basyx-dotnet`: Welche Basisklassen/Interfaces aus dem offiziellen Repo sollen verwendet werden? (Link unten)
9. Security / Auth: Muss Adapter Authentifizierungs-Mechanismen (OAuth2, Basic, Zertifikate) weiterleiten?
10. Deployment: Soll das Projekt NuGet-Paket + SourceGenerator oder konventioneller Code-Generator werden?

Bitte priorisieren und kurze Antworten/Entscheidungen zu diesen Punkten geben, damit die sp�tere Implementierung konsistent ist.

## Roadmap (erste Schritte)
- [ ] Abgrenzung der unterst�tzten Template-Formate.
- [ ] Minimaler Parser f�r `JSON`/`JSON-LD` Submodel-Templates.
- [ ] Proof-of-Concept: Generator erzeugt POCO + Adapter f�r einfache Property-Get/Set.
- [ ] Implementierung von Operation-Stub-Generation (sync/async Option).
- [ ] Integrationstest mit `basyx-dotnet` Beispiel-Server.

## Zusammenarbeit
Dieses README dient als lebendiges Dokument. Nutze es, um Entscheidungen, Anforderungen und offene Aufgaben festzuhalten, die ein LLM oder Entwicklerteam f�r die Umsetzung ben�tigt. Bitte beantworte die offenen Fragen oben oder markiere Priorit�ten.

## Referenzen
- Eclipse Basyx .NET SDK: https://github.com/eclipse-basyx/basyx-dotnet

## Lizenz
Standardm��ig kein Lizenz-Text enthalten � bitte Projektlizenz erg�nzen.

## Dependency Injection (DI) & Beispiele

Das Paket ist als Bibliothek konzipiert. Verbraucher (Applications) sollten die DI-Registrierung in ihrem Startup / `Program.cs` vornehmen.

Minimalbeispiel (registriert `IHttpClientFactory`, den BaSyx-Client-Factory-Fabrik und den `IRemoteScheduleSyncService`):

```csharp
using Microsoft.Extensions.DependencyInjection;
using System;

var services = new ServiceCollection();

// Konfiguriere benannten HttpClient für BaSyx-Clients
services.AddHttpClient("basyx", client =>
{
	client.Timeout = TimeSpan.FromSeconds(30);
});

// Registriere die Bibliotheks-Services (Submodel client factory + Remote sync service)
services.AddBaSyxClientServices("basyx");

var provider = services.BuildServiceProvider();
// Jetzt: provider.GetRequiredService<IRemoteScheduleSyncService>() usw.
```

Optional: Authentifizierung / Retry

- Für Auth: Füge einen `DelegatingHandler` hinzu oder konfiguriere `HttpClient`-Handler (z. B. `Client.DefaultRequestHeaders.Authorization`).
- Für Retry/Resiliency: benutze `Polly` in Kombination mit `Microsoft.Extensions.Http.Polly` (Beispiel im `examples/`-Ordner).

Beispielprojekt

Ein einfaches Beispiel befindet sich in `examples/SampleClient`. Es zeigt:
- Konfiguration eines benannten `HttpClient` mit Polly Retry-Policy
- Aufruf von `IRemoteScheduleSyncService.SyncToAsync(...)`

Wie ausführen:

1. Wechsel in das Beispielverzeichnis:

```powershell
cd "examples\SampleClient"
dotnet run -- "https://your-testserver/submodels/mysm"
```

2. Ersetze die URL mit deinem Testserver-Endpunkt. Falls erforderlich: füge Auth-Konfiguration zum benannten HttpClient hinzu.

Wenn du möchtest, richte ich zusätzlich ein lokales Mock-Server-Beispiel (WireMock.Net) ein und schreibe Integrationstests dagegen.
