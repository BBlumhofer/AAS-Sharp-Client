<!-- Copilot / AI Agent instructions for AAS-Sharp-Client -->
# Kurzüberblick
Dieses Repository stellt einen .NET-Client und Modellklassen für Asset Administration Shell (AAS) Objekte bereit
und enthält einen umfangreichen Satz an Submodel-Implementierungen, Serialisierern und Tests.
Ziel dieser Datei ist, AI-Coding-Agenten schnell produktiv zu machen: Architektur, Muster, wichtige Dateien
und konkrete Befehle zum Bauen und Testen.

**Big Picture**
- **Domain:** Verwaltungsschalen / AAS (Asset Administration Shell) mit Submodels (Skills, CapabilityDescription, Nameplate, u.v.m.).
- **Architektur:** POCO-Submodel-Klassen im Verzeichnis `Models/` (z.B. `Models/Skills.cs`, `Models/CapabilityDescription/CapabilityDescription.cs`).
- **Workflows:** Erzeugung von Submodels, Serialisierung via `SubmodelSerialization`, Hochladen/Lesen via BaSyx HTTP-Clients (`Program.cs`).

**Wichtige Dateien / Einstiegspunkte**
- **`Program.cs`** : Beispiel-Runner, zeigt Erzeugung mehrerer Submodel-Instanzen, Upload via `AssetAdministrationShellRepositoryHttpClient` / `SubmodelRepositoryHttpClient`.
- **`Models/Skills.cs`** : Implementierung eines `SkillsSubmodel` mit Hilfsfunktionen zum Erzeugen von Skill-Elementen.
- **`Models/CapabilityDescription/CapabilityDescription.cs`** : Factory- und Template-Pattern zur Erzeugung komplexer CapabilityDescription-Submodels.
- **`SubmodelSerialization`** (mehrere Verwendungen in `Models/*`) : Verwenden zum Serialisieren/Deserialisieren von Submodels.
- **`tests/AasSharpClient.Tests/`** : Viele Tests, inkl. `TemplateAndMessageDeserializationTests.cs`, die zeigen wie JSON-Templates geladen und in Laufzeitklassen zurück deserialisiert werden.

**Projekt-/Build-Konventionen**
- Ziel-Framework der Tests ist `net10.0` (`tests/AasSharpClient.Tests/AasSharpClient.Tests.csproj`).
- Repo-Projekte verwenden moderne `dotnet`-SDK-Projekte; für neue Tools empfehle `net10.0`.
- Neue Projekte: lege sie unter `Tools/ModuleGenerator` an und füge `ProjectReference` zur Lösung bzw. Tests hinzu.

**Serialisierung / Deserialisierung**
- Serialisierung: `SubmodelSerialization.SerializeAsync(...)` bzw. `SubmodelSerialization.SerializeElements(...)`.
- Deserialisierung / Loader: Tests verwenden `BasyxJsonLoader.Options` beim `JsonSerializer.Deserialize<ISubmodelElement>(..., options)`.
- CapabilityDescription hat zusätzlich einen JSON-Normalizer: `CapabilityDescriptionJsonNormalizer.Normalize(serialized)` — halte diesen Schritt für Capability-Templates im Kopf.

**Wichtige Patterns / Konventionen im Code**
- Ids: Viele Submodel-Ids werden als URIs mit Guid erzeugt, z.B. `https://smartfactory.de/submodels/{type}/{Guid}` (siehe `Program.cs`).
- Semantic References: `ReferenceFactory.External((KeyType.GlobalReference, "https://..."))` wird breit verwendet; achte beim Erzeugen auf korrekte `Reference`-Typen.
- Factory-Pattern: `CapabilityDescriptionElementFactory` erzeugt Collections/Containers; benutze vorhandene Factory-Methoden statt low-level ISubmodelElement-Konstruktionen.
- Ohne Kind/Kontext: Helper wie `WithoutKind(...)` werden eingesetzt, um `Kind`/`modelType`-Details zu bereinigen — beim Roundtrip beachten.

**Tests & Entwicklertools**
- Build: `dotnet build` vom Repo-Root.
- Tests: `dotnet test tests/AasSharpClient.Tests` (target framework `net10.0`).
- Beispiel: `tests/AasSharpClient.Tests/TemplateAndMessageDeserializationTests.cs` zeigt, wie Templates geladen und per `JsonSerializer`/`BasyxJsonLoader.Options` validiert werden.

**Integration / Laufzeit**
- HTTP-Clients: `BaSyx.Clients.AdminShell.Http`-Clients (`AssetAdministrationShellRepositoryHttpClient`, `SubmodelRepositoryHttpClient`) werden in `Program.cs` für Upload/Download verwendet.
- Wenn Du neue JSON-Dateien erzeugst (ModuleGenerator), lege sie unter `Tools/ModuleGenerator/generated/` ab und nutze vorhandene Deserialisierer (BasyxJsonLoader) zum Validieren.

**Konkrete Beispiele für AI-Agenten (Code-Snippets / Tasks)**
- Erzeuge ein `SkillsSubmodel`-Eintrag: nutze `new SubmodelElementCollection(idShort)` und `SubmodelElementFactory.CreateProperty(...)` oder die Helper in `SkillsSubmodel.CreateSkill(...)`.
- Erzeuge eine CapabilityContainer: verwende `CapabilityDescriptionElementFactory.CreateCapabilityContainer(...)` oder baue `CapabilityContainerDefinition` und rufe `CapabilityDescriptionSubmodel.Apply(...)`.
- Serialisierungs-Check: `var json = await mySubmodel.ToJsonAsync(); var deserialized = JsonSerializer.Deserialize<ISubmodelElement>(elementJson, BasyxJsonLoader.Options);` — Tests zeigen Muster.

**When changing the repo**
- Füge neue Projekte zur Solution hinzu (öffentliche SlN: `AAS Sharp Client.slnx`).
- Falls Du neue Serialisierer/Converters hinzufügst: prüfe bestehende Tests in `tests/AasSharpClient.Tests/JsonConverters` auf Stil und Optionen (z.B. `OperationVariableSetJsonConverter`).

**Kurzcheck-Liste vor PR**
- Kompiliert `dotnet build` ohne Fehler.
- `dotnet test tests/AasSharpClient.Tests` läuft lokal durch (oder neue Tests mindestens lokal grün).
- Neue JSON-Beispiele in `tools`/`templates` haben Roundtrip-Validierung mittels `BasyxJsonLoader.Options`.

Feedback
- Ist etwas unklar oder fehlen Dir Beispiele (z. B. CLI-Flags, CI-Schritte)? Sag kurz welche Bereiche Du erweitern möchtest — ich passe die Anleitung an.

---
<!-- Ende der Copilot-Instructions -->
