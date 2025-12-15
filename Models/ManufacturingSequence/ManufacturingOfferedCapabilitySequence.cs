using System.Collections.Generic;
using BaSyx.Models.AdminShell;
using AasSharpClient.Models.ProcessChain;

namespace AasSharpClient.Models.ManufacturingSequence;

/// <summary>
/// Represents a candidate capability sequence (e.g., transports + main capability) for a required capability.
/// </summary>
public class ManufacturingOfferedCapabilitySequence : SubmodelElementCollection
{
    public const string CapabilitySequenceIdShort = "CapabilitySequence";

    public SubmodelElementList CapabilitySequence { get; }

    public ManufacturingOfferedCapabilitySequence(string idShort = "OfferedCapabilitySequence") : base(idShort)
    {
        CapabilitySequence = new SubmodelElementList(CapabilitySequenceIdShort);
        Add(CapabilitySequence);
    }

    public void AddCapability(OfferedCapability capability)
    {
        if (capability == null)
        {
            return;
        }

        capability.IdShort = string.Empty;
        CapabilitySequence.Add(capability);
    }

    public IEnumerable<OfferedCapability> GetCapabilities()
    {
        foreach (var element in CapabilitySequence)
        {
            if (element is OfferedCapability capability)
            {
                yield return capability;
            }
        }
    }
}
