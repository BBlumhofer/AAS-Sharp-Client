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
    public const string InstanceIdentifierIdShort = "InstanceIdentifier";

    public ReferenceElement RequiredCapabilityReference { get; }
    public Property<string> InstanceIdentifier { get; }
    public SubmodelElementList OfferedCapabilities { get; }
    public SubmodelElementCollection? InitialPlannedScheduling { get; private set; }

    public RequiredCapability(string idShort) : base(idShort)
    {
        RequiredCapabilityReference = new ReferenceElement(RequiredCapabilityReferenceIdShort);
        InstanceIdentifier = new Property<string>(InstanceIdentifierIdShort)
        {
            Value = new PropertyValue<string>(string.Empty)
        };
        OfferedCapabilities = new SubmodelElementList(OfferedCapabilitiesIdShort);

        Add(RequiredCapabilityReference);
        Add(InstanceIdentifier);
        Add(OfferedCapabilities);
    }

    public void SetInstanceIdentifier(string value)
    {
        InstanceIdentifier.Value = new PropertyValue<string>(value ?? string.Empty);
    }

    public void SetRequiredCapabilityReference(Reference reference)
    {
        RequiredCapabilityReference.Value = new ReferenceElementValue(reference);
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
        AddCapabilityOffer(capability);
    }

    public void AddCapabilityOffer(OfferedCapability capability)
    {
        if (capability != null)
        {
            if (!string.IsNullOrEmpty(capability.IdShort))
            {
                capability.IdShort = string.Empty;
            }
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

    public IEnumerable<OfferedCapability> GetCapabilityOffers() => GetOfferedCapabilities();
}
