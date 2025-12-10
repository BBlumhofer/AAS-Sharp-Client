using System.Collections.Generic;
using BaSyx.Models.AdminShell;
using AasSharpClient.Models;
using Xunit;

namespace AasSharpClient.Tests;

public class ProductionPlanEnrichmentTests
{
    [Fact]
    public void FillSteps_ResolvesInputParametersFromSubmodel()
    {
        // create referenced submodel with a property
        var referenced = new Submodel("RefSm", new Identifier("ref-1"));
        referenced.SemanticId = ReferenceFactory.External((KeyType.GlobalReference, "RefSm"));
        var temperature = AasSharpClient.Models.SubmodelElementFactory.CreateProperty("Temperature", "42", null, "xs:integer");
        referenced.SubmodelElements.Add(temperature);

        // create action with input parameter referencing the submodel property
        var action = new AasSharpClient.Models.Action("Action001", "Action 1", ActionStatusEnum.OPEN, null, null, null, null, "MachineA");
        action.SetInputParameter("Temp", "RefSm/Temperature");

        var step = new Step("Step001", "Step 1", StepStatusEnum.OPEN, action, "Station1", new SchedulingContainer(), "Ent", "WC");
        var plan = new ProductionPlan(false, 1, step);

        // perform enrichment
        plan.FillSteps(referenced);

        // after fill, input parameter should have been replaced with referenced value
        Assert.True(action.TryGetInputParameter("Temp", out var resolved));
        Assert.Equal("42", resolved);
    }
}
