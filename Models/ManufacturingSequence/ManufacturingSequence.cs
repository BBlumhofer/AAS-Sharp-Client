using System.Collections.Generic;
using BaSyx.Models.AdminShell;
using AasSharpClient.Models.ProcessChain;

namespace AasSharpClient.Models.ManufacturingSequence;

/// <summary>
/// Top-level ManufacturingSequence submodel element (SubmodelElementCollection).
/// Mirrors ProcessChain but allows sequences of offered capabilities per requirement.
/// </summary>
public class ManufacturingSequence : SubmodelElementCollection
{
    public const string RequiredCapabilitiesIdShort = "RequiredCapabilities";

    public SubmodelElementList RequiredCapabilities { get; }

    public ManufacturingSequence(string idShort = "ManufacturingSequence") : base(idShort)
    {
        RequiredCapabilities = new SubmodelElementList(RequiredCapabilitiesIdShort);
        Add(RequiredCapabilities);
    }

    public void AddRequiredCapability(ManufacturingRequiredCapability capability)
    {
        if (capability == null)
        {
            return;
        }

        capability.IdShort = string.Empty;
        RequiredCapabilities.Add(capability);
    }

    public IEnumerable<ManufacturingRequiredCapability> GetRequiredCapabilities()
    {
        foreach (var element in RequiredCapabilities)
        {
            if (element is ManufacturingRequiredCapability required)
            {
                yield return required;
            }
        }
    }
}
