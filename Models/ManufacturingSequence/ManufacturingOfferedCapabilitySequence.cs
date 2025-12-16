using System.Collections.Generic;
using BaSyx.Models.AdminShell;
using AasSharpClient.Models.ProcessChain;

namespace AasSharpClient.Models.ManufacturingSequence;

/// <summary>
/// Represents a candidate capability sequence (e.g., transports + main capability) for a required capability.
/// </summary>
public class ManufacturingOfferedCapabilitySequence : SubmodelElementList
{
    public ManufacturingOfferedCapabilitySequence(string idShort = "OfferedCapabilitySequence") : base(idShort)
    {
        OrderRelevant = false;
    }

    public void AddCapability(OfferedCapability capability)
    {
        if (capability == null)
        {
            return;
        }

        capability.IdShort = string.Empty;
        Add(capability);
    }

    public IEnumerable<OfferedCapability> GetCapabilities()
    {
        foreach (var element in this)
        {
            if (element is OfferedCapability capability)
            {
                yield return capability;
            }
        }
    }
}
