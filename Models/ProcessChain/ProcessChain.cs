using System.Collections.Generic;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.ProcessChain;

/// <summary>
/// AAS SubmodelElementCollection representing a ProcessChain with a list of required capabilities.
/// </summary>
public class ProcessChain : SubmodelElementCollection
{
    public const string RequiredCapabilitiesIdShort = "RequiredCapabilities";

    public SubmodelElementList RequiredCapabilities { get; }

    public ProcessChain(string idShort = "ProcessChain") : base(idShort)
    {
        RequiredCapabilities = new SubmodelElementList(RequiredCapabilitiesIdShort)
        {
            // Hint: ElementType left unset (mixed collections allowed)
        };

        Add(RequiredCapabilities);
    }

    public void AddRequiredCapability(RequiredCapability capability)
    {
        if (capability != null)
        {
            // SubmodelElementList items must not carry their own IdShort
            capability.IdShort = string.Empty;
            RequiredCapabilities.Add(capability);
        }
    }

    public IEnumerable<RequiredCapability> GetRequiredCapabilities()
    {
        foreach (var element in RequiredCapabilities)
        {
            if (element is RequiredCapability rc)
            {
                yield return rc;
            }
        }
    }
}
