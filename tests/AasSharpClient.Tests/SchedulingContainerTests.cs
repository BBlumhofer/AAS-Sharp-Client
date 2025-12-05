using System;
using AasSharpClient.Models;
using Xunit;

namespace AasSharpClient.Tests;

public class SchedulingContainerTests
{
    [Fact]
    public void CalculatesCycleTimeFromStartAndEnd()
    {
        var container = new SchedulingContainer("2025-01-01 08:00:00", "2025-01-01 09:30:00", "00:10:00", "00:00:00");

        container.CalculateCycleTime();

        Assert.Equal(TimeSpan.FromMinutes(90), container.GetCycleTime().GetValueOrDefault());
    }

    [Fact]
    public void NormalizesAbsoluteDatesRelativeToAnchor()
    {
        var anchor = new DateTime(2025, 05, 05, 08, 00, 00, DateTimeKind.Utc);
        var container = new SchedulingContainer("2023-01-01 01:15:30", "2023-01-01 02:00:00", "00:00:00", "00:00:00");

        container.NormalizeToAbsoluteDates(anchor);

        Assert.Equal(anchor.AddHours(1).AddMinutes(15).AddSeconds(30), container.GetStartDateTime());
        Assert.Equal(anchor.AddHours(2), container.GetEndDateTime());
    }

    [Fact]
    public void AllowedToStartStepReflectsStartTime()
    {
        var container = new SchedulingContainer("2025-01-01 08:00:00", "2025-01-01 08:05:00", "00:00:00", "00:00:00");
        var anchor = new DateTime(2025, 01, 01, 08, 05, 00, DateTimeKind.Utc);

        Assert.True(container.AllowedToStartStep(anchor));

        var futureAnchor = new DateTime(2024, 12, 31, 23, 00, 00, DateTimeKind.Utc);
        Assert.False(container.AllowedToStartStep(futureAnchor));
    }

    [Fact]
    public void DurationUntilStartReturnsRemainingTime()
    {
        var container = new SchedulingContainer("2025-01-01 10:00:00", "2025-01-01 11:00:00", "00:00:00", "00:00:00");
        var anchor = new DateTime(2025, 01, 01, 09, 45, 00, DateTimeKind.Utc);

        Assert.Equal(TimeSpan.FromMinutes(15), container.DurationUntilStart(anchor));
    }

    [Fact]
    public void SupportsTimeSpanMutators()
    {
        var container = new SchedulingContainer("", "", "", "");

        container.SetSetupTime(TimeSpan.FromMinutes(5));
        container.SetCycleTime(TimeSpan.FromMinutes(15));

        Assert.Equal(TimeSpan.FromMinutes(5), container.GetSetupTime().GetValueOrDefault());
        Assert.Equal(TimeSpan.FromMinutes(15), container.GetCycleTime().GetValueOrDefault());
    }
}
