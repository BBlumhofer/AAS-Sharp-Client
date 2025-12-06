using System;
using System.Collections.Generic;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models;

public class Action : SubmodelElementCollection
{
    public Property<string> ActionTitle { get; }
    public Property<string> Status { get; }
    public InputParameters InputParameters { get; }
    public FinalResultData FinalResultData { get; }
    public SubmodelElementCollection Preconditions { get; }
    public SkillReference SkillReference { get; }
    public SubmodelElementCollection Effects { get; }
    public Property<string> MachineName { get; }
    public ActionStatusEnum State { get; private set; }
    internal Step? ParentStep { get; private set; }
    private readonly OrderStateMachine _stateMachine;

    public Action(
        string idShort,
        string actionTitle,
        ActionStatusEnum status,
        InputParameters? inputParameters,
        FinalResultData? finalResultData,
        SkillReference? skillReference,
        string machineName)
        : base(NormalizeIdShort(idShort))
    {
        SemanticId = SemanticReferences.StepAction;

        ActionTitle = SubmodelElementFactory.CreateStringProperty("ActionTitle", actionTitle, SemanticReferences.ActionTitle);
        Status = SubmodelElementFactory.CreateStringProperty("Status", string.Empty, SemanticReferences.ActionStatus);
        InputParameters = inputParameters ?? new InputParameters();
        FinalResultData = finalResultData ?? new FinalResultData();
        Preconditions = CreateEmptyCollection("Preconditions", SemanticReferences.ActionPreconditions);
        SkillReference = skillReference ?? new SkillReference(Array.Empty<(object Key, string Value)>());
        Effects = CreateEmptyCollection("Effects", SemanticReferences.ActionEffects);
        MachineName = SubmodelElementFactory.CreateStringProperty("MachineName", machineName?.Trim() ?? string.Empty, SemanticReferences.ActionMachineName);

        _stateMachine = new OrderStateMachine(OrderStateMapper.FromAction(status));
        SetStatus(status);

        Add(ActionTitle);
        Add(Status);
        Add(InputParameters);
        Add(FinalResultData);
        Add(Preconditions);
        Add(SkillReference);
        Add(Effects);
        Add(MachineName);
    }

    public void SetStatus(ActionStatusEnum status)
    {
        _stateMachine.ForceSet(OrderStateMapper.FromAction(status));
        ApplyStatus(status);
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

    internal void AttachToStep(Step step)
    {
        ParentStep = step;
    }

    internal void DetachFromStep(Step step)
    {
        if (ParentStep == step)
        {
            ParentStep = null;
        }
    }

    public void SetInputParameter(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key must not be null or whitespace.", nameof(key));
        }

        InputParameters.SetParameter(key, value);
    }

    public bool TryGetInputParameter(string key, out string value)
    {
        if (InputParameters.TryGetParameterValue(key, out string? storedValue) && storedValue != null)
        {
            value = storedValue;
            return true;
        }

        value = string.Empty;
        return false;
    }

    public void SetFinalResultValue(string key, object? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key must not be null or whitespace.", nameof(key));
        }

        FinalResultData.SetParameter(key, value);
    }
    
    /// <summary>
    /// Gibt alle FinalResultData als Dictionary zurück (für SkillResponse)
    /// </summary>
    public IDictionary<string, object> GetFinalResultData()
    {
        var result = new Dictionary<string, object>();
        foreach (var param in FinalResultData.Parameters)
        {
            if (param.Value.Value?.Value != null)
            {
                result[param.Key] = param.Value.Value.Value;
            }
        }
        return result;
    }
    
    /// <summary>
    /// Gibt alle InputParameters als Dictionary zurück
    /// </summary>
    public IDictionary<string, string> GetInputParameters()
    {
        var result = new Dictionary<string, string>();
        foreach (var param in InputParameters.Parameters)
        {
            if (InputParameters.TryGetParameterValue(param.Key, out string? value) && value != null)
            {
                result[param.Key] = value;
            }
        }
        return result;
    }
    
    /// <summary>
    /// Gibt den ActionTitle zurück
    /// </summary>
    public string GetActionTitle()
    {
        return ActionTitle.Value?.Value?.ToString() ?? string.Empty;
    }
    
    /// <summary>
    /// Gibt den MachineName zurück
    /// </summary>
    public string GetMachineName()
    {
        return MachineName.Value?.Value?.ToString() ?? string.Empty;
    }

    public void LinkSkillReference(IEnumerable<(object Key, string Value)> referenceChain)
    {
        SkillReference.UpdateReferenceChain(referenceChain ?? Array.Empty<(object Key, string Value)>());
    }

    private static SubmodelElementCollection CreateEmptyCollection(string idShort, Reference semanticId)
    {
        return new SubmodelElementCollection(idShort)
        {
            SemanticId = semanticId
        };
    }

    private static string NormalizeIdShort(string? idShort)
    {
        const string prefix = "Action";
        if (string.IsNullOrWhiteSpace(idShort))
        {
            return $"{prefix}001";
        }

        var trimmed = idShort.Trim();
        if (!trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        var suffix = trimmed[prefix.Length..];
        if (suffix.Length == 0)
        {
            return $"{prefix}001";
        }

        var isNumeric = suffix.All(char.IsDigit);
        if (isNumeric && suffix.Length >= 3)
        {
            return $"{prefix}{suffix}";
        }

        if (isNumeric && int.TryParse(suffix, out var number))
        {
            return $"{prefix}{number:000}";
        }

        return trimmed;
    }

    private bool ApplyTransition(OrderTransition transition)
    {
        if (!_stateMachine.TryApply(transition))
        {
            return false;
        }

        var status = OrderStateMapper.ToAction(_stateMachine.State);
        ApplyStatus(status);
        return true;
    }

    private void ApplyStatus(ActionStatusEnum status)
    {
        State = status;
        Status.Value = new PropertyValue<string>(status.ToAasValue());
        ParentStep?.OnActionStatusChanged(this, status);
    }
}
