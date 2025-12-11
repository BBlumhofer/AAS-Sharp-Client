**ModuleGenerator — README**

Kurzbeschreibung: Dieses kleine .NET-Konsolen-Tool erzeugt vollständige Asset-Administration-Shell (AAS) JSON-Dateien aus kleinen Modul-Konfigurationsdateien. Es verwendet die Model-Klassen des Repos, serialisiert Submodelle mit den vorhandenen Normalizern und merge-t diese in die `template.json`-Basis.

**Voraussetzungen**:
- **.NET SDK**: `net10.0` (verwende die systemweit installierte `dotnet`-CLI).
- Repository-Kopie mit intakten Projektreferenzen (arbeite im Repo-Root).

**Wichtige Pfade**
- **Tool-Projekt**: `Tools/ModuleGenerator/ModuleGenerator.csproj`
- **Generator-Code**: `Tools/ModuleGenerator/ModuleGenerator.cs`
- **CLI-Wrapper**: `Tools/ModuleGenerator/Program.cs`
- **Template**: `Tools/ModuleGenerator/template.json` (wird als Basis verwendet)
- **Example/Configs**: `Tools/ModuleGenerator/Configs/` (Input: `*_config.json`)
-- **Standard-Ausgabe**: `$PWD/generated/` (relativ zum Arbeitsverzeichnis beim Aufruf). Beispiel beim Ausführen aus `Tools/ModuleGenerator`:
     `Tools/ModuleGenerator/generated/{ShellId}.json`
    Du kannst einen alternativen Ausgabepfad als zweites Argument angeben.

**Design-Entscheidungen / Verhalten**
- Basis: Immer `template.json` verwenden; nur die relevanten Submodelle (Skills, Capability, AssetLocation) werden ersetzt bzw. ergänzt.
- ID-Kollisionsschutz: Template-Submodel-IDs werden pro erzeugter Verwaltungsschale eindeutig gemacht — das Tool hängt `-<ShellId>` an die originalen Template-Submodel-IDs. Dadurch entstehen keine doppelten Submodel-IDs beim Laden mehrerer generierter Verwaltungsschalen.
- JsonNode-Sicherheit: Submodel-JSONs werden serialisiert und erneut geparst, bevor sie ins Template eingefügt werden, um System.Text.Json "already has a parent"-Fehler zu vermeiden.
- Reference-Enum-Korrektheit: `Reference.type` wird bewusst auf `ModelReference` (für modelinterne Pfade) oder `ExternalReference` (für GlobalReference-basierte Pfade) gesetzt, um inkompatible Enum-Werte wie `Undefined` zu vermeiden (AAS4J-kompatibel).

**Konfigurationsformat (Kurz)**
Die Eingabe ist eine kleine JSON-Datei (Beispiel: `P100_config.json`) mit Angaben wie Shell-Id, Skill-Definitionen, Capability-Constraints und optionalen AssetLocation-Feldern. Die genauen Felder sind projektintern und im `ModuleGenerator.cs` definiert.

**Ausführen / Beispiele**
- Verarbeite das gesamte Config-Ordner (Bulk-Modus, Standard, wenn keine Argumente):

```bash
dotnet run --project Tools/ModuleGenerator/ModuleGenerator.csproj --
```

- Verarbeite ein einzelnes Config-File:

```bash
dotnet run --project Tools/ModuleGenerator/ModuleGenerator.csproj -- /pfad/zu/P100_config.json
```

- Verarbeite alle Configs in einem anderen Verzeichnis:

```bash
dotnet run --project Tools/ModuleGenerator/ModuleGenerator.csproj -- /pfad/zum/configs-ordner
```

Die generierten Dateien landen in `Tools/ModuleGenerator/generated_examples/` mit dem Dateinamen `{ShellId}.json`.

**Typische Probleme & Lösungen**
- "Duplicate element id detected": Ursache war identische Template-Submodel-IDs für mehrere Verwaltungsschalen. Lösung: Generator hängt `-<ShellId>` an Template-Submodel-IDs und passt die AAS-Referenzen an.
- System.Text.Json JsonNode Parent-Fehler: Beim Re-Insert von JsonNode-Objekten trat "already has a parent" auf. Lösung: Die Submodel-JSONs werden serialisiert und mit `JsonNode.Parse(...)` erneut geparst (Deep clone) bevor sie ins Template gesetzt werden.
- AAS4J `No enum constant ... ReferenceTypes.UNDEFINED`: Das wurde durch explizites Setzen von `Reference.Type` behoben (ModelReference / ExternalReference).

