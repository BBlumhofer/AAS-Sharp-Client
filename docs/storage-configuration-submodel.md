# StorageConfiguration Submodel (AAS)

[<- Zurueck zur Uebersicht](README.md)

---

## 1. Zweck und Geltungsbereich

Dieses Dokument spezifiziert das Verwaltungsschalen-Submodell **StorageConfiguration** fuer ressourcenspezifische Lagerparameter im Carrier Management System (CMS). Das Submodell beschreibt:

- die logische Struktur von Storages und Slots einer Ressource,
- Kostenfunktions- und Demand-Parameter,
- Projektionseinstellungen fuer Kostenbewertung.

Die Konfiguration im Submodell hat Vorrang vor lokaler Konfiguration und Umgebungsvariablen (siehe [configuration.md](configuration.md)).
Weitere relevante Dokumente:
- [Kostenmodell](cost-model.md) (Kostenfunktionen und Parameter)
- [Demand-Matching](demand-matching.md) (Demand-Status und Gewichtung)
- [Datenmodell](data-model.md) (Graph-Mapping und Slot-Properties)

---

## 2. Identifikation und Konventionen

### 2.1 Submodel-Identifikation

| Property | Wert | Quelle |
|---|---|---|
| `idShort` | `StorageConfiguration` | AAS Rules |
| `semanticId` | `https://smartfactory.de/semantics/submodel/CarrierManagement/StorageConfiguration#1/0` | AAS Rules |
| `id` | `https://smartfactory.de/submodels/$UUID` | AAS Rules |

### 2.2 Shell-Kontext (Ressource)

| Property | Wert |
|---|---|
| `assetType` der Shell | `resource` oder `resource/$ResourceType` |
| `namespace` (Extension) | Name der Insel, z. B. `_KUBA` |
| `specificAssetId` | `{ "name": "de.smartfactory.specificAssetId", "value": "$internalId" }` |

---

## 3. Submodel-Struktur (Uebersicht)

```
StorageConfiguration
├─ Storages (Collection) -> Liste konfigurierter Storages einer Ressource
│  ├─ Storage_* (Collection, je Storage) -> Konfiguration eines Storages
│  │  ├─ StorageId (Property, string) -> eindeutige Storage-ID
│  │  ├─ Name (Property, string) -> Anzeigename des Storages
│  │  ├─ TotalSlots (Property, int) -> Anzahl Slots im Storage
│  │  ├─ CostFunctionType (Property, string) -> Typ der Kostenfunktion
│  │  ├─ BaseCost (Property, double) -> Basiskosten $c_0$
│  │  ├─ Alpha (Property, double) -> Steilheit (nur exponential)
│  │  ├─ LowCost (Property, double, optional) -> niedrige Kosten (nur step)
│  │  ├─ HighCost (Property, double, optional) -> hohe Kosten (nur step)
│  │  ├─ StepThreshold (Property, double, optional) -> Schwellwert (nur step)
│  │  ├─ MaxCost (Property, double) -> Kostendeckel
│  │  └─ Slots (Collection) -> Slot-spezifische Parameter
│  │     ├─ Slot_* (Collection, je Slot) -> Konfiguration eines Slots
│  │     │  ├─ SlotId (Property, string) -> eindeutige Slot-ID
│  │     │  ├─ PreferredType (Property, string, optional) -> bevorzugter WST des Lagerplatzes
│  │     │  ├─ AffinityReward (Property, double) -> Belohnung bei Typ-Match
│  │     │  └─ AffinityPenalty (Property, double) -> Strafe bei Typ-Mismatch
├─ DemandConfig (Collection) -> Demand-Bonus und Statusgewichte
│  ├─ DemandBonusBase (Property, double) -> Basisbonus $b$
│  ├─ DemandBonusMax (Property, double) -> maximaler Bonus
│  ├─ DemandWeightPotential (Property, double) -> Gewicht fuer Potential
│  ├─ DemandWeightPlanned (Property, double) -> Gewicht fuer Planned
│  ├─ DemandWeightImminent (Property, double) -> Gewicht fuer Imminent
│  ├─ DemandWeightExecuting (Property, double) -> Gewicht fuer Executing
│  └─ UrgencyEnabled (Property, boolean) -> Dringlichkeitsmultiplikator aktiv
└─ ProjectionConfig (Collection) -> Projektion fuer belegungsbasierte Kosten
  ├─ WeightNoAgent (Property, double) -> Gewicht ohne Agent
  ├─ WeightStepOpen (Property, double) -> Gewicht fuer offenen Schritt
  ├─ WeightStepPlanned (Property, double) -> Gewicht fuer geplanten Schritt
  ├─ WeightStepExecuting (Property, double) -> Gewicht fuer laufenden Schritt
  └─ MaxStepsAhead (Property, int) -> Projektionstiefe
```

