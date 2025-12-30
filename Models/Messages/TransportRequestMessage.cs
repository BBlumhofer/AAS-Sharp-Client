using System;
using System.Linq;
using AasSharpClient.Models.Helpers;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Messages;

/// <summary>
/// TransportRequestMessage - SubmodelElementCollection describing a transport request
/// that can be enriched as it flows through the dispatching/transport planning pipeline.
/// </summary>
public class TransportRequestMessage : SubmodelElementCollection
{
    public const string InstanceIdentifierIdShort = "InstanceIdentifier";
    public const string OfferedCapabilityIdentifierIdShort = "OfferedCapabilityIdentifier";
    public const string TransportStartStationIdShort = "TransportStartStation";
    public const string TransportGoalStationIdShort = "TransportGoalStation";
    public const string IdentifierTypeIdShort = "IdentifierType";
    public const string IdentifierValueIdShort = "IdentifierValue";
    public const string AmountIdShort = "Amount";
    public const string EstimatedTransportTimeIdShort = "EstimatedTransportTime";
    public const string CapabilitiesSequenceIdShort = "CapabilitySequence";

    public enum IdentifierTypeEnum
    {
        CarrierType,
        CarrierId,
        ProductType,
        ProductId,
        ToolType,
        ToolId
    }

    public Property<string> InstanceIdentifier { get; }
    public Property<string> OfferedCapabilityIdentifier { get; }
    public Property<string> TransportStartStation { get; }
    public Property<string> TransportGoalStation { get; }
    public Property<string> IdentifierType { get; }
    public Property<string> IdentifierValue { get; }
    public Property<int> Amount { get; }
    public Property<double> EstimatedTransportTime { get; }
    public CapabilitySequence CapabilitiesSequence { get; }

    public string? InstanceIdentifierText => InstanceIdentifier?.Value?.Value?.ToString();
    public string? TransportStartStationText => TransportStartStation?.Value?.Value?.ToString();
    public string? TransportGoalStationText => TransportGoalStation?.Value?.Value?.ToString();
    public string? IdentifierTypeText => IdentifierType?.Value?.Value?.ToString();
    public string? IdentifierValueText => IdentifierValue?.Value?.Value?.ToString();
    public int AmountValue
    {
        get
        {
            return Amount.GetIntValue(0);
        }
    }

    public string IdentifierKeyText
    {
        get
        {
            var type = IdentifierTypeText;
            var value = IdentifierValueText;

            if (string.IsNullOrWhiteSpace(type) && string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                return value ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return type;
            }

            return $"{NormalizeIdentifierTypeForKey(type)}: {value}";
        }
    }

    public TransportRequestMessage(string idShort = "TransportRequest") : base(idShort)
    {
        InstanceIdentifier = CreateStringProperty(InstanceIdentifierIdShort);
        OfferedCapabilityIdentifier = CreateStringProperty(OfferedCapabilityIdentifierIdShort);
        TransportStartStation = CreateStringProperty(TransportStartStationIdShort);
        TransportGoalStation = CreateStringProperty(TransportGoalStationIdShort);
        IdentifierType = CreateStringProperty(IdentifierTypeIdShort);
        IdentifierValue = CreateStringProperty(IdentifierValueIdShort);
        Amount = new Property<int>(AmountIdShort)
        {
            Value = new PropertyValue<int>(1)
        };
        EstimatedTransportTime = new Property<double>(EstimatedTransportTimeIdShort)
        {
            Value = new PropertyValue<double>(0d)
        };
        CapabilitiesSequence = new CapabilitySequence(CapabilitiesSequenceIdShort);

        Add(InstanceIdentifier);
        Add(OfferedCapabilityIdentifier);
        Add(TransportStartStation);
        Add(TransportGoalStation);
        Add(IdentifierType);
        Add(IdentifierValue);
        Add(Amount);
        Add(EstimatedTransportTime);
        Add(CapabilitiesSequence);
    }

