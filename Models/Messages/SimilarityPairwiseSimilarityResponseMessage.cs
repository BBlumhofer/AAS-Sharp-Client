using System;
using System.Collections.Generic;
using System.Linq;
using BaSyx.Models.AdminShell;
using I40Sharp.Messaging.Core;
using I40Sharp.Messaging.Models;

namespace AasSharpClient.Models.Messages;

public sealed class SimilarityPairwiseSimilarityResponseMessage
{
    public SimilarityPairwiseSimilarityResponseMessage(
        string senderId,
        string receiverId,
        string? receiverRole,
        string conversationId,
        IReadOnlyList<(string ElementA, string ElementB, double Similarity)> pairs)
    {
        SenderId = senderId;
        ReceiverId = receiverId;
        ReceiverRole = receiverRole;
        ConversationId = conversationId;
        Pairs = pairs;
    }

    public string SenderId { get; }
    public string ReceiverId { get; }
    public string? ReceiverRole { get; }
    public string ConversationId { get; }
    public IReadOnlyList<(string ElementA, string ElementB, double Similarity)> Pairs { get; }

    public I40Message ToI40Message()
    {
        if (string.IsNullOrWhiteSpace(SenderId)) throw new ArgumentException("SenderId missing", nameof(SenderId));
        if (string.IsNullOrWhiteSpace(ReceiverId)) throw new ArgumentException("ReceiverId missing", nameof(ReceiverId));
        if (string.IsNullOrWhiteSpace(ConversationId)) throw new ArgumentException("ConversationId missing", nameof(ConversationId));
        if (Pairs == null) throw new ArgumentNullException(nameof(Pairs));

        var builder = new I40MessageBuilder()
            .From(SenderId, "AIAgent")
            .To(ReceiverId, ReceiverRole)
            .WithType("informConfirm")
            .WithConversationId(ConversationId);

        var sorted = Pairs.OrderByDescending(p => p.Similarity).ToList();
        if (sorted.Count > 0)
        {
            builder.AddElement(new Property<double>("CosineSimilarity") { Value = new PropertyValue<double>(sorted[0].Similarity) });
        }

        var matrix = new SubmodelElementCollection("SimilarityMatrix");
        for (var i = 0; i < sorted.Count; i++)
        {
            var p = sorted[i];
            var pairCol = new SubmodelElementCollection($"Pair_{i}");
            pairCol.Add(new Property<string>("ElementA") { Value = new PropertyValue<string>(p.ElementA) });
            pairCol.Add(new Property<string>("ElementB") { Value = new PropertyValue<string>(p.ElementB) });
            pairCol.Add(new Property<double>("Similarity") { Value = new PropertyValue<double>(p.Similarity) });
            matrix.Add(pairCol);
        }

        builder.AddElement(matrix);
        return builder.Build();
    }
}
