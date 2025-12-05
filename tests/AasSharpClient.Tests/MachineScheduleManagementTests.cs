using System;
using System.Linq;
using System.Threading.Tasks;
using AasSharpClient.Models;
using Xunit;

namespace AasSharpClient.Tests;

public class MachineScheduleManagementTests
{
    [Fact]
    public void AddOrUpdate_AppendsNew()
    {
        var submodel = MachineScheduleSubmodel.CreateWithIdentifier("urn:sm:machineschedule");

        var container = new SchedulingContainer();
        container.SetStartDateTime(DateTime.UtcNow);

        submodel.AddOrUpdateSchedule(container);

        Assert.Single(submodel.GetSchedules());
    }

    [Fact]
    public void AddOrUpdate_ReplacesExisting_ByReferredStep()
    {
        var submodel = MachineScheduleSubmodel.CreateWithIdentifier("urn:sm:machineschedule");

        var step = new Step("StepA", "title", StepStatusEnum.OPEN, (AasSharpClient.Models.Action?)null, "", new SchedulingContainer(), "", "");

        var original = new SchedulingContainer(step);
        original.SetStartDateTime(DateTime.UtcNow);

        submodel.AddOrUpdateSchedule(original);

        var replacement = new SchedulingContainer(step);
        replacement.SetStartDateTime(DateTime.UtcNow.AddHours(1));

        submodel.AddOrUpdateSchedule(replacement);

        var schedules = submodel.GetSchedules();
        Assert.Single(schedules);
        Assert.Same(replacement, schedules.First());
    }

    [Fact]
    public void RemoveSchedule_RemovesEntry_AndUpdatesFlags()
    {
        var submodel = MachineScheduleSubmodel.CreateWithIdentifier("urn:sm:machineschedule");

        var container = new SchedulingContainer();
        container.SetStartDateTime(DateTime.UtcNow);

        submodel.AddOrUpdateSchedule(container);
        Assert.Single(submodel.GetSchedules());

        var removed = submodel.RemoveSchedule(container);
        Assert.True(removed);
        Assert.Empty(submodel.GetSchedules());
    }

    [Fact]
    public void HasOpenTasks_IsTrue_WhenEndInFuture()
    {
        var submodel = MachineScheduleSubmodel.CreateWithIdentifier("urn:sm:machineschedule");
        var step = new Step("StepB", "title", StepStatusEnum.OPEN, (AasSharpClient.Models.Action?)null, "", new SchedulingContainer(), "", "");

        var sc = new SchedulingContainer(step);
        sc.SetEndDateTime(DateTime.UtcNow.AddHours(2));

        submodel.AddOrUpdateSchedule(sc);

        // HasOpenTasks property's value is stored inside Property.Value.Value as a primitive string/bool
        var hasOpenRaw = submodel.HasOpenTasks.Value?.Value?.ToString();
        Assert.True(bool.TryParse(hasOpenRaw, out var hasOpen) && hasOpen);
    }

    [Fact]
    public void UpdateLastTimeUpdated_SetsTimestamp()
    {
        var submodel = MachineScheduleSubmodel.CreateWithIdentifier("urn:sm:machineschedule");
        var container = new SchedulingContainer();

        submodel.AddOrUpdateSchedule(container);

        var ts = submodel.LastTimeUpdated.Value?.Value?.ToString();
        Assert.False(string.IsNullOrWhiteSpace(ts));
    }
}
