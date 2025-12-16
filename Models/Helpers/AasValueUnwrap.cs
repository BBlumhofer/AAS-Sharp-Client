using System;
using System.Reflection;
using BaSyx.Models.AdminShell;

namespace AasSharpClient.Models.Helpers;

public static class AasValueUnwrap
{
    /// <summary>
    /// Best-effort unwrapping of nested BaSyx wrapper types.
    /// Handles chains like Property -&gt; PropertyValue -&gt; ElementValue&lt;T&gt; -&gt; primitive/string.
    /// </summary>
    public static object? Unwrap(object? value, int maxDepth = 8)
    {
        object? current = value;

        for (var depth = 0; depth < maxDepth && current != null; depth++)
        {
            if (current is IValue iv)
            {
                current = iv.Value;
                continue;
            }

            var t = current.GetType();

            // Prefer a non-indexed instance property named "Value".
            var valueProp = FindValueProperty(t);
            if (valueProp == null)
            {
                return current;
            }

            current = valueProp.GetValue(current);
        }

        return current;
    }

    public static string? UnwrapToString(object? value)
    {
        var unwrapped = Unwrap(value);
        return unwrapped?.ToString();
    }

    public static int? UnwrapToInt(object? value)
    {
        var unwrapped = Unwrap(value);
        if (unwrapped == null) return null;
        if (unwrapped is int i) return i;
        return int.TryParse(unwrapped.ToString(), out var parsed) ? parsed : null;
    }

    public static long? UnwrapToLong(object? value)
    {
        var unwrapped = Unwrap(value);
        if (unwrapped == null) return null;
        if (unwrapped is long l) return l;
        return long.TryParse(unwrapped.ToString(), out var parsed) ? parsed : null;
    }

    public static double? UnwrapToDouble(object? value)
    {
        var unwrapped = Unwrap(value);
        if (unwrapped == null) return null;
        if (unwrapped is double d) return d;
        if (unwrapped is float f) return f;
        if (unwrapped is decimal m) return (double)m;
        if (unwrapped is int i) return i;
        if (unwrapped is long l) return l;

        return double.TryParse(
            unwrapped.ToString(),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : null;
    }

    public static bool? UnwrapToBool(object? value)
    {
        var unwrapped = Unwrap(value);
        if (unwrapped == null) return null;
        if (unwrapped is bool b) return b;
        return bool.TryParse(unwrapped.ToString(), out var parsed) ? parsed : null;
    }

    public static System.Collections.Generic.IEnumerable<T> UnwrapToEnumerable<T>(object? value)
    {
        var unwrapped = Unwrap(value);
        if (unwrapped is System.Collections.Generic.IEnumerable<T> typed)
        {
            foreach (var item in typed)
            {
                yield return item;
            }

            yield break;
        }

        if (unwrapped is System.Collections.IEnumerable untyped)
        {
            foreach (var item in untyped)
            {
                if (item is T t)
                {
                    yield return t;
                }
            }
        }
    }

    private static PropertyInfo? FindValueProperty(Type t)
    {
        // GetProperty("Value") can throw AmbiguousMatchException on some BaSyx types.
        foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (p.Name == "Value" && p.GetIndexParameters().Length == 0)
            {
                return p;
            }
        }

        return null;
    }
}
