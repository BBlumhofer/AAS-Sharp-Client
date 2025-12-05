using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models;

public abstract class KeyValueSubmodelCollection : SubmodelElementCollection
{
    private readonly Dictionary<string, Property> _parameters = new(StringComparer.OrdinalIgnoreCase);

    protected KeyValueSubmodelCollection(string idShort, Reference semanticId)
        : base(idShort)
    {
        SemanticId = semanticId;
    }

    public IReadOnlyDictionary<string, Property> Parameters => _parameters;

    public void SetParameter(string key, object? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (SubmodelElementFactory.CreateProperty(key, value, ResolveSemanticId(key)) is not Property property)
        {
            return;
        }

        AddOrReplaceParameter(property);
    }

    public bool RemoveParameter(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || !_parameters.TryGetValue(key, out var existing))
        {
            return false;
        }

        _parameters.Remove(key);
        Remove(existing);
        return true;
    }

    public Property? GetParameter(string key)
    {
        return key != null && _parameters.TryGetValue(key, out var property) ? property : null;
    }

    public bool TryGetParameterValue<T>(string key, out T? value)
    {
        value = default;

        if (key == null || !_parameters.TryGetValue(key, out var property))
        {
            return false;
        }

        var raw = ExtractRawValue(property);
        if (raw is null)
        {
            return false;
        }

        if (raw is T typedValue)
        {
            value = typedValue;
            return true;
        }

        try
        {
            var converted = (T?)Convert.ChangeType(raw, typeof(T), CultureInfo.InvariantCulture);
            if (converted is not null || typeof(T).IsValueType)
            {
                value = converted;
                return true;
            }
        }
        catch
        {
            // ignored
        }

        return false;
    }

    protected virtual Reference? ResolveSemanticId(string key) => SemanticReferences.EmptyExternal;

    private static object? ExtractRawValue(Property property)
    {
        var value = property.Value?.Value;
        if (value is IValue inner)
        {
            return inner.Value;
        }

        return value;
    }

    private void AddOrReplaceParameter(Property property)
    {
        if (_parameters.TryGetValue(property.IdShort, out var existing))
        {
            Remove(existing);
        }

        _parameters[property.IdShort] = property;
        Add(property);
    }
}

public class InputParameters : KeyValueSubmodelCollection
{
    public InputParameters(IDictionary<string, string>? values = null)
        : base("InputParameters", SemanticReferences.ActionInputParameters)
    {
        if (values == null)
        {
            return;
        }

        foreach (var (key, value) in values)
        {
            SetParameter(key, value);
        }
    }
}

public class FinalResultData : KeyValueSubmodelCollection
{
    public FinalResultData(IDictionary<string, object>? values = null)
        : base("FinalResultData", SemanticReferences.ActionFinalResultData)
    {
        if (values == null)
        {
            return;
        }

        foreach (var (key, value) in values)
        {
            SetParameter(key, value);
        }
    }

    protected override Reference? ResolveSemanticId(string key)
    {
        return key switch
        {
            "EndTime" => SemanticReferences.ActionFinalResultDataEndTime,
            "StartTime" => SemanticReferences.ActionFinalResultDataStartTime,
            _ => SemanticReferences.EmptyExternal
        };
    }
}

public class SchedulingContainer : SubmodelElementCollection
{
    public ReferenceElement? ReferredStep { get; private set; }
    private const string SchedulingIdShort = "Scheduling";
    private const string StartDateTimeId = "StartDateTime";
    private const string EndDateTimeId = "EndDateTime";
    private const string SetupTimeId = "SetupTime";
    private const string CycleTimeId = "CycleTime";
    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    private const string TimeFormat = "HH:mm:ss";

    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    private readonly Property<string> _startDateTime;
    private readonly Property<string> _endDateTime;
    private readonly Property<string> _setupTime;
    private readonly Property<string> _cycleTime;
    public SubmodelElementCollection InitialScheduling { get; }
    public SubmodelElementCollection ActualScheduling { get; }

