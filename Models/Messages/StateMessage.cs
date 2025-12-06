using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// StateMessage Helper - Modulzustand (Ready, Locked, Notifications)
/// Nur InteractionElements - Frame wird im Messaging Client erstellt
/// </summary>
public static class StateMessage
{
    /// <summary>
    /// Erstellt InteractionElements für StateMessage
    /// </summary>
    public static List<ISubmodelElement> CreateInteractionElements(
        bool isLocked,
        bool isReady,
        string moduleState,
        bool startupSkillRunning = false)
    {
        var elements = new List<ISubmodelElement>();

        var stateCollection = new SubmodelElementCollection("State")
        {
            SemanticId = new Reference(new Key(KeyType.GlobalReference, "https://smartfactory.de/semantics/ModuleState"))
        };

        stateCollection.Add(new Property<bool>("ModuleLocked") { Value = new PropertyValue<bool>(isLocked) });
        stateCollection.Add(new Property<bool>("StartupSkillRunning") { Value = new PropertyValue<bool>(startupSkillRunning) });
        stateCollection.Add(new Property<bool>("ModuleReady") { Value = new PropertyValue<bool>(isReady) });
        stateCollection.Add(new Property<string>("ModuleState") { Value = new PropertyValue<string>(moduleState) });

        elements.Add(stateCollection);
        return elements;
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
