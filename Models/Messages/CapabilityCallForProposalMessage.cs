using System;
using System.Threading.Tasks;
using BaSyx.Models.AdminShell;
using I40Sharp.Messaging;
using I40Sharp.Messaging.Core;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

public sealed class CapabilityCallForProposalMessage
{
    public CapabilityCallForProposalMessage(
        string senderId,
        string? senderRole,
        string receiverId,
        string receiverRole,
        string conversationId,
        I40MessageTypeSubtypes subtype,
        string capability,
        string requirementId,
        string? productId = null,
        string? capabilityDescription = null,
        SubmodelElementCollection? capabilityContainer = null)
    {
        SenderId = senderId;
        SenderRole = senderRole;
        ReceiverId = receiverId;
        ReceiverRole = receiverRole;
        ConversationId = conversationId;
        Subtype = subtype;
        Capability = capability;
        RequirementId = requirementId;
        ProductId = productId;
        CapabilityDescription = capabilityDescription;
        CapabilityContainer = capabilityContainer;
    }

    public string SenderId { get; }
    public string? SenderRole { get; }
    public string ReceiverId { get; }
    public string ReceiverRole { get; }
    public string ConversationId { get; }
    public I40MessageTypeSubtypes Subtype { get; }
    public string Capability { get; }
    public string RequirementId { get; }
    public string? ProductId { get; }
    public string? CapabilityDescription { get; }
    public SubmodelElementCollection? CapabilityContainer { get; }

    public I40Message ToI40Message()
    {
        if (string.IsNullOrWhiteSpace(SenderId)) throw new ArgumentException("SenderId missing", nameof(SenderId));
        if (string.IsNullOrWhiteSpace(ReceiverId)) throw new ArgumentException("ReceiverId missing", nameof(ReceiverId));
        if (string.IsNullOrWhiteSpace(ReceiverRole)) throw new ArgumentException("ReceiverRole missing", nameof(ReceiverRole));
        if (string.IsNullOrWhiteSpace(ConversationId)) throw new ArgumentException("ConversationId missing", nameof(ConversationId));
        if (string.IsNullOrWhiteSpace(Capability)) throw new ArgumentException("Capability missing", nameof(Capability));
        if (string.IsNullOrWhiteSpace(RequirementId)) throw new ArgumentException("RequirementId missing", nameof(RequirementId));

        var builder = new I40MessageBuilder()
            .From(SenderId, SenderRole)
            .To(ReceiverId, ReceiverRole)
            .WithType(I40MessageTypes.CALL_FOR_PROPOSAL, Subtype)
            .WithConversationId(ConversationId)
            .AddElement(new Property<string>("Capability") { Value = new PropertyValue<string>(Capability) })
            .AddElement(new Property<string>("RequirementId") { Value = new PropertyValue<string>(RequirementId) });

        if (!string.IsNullOrWhiteSpace(ProductId))
        {
            builder.AddElement(new Property<string>("ProductId") { Value = new PropertyValue<string>(ProductId!) });
        }

        if (!string.IsNullOrWhiteSpace(CapabilityDescription))
        {
            builder.AddElement(new Property<string>("CapabilityDescription") { Value = new PropertyValue<string>(CapabilityDescription!) });
        }

        if (CapabilityContainer != null)
        {
            builder.AddElement(CapabilityContainer);
        }

        return builder.Build();
    }

    public async Task PublishAsync(MessagingClient client, string topic)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Topic missing", nameof(topic));

        await client.PublishAsync(ToI40Message(), topic).ConfigureAwait(false);
    }
}
