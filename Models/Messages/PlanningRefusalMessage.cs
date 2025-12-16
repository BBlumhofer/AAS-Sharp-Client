using System;
using System.Text.Json;
using System.Threading.Tasks;
using BaSyx.Models.AdminShell;
using I40Sharp.Messaging;
using I40Sharp.Messaging.Core;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

public sealed class PlanningRefusalMessage
{
    public PlanningRefusalMessage(
        string senderId,
        string senderRole,
        string receiverId,
        string receiverRole,
        string conversationId,
        string refusalReason,
        string? failureDetail = null)
    {
        SenderId = senderId;
        SenderRole = senderRole;
        ReceiverId = receiverId;
        ReceiverRole = receiverRole;
        ConversationId = conversationId;
        RefusalReason = refusalReason;
        FailureDetail = failureDetail;
    }

    public string SenderId { get; }
    public string SenderRole { get; }
    public string ReceiverId { get; }
    public string ReceiverRole { get; }
    public string ConversationId { get; }
    public string RefusalReason { get; }
    public string? FailureDetail { get; }

    public I40Message ToI40Message()
    {
        if (string.IsNullOrWhiteSpace(SenderId)) throw new ArgumentException("SenderId missing", nameof(SenderId));
        if (string.IsNullOrWhiteSpace(SenderRole)) throw new ArgumentException("SenderRole missing", nameof(SenderRole));
        if (string.IsNullOrWhiteSpace(ReceiverId)) throw new ArgumentException("ReceiverId missing", nameof(ReceiverId));
        if (string.IsNullOrWhiteSpace(ReceiverRole)) throw new ArgumentException("ReceiverRole missing", nameof(ReceiverRole));
        if (string.IsNullOrWhiteSpace(ConversationId)) throw new ArgumentException("ConversationId missing", nameof(ConversationId));

        var builder = new I40MessageBuilder()
            .From(SenderId, SenderRole)
            .To(ReceiverId, ReceiverRole)
            .WithType(I40MessageTypes.REFUSAL)
            .WithConversationId(ConversationId);

        builder.AddElement(new Property<string>("RefusalReason")
        {
            Value = new PropertyValue<string>(JsonSerializer.Serialize(RefusalReason ?? string.Empty))
        });

        if (!string.IsNullOrWhiteSpace(FailureDetail))
        {
            builder.AddElement(new Property<string>("FailureDetail")
            {
                Value = new PropertyValue<string>(JsonSerializer.Serialize(FailureDetail!))
            });
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