    public SchedulingContainer(string startDateTime, string endDateTime, string setupTime, string cycleTime)
        : this()
    {
        SetStartDateTimeRaw(startDateTime);
        SetEndDateTimeRaw(endDateTime);
        SetSetupTimeRaw(setupTime);
        SetCycleTimeRaw(cycleTime);
    }

    internal SchedulingContainer()
        : base(SchedulingIdShort)
    {
        SemanticId = SemanticReferences.StepScheduling;

        _startDateTime = CreateSchedulingProperty(StartDateTimeId, SemanticReferences.SchedulingStartDateTime);
        _endDateTime = CreateSchedulingProperty(EndDateTimeId, SemanticReferences.SchedulingEndDateTime);
        _setupTime = CreateSchedulingProperty(SetupTimeId, SemanticReferences.SchedulingSetupTime);
        _cycleTime = CreateSchedulingProperty(CycleTimeId, SemanticReferences.SchedulingCycleTime);

        Add(_startDateTime);
        Add(_endDateTime);
        Add(_setupTime);
        Add(_cycleTime);

        InitialScheduling = new SubmodelElementCollection("InitialScheduling")
        {
            SemanticId = SemanticReferences.StepScheduling
        };

        ActualScheduling = new SubmodelElementCollection("ActualScheduling")
        {
            SemanticId = SemanticReferences.StepScheduling
        };

        // create placeholder properties inside the sub-collections
        InitialScheduling.Add(CreateSchedulingProperty(StartDateTimeId, SemanticReferences.SchedulingStartDateTime));
        InitialScheduling.Add(CreateSchedulingProperty(EndDateTimeId, SemanticReferences.SchedulingEndDateTime));
        InitialScheduling.Add(CreateSchedulingProperty(SetupTimeId, SemanticReferences.SchedulingSetupTime));
        InitialScheduling.Add(CreateSchedulingProperty(CycleTimeId, SemanticReferences.SchedulingCycleTime));

        ActualScheduling.Add(CreateSchedulingProperty(StartDateTimeId, SemanticReferences.SchedulingStartDateTime));
        ActualScheduling.Add(CreateSchedulingProperty(EndDateTimeId, SemanticReferences.SchedulingEndDateTime));
        ActualScheduling.Add(CreateSchedulingProperty(SetupTimeId, SemanticReferences.SchedulingSetupTime));
        ActualScheduling.Add(CreateSchedulingProperty(CycleTimeId, SemanticReferences.SchedulingCycleTime));

        Add(InitialScheduling);
        Add(ActualScheduling);
    }

    public SchedulingContainer(Step step)
        : this()
    {
        if (step == null) return;

        // create a lightweight reference to the step using its idShort
        ReferredStep = new ReferenceElement("ReferredStep")
        {
            Value = new ReferenceElementValue(ReferenceFactory.External((KeyType.SubmodelElementCollection, step.IdShort)))
        };

        Add(ReferredStep);

        // copy scheduling values from the step's scheduling if available
        try
        {
            var source = step.Scheduling;
            if (source != null)
            {
                var start = source.GetStartDateTime();
                var end = source.GetEndDateTime();
                var setup = source.GetSetupTime();
                var cycle = source.GetCycleTime();

                if (start.HasValue) SetStartDateTime(start.Value);
                if (end.HasValue) SetEndDateTime(end.Value);
                if (setup.HasValue) SetSetupTime(setup.Value);
                if (cycle.HasValue) SetCycleTime(cycle.Value);
            }
        }
        catch
        {
            // best-effort copy; ignore failures
        }
    }

