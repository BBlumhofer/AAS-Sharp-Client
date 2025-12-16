using System;
using System.IO;
using Xunit;
using AasSharpClient.Models.Messages;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Tests
{
    public class TransportRequestMessageTests
    {
        [Fact]
        public void TransportRequestMessage_ConstructsFromSubmodelElementCollection_File()
        {
            var coll = BasyxJsonLoader.LoadCollectionFromFile("TestTransportPlan.json");
            Assert.NotNull(coll);

            var req = new TransportRequestMessage(coll!);

            // InstanceIdentifier
            var inst = req.InstanceIdentifier?.Value?.Value?.ToString();
            Assert.Equal("dc05a527-7a63-499a-81ac-1fe49ac061c5:1", inst);

            // Goal station
            var goal = req.TransportGoalStation?.Value?.Value?.ToString();
            Assert.Equal("Screw", goal);

            // Identifier type & value
            var idType = req.IdentifierType?.Value?.Value?.ToString();
            Assert.Equal("ProductId", idType);

            var idVal = req.IdentifierValue?.Value?.Value?.ToString();
            Assert.Equal("https://smartfactory.de/shells/LG3JsASu4_", idVal);

            // Amount
            Assert.Equal(1, req.AmountValue);
        }
    }
}
