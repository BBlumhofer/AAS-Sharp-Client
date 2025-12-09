
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using BaSyx.Models.AdminShell;
using SubmodelFactory = AasSharpClient.Models.SubmodelElementFactory;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// Lightweight ActionQueue message that publishes the current queue snapshot to other agents.
/// </summary>
public class ActionQueueMessage : SubmodelElementCollection
{
    private readonly List<ActionQueueEntry> _actions = new();
    private readonly IReadOnlyList<ActionQueueEntry> _actionsView;
    private string _moduleId;
    private string _reason;
    private string _changedConversationId;
    private DateTime _publishedAtUtc;

    public IReadOnlyList<ActionQueueEntry> Actions => _actionsView;

    public ActionQueueMessage(
        string moduleId,
        string reason,
        string changedConversationId,
        DateTime publishedAtUtc,
        IEnumerable<ActionQueueEntry>? entries = null)
        : base("ActionQueue")
    {
        _actionsView = new ReadOnlyCollection<ActionQueueEntry>(_actions);
        _moduleId = moduleId ?? string.Empty;
        _reason = reason ?? string.Empty;
        _changedConversationId = changedConversationId ?? string.Empty;
        _publishedAtUtc = publishedAtUtc;

        if (entries != null)
        {
            foreach (var entry in entries)
            {
                _actions.Add(entry.Clone());
            }
        }

        RebuildStructure();
    }

    public void AddAction(ActionQueueEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _actions.Add(entry.Clone());
        TouchTimestamp();
        RebuildStructure();
    }

    public bool RemoveAction(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return false;
        }

        var removed = _actions.RemoveAll(a =>
            string.Equals(a.ConversationId, conversationId, StringComparison.OrdinalIgnoreCase));

        if (removed == 0)
        {
            return false;
        }

        TouchTimestamp();
        RebuildStructure();
        return true;
    }

    public bool UpdateAction(ActionQueueEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (string.IsNullOrWhiteSpace(entry.ConversationId))
        {
            return false;
        }

        var index = _actions.FindIndex(existing =>
            string.Equals(existing.ConversationId, entry.ConversationId, StringComparison.OrdinalIgnoreCase));

        if (index < 0)
        {
            return false;
        }

        _actions[index] = entry.Clone();
        TouchTimestamp();
        RebuildStructure();
        return true;
    }

    public ActionQueueEntry? FindAction(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return null;
        }

        return _actions.FirstOrDefault(a =>
            string.Equals(a.ConversationId, conversationId, StringComparison.OrdinalIgnoreCase))?.Clone();
    }

    private void TouchTimestamp()
    {
        _publishedAtUtc = DateTime.UtcNow;
    }

    private void RebuildStructure()
    {
        Clear();
        Add(SubmodelFactory.CreateProperty("QueueLength", _actions.Count));
        Add(SubmodelFactory.CreateStringProperty("PublishedAtUtc", FormatTimestamp(_publishedAtUtc)));

        var actionsCollection = new SubmodelElementCollection("Actions");
        for (var i = 0; i < _actions.Count; i++)
        {
            var action = _actions[i].Clone();
            action.QueuePosition = i;
            actionsCollection.Add(action.ToSubmodelElementCollection());
        }

        Add(actionsCollection);
    }

    private static string FormatTimestamp(DateTime? value)
    {
        return value.HasValue
            ? value.Value.ToString("o", CultureInfo.InvariantCulture)
            : string.Empty;
    }

    public static List<ISubmodelElement> CreateInteractionElements(
        string moduleId,
        string reason,
        string changedConversationId,
        DateTime publishedAtUtc,
        IEnumerable<ActionQueueEntry>? entries = null)
    {
        return new List<ISubmodelElement>
        {
            new ActionQueueMessage(moduleId, reason, changedConversationId, publishedAtUtc, entries)
        };
    }

    public static ActionQueueSnapshot? ReadSnapshot(IEnumerable<ISubmodelElement>? interactionElements)
    {
        var queueCollection = FindQueueCollection(interactionElements);
        return queueCollection == null ? null : ActionQueueSnapshot.FromCollection(queueCollection);
    }

    public static ActionQueueEntry? FindEntryByConversationId(
        IEnumerable<ISubmodelElement>? interactionElements,
        string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return null;
        }

        return ReadSnapshot(interactionElements)?.Entries
            .FirstOrDefault(entry => string.Equals(entry.ConversationId, conversationId, StringComparison.OrdinalIgnoreCase));
    }

    public static bool HasConversationId(
        IEnumerable<ISubmodelElement>? interactionElements,
        string conversationId)
    {
        return FindEntryByConversationId(interactionElements, conversationId) != null;
    }

    private static SubmodelElementCollection? FindQueueCollection(IEnumerable<ISubmodelElement>? interactionElements)
    {
        return (interactionElements ?? Array.Empty<ISubmodelElement>())
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(c => c.IdShort == "ActionQueue");
    }
}

