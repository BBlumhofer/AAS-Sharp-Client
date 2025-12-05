using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BaSyx.Models.AdminShell;
using BaSyx.Utils;
using BaSyx.Models.Extensions;

namespace AasSharpClient.Models;

public class Step : SubmodelElementCollection
{
    public Property<string> StepTitle { get; }
    public Property<string> Status { get; }
    public SubmodelElementCollection ActionsCollection { get; }
    public List<Action> Actions { get; }
    public Property<string> Station { get; }
    public SubmodelElementCollection InitialState { get; }
    public SubmodelElementCollection FinalState { get; }
    public SchedulingContainer Scheduling { get; }
    public Property<string> Enterprise { get; }
    public Property<string> Workcentre { get; }
    public StepStatusEnum State { get; private set; }
    private readonly OrderStateMachine _stateMachine;

    public Step(
        string idShort,
        string stepTitle,
        StepStatusEnum status,
        Action? action,
        string station,
        SchedulingContainer scheduling,
        string enterprise,
        string workcentre)
        : base(idShort)
    {
        SemanticId = SemanticReferences.Step;
        Actions = new List<Action>();

        StepTitle = new Property<string>("StepTitle", stepTitle);
        StepTitle.SemanticId = SemanticReferences.StepTitle;
        SubmodelElementFactory.SetValueType(StepTitle, "xs:string");
        Status = new Property<string>("Status", status.ToAasValue());
        Status.SemanticId = SemanticReferences.StepStatus;
        SubmodelElementFactory.SetValueType(Status, "xs:string");
        State = status;
        _stateMachine = new OrderStateMachine(OrderStateMapper.FromStep(status));
        ActionsCollection = new SubmodelElementCollection("Actions");
        ActionsCollection.SemanticId = SemanticReferences.StepActions;
        InitialState = new SubmodelElementCollection("InitialState");
        InitialState.SemanticId = SemanticReferences.StepInitialState;
        FinalState = new SubmodelElementCollection("FinalState");
        FinalState.SemanticId = SemanticReferences.StepFinalState;
        Scheduling = scheduling;
        Station = new Property<string>("Station", station);
        Station.SemanticId = SemanticReferences.StepStation;
        SubmodelElementFactory.SetValueType(Station, "xs:string");
        Enterprise = new Property<string>("Enterprise", enterprise);
        Enterprise.SemanticId = SemanticReferences.StepEnterprise;
        SubmodelElementFactory.SetValueType(Enterprise, "xs:string");
        Workcentre = new Property<string>("Workcentre", workcentre);
        Workcentre.SemanticId = SemanticReferences.StepWorkcentre;
        SubmodelElementFactory.SetValueType(Workcentre, "xs:string");

        Add(StepTitle);
        Add(Status);
        Add(ActionsCollection);
        Add(Station);
        Add(InitialState);
        Add(FinalState);
        Add(Scheduling);
        Add(Enterprise);
        Add(Workcentre);

        if (action != null)
        {
            AddAction(action);
        }
    }

    public Step(
        string idShort,
        string stepTitle,
        StepStatusEnum status,
        IEnumerable<Action> actions,
        string station,
        SchedulingContainer scheduling,
        string enterprise,
        string workcentre)
        : base(idShort)
    {
        SemanticId = SemanticReferences.Step;
        Actions = new List<Action>();

        StepTitle = new Property<string>("StepTitle", stepTitle);
        StepTitle.SemanticId = SemanticReferences.StepTitle;
        SubmodelElementFactory.SetValueType(StepTitle, "xs:string");
        Status = new Property<string>("Status", status.ToAasValue());
        Status.SemanticId = SemanticReferences.StepStatus;
        SubmodelElementFactory.SetValueType(Status, "xs:string");
        State = status;
        _stateMachine = new OrderStateMachine(OrderStateMapper.FromStep(status));
        ActionsCollection = new SubmodelElementCollection("Actions");
        ActionsCollection.SemanticId = SemanticReferences.StepActions;
        InitialState = new SubmodelElementCollection("InitialState");
        InitialState.SemanticId = SemanticReferences.StepInitialState;
        FinalState = new SubmodelElementCollection("FinalState");
        FinalState.SemanticId = SemanticReferences.StepFinalState;
        Scheduling = scheduling;
        Station = new Property<string>("Station", station);
        Station.SemanticId = SemanticReferences.StepStation;
        SubmodelElementFactory.SetValueType(Station, "xs:string");
        Enterprise = new Property<string>("Enterprise", enterprise);
        Enterprise.SemanticId = SemanticReferences.StepEnterprise;
        SubmodelElementFactory.SetValueType(Enterprise, "xs:string");
        Workcentre = new Property<string>("Workcentre", workcentre);
        Workcentre.SemanticId = SemanticReferences.StepWorkcentre;
        SubmodelElementFactory.SetValueType(Workcentre, "xs:string");

        Add(StepTitle);
        Add(Status);
        Add(ActionsCollection);
        Add(Station);
        Add(InitialState);
        Add(FinalState);
        Add(Scheduling);
        Add(Enterprise);
        Add(Workcentre);

        foreach (var action in actions)
        {
            AddAction(action);
        }
    }

