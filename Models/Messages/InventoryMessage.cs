using System;
using System.Collections.Generic;
using System.Linq;
using BaSyx.Models.AdminShell;
using AasSharpClient.Models;
using SubmodelFactory = AasSharpClient.Models.SubmodelElementFactory;
/// <summary>
/// InventoryMessage helper - stellt nur die InteractionElements dar (kein Frame)
/// </summary>
public class InventoryMessage : SubmodelElementCollection
{
    private readonly List<StorageUnit> _storageUnits;

    public IReadOnlyList<StorageUnit> StorageUnits => _storageUnits;

    /// <summary>
    /// Erstellt InteractionElements, die den Inventarinhalt beschreiben.
    /// Struktur: SubmodelElementCollection "StorageUnits" -> für jede StorageUnit: SubmodelElementCollection(Name) -> SubmodelElementCollection "Slots" -> Slot_N: SubmodelElementCollection mit Properties
    /// </summary>
    public InventoryMessage(List<StorageUnit> storageUnits) : base("StorageUnits")
    {
        _storageUnits = storageUnits ?? new List<StorageUnit>();
        foreach (var storage in _storageUnits)
        {
            var storageCollection = new SubmodelElementCollection(string.IsNullOrWhiteSpace(storage.Name) ? "Storage" : storage.Name);

            var slotsCollection = new SubmodelElementCollection("Slots");
            foreach (var slot in storage.Slots)
            {
                var slotCollection = new SubmodelElementCollection($"Slot_{slot.Index}");
                slotCollection.Add(SubmodelFactory.CreateProperty("Index", slot.Index));
                slotCollection.Add(SubmodelFactory.CreateStringProperty("CarrierID", slot.Content.CarrierID));
                slotCollection.Add(SubmodelFactory.CreateStringProperty("CarrierType", slot.Content.CarrierType));
                slotCollection.Add(SubmodelFactory.CreateStringProperty("ProductType", slot.Content.ProductType));
                slotCollection.Add(SubmodelFactory.CreateStringProperty("ProductID", slot.Content.ProductID));
                slotCollection.Add(SubmodelFactory.CreateProperty("IsSlotEmpty", slot.Content.IsSlotEmpty));

                slotsCollection.Add(slotCollection);
            }

            storageCollection.Add(slotsCollection);
            Add(storageCollection);
        }
    }

    /// <summary>
    /// Erstellt eine InventoryMessage aus bestehenden InteractionElements (z.B. empfangenen Nachrichten)
    /// </summary>
    public InventoryMessage(IEnumerable<ISubmodelElement> interactionElements) : base("StorageUnits")
    {
        _storageUnits = ExtractStorageUnits(interactionElements);
    }

    public static List<ISubmodelElement> CreateInteractionElements(List<StorageUnit> storageUnits)
    {
        return new List<ISubmodelElement> { new InventoryMessage(storageUnits) };
    }

    public static bool ContainsStorageUnits(IEnumerable<ISubmodelElement>? interactionElements)
    {
        return FindStorageUnitsCollection(interactionElements) != null;
    }

    public static List<StorageUnit> ExtractStorageUnits(IEnumerable<ISubmodelElement>? interactionElements)
    {
        var result = new List<StorageUnit>();
        var storageUnits = FindStorageUnitsCollection(interactionElements);
        if (storageUnits == null)
        {
            return result;
        }

        foreach (var storage in storageUnits.OfType<SubmodelElementCollection>())
        {
            var storageUnit = new StorageUnit
            {
                Name = storage.IdShort ?? "Storage",
                Slots = ExtractSlots(storage)
            };

            result.Add(storageUnit);
        }

        return result;
    }

    private static List<Slot> ExtractSlots(SubmodelElementCollection storage)
    {
        var slots = new List<Slot>();
        var slotsCollection = storage
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(c => string.Equals(c.IdShort, "Slots", StringComparison.OrdinalIgnoreCase));

        if (slotsCollection == null)
        {
            return slots;
        }

        foreach (var slotCollection in slotsCollection.OfType<SubmodelElementCollection>())
        {
            var slot = new Slot
            {
                Index = GetIntValue(slotCollection, "Index"),
                Content = new SlotContent
                {
                    CarrierID = GetStringValue(slotCollection, "CarrierID"),
                    CarrierType = GetStringValue(slotCollection, "CarrierType"),
                    ProductType = GetStringValue(slotCollection, "ProductType"),
                    ProductID = GetStringValue(slotCollection, "ProductID"),
                    IsSlotEmpty = GetBoolValue(slotCollection, "IsSlotEmpty")
                }
            };

            slots.Add(slot);
        }

        return slots;
    }

