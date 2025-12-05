using System.Collections.Generic;
using System.Linq;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;
using Xunit;
using ProductionAction = AasSharpClient.Models.Action;

namespace AasSharpClient.Tests;

public class ProductionPlanApiTests
{
    [Fact]
    public void GettersAndPendingActionsReflectCurrentState()
    {
        var plan = new ProductionPlan(false, 5);
        var openAction = CreateAction("Action001", ActionStatusEnum.OPEN);
        var doneAction = CreateAction("Action002", ActionStatusEnum.DONE);
        var step = CreateStep("Step001", StepStatusEnum.PLANNED, openAction, doneAction);
        plan.append_step(step);

        Assert.Same(step, plan.GetStep("Step001"));

        var pending = plan.GetPendingActions().ToList();
        Assert.Single(pending);
        Assert.Same(openAction, pending[0]);
    }

    [Fact]
    public void QuantityAndFinishedFlagsCanBeUpdated()
    {
        var plan = new ProductionPlan(false, 5);

        plan.UpdateQuantity(42);
        plan.SetFinished(true);

        Assert.Equal("42", plan.QuantityInformation.TotalNumberOfPieces.Value.Value?.ToString());
        Assert.True(plan.IsCompleted());

        plan.SetFinished(false);
        Assert.False(plan.IsCompleted());
    }

    [Fact]
    public void StepsCanBeInsertedAndRemoved()
    {
        var plan = new ProductionPlan(false, 5);
        var first = CreateStep("Step001", StepStatusEnum.OPEN);
        var second = CreateStep("Step002", StepStatusEnum.OPEN);
        var third = CreateStep("Step003", StepStatusEnum.OPEN);

        plan.append_step(first);
        plan.append_step(third);
        plan.InsertStep(1, second);

        Assert.Equal(new[] { first, second, third }, plan.Steps);

        var removed = plan.RemoveStep("Step002");
        Assert.True(removed);
        Assert.Equal(new[] { first, third }, plan.Steps);
    }

    [Fact]
    public void StepActionAccessorsAndSchedulingUpdatesWork()
    {
        var step = CreateStep("Step001", StepStatusEnum.OPEN,
            CreateAction("Action001", ActionStatusEnum.OPEN),
            CreateAction("Action002", ActionStatusEnum.PLANNED));

        var fetched = step.GetAction("Action002");
        Assert.NotNull(fetched);
        Assert.Equal("Action002", fetched!.IdShort);

        var openActions = step.GetActionsByStatus(ActionStatusEnum.OPEN).ToList();
        Assert.Single(openActions);
        Assert.Equal("Action001", openActions[0].IdShort);

        Assert.True(step.RemoveAction("Action001"));
        Assert.Null(step.GetAction("Action001"));

        step.UpdateScheduling("2024-01-01", "2024-01-02", "PT1H", "PT2H");
        Assert.Equal("2024-01-01", GetSchedulingValue(step, "StartDateTime"));
        Assert.Equal("PT2H", GetSchedulingValue(step, "CycleTime"));
    }

    [Fact]
    public void StepStateMutatorsReplaceCollections()
    {
        var step = CreateStep("Step001", StepStatusEnum.OPEN);

        step.SetInitialState(new Dictionary<string, string> { { "Temperature", "20" } });
        step.SetFinalState(new Dictionary<string, string> { { "Temperature", "25" } });

        Assert.Equal("20", GetStateValue(step.InitialState, "Temperature"));
        Assert.Equal("25", GetStateValue(step.FinalState, "Temperature"));
    }

    [Fact]
    public void ActionParameterHelpersManageValues()
    {
        var action = CreateAction("Action001", ActionStatusEnum.OPEN);

        action.SetInputParameter("Torque", "5Nm");
        Assert.True(action.TryGetInputParameter("Torque", out var torque));
        Assert.Equal("5Nm", torque);

        action.SetFinalResultValue("EndTime", "2024-01-01T00:00:00Z");
        var finalResult = action.FinalResultData.OfType<Property>().First(p => p.IdShort == "EndTime");
        Assert.Equal("2024-01-01T00:00:00Z", finalResult.Value.Value?.ToString());

        var referenceChain = new List<(object Key, string Value)>
        {
            ((object)ModelReferenceEnum.Submodel, "https://example.com/sm"),
            ((object)ModelReferenceEnum.SubmodelElementCollection, "Skills")
        };
        action.LinkSkillReference(referenceChain);
        Assert.NotNull(action.SkillReference.Value);
    }

    [Fact]
    public void ActionStatusUpdatesStepState()
    {
        var action1 = CreateAction("Action001", ActionStatusEnum.OPEN);
        var step = CreateStep("Step001", StepStatusEnum.OPEN, action1);

        action1.SetStatus(ActionStatusEnum.PLANNED);
        Assert.Equal(StepStatusEnum.PLANNED, step.State);

        action1.SetStatus(ActionStatusEnum.EXECUTING);
        Assert.Equal(StepStatusEnum.EXECUTING, step.State);

        var action2 = CreateAction("Action002", ActionStatusEnum.OPEN);
        step.AddAction(action2);

        action1.SetStatus(ActionStatusEnum.DONE);
        Assert.NotEqual(StepStatusEnum.DONE, step.State);

        action2.SetStatus(ActionStatusEnum.DONE);
        Assert.Equal(StepStatusEnum.DONE, step.State);
    }

