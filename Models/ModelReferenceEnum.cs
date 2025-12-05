using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models;

public static class ModelReferenceEnum
{
    public static readonly ModelReferenceType Submodel = new(KeyType.Submodel);
    public static readonly ModelReferenceType SubmodelElementCollection = new(KeyType.SubmodelElementCollection);
    public static readonly ModelReferenceType Property = new(KeyType.Property);

    public sealed class ModelReferenceType
    {
        internal ModelReferenceType(KeyType keyType)
        {
            KeyType = keyType;
        }

        internal KeyType KeyType { get; }

        internal Key ToKey(string value) => new(KeyType, value);
    }
}