---

## 4. Datenelemente (Normativ)

### 4.1 Storages (Collection)

- `idShort`: `Storages`
- `modelType`: `SubmodelElementCollection`
- `value`: Liste von Storage-Collections

### 4.2 Storage_* (Collection)

| Feld | Typ | Pflicht | Beschreibung |
|---|---|---|---|
| `StorageId` | string | ja | Eindeutige Storage-ID (z. B. `storage-zentrallager-01`) |
| `Name` | string | ja | Anzeigename |
| `TotalSlots` | int | ja | Gesamtanzahl Slots |
| `CostFunctionType` | string | ja | `exponential` | `step` | `hyperbolic` |
| `BaseCost` | double | ja | Basis $c_0$ (alle Typen) |
| `Alpha` | double | optional | Steilheit fuer `exponential` |
| `LowCost` | double | optional | Niedrige Kosten fuer `step` |
| `HighCost` | double | optional | Hohe Kosten fuer `step` |
| `StepThreshold` | double | optional | Schwellwert fuer `step` |
| `MaxCost` | double | ja | Kostendeckel |
| `Slots` | Collection | ja | Liste von Slot-Collections |

**Regeln:**
- `CostFunctionType=exponential` -> `Alpha` erforderlich, `LowCost/HighCost/StepThreshold` verboten.
- `CostFunctionType=step` -> `LowCost`, `HighCost`, `StepThreshold` erforderlich, `Alpha` verboten.
- `CostFunctionType=hyperbolic` -> nur `BaseCost` und `MaxCost` erforderlich.

### 4.3 Slots (Collection) und Slot_* (Collection)

| Feld | Typ | Pflicht | Beschreibung |
|---|---|---|---|
| `SlotId` | string | ja | Eindeutige Slot-ID |
| `PreferredType` | string | optional | Bevorzugter Carrier-Typ |
| `AffinityReward` | double | ja | Belohnung bei Typ-Match |
| `AffinityPenalty` | double | ja | Strafe bei Typ-Mismatch |

### 4.4 DemandConfig (Collection)

| Feld | Typ | Pflicht | Beschreibung |
|---|---|---|---|
| `DemandBonusBase` | double | ja | Basisbonus $b$ |
| `DemandBonusMax` | double | ja | Maximaler Bonus |
| `DemandWeightPotential` | double | ja | Gewicht fuer Status `Potential` |
| `DemandWeightPlanned` | double | ja | Gewicht fuer Status `Planned` |
| `DemandWeightImminent` | double | ja | Gewicht fuer Status `Imminent` |
| `DemandWeightExecuting` | double | ja | Gewicht fuer Status `Executing` |
| `UrgencyEnabled` | boolean | ja | Aktiviert Dringlichkeitsmultiplikator |

### 4.5 ProjectionConfig (Collection)

| Feld | Typ | Pflicht | Beschreibung |
|---|---|---|---|
| `WeightNoAgent` | double | ja | Gewicht ohne Agent |
| `WeightStepOpen` | double | ja | Gewicht fuer offenen Schritt |
| `WeightStepPlanned` | double | ja | Gewicht fuer geplanten Schritt |
| `WeightStepExecuting` | double | ja | Gewicht fuer laufenden Schritt |
| `MaxStepsAhead` | int | ja | Projektionstiefe |

---

## 5. Validierungsregeln (Normativ)

- `BaseCost > 0`
- `MaxCost > 0`
- `TotalSlots > 0`
- `AffinityReward >= 0`
- `AffinityPenalty >= 0`
- Demand-Gewichte in `[0, 1]`
- `DemandBonusMax > 0`
- `StepThreshold` in `(0, 1)` wenn `CostFunctionType=step`
- `HighCost > LowCost` wenn `CostFunctionType=step`
- `Alpha > 0` wenn `CostFunctionType=exponential`

Wenn Validierung fehlschlaegt, wird die lokale Konfiguration als Fallback genutzt (siehe [configuration.md](configuration.md)).

---

## 6. Mapping auf Graph-Datenmodell

| Submodel-Feld | Graph-Ziel | Kommentar |
|---|---|---|
| `StorageId` | `Storage.storageId` | Knoten-ID fuer Storage |
| `Name` | `Storage.name` | Optionaler Anzeigename |
| `TotalSlots` | `Storage.totalSlots` | Pflicht im Graph |
| `CostFunctionType` | `Storage.costFunctionType` | Lowercase in AAS, Mapping erforderlich |
| Kostenparameter | `Storage.costParams` oder Einzelproperties | Implementationsabhaengig |
| `SlotId` | `Slot.slotId` | Knoten-ID fuer Slot |
| `PreferredType` | `Slot.preferredType` | Statisch konfiguriert |
| `AffinityReward` | `Slot.affinityReward` | Default 1.0 wenn nicht gesetzt |
| `AffinityPenalty` | `Slot.affinityPenalty` | Default 0.5 wenn nicht gesetzt |

