using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BaSyx.Models.AdminShell;
using BaSyx.Models.Extensions;

namespace AasSharpClient.Models;

public static class SubmodelSerialization
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new FullSubmodelElementConverter(new ConverterOptions()), new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static Task<string> SerializeAsync(Submodel submodel, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => SerializeInternal(submodel), cancellationToken);
    }

    public static string Serialize(Submodel submodel) => SerializeInternal(submodel);

    public static string SerializeElements(IEnumerable<ISubmodelElement> elements)
    {
        var json = JsonSerializer.Serialize(elements, Options);
        return NormalizeMultiLanguageValues(json);
    }

    private static string SerializeInternal(Submodel submodel)
    {
        var json = JsonSerializer.Serialize(submodel, Options);
        return NormalizeMultiLanguageValues(json);
    }

    private static string NormalizeMultiLanguageValues(string json)
    {
        JsonNode? node;
        try
        {
            node = JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            return json;
        }

        if (node is null)
        {
            return json;
        }

        NormalizeNode(node);
        var normalized = node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        // Remove trailing slash from anyURI values that are only host:port (e.g. "http://host:4845/")
        var sanitized = normalized.Replace("\"inOutputVariables\"", "\"inoutputVariables\"", System.StringComparison.Ordinal);
        sanitized = Regex.Replace(sanitized, "\"(https?://[^/\\\"]+:\\d+)/\"", "\"$1\"");
        System.IO.File.WriteAllText("skills_actual.json", sanitized);

        if (sanitized.Contains("\"inOutputVariables\"", System.StringComparison.Ordinal))
        {
            throw new InvalidOperationException("SubmodelSerialization failed to rename inOutputVariables.");
        }

        return sanitized;
    }

    private static void NormalizeNode(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            if (obj.TryGetPropertyValue("modelType", out var modelTypeNode) &&
                string.Equals(modelTypeNode?.GetValue<string>(), "MultiLanguageProperty", StringComparison.OrdinalIgnoreCase))
            {
                NormalizeMultiLanguageValue(obj);
            }

            var propertiesToRename = new List<(string OldKey, string NewKey, JsonNode? Value)>();
            foreach (var property in obj)
            {
                if (string.Equals(property.Key, "inOutputVariables", StringComparison.Ordinal))
                {
                    propertiesToRename.Add((property.Key, "inoutputVariables", property.Value));
                }
            }

            foreach (var rename in propertiesToRename)
            {
                obj.Remove(rename.OldKey);
                obj[rename.NewKey] = rename.Value;
            }

            foreach (var property in obj)
            {
                if (string.Equals(property.Key, "valueTypeListElement", StringComparison.Ordinal) &&
                    property.Value is JsonValue valueNode &&
                    valueNode.TryGetValue<string>(out var valueType) &&
                    !string.IsNullOrEmpty(valueType) &&
                    !valueType.StartsWith("xs:", StringComparison.OrdinalIgnoreCase))
                {
                    obj[property.Key] = $"xs:{valueType}";
                }

                // Trim trailing slash for anyURI string values to match templates exactly
                if (string.Equals(property.Key, "valueType", StringComparison.Ordinal) &&
                    property.Value is JsonValue vtNode &&
                    vtNode.TryGetValue<string>(out var vtString) &&
                    !string.IsNullOrEmpty(vtString) &&
                    (vtString.Equals("anyURI", StringComparison.OrdinalIgnoreCase) || vtString.Equals("xs:anyURI", StringComparison.OrdinalIgnoreCase)))
                {
                    if (obj.TryGetPropertyValue("value", out var valueNode2) && valueNode2 is JsonValue vNode && vNode.TryGetValue<string>(out var vString) && vString != null)
                    {
                        var trimmed = vString.EndsWith("/") ? vString.TrimEnd('/') : vString;
                        if (!string.Equals(trimmed, vString, StringComparison.Ordinal))
                        {
                            obj["value"] = trimmed;
                        }
                    }
                }

                NormalizeNode(property.Value);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var element in array)
            {
                NormalizeNode(element);
            }
        }
    }

    private static void NormalizeMultiLanguageValue(JsonObject container)
    {
        if (!container.TryGetPropertyValue("value", out var valueNode) || valueNode is not JsonArray array)
        {
            return;
        }

        for (int i = 0; i < array.Count; i++)
        {
            if (array[i] is not JsonObject entry)
            {
                continue;
            }

            if (entry.ContainsKey("language") && entry.ContainsKey("text"))
            {
                continue;
            }

            string? language = null;
            string? text = null;

            foreach (var property in entry)
            {
                language = property.Key;
                text = property.Value?.GetValue<string>();
                break;
            }

            if (language is null)
            {
                continue;
            }

            var normalized = new JsonObject
            {
                ["language"] = language,
                ["text"] = text ?? string.Empty
            };

            array[i] = normalized;
        }
    }
}
