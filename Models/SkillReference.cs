using System.Collections.Generic;
using System.Linq;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models;

public class SkillReference : ReferenceElement
{
    public SkillReference(IEnumerable<(object Key, string Value)> referenceChain)
        : base("SkillReference")
    {
        SemanticId = SemanticReferences.ActionSkillReference;

        UpdateReferenceChain(referenceChain);
    }

    public void UpdateReferenceChain(IEnumerable<(object Key, string Value)> referenceChain)
    {
        var keys = CreateKeyChain(referenceChain ?? Enumerable.Empty<(object Key, string Value)>()).ToArray();
        var reference = new Reference(keys)
        {
            Type = ReferenceType.ModelReference
        };

        Value = new ReferenceElementValue(reference);
    }

    private static IEnumerable<IKey> CreateKeyChain(IEnumerable<(object Key, string Value)> referenceChain)
    {
        var result = new List<IKey>();
        foreach (var (key, value) in referenceChain)
        {
            if (key is ModelReferenceEnum.ModelReferenceType type && !string.IsNullOrWhiteSpace(value))
            {
                result.Add(type.ToKey(value));
            }
        }

        if (result.Count == 0)
        {
            result.Add(new Key(KeyType.GlobalReference, "EMPTY"));
        }

        return result;
    }
}
