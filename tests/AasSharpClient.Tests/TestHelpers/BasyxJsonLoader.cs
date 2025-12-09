using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BaSyx.Models.AdminShell;
using BaSyx.Models.Extensions;

namespace AasSharpClient.Tests;

internal static class BasyxJsonLoader
{
    internal static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var builder = new DefaultJsonSerializerOptions();
        builder.AddFullSubmodelElementConverter();

        var options = builder.Build();
        options.PropertyNameCaseInsensitive = true;
        options.Converters.Add(new ReferenceJsonConverter());
        options.Converters.Add(new OperationVariableSetJsonConverter());
        options.Converters.Add(new QualifierJsonConverter());

        return options;
    }

    internal static ISubmodelElement? LoadElementFromFile(string relativePath)
    {
        var fullPath = ResolvePath(relativePath);
        var json = File.ReadAllText(fullPath);
        return DeserializeElement(json);
    }

    internal static SubmodelElementCollection LoadCollectionFromFile(string relativePath)
    {
        var fullPath = ResolvePath(relativePath);
        using var doc = JsonDocument.Parse(File.ReadAllText(fullPath));
        var root = doc.RootElement;

        var idShort = root.TryGetProperty("idShort", out var idShortNode)
            ? idShortNode.GetString() ?? "Collection"
            : "Collection";

        var collection = new SubmodelElementCollection(idShort);

        if (root.TryGetProperty("value", out var val) && val.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in val.EnumerateArray())
            {
                var sme = DeserializeElement(element);
                if (sme != null)
                {
                    collection.Add(sme);
                    continue;
                }

                var fallback = CreateFallbackElement(element);
                if (fallback != null)
                {
                    collection.Add(fallback);
                }
            }
        }

        return collection;
    }

    private static ISubmodelElement? CreateFallbackElement(JsonElement element)
    {
        if (!element.TryGetProperty("idShort", out var idShortNode))
        {
            return null;
        }

        var idShort = idShortNode.GetString() ?? "";
        var modelType = element.TryGetProperty("modelType", out var mtNode) ? mtNode.GetString() : null;

        if (string.Equals(modelType, "SubmodelElementCollection", StringComparison.OrdinalIgnoreCase))
        {
            return BuildNestedCollection(element, idShort);
        }

        if (string.Equals(modelType, "Property", StringComparison.OrdinalIgnoreCase))
        {
            var value = element.TryGetProperty("value", out var valueNode) ? valueNode.GetString() ?? string.Empty : string.Empty;
            return new Property<string>(idShort, value);
        }

        return null;
    }

    private static SubmodelElementCollection BuildNestedCollection(JsonElement element, string idShort)
    {
        var collection = new SubmodelElementCollection(idShort);
        if (element.TryGetProperty("value", out var valNode) && valNode.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in valNode.EnumerateArray())
            {
                var sme = DeserializeElement(child) ?? CreateFallbackElement(child);
                if (sme != null)
                {
                    collection.Add(sme);
                }
            }
        }

        return collection;
    }

    private static string ResolvePath(string relativePath)
    {
        var baseDir = AppContext.BaseDirectory; // bin/Debug/netX.X
        var candidate = Path.Combine(baseDir, "TestData", relativePath);
        if (File.Exists(candidate))
        {
            return candidate;
        }

        // Fallback to project-root relative
        var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
        var fallback = Path.Combine(projectRoot, "tests", "AasSharpClient.Tests", "TestData", relativePath);
        return fallback;
    }

    internal static ISubmodelElement? DeserializeElement(JsonElement element) => DeserializeElement(element.GetRawText());

    internal static ISubmodelElement? DeserializeElement(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ISubmodelElement>(json, Options);
        }
        catch (Exception)
        {
            var patched = EnsureModelType(json);
            if (!ReferenceEquals(patched, json))
            {
                try
                {
                    return JsonSerializer.Deserialize<ISubmodelElement>(patched, Options);
                }
                catch
                {
                    // fall through to structural fallback
                }
            }

            using var doc = JsonDocument.Parse(json);
            return CreateFallbackElement(doc.RootElement);
        }
    }

    private static string EnsureModelType(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
        {
            return json;
        }

        if (doc.RootElement.TryGetProperty("modelType", out _))
        {
            return json;
        }

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                property.WriteTo(writer);
            }
            writer.WriteString("modelType", "SubmodelElementCollection");
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}