public class ActionQueueSnapshot
{
    public string ModuleId { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string ChangedConversationId { get; init; } = string.Empty;
    public int QueueLength { get; init; }
    public DateTime? PublishedAtUtc { get; init; }
    public IReadOnlyList<ActionQueueEntry> Entries { get; init; } = Array.Empty<ActionQueueEntry>();

    internal static ActionQueueSnapshot FromCollection(SubmodelElementCollection collection)
    {
        var actionsCollection = collection.FindChildCollection("Actions");
        var entries = actionsCollection == null
            ? new List<ActionQueueEntry>()
            : actionsCollection.Children
                .OfType<SubmodelElementCollection>()
                .Select(ActionQueueEntry.FromSubmodelElementCollection)
                .OrderBy(e => e.QueuePosition)
                .ToList();

        return new ActionQueueSnapshot
        {
            QueueLength = collection.ReadIntProperty("QueueLength"),
            PublishedAtUtc = collection.ReadDateTimeProperty("PublishedAtUtc"),
            Entries = entries
        };
    }
}

public class ActionQueueEntry
{
    public int QueuePosition { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string ActionTitle { get; set; } = string.Empty;
    public string ActionState { get; set; } = string.Empty;
    public DateTime EnqueuedAtUtc { get; set; }
    public DateTime? ScheduledAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }

    internal SubmodelElementCollection ToSubmodelElementCollection()
    {
        var collection = new SubmodelElementCollection($"Action{QueuePosition}");
        collection.Add(SubmodelFactory.CreateProperty("QueuePosition", QueuePosition));
        collection.Add(SubmodelFactory.CreateStringProperty("ConversationId", ConversationId));
        collection.Add(SubmodelFactory.CreateStringProperty("ActionTitle", ActionTitle));
        collection.Add(SubmodelFactory.CreateStringProperty("ActionState", ActionState));
        collection.Add(SubmodelFactory.CreateStringProperty("EnqueuedAtUTC", FormatTimestamp(EnqueuedAtUtc)));
        collection.Add(SubmodelFactory.CreateStringProperty("ScheduledAtUTC", FormatTimestamp(ScheduledAtUtc ?? EnqueuedAtUtc)));
        collection.Add(SubmodelFactory.CreateStringProperty("StartedAtUTC", FormatTimestamp(StartedAtUtc)));
        return collection;
    }

    internal static ActionQueueEntry FromSubmodelElementCollection(SubmodelElementCollection collection)
    {
        return new ActionQueueEntry
        {
            QueuePosition = collection.ReadIntProperty("QueuePosition"),
            ConversationId = collection.ReadStringProperty("ConversationId"),
            ActionTitle = collection.ReadStringProperty("ActionTitle"),
            ActionState = collection.ReadStringProperty("ActionState"),
            EnqueuedAtUtc = collection.ReadDateTimeProperty("EnqueuedAtUTC") ?? DateTime.MinValue,
            ScheduledAtUtc = collection.ReadDateTimeProperty("ScheduledAtUTC"),
            StartedAtUtc = collection.ReadDateTimeProperty("StartedAtUTC")
        };
    }

    public ActionQueueEntry Clone()
    {
        return new ActionQueueEntry
        {
            QueuePosition = QueuePosition,
            ConversationId = ConversationId,
            ActionTitle = ActionTitle,
            ActionState = ActionState,
            EnqueuedAtUtc = EnqueuedAtUtc,
            ScheduledAtUtc = ScheduledAtUtc,
            StartedAtUtc = StartedAtUtc
        };
    }

    private static string FormatTimestamp(DateTime? value)
    {
        return value.HasValue
            ? value.Value.ToString("o", CultureInfo.InvariantCulture)
            : string.Empty;
    }
}

internal static class ActionQueueMessageExtensions
{
    public static string ReadStringProperty(this SubmodelElementCollection collection, string idShort)
    {
        var property = collection.Children.OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == idShort);
        return property?.Value?.Value?.ToObject<string>() ?? string.Empty;
    }

    public static int ReadIntProperty(this SubmodelElementCollection collection, string idShort)
    {
        var property = collection.Children.OfType<IProperty>()
            .FirstOrDefault(p => p.IdShort == idShort);
        return property?.Value?.Value?.ToObject<int>() ?? 0;
    }

    public static DateTime? ReadDateTimeProperty(this SubmodelElementCollection collection, string idShort)
    {
        var rawValue = collection.ReadStringProperty(idShort);
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        return DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : null;
    }

    public static SubmodelElementCollection? FindChildCollection(
        this SubmodelElementCollection collection,
        string idShort)
    {
        return collection.Children
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(child => child.IdShort == idShort);
    }
}
