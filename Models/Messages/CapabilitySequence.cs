using System;
using System.Collections.Generic;
using AasSharpClient.Models.ProcessChain;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// Typed wrapper around a BaSyx <see cref="SubmodelElementList"/> that is expected to contain
/// <see cref="OfferedCapability"/> elements.
/// </summary>
public class CapabilitySequence : SubmodelElementList
{
    public CapabilitySequence(string idShort = TransportRequestMessage.CapabilitiesSequenceIdShort) : base(idShort)
    {
        OrderRelevant = false;
    }

    public IEnumerable<OfferedCapability> OfferedCapabilities
    {
        get
        {
            foreach (var element in this)
            {
                if (element is OfferedCapability cap)
                {
                    yield return cap;
                }
            }
        }
    }

    public void AddCapability(OfferedCapability capability)
    {
        if (capability == null)
        {
            return;
        }

        // Keep list entries anonymous; consumers typically rely on InstanceIdentifier.
        if (!string.IsNullOrEmpty(capability.IdShort))
        {
            capability.IdShort = string.Empty;
        }

        Add(capability);
    }
}
