using System.Collections.Generic;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.ProcessChain;

/// <summary>
/// AAS SubmodelElementCollection representing a RequiredCapability with offered capabilities and a reference to the required capability.
/// </summary>
public class RequiredCapability : SubmodelElementCollection
{
    public const string RequiredCapabilityReferenceIdShort = "RequiredCapabilityReference";
    public const string OfferedCapabilitiesIdShort = "OfferedCapabilities";
    public const string InitialPlannedSchedulingIdShort = "InitialPlannedScheduling";

    public ReferenceElement RequiredCapabilityReference { get; }
    public SubmodelElementList OfferedCapabilities { get; }
    public SubmodelElementCollection? InitialPlannedScheduling { get; private set; }

    public RequiredCapability(string idShort) : base(idShort)
    {
        RequiredCapabilityReference = new ReferenceElement(RequiredCapabilityReferenceIdShort);
        OfferedCapabilities = new SubmodelElementList(OfferedCapabilitiesIdShort);

        Add(RequiredCapabilityReference);
        Add(OfferedCapabilities);
    }

    public void SetInitialPlannedScheduling(SubmodelElementCollection scheduling)
    {
        InitialPlannedScheduling = scheduling;
        if (!Contains(scheduling))
        {
            Add(scheduling);
        }
    }

    public void AddOfferedCapability(OfferedCapability capability)
    {
        if (capability != null)
        {
            OfferedCapabilities.Add(capability);
        }
    }

    public IEnumerable<OfferedCapability> GetOfferedCapabilities()
    {
        foreach (var element in OfferedCapabilities)
        {
            if (element is OfferedCapability oc)
            {
                yield return oc;
            }
        }
    }
}
