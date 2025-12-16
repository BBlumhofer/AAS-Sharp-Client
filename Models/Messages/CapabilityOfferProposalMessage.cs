using System;
using System.Globalization;
using System.Threading.Tasks;
using AasSharpClient.Models.ManufacturingSequence;
using BaSyx.Models.AdminShell;
using I40Sharp.Messaging;
using I40Sharp.Messaging.Core;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// Strongly-typed helper for publishing a capability offer proposal.
/// Encapsulates the I4.0 messaging envelope + AAS payload to keep nodes free of manual message construction.
/// </summary>
public sealed class CapabilityOfferProposalMessage
{
    public string Capability { get; }
    public string RequirementId { get; }
    public string OfferId { get; }
    public string Station { get; }
    public DateTime EarliestStartUtc { get; }
    public TimeSpan CycleTime { get; }
    public TimeSpan SetupTime { get; }
    public double Cost { get; }
    public string? ProductId { get; }
    public ManufacturingOfferedCapabilitySequence OfferedCapabilitySequence { get; }

    public CapabilityOfferProposalMessage(
        string capability,
        string requirementId,
        string offerId,
        string station,
        DateTime earliestStartUtc,
        TimeSpan cycleTime,
        TimeSpan setupTime,
        double cost,
        ManufacturingOfferedCapabilitySequence offeredCapabilitySequence,
        string? productId = null)
    {
        Capability = capability ?? string.Empty;
        RequirementId = requirementId ?? string.Empty;
        OfferId = offerId ?? string.Empty;
        Station = station ?? string.Empty;
        EarliestStartUtc = earliestStartUtc;
        CycleTime = cycleTime;
        SetupTime = setupTime;
        Cost = cost;
        ProductId = productId;
        OfferedCapabilitySequence = offeredCapabilitySequence ?? throw new ArgumentNullException(nameof(offeredCapabilitySequence));
    }

    public I40Message ToI40Message(
        string senderId,
        string senderRole,
        string receiverId,
        string? receiverRole,
        string conversationId)
    {
        if (string.IsNullOrWhiteSpace(senderId)) throw new ArgumentException("senderId required", nameof(senderId));
        if (string.IsNullOrWhiteSpace(receiverId)) throw new ArgumentException("receiverId required", nameof(receiverId));
        if (string.IsNullOrWhiteSpace(conversationId)) throw new ArgumentException("conversationId required", nameof(conversationId));

        var builder = new I40MessageBuilder()
            .From(senderId, string.IsNullOrWhiteSpace(senderRole) ? null : senderRole)
            .To(receiverId, string.IsNullOrWhiteSpace(receiverRole) ? null : receiverRole)
            .WithType(I40MessageTypes.PROPOSAL)
            .WithConversationId(conversationId)
            .AddElement(CreateStringProperty("Capability", Capability))
            .AddElement(CreateStringProperty("RequirementId", RequirementId))
            .AddElement(CreateStringProperty("OfferId", OfferId))
            .AddElement(CreateStringProperty("Station", Station))
            .AddElement(CreateStringProperty("EarliestStartUtc", EarliestStartUtc.ToString("o")))
            .AddElement(CreateStringProperty("CycleTimeMinutes", CycleTime.TotalMinutes.ToString("0.###", CultureInfo.InvariantCulture)))
            .AddElement(CreateStringProperty("SetupTimeMinutes", SetupTime.TotalMinutes.ToString("0.###", CultureInfo.InvariantCulture)))
            .AddElement(CreateStringProperty("Cost", Cost.ToString("0.##", CultureInfo.InvariantCulture)));

        if (!string.IsNullOrWhiteSpace(ProductId))
        {
            builder.AddElement(CreateStringProperty("ProductId", ProductId!));
        }

        builder.AddElement(OfferedCapabilitySequence);
        return builder.Build();
    }

    public async Task PublishAsync(
        MessagingClient client,
        string topic,
        string senderId,
        string senderRole,
        string receiverId,
        string? receiverRole,
        string conversationId)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("topic required", nameof(topic));

        var msg = ToI40Message(senderId, senderRole, receiverId, receiverRole, conversationId);
        await client.PublishAsync(msg, topic).ConfigureAwait(false);
    }

    private static Property<string> CreateStringProperty(string idShort, string value)
    {
        return new Property<string>(idShort)
        {
            Value = new PropertyValue<string>(value ?? string.Empty)
        };
    }
}
