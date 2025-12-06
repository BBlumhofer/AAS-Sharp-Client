using BaSyx.Models.AdminShell;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// StateMessage - Modulzustand (Ready, Locked, Notifications)
/// </summary>
public class StateMessage
{
    public MessageFrame Frame { get; set; }
    public List<ISubmodelElement> InteractionElements { get; set; }

    public StateMessage()
    {
        Frame = new MessageFrame();
        InteractionElements = new List<ISubmodelElement>();
    }

    /// <summary>
    /// Erstellt StateMessage mit Modulzustand
    /// </summary>
    public static StateMessage Create(
        string senderId,
        string receiverId,
        bool isLocked,
        bool isReady,
        string moduleState,
        bool startupSkillRunning = false)
    {
        var message = new StateMessage
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
                Type = "inform",
                ConversationId = Guid.NewGuid().ToString()
            }
        };

        // Erstelle State SubmodelElementCollection
        var stateCollection = new SubmodelElementCollection("State")
        {
            SemanticId = new Reference(new Key(KeyType.GlobalReference, "https://smartfactory.de/semantics/ModuleState"))
        };

        stateCollection.Add(new Property<bool>("ModuleLocked", isLocked));
        stateCollection.Add(new Property<bool>("StartupSkillRunning", startupSkillRunning));
        stateCollection.Add(new Property<bool>("ModuleReady", isReady));
        stateCollection.Add(new Property<string>("ModuleState", moduleState));

        message.InteractionElements.Add(stateCollection);
        return message;
    }

    /// <summary>
    /// Extrahiert Locked-Status aus StateMessage
    /// </summary>
    public bool GetIsLocked()
    {
        var stateCollection = InteractionElements
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort == "State");

        return stateCollection?.Value
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "ModuleLocked")
            ?.Value?.Value?.ToObject<bool>() ?? false;
    }

    /// <summary>
    /// Extrahiert Ready-Status aus StateMessage
    /// </summary>
    public bool GetIsReady()
    {
        var stateCollection = InteractionElements
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort == "State");

        return stateCollection?.Value
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "ModuleReady")
            ?.Value?.Value?.ToObject<bool>() ?? false;
    }

    /// <summary>
    /// Extrahiert ModuleState aus StateMessage
    /// </summary>
    public string GetModuleState()
    {
        var stateCollection = InteractionElements
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort == "State");

        return stateCollection?.Value
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "ModuleState")
            ?.Value?.Value?.ToObject<string>() ?? "Unknown";
    }
}
