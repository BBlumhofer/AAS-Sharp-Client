/*******************************************************************************
* Copyright (c) 2024 Bosch Rexroth AG
* Author: Constantin Ziesche (constantin.ziesche@bosch.com)
*
* This program and the accompanying materials are made available under the
* terms of the MIT License which is available at
* https://github.com/eclipse-basyx/basyx-dotnet/blob/main/LICENSE
*
* SPDX-License-Identifier: MIT
*******************************************************************************/
using BaSyx.Models.AdminShell;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaSyx.Models.Extensions
{
    public class QualifierJsonConverter : JsonConverter<IQualifier>
    {
        public override bool CanConvert(Type typeToConvert) => typeof(IQualifier).IsAssignableFrom(typeToConvert);

        public override IQualifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null!;

            try
            {
                using JsonDocument document = JsonDocument.ParseValue(ref reader);
                JsonElement root = document.RootElement;

                Qualifier qualifier = new Qualifier();

                if (root.TryGetProperty("kind", out JsonElement kindElement))
                {
                    if (kindElement.ValueKind == JsonValueKind.String && Enum.TryParse(kindElement.GetString(), true, out QualifierKind parsedKind))
                        qualifier.Kind = parsedKind;
                    else if (kindElement.ValueKind == JsonValueKind.Number && kindElement.TryGetInt32(out int kindInt))
                        qualifier.Kind = (QualifierKind)kindInt;
                }

                if (root.TryGetProperty("type", out JsonElement typeElement) && typeElement.ValueKind == JsonValueKind.String)
                    qualifier.Type = typeElement.GetString();

                if (root.TryGetProperty("valueType", out JsonElement valueTypeElement) && valueTypeElement.ValueKind == JsonValueKind.String)
                {
                    string vt = valueTypeElement.GetString() ?? string.Empty;
                    if (vt.StartsWith("xs:", StringComparison.OrdinalIgnoreCase))
                        vt = vt.Substring(3);

                    qualifier.ValueType = JsonSerializer.Deserialize<DataType>(JsonDocument.Parse($"\"{vt}\"").RootElement.GetRawText(), options);
                }

                if (root.TryGetProperty("value", out JsonElement valueElement) && valueElement.ValueKind != JsonValueKind.Null)
                {
                    qualifier.Value = valueElement.ValueKind switch
                    {
                        JsonValueKind.String => valueElement.GetString(),
                        JsonValueKind.Number => valueElement.TryGetInt64(out long longVal) ? longVal : valueElement.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => valueElement.ToString()
                    };
                }

                if (root.TryGetProperty("valueId", out JsonElement valueIdElement) && valueIdElement.ValueKind != JsonValueKind.Null)
                    qualifier.ValueId = valueIdElement.Deserialize<IReference>(options);

                if (root.TryGetProperty("semanticId", out JsonElement semanticIdElement) && semanticIdElement.ValueKind != JsonValueKind.Null)
                    qualifier.SemanticId = semanticIdElement.Deserialize<IReference>(options);

                if (root.TryGetProperty("supplementalSemanticIds", out JsonElement suppSemIdsElement) && suppSemIdsElement.ValueKind == JsonValueKind.Array)
                    qualifier.SupplementalSemanticIds = suppSemIdsElement.Deserialize<IEnumerable<IReference>>(options);

                return qualifier;
            }
            catch
            {
                // Return an empty qualifier if the payload is malformed to avoid hard failures during deserialization
                return new Qualifier();
            }
        }

        public override void Write(Utf8JsonWriter writer, IQualifier value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value.Kind != null)
                writer.WriteString("kind", value.Kind.ToString());

            if (!string.IsNullOrEmpty(value.Type))
                writer.WriteString("type", value.Type);

            if (value.ValueType != null)
                writer.WriteString("valueType", value.ValueType.ToString());

            if (value.Value != null)
            {
                writer.WritePropertyName("value");
                JsonSerializer.Serialize(writer, value.Value, value.Value.GetType());
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
}
