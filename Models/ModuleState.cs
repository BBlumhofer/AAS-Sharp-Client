using System;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models;

/// <summary>
/// ModuleState - Repräsentiert den Zustand eines Moduls (Locked, Ready, HasError)
/// Wird für State Messages verwendet: /Modules/{ModuleID}/State/
/// </summary>
public class ModuleState : SubmodelElementCollection
{
    public Property<bool> ModuleLocked { get; }
    public Property<bool> ModuleReady { get; }
    public Property<bool> HasError { get; }

    public ModuleState(bool isLocked, bool isReady, bool hasError)
        : base("ModuleState")
    {
        SemanticId = SemanticReferences.EmptyExternal; // TODO: Add proper semantic reference
        
        ModuleLocked = new Property<bool>("ModuleLocked", isLocked);
        ModuleReady = new Property<bool>("ModuleReady", isReady);
        HasError = new Property<bool>("HasError", hasError);
        
        Add(ModuleLocked);
        Add(ModuleReady);
        Add(HasError);
    }

    /// <summary>
    /// Setzt den Locked-Status
    /// </summary>
    public void SetLocked(bool isLocked)
    {
        ModuleLocked.Value = new PropertyValue<bool>(isLocked);
    }

    /// <summary>
    /// Setzt den Ready-Status
    /// </summary>
    public void SetReady(bool isReady)
    {
        ModuleReady.Value = new PropertyValue<bool>(isReady);
    }

    /// <summary>
    /// Setzt den Error-Status
    /// </summary>
    public void SetError(bool hasError)
    {
        HasError.Value = new PropertyValue<bool>(hasError);
    }

    /// <summary>
    /// Gibt den aktuellen Locked-Status zurück
    /// </summary>
    public bool GetLocked()
    {
        var raw = ExtractRawValue(ModuleLocked);
        return raw is bool value ? value : false;
    }

    /// <summary>
    /// Gibt den aktuellen Ready-Status zurück
    /// </summary>
    public bool GetReady()
    {
        var raw = ExtractRawValue(ModuleReady);
        return raw is bool value ? value : false;
    }

    /// <summary>
    /// Gibt den aktuellen Error-Status zurück
    /// </summary>
    public bool GetError()
    {
        var raw = ExtractRawValue(HasError);
        return raw is bool value ? value : false;
    }
    
    /// <summary>
    /// Extrahiert den tatsächlichen Wert aus einer Property (analog zu KeyValueSubmodelCollection)
    /// </summary>
    private static object? ExtractRawValue(Property property)
    {
        var value = property.Value?.Value;
        if (value is IValue inner)
        {
            return inner.Value;
        }
        return value;
    }
}
