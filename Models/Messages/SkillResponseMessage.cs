using System;
using System.Collections.Generic;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// SkillResponseMessage - fasst ActionStatus, InputParameter und FinalResultData zusammen.
/// </summary>
public class SkillResponseMessage : SubmodelElementCollection
{
    public SkillResponseMessage(
        string actionState,
        string status,
        string? actionTitle = null,
        string? machineName = null,
        string? step = null,
        IDictionary<string, string>? inputParameters = null,
        IDictionary<string, object?>? finalResultData = null,
        string? logMessage = null,
        long? successfulExecutionsCount = null)
        : base("ActionResponse")
    {
        Add(SubmodelElementFactory.CreateStringProperty("ActionState", actionState));
        Add(SubmodelElementFactory.CreateStringProperty("Status", status));

        if (!string.IsNullOrWhiteSpace(actionTitle))
        {
            Add(SubmodelElementFactory.CreateStringProperty("ActionTitle", actionTitle));
        }

        if (!string.IsNullOrWhiteSpace(machineName))
        {
            Add(SubmodelElementFactory.CreateStringProperty("MachineName", machineName));
        }

        if (!string.IsNullOrWhiteSpace(step))
        {
            Add(SubmodelElementFactory.CreateStringProperty("Step", step));
        }

        if (!string.IsNullOrWhiteSpace(logMessage))
        {
            Add(SubmodelElementFactory.CreateStringProperty("LogMessage", logMessage));
        }

        if (successfulExecutionsCount.HasValue)
        {
            Add(SubmodelElementFactory.CreateProperty("SuccessfulExecutionsCount", successfulExecutionsCount.Value));
        }

        if (inputParameters is { Count: > 0 })
        {
            var inputCollection = new SubmodelElementCollection("InputParameters");
            foreach (var kvp in inputParameters)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                    continue;

                inputCollection.Add(SubmodelElementFactory.CreateStringProperty(kvp.Key, kvp.Value));
            }

            Add(inputCollection);
        }

        if (finalResultData is { Count: > 0 })
        {
            var resultCollection = new SubmodelElementCollection("FinalResultData");
            foreach (var kvp in finalResultData)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                    continue;

                resultCollection.Add(SubmodelElementFactory.CreateProperty(kvp.Key, kvp.Value));
            }

            Add(resultCollection);
        }
    }

    public static List<ISubmodelElement> CreateInteractionElements(
        string actionState,
        string status,
        string? actionTitle = null,
        string? machineName = null,
        string? step = null,
        IDictionary<string, string>? inputParameters = null,
        IDictionary<string, object?>? finalResultData = null,
        string? logMessage = null,
        long? successfulExecutionsCount = null)
    {
        return new List<ISubmodelElement>
        {
            new SkillResponseMessage(
                actionState,
                status,
                actionTitle,
                machineName,
                step,
                inputParameters,
                finalResultData,
                logMessage,
                successfulExecutionsCount)
        };
    }
}
