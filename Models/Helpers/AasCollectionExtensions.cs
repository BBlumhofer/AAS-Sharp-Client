using System;
using System.Collections.Generic;
using System.Linq;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Helpers;

public static class AasCollectionExtensions
{
    public static ISubmodelElement? GetElement(this SubmodelElementCollection? collection, string idShort)
    {
        if (collection == null || string.IsNullOrWhiteSpace(idShort)) return null;
        return collection.FirstOrDefault(e => string.Equals(e.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
    }

    public static ISubmodelElement? GetElement(this SubmodelElementList? list, string idShort)
    {
        if (list == null || string.IsNullOrWhiteSpace(idShort)) return null;
        return list.FirstOrDefault(e => string.Equals(e.IdShort, idShort, StringComparison.OrdinalIgnoreCase));
    }

    public static Property<string>? GetStringProperty(this SubmodelElementCollection? collection, string idShort)
    {
        return collection.GetElement(idShort) as Property<string>;
    }

    public static string? GetString(this SubmodelElementCollection? collection, string idShort)
    {
        return collection.GetStringProperty(idShort).GetText();
    }

    public static Property<int>? GetIntProperty(this SubmodelElementCollection? collection, string idShort)
    {
        return collection.GetElement(idShort) as Property<int>;
    }

    public static int? GetInt(this SubmodelElementCollection? collection, string idShort)
    {
        return AasValueUnwrap.UnwrapToInt(collection.GetIntProperty(idShort)?.Value);
    }

    public static SubmodelElementCollection? GetCollection(this SubmodelElementCollection? collection, string idShort)
    {
        return collection.GetElement(idShort) as SubmodelElementCollection;
    }

    public static SubmodelElementList? GetList(this SubmodelElementCollection? collection, string idShort)
    {
        return collection.GetElement(idShort) as SubmodelElementList;
    }

    public static T AddElement<T>(this SubmodelElementCollection collection, T element, bool replaceByIdShort = true)
        where T : ISubmodelElement
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        if (element == null) throw new ArgumentNullException(nameof(element));

        if (replaceByIdShort && !string.IsNullOrWhiteSpace(element.IdShort))
        {
            var existing = collection.GetElement(element.IdShort);
            if (existing != null && !ReferenceEquals(existing, element))
            {
                collection.Remove(existing);
            }
        }

        collection.Add(element);
        return element;
    }

    public static T AddElement<T>(this SubmodelElementList list, T element)
        where T : ISubmodelElement
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        if (element == null) throw new ArgumentNullException(nameof(element));

        list.Add(element);
        return element;
    }

    public static SubmodelElementCollection EnsureCollection(this SubmodelElementCollection parent, string idShort)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        if (string.IsNullOrWhiteSpace(idShort)) throw new ArgumentException("idShort must not be empty", nameof(idShort));

        if (parent.GetElement(idShort) is SubmodelElementCollection existing)
        {
            return existing;
        }