---

## 7. Beispiel (Minimal)

```json
{
  "idShort": "StorageConfiguration",
  "id": "https://smartfactory.de/submodels/a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "semanticId": {
    "type": "ExternalReference",
    "keys": [{
      "type": "GlobalReference",
      "value": "https://smartfactory.de/semantics/submodel/CarrierManagement/StorageConfiguration#1/0"
    }]
  },
  "submodelElements": [
    {
      "idShort": "Storages",
      "modelType": "SubmodelElementCollection",
      "value": [
        {
          "idShort": "Storage_Hauptlager",
          "modelType": "SubmodelElementCollection",
          "value": [
            { "idShort": "StorageId", "modelType": "Property", "valueType": "xs:string", "value": "storage-zentrallager-01" },
            { "idShort": "Name", "modelType": "Property", "valueType": "xs:string", "value": "Hauptlager" },
            { "idShort": "TotalSlots", "modelType": "Property", "valueType": "xs:int", "value": "10" },
            { "idShort": "CostFunctionType", "modelType": "Property", "valueType": "xs:string", "value": "exponential" },
            { "idShort": "BaseCost", "modelType": "Property", "valueType": "xs:double", "value": "0.5" },
            { "idShort": "Alpha", "modelType": "Property", "valueType": "xs:double", "value": "3.0" },
            { "idShort": "MaxCost", "modelType": "Property", "valueType": "xs:double", "value": "10000.0" },
            {
              "idShort": "Slots",
              "modelType": "SubmodelElementCollection",
              "value": [
                {
                  "idShort": "Slot_01",
                  "modelType": "SubmodelElementCollection",
                  "value": [
                    { "idShort": "SlotId", "modelType": "Property", "valueType": "xs:string", "value": "slot-zl-01" },
                    { "idShort": "PreferredType", "modelType": "Property", "valueType": "xs:string", "value": "KLT-400" },
                    { "idShort": "AffinityReward", "modelType": "Property", "valueType": "xs:double", "value": "1.0" },
                    { "idShort": "AffinityPenalty", "modelType": "Property", "valueType": "xs:double", "value": "0.5" }
                  ]
                }
              ]
            }
          ]
        }
      ]
    },
    {
      "idShort": "DemandConfig",
      "modelType": "SubmodelElementCollection",
      "value": [
        { "idShort": "DemandBonusBase", "modelType": "Property", "valueType": "xs:double", "value": "4.0" },
        { "idShort": "DemandBonusMax", "modelType": "Property", "valueType": "xs:double", "value": "5.0" },
        { "idShort": "DemandWeightPotential", "modelType": "Property", "valueType": "xs:double", "value": "0.0" },
        { "idShort": "DemandWeightPlanned", "modelType": "Property", "valueType": "xs:double", "value": "0.3" },
        { "idShort": "DemandWeightImminent", "modelType": "Property", "valueType": "xs:double", "value": "0.7" },
        { "idShort": "DemandWeightExecuting", "modelType": "Property", "valueType": "xs:double", "value": "1.0" },
        { "idShort": "UrgencyEnabled", "modelType": "Property", "valueType": "xs:boolean", "value": "false" }
      ]
    },
    {
      "idShort": "ProjectionConfig",
      "modelType": "SubmodelElementCollection",
      "value": [
        { "idShort": "WeightNoAgent", "modelType": "Property", "valueType": "xs:double", "value": "0.0" },
        { "idShort": "WeightStepOpen", "modelType": "Property", "valueType": "xs:double", "value": "0.3" },
        { "idShort": "WeightStepPlanned", "modelType": "Property", "valueType": "xs:double", "value": "0.7" },
        { "idShort": "WeightStepExecuting", "modelType": "Property", "valueType": "xs:double", "value": "1.0" },
        { "idShort": "MaxStepsAhead", "modelType": "Property", "valueType": "xs:int", "value": "3" }
      ]
    }
  ]
}
```

---

## 8. Versions- und Kompatibilitaet

- `semanticId` versioniert das Submodell.
- Aenderungen, die neue Felder hinzufuegen, muessen abwaertskompatibel sein.
- Entfernen oder Umbenennen von Feldern erfordert eine neue `semanticId`-Version.
