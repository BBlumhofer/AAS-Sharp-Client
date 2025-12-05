# TODO

## ProductionPlan parity
- [x] Port the Java scheduling utilities (`referable/Scheduling/Scheduling.java`) into `SchedulingContainer` so steps and scheduling containers can normalize timestamps, compute cycle times, and gate execution based on `StartDateTime`.
- Introduce a key/value container abstraction for action parameters similar to `referable/KeyValueContainer/KeyValueContainer.java` so `Action` input/final data can store complex values (lists, nested objects) while keeping the underlying submodel elements in sync.
- Add ProductionPlan enrichment helpers (mirroring `ProductionPlan.fill_steps`/`fill_action` in Java) to resolve action parameters from referenced submodels and to normalize scheduling data when loading a plan.

## MachineSchedule parity
- Extend `MachineScheduleSubmodel` to manage a collection of strongly typed scheduling containers (including add/update/remove and `HasOpenTasks` auto-toggle) instead of exposing a passive `SubmodelElementList`.
- Add remote sync helpers (`upload_data_bundle`, `set_open_tasks_remote`, etc.) and make `LastTimeUpdated` refresh automatically when containers change, matching `MachineSchedule.java`.

## SchedulingContainer parity
- Enrich `SchedulingContainer` with `ReferenceElement` links to the referred step plus dedicated initial/actual `Scheduling` sub-collections, matching the Java container constructors.
- Provide constructors/factory methods that accept a `ProductionPlan` + `Step`, automatically copying the step’s scheduling data into the container’s initial schedule.

## ProductIdentification enhancements
- [x] Add getter/setter APIs so the submodel can update identifier, order, and branding fields in place rather than only recreating the full element list.
- [x] Implement cargo hazard class helpers (presence checks, equality via reference comparison, reference resolution) analogous to `ProductIdentification.java`.

## CapabilityDescription improvements
- Add read/query helpers (e.g., `GetCapabilities`, `GetCapabilityNames`, `FindCapabilityContainer`) comparable to `Capabilities.java`, enabling inspection of existing capability sets and constraints.

## Skills submodel parsing
- Teach `SkillsSubmodel` to load and expose existing SkillSet entries, endpoint metadata, and skill metadata (like Java’s `Skills` wrapper) instead of only generating new structures from DTOs.

## Documentation
- Produce extensive developer/user documentation covering each submodel implementation, the Java-to-C# parity plan, lifecycle helpers, and usage patterns so consumers understand the new APIs and maintenance expectations.