        var created = new SubmodelElementCollection(idShort);
        parent.Add(created);
        return created;
    }

    public static SubmodelElementList EnsureList(this SubmodelElementCollection parent, string idShort)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        if (string.IsNullOrWhiteSpace(idShort)) throw new ArgumentException("idShort must not be empty", nameof(idShort));

        if (parent.GetElement(idShort) is SubmodelElementList existing)
        {
            return existing;
        }

        var created = new SubmodelElementList(idShort);
        parent.Add(created);
        return created;
    }

    public static Property<string> EnsureStringProperty(this SubmodelElementCollection parent, string idShort)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        if (string.IsNullOrWhiteSpace(idShort)) throw new ArgumentException("idShort must not be empty", nameof(idShort));

        if (parent.GetElement(idShort) is Property<string> existing)
        {
            return existing;
        }

        var created = new Property<string>(idShort);
        parent.Add(created);
        return created;
    }

    public static Property<int> EnsureIntProperty(this SubmodelElementCollection parent, string idShort)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        if (string.IsNullOrWhiteSpace(idShort)) throw new ArgumentException("idShort must not be empty", nameof(idShort));

        if (parent.GetElement(idShort) is Property<int> existing)
        {
            return existing;
        }

        var created = new Property<int>(idShort);
        parent.Add(created);
        return created;
    }

    public static Property<double> EnsureDoubleProperty(this SubmodelElementCollection parent, string idShort)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        if (string.IsNullOrWhiteSpace(idShort)) throw new ArgumentException("idShort must not be empty", nameof(idShort));

        if (parent.GetElement(idShort) is Property<double> existing)
        {
            return existing;
        }

        var created = new Property<double>(idShort);
        parent.Add(created);
        return created;
    }

    public static void SetString(this SubmodelElementCollection parent, string idShort, string? value)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        parent.EnsureStringProperty(idShort).SetText(value);
    }

    public static void SetInt(this SubmodelElementCollection parent, string idShort, int value)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        parent.EnsureIntProperty(idShort).SetIntValue(value);
    }

    public static void SetDouble(this SubmodelElementCollection parent, string idShort, double value)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        parent.EnsureDoubleProperty(idShort).SetDoubleValue(value);
    }

    public static IEnumerable<ISubmodelElement> GetElementsBySemanticId(this SubmodelElementCollection? collection, IReference semanticId, bool recursive = false)
    {
        if (collection == null || semanticId == null)
        {
            return Enumerable.Empty<ISubmodelElement>();
        }

        return recursive
            ? EnumerateElementsRecursive(collection).Where(e => SemanticIdEquals(e.SemanticId, semanticId))
            : Elements(collection).Where(e => SemanticIdEquals(e.SemanticId, semanticId));
    }

    public static ISubmodelElement? GetElementBySemanticId(this SubmodelElementCollection? collection, IReference semanticId, bool recursive = false)
    {
        return collection.GetElementsBySemanticId(semanticId, recursive).FirstOrDefault();
    }

    public static IEnumerable<ISubmodelElement> GetElementsBySemanticId(this SubmodelElementCollection? collection, string semanticIdValue, bool recursive = false)
    {
        if (collection == null || string.IsNullOrWhiteSpace(semanticIdValue))
        {
            return Enumerable.Empty<ISubmodelElement>();
        }

        return recursive
            ? EnumerateElementsRecursive(collection).Where(e => SemanticIdContainsValue(e.SemanticId, semanticIdValue))
            : Elements(collection).Where(e => SemanticIdContainsValue(e.SemanticId, semanticIdValue));
    }

    public static ISubmodelElement? GetElementBySemanticId(this SubmodelElementCollection? collection, string semanticIdValue, bool recursive = false)
    {
        return collection.GetElementsBySemanticId(semanticIdValue, recursive).FirstOrDefault();
    }

    public static IEnumerable<ISubmodelElement> GetElementsBySemanticId(this SubmodelElementList? list, IReference semanticId, bool recursive = false)
    {
        if (list == null || semanticId == null)
        {
            return Enumerable.Empty<ISubmodelElement>();
        }

        return recursive
            ? EnumerateElementsRecursive(list).Where(e => SemanticIdEquals(e.SemanticId, semanticId))
            : list.Where(e => SemanticIdEquals(e.SemanticId, semanticId));
    }

    public static ISubmodelElement? GetElementBySemanticId(this SubmodelElementList? list, IReference semanticId, bool recursive = false)
    {
        return list.GetElementsBySemanticId(semanticId, recursive).FirstOrDefault();
    }

    public static IEnumerable<ISubmodelElement> GetElementsBySemanticId(this SubmodelElementList? list, string semanticIdValue, bool recursive = false)
    {
        if (list == null || string.IsNullOrWhiteSpace(semanticIdValue))
        {
            return Enumerable.Empty<ISubmodelElement>();
        }

        return recursive
            ? EnumerateElementsRecursive(list).Where(e => SemanticIdContainsValue(e.SemanticId, semanticIdValue))
            : list.Where(e => SemanticIdContainsValue(e.SemanticId, semanticIdValue));
    }

    public static ISubmodelElement? GetElementBySemanticId(this SubmodelElementList? list, string semanticIdValue, bool recursive = false)
    {
        return list.GetElementsBySemanticId(semanticIdValue, recursive).FirstOrDefault();
    }

    public static bool SemanticIdEquals(IReference? left, IReference? right)
    {
        if (left == null || right == null)
        {
            return false;
        }

        var leftKeys = left.Keys?.ToList();
        var rightKeys = right.Keys?.ToList();

        if (leftKeys == null || rightKeys == null || leftKeys.Count != rightKeys.Count)
        {
            return false;
        }

        for (var i = 0; i < leftKeys.Count; i++)
        {
            var leftKey = leftKeys[i];
            var rightKey = rightKeys[i];

            if (leftKey.Type != rightKey.Type)
            {
                return false;
            }

            if (!string.Equals(leftKey.Value, rightKey.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SemanticIdContainsValue(IReference? semanticId, string semanticIdValue)
    {
        if (semanticId == null || string.IsNullOrWhiteSpace(semanticIdValue))
        {
            return false;
        }

        var keys = semanticId.Keys;
        if (keys == null)
        {
            return false;
        }

        foreach (var key in keys)
        {
            if (key?.Value != null && string.Equals(key.Value, semanticIdValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<ISubmodelElement> Elements(SubmodelElementCollection? collection)
    {
        return collection?.Values ?? Enumerable.Empty<ISubmodelElement>();
    }

    private static IEnumerable<ISubmodelElement> EnumerateElementsRecursive(SubmodelElementCollection collection)
    {
        foreach (var element in Elements(collection))
        {
            if (element == null)
            {
                continue;
            }

            yield return element;

            if (element is SubmodelElementCollection nestedCollection)
            {
                foreach (var nested in EnumerateElementsRecursive(nestedCollection))
                {
                    yield return nested;
                }
            }
            else if (element is SubmodelElementList nestedList)
            {
                foreach (var nested in EnumerateElementsRecursive(nestedList))
                {
                    yield return nested;
                }
            }
        }
    }

    private static IEnumerable<ISubmodelElement> EnumerateElementsRecursive(SubmodelElementList list)
    {
        foreach (var element in list)
        {
            if (element == null)
            {
                continue;
            }

            yield return element;

            if (element is SubmodelElementCollection nestedCollection)
            {
                foreach (var nested in EnumerateElementsRecursive(nestedCollection))
                {
                    yield return nested;
                }
            }
            else if (element is SubmodelElementList nestedList)
            {
                foreach (var nested in EnumerateElementsRecursive(nestedList))
                {
                    yield return nested;
                }
            }
        }
    }

    public static Reference? GetReference(this ReferenceElement? referenceElement)
    {
        if (referenceElement == null) return null;
        // ReferenceElement.Value is a ReferenceElementValue -> unwrap to Reference
        var raw = AasValueUnwrap.Unwrap(referenceElement.Value);
        return raw as Reference;
    }
}
