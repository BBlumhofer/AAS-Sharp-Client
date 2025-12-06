using BaSyx.Models.AdminShell;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// LogMessage - Logging für Agenten
/// </summary>
public class LogMessage
{
    public MessageFrame Frame { get; set; }
    public List<ISubmodelElement> InteractionElements { get; set; }

    public LogMessage()
    {
        Frame = new MessageFrame();
        InteractionElements = new List<ISubmodelElement>();
    }

    /// <summary>
    /// Erstellt LogMessage
    /// </summary>
    public static LogMessage Create(
        string senderId,
        string logLevel,
        string message,
        string agentRole = "ResourceHolon")
    {
        var logMessage = new LogMessage
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
                    Identification = new Identification { Id = "broadcast" },
                    Role = new Role()
                },
                Type = "inform",
                ConversationId = Guid.NewGuid().ToString(),
                MessageId = Guid.NewGuid().ToString()
            }
        };

        logMessage.InteractionElements.Add(new Property<string>("LogLevel", logLevel));
        logMessage.InteractionElements.Add(new Property<string>("Message", message));
        logMessage.InteractionElements.Add(new Property<string>("Timestamp", DateTime.UtcNow.ToString("o")));
        logMessage.InteractionElements.Add(new Property<string>("AgentRole", agentRole));

        return logMessage;
    }

    /// <summary>
    /// Extrahiert LogLevel
    /// </summary>
    public string GetLogLevel()
    {
        return InteractionElements
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "LogLevel")
            ?.Value?.Value?.ToObject<string>() ?? "INFO";
    }

    /// <summary>
    /// Extrahiert Message
    /// </summary>
    public string GetMessage()
    {
        return InteractionElements
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "Message")
            ?.Value?.Value?.ToObject<string>() ?? "";
    }
}
