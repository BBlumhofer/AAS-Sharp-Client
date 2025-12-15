using System;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// TransportRequestMessage - SubmodelElementCollection describing a transport request
/// that can be enriched as it flows through the dispatching/transport planning pipeline.
/// </summary>
public class TransportRequestMessage : SubmodelElementCollection
{
    public const string InstanceIdentifierIdShort = "InstanceIdentifier";
    public const string OfferedCapabilityIdentifierIdShort = "OfferedCapabilityIdentifier";
    public const string TransportStartStationIdShort = "TransportStartStation";
    public const string TransportGoalStationIdShort = "TransportGoalStation";
    public const string IdentifierTypeIdShort = "IdentifierType";
    public const string IdentifierValueIdShort = "IdentifierValue";
    public const string AmountIdShort = "Amount";
    public const string EstimatedTransportTimeIdShort = "EstimatedTransportTime";
    public const string CapabilitiesSequenceIdShort = "CapabilitiesSequence";

    public enum IdentifierTypeEnum
    {
        CarrierType,
        CarrierId,
        ProductType,
        ProductId,
        ToolType,
        ToolId
    }

    public Property<string> InstanceIdentifier { get; }
    public Property<string> OfferedCapabilityIdentifier { get; }
    public Property<string> TransportStartStation { get; }
    public Property<string> TransportGoalStation { get; }
    public Property<string> IdentifierType { get; }
    public Property<string> IdentifierValue { get; }
    public Property<int> Amount { get; }
    public Property<double> EstimatedTransportTime { get; }
    public SubmodelElementList CapabilitiesSequence { get; }

    public TransportRequestMessage(string idShort = "TransportRequest") : base(idShort)
    {
        InstanceIdentifier = CreateStringProperty(InstanceIdentifierIdShort);
        OfferedCapabilityIdentifier = CreateStringProperty(OfferedCapabilityIdentifierIdShort);
        TransportStartStation = CreateStringProperty(TransportStartStationIdShort);
        TransportGoalStation = CreateStringProperty(TransportGoalStationIdShort);
        IdentifierType = CreateStringProperty(IdentifierTypeIdShort);
        IdentifierValue = CreateStringProperty(IdentifierValueIdShort);
        Amount = new Property<int>(AmountIdShort)
        {
            Value = new PropertyValue<int>(1)
        };
        EstimatedTransportTime = new Property<double>(EstimatedTransportTimeIdShort)
        {
            Value = new PropertyValue<double>(0d)
        };
        CapabilitiesSequence = new SubmodelElementList(CapabilitiesSequenceIdShort);

        Add(InstanceIdentifier);
        Add(OfferedCapabilityIdentifier);
        Add(TransportStartStation);
        Add(TransportGoalStation);
        Add(IdentifierType);
        Add(IdentifierValue);
        Add(Amount);
        Add(EstimatedTransportTime);
        Add(CapabilitiesSequence);
    }

    public void SetIdentifierType(IdentifierTypeEnum type)
    {
        IdentifierType.Value = new PropertyValue<string>(type.ToString());
    }

    public void SetIdentifierType(string? rawType)
    {
        IdentifierType.Value = new PropertyValue<string>(rawType ?? string.Empty);
    }

    public void SetAmount(int amount)
    {
        Amount.Value = new PropertyValue<int>(Math.Max(1, amount));
    }

    public void SetEstimatedTransportTime(double seconds)
    {
        EstimatedTransportTime.Value = new PropertyValue<double>(Math.Max(0d, seconds));
    }

    private static Property<string> CreateStringProperty(string idShort)
    {
        return new Property<string>(idShort)
        {
            Value = new PropertyValue<string>(string.Empty)
        };
    }
}