    public SchedulingContainer(ProductionPlan plan, Step step)
        : this(step)
    {
        if (step == null) return;

        // copy the step's scheduling into the InitialScheduling sub-collection
        try
        {
            var source = step.Scheduling;
            if (source != null)
            {
                var initStart = source.GetStartDateTime();
                var initEnd = source.GetEndDateTime();
                var initSetup = source.GetSetupTime();
                var initCycle = source.GetCycleTime();

                // set values on the InitialScheduling properties if present
                void SetIf(Property<string> p, string? v)
                {
                    if (p == null) return;
                    p.Value = new PropertyValue<string>(v ?? string.Empty);
                }

                foreach (var prop in InitialScheduling.OfType<Property<string>>())
                {
                    switch (prop.IdShort)
                    {
                        case StartDateTimeId:
                            SetIf(prop, initStart.HasValue ? FormatDateTime(initStart.Value) : null);
                            break;
                        case EndDateTimeId:
                            SetIf(prop, initEnd.HasValue ? FormatDateTime(initEnd.Value) : null);
                            break;
                        case SetupTimeId:
                            SetIf(prop, initSetup.HasValue ? FormatTime(initSetup.Value) : null);
                            break;
                        case CycleTimeId:
                            SetIf(prop, initCycle.HasValue ? FormatTime(initCycle.Value) : null);
                            break;
                    }
                }
            }
        }
        catch
        {
            // ignore
        }
    }

    public DateTime? GetStartDateTime() => ParseDateTime(_startDateTime);
    public DateTime? GetEndDateTime() => ParseDateTime(_endDateTime);
    public TimeSpan? GetSetupTime() => ParseTimeSpan(_setupTime);
    public TimeSpan? GetCycleTime() => ParseTimeSpan(_cycleTime);

    public void SetStartDateTime(DateTime value) => SetStartDateTimeRaw(FormatDateTime(value));
    public void SetEndDateTime(DateTime value) => SetEndDateTimeRaw(FormatDateTime(value));
    public void SetSetupTime(TimeSpan value) => SetSetupTimeRaw(FormatTime(value));
    public void SetCycleTime(TimeSpan value) => SetCycleTimeRaw(FormatTime(value));

    public void SetStartTimeNow() => SetStartDateTime(DateTime.UtcNow);
    public void SetEndTimeNow() => SetEndDateTime(DateTime.UtcNow);

    public void CalculateCycleTime()
    {
        var start = GetStartDateTime();
        var end = GetEndDateTime();
        if (start.HasValue && end.HasValue)
        {
            var duration = end.Value - start.Value;
            if (duration < TimeSpan.Zero)
            {
                duration = duration.Negate();
            }

            SetCycleTime(duration);
        }
    }

    public void NormalizeToAbsoluteDates(DateTime? referenceTime = null)
    {
        var anchor = referenceTime ?? DateTime.UtcNow;
        NormalizeProperty(_startDateTime, anchor);
        NormalizeProperty(_endDateTime, anchor);
    }

    public TimeSpan DurationUntilStart(DateTime? referenceTime = null)
    {
        var anchor = referenceTime ?? DateTime.UtcNow;
        var start = GetStartDateTime();
        return start.HasValue ? start.Value - anchor : TimeSpan.Zero;
    }

    public bool AllowedToStartStep(DateTime? referenceTime = null)
    {
        return DurationUntilStart(referenceTime) <= TimeSpan.Zero;
    }

    private void SetStartDateTimeRaw(string value) => _startDateTime.Value = new PropertyValue<string>(value ?? string.Empty);
    private void SetEndDateTimeRaw(string value) => _endDateTime.Value = new PropertyValue<string>(value ?? string.Empty);
    private void SetSetupTimeRaw(string value) => _setupTime.Value = new PropertyValue<string>(value ?? string.Empty);
    private void SetCycleTimeRaw(string value) => _cycleTime.Value = new PropertyValue<string>(value ?? string.Empty);

    private static Property<string> CreateSchedulingProperty(string idShort, Reference semantic)
    {
        return SubmodelElementFactory.CreateStringProperty(idShort, string.Empty, semantic);
    }

    private static DateTime? ParseDateTime(Property<string> property)
    {
        var raw = property.Value.Value?.ToString();
        if (DateTime.TryParseExact(raw, DateTimeFormat, Culture, DateTimeStyles.None, out var value))
        {
            return value;
        }

        return null;
    }

