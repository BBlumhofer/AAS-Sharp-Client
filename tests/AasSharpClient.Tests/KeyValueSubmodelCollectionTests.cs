using System.Collections.Generic;
using System.Linq;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;
using Xunit;

namespace AasSharpClient.Tests;

public class KeyValueSubmodelCollectionTests
{
    [Fact]
    public void SetParameter_UpsertsValues()
    {
        var inputs = new InputParameters(new Dictionary<string, string>
        {
            { "Torque", "5" }
        });

        inputs.SetParameter("Torque", "10");
        inputs.SetParameter("Speed", "2000");

        Assert.Equal(2, inputs.Parameters.Count);
        Assert.True(inputs.Parameters.TryGetValue("Torque", out var torque));
        Assert.Equal("10", torque.Value.Value?.ToString());
    }

    [Fact]
    public void TryGetParameterValue_ReturnsTypedValue()
    {
        var inputs = new InputParameters();
        inputs.SetParameter("Torque", "5");

        var result = inputs.TryGetParameterValue("Torque", out string? value);

        Assert.True(result);
        Assert.Equal("5", value);
    }

    [Fact]
    public void RemoveParameter_ClearsCollection()
    {
        var inputs = new InputParameters();
        inputs.SetParameter("Torque", "5");

        var removed = inputs.RemoveParameter("Torque");

        Assert.True(removed);
        Assert.Empty(inputs.Parameters);
        Assert.Null(inputs.GetParameter("Torque"));
    }

    [Fact]
    public void FinalResultData_AssignsSemanticIds()
    {
        var results = new FinalResultData();
        results.SetParameter("EndTime", "2025-12-05");
        results.SetParameter("StartTime", "2025-12-04");
        results.SetParameter("Temperature", 42);

        var end = results.GetParameter("EndTime");
        var start = results.GetParameter("StartTime");
        var other = results.GetParameter("Temperature");

        Assert.NotNull(end?.SemanticId);
        Assert.NotNull(start?.SemanticId);
        Assert.NotNull(other?.SemanticId);

        var endKey = Assert.Single(end!.SemanticId!.Keys);
        Assert.Equal(KeyType.GlobalReference, endKey.Type);
        Assert.Equal("https://smartfactory.de/semantics/submodel-element/Step/Actions/Action/FinalResultData/EndTime", endKey.Value);

        var startKey = Assert.Single(start!.SemanticId!.Keys);
        Assert.Equal("https://smartfactory.de/semantics/submodel-element/Step/Actions/Action/FinalResultData/StartTime", startKey.Value);

        Assert.Empty(other!.SemanticId!.Keys);
    }
}
