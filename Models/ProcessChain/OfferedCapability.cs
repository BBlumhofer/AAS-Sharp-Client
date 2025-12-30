using System;
using System.Collections.Generic;
using System.Linq;
using AasSharpClient.Models;
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
    public const string EarliestSchedulingInformationIdShort = "EarliestSchedulingInformation";
    public const string ActionsIdShort = "Actions";
    public const string CostIdShort = "Cost";
    public const string SequencePlacementIdShort = "SequencePlacement";

    public ReferenceElement OfferedCapabilityReference { get; }
    public Property<string> InstanceIdentifier { get; }
    public Property<double> MatchingScore { get; }
    public Property<string> Station { get; }
    public SchedulingContainer EarliestSchedulingInformation { get; }
    public SubmodelElementList Actions { get; }
    public SubmodelElementCollection Preconditions { get; }
    public SubmodelElementCollection FinalResultData { get; }
    public SubmodelElementCollection Effects { get; }
    public SubmodelElementCollection Postconditions { get; }
    public Property<double> Cost { get; }
    public Property<string> SequencePlacement { get; }

    public OfferedCapability(string idShort) : base(idShort)
    {
        OfferedCapabilityReference = new ReferenceElement(OfferedCapabilityReferenceIdShort);
        InstanceIdentifier = new Property<string>(InstanceIdentifierIdShort) { Value = new PropertyValue<string>(string.Empty) };
        MatchingScore = new Property<double>(MatchingScoreIdShort) { Value = new PropertyValue<double>(0.0) };
        Station = new Property<string>(StationIdShort) { Value = new PropertyValue<string>(string.Empty) };
        EarliestSchedulingInformation = new SchedulingContainer()
        {
            IdShort = EarliestSchedulingInformationIdShort
        };
        Actions = new SubmodelElementList(ActionsIdShort);
        Cost = new Property<double>(CostIdShort)
        {
            Value = new PropertyValue<double>(0.0)
        };
        SequencePlacement = new Property<string>(SequencePlacementIdShort)
        {
            Value = new PropertyValue<string>(string.Empty)
        };

        Add(OfferedCapabilityReference);
        Add(InstanceIdentifier);
        Add(MatchingScore);
        Add(Station);
        Add(EarliestSchedulingInformation);
        Add(Actions);
        Preconditions = new SubmodelElementCollection("Preconditions");
        FinalResultData = new SubmodelElementCollection("FinalResultData");
        Effects = new SubmodelElementCollection("Effects");
        Postconditions = new SubmodelElementCollection("Postconditions");
        Add(Preconditions);
        Add(FinalResultData);
        Add(Effects);
        Add(Postconditions);
        Add(Cost);
        Add(SequencePlacement);
    }

    public void SetEarliestScheduling(DateTime start, DateTime end, TimeSpan setup, TimeSpan cycle)
    {
        EarliestSchedulingInformation.SetStartDateTime(start);
        EarliestSchedulingInformation.SetEndDateTime(end);
        EarliestSchedulingInformation.SetSetupTime(setup);
        EarliestSchedulingInformation.SetCycleTime(cycle);
    }

    public void AddAction(Action action)
    {
        if (action != null)
        {
            // ensure anonymous entries (no meaningful IdShort)
            try { action.IdShort = string.Empty; } catch { }
            Actions.Add(action);
        }
    }

    /// <summary>
    /// Typed view of the contained actions as a list of <see cref="AasSharpClient.Models.Action"/>.
    /// This materializes a snapshot from the underlying SubmodelElementList.
    /// </summary>
    public List<AasSharpClient.Models.Action> ActionsList
    {
        get
        {
            return Actions.OfType<AasSharpClient.Models.Action>().ToList();
        }
    }
    /// <summary>
    /// Typed view of contained preconditions as elements.
    /// </summary>
    public IEnumerable<ISubmodelElement> PreconditionsList => Preconditions;

    /// <summary>
    /// Typed view of contained final result data as elements.
    /// </summary>
    public IEnumerable<ISubmodelElement> FinalResultDataList => FinalResultData;

    /// <summary>
    /// Typed view of contained effects as elements.
    /// </summary>
    public IEnumerable<ISubmodelElement> EffectsList => Effects;

    /// <summary>
    /// Typed view of contained postconditions as elements.
    /// </summary>
    public IEnumerable<ISubmodelElement> PostconditionsList => Postconditions;

    public void SetCost(double amount)
    {
        Cost.Value = new PropertyValue<double>(amount);
    }

    public void SetSequencePlacement(string placement)
    {
        SequencePlacement.Value = new PropertyValue<string>(placement ?? string.Empty);
    }
}