**Erweiterungen / ToDos**
- Optional: Automatische Validierung nach Generierung (prüfen auf `"type":"Undefined"` und doppelte Submodel-IDs). Diese Aufgabe ist als TODO markiert und kann beim nächsten Schritt implementiert werden.
- Optional: Anpassbares ID-Format (statt `oldId + "-" + shellId` z.B. GUID, Prefix, usw.). Sag mir Bescheid, wenn Du ein anderes Format möchtest.

**Entwicklungs-Hinweise**
- Beim Entwickeln lokal immer `dotnet build` und optional `dotnet run` aus dem Projektpfad testen.
- Wenn Du neue Configs anlegst, lege sie in `Tools/ModuleGenerator/Configs/` ab und führe das Tool im Bulk-Modus aus.

**Kontakt / Änderungen**
Änderungswünsche, z. B. anderes ID-Schema oder Integration einer Validierungsstufe, kann ich gerne implementieren — sag kurz welches Verhalten Du bevorzugst.

----
(Diese README wurde automatisch erzeugt; sie fasst aktuelles Verhalten des Generators zusammen.)
## Module Generator
Ziel des Tools: Vereinfachtes Erstellen von ModulVerwaltungsschalen aus minimal_config.json:
Allgemein: Erzeugt aus einer sehr einfachen Konfiguration (`config.json`) eine AAS-Asset-Administration-Shell
und die zugehörigen Submodel-JSONs. Die erzeugten Dateien landen unter `Tools/ModuleGenerator/generated/`.

Konkretes Ziel (erste Minimalversion):
- Erzeuge `AssetAdministrationShell` mit `id` = `Id` aus der Konfiguration.
- Erzeuge ein `Skills`-Submodel mit einem einfachen `Skill` (Name, RequiredInputParameters -> `ProductId`).
- Erzeuge ein `OfferedCapabilityDescription`-Submodel mit einem Capability-Container und Property-Containern
    (Range oder FixedValue) basierend auf `Capability.PropertyContainers` in der Config.
- Exportiere ein kombiniertes JSON mit `assetAdministrationShells` und `submodels` nach
    `Tools/ModuleGenerator/generated/{Id}.json`.

Konfigurationsschema (vereinfachte Form - siehe `Examples/P18_config.json`):

- `Id` (string): Identifikator des Moduls (z.B. `P1001`).
- `Skill` (string): Name des Skills (z.B. `Screw`).
- `Capability` (object): {
    - `Name` (string)
    - `SkillReference` (string)
    - `PropertyContainers` (object): Schlüssel -> Objekt mit entweder `min`+`max` oder `value`.
    - `Constraints` (array): optionale Constraint-Objekte (erste Minimalausprägung unterstützt)
}

Beispiel-Input: `Tools/ModuleGenerator/Examples/P18_config.json` (bereits im Repo).

Usage (Entwicklungsablauf):

1. Build des Projekts (vom Repo-Root):

```bash
dotnet build
```

2. Ausführen des Generators (lokal, Beispiel):

```bash
cd Tools/ModuleGenerator
dotnet run --project ModuleGenerator.csproj -- Examples/P18_config.json
# Ausgabe: Tools/ModuleGenerator/generated/P18.json
```

3. Tests: Es gibt einen Unit-Test in `tests/AasSharpClient.Tests/ModuleGeneratorTests.cs`, der den Generator
     aufruft und prüft, ob die erzeugte Datei durch die vorhandenen Deserialisierer (`BasyxJsonLoader.Options`)
     geladen werden kann.

Entwicklerhinweise / Patterns:

- Verwende die vorhandenen Factory-Methoden in `Models/` (z.B. `SubmodelElementFactory`, `CapabilityDescription*`-Records),
    statt rohe JSON-Strukturen zu bauen.
- Ziel-Framework: `net10.0` (Tests und bestehender Code verwenden `net10.0`).
- Generated files: `Tools/ModuleGenerator/generated/{Id}.json`.

Nächste Schritte / Erweiterungen:
- Bessere Unterstützung für Constraints (vollständige Abbildung auf `PropertyConstraintContainerDefinition`).
- CLI-Flags (z.B. output path, namespace, thumbnail) und Integration in CI.
