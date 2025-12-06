using BaSyx.Models.AdminShell;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// SkillRequest Message - wird vom Planning Agent an den Execution Agent gesendet
/// </summary>
public class SkillRequestMessage
{
    public MessageFrame Frame { get; set; }
    public List<ISubmodelElement> InteractionElements { get; set; }

    public SkillRequestMessage()
    {
        Frame = new MessageFrame();
        InteractionElements = new List<ISubmodelElement>();
    }

    /// <summary>
    /// Erstellt eine SkillRequest Message aus einer Action
    /// </summary>
    public static SkillRequestMessage FromAction(Action action, string senderId, string receiverId, string conversationId)
    {
        var message = new SkillRequestMessage
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
                Type = "request",
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

        // Status
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

        // Preconditions
        var preconditions = new SubmodelElementCollection("Preconditions")
        {
            SemanticId = SemanticReferences.ActionPreconditions
        };
        actionCollection.Add(preconditions);

        // MachineName
        actionCollection.Add(new Property<string>("MachineName", action.MachineName.Value.Value.ToObject<string>())
        {
            SemanticId = SemanticReferences.ActionMachineName
        });

        message.InteractionElements.Add(actionCollection);
        return message;
    }

    /// <summary>
    /// Extrahiert Action aus SkillRequest
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

        // Erstelle Action
        var actionStatus = ActionStatusEnumExtensions.FromAasValue(status);
        return new Action(
            actionCollection.IdShort,
            actionTitle,
            actionStatus,
            inputParams,
            new FinalResultData(),
            null,
            machineName
        );
    }
}
