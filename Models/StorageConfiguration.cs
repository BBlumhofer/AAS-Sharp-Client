using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AasSharpClient.Models.Helpers;
using AasSharpClient.Tools;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models;

public static class StorageConfigurationSemantics
{
    public const string SubmodelSemantic = "https://smartfactory.de/semantics/submodel/CarrierManagement/StorageConfiguration#1/0";

    public static readonly Reference Submodel = ReferenceFactory.External(
        (KeyType.GlobalReference, SubmodelSemantic));
}

public sealed class StorageConfigurationSubmodel : Submodel
{
    public const string DefaultIdShort = "StorageConfiguration";

    public SubmodelElementCollection Storages { get; private set; }
    public StorageConfigurationDemandConfig DemandConfig { get; private set; }
    public StorageConfigurationProjectionConfig ProjectionConfig { get; private set; }

    public StorageConfigurationSubmodel(
        string? submodelIdentifier = null,
        string idShort = DefaultIdShort,
        Reference? semanticId = null)
        : base(idShort, new Identifier(submodelIdentifier ?? Guid.NewGuid().ToString()))
    {
        Kind = ModelingKind.Instance;
        SemanticId = semanticId ?? StorageConfigurationSemantics.Submodel;

        Storages = new SubmodelElementCollection("Storages");
        DemandConfig = new StorageConfigurationDemandConfig();
        ProjectionConfig = new StorageConfigurationProjectionConfig();

        SubmodelElements.Add(Storages);
        SubmodelElements.Add(DemandConfig);
        SubmodelElements.Add(ProjectionConfig);
    }

    public static StorageConfigurationSubmodel CreateWithIdentifier(string submodelIdentifier) => new(submodelIdentifier);

    public Task<string> ToJsonAsync(CancellationToken cancellationToken = default)
    {
        return SubmodelSerialization.SerializeAsync(this, cancellationToken);
    }

    public void Apply(StorageConfigurationData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (!string.IsNullOrWhiteSpace(data.SubmodelIdShort))
        {
            IdShort = data.SubmodelIdShort!;
        }

        if (!string.IsNullOrWhiteSpace(data.SubmodelIdentifier))
        {
            Id = new Identifier(data.SubmodelIdentifier!);
        }

        if (data.SemanticId is not null)
        {
            SemanticId = data.SemanticId;
        }

        SubmodelElements.Clear();

        Storages = StorageConfigurationElementFactory.CreateStorages(
            data.Storages ?? Array.Empty<StorageConfigurationStorageData>(),
            data.StoragesSemanticId,
            data.StoragesQualifiers);
        DemandConfig = StorageConfigurationElementFactory.CreateDemandConfig(data.DemandConfig);
        ProjectionConfig = StorageConfigurationElementFactory.CreateProjectionConfig(data.ProjectionConfig);

        SubmodelElements.Add(Storages);
        SubmodelElements.Add(DemandConfig);
        SubmodelElements.Add(ProjectionConfig);

        if (data.AdditionalElements != null)
        {
            foreach (var element in data.AdditionalElements)
            {
                if (element != null)
                {
                    SubmodelElements.Add(element);
                }
            }
        }
    }

    public IEnumerable<StorageConfigurationStorage> GetStorages()
    {
        return Storages
            .OfType<SubmodelElementCollection>()
            .Select(StorageConfigurationStorage.FromCollection);
    }

    public static StorageConfigurationSubmodel FromSubmodel(Submodel source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var submodel = new StorageConfigurationSubmodel(
            null,
            string.IsNullOrWhiteSpace(source.IdShort) ? DefaultIdShort : source.IdShort,
            source.SemanticId as Reference ?? StorageConfigurationSemantics.Submodel);

        if (source.Id != null)
        {
            submodel.Id = source.Id;
        }

        submodel.Kind = source.Kind;
        submodel.LoadFromElements(source.SubmodelElements);
        return submodel;
    }

    public static StorageConfigurationSubmodel FromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var id = root.TryGetProperty("id", out var idNode) ? idNode.GetString() : null;
        var idShort = root.TryGetProperty("idShort", out var idShortNode)
            ? idShortNode.GetString() ?? DefaultIdShort
            : DefaultIdShort;

