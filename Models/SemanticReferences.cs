using System;
using System.Collections.Generic;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models;

internal static class SemanticReferences
{
    private static Reference CreateModelReference(params (KeyType Type, string Value)[] keys)
    {
        var keyList = new List<IKey>();
        foreach (var (type, value) in keys)
        {
            keyList.Add(new Key(type, value));
        }

        return new Reference(keyList)
        {
            Type = ReferenceType.ModelReference
        };
    }

    private static Reference CreateExternalReference(params string[] globalReferences)
    {
        var keyList = new List<IKey>();
        foreach (var reference in globalReferences)
        {
            keyList.Add(new Key(KeyType.GlobalReference, reference));
        }

        return new Reference(keyList)
        {
            Type = ReferenceType.ExternalReference
        };
    }

    private static Reference CreateEmptyExternalReference() => new Reference(Array.Empty<IKey>())
    {
        Type = ReferenceType.ExternalReference
    };

    public static Reference ProductionPlanSemanticId { get; } = CreateModelReference(
        (KeyType.Submodel, "ProductionPlan"),
        (KeyType.Submodel, "ProductionSchedule"),
        (KeyType.Submodel, "https://smartfactory.de/semantics/submodel/ProductionPlan#1/0"));

    public static Reference QuantityInformation { get; } = CreateEmptyExternalReference();
    public static Reference TotalNumberOfPieces { get; } = CreateEmptyExternalReference();
    public static Reference IsFinished { get; } = CreateEmptyExternalReference();

    public static Reference Step { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step");
    public static Reference StepTitle { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/StepTitle");
    public static Reference StepStatus { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Status");
    public static Reference StepActions { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Actions");
    public static Reference StepAction { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Actions/Action");
    public static Reference StepStation { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Station");
    public static Reference StepInitialState { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/InitialState");
    public static Reference StepFinalState { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/FinalState");
    public static Reference StepScheduling { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Scheduling");
    public static Reference StepEnterprise { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Enterprise");
    public static Reference StepWorkcentre { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Workcentre");

    public static Reference SchedulingStartDateTime { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Scheduling/StartDateTime");
    public static Reference SchedulingEndDateTime { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Scheduling/EndTime");
    public static Reference SchedulingSetupTime { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Scheduling/SetupTime");
    public static Reference SchedulingCycleTime { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Scheduling/CycleTime");

    public static Reference ActionTitle { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Actions/Action/ActionTitle");
    public static Reference ActionStatus { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Actions/Action/Status");
    public static Reference ActionInputParameters { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Actions/Action/InputParameters");
    public static Reference ActionFinalResultData { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Actions/Action/FinalResultData");
    public static Reference ActionFinalResultDataEndTime { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Actions/Action/FinalResultData/EndTime");
    public static Reference ActionFinalResultDataStartTime { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Actions/Action/FinalResultData/StartTime");
    public static Reference ActionPreconditions { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Actions/Action/Preconditions");
    public static Reference ActionSkillReference { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Actions/Action/SkillReference");
    public static Reference ActionEffects { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Actions/Action/Effecs");
    public static Reference ActionMachineName { get; } = CreateExternalReference("https://smartfactory.de/semantics/submodel-element/Step/Actions/Action/MachineName");

    public static Reference EmptyExternal { get; } = CreateEmptyExternalReference();
}
