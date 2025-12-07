using System;
using System.Collections.Generic;
using System.Linq;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// NeighborMessage - Nachbarn eines Moduls (nur InteractionElements)
/// </summary>
public static class NeighborMessage
{
    /// <summary>
    /// Erstellt InteractionElements mit einer Liste von Nachbarn
    /// </summary>
    public static List<ISubmodelElement> CreateInteractionElements(string[] neighbors)
    {
        var elements = new List<ISubmodelElement>();

        var neighborsCollection = new SubmodelElementCollection("Neighbors");

        int index = 0;
        foreach (var neighbor in neighbors)
        {
            var property = new Property<string>($"Neighbor_{index++}");
            property.Value = new PropertyValue<string>(neighbor);
            neighborsCollection.Add(property);
        }

        elements.Add(neighborsCollection);
        return elements;
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
        var prop = new Property<string>($"Neighbor_{index}") { Value = new PropertyValue<string>(neighbor) };
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
