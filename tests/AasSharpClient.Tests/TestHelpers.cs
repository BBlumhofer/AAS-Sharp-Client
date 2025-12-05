using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using BaSyx.Models.AdminShell;
using Xunit.Sdk;

namespace AasSharpClient.Tests;

internal static class TestHelpers
{
    public static void AssertJsonEqual(string expectedJson, string actualJson)
    {
        using var expectedDoc = JsonDocument.Parse(expectedJson);
        using var actualDoc = JsonDocument.Parse(actualJson);

        if (!JsonEquals(expectedDoc.RootElement, actualDoc.RootElement, "$", out var errorMessage))
        {
            var detail = errorMessage ?? "Generated JSON does not match template.";
            var message = $"{detail}{Environment.NewLine}Expected:{Environment.NewLine}{expectedJson}{Environment.NewLine}Actual:{Environment.NewLine}{actualJson}";
            throw new XunitException(message);
        }
    }

    private static bool JsonEquals(JsonElement expected, JsonElement actual, string path, out string? error)
    {
        if (expected.ValueKind != actual.ValueKind)
        {
            error = $"{path}: Expected kind {expected.ValueKind} but found {actual.ValueKind}.";
            return false;
        }

        switch (expected.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var expectedProperty in expected.EnumerateObject())
                {
                    if (!actual.TryGetProperty(expectedProperty.Name, out var actualValue))
                    {
                        var available = string.Join(", ", actual.EnumerateObject().Select(p => p.Name));
                        Console.WriteLine($"DEBUG Missing '{expectedProperty.Name}' at {path}. Available: [{available}]");
                        error = $"{path}: Missing property '{expectedProperty.Name}' in generated JSON.";
                        return false;
                    }

                    if (!JsonEquals(expectedProperty.Value, actualValue, $"{path}.{expectedProperty.Name}", out error))
                    {
                        return false;
                    }
                }

                error = null;
                return true;
            case JsonValueKind.Array:
                var expectedArray = expected.EnumerateArray().ToArray();
                var actualArray = actual.EnumerateArray().ToArray();
                if (expectedArray.Length != actualArray.Length)
                {
                    error = $"{path}: Expected array length {expectedArray.Length} but found {actualArray.Length}.";
                    return false;
                }

                for (int i = 0; i < expectedArray.Length; i++)
                {
                    if (!JsonEquals(expectedArray[i], actualArray[i], $"{path}[{i}]", out error))
                    {
                        return false;
                    }
                }

                error = null;
                return true;
            case JsonValueKind.String:
                if (!StringEquals(expected.GetString(), actual.GetString(), path))
                {
                    error = $"{path}: Expected '{expected.GetString()}' but found '{actual.GetString()}'.";
                    return false;
                }

                error = null;
                return true;
            case JsonValueKind.Number:
                if (expected.GetRawText() != actual.GetRawText())
                {
                    error = $"{path}: Expected number {expected.GetRawText()} but found {actual.GetRawText()}.'";
                    return false;
                }

                error = null;
                return true;
            case JsonValueKind.True:
            case JsonValueKind.False:
                if (expected.GetBoolean() != actual.GetBoolean())
                {
                    error = $"{path}: Expected {expected.GetBoolean()} but found {actual.GetBoolean()}.'";
                    return false;
                }

                error = null;
                return true;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                error = null;
                return true;
            default:
                if (!string.Equals(expected.ToString(), actual.ToString(), System.StringComparison.Ordinal))
                {
                    error = $"{path}: Values differ.";
                    return false;
                }

                error = null;
                return true;
        }
    }

    private static bool StringEquals(string? expected, string? actual, string path)
    {
        if (IsValueTypePath(path))
        {
            return ValueTypeEquals(expected, actual);
        }

        if (!string.IsNullOrEmpty(expected) && !string.IsNullOrEmpty(actual))
        {
            if (Uri.TryCreate(expected, UriKind.Absolute, out var _) && Uri.TryCreate(actual, UriKind.Absolute, out var _))
            {
                var eTrim = expected.TrimEnd('/');
                var aTrim = actual.TrimEnd('/');
                return string.Equals(eTrim, aTrim, System.StringComparison.Ordinal);
            }
        }

        if (TryParseBoolean(expected, out var expectedBool) && TryParseBoolean(actual, out var actualBool))
        {
            return expectedBool == actualBool;
        }

        return string.Equals(expected, actual, System.StringComparison.Ordinal);
    }

    private static bool IsValueTypePath(string path) => path.EndsWith(".valueType", System.StringComparison.Ordinal);

    private static bool ValueTypeEquals(string? expected, string? actual)
    {
        if (string.Equals(expected, actual, System.StringComparison.Ordinal))
        {
            return true;
        }

        if (TryParseDataObjectType(expected, out var expectedType) && TryParseDataObjectType(actual, out var actualType))
        {
            return expectedType == actualType;
        }

        return false;
    }

    private static bool TryParseDataObjectType(string? value, out DataObjectType dataObjectType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            dataObjectType = DataObjectType.None;
            return false;
        }

        return DataObjectType.TryParse(value, out dataObjectType);
    }

    private static bool TryParseBoolean(string? value, out bool parsed)
    {
        return bool.TryParse(value, out parsed);
    }

    public static LangStringSet Lang(params (string Language, string Text)[] entries)
    {
        return new LangStringSet(entries.Select(entry => new LangString(entry.Language, entry.Text)));
    }
}
