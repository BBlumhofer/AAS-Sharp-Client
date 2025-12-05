**MachineScheduleSubmodel API**

Kurzbeschreibung:
- Die Klasse `MachineScheduleSubmodel` repräsentiert das MachineSchedule‑Submodel und bietet lokale Management‑Methoden für Scheduling‑Einträge sowie Remote‑Synchronisation über den BaSyx HTTP Client.

Klassen und Methoden:

- `MachineScheduleSubmodel` (Klasse)
  - Konstruktor: `MachineScheduleSubmodel(string? submodelIdentifier = null)` — erstellt ein Submodel mit `LastTimeUpdated`, `HasOpenTasks` und `Schedule`.
  - `static MachineScheduleSubmodel CreateWithIdentifier(string submodelIdentifier)` — Factory für vorgegebene Identifier.
  - `Task<string> ToJsonAsync(CancellationToken cancellationToken = default)` — serialisiert das Submodel nach JSON.
  - `void Apply(MachineScheduleData data)` — wendet Daten (typ `MachineScheduleData`) auf das Submodel an; aktualisiert `LastTimeUpdated`, `HasOpenTasks`, `Schedule` und den internen Managed‑Cache.
  - `IReadOnlyList<SchedulingContainer> GetSchedules()` — liefert die intern verwalteten `SchedulingContainer`‑Einträge (read‑only).
  - `void AddOrUpdateSchedule(SchedulingContainer container)` — fügt ein Scheduling hinzu oder ersetzt ein bestehendes (Matching über `ReferredStep`).
  - `bool RemoveSchedule(SchedulingContainer container)` — entfernt ein Scheduling (falls vorhanden) und aktualisiert Status/LastTimeUpdated.
  - `Task SyncToRemoteAsync(string remoteEndpoint, CancellationToken token = default)` — synchronisiert (ersetzt) das Submodel auf einem entfernten BaSyx Submodel HTTP Endpoint.
    - Implementation: nutzt `BaSyx.Clients.AdminShell.Http.SubmodelHttpClient` und führt `ReplaceSubmodelAsync(this)` durch.
  - `Task SyncFromRemoteAsync(string remoteEndpoint, CancellationToken token = default)` — lädt das Submodel vom entfernten Endpoint und wendet es lokal via `Apply(...)` an.
    - Implementation: ruft `RetrieveSubmodelAsync()` auf, extrahiert `LastTimeUpdated`, `HasOpenTasks` und `Schedule` und ruft `Apply(...)`.

- `SchedulingContainer` (Klasse)
  - Verwaltet einzelne Scheduling‑Einträge mit Properties `StartDateTime`, `EndDateTime`, `SetupTime`, `CycleTime`.
  - Subcollections: `InitialScheduling`, `ActualScheduling`.
  - Konstruktoren: `SchedulingContainer()`, `SchedulingContainer(Step step)`, `SchedulingContainer(ProductionPlan plan, Step step)`.
  - Hilfsmethoden: `GetStartDateTime()`, `GetEndDateTime()`, `SetStartDateTime(...)`, `NormalizeToAbsoluteDates(...)`, `AllowedToStartStep(...)`, uvm.

- `MachineScheduleData` (record)
  - `string SubmodelIdentifier`, `string? LastTimeUpdated`, `bool HasOpenTasks`, `IReadOnlyList<ISubmodelElement>? ScheduleEntries`
  - Verwendet von `Apply(...)`.

Hinweise und Verhalten:
- Remote‑Sync verwendet ausschließlich den vorhandenen BaSyx `SubmodelHttpClient` (BaSyx HTTP Client) als einzige Außen­schnittstelle.
- Fehler beim Remote‑Aufruf werden als Exception zurückgegeben (z. B. wenn das entfernte Submodel nicht erreichbar ist oder ein HTTP‑Fehler zurückkommt).
- Beim Schreiben auf den Remote‑Endpoint wird das komplette Submodel ersetzt (HTTP PUT/Replace), beim Lesen wird das entfernte Submodel vollständig geladen und lokal angewendet.

Konfigurationshinweis:
- `remoteEndpoint` erwartet die vollständige URL zur Submodel‑Ressource (z. B. `http://host:4845/api/registry/submodels/{submodelId}` oder die Basis‑URL des Submodel endpoints — `SubmodelHttpClient` entfernt/erwartet Pfadsegmente intern).

Weitere Verbesserungen (optional):
- Authentifizierung/Timeout/Retry via `HttpMessageHandler` oder DI/`HttpClientFactory` integrieren.
- Granulares Patchen statt vollständigem Replace, wenn nur `Schedule` aktualisiert werden soll.
