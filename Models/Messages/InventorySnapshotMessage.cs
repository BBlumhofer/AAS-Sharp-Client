using System;
using System.Collections.Generic;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// InventorySnapshotMessage - verpackt flache Key/Value-Daten (z. B. FinalResultData) als SubmodelElementCollection.
/// </summary>
public class InventorySnapshotMessage : SubmodelElementCollection
{
    public InventorySnapshotMessage(IDictionary<string, object?> values, string idShort = "Inventory")
        : base(string.IsNullOrWhiteSpace(idShort) ? "Inventory" : idShort)
    {
        foreach (var kvp in values)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key))
                continue;

            Add(SubmodelElementFactory.CreateProperty(kvp.Key, kvp.Value));
        }
    }

    public static List<ISubmodelElement> CreateInteractionElements(IDictionary<string, object?> values, string idShort = "Inventory")
    {
        return new List<ISubmodelElement> { new InventorySnapshotMessage(values, idShort) };
    }
}
