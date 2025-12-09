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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaSyx.Models.Extensions
{
    public class OperationVariableSetJsonConverter : JsonConverter<IOperationVariableSet>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IOperationVariableSet).IsAssignableFrom(typeToConvert);
        }

        public override IOperationVariableSet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null!;

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("OperationVariableSet must start with array token");

            OperationVariableSet variableSet = new OperationVariableSet();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("OperationVariable entry must be an object");

                using JsonDocument document = JsonDocument.ParseValue(ref reader);
                JsonElement root = document.RootElement;

                if (root.TryGetProperty("value", out JsonElement valueElement) && valueElement.ValueKind != JsonValueKind.Null)
                {
                    ISubmodelElement sme = valueElement.Deserialize<ISubmodelElement>(options)!;
                    variableSet.Add(new OperationVariable() { Value = sme });
                }
            }

            return variableSet;
        }

        public override void Write(Utf8JsonWriter writer, IOperationVariableSet value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (IOperationVariable variable in value)
            {
                if (variable == null)
                    continue;

                writer.WriteStartObject();
                writer.WritePropertyName("value");
                JsonSerializer.Serialize(writer, variable.Value, options);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}
