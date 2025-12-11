# ProductGenerator

Kurzbeschreibung
---------------
`ProductGenerator` erzeugt Asset-Administration-Shell (AAS) JSON-Dateien für Produkte aus kleinen Konfigurationsdateien. Das Tool verwendet ein vordefiniertes Template (`Cab_A_Blue-environment.json`) und baut die Capability-Submodelle gemäß der Eingangs-Config auf.

Wichtiges Verhalten
-------------------
- Es werden Capability-Einträge für jedes Element in `Capabilities` der Config erzeugt.
- Constraints werden nicht erzeugt.
- Es wird bewusst **keine** `RealizedBy`-Relation in der Produkt-Ausgabe erzeugt (Produkt-Relationen werden weggelassen).
- Um ID-Kollisionen zu vermeiden, werden vorhandene Template-Submodel-IDs mit `-<ShellShort>` erweitert und die `assetAdministrationShells[*].submodels`-Referenzen entsprechend aktualisiert.

Voraussetzungen
-------------
- .NET SDK (Target: `net10.0`)

Wichtige Pfade
--------------
- Projekt: `Tools/ProductGenerator/ProductGenerator.csproj`
- Template: `Tools/ProductGenerator/Template/Cab_A_Blue-environment.json`
- Beispiel-Config: `Tools/ProductGenerator/configs/Cab_B_Red_config.json`
- Standard-Ausgabe: `$PWD/generated/` (relativ zum aktuellen Arbeitsverzeichnis beim Aufruf). Alternativ kannst Du einen anderen Ausgabepfad als zweites CLI-Argument angeben.

Konfigurationsformat (Beispiel)
--------------------------------
Beispiel `Cab_B_Red_config.json`:

```json
{
  "Id": "https://smartfactory.de/shells/IZ_LH-AkUF",
  "Capabilities": [
    {
      "Name": "Drill",
      "PropertyContainers": {
        "Depth": 54.5,
        "Diameter": 5.7,
        "ProductId": "https://smartfactory.de/shells/IZ_LH-AkUF"
      }
    },
    {
      "Name": "Screw",
      "PropertyContainers": {
        "Torque": 45,
        "Diameter": 6,
        "ProductId": "https://smartfactory.de/shells/IZ_LH-AkUF"
      }
    }
  ]
}
```

Wie der Generator arbeitet (Kurz)
-------------------------------
1. Lade das Template `Cab_A_Blue-environment.json`.
2. Lese die Config und erzeuge für jede Capability einen `SubmodelElementCollection`-Container mit:
   - einem `Capability`-Element (idShort = Capability-Name),
   - einem `PropertySet` mit `Property`-Elementen für die Felder in `PropertyContainers` (ausgenommen `ProductId`).
3. Füge die erzeugten Capability-Container in das Capability-Submodel (`OfferedCapabilityDescription`) ein.
4. Ersetze/ergänze Template-Submodel-IDs (Suffix `-<ShellShort>`) und passe die AAS-Referenzen an.
5. Schreibe die resultierende AAS JSON-Datei nach `$PWD/generated/{ShellShort}.json`.

Ausführen / Beispiele
----------------------
- Standard (verwendet die Beispiel-Config, ausgeführt aus Repo-Root):

```bash
dotnet run --project Tools/ProductGenerator/ProductGenerator.csproj --
```

- Von innerhalb des Tools-Ordners:

```bash
cd Tools/ProductGenerator
dotnet run --project . -- configs/Cab_B_Red_config.json
```

- Mit alternativem Ausgabeordner:

```bash
dotnet run --project Tools/ProductGenerator/ProductGenerator.csproj -- configs/Cab_B_Red_config.json /tmp/my-output
```

Ausgabe
-------
- Standard: `./generated/{ShellShort}.json` relativ zum aktuellen Arbeitsverzeichnis.

Typisierung der Properties
-------------------------
- Aktuell werden Property-Werte als einfache `Property`-Elemente mit `valueType: double` erzeugt. Falls Du eine präzisere Typisierung (z. B. `integer` oder `string`) willst, kann ich die Logik erweitern, um die Typen aus den JSON-Werten zu bestimmen.

Fehlerbehebung / Known Issues
----------------------------
- Wenn beim Laden der generierten JSONs in anderen Tools Fehler auftreten (z. B. `Reference.type: Undefined`), prüfe, ob `Reference.type` in generierten JSONs gültige Werte (`ModelReference` oder `ExternalReference`) hat. Der Generator versucht, gültige Werte zu setzen; wenn Du weiterhin Probleme siehst, schick mir bitte die Fehlermeldung und ein Beispiel-JSON.
- Beim Kompilieren mehrerer Projekte in der Solution kann der Compiler über mehrere `Main`-Methoden klagen — starte das Tool direkt mit `--project Tools/ProductGenerator/ProductGenerator.csproj`, damit nur dieses Projekt gebaut wird.

Weiterentwicklung / Vorschläge
----------------------------
- Automatische Validierung: Vor dem Schreiben prüfen auf doppelte Submodel-IDs oder ungültige Enum-Werte (kann ich optional hinzufügen).
- Bessere Typableitung für Property-Werte (int/float/string) statt statisch `double`.

Kontakt / Änderungen
--------------------
- Sag mir, ob ich Validierung oder strengere Typisierung implementieren soll — ich übernehme das gern.
