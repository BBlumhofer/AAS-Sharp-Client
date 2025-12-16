using System;
using BaSyx.Models.AdminShell;
using I40Sharp.Messaging.Core;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

public sealed class SimilarityCreateDescriptionResponseMessage
{
    public SimilarityCreateDescriptionResponseMessage(
        string senderId,
        string receiverId,
        string? receiverRole,
        string conversationId,
        string description)
    {
        SenderId = senderId;
        ReceiverId = receiverId;
        ReceiverRole = receiverRole;
        ConversationId = conversationId;
        Description = description;
    }

    public string SenderId { get; }
    public string ReceiverId { get; }
    public string? ReceiverRole { get; }
    public string ConversationId { get; }
    public string Description { get; }

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

        builder.AddElement(new Property<string>("Description_Result") { Value = new PropertyValue<string>(Description ?? string.Empty) });
        return builder.Build();
    }
}