    public void AddAction(Action action)
    {
        Actions.Add(action);
        ActionsCollection.Add(action);
        action.AttachToStep(this);
    }

    public bool Reset() => ApplyTransition(OrderTransition.Reset);

    public bool Schedule() => ApplyTransition(OrderTransition.Schedule);

    public bool StartProduction() => ApplyTransition(OrderTransition.StartProduction);

    public bool Suspend() => ApplyTransition(OrderTransition.Suspend);

    public bool Resume() => ApplyTransition(OrderTransition.Resume);

    public bool EndProduction() => ApplyTransition(OrderTransition.EndProduction);

    public bool Error() => ApplyTransition(OrderTransition.Error);

    public bool Abort() => ApplyTransition(OrderTransition.Abort);

    public bool ReturnToCreated() => ApplyTransition(OrderTransition.ReturnToCreated);

    public bool ReturnToPlanned() => ApplyTransition(OrderTransition.ReturnToPlanned);

    public bool ReturnToExecuting() => ApplyTransition(OrderTransition.ReturnToExecuting);

    public bool ReturnToSuspended() => ApplyTransition(OrderTransition.ReturnToSuspended);

    public bool ReturnToCompleted() => ApplyTransition(OrderTransition.ReturnToCompleted);

    public Action? GetAction(string idShort)
    {
        if (string.IsNullOrWhiteSpace(idShort))
        {
            return null;
        }

        return Actions.FirstOrDefault(action => string.Equals(action.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<Action> GetActionsByStatus(ActionStatusEnum status)
    {
        return Actions.Where(action => action.State == status);
    }

    public bool RemoveAction(string idShort)
    {
        var action = GetAction(idShort);
        if (action is null)
        {
            return false;
        }

        Actions.Remove(action);
        ActionsCollection.Remove(action);
        action.DetachFromStep(this);
        return true;
    }

    public void UpdateScheduling(string startDateTime, string endDateTime, string setupTime, string cycleTime)
    {
        SetSchedulingValue("StartDateTime", startDateTime, SemanticReferences.SchedulingStartDateTime);
        SetSchedulingValue("EndDateTime", endDateTime, SemanticReferences.SchedulingEndDateTime);
        SetSchedulingValue("SetupTime", setupTime, SemanticReferences.SchedulingSetupTime);
        SetSchedulingValue("CycleTime", cycleTime, SemanticReferences.SchedulingCycleTime);
    }

    public void SetInitialState(IDictionary<string, string> values)
    {
        ReplaceStateCollection(InitialState, values);
    }

    public void SetFinalState(IDictionary<string, string> values)
    {
        ReplaceStateCollection(FinalState, values);
    }

    public void SetStatus(StepStatusEnum status)
    {
        if (!CanTransitionTo(status))
        {
            return;
        }

        var targetState = OrderStateMapper.FromStep(status);
        var transition = OrderStateMachine.FindTransition(_stateMachine.State, targetState);
        if (transition.HasValue)
        {
            ApplyTransition(transition.Value);
            return;
        }

        _stateMachine.ForceSet(targetState);
        ApplyState(status);
    }

    internal void OnActionStatusChanged(Action action, ActionStatusEnum newStatus)
    {
        var targetState = OrderStateMapper.FromAction(newStatus);
        var transition = OrderStateMachine.FindTransition(_stateMachine.State, targetState);
        if (transition.HasValue)
        {
            ApplyTransition(transition.Value);
        }
        else if (OrderStateMapper.ToStep(targetState) == State)
        {
            ApplyState(State);
        }
    }

    internal static Step FromJson(JsonElement element)
    {
        var idShort = element.GetProperty("idShort").GetString() ?? "Step";
        string stepTitle = string.Empty;
        string status = "open";
        string station = string.Empty;
        string enterprise = string.Empty;
        string workcentre = string.Empty;
        SchedulingContainer scheduling = new();
        var actions = new List<Action>();

        if (element.TryGetProperty("value", out var values) && values.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in values.EnumerateArray())
            {
                if (!entry.TryGetProperty("idShort", out var entryIdShort)) continue;
                var entryId = entryIdShort.GetString();
                switch (entryId)
                {
                    case "StepTitle":
                        stepTitle = entry.GetProperty("value").GetString() ?? string.Empty;
                        break;
                    case "Status":
                        status = entry.GetProperty("value").GetString() ?? "open";
                        break;
                    case "Actions":
                        actions.AddRange(ParseActions(entry));
                        break;
                    case "Station":
                        station = entry.GetProperty("value").GetString() ?? string.Empty;
                        break;
                    case "Scheduling":
                        scheduling = ParseScheduling(entry);
                        break;
                    case "Enterprise":
                        enterprise = entry.GetProperty("value").GetString() ?? string.Empty;
                        break;
                    case "Workcentre":
                        workcentre = entry.GetProperty("value").GetString() ?? string.Empty;
                        break;
                }
            }
        }

        var firstAction = actions.FirstOrDefault();
        var step = new Step(idShort, stepTitle, StatusExtensions.FromAasValue(status), firstAction, station, scheduling, enterprise, workcentre);
        foreach (var action in actions.Skip(1))
        {
            step.AddAction(action);
        }

        return step;
    }

        private void SetSchedulingValue(string idShort, string value, Reference semanticId)
        {
            var property = Scheduling.OfType<Property>().FirstOrDefault(p => p.IdShort == idShort);
            if (property is null)
            {
                property = SubmodelElementFactory.CreateStringProperty(idShort, value, semanticId);
                Scheduling.Add(property);
                return;
            }

            property.Value = new PropertyValue<string>(value ?? string.Empty);
        }

        private static void ReplaceStateCollection(SubmodelElementCollection target, IDictionary<string, string> values)
        {
            target.Clear();
            if (values == null)
            {
                return;
            }

            foreach (var (key, value) in values)
            {
                target.Add(SubmodelElementFactory.CreateStringProperty(key, value));
            }
        }

        private bool CanTransitionTo(StepStatusEnum status)
        {
            var targetState = OrderStateMapper.FromStep(status);
            if (targetState == _stateMachine.State)
            {
                return true;
            }

            var transition = OrderStateMachine.FindTransition(_stateMachine.State, targetState);
            if (!transition.HasValue)
            {
                return false;
            }

            if (transition == OrderTransition.EndProduction && Actions.Any(action => action.State != ActionStatusEnum.DONE))
            {
                return false;
            }

            return _stateMachine.CanApply(transition.Value);
        }

        private bool ApplyTransition(OrderTransition transition)
        {
            if (transition == OrderTransition.EndProduction && Actions.Any(action => action.State != ActionStatusEnum.DONE))
            {
                return false;
            }

            if (!_stateMachine.TryApply(transition))
            {
                return false;
            }

            var mapped = OrderStateMapper.ToStep(_stateMachine.State);
            ApplyState(mapped);
            return true;
        }

        private void ApplyState(StepStatusEnum status)
        {
            State = status;
            Status.Value = new PropertyValue<string>(status.ToAasValue());
        }

    private static IEnumerable<Action> ParseActions(JsonElement actionsElement)
    {
        if (!actionsElement.TryGetProperty("value", out var actionArray) || actionArray.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var actionElement in actionArray.EnumerateArray())
        {
            yield return ParseAction(actionElement);
        }
    }

    private static Action ParseAction(JsonElement actionElement)
    {
        var idShort = actionElement.GetProperty("idShort").GetString() ?? "Action";
        string title = string.Empty;
        string status = "open";
        string machineName = string.Empty;
        var inputParameters = new Dictionary<string, string>();
        var finalResultData = new Dictionary<string, object>();
        var skillReferenceChain = new List<(object Key, string Value)>();

        if (actionElement.TryGetProperty("value", out var valueArray) && valueArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in valueArray.EnumerateArray())
            {
                if (!entry.TryGetProperty("idShort", out var entryId)) continue;
                var id = entryId.GetString();
                switch (id)
                {
                    case "ActionTitle":
                        title = entry.GetProperty("value").GetString() ?? string.Empty;
                        break;
                    case "Status":
                        status = entry.GetProperty("value").GetString() ?? "open";
                        break;
                    case "InputParameters":
                        ParseStringDictionary(entry, inputParameters);
                        break;
                    case "FinalResultData":
                        ParseObjectDictionary(entry, finalResultData);
                        break;
                    case "MachineName":
                        machineName = entry.GetProperty("value").GetString() ?? string.Empty;
                        break;
                    case "SkillReference":
                        skillReferenceChain = ParseReference(entry);
                        break;
                }
            }
        }

        var action = new Action(
            idShort,
            title,
            StatusExtensions.FromActionValue(status),
            new InputParameters(inputParameters),
            new FinalResultData(finalResultData),
            new SkillReference(skillReferenceChain),
            machineName);

        return action;
    }

    private static SchedulingContainer ParseScheduling(JsonElement schedulingElement)
    {
        var values = new Dictionary<string, string>
        {
            { "StartDateTime", string.Empty },
            { "EndDateTime", string.Empty },
            { "SetupTime", string.Empty },
            { "CycleTime", string.Empty }
        };

        if (schedulingElement.TryGetProperty("value", out var array) && array.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in array.EnumerateArray())
            {
                if (!item.TryGetProperty("idShort", out var idProp)) continue;
                var id = idProp.GetString();
                if (id != null && values.ContainsKey(id))
                {
                    values[id] = item.GetProperty("value").GetString() ?? string.Empty;
                }
            }
        }

        return new SchedulingContainer(
            values["StartDateTime"],
            values["EndDateTime"],
            values["SetupTime"],
            values["CycleTime"]);
    }

