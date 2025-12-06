using BaSyx.Models.AdminShell;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// InventoryMessage - Lagerbestand eines Moduls
/// </summary>
public class InventoryMessage
{
    public MessageFrame Frame { get; set; }
    public List<StorageUnit> StorageUnits { get; set; }

    public InventoryMessage()
    {
        Frame = new MessageFrame();
        StorageUnits = new List<StorageUnit>();
    }

    /// <summary>
    /// Erstellt InventoryMessage
    /// </summary>
    public static InventoryMessage Create(string senderId, List<StorageUnit> storageUnits)
    {
        return new InventoryMessage
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
                Type = "inform",
                ConversationId = Guid.NewGuid().ToString()
            },
            StorageUnits = storageUnits
        };
    }

    /// <summary>
    /// Prüft, ob ein Item im Inventar ist
    /// </summary>
    public bool HasItem(string itemId, int minAmount = 1)
    {
        foreach (var storage in StorageUnits)
        {
            var count = storage.Slots.Count(slot => 
                !slot.Content.IsSlotEmpty && 
                (slot.Content.ProductID == itemId || slot.Content.ProductType == itemId));

            if (count >= minAmount)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Findet Slot mit bestimmtem Item
    /// </summary>
    public SlotContent? FindItem(string itemId)
    {
        foreach (var storage in StorageUnits)
        {
            var slot = storage.Slots.FirstOrDefault(s => 
                !s.Content.IsSlotEmpty && 
                (s.Content.ProductID == itemId || s.Content.ProductType == itemId));

            if (slot != null)
                return slot.Content;
        }
        return null;
    }
}

/// <summary>
/// Storage Unit (z.B. Storage, RFIDStorage)
/// </summary>
public class StorageUnit
{
    public string Name { get; set; } = string.Empty;
    public List<Slot> Slots { get; set; } = new();
}

/// <summary>
/// Slot im Storage
/// </summary>
public class Slot
{
    public int Index { get; set; }
    public SlotContent Content { get; set; } = new();
}

/// <summary>
/// Inhalt eines Slots
/// </summary>
public class SlotContent
{
    public string CarrierID { get; set; } = string.Empty;
    public string CarrierType { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public string ProductID { get; set; } = string.Empty;
    public bool IsSlotEmpty { get; set; }
}
