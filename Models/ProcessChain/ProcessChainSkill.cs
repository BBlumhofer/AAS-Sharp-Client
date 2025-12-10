using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.ProcessChain;

/// <summary>
/// AAS SubmodelElementCollection representing a skill entry within an offered capability.
/// </summary>
public class ProcessChainSkill : SubmodelElementCollection
{
    public const string SkillReferenceIdShort = "SkillReference";
    public const string FeasibilityCheckDataIdShort = "FeasibilityCheckData";
    public const string RequiredInputParametersIdShort = "RequiredInputParameters";
    public const string ExpectedProcessTimeIdShort = "ExpectedProcessTime";

    public ReferenceElement SkillReference { get; }
    public SubmodelElementCollection FeasibilityCheckData { get; }
    public SubmodelElementCollection RequiredInputParameters { get; }
    public Property<double> ExpectedProcessTime { get; }

    public ProcessChainSkill(string idShort) : base(idShort)
    {
        SkillReference = new ReferenceElement(SkillReferenceIdShort);
        FeasibilityCheckData = new SubmodelElementCollection(FeasibilityCheckDataIdShort);
        RequiredInputParameters = new SubmodelElementCollection(RequiredInputParametersIdShort);
        ExpectedProcessTime = new Property<double>(ExpectedProcessTimeIdShort)
        {
            Value = new PropertyValue<double>(0.0)
        };

        Add(SkillReference);
        Add(FeasibilityCheckData);
        Add(RequiredInputParameters);
        Add(ExpectedProcessTime);
    }
}
