using System;
using System.Threading.Tasks;
using BaSyx.Models.AdminShell;
using I40Sharp.Messaging;
using I40Sharp.Messaging.Core;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

public sealed class SimilarityCalcSimilarityRequestMessage
{
    public SimilarityCalcSimilarityRequestMessage(
        string senderId,
        string? senderRole,
        string receiverId,
        string receiverRole,
        string conversationId,
        string description1,
        string description2)
    {
        SenderId = senderId;
        SenderRole = senderRole;
        ReceiverId = receiverId;
        ReceiverRole = receiverRole;
        ConversationId = conversationId;
        Description1 = description1;
        Description2 = description2;
    }

    public string SenderId { get; }
    public string? SenderRole { get; }
    public string ReceiverId { get; }
    public string ReceiverRole { get; }
    public string ConversationId { get; }
    public string Description1 { get; }
    public string Description2 { get; }

    public I40Message ToI40Message()
    {
        if (string.IsNullOrWhiteSpace(SenderId)) throw new ArgumentException("SenderId missing", nameof(SenderId));
        if (string.IsNullOrWhiteSpace(ReceiverId)) throw new ArgumentException("ReceiverId missing", nameof(ReceiverId));
        if (string.IsNullOrWhiteSpace(ReceiverRole)) throw new ArgumentException("ReceiverRole missing", nameof(ReceiverRole));
        if (string.IsNullOrWhiteSpace(ConversationId)) throw new ArgumentException("ConversationId missing", nameof(ConversationId));
        if (string.IsNullOrWhiteSpace(Description1)) throw new ArgumentException("Description1 missing", nameof(Description1));
        if (string.IsNullOrWhiteSpace(Description2)) throw new ArgumentException("Description2 missing", nameof(Description2));

        return new I40MessageBuilder()
            .From(SenderId, SenderRole)
            .To(ReceiverId, ReceiverRole)
            .WithType("calcSimilarity")
            .WithConversationId(ConversationId)
            .AddElement(new Property<string>("Description_1") { Value = new PropertyValue<string>(Description1) })
            .AddElement(new Property<string>("Description_2") { Value = new PropertyValue<string>(Description2) })
            .Build();
    }

    public async Task PublishAsync(MessagingClient client, string topic)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Topic missing", nameof(topic));

        await client.PublishAsync(ToI40Message(), topic).ConfigureAwait(false);
    }
}
