using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AasSharpClient.Models;
using BaSyx.Models.AdminShell;
using Xunit;

namespace AasSharpClient.Tests;

public class ProductIdentificationTests
{
    [Fact]
    public async Task ProductIdentificationTemplateMatchesJson()
    {
        var data = CreateSampleData();
        var submodel = ProductIdentificationSubmodel.CreateWithIdentifier(data.SubmodelIdentifier);
        submodel.Apply(data);

        var actual = await submodel.ToJsonAsync();
        var expected = await File.ReadAllTextAsync("TestData/Test_SM_ProductIdentification.json");

        TestHelpers.AssertJsonEqual(expected, actual);
    }

    [Fact]
    public void ProductIdentificationSupportsInPlaceFieldUpdates()
    {
        var data = CreateSampleData();
        var submodel = ProductIdentificationSubmodel.CreateWithIdentifier(data.SubmodelIdentifier);
        submodel.Apply(data);

        submodel.SetProductName("UpdatedName");
        submodel.SetOrderNumber("order-001");
        submodel.SetOrderTimestamp("42");
        submodel.SetBrand("BrandX");
        submodel.SetEffectiveDate("02.02.2025");

        Assert.Equal("UpdatedName", submodel.GetProductName());
        Assert.Equal("order-001", submodel.GetOrderNumber());
        Assert.Equal("42", submodel.GetOrderTimestamp());
        Assert.Equal("BrandX", submodel.GetBrand());
        Assert.Equal("02.02.2025", submodel.GetEffectiveDate());
    }

    [Fact]
    public void ProductIdentificationManagesCargoHazardReferences()
    {
        var data = CreateSampleData();
        var submodel = ProductIdentificationSubmodel.CreateWithIdentifier(data.SubmodelIdentifier);
        submodel.Apply(data);

        Assert.True(submodel.HasCargoHazardReference());
        var initialReference = ReferenceFactory.Model(data.CargoHazardReferenceKeys.ToArray());
        Assert.True(submodel.CargoHazardReferenceEquals(initialReference));

        var newReferenceKeys = new List<(KeyType Type, string Value)>
        {
            (KeyType.Submodel, "https://smartfactory.de/sm/new"),
            (KeyType.SubmodelElementCollection, "Class1"),
            (KeyType.Property, "Class1")
        };

        submodel.SetCargoHazardReference(newReferenceKeys);

        var newReference = ReferenceFactory.Model(newReferenceKeys.ToArray());
        Assert.True(submodel.CargoHazardReferenceEquals(newReference));
    }

    private static ProductIdentificationData CreateSampleData()
    {
        return new ProductIdentificationData(
            "https://smartfactory.de/aas/sm/05ae0c14-cbd5-4340-b41b-c11e601165a8",
            "sf_aas_8163_8721_9287_5326",
            "Battery_Pack",
            "Battery_Pack",
            "GER",
            "sf_order_7osdf9",
            "1701762007560",
            "SmartFactoryKL - Shared Production",
            "01.01.2022",
            new List<(KeyType Type, string Value)>
            {
                (KeyType.Submodel, "https://smartfactory.de/sm/d3833b2a-5503-40c7-a460-53892ffb2784"),
                (KeyType.SubmodelElementCollection, "Class0"),
                (KeyType.Property, "Class0")
            });
    }
}