    private static TimeSpan? ParseTimeSpan(Property<string> property)
    {
        var raw = property.Value.Value?.ToString();
        if (TimeSpan.TryParseExact(raw, TimeFormat, Culture, out var value))
        {
            return value;
        }

        if (TimeSpan.TryParse(raw, Culture, out value))
        {
            return value;
        }

        return null;
    }

    private static string FormatDateTime(DateTime value) => value.ToString(DateTimeFormat, Culture);

    private static string FormatTime(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
        {
            value = value.Negate();
        }

        var totalHours = (int)Math.Floor(value.TotalHours);
        var remainder = value - TimeSpan.FromHours(totalHours);
        return $"{totalHours:00}:{remainder.Minutes:00}:{remainder.Seconds:00}";
    }

    private static void NormalizeProperty(Property<string> property, DateTime anchor)
    {
        var raw = property.Value.Value?.ToString();
        if (!DateTime.TryParseExact(raw, DateTimeFormat, Culture, DateTimeStyles.None, out var parsed))
        {
            return;
        }

        var adjusted = anchor
            .AddHours(parsed.Hour)
            .AddMinutes(parsed.Minute)
            .AddSeconds(parsed.Second);

        property.Value = new PropertyValue<string>(FormatDateTime(adjusted));
    }
}

public class QuantityInformation : SubmodelElementCollection
{
    public Property<string> TotalNumberOfPieces { get; }

    public QuantityInformation(int totalNumberOfPieces, string idShort = "TotalNumberOfPieces")
        : base("QuantityInformation")
    {
        SemanticId = SemanticReferences.QuantityInformation;
        TotalNumberOfPieces = SubmodelElementFactory.CreateStringProperty(idShort, totalNumberOfPieces.ToString(), SemanticReferences.TotalNumberOfPieces);
        SubmodelElementFactory.SetValueType(TotalNumberOfPieces, "xs:integer");
        Add(TotalNumberOfPieces);
    }
}

internal static class SubmodelElementFactory
{
    public static Property<string> CreateStringProperty(string idShort, string? value, Reference? semanticId = null, string valueType = "xs:string")
    {
        var property = new Property<string>(idShort, value ?? string.Empty)
        {
            SemanticId = semanticId ?? SemanticReferences.EmptyExternal
        };

        SetValueType(property, valueType);

        return property;
    }

    public static ISubmodelElement CreateProperty(string idShort, object? value, Reference? semanticId = null)
    {
        Property property = value switch
        {
            int i => new Property<int>(idShort, i),
            long l => new Property<long>(idShort, l),
            double d => new Property<double>(idShort, d),
            float f => new Property<float>(idShort, f),
            decimal m => new Property<decimal>(idShort, m),
            bool b => new Property<string>(idShort, b ? "true" : "false"),
            null => new Property<string>(idShort, string.Empty),
            _ => new Property<string>(idShort, value?.ToString() ?? string.Empty)
        };

        string valueType = value switch
        {
            int => "xs:integer",
            long => "xs:long",
            double => "xs:double",
            float => "xs:float",
            decimal => "xs:decimal",
            bool => "xs:boolean",
            _ => "xs:string"
        };

        property.SemanticId = semanticId ?? SemanticReferences.EmptyExternal;
        SetValueType(property, valueType);

        return property;
    }

    public static Property CreateProperty(string idShort, object? value, Reference? semanticId, string valueType)
    {
        var dataType = ParseDataType(valueType);
        Property property = value is null
            ? new Property(idShort, dataType)
            : new Property(idShort, dataType, value);

        property.SemanticId = semanticId ?? SemanticReferences.EmptyExternal;
        return property;
    }

    public static void SetValueType(Property property, string valueType)
    {
        var valueTypeProp = property
            .GetType()
            .GetProperty(
                "ValueType",
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);
        if (valueTypeProp != null && valueTypeProp.CanWrite)
        {
            valueTypeProp.SetValue(property, valueType);
        }
    }

    private static DataType ParseDataType(string valueType)
    {
        if (DataObjectType.TryParse(valueType, out var dataObjectType))
        {
            return new DataType(dataObjectType);
        }

        return new DataType(DataObjectType.String);
    }
}
