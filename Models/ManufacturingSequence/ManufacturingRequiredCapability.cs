using System.Collections.Generic;
using BaSyx.Models.AdminShell;
using AasSharpClient.Models.ProcessChain;

namespace AasSharpClient.Models.ManufacturingSequence;

/// <summary>
/// Required capability entry inside a ManufacturingSequence. Each entry can contain multiple offered capability sequences.
/// </summary>
public class ManufacturingRequiredCapability : SubmodelElementCollection
{
    public const string RequiredCapabilityReferenceIdShort = "RequiredCapabilityReference";
    public const string InstanceIdentifierIdShort = "InstanceIdentifier";
    public const string OfferedCapabilitySequencesIdShort = "OfferedCapabilitySequences";

    public ReferenceElement RequiredCapabilityReference { get; }
    public Property<string> InstanceIdentifier { get; }
    public SubmodelElementList OfferedCapabilitySequences { get; }

    public ManufacturingRequiredCapability(string idShort) : base(idShort)
    {
        RequiredCapabilityReference = new ReferenceElement(RequiredCapabilityReferenceIdShort);
        InstanceIdentifier = new Property<string>(InstanceIdentifierIdShort)
        {
            Value = new PropertyValue<string>(string.Empty)
        };
        OfferedCapabilitySequences = new SubmodelElementList(OfferedCapabilitySequencesIdShort);

        Add(RequiredCapabilityReference);
        Add(InstanceIdentifier);
        Add(OfferedCapabilitySequences);
    }

    public void SetInstanceIdentifier(string value)
    {
        InstanceIdentifier.Value = new PropertyValue<string>(value ?? string.Empty);
    }

    public void SetRequiredCapabilityReference(Reference reference)
    {
        if (reference == null)
        {
            return;
        }

        RequiredCapabilityReference.Value = new ReferenceElementValue(reference);
    }

    public void AddSequence(ManufacturingOfferedCapabilitySequence sequence)
    {
        if (sequence == null)
        {
            return;
        }

        sequence.IdShort = string.Empty;
        OfferedCapabilitySequences.Add(sequence);
    }

    public IEnumerable<ManufacturingOfferedCapabilitySequence> GetSequences()
    {
        foreach (var element in OfferedCapabilitySequences)
        {
            if (element is ManufacturingOfferedCapabilitySequence seq)
            {
                yield return seq;
            }
        }
    }
}
