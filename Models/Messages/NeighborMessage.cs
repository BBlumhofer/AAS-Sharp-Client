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

        // Erstelle Neighbors SubmodelElementCollection
        var neighborsCollection = new SubmodelElementCollection("Neighbors");

        int index = 0;
        foreach (var neighbor in neighbors)
        {
            var property = new Property<string>($"Neighbor_{index++}");
            property.Value = new PropertyValue<string>(neighbor);
            neighborsCollection.Add(property);
        }

        message.InteractionElements.Add(neighborsCollection);
        return message;
    }

    /// <summary>
    /// Extrahiert Nachbarn-Liste
    /// </summary>
    public List<string> GetNeighbors()
    {
        var neighborsCollection = InteractionElements
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort == "Neighbors");

        if (neighborsCollection == null)
            return new List<string>();

        var result = new List<string>();
        // SubmodelElementCollection implementiert IElementContainer<ISubmodelElement>
        foreach (var element in neighborsCollection)
        {
            if (element is IProperty prop && prop.Value != null)
            {
                var value = prop.Value.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    result.Add(value);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Prüft, ob ein bestimmter Nachbar verfügbar ist
    /// </summary>
    public bool HasNeighbor(string neighborId)
    {
        return GetNeighbors().Contains(neighborId, StringComparer.OrdinalIgnoreCase);
    }
}
