using System;
using System.Collections.Generic;
using System.Linq;
using BaSyx.Models.AdminShell;
using AasSharpClient.Models;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// LogMessage - Logging für Agenten (nur InteractionElements)
/// </summary>
public class LogMessage : SubmodelElementCollection
{
    public enum LogLevel { Trace, Debug, Info, Warn, Error, Fatal }

    public LogMessage(
        LogLevel level,
        string message,
        string agentRole = "ResourceHolon",
        string agentState = "",
        string moduleId = "")
        : this(level.ToString().ToUpperInvariant(), message, agentRole, agentState, moduleId)
    {
    }

    public LogMessage(
        string logLevel,
        string message,
        string agentRole = "ResourceHolon",
        string agentState = "",
        string moduleId = "")
        : base("Log")
    {
        // Use SubmodelElementFactory so ValueType and Value are set correctly
        Add(SubmodelElementFactory.CreateStringProperty("LogLevel", logLevel));
        Add(SubmodelElementFactory.CreateStringProperty("Message", message));
        Add(SubmodelElementFactory.CreateStringProperty("Timestamp", DateTime.UtcNow.ToString("o")));
        Add(SubmodelElementFactory.CreateStringProperty("AgentRole", agentRole));
        Add(SubmodelElementFactory.CreateStringProperty("AgentState", agentState));

        if (!string.IsNullOrWhiteSpace(moduleId))
        {
            Add(SubmodelElementFactory.CreateStringProperty("ModuleId", moduleId));
        }
    }

    // Backwards-compatible factory
    public static List<ISubmodelElement> CreateInteractionElements(
        LogLevel level,
        string message,
        string agentRole = "ResourceHolon",
        string agentState = "",
        string moduleId = "")
    {
        return CreateInteractionElements(level.ToString().ToUpperInvariant(), message, agentRole, agentState, moduleId);
    }

    public static List<ISubmodelElement> CreateInteractionElements(
        string logLevel,
        string message,
        string agentRole,
        string agentState,
        string moduleId = "")
    {
        return new List<ISubmodelElement> { new LogMessage(logLevel, message, agentRole, agentState, moduleId) };
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