    private static SubmodelElementCollection? FindStorageUnitsCollection(IEnumerable<ISubmodelElement>? elements)
    {
        if (elements == null)
        {
            return null;
        }

        foreach (var collection in elements.OfType<SubmodelElementCollection>())
        {
            var match = FindStorageUnitsCollection(collection);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static SubmodelElementCollection? FindStorageUnitsCollection(SubmodelElementCollection? collection)
    {
        if (collection == null)
        {
            return null;
        }

        if (string.Equals(collection.IdShort, "StorageUnits", StringComparison.OrdinalIgnoreCase))
        {
            return collection;
        }

        if (string.Equals(collection.IdShort, "Inventory", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var child in collection.OfType<SubmodelElementCollection>())
            {
                var nested = FindStorageUnitsCollection(child);
                if (nested != null)
                {
                    return nested;
                }
            }
        }
        else
        {
            foreach (var child in collection.OfType<SubmodelElementCollection>())
            {
                if (string.Equals(child.IdShort, "StorageUnits", StringComparison.OrdinalIgnoreCase))
                {
                    return child;
                }

                var nested = FindStorageUnitsCollection(child);
                if (nested != null)
                {
                    return nested;
                }
            }
        }

        return null;
    }

    private static string GetStringValue(SubmodelElementCollection collection, string idShort)
    {
        var property = collection.Values
            .OfType<IProperty>()
            .FirstOrDefault(p => string.Equals(p.IdShort, idShort, StringComparison.OrdinalIgnoreCase));

        if (property?.Value?.Value == null)
        {
            return string.Empty;
        }

        try
        {
            return property.Value.Value.ToObject<string>() ?? string.Empty;
        }
        catch
        {
            return property.Value.Value.ToString() ?? string.Empty;
        }
    }

    private static int GetIntValue(SubmodelElementCollection collection, string idShort)
    {
        var property = collection.Values
            .OfType<IProperty>()
            .FirstOrDefault(p => string.Equals(p.IdShort, idShort, StringComparison.OrdinalIgnoreCase));

        if (property?.Value?.Value == null)
        {
            return 0;
        }

        try
        {
            return property.Value.Value.ToObject<int>();
        }
        catch
        {
            var fallback = property.Value.Value.ToString();
            return int.TryParse(fallback, out var value) ? value : 0;
        }
    }

    private static bool GetBoolValue(SubmodelElementCollection collection, string idShort)
    {
        var property = collection.Values
            .OfType<IProperty>()
            .FirstOrDefault(p => string.Equals(p.IdShort, idShort, StringComparison.OrdinalIgnoreCase));

        if (property?.Value?.Value == null)
        {
            return false;
        }

        try
        {
            return property.Value.Value.ToObject<bool>();
        }
        catch
        {
            var raw = property.Value.Value.ToString();
            if (bool.TryParse(raw, out var boolValue))
            {
                return boolValue;
            }

            return int.TryParse(raw, out var intValue) && intValue != 0;
        }
    }
    /// </summary>
    public static bool HasItem(IEnumerable<ISubmodelElement> interactionElements, string itemId, int minAmount = 1)
    {
        var storageUnits = (interactionElements ?? Enumerable.Empty<ISubmodelElement>())
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort == "StorageUnits");

        if (storageUnits == null)
            return false;

        foreach (var storage in storageUnits.OfType<SubmodelElementCollection>())
        {
            var slotsCollection = storage
                .OfType<SubmodelElementCollection>()
                .FirstOrDefault(c => c.IdShort == "Slots");

            if (slotsCollection == null)
                continue;

            int count = 0;
            foreach (var slot in slotsCollection.OfType<SubmodelElementCollection>())
            {
                var isEmpty = slot.Children.OfType<IProperty>()
                    .FirstOrDefault(p => p.IdShort == "IsSlotEmpty")
                    ?.Value?.Value?.ToObject<bool>() ?? true;

                if (isEmpty)
                    continue;

                var productId = slot.Children.OfType<IProperty>()
                    .FirstOrDefault(p => p.IdShort == "ProductID")
                    ?.Value?.Value?.ToObject<string>() ?? string.Empty;

                var productType = slot.Children.OfType<IProperty>()
                    .FirstOrDefault(p => p.IdShort == "ProductType")
                    ?.Value?.Value?.ToObject<string>() ?? string.Empty;

                if (string.Equals(productId, itemId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(productType, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    if (count >= minAmount)
                        return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Findet erstes SlotContent mit bestimmtem Item in den InteractionElements
    /// </summary>
    public static StorageSlot? FindItem(IEnumerable<ISubmodelElement> interactionElements, string itemId)
    {
        var storageUnits = (interactionElements ?? Enumerable.Empty<ISubmodelElement>())
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort == "StorageUnits");

        if (storageUnits == null)
            return null;

        foreach (var storage in storageUnits.OfType<SubmodelElementCollection>())
        {
            var slotsCollection = storage
                .OfType<SubmodelElementCollection>()
                .FirstOrDefault(c => c.IdShort == "Slots");

            if (slotsCollection == null)
                continue;

            foreach (var slot in slotsCollection.OfType<SubmodelElementCollection>())
            {
                var isEmpty = slot.Children.OfType<IProperty>()
                    .FirstOrDefault(p => p.IdShort == "IsSlotEmpty")
                    ?.Value?.Value?.ToObject<bool>() ?? true;

                if (isEmpty)
                    continue;

                var productId = slot.Children.OfType<IProperty>()
                    .FirstOrDefault(p => p.IdShort == "ProductID")
                    ?.Value?.Value?.ToObject<string>() ?? string.Empty;

                var productType = slot.Children.OfType<IProperty>()
                    .FirstOrDefault(p => p.IdShort == "ProductType")
                    ?.Value?.Value?.ToObject<string>() ?? string.Empty;

                if (string.Equals(productId, itemId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(productType, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    // Build SlotContent from properties
                    var content = new SlotContent
                    {
                        CarrierID = slot.Children.OfType<IProperty>()
                            .FirstOrDefault(p => p.IdShort == "CarrierID")
                            ?.Value?.Value?.ToObject<string>() ?? string.Empty,
                        CarrierType = slot.Children.OfType<IProperty>()
                            .FirstOrDefault(p => p.IdShort == "CarrierType")
                            ?.Value?.Value?.ToObject<string>() ?? string.Empty,
                        ProductType = productType,
                        ProductID = productId,
                        IsSlotEmpty = false
                    };

                    var storageName = storage.IdShort ?? string.Empty;
                    var index = slot.Children.OfType<IProperty>()
                        .FirstOrDefault(p => p.IdShort == "Index")
                        ?.Value?.Value?.ToObject<int>() ?? -1;

                    return new StorageSlot
                    {
                        StorageName = storageName,
                        Index = index,
                        Content = content
                    };
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Findet alle StorageSlots mit einem bestimmten Item
    /// </summary>
    public static List<StorageSlot> FindItems(IEnumerable<ISubmodelElement> interactionElements, string itemId)
    {
        var result = new List<StorageSlot>();
        var storageUnits = (interactionElements ?? Enumerable.Empty<ISubmodelElement>())
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort == "StorageUnits");

        if (storageUnits == null)
            return result;

        foreach (var storage in storageUnits.OfType<SubmodelElementCollection>())
        {
            var slotsCollection = storage
                .OfType<SubmodelElementCollection>()
                .FirstOrDefault(c => c.IdShort == "Slots");

            if (slotsCollection == null)
                continue;

            foreach (var slot in slotsCollection.OfType<SubmodelElementCollection>())
            {
                var isEmpty = slot.Children.OfType<IProperty>()
                    .FirstOrDefault(p => p.IdShort == "IsSlotEmpty")
                    ?.Value?.Value?.ToObject<bool>() ?? true;

                if (isEmpty)
                    continue;

                var productId = slot.Children.OfType<IProperty>()
                    .FirstOrDefault(p => p.IdShort == "ProductID")
                    ?.Value?.Value?.ToObject<string>() ?? string.Empty;

                var productType = slot.Children.OfType<IProperty>()
                    .FirstOrDefault(p => p.IdShort == "ProductType")
                    ?.Value?.Value?.ToObject<string>() ?? string.Empty;

                if (string.Equals(productId, itemId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(productType, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    var content = new SlotContent
                    {
                        CarrierID = slot.Children.OfType<IProperty>()
                            .FirstOrDefault(p => p.IdShort == "CarrierID")
                            ?.Value?.Value?.ToObject<string>() ?? string.Empty,
                        CarrierType = slot.Children.OfType<IProperty>()
                            .FirstOrDefault(p => p.IdShort == "CarrierType")
                            ?.Value?.Value?.ToObject<string>() ?? string.Empty,
                        ProductType = productType,
                        ProductID = productId,
                        IsSlotEmpty = false
                    };

                    var storageName = storage.IdShort ?? string.Empty;
                    var index = slot.Children.OfType<IProperty>()
                        .FirstOrDefault(p => p.IdShort == "Index")
                        ?.Value?.Value?.ToObject<int>() ?? -1;

                    result.Add(new StorageSlot { StorageName = storageName, Index = index, Content = content });
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Zählt, wie oft ein Item vorkommt
    /// </summary>
    public static int CountItemOccurrences(IEnumerable<ISubmodelElement> interactionElements, string itemId)
    {
        return FindItems(interactionElements, itemId).Count;
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

/// <summary>
/// Repräsentiert einen gefundenen Slot inklusive Storage-Name und Index
/// </summary>
public class StorageSlot
{
    public string StorageName { get; set; } = string.Empty;
    public int Index { get; set; }
    public SlotContent Content { get; set; } = new();
}
