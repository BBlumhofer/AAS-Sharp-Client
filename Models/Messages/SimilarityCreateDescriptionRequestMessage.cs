using System;
using System.Threading.Tasks;
using BaSyx.Models.AdminShell;
using I40Sharp.Messaging;
using I40Sharp.Messaging.Core;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

public sealed class SimilarityCreateDescriptionRequestMessage
{
    public SimilarityCreateDescriptionRequestMessage(
        string senderId,
        string? senderRole,
        string receiverId,
        string receiverRole,
        string conversationId,
        string capability)
    {
        SenderId = senderId;
        SenderRole = senderRole;
        ReceiverId = receiverId;
        ReceiverRole = receiverRole;
        ConversationId = conversationId;
        Capability = capability;
    }

    public string SenderId { get; }
    public string? SenderRole { get; }
    public string ReceiverId { get; }
    public string ReceiverRole { get; }
    public string ConversationId { get; }
    public string Capability { get; }

    public I40Message ToI40Message()
    {
        if (string.IsNullOrWhiteSpace(SenderId)) throw new ArgumentException("SenderId missing", nameof(SenderId));
        if (string.IsNullOrWhiteSpace(ReceiverId)) throw new ArgumentException("ReceiverId missing", nameof(ReceiverId));
        if (string.IsNullOrWhiteSpace(ReceiverRole)) throw new ArgumentException("ReceiverRole missing", nameof(ReceiverRole));
        if (string.IsNullOrWhiteSpace(ConversationId)) throw new ArgumentException("ConversationId missing", nameof(ConversationId));
        if (string.IsNullOrWhiteSpace(Capability)) throw new ArgumentException("Capability missing", nameof(Capability));

        return new I40MessageBuilder()
            .From(SenderId, SenderRole)
            .To(ReceiverId, ReceiverRole)
            .WithType("createDescription")
            .WithConversationId(ConversationId)
            .AddElement(new Property<string>("Capability_0") { Value = new PropertyValue<string>(Capability.Trim()) })
            .Build();
    }

    public async Task PublishAsync(MessagingClient client, string topic)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Topic missing", nameof(topic));

        await client.PublishAsync(ToI40Message(), topic).ConfigureAwait(false);
    }
}
