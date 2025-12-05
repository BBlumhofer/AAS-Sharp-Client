using System.Collections.Generic;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models;

internal static class ReferenceFactory
{
    public static Reference External(params (KeyType Type, string Value)[] keys) => CreateReference(ReferenceType.ExternalReference, keys);

    public static Reference External(IEnumerable<(KeyType Type, string Value)> keys) => CreateReference(ReferenceType.ExternalReference, keys);

    public static Reference Model(params (KeyType Type, string Value)[] keys) => CreateReference(ReferenceType.ModelReference, keys);

    public static Reference Model(IEnumerable<(KeyType Type, string Value)> keys) => CreateReference(ReferenceType.ModelReference, keys);

    private static Reference CreateReference(ReferenceType type, IEnumerable<(KeyType Type, string Value)> keys)
    {
        var keyList = new List<IKey>();
        foreach (var (keyType, value) in keys)
        {
            keyList.Add(new Key(keyType, value));
        }

        return new Reference(keyList)
        {
            Type = type
        };
    }
}
