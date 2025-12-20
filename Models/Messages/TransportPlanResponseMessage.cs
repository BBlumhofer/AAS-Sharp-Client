using System;
using System.Threading.Tasks;
using AasSharpClient.Models.Messages;
using BaSyx.Models.AdminShell;
using I40Sharp.Messaging;
using I40Sharp.Messaging.Core;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

public sealed class TransportPlanResponseMessage
{
    public TransportPlanResponseMessage(
        string senderId,
        string? senderRole,
        string receiverId,
        string? receiverRole,
        string conversationId,
        TransportRequestMessage response)
    {
        SenderId = senderId;
        SenderRole = senderRole;
        ReceiverId = receiverId;
        ReceiverRole = receiverRole;
        ConversationId = conversationId;
        Response = response ?? throw new ArgumentNullException(nameof(response));
    }

    public string SenderId { get; }
    public string? SenderRole { get; }
    public string ReceiverId { get; }
    public string? ReceiverRole { get; }
    public string ConversationId { get; }
    public TransportRequestMessage Response { get; }

    public I40Message ToI40Message()
    {
        throw new InvalidOperationException("Use ToI40Message(senderId, senderRole, receiverId, receiverRole, conversationId)");
    }

    public I40Message ToI40Message(
        string senderId,
        string? senderRole,
        string receiverId,
        string? receiverRole,
        string conversationId)
    {
        if (string.IsNullOrWhiteSpace(senderId)) throw new ArgumentException("SenderId missing", nameof(senderId));
        if (string.IsNullOrWhiteSpace(receiverId)) throw new ArgumentException("ReceiverId missing", nameof(receiverId));
        if (string.IsNullOrWhiteSpace(conversationId)) throw new ArgumentException("ConversationId missing", nameof(conversationId));

        return new I40MessageBuilder()
            .From(senderId, string.IsNullOrWhiteSpace(senderRole) ? null : senderRole)
            .To(receiverId, string.IsNullOrWhiteSpace(receiverRole) ? null : receiverRole)
            .WithType(I40MessageTypes.CONSENT, I40MessageTypeSubtypes.TransportRequest)
            .WithConversationId(conversationId)
            .AddElement(Response)
            .Build();
    }

    public async Task PublishAsync(
        MessagingClient client,
        string topic,
        string senderId,
        string? senderRole,
        string receiverId,
        string? receiverRole,
        string conversationId)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("topic required", nameof(topic));

        var msg = ToI40Message(senderId, senderRole, receiverId, receiverRole, conversationId);
        await client.PublishAsync(msg, topic).ConfigureAwait(false);
    }
}