        var submodel = new StorageConfigurationSubmodel(id, idShort);

        if (root.TryGetProperty("semanticId", out var semanticNode))
        {
            try
            {
                var semantic = JsonSerializer.Deserialize<Reference>(semanticNode.GetRawText(), JsonLoader.Options);
                if (semantic != null)
                {
                    submodel.SemanticId = semantic;
                }
            }
            catch
            {
                // ignore invalid semantic id
            }
        }

        if (root.TryGetProperty("submodelElements", out var elementsNode) && elementsNode.ValueKind == JsonValueKind.Array)
        {
            var elements = new List<ISubmodelElement>();
            foreach (var element in elementsNode.EnumerateArray())
            {
                var sme = JsonLoader.DeserializeElement(element);
                if (sme != null)
                {
                    elements.Add(sme);
                }
            }

            submodel.LoadFromElements(elements);
        }

        return submodel;
    }

    private void LoadFromElements(IEnumerable<ISubmodelElement>? elements)
    {
        if (elements == null)
        {
            return;
        }

        var elementList = elements.ToList();
        var storagesSource = elementList
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(el => string.Equals(el.IdShort, "Storages", StringComparison.OrdinalIgnoreCase));
        var demandSource = elementList
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(el => string.Equals(el.IdShort, "DemandConfig", StringComparison.OrdinalIgnoreCase));
        var projectionSource = elementList
            .OfType<SubmodelElementCollection>()
            .FirstOrDefault(el => string.Equals(el.IdShort, "ProjectionConfig", StringComparison.OrdinalIgnoreCase));

        SubmodelElements.Clear();

        Storages = storagesSource != null
            ? StorageConfigurationElementFactory.CloneStorages(storagesSource)
            : new SubmodelElementCollection("Storages");

        DemandConfig = demandSource != null
            ? StorageConfigurationDemandConfig.FromCollection(demandSource)
            : new StorageConfigurationDemandConfig();

        ProjectionConfig = projectionSource != null
            ? StorageConfigurationProjectionConfig.FromCollection(projectionSource)
            : new StorageConfigurationProjectionConfig();

        SubmodelElements.Add(Storages);
        SubmodelElements.Add(DemandConfig);
        SubmodelElements.Add(ProjectionConfig);

        foreach (var element in elementList)
        {
            if (ReferenceEquals(element, storagesSource) || ReferenceEquals(element, demandSource) || ReferenceEquals(element, projectionSource))
            {
                continue;
            }

            SubmodelElements.Add(element);
        }
    }
}

public sealed class StorageConfigurationStorage : SubmodelElementCollection
{
    private IProperty? _storageId;
    private IProperty? _name;
    private IProperty? _totalSlots;
    private IProperty? _costFunctionType;
    private IProperty? _baseCost;
    private IProperty? _alpha;
    private IProperty? _lowCost;
    private IProperty? _highCost;
    private IProperty? _stepThreshold;
    private IProperty? _maxCost;
    private SubmodelElementCollection? _slots;

    public StorageConfigurationStorage(string idShort) : base(idShort)
    {
    }

    public StorageConfigurationStorage(SubmodelElementCollection source) : base(source.IdShort)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        if (string.IsNullOrWhiteSpace(source.IdShort))
        {
            IdShort = "Storage";
        }

        SemanticId = source.SemanticId;
        Description = source.Description;
        Qualifiers = source.Qualifiers;

