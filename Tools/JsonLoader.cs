using System;
using System.IO;
using System.Text;
using System.Text.Json;
using BaSyx.Models.AdminShell;
using BaSyx.Models.Extensions;

namespace AasSharpClient.Tools;

public static class JsonLoader
{
    public static readonly JsonSerializerOptions Options = CreateOptions();

    public static string SerializeElement(ISubmodelElement element, bool indented = false)
    {
        if (element == null)
        {
            return "null";
        }

        var options = new JsonSerializerOptions(Options)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize<ISubmodelElement>(element, options);
    }

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

    public static JsonElement LoadJsonElementFromFile(string path)
    {
        var fullPath = ResolvePath(path);
        using var doc = JsonDocument.Parse(File.ReadAllText(fullPath));
        return doc.RootElement.Clone();
    }

    public static ISubmodelElement? LoadElementFromFile(string path)
    {
        var fullPath = ResolvePath(path);
        var json = File.ReadAllText(fullPath);
        return DeserializeElement(json);
    }

    public static SubmodelElementCollection LoadCollectionFromFile(string path)
    {
        var fullPath = ResolvePath(path);
        using var doc = JsonDocument.Parse(File.ReadAllText(fullPath));
        return DeserializeCollection(doc.RootElement);
    }

    public static ISubmodelElement? DeserializeElement(JsonElement element) => DeserializeElement(element.GetRawText());

    public static ISubmodelElement? DeserializeElement(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ISubmodelElement>(json, Options);
        }
        catch
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
                    // fall through
                }
            }

            using var doc = JsonDocument.Parse(json);
            return CreateFallbackElement(doc.RootElement);
        }
    }

    public static SubmodelElementCollection DeserializeCollection(JsonElement element)
    {
        var idShort = element.TryGetProperty("idShort", out var idShortNode)
            ? idShortNode.GetString() ?? "Collection"
            : "Collection";

        var collection = new SubmodelElementCollection(idShort);

        if (element.TryGetProperty("value", out var valueNode) && valueNode.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in valueNode.EnumerateArray())
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
            var value = element.TryGetProperty("value", out var valueNode)
                ? (valueNode.ValueKind == JsonValueKind.String ? valueNode.GetString() ?? string.Empty : valueNode.GetRawText())
                : string.Empty;

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

    private static string ResolvePath(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        var fullPath = Path.GetFullPath(path);
        if (File.Exists(fullPath))
        {
            return fullPath;
        }

        // Convenience for callers that pass repo-relative paths while executing from bin/...
        var baseDir = AppContext.BaseDirectory;
        var candidate = Path.GetFullPath(Path.Combine(baseDir, path));
        if (File.Exists(candidate))
        {
            return candidate;
        }

        return fullPath;
    }
}
