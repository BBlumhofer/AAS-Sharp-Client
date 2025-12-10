using System.Collections.Generic;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.ProcessChain;

/// <summary>
/// AAS SubmodelElementCollection representing an offered capability, including skills, matching score, and references.
/// </summary>
public class OfferedCapability : SubmodelElementCollection
{
    public const string OfferedCapabilityReferenceIdShort = "OfferedCapabilityReference";
    public const string InstanceIdentifierIdShort = "InstanceIdentifier";
    public const string MatchingScoreIdShort = "MatchingScore";
    public const string StationIdShort = "Station";
    public const string SkillsIdShort = "Skills";

    public ReferenceElement OfferedCapabilityReference { get; }
    public Property<string> InstanceIdentifier { get; }
    public Property<double> MatchingScore { get; }
    public Property<string> Station { get; }
    public SubmodelElementList Skills { get; }

    public OfferedCapability(string idShort) : base(idShort)
    {
        OfferedCapabilityReference = new ReferenceElement(OfferedCapabilityReferenceIdShort);
        InstanceIdentifier = new Property<string>(InstanceIdentifierIdShort) { Value = new PropertyValue<string>(string.Empty) };
        MatchingScore = new Property<double>(MatchingScoreIdShort) { Value = new PropertyValue<double>(0.0) };
        Station = new Property<string>(StationIdShort) { Value = new PropertyValue<string>(string.Empty) };
        Skills = new SubmodelElementList(SkillsIdShort);

        Add(OfferedCapabilityReference);
        Add(InstanceIdentifier);
        Add(MatchingScore);
        Add(Station);
        Add(Skills);
    }

    public void AddSkill(ProcessChainSkill skill)
    {
        if (skill != null)
        {
            Skills.Add(skill);
        }
    }

    public IEnumerable<ProcessChainSkill> GetSkills()
    {
        foreach (var element in Skills)
        {
            if (element is ProcessChainSkill skill)
            {
                yield return skill;
            }
        }
    }
}