    private static List<(object Key, string Value)> ParseReference(JsonElement referenceElement)
    {
        var result = new List<(object Key, string Value)>();
        if (referenceElement.TryGetProperty("value", out var valueElement) &&
            valueElement.TryGetProperty("keys", out var keysElement) &&
            keysElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var key in keysElement.EnumerateArray())
            {
                var type = key.GetProperty("type").GetString();
                var value = key.GetProperty("value").GetString() ?? string.Empty;
                var mapped = MapKey(type);
                result.Add((mapped, value));
            }
        }

        return result;
    }

    private static object MapKey(string? type)
    {
        return type switch
        {
            "Submodel" => ModelReferenceEnum.Submodel,
            "SubmodelElementCollection" => ModelReferenceEnum.SubmodelElementCollection,
            "Property" => ModelReferenceEnum.Property,
            _ => ModelReferenceEnum.Submodel
        };
    }

    private static void ParseStringDictionary(JsonElement element, IDictionary<string, string> target)
    {
        if (element.TryGetProperty("value", out var array) && array.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in array.EnumerateArray())
            {
                if (!entry.TryGetProperty("idShort", out var idShort)) continue;
                target[idShort.GetString() ?? string.Empty] = entry.GetProperty("value").GetString() ?? string.Empty;
            }
        }
    }

    private static void ParseObjectDictionary(JsonElement element, IDictionary<string, object> target)
    {
        if (element.TryGetProperty("value", out var array) && array.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in array.EnumerateArray())
            {
                if (!entry.TryGetProperty("idShort", out var idShort)) continue;
                var id = idShort.GetString() ?? string.Empty;
                if (!entry.TryGetProperty("value", out var value)) continue;
                target[id] = value.ValueKind switch
                {
                    JsonValueKind.Number when value.TryGetInt64(out var longValue) => longValue,
                    JsonValueKind.Number when value.TryGetDouble(out var doubleValue) => doubleValue,
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => value.GetString() ?? string.Empty
                };
            }
        }
    }

}
