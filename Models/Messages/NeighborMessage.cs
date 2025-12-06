using BaSyx.Models.AdminShell;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// NeighborMessage - Nachbarn eines Moduls
/// </summary>
public class NeighborMessage
{
    public MessageFrame Frame { get; set; }
    public List<ISubmodelElement> InteractionElements { get; set; }

    public NeighborMessage()
    {
        Frame = new MessageFrame();
        InteractionElements = new List<ISubmodelElement>();
    }

    /// <summary>
    /// Erstellt NeighborMessage mit Liste von Nachbarn
    /// </summary>
    public static NeighborMessage Create(string senderId, List<string> neighbors)
    {
        var message = new NeighborMessage
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
                    Identification = new Identification { Id = "" },
                    Role = new Role()
                },
                Type = "consent",
                ConversationId = Guid.NewGuid().ToString()
            }
        };

        // Erstelle Neighbors SubmodelElementList
        var neighborsList = new SubmodelElementList("Neighbors")
        {
            TypeValueListElement = "Property"
        };

        foreach (var neighbor in neighbors)
        {
            neighborsList.Value.Add(new Property<string>("", neighbor));
        }

        message.InteractionElements.Add(neighborsList);
        return message;
    }

    /// <summary>
    /// Extrahiert Nachbarn-Liste
    /// </summary>
    public List<string> GetNeighbors()
    {
        var neighborsList = InteractionElements
            .OfType<SubmodelElementList>()
            .FirstOrDefault(e => e.IdShort == "Neighbors");

        if (neighborsList == null)
            return new List<string>();

        return neighborsList.Value
            .OfType<IProperty>()
            .Select(p => p.Value?.Value?.ToObject<string>() ?? "")
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();
    }

    /// <summary>
    /// Prüft, ob ein bestimmter Nachbar verfügbar ist
    /// </summary>
    public bool HasNeighbor(string neighborId)
    {
        return GetNeighbors().Contains(neighborId, StringComparer.OrdinalIgnoreCase);
    }
}
