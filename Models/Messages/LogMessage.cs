using System;
using System.Collections.Generic;
using System.Linq;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// LogMessage - Logging für Agenten (nur InteractionElements)
/// </summary>
public static class LogMessage
{
    public enum LogLevel { Trace, Debug, Info, Warn, Error, Fatal }

    /// <summary>
    /// Create overload accepting LogLevel enum
    /// </summary>
    public static List<ISubmodelElement> CreateInteractionElements(LogLevel level, string message, string agentRole = "ResourceHolon", string agentState = "")
    {
        return CreateInteractionElements(level.ToString().ToUpperInvariant(), message, agentRole, agentState);
    }

    /// <summary>
    /// Erstellt InteractionElements für ein Log-Event.
    /// </summary>
    public static List<ISubmodelElement> CreateInteractionElements(
        string logLevel,
        string message,
        string agentRole,
        string agentState)
    {
        var elements = new List<ISubmodelElement>();

        elements.Add(new Property<string>("LogLevel") { Value = new PropertyValue<string>(logLevel) });
        elements.Add(new Property<string>("Message") { Value = new PropertyValue<string>(message) });
        elements.Add(new Property<string>("Timestamp") { Value = new PropertyValue<string>(DateTime.UtcNow.ToString("o")) });
        elements.Add(new Property<string>("AgentRole") { Value = new PropertyValue<string>(agentRole) });
        elements.Add(new Property<string>("AgentState") { Value = new PropertyValue<string>(agentState) });

        return elements;
    }

    /// <summary>
    /// Extrahiert LogLevel aus InteractionElements
    /// </summary>
    public static string GetLogLevel(List<ISubmodelElement> interactionElements)
    {
        return interactionElements
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "LogLevel")
            ?.Value?.Value?.ToObject<string>() ?? "INFO";
    }

    /// <summary>
    /// Extrahiert Message aus InteractionElements
    /// </summary>
    public static string GetMessage(List<ISubmodelElement> interactionElements)
    {
        return interactionElements
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "Message")
            ?.Value?.Value?.ToObject<string>() ?? string.Empty;
    }

    /// <summary>
    /// Extrahiert Timestamp (falls vorhanden)
    /// </summary>
    public static DateTime? GetTimestamp(List<ISubmodelElement> interactionElements)
    {
        var ts = interactionElements
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "Timestamp")
            ?.Value?.Value?.ToObject<string>();

        if (string.IsNullOrEmpty(ts)) return null;
        if (DateTime.TryParse(ts, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt)) return dt;
        return null;
    }

    /// <summary>
    /// Extrahiert AgentRole
    /// </summary>
    public static string GetAgentRole(List<ISubmodelElement> interactionElements)
    {
        return interactionElements
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "AgentRole")
            ?.Value?.Value?.ToObject<string>() ?? string.Empty;
    }
}