    [Fact]
    public void ActionLifecycleMethodsDriveStepState()
    {
        var action = CreateAction("Action001", ActionStatusEnum.OPEN);
        var step = CreateStep("Step010", StepStatusEnum.OPEN, action);

        Assert.True(action.Schedule());
        Assert.Equal(ActionStatusEnum.PLANNED, action.State);
        Assert.Equal(StepStatusEnum.PLANNED, step.State);

        Assert.True(action.StartProduction());
        Assert.Equal(ActionStatusEnum.EXECUTING, action.State);
        Assert.Equal(StepStatusEnum.EXECUTING, step.State);

        Assert.True(action.Suspend());
        Assert.Equal(ActionStatusEnum.SUSPENDED, action.State);
        Assert.Equal(StepStatusEnum.SUSPENDED, step.State);

        Assert.True(action.Resume());
        Assert.Equal(ActionStatusEnum.EXECUTING, action.State);
        Assert.Equal(StepStatusEnum.EXECUTING, step.State);

        Assert.True(action.EndProduction());
        Assert.Equal(ActionStatusEnum.DONE, action.State);
        Assert.Equal(StepStatusEnum.DONE, step.State);
    }

    [Fact]
    public void StepEndProductionRequiresCompletedActions()
    {
        var action1 = CreateAction("Action001", ActionStatusEnum.OPEN);
        var action2 = CreateAction("Action002", ActionStatusEnum.OPEN);
        var step = CreateStep("Step020", StepStatusEnum.OPEN, action1, action2);

        Assert.True(step.Schedule());
        Assert.True(step.StartProduction());

        Assert.True(action1.Schedule());
        Assert.True(action1.StartProduction());
        Assert.True(action2.Schedule());
        Assert.True(action2.StartProduction());

        Assert.False(step.EndProduction());

        Assert.True(action1.EndProduction());
        Assert.NotEqual(StepStatusEnum.DONE, step.State);

        Assert.True(action2.EndProduction());
        Assert.Equal(StepStatusEnum.DONE, step.State);
    }

    [Fact]
    public void ProductionPlanStepLifecycleHelpersApplyTransitions()
    {
        var plan = new ProductionPlan(false, 1);
        var step = CreateStep("Step100", StepStatusEnum.OPEN, CreateAction("Action100", ActionStatusEnum.OPEN));
        plan.append_step(step);

        Assert.Same(step, plan.GetStep("Step100"));
        Assert.Equal(StepStatusEnum.OPEN, step.State);

        Assert.True(plan.ScheduleStep("Step100"));
        Assert.Equal(StepStatusEnum.PLANNED, step.State);

        Assert.True(plan.StartStepProduction("Step100"));
        Assert.Equal(StepStatusEnum.EXECUTING, step.State);

        Assert.True(plan.SuspendStep("Step100"));
        Assert.Equal(StepStatusEnum.SUSPENDED, step.State);

        Assert.True(plan.ResumeStep("Step100"));
        Assert.Equal(StepStatusEnum.EXECUTING, step.State);

        Assert.True(plan.ScheduleAction("Step100", "Action100"));
        Assert.True(plan.StartActionProduction("Step100", "Action100"));
        Assert.True(plan.CompleteAction("Step100", "Action100"));

        Assert.True(plan.CompleteStep("Step100"));
        Assert.Equal(StepStatusEnum.DONE, step.State);
    }

    [Fact]
    public void ProductionPlanActionLifecycleHelpersPropagateToSteps()
    {
        var plan = new ProductionPlan(false, 1);
        var step = CreateStep("Step200", StepStatusEnum.OPEN, CreateAction("Action200", ActionStatusEnum.OPEN));
        plan.append_step(step);

        Assert.True(plan.ScheduleAction("Step200", "Action200"));
        Assert.Equal(ActionStatusEnum.PLANNED, step.GetAction("Action200")!.State);
        Assert.Equal(StepStatusEnum.PLANNED, step.State);

        Assert.True(plan.ErrorAction("Step200", "Action200"));
        Assert.Equal(ActionStatusEnum.ERROR, step.GetAction("Action200")!.State);
        Assert.Equal(StepStatusEnum.ERROR, step.State);

        Assert.True(plan.ReturnActionToCreated("Step200", "Action200"));
        Assert.Equal(ActionStatusEnum.OPEN, step.GetAction("Action200")!.State);
        Assert.Equal(StepStatusEnum.OPEN, step.State);
    }

    private static Step CreateStep(string idShort, StepStatusEnum status, params ProductionAction[] actions)
    {
        var scheduling = new SchedulingContainer("", "", "", "");
        var firstAction = actions.FirstOrDefault();
        var step = new Step(idShort, $"Step {idShort}", status, firstAction, "Station", scheduling, "Enterprise", "WC");
        foreach (var action in actions.Skip(1))
        {
            step.AddAction(action);
        }

        return step;
    }

    private static ProductionAction CreateAction(string idShort, ActionStatusEnum status)
    {
        var referenceChain = new List<(object Key, string Value)>
        {
            ((object)ModelReferenceEnum.Submodel, "https://example.com/sm")
        };

        return new ProductionAction(idShort, $"Action {idShort}", status, new InputParameters(), new FinalResultData(),
            new SkillReference(referenceChain), "Machine");
    }

    private static string? GetSchedulingValue(Step step, string idShort)
    {
        return step.Scheduling.OfType<Property>().First(p => p.IdShort == idShort).Value.Value?.ToString();
    }

    private static string? GetStateValue(SubmodelElementCollection collection, string idShort)
    {
        return collection.OfType<Property>().First(p => p.IdShort == idShort).Value.Value?.ToString();
    }
}
