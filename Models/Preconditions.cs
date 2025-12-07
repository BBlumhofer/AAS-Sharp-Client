using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models;

public enum PreconditionsEnum
{
    InStorage
}

public enum SlotContentTypeEnum
{
    CarrierId,
    CarrierType,
    ProductType,
    EmptySlot
}

internal static class PreconditionsExtensions
{
    public static string ToAasValue(this PreconditionsEnum precondition) => precondition switch
    {
        PreconditionsEnum.InStorage => "instorage",
        _ => "unknown"
    };

    public static PreconditionsEnum FromPreconditionValue(string? value) => value?.ToLowerInvariant() switch
    {
        "instorage" => PreconditionsEnum.InStorage,
        _ => PreconditionsEnum.InStorage
    };

    public static string ToAasValue(this SlotContentTypeEnum contentType) => contentType switch
    {
        SlotContentTypeEnum.CarrierId => "carrierId",
        SlotContentTypeEnum.CarrierType => "carrierType",
        SlotContentTypeEnum.ProductType => "productType",
        SlotContentTypeEnum.EmptySlot => "emptySlot",
        _ => "carrierId"
    };

    public static SlotContentTypeEnum FromSlotContentTypeValue(string? value) => value?.ToLowerInvariant() switch
    {
        "carrierid" => SlotContentTypeEnum.CarrierId,
        "carriertype" => SlotContentTypeEnum.CarrierType,
        "producttype" => SlotContentTypeEnum.ProductType,
        "emptyslot" => SlotContentTypeEnum.EmptySlot,
        _ => SlotContentTypeEnum.CarrierId
    };
}

public class Precondition : SubmodelElementCollection
{
    public Property<string> Type { get; }
    public SubmodelElementCollection ConditionValue { get; }

    public Precondition(PreconditionsEnum preconditionType, SlotContentTypeEnum slotContentType, string slotValue)
        : base("Precondition")
    {
        SemanticId = SemanticReferences.ActionPreconditions;

        Type = SubmodelElementFactory.CreateStringProperty("PreconditionType", preconditionType.ToAasValue(), SemanticReferences.EmptyExternal);
        ConditionValue = new SubmodelElementCollection("ConditionValue")
        {
            SemanticId = SemanticReferences.EmptyExternal
        };

        Add(Type);
        Add(ConditionValue);

        if (preconditionType == PreconditionsEnum.InStorage)
        {
            SetInStorageCondition(slotContentType, slotValue);
        }
    }

    public void SetInStorageCondition(SlotContentTypeEnum slotContentType, string slotValue)
    {
        ConditionValue.Clear();
        var contentTypeProp = SubmodelElementFactory.CreateStringProperty("SlotContentType", slotContentType.ToAasValue(), SemanticReferences.EmptyExternal);
        var slotValueProp = SubmodelElementFactory.CreateStringProperty("SlotValue", slotValue ?? string.Empty, SemanticReferences.EmptyExternal);
        ConditionValue.Add(contentTypeProp);
        ConditionValue.Add(slotValueProp);
    }
}
