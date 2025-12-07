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
    public StateMessage(bool isLocked, bool isReady, string moduleState, bool startupSkillRunning = false)
        : base("State")
    {
        SemanticId = new Reference(new Key(KeyType.GlobalReference, "https://smartfactory.de/semantics/ModuleState"));

        Add(SubmodelElementFactory.CreateProperty("ModuleLocked", isLocked));
        Add(SubmodelElementFactory.CreateProperty("StartupSkillRunning", startupSkillRunning));
        Add(SubmodelElementFactory.CreateProperty("ModuleReady", isReady));
        Add(SubmodelElementFactory.CreateStringProperty("ModuleState", moduleState));
    }

    public static List<ISubmodelElement> CreateInteractionElements(
        bool isLocked,
        bool isReady,
        string moduleState,
        bool startupSkillRunning = false)
    {
        return new List<ISubmodelElement> { new StateMessage(isLocked, isReady, moduleState, startupSkillRunning) };
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
}
