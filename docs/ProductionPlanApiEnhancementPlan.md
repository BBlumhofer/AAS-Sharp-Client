# ProductionPlan API Enhancement Plan

## Goals
- Provide high-level manipulation APIs for `ProductionPlan`, `Step`, and `Action` so runtime orchestration code can adapt the information model without accessing raw `SubmodelElementCollection`s.
- Keep the BaSyx backing collections (`SubmodelElements`, `ActionsCollection`, etc.) and the rich object graph (`Steps`, `Actions`) in sync to avoid serialization drift.
- Maintain backward compatibility with existing constructors and JSON parsing helpers.

## Proposed Additions

### ProductionPlan
1. `Step? GetStep(string idShort)` – retrieves a step by identifier.
2. `IReadOnlyList<Step> GetStepsByStatus(StepStatusEnum status)` – filters cached steps.
3. `void UpdateQuantity(int totalPieces)` – updates `QuantityInformation` and its property value.
4. `void SetFinished(bool finished)` – toggles `IsFinished` and allows reopening a plan.
5. `bool RemoveStep(string idShort)` – removes a step from both `Steps` and `SubmodelElements`.
6. `void InsertStep(int index, Step step)` – inserts/reorders while keeping collections aligned.
7. `IEnumerable<Action> GetPendingActions()` – flattens steps to return actions not yet done.

### Step
1. `Action? GetAction(string idShort)` – direct lookup.
2. `IEnumerable<Action> GetActionsByStatus(ActionStatusEnum status)` – filter helper.
3. `bool RemoveAction(string idShort)` – removes from `Actions` list and `ActionsCollection`.
4. `void UpdateScheduling(string startDateTime, string endDateTime, string setupTime, string cycleTime)` – replaces entries in the `Scheduling` container.
5. `void SetInitialState(IDictionary<string, string> values)` and `void SetFinalState(IDictionary<string, string> values)` – rebuilds the respective sub-collections.

### Action
1. `void SetInputParameter(string key, string value)` – upserts entries while preserving semantic IDs.
2. `bool TryGetInputParameter(string key, out string value)` – read helper.
3. `void SetFinalResultValue(string key, object value)` – upserts result data with semantic hints.
4. `void LinkSkillReference(IEnumerable<(object Key, string Value)> referenceChain)` – rebinds the skill reference.

## Implementation Strategy
1. **Tests first**: add a new `ProductionPlanApiTests` fixture in `tests/AasSharpClient.Tests` covering the new behaviors end-to-end.
2. **ProductionPlan updates**: implement lookup/mutation helpers ensuring both the strong lists and BaSyx collections stay aligned.
3. **Step updates**: add manipulation helpers that work with `ActionsCollection`, state collections, and scheduling entries.
4. **Action updates**: add dictionary-style mutators that leverage `SubmodelElementFactory` to keep semantics/value types intact.
5. **Validation**: run `dotnet test tests/AasSharpClient.Tests/AasSharpClient.Tests.csproj` to confirm behavior.
