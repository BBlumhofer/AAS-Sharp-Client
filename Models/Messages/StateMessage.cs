using BaSyx.Models.AdminShell;
using AasSharpClient.Models;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// StateMessage Helper - Modulzustand (Ready, Locked, Notifications)
/// Nur InteractionElements - Frame wird im Messaging Client erstellt
/// </summary>
public class StateMessage : SubmodelElementCollection
{
    /// <summary>
    /// Erstellt InteractionElements für StateMessage
    /// </summary>
    public StateMessage(
        bool? isLocked,
        bool? isReady,
        string moduleState,
        bool hasError = false,
        bool startupSkillRunning = false)
        : base("State")
    {
        SemanticId = new Reference(new Key(KeyType.GlobalReference, "https://smartfactory.de/semantics/ModuleState"));

        if (isLocked.HasValue)
        {
            Add(SubmodelElementFactory.CreateProperty("ModuleLocked", isLocked.Value));
        }

        Add(SubmodelElementFactory.CreateProperty("StartupSkillRunning", startupSkillRunning));
        if (isReady.HasValue)
        {
            Add(SubmodelElementFactory.CreateProperty("ModuleReady", isReady.Value));
        }
        Add(SubmodelElementFactory.CreateStringProperty("ModuleState", moduleState));
        Add(SubmodelElementFactory.CreateProperty("HasError", hasError));
    }

    public static List<ISubmodelElement> CreateInteractionElements(
        bool? isLocked,
        bool? isReady,
        string moduleState,
        bool hasError = false,
        bool startupSkillRunning = false)
    {
        return new List<ISubmodelElement>
        {
            new StateMessage(isLocked, isReady, moduleState, hasError, startupSkillRunning)
        };
    }

    /// <summary>
    /// Extrahiert ModuleLocked aus InteractionElements
    /// </summary>
    public static bool GetIsLocked(List<ISubmodelElement> interactionElements)
    {
        var stateCollection = interactionElements
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort == "State");

        return stateCollection?.Children
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "ModuleLocked")
            ?.Value?.Value?.ToObject<bool>() ?? false;
    }

    /// <summary>
    /// Extrahiert ModuleReady aus InteractionElements
    /// </summary>
    public static bool GetIsReady(List<ISubmodelElement> interactionElements)
    {
        var stateCollection = interactionElements
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort == "State");

        return stateCollection?.Children
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "ModuleReady")
            ?.Value?.Value?.ToObject<bool>() ?? false;
    }

    /// <summary>
    /// Extrahiert ModuleState aus InteractionElements
    /// </summary>
    public static string GetModuleState(List<ISubmodelElement> interactionElements)
    {
        var stateCollection = interactionElements
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort == "State");

        return stateCollection?.Children
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "ModuleState")
            ?.Value?.Value?.ToObject<string>() ?? "Unknown";
    }

    /// <summary>
    /// Extrahiert HasError aus InteractionElements
    /// </summary>
    public static bool GetHasError(List<ISubmodelElement> interactionElements)
    {
        var stateCollection = interactionElements
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(e => e.IdShort == "State");

        return stateCollection?.Children
            .OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == "HasError")
            ?.Value?.Value?.ToObject<bool>() ?? false;
    }
}
