using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using BaSyx.Models.AdminShell;

namespace BaSyx.Models.Extensions;

public sealed class QualifierJsonConverter : JsonConverter<IQualifier>
{
    public override bool CanConvert(Type typeToConvert) => typeof(IQualifier).IsAssignableFrom(typeToConvert);

    public override IQualifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null!;
        }

        try
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            var qualifier = new Qualifier();

            if (root.TryGetProperty("kind", out var kindElement))
            {
                if (kindElement.ValueKind == JsonValueKind.String &&
                    Enum.TryParse<QualifierKind>(kindElement.GetString(), true, out var parsedKind))
                {
                    qualifier.Kind = parsedKind;
                }
                else if (kindElement.ValueKind == JsonValueKind.Number && kindElement.TryGetInt32(out var kindInt))
                {
                    qualifier.Kind = (QualifierKind)kindInt;
                }
            }

            if (root.TryGetProperty("type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String)
            {
                qualifier.Type = typeElement.GetString();
            }

            if (root.TryGetProperty("valueType", out var valueTypeElement) && valueTypeElement.ValueKind == JsonValueKind.String)
            {
                var vt = valueTypeElement.GetString() ?? string.Empty;
                if (vt.StartsWith("xs:", StringComparison.OrdinalIgnoreCase))
                {
                    vt = vt[3..];
                }

                // DataType is handled by BaSyx converters; keep it consistent with their parsing.
                qualifier.ValueType = JsonSerializer.Deserialize<DataType>($"\"{vt}\"", options);
            }

            if (root.TryGetProperty("value", out var valueElement) && valueElement.ValueKind != JsonValueKind.Null)
            {
                qualifier.Value = valueElement.ValueKind switch
                {
                    JsonValueKind.String => valueElement.GetString(),
                    JsonValueKind.Number => valueElement.TryGetInt64(out var longVal) ? longVal : valueElement.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => valueElement.ToString()
                };
            }

            if (root.TryGetProperty("valueId", out var valueIdElement) && valueIdElement.ValueKind != JsonValueKind.Null)
            {
                qualifier.ValueId = valueIdElement.Deserialize<IReference>(options);
            }

            if (root.TryGetProperty("semanticId", out var semanticIdElement) && semanticIdElement.ValueKind != JsonValueKind.Null)
            {
                qualifier.SemanticId = semanticIdElement.Deserialize<IReference>(options);
            }

            if (root.TryGetProperty("supplementalSemanticIds", out var suppElement) && suppElement.ValueKind == JsonValueKind.Array)
            {
                qualifier.SupplementalSemanticIds = suppElement.Deserialize<IEnumerable<IReference>>(options);
            }

            return qualifier;
        }
        catch
        {
            // Be lenient: keep deserialization resilient.
            return new Qualifier();
        }
    }

    public override void Write(Utf8JsonWriter writer, IQualifier value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.Kind != null)
        {
            writer.WriteString("kind", value.Kind.ToString());
        }

        if (!string.IsNullOrEmpty(value.Type))
        {
            writer.WriteString("type", value.Type);
        }

        if (value.ValueType != null)
        {
            writer.WriteString("valueType", value.ValueType.ToString());
        }

        if (value.Value != null)
        {
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.Value, value.Value.GetType(), options);
        }

        if (value.ValueId != null)
        {
            writer.WritePropertyName("valueId");
            JsonSerializer.Serialize(writer, value.ValueId, options);
        }

        if (value.SemanticId != null)
        {
            writer.WritePropertyName("semanticId");
            JsonSerializer.Serialize(writer, value.SemanticId, options);
        }

        if (value.SupplementalSemanticIds != null)
        {
            writer.WritePropertyName("supplementalSemanticIds");
            JsonSerializer.Serialize(writer, value.SupplementalSemanticIds, options);
        }

        writer.WriteEndObject();
    }
}
