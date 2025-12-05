using System;
using System.Linq;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;
using Xunit;

namespace AasSharpClient.Tests;

public class SchedulingContainerParityTests
{
    [Fact]
    public void ConstructorCopiesStepSchedulingToInitial()
    {
        var scheduling = new SchedulingContainer("2025-12-05 08:00:00", "2025-12-05 09:00:00", "00:05:00", "00:55:00");
        var step = new Step("StepX", "t", StepStatusEnum.OPEN, (AasSharpClient.Models.Action?)null, "", scheduling, "", "");

        var plan = new ProductionPlan(false, 1, step);

        var container = new SchedulingContainer(plan, step);

        // read initial scheduling start from the sub-collection
        var initStart = container.InitialScheduling.OfType<Property<string>>().FirstOrDefault(p => p.IdShort == "StartDateTime");
        Assert.NotNull(initStart);
        Assert.Equal("2025-12-05 08:00:00", initStart.Value?.Value?.ToString());
    }
}
