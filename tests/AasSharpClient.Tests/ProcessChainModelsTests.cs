using System;
using System.Linq;
using AasSharpClient.Models;
using AasSharpClient.Models.ProcessChain;
using BaSyx.Models.AdminShell;
using Xunit;
using ActionModel = AasSharpClient.Models.Action;

namespace AasSharpClient.Tests;

public class ProcessChainModelsTests
{
    [Fact]
    public void RequiredCapabilityStoresInstanceIdentifier()
    {
        var capability = new RequiredCapability("RequiredCapability001");
        capability.SetInstanceIdentifier("req-123");

        var storedValue = Assert.IsType<PropertyValue<string>>(capability.InstanceIdentifier.Value);
        Assert.Equal("req-123", storedValue.Value.ToObject<string>());
    }

    [Fact]
    public void RequiredCapabilityCollectsCapabilityOffers()
    {
        var capability = new RequiredCapability("RequiredCapability001");
        var offer = new OfferedCapability("Offer001");
        offer.InstanceIdentifier.Value = new PropertyValue<string>("offer-1");
        capability.AddCapabilityOffer(offer);

        var offers = capability.GetCapabilityOffers().ToList();
        Assert.Single(offers);
        var storedValue = Assert.IsType<PropertyValue<string>>(offers[0].InstanceIdentifier.Value);
        Assert.Equal("offer-1", storedValue.Value.ToObject<string>());
    }

    [Fact]
    public void CapabilityOfferStoresSchedulingAndActions()
    {
        var offer = new OfferedCapability("Offer001");
        var start = DateTime.UtcNow;
        var end = start.AddMinutes(5);
        offer.SetEarliestScheduling(start, end, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(4));

        var action = new ActionModel(
            "Action001",
            "DemoAction",
            ActionStatusEnum.PLANNED,
            inputParameters: null,
            finalResultData: null,
            preconditions: null,
            skillReference: null,
            machineName: "Module-1");

        offer.AddAction(action);

        Assert.Equal("EarliestSchedulingInformation", offer.EarliestSchedulingInformation.IdShort);
        var storedStart = offer.EarliestSchedulingInformation.GetStartDateTime();
        Assert.True(storedStart.HasValue);
        Assert.InRange((storedStart!.Value - start).TotalSeconds, -1, 1);

        Assert.Single(offer.Actions);
        var storedAction = Assert.IsType<ActionModel>(offer.Actions.First());
        var actionTitle = storedAction.ActionTitle.Value.Value.ToObject<string>();
        Assert.Equal("DemoAction", actionTitle);
    }

    [Fact]
    public void CapabilityOfferStoresCost()
    {
        var offer = new OfferedCapability("Offer001");
        offer.SetCost(123.45);

        var storedValue = Assert.IsType<PropertyValue<double>>(offer.Cost.Value);
        Assert.Equal(123.45, storedValue.Value.ToObject<double>(), 3);
    }
}
