Baue ein Klassenmodell in C# Sharp auf ähnlich zu dem Java beispiel. Verwende Basyx.Models Paket..

Java Teilbeispiel:
public static class States
{
    public const string NotPlanned = "open";
    public const string Planned = "planned";
    public const string Executing = "executing";
    public const string Suspended = "suspended";
    public const string Completed = "done";
    public const string Aborted = "aborted";
    public const string Error = "error";
}
using BaSyx.Models.AdminShell;
using System.Collections.Generic;

public class Action
{
    public static readonly string MODELTYPE = "Action";

    public Property<string> ActionTitle { get; private set; }
    public Property<string> Status { get; private set; }
    public SubmodelElementCollection InputParameters { get; private set; }
    public SubmodelElementCollection FinalResultData { get; private set; }
    public SubmodelElementCollection Preconditions { get; private set; }
    public SubmodelElementCollection Effects { get; private set; }
    public Reference SkillReference { get; private set; }
    public Property<string> MachineName { get; private set; }

    public Action(string idShort, Reference semanticId)
    {
        ActionTitle = new Property<string>("ActionTitle", semanticId, "");
        Status = new Property<string>("Status", semanticId, States.NotPlanned);
        InputParameters = new SubmodelElementCollection("InputParameters", semanticId);
        FinalResultData = new SubmodelElementCollection("FinalResultData", semanticId);
        Preconditions = new SubmodelElementCollection("Preconditions", semanticId);
        Effects = new SubmodelElementCollection("Effects", semanticId);
        SkillReference = new Reference(new List<IKey> { new Key("GlobalReference", "EMPTY") }, KeyTypes.ExternalReference);
        MachineName = new Property<string>("MachineName", semanticId, "");
    }
}
using BaSyx.Models.AdminShell;
using System.Collections.Generic;

public class Step
{
    public static readonly string MODELTYPE = "Step";

    public Property<string> StepTitle { get; private set; }
    public Property<string> Status { get; private set; }
    public Property<string> Station { get; private set; }
    public SubmodelElementCollection ActionsCollection { get; private set; }
    public List<Action> Actions { get; private set; }

    public Step(string idShort, Reference semanticId)
    {
        StepTitle = new Property<string>("StepTitle", semanticId, "");
        Status = new Property<string>("Status", semanticId, States.NotPlanned);
        Station = new Property<string>("Station", semanticId, "");
        ActionsCollection = new SubmodelElementCollection("Actions", semanticId);
        Actions = new List<Action>();
    }

    public void AddAction(Action action)
    {
        Actions.Add(action);
        ActionsCollection.Value.Add(action); // Fügt Action in Collection ein
    }

    public void SetStatus(string status)
    {
        Status.Value = status;
        if (status == States.Completed)
        {
            foreach (var action in Actions)
            {
                if (action.Status.Value != States.Completed)
                {
                    Status.Value = States.NotPlanned;
                    break;
                }
            }
        }
    }
}
using BaSyx.Models.AdminShell;
using System.Collections.Generic;

public class ProductionPlan
{
    public static readonly Reference MODELTYPE = new Reference(new List<IKey> {
        new Key("GlobalReference", "https://smartfactory.de/semantics/submodel/ProductionPlan#1/0")
    }, KeyTypes.ExternalReference);

    public Property<string> IsFinished { get; private set; }
    public List<Step> Steps { get; private set; }

    public ProductionPlan()
    {
        IsFinished = new Property<string>("IsFinished", MODELTYPE, "false");
        Steps = new List<Step>();
    }

    public void AddStep(Step step)
    {
        Steps.Add(step);
    }

    public void Complete()
    {
        IsFinished.Value = "true";
    }

    public bool IsCompleted()
    {
        return IsFinished.Value == "true";
    }
}
