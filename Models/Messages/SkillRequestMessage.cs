using System;
using System.Threading.Tasks;
using AasSharpClient.Models;
using I40Sharp.Messaging;
using I40Sharp.Messaging.Core;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

public sealed class SkillRequestMessage
{
    public SkillRequestMessage(
        string senderId,
        string senderRole,
        string receiverId,
        string receiverRole,
        string conversationId,
        Action action)
    {
        SenderId = senderId;
        SenderRole = senderRole;
        ReceiverId = receiverId;
        ReceiverRole = receiverRole;
        ConversationId = conversationId;
        Action = action;
    }

    public string SenderId { get; }
    public string SenderRole { get; }
    public string ReceiverId { get; }
    public string ReceiverRole { get; }
    public string ConversationId { get; }
    public Action Action { get; }

    public I40Message ToI40Message()
    {
        if (string.IsNullOrWhiteSpace(SenderId)) throw new ArgumentException("SenderId missing", nameof(SenderId));
        if (string.IsNullOrWhiteSpace(SenderRole)) throw new ArgumentException("SenderRole missing", nameof(SenderRole));
        if (string.IsNullOrWhiteSpace(ReceiverId)) throw new ArgumentException("ReceiverId missing", nameof(ReceiverId));
        if (string.IsNullOrWhiteSpace(ReceiverRole)) throw new ArgumentException("ReceiverRole missing", nameof(ReceiverRole));
        if (string.IsNullOrWhiteSpace(ConversationId)) throw new ArgumentException("ConversationId missing", nameof(ConversationId));
        if (Action == null) throw new ArgumentNullException(nameof(Action));

        return new I40MessageBuilder()
            .From(SenderId, SenderRole)
            .To(ReceiverId, ReceiverRole)
            .WithType("request")
            .WithConversationId(ConversationId)
            .AddElement(Action)
            .Build();
    }

    public async Task PublishAsync(MessagingClient client, string topic)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Topic missing", nameof(topic));

        await client.PublishAsync(ToI40Message(), topic).ConfigureAwait(false);
    }
}
