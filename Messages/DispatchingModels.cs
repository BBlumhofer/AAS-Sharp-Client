using System;
using System.Collections.Generic;
using System.Linq;
using BaSyx.Models.AdminShell;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Messages;

/// <summary>
/// Registration information for a module known to the dispatching agent.
/// </summary>
public class DispatchingModuleInfo
{
    public string ModuleId { get; set; } = string.Empty;
    public string? AasId { get; set; }
    public List<string> Capabilities { get; set; } = new();
    public List<string> Neighbors { get; set; } = new();
    public DateTime LastRegistrationUtc { get; set; } = DateTime.UtcNow;

    public static DispatchingModuleInfo FromMessage(I40Message? message)
    {
        var info = new DispatchingModuleInfo
        {
            LastRegistrationUtc = DateTime.UtcNow
        };

        if (message != null)
        {
            // try interaction elements
            foreach (var element in message.InteractionElements)
            {
                if (element is Property prop && !string.IsNullOrWhiteSpace(prop.IdShort))
                {
                    var rawValue = prop.Value?.Value?.Value; // extract the underlying value from IValue
                    var val = rawValue as string ?? rawValue?.ToString();
                    if (string.Equals(prop.IdShort, "ModuleId", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(val))
                    {
                        info.ModuleId = val!;
                    }
                }
            }

            // fallback: frame sender id
            if (string.IsNullOrWhiteSpace(info.ModuleId))
            {
                info.ModuleId = message.Frame?.Sender?.Identification?.Id ?? string.Empty;
            }
        }

        return info;
    }
}

/// <summary>
/// In-memory registry/state for dispatching, indexing modules by capability.
/// </summary>
public class DispatchingState
{
    private readonly Dictionary<string, DispatchingModuleInfo> _modules = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _capabilityIndex = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<DispatchingModuleInfo> Modules => _modules.Values;

    public void Upsert(DispatchingModuleInfo module)
    {
        if (string.IsNullOrWhiteSpace(module.ModuleId))
        {
            return;
        }

        if (_modules.TryGetValue(module.ModuleId, out var existing))
        {
            RemoveFromIndex(existing);
        }

        _modules[module.ModuleId] = module;
        AddToIndex(module);
    }

    public IReadOnlyCollection<string> FindModulesForCapability(string capability)
    {
        if (string.IsNullOrWhiteSpace(capability))
        {
            return _modules.Keys.ToList();
        }

        if (_capabilityIndex.TryGetValue(capability, out var set))
        {
            return set.ToList();
        }

        return Array.Empty<string>();
    }

    public IReadOnlyCollection<string> AllModuleIds() => _modules.Keys.ToList();

    private void AddToIndex(DispatchingModuleInfo module)
    {
        foreach (var cap in module.Capabilities.Where(c => !string.IsNullOrWhiteSpace(c)))
        {
            if (!_capabilityIndex.TryGetValue(cap, out var set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _capabilityIndex[cap] = set;
            }
            set.Add(module.ModuleId);
        }
    }

    private void RemoveFromIndex(DispatchingModuleInfo module)
    {
        foreach (var cap in module.Capabilities.Where(c => !string.IsNullOrWhiteSpace(c)))
        {
            if (_capabilityIndex.TryGetValue(cap, out var set))
            {
                set.Remove(module.ModuleId);
                if (set.Count == 0)
                {
                    _capabilityIndex.Remove(cap);
                }
            }
        }
    }
}
