using BaSyx.Models.AdminShell;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// SkillResponse Message - wird vom Execution Agent an den Planning Agent gesendet
/// </summary>
public class SkillResponseMessage
{
    public MessageFrame Frame { get; set; }
    public List<ISubmodelElement> InteractionElements { get; set; }

    public SkillResponseMessage()
    {
        Frame = new MessageFrame();
        InteractionElements = new List<ISubmodelElement>();
    }

    /// <summary>
    /// Erstellt eine SkillResponse Message aus einer Action
    /// </summary>
    public static SkillResponseMessage FromAction(
        Action action, 
        string senderId, 
        string receiverId, 
        string conversationId,
        string actionState,
        string messageType = "consent")
    {
        var message = new SkillResponseMessage
        {
            Frame = new MessageFrame
            {
                Sender = new Participant
                {
                    Identification = new Identification { Id = senderId },
                    Role = new Role()
                },
                Receiver = new Participant
                {
                    Identification = new Identification { Id = receiverId },
                    Role = new Role()
                },
                Type = messageType, // "consent" für Bestätigung, "update" für Statusänderung
                ConversationId = conversationId
            }
        };

        // Erstelle Action SubmodelElementCollection
        var actionCollection = new SubmodelElementCollection(action.IdShort)
        {
            SemanticId = SemanticReferences.StepAction
        };

        // ActionTitle
        actionCollection.Add(new Property<string>("ActionTitle", action.ActionTitle.Value.Value.ToObject<string>())
        {
            SemanticId = SemanticReferences.ActionTitle
        });

        // ActionState (PackML: Starting, Running, Completed, etc.)
        actionCollection.Add(new Property<string>("ActionState", actionState)
        {
            SemanticId = new Reference(new Key(KeyType.GlobalReference, "https://smartfactory.de/semantics/submodel-element/Step/Actions/Action/ActionState"))
        });

        // Status (für Kompatibilität)
        actionCollection.Add(new Property<string>("Status", action.Status.Value.Value.ToObject<string>())
        {
            SemanticId = SemanticReferences.ActionStatus
        });

        // InputParameters
        if (action.InputParameters != null && action.InputParameters.Value.Count > 0)
        {
            var inputParams = new SubmodelElementCollection("InputParameters")
            {
                SemanticId = SemanticReferences.ActionInputParameters
            };

            foreach (var param in action.InputParameters.Value)
            {
                if (param is IProperty prop)
                {
                    inputParams.Add(new Property<string>(prop.IdShort, prop.Value?.Value?.ToObject<string>() ?? ""));
                }
            }

            actionCollection.Add(inputParams);
        }

        // FinalResultData (nur wenn Completed)
        if (actionState == "Completed" && action.FinalResultData != null && action.FinalResultData.Value.Count > 0)
        {
            var resultData = new SubmodelElementCollection("FinalResultData")
            {
                SemanticId = SemanticReferences.ActionFinalResultData
            };

            foreach (var result in action.FinalResultData.Value)
            {
                if (result is IProperty prop)
                {
                    resultData.Add(new Property<string>(prop.IdShort, prop.Value?.Value?.ToObject<string>() ?? ""));
                }
            }

            actionCollection.Add(resultData);
        }

        // Preconditions
        var preconditions = new SubmodelElementCollection("Preconditions")
        {
            SemanticId = SemanticReferences.ActionPreconditions
        };
        actionCollection.Add(preconditions);

        // SkillReference
        if (action.SkillReference != null)
        {
            actionCollection.Add(action.SkillReference);
        }

        // Effects
        var effects = new SubmodelElementCollection("Effects")
        {
            SemanticId = SemanticReferences.ActionEffects
        };
        actionCollection.Add(effects);

        // MachineName
        actionCollection.Add(new Property<string>("MachineName", action.MachineName.Value.Value.ToObject<string>())
        {
            SemanticId = SemanticReferences.ActionMachineName
        });

        message.InteractionElements.Add(actionCollection);
        return message;
    }

    /// <summary>
    /// Extrahiert ActionState aus SkillResponse
    /// </summary>
    public string? GetActionState()
    {
        var actionCollection = InteractionElements
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort?.StartsWith("Action", StringComparison.OrdinalIgnoreCase) == true);

        return actionCollection?.Value
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "ActionState")
            ?.Value?.Value?.ToObject<string>();
    }

    /// <summary>
    /// Extrahiert Action aus SkillResponse
    /// </summary>
    public Action? ExtractAction()
    {
        var actionCollection = InteractionElements
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort?.StartsWith("Action", StringComparison.OrdinalIgnoreCase) == true);

        if (actionCollection == null)
            return null;

        var actionTitle = actionCollection.Value
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "ActionTitle")
            ?.Value?.Value?.ToObject<string>() ?? "Unknown";

        var status = actionCollection.Value
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "Status")
            ?.Value?.Value?.ToObject<string>() ?? "planned";

        var machineName = actionCollection.Value
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "MachineName")
            ?.Value?.Value?.ToObject<string>() ?? "";

        // Parse InputParameters
        var inputParamsCollection = actionCollection.Value
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(c => c.IdShort == "InputParameters");

        var inputParams = new InputParameters();
        if (inputParamsCollection != null)
        {
            foreach (var param in inputParamsCollection.Value.OfType<IProperty>())
            {
                inputParams.SetParameter(param.IdShort, param.Value?.Value?.ToObject<string>() ?? "");
            }
        }

        // Parse FinalResultData
        var finalResultCollection = actionCollection.Value
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(c => c.IdShort == "FinalResultData");

        var finalResultData = new FinalResultData();
        if (finalResultCollection != null)
        {
            foreach (var result in finalResultCollection.Value.OfType<IProperty>())
            {
                finalResultData.SetParameter(result.IdShort, result.Value?.Value?.ToObject<string>() ?? "");
            }
        }

        // Erstelle Action
        var actionStatus = ActionStatusEnumExtensions.FromAasValue(status);
        return new Action(
            actionCollection.IdShort,
            actionTitle,
            actionStatus,
            inputParams,
            finalResultData,
            null,
            machineName
        );
    }
}
