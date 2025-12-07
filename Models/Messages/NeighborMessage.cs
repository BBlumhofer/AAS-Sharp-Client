using System;
using System.Collections.Generic;
using System.Linq;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// NeighborMessage - Nachbarn eines Moduls (nur InteractionElements)
/// </summary>
public class NeighborMessage : SubmodelElementCollection
{
    public NeighborMessage(IEnumerable<string> neighbors) : base("Neighbors")
    {
        int index = 0;
        foreach (var neighbor in neighbors)
        {
            Add(SubmodelElementFactory.CreateStringProperty($"Neighbor_{index++}", neighbor));
        }
    }

    public NeighborMessage(string[] neighbors) : base("Neighbors")
    {
        int index = 0;
        foreach (var neighbor in neighbors)
        {
            var property = SubmodelElementFactory.CreateStringProperty($"Neighbor_{index++}", neighbor);
            Add(property);
        }
    }

    public static List<ISubmodelElement> CreateInteractionElements(string[] neighbors)
    {
        return new List<ISubmodelElement> { new NeighborMessage(neighbors) };
    }
    

    /// <summary>
    /// Extrahiert Nachbarn-Liste aus InteractionElements
    /// </summary>
    public static List<string> GetNeighbors(List<ISubmodelElement> interactionElements)
    {
        var neighborsCollection = interactionElements
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort == "Neighbors");

        if (neighborsCollection == null)
            return new List<string>();

        var result = new List<string>();
        foreach (var element in neighborsCollection)
        {
            if (element is IProperty prop && prop.Value != null)
            {
                var value = prop.Value?.Value?.ToObject<string>() ?? prop.Value?.ToString() ?? string.Empty;
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
    public static bool HasNeighbor(List<ISubmodelElement> interactionElements, string neighborId)
    {
        return GetNeighbors(interactionElements).Contains(neighborId, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Fügt einen Neighbor zur InteractionElements-Liste hinzu (mutierend)
    /// </summary>
    public static void AddNeighbor(IList<ISubmodelElement> interactionElements, string neighbor)
    {
        if (interactionElements == null) throw new ArgumentNullException(nameof(interactionElements));

        var neighborsCollection = interactionElements.OfType<SubmodelElementCollection>().FirstOrDefault(e => e.IdShort == "Neighbors");
        if (neighborsCollection == null)
        {
            neighborsCollection = new SubmodelElementCollection("Neighbors");
            interactionElements.Add(neighborsCollection);
        }

        var index = neighborsCollection.Children.OfType<IProperty>().Count();
        var prop = SubmodelElementFactory.CreateStringProperty($"Neighbor_{index}", neighbor);
        neighborsCollection.Add(prop);
    }

    /// <summary>
    /// Fügt mehrere Neighbors hinzu
    /// </summary>
    public static void AddNeighbors(IList<ISubmodelElement> interactionElements, IEnumerable<string> neighbors)
    {
        foreach (var n in neighbors) AddNeighbor(interactionElements, n);
    }
}
