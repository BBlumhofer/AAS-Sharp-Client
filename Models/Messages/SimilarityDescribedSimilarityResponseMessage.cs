using System;
using BaSyx.Models.AdminShell;
using I40Sharp.Messaging.Core;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

public sealed class SimilarityDescribedSimilarityResponseMessage
{
    public SimilarityDescribedSimilarityResponseMessage(
        string senderId,
        string receiverId,
        string? receiverRole,
        string conversationId,
        string description1,
        string description2,
        double cosineSimilarity)
    {
        SenderId = senderId;
        ReceiverId = receiverId;
        ReceiverRole = receiverRole;
        ConversationId = conversationId;
        Description1 = description1;
        Description2 = description2;
        CosineSimilarity = cosineSimilarity;
    }

    public string SenderId { get; }
    public string ReceiverId { get; }
    public string? ReceiverRole { get; }
    public string ConversationId { get; }
    public string Description1 { get; }
    public string Description2 { get; }
    public double CosineSimilarity { get; }

    public I40Message ToI40Message()
    {
        if (string.IsNullOrWhiteSpace(SenderId)) throw new ArgumentException("SenderId missing", nameof(SenderId));
        if (string.IsNullOrWhiteSpace(ReceiverId)) throw new ArgumentException("ReceiverId missing", nameof(ReceiverId));
        if (string.IsNullOrWhiteSpace(ConversationId)) throw new ArgumentException("ConversationId missing", nameof(ConversationId));

        var builder = new I40MessageBuilder()
            .From(SenderId, "AIAgent")
            .To(ReceiverId, ReceiverRole)
            .WithType("informConfirm")
            .WithConversationId(ConversationId);

        builder.AddElement(new Property<string>("Description1_Result") { Value = new PropertyValue<string>(Description1 ?? string.Empty) });
        builder.AddElement(new Property<string>("Description2_Result") { Value = new PropertyValue<string>(Description2 ?? string.Empty) });
        builder.AddElement(new Property<double>("CosineSimilarity") { Value = new PropertyValue<double>(CosineSimilarity) });

        return builder.Build();
    }
}