    /// <summary>
    /// Erzeugt eine TransportRequestMessage aus einem bestehenden BaSyx SubmodelElementCollection.
    /// Werte werden übernommen wenn vorhanden, und die Capability-Sequence wird kopiert.
    /// </summary>
    public TransportRequestMessage(SubmodelElementCollection element) : this(element?.IdShort ?? "TransportRequest")
    {
        if (element == null) return;

        // Helper to read string property value by recursively unwrapping Value properties
        static string? readProp(SubmodelElementCollection col, string idShort)
        {
            var elem = col.FirstOrDefault(e => string.Equals(e.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
            if (elem == null) return null;

            try
            {
                object? current = elem;
                // Walk up to a few levels of nested "Value" properties to find a primitive/string
                for (int depth = 0; depth < 8 && current != null; depth++)
                {
                    // If this is an IValue, return its Value
                    if (current is BaSyx.Models.AdminShell.IValue iv)
                    {
                        if (iv.Value != null) return iv.Value.ToString();
                        return null;
                    }

                    var t = current.GetType();
                    var valProp = Array.Find(t.GetProperties(), p => p.Name == "Value" && p.GetIndexParameters().Length == 0);
                    if (valProp == null) break;

                    current = valProp.GetValue(current);

                    if (current == null) break;

                    // If we reached a primitive or string, return it
                    var ct = current.GetType();
                    if (current is string || ct.IsPrimitive || ct.IsEnum || current is decimal)
                    {
                        return current.ToString();
                    }
                }
            }
            catch
            {
                // ignore and fallback
            }

            // Fallback: try the element's ToString()
            try { return elem?.ToString(); } catch { return null; }
        }

        var inst = readProp(element, InstanceIdentifierIdShort);
        if (!string.IsNullOrWhiteSpace(inst))
            InstanceIdentifier.Value = new PropertyValue<string>(inst);

        var start = readProp(element, TransportStartStationIdShort);
        if (!string.IsNullOrWhiteSpace(start))
            TransportStartStation.Value = new PropertyValue<string>(start);

        var goal = readProp(element, TransportGoalStationIdShort);
        if (!string.IsNullOrWhiteSpace(goal))
            TransportGoalStation.Value = new PropertyValue<string>(goal);

        var idType = readProp(element, IdentifierTypeIdShort);
        if (!string.IsNullOrWhiteSpace(idType))
            IdentifierType.Value = new PropertyValue<string>(idType);

        var idValue = readProp(element, IdentifierValueIdShort);
        if (!string.IsNullOrWhiteSpace(idValue))
            IdentifierValue.Value = new PropertyValue<string>(idValue);

        var amountStr = readProp(element, AmountIdShort);
        if (int.TryParse(amountStr, out var a))
            SetAmount(a);

        var est = element.FirstOrDefault(e => string.Equals(e.IdShort, EstimatedTransportTimeIdShort, StringComparison.OrdinalIgnoreCase)) as Property<double>;
        if (est?.Value?.Value != null)
        {
            var raw = est.Value.Value?.ToString();
            if (double.TryParse(raw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var dbl))
            {
                SetEstimatedTransportTime(dbl);
            }
        }

        // Copy CapabilitiesSequence items if present
        var seq = element.FirstOrDefault(e => string.Equals(e.IdShort, CapabilitiesSequenceIdShort, StringComparison.OrdinalIgnoreCase)) as SubmodelElementList;
        if (seq != null && seq.Values != null)
        {
            foreach (var child in seq.Values)
            {
                try
                {
                    CapabilitiesSequence.Add(child);
                }
                catch
                {
                    // best-effort copy; ignore items that cannot be added
                }
            }
        }
    }

    private static string NormalizeIdentifierTypeForKey(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        // Cosmetic normalization: "ProductId" -> "ProductID" (matches user-facing expectation)
        if (raw.EndsWith("Id", StringComparison.Ordinal) && raw.Length > 2)
        {
            return raw[..^2] + "ID";
        }

        return raw;
    }

    public void SetIdentifierType(IdentifierTypeEnum type)
    {
        IdentifierType.Value = new PropertyValue<string>(type.ToString());
    }

    public void SetIdentifierType(string? rawType)
    {
        IdentifierType.Value = new PropertyValue<string>(rawType ?? string.Empty);
    }

    public void SetAmount(int amount)
    {
        Amount.Value = new PropertyValue<int>(Math.Max(1, amount));
    }

    public void SetEstimatedTransportTime(double seconds)
    {
        EstimatedTransportTime.Value = new PropertyValue<double>(Math.Max(0d, seconds));
    }

    private static Property<string> CreateStringProperty(string idShort)
    {
        return new Property<string>(idShort)
        {
            Value = new PropertyValue<string>(string.Empty)
        };
    }
}
