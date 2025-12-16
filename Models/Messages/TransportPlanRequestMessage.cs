using System;
using System.Threading.Tasks;
using AasSharpClient.Models.Messages;
using BaSyx.Models.AdminShell;
using I40Sharp.Messaging;
using I40Sharp.Messaging.Core;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

public sealed class TransportPlanRequestMessage
{
    public TransportPlanRequestMessage(
        string senderId,
        string? senderRole,
        string receiverId,
        string receiverRole,
        string conversationId,
        TransportRequestMessage request)
    {
        SenderId = senderId;
        SenderRole = senderRole;
        ReceiverId = receiverId;
        ReceiverRole = receiverRole;
        ConversationId = conversationId;
        Request = request;
    }

    public string SenderId { get; }
    public string? SenderRole { get; }
    public string ReceiverId { get; }
    public string ReceiverRole { get; }
    public string ConversationId { get; }
    public TransportRequestMessage Request { get; }

    public I40Message ToI40Message()
    {
        if (string.IsNullOrWhiteSpace(SenderId)) throw new ArgumentException("SenderId missing", nameof(SenderId));
        if (string.IsNullOrWhiteSpace(ReceiverId)) throw new ArgumentException("ReceiverId missing", nameof(ReceiverId));
        if (string.IsNullOrWhiteSpace(ReceiverRole)) throw new ArgumentException("ReceiverRole missing", nameof(ReceiverRole));
        if (string.IsNullOrWhiteSpace(ConversationId)) throw new ArgumentException("ConversationId missing", nameof(ConversationId));
        if (Request == null) throw new ArgumentNullException(nameof(Request));

        return new I40MessageBuilder()
            .From(SenderId, SenderRole)
            .To(ReceiverId, ReceiverRole)
            .WithType(I40MessageTypes.REQUIREMENT, I40MessageTypeSubtypes.TransportRequest)
            .WithConversationId(ConversationId)
            .AddElement(Request)
            .Build();
    }

    public async Task PublishAsync(MessagingClient client, string topic)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Topic missing", nameof(topic));

        await client.PublishAsync(ToI40Message(), topic).ConfigureAwait(false);
    }
}