        if (source.Values != null)
        {
            foreach (var child in source.Values)
            {
                Add(child);
            }
        }
    }

    public IProperty? StorageId => _storageId ??= GetProperty("StorageId");
    public IProperty? Name => _name ??= GetProperty("Name");
    public IProperty? TotalSlots => _totalSlots ??= GetProperty("TotalSlots");
    public IProperty? CostFunctionType => _costFunctionType ??= GetProperty("CostFunctionType");
    public IProperty? BaseCost => _baseCost ??= GetProperty("BaseCost");
    public IProperty? Alpha => _alpha ??= GetProperty("Alpha");
    public IProperty? LowCost => _lowCost ??= GetProperty("LowCost");
    public IProperty? HighCost => _highCost ??= GetProperty("HighCost");
    public IProperty? StepThreshold => _stepThreshold ??= GetProperty("StepThreshold");
    public IProperty? MaxCost => _maxCost ??= GetProperty("MaxCost");

    public SubmodelElementCollection? SlotsCollection => _slots ??= GetCollection("Slots");

    public IEnumerable<StorageConfigurationSlot> Slots =>
        SlotsCollection?.OfType<SubmodelElementCollection>().Select(StorageConfigurationSlot.FromCollection)
        ?? Enumerable.Empty<StorageConfigurationSlot>();

    public static StorageConfigurationStorage FromCollection(SubmodelElementCollection collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        return new StorageConfigurationStorage(collection);
    }

    public string? GetStorageId() => AasValueUnwrap.UnwrapToString(StorageId?.Value);

    public string? GetName() => AasValueUnwrap.UnwrapToString(Name?.Value);

    public int? GetTotalSlots() => AasValueUnwrap.UnwrapToInt(TotalSlots?.Value);

    public string? GetCostFunctionType() => AasValueUnwrap.UnwrapToString(CostFunctionType?.Value);

    public double? GetBaseCost() => AasValueUnwrap.UnwrapToDouble(BaseCost?.Value);

    public double? GetAlpha() => AasValueUnwrap.UnwrapToDouble(Alpha?.Value);

    public double? GetLowCost() => AasValueUnwrap.UnwrapToDouble(LowCost?.Value);

    public double? GetHighCost() => AasValueUnwrap.UnwrapToDouble(HighCost?.Value);

    public double? GetStepThreshold() => AasValueUnwrap.UnwrapToDouble(StepThreshold?.Value);

    public double? GetMaxCost() => AasValueUnwrap.UnwrapToDouble(MaxCost?.Value);

    private IProperty? GetProperty(string idShort)
    {
        return Values?.OfType<IProperty>()
            .FirstOrDefault(prop => string.Equals(prop.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
    }

    private SubmodelElementCollection? GetCollection(string idShort)
    {
        return Values?.OfType<SubmodelElementCollection>()
            .FirstOrDefault(collection => string.Equals(collection.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class StorageConfigurationSlot : SubmodelElementCollection
{
    private IProperty? _slotId;
    private IProperty? _preferredType;
    private IProperty? _affinityReward;
    private IProperty? _affinityPenalty;

    public StorageConfigurationSlot(string idShort) : base(idShort)
    {
    }

    public StorageConfigurationSlot(SubmodelElementCollection source) : base(source.IdShort)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        if (string.IsNullOrWhiteSpace(source.IdShort))
        {
            IdShort = "Slot";
        }

        SemanticId = source.SemanticId;
        Description = source.Description;
        Qualifiers = source.Qualifiers;

        if (source.Values != null)
        {
            foreach (var child in source.Values)
            {
                Add(child);
            }
        }
    }

    public IProperty? SlotId => _slotId ??= GetProperty("SlotId");
    public IProperty? PreferredType => _preferredType ??= GetProperty("PreferredType");
    public IProperty? AffinityReward => _affinityReward ??= GetProperty("AffinityReward");
    public IProperty? AffinityPenalty => _affinityPenalty ??= GetProperty("AffinityPenalty");

    public static StorageConfigurationSlot FromCollection(SubmodelElementCollection collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        return new StorageConfigurationSlot(collection);
    }

    public string? GetSlotId() => AasValueUnwrap.UnwrapToString(SlotId?.Value);

    public string? GetPreferredType() => AasValueUnwrap.UnwrapToString(PreferredType?.Value);

    public double? GetAffinityReward() => AasValueUnwrap.UnwrapToDouble(AffinityReward?.Value);

    public double? GetAffinityPenalty() => AasValueUnwrap.UnwrapToDouble(AffinityPenalty?.Value);

    private IProperty? GetProperty(string idShort)
    {
        return Values?.OfType<IProperty>()
            .FirstOrDefault(prop => string.Equals(prop.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class StorageConfigurationDemandConfig : SubmodelElementCollection
{
    private IProperty? _demandBonusBase;
    private IProperty? _demandBonusMax;
    private IProperty? _weightPotential;
    private IProperty? _weightPlanned;
    private IProperty? _weightImminent;
    private IProperty? _weightExecuting;
    private IProperty? _urgencyEnabled;

    public StorageConfigurationDemandConfig(string idShort = "DemandConfig") : base(idShort)
    {
    }

    public StorageConfigurationDemandConfig(SubmodelElementCollection source) : base(source.IdShort)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        if (string.IsNullOrWhiteSpace(source.IdShort))
        {
            IdShort = "DemandConfig";
        }

        SemanticId = source.SemanticId;
        Description = source.Description;
        Qualifiers = source.Qualifiers;

        if (source.Values != null)
        {
            foreach (var child in source.Values)
            {
                Add(child);
            }
        }
    }

    public IProperty? DemandBonusBase => _demandBonusBase ??= GetProperty("DemandBonusBase");
    public IProperty? DemandBonusMax => _demandBonusMax ??= GetProperty("DemandBonusMax");
    public IProperty? DemandWeightPotential => _weightPotential ??= GetProperty("DemandWeightPotential");
    public IProperty? DemandWeightPlanned => _weightPlanned ??= GetProperty("DemandWeightPlanned");
    public IProperty? DemandWeightImminent => _weightImminent ??= GetProperty("DemandWeightImminent");
    public IProperty? DemandWeightExecuting => _weightExecuting ??= GetProperty("DemandWeightExecuting");
    public IProperty? UrgencyEnabled => _urgencyEnabled ??= GetProperty("UrgencyEnabled");

    public static StorageConfigurationDemandConfig FromCollection(SubmodelElementCollection collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        return new StorageConfigurationDemandConfig(collection);
    }

    public double? GetDemandBonusBase() => AasValueUnwrap.UnwrapToDouble(DemandBonusBase?.Value);

    public double? GetDemandBonusMax() => AasValueUnwrap.UnwrapToDouble(DemandBonusMax?.Value);

    public double? GetDemandWeightPotential() => AasValueUnwrap.UnwrapToDouble(DemandWeightPotential?.Value);

    public double? GetDemandWeightPlanned() => AasValueUnwrap.UnwrapToDouble(DemandWeightPlanned?.Value);

    public double? GetDemandWeightImminent() => AasValueUnwrap.UnwrapToDouble(DemandWeightImminent?.Value);

    public double? GetDemandWeightExecuting() => AasValueUnwrap.UnwrapToDouble(DemandWeightExecuting?.Value);

    public bool? GetUrgencyEnabled() => AasValueUnwrap.UnwrapToBool(UrgencyEnabled?.Value);

    private IProperty? GetProperty(string idShort)
    {
        return Values?.OfType<IProperty>()
            .FirstOrDefault(prop => string.Equals(prop.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class StorageConfigurationProjectionConfig : SubmodelElementCollection
{
    private IProperty? _weightNoAgent;
    private IProperty? _weightStepOpen;
    private IProperty? _weightStepPlanned;
    private IProperty? _weightStepExecuting;
    private IProperty? _maxStepsAhead;

    public StorageConfigurationProjectionConfig(string idShort = "ProjectionConfig") : base(idShort)
    {
    }

    public StorageConfigurationProjectionConfig(SubmodelElementCollection source) : base(source.IdShort)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        if (string.IsNullOrWhiteSpace(source.IdShort))
        {
            IdShort = "ProjectionConfig";
        }

        SemanticId = source.SemanticId;
        Description = source.Description;
        Qualifiers = source.Qualifiers;

        if (source.Values != null)
        {
            foreach (var child in source.Values)
            {
                Add(child);
            }
        }
    }

    public IProperty? WeightNoAgent => _weightNoAgent ??= GetProperty("WeightNoAgent");
    public IProperty? WeightStepOpen => _weightStepOpen ??= GetProperty("WeightStepOpen");
    public IProperty? WeightStepPlanned => _weightStepPlanned ??= GetProperty("WeightStepPlanned");
    public IProperty? WeightStepExecuting => _weightStepExecuting ??= GetProperty("WeightStepExecuting");
    public IProperty? MaxStepsAhead => _maxStepsAhead ??= GetProperty("MaxStepsAhead");

    public static StorageConfigurationProjectionConfig FromCollection(SubmodelElementCollection collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        return new StorageConfigurationProjectionConfig(collection);
    }

    public double? GetWeightNoAgent() => AasValueUnwrap.UnwrapToDouble(WeightNoAgent?.Value);

    public double? GetWeightStepOpen() => AasValueUnwrap.UnwrapToDouble(WeightStepOpen?.Value);

    public double? GetWeightStepPlanned() => AasValueUnwrap.UnwrapToDouble(WeightStepPlanned?.Value);

    public double? GetWeightStepExecuting() => AasValueUnwrap.UnwrapToDouble(WeightStepExecuting?.Value);

    public int? GetMaxStepsAhead() => AasValueUnwrap.UnwrapToInt(MaxStepsAhead?.Value);

    private IProperty? GetProperty(string idShort)
    {
        return Values?.OfType<IProperty>()
            .FirstOrDefault(prop => string.Equals(prop.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
    }
}

internal static class StorageConfigurationElementFactory
{
    public static SubmodelElementCollection CreateStorages(
        IReadOnlyList<StorageConfigurationStorageData> storages,
        Reference? semanticId,
        IReadOnlyList<IQualifier>? qualifiers)
    {
        var collection = new SubmodelElementCollection("Storages")
        {
            SemanticId = semanticId
        };

        ApplyQualifiers(qualifiers, q => collection.Qualifiers = q);

        if (storages != null)
        {
            foreach (var storage in storages)
            {
                collection.Add(CreateStorage(storage));
            }
        }

        return collection;
    }

    public static SubmodelElementCollection CloneStorages(SubmodelElementCollection source)
    {
        var clone = new SubmodelElementCollection(string.IsNullOrWhiteSpace(source.IdShort) ? "Storages" : source.IdShort)
        {
            SemanticId = source.SemanticId,
            Description = source.Description,
            Qualifiers = source.Qualifiers
        };

        foreach (var element in Elements(source))
        {
            clone.Add(element);
        }

        return clone;
    }

    public static StorageConfigurationStorage CreateStorage(StorageConfigurationStorageData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        var storage = new StorageConfigurationStorage(data.IdShort)
        {
            SemanticId = data.SemanticId,
            Description = data.Description
        };

        ApplyQualifiers(data.Qualifiers, q => storage.Qualifiers = q);

        storage.Add(CreateProperty("StorageId", data.StorageId, "xs:string", data.PropertyQualifiers));
        storage.Add(CreateProperty("Name", data.Name, "xs:string", data.PropertyQualifiers));
        storage.Add(CreateProperty("TotalSlots", data.TotalSlots, "xs:int", data.PropertyQualifiers));
        storage.Add(CreateProperty("CostFunctionType", data.CostFunctionType, "xs:string", data.PropertyQualifiers));
        storage.Add(CreateProperty("BaseCost", data.BaseCost, "xs:double", data.PropertyQualifiers));

        if (data.Alpha.HasValue)
        {
            storage.Add(CreateProperty("Alpha", data.Alpha.Value, "xs:double", data.PropertyQualifiers));
        }

        if (data.LowCost.HasValue)
        {
            storage.Add(CreateProperty("LowCost", data.LowCost.Value, "xs:double", data.PropertyQualifiers));
        }

        if (data.HighCost.HasValue)
        {
            storage.Add(CreateProperty("HighCost", data.HighCost.Value, "xs:double", data.PropertyQualifiers));
        }

        if (data.StepThreshold.HasValue)
        {
            storage.Add(CreateProperty("StepThreshold", data.StepThreshold.Value, "xs:double", data.PropertyQualifiers));
        }

        storage.Add(CreateProperty("MaxCost", data.MaxCost, "xs:double", data.PropertyQualifiers));

        var slotsCollection = new SubmodelElementCollection("Slots")
        {
            SemanticId = data.SlotsSemanticId
        };

        ApplyQualifiers(data.SlotsQualifiers, q => slotsCollection.Qualifiers = q);

        if (data.Slots != null)
        {
            foreach (var slot in data.Slots)
            {
                slotsCollection.Add(CreateSlot(slot));
            }
        }

        storage.Add(slotsCollection);

        return storage;
    }

    public static StorageConfigurationSlot CreateSlot(StorageConfigurationSlotData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        var slot = new StorageConfigurationSlot(data.IdShort)
        {
            SemanticId = data.SemanticId,
            Description = data.Description
        };

        ApplyQualifiers(data.Qualifiers, q => slot.Qualifiers = q);

        slot.Add(CreateProperty("SlotId", data.SlotId, "xs:string", data.PropertyQualifiers));
        if (!string.IsNullOrWhiteSpace(data.PreferredType))
        {
            slot.Add(CreateProperty("PreferredType", data.PreferredType, "xs:string", data.PropertyQualifiers));
        }

        slot.Add(CreateProperty("AffinityReward", data.AffinityReward, "xs:double", data.PropertyQualifiers));
        slot.Add(CreateProperty("AffinityPenalty", data.AffinityPenalty, "xs:double", data.PropertyQualifiers));

        return slot;
    }

    public static StorageConfigurationDemandConfig CreateDemandConfig(StorageConfigurationDemandConfigData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        var demand = new StorageConfigurationDemandConfig(data.IdShort)
        {
            SemanticId = data.SemanticId,
            Description = data.Description
        };

        ApplyQualifiers(data.Qualifiers, q => demand.Qualifiers = q);

        demand.Add(CreateProperty("DemandBonusBase", data.DemandBonusBase, "xs:double", data.PropertyQualifiers));
        demand.Add(CreateProperty("DemandBonusMax", data.DemandBonusMax, "xs:double", data.PropertyQualifiers));
        demand.Add(CreateProperty("DemandWeightPotential", data.DemandWeightPotential, "xs:double", data.PropertyQualifiers));
        demand.Add(CreateProperty("DemandWeightPlanned", data.DemandWeightPlanned, "xs:double", data.PropertyQualifiers));
        demand.Add(CreateProperty("DemandWeightImminent", data.DemandWeightImminent, "xs:double", data.PropertyQualifiers));
        demand.Add(CreateProperty("DemandWeightExecuting", data.DemandWeightExecuting, "xs:double", data.PropertyQualifiers));
        demand.Add(CreateProperty("UrgencyEnabled", data.UrgencyEnabled, "xs:boolean", data.PropertyQualifiers));

        return demand;
    }

    public static StorageConfigurationProjectionConfig CreateProjectionConfig(StorageConfigurationProjectionConfigData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        var projection = new StorageConfigurationProjectionConfig(data.IdShort)
        {
            SemanticId = data.SemanticId,
            Description = data.Description
        };

        ApplyQualifiers(data.Qualifiers, q => projection.Qualifiers = q);

        projection.Add(CreateProperty("WeightNoAgent", data.WeightNoAgent, "xs:double", data.PropertyQualifiers));
        projection.Add(CreateProperty("WeightStepOpen", data.WeightStepOpen, "xs:double", data.PropertyQualifiers));
        projection.Add(CreateProperty("WeightStepPlanned", data.WeightStepPlanned, "xs:double", data.PropertyQualifiers));
        projection.Add(CreateProperty("WeightStepExecuting", data.WeightStepExecuting, "xs:double", data.PropertyQualifiers));
        projection.Add(CreateProperty("MaxStepsAhead", data.MaxStepsAhead, "xs:int", data.PropertyQualifiers));

        return projection;
    }

    private static Property CreateProperty(
        string idShort,
        object? value,
        string valueType,
        IReadOnlyDictionary<string, IReadOnlyList<IQualifier>>? qualifiersByIdShort)
    {
        var property = SubmodelElementFactory.CreateProperty(idShort, value, null, valueType);
        if (qualifiersByIdShort != null && qualifiersByIdShort.TryGetValue(idShort, out var qualifiers))
        {
            ApplyQualifiers(qualifiers, q => property.Qualifiers = q);
        }

        return property;
    }

    private static void ApplyQualifiers(IReadOnlyList<IQualifier>? qualifiers, Action<ICollection<IQualifier>> assign)
    {
        if (qualifiers is { Count: > 0 })
        {
            assign(new List<IQualifier>(qualifiers));
        }
    }

    private static IEnumerable<ISubmodelElement> Elements(SubmodelElementCollection? coll)
    {
        if (coll is null)
        {
            return Array.Empty<ISubmodelElement>();
        }

        if (coll.Value is IEnumerable<ISubmodelElement> seq)
        {
            return seq;
        }

        if (coll is IEnumerable<ISubmodelElement> enumerable)
        {
            return enumerable;
        }

        return Array.Empty<ISubmodelElement>();
    }
}

public sealed record StorageConfigurationData(
    string SubmodelIdentifier,
    IReadOnlyList<StorageConfigurationStorageData> Storages,
    StorageConfigurationDemandConfigData DemandConfig,
    StorageConfigurationProjectionConfigData ProjectionConfig,
    string? SubmodelIdShort = StorageConfigurationSubmodel.DefaultIdShort,
    Reference? SemanticId = null,
    Reference? StoragesSemanticId = null,
    IReadOnlyList<IQualifier>? StoragesQualifiers = null,
    IReadOnlyList<ISubmodelElement>? AdditionalElements = null);

public sealed record StorageConfigurationStorageData(
    string IdShort,
    string StorageId,
    string Name,
    int TotalSlots,
    string CostFunctionType,
    double BaseCost,
    double MaxCost,
    double? Alpha = null,
    double? LowCost = null,
    double? HighCost = null,
    double? StepThreshold = null,
    IReadOnlyList<StorageConfigurationSlotData>? Slots = null,
    Reference? SemanticId = null,
    LangStringSet? Description = null,
    IReadOnlyList<IQualifier>? Qualifiers = null,
    Reference? SlotsSemanticId = null,
    IReadOnlyList<IQualifier>? SlotsQualifiers = null,
    IReadOnlyDictionary<string, IReadOnlyList<IQualifier>>? PropertyQualifiers = null);

public sealed record StorageConfigurationSlotData(
    string IdShort,
    string SlotId,
    string? PreferredType,
    double AffinityReward,
    double AffinityPenalty,
    Reference? SemanticId = null,
    LangStringSet? Description = null,
    IReadOnlyList<IQualifier>? Qualifiers = null,
    IReadOnlyDictionary<string, IReadOnlyList<IQualifier>>? PropertyQualifiers = null);

public sealed record StorageConfigurationDemandConfigData(
    double DemandBonusBase,
    double DemandBonusMax,
    double DemandWeightPotential,
    double DemandWeightPlanned,
    double DemandWeightImminent,
    double DemandWeightExecuting,
    bool UrgencyEnabled,
    string IdShort = "DemandConfig",
    Reference? SemanticId = null,
    LangStringSet? Description = null,
    IReadOnlyList<IQualifier>? Qualifiers = null,
    IReadOnlyDictionary<string, IReadOnlyList<IQualifier>>? PropertyQualifiers = null);

public sealed record StorageConfigurationProjectionConfigData(
    double WeightNoAgent,
    double WeightStepOpen,
    double WeightStepPlanned,
    double WeightStepExecuting,
    int MaxStepsAhead,
    string IdShort = "ProjectionConfig",
    Reference? SemanticId = null,
    LangStringSet? Description = null,
    IReadOnlyList<IQualifier>? Qualifiers = null,
    IReadOnlyDictionary<string, IReadOnlyList<IQualifier>>? PropertyQualifiers = null);
