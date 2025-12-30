# Integrated Capability + Skill Handshake Concept

This concept integrates capability semantics (planning & matchmaking) with skill-level handshake behavior (execution & safety). It avoids duplication by mapping each handover capability to a specific skill role and a single handshake sequence.

## Integrated Model Overview

| Aspect | Capability Model (Planning/Matchmaking) | Skill Model (Execution/Handshake) | Integration Rule |
| --- | --- | --- | --- |
| Handover capability type | `DockStoreHandoverCapability`, `DockRetrieveHandoverCapability` | External skill coordinates handover | Each composite capability maps to an **external skill** that drives handshake + monitors partner |
| Transfer role | `transferRole` = `initiator` / `acceptor` | External/Internal skill assignment | `initiator` => module runs **external skill** and expects partner internal; `acceptor` => module runs **internal skill** and expects partner external |
| Dock role | `dockRole` = `dockClient` / `Host` | Docking skill state machine | Dock role constrains which docking skill transitions are valid (client initiates, host responds) |
| Topology | `topologyType` = `Dock` / `Neighbor` | Docking/Neighbor coupling skills | Topology gates which coupling skills are used; same topology required for match |
| Partner compatibility | `dockClass` / `neighborClass`, `supportedLoadCarrierClasses` | Skill handshake parameters | Partner match requires same class + intersecting load carrier classes before handshake starts |
| Preconditions | `TransitionConstraints` (Pre) | `ready` -> `running` gating | Pre-constraints must pass before external skill enters `running` |
| Postconditions | `TransitionConstraints` (Post) | `completed` -> `halting` gate | Post-constraints evaluate after internal completion before external halting |
| Safety abort | (implicit in constraints) | `safetySignal` / unexpected `halted` | Any safety event forces both skills to `halting` -> `halted` and ends coupling |
| Capacity/availability | `PropertyConstraints` | Skill readiness checks | Property constraints must be true before handshake is allowed |

## Unified Handshake Sequence (Docking Example)

| Phase | External Skill (Coordinator) | Internal Skill (Handler) | Trigger/Condition | Capability Alignment |
| --- | --- | --- | --- | --- |
| 1. Ready | `ready` | `ready` | Preconditions satisfied | `TransitionConstraints` Pre (e.g., `DockPrecondition`) |
| 2. Start | `running` | `ready` | External skill started | Capability match verified (role/topology/class) |
| 3. Engage | `running` | `running` | Internal skill activated | `transferRole` determines who is external vs internal |
| 4. Complete | `running` | `completed` | Internal finishes | Property constraints still valid |
| 5. Halt | `halting` | `completed` | External stops after internal success | `TransitionConstraints` Post (e.g., `UndockPostcondition`) |
| 6. Halted | `halted` | `halted` | Normal completion | Coupling ends cleanly |
| Abort | `halting` -> `halted` | `halting` -> `halted` | Safety event or unexpected halt | Immediate decoupling on both sides |

## Matchmaking Rules (Integrated)

| Rule | Capability Requirement | Skill Requirement | Effect |
| --- | --- | --- | --- |
| Role complement | `initiator` vs `acceptor` | External vs Internal pairing | Reject pairing if roles are equal |
| Topology match | same `topologyType` | correct coupling skill | Prevents Dock/Neighbor mix |
| Class match | same `dockClass` or `neighborClass` | compatible interface config | Ensures physical compatibility |
| Load carrier overlap | intersecting `supportedLoadCarrierClasses` | handler can accept carrier | Prevents incompatible transfer |
| Pre-conditions | `TransitionConstraints` Pre | external `ready` -> `running` | Blocks handshake until satisfied |
| Post-conditions | `TransitionConstraints` Post | external halting after internal complete | Ensures safe undock |

## Minimal Property Set for Planning

| Property | Purpose |
| --- | --- |
| `transferRole` | initiator/acceptor for skill assignment |
| `topologyType` | Dock/Neighbor selection |
| `dockRole` | dockClient/Host interaction constraint |
| `dockClass` / `neighborClass` | partner class matching |
| `supportedLoadCarrierClasses` | load carrier compatibility |
| `direction` | Store/Retrieve specialization |
| `capacity` | docking capacity check |

## Notes

- External skill is responsible for coordination and safety supervision.
- Internal skill represents the active handling component (store/retrieve).
- Any safety event forces synchronized halt on both sides.
- The model supports synchronous and asynchronous transfer while keeping constraints centralized.
