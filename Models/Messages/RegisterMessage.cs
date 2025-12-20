using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using BaSyx.Models.AdminShell;

namespace AAS_Sharp_Client.Models.Messages
{
    /// <summary>
    /// Standardisierte Registrierungsnachricht zwischen Agenten im hierarchischen System.
    /// Jeder Agent registriert sich bei seinem übergeordneten Parent-Agenten:
    /// - P102_Execution/Planning Agent → P102 (Topic: {ns}/P102/Register)
    /// - P102 → DispatchingAgent (Topic: {ns}/DispatchingAgent/Register)
    /// - DispatchingAgent → Namespace (Topic: {ns}/Register)
    /// </summary>
    public class RegisterMessage
    {
        /// <summary>
        /// Zeitstempel der Registrierung (ISO 8601 Format)
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Eindeutige ID des sich registrierenden Agenten
        /// </summary>
        [JsonPropertyName("agentId")]
        public string AgentId { get; set; } = string.Empty;

        /// <summary>
        /// Liste der direkten Subagenten (keine Sub-Subs)
        /// </summary>
        [JsonPropertyName("subagents")]
        public List<string> Subagents { get; set; }

        /// <summary>
        /// Namen der CapabilityContainer (nur Namen, nicht die vollständigen Container)
        /// </summary>
        [JsonPropertyName("capabilities")]
        public List<string> Capabilities { get; set; }

        public RegisterMessage()
        {
            Timestamp = DateTime.UtcNow;
            Subagents = new List<string>();
            Capabilities = new List<string>();
        }

        public RegisterMessage(string agentId, List<string> subagents, List<string> capabilities)
        {
            Timestamp = DateTime.UtcNow;
            AgentId = agentId;
            Subagents = subagents ?? new List<string>();
            Capabilities = capabilities ?? new List<string>();
        }

        /// <summary>
        /// Erstellt das Topic für die Registrierung beim Parent-Agenten
        /// Format: {namespace}/{parentAgent}/Register
        /// </summary>
        /// <param name="namespace">Der Namespace (z.B. "phuket")</param>
        /// <param name="parentAgent">Der übergeordnete Agent, bei dem sich registriert wird (z.B. "DispatchingAgent", "P102")</param>
        /// <returns>Das vollständige MQTT-Topic für die Registrierung beim Parent</returns>
        public static string GetRegisterTopic(string @namespace, string parentAgent)
        {
            // Wenn Parent leer ist, registriert sich direkt beim Namespace
            if (string.IsNullOrEmpty(parentAgent))
            {
                return $"{@namespace}/Register";
            }
            return $"{@namespace}/{parentAgent}/Register";
        }

        /// <summary>
        /// Erstellt eine RegisterMessage aus BaSyx SubmodelElementCollection
        /// </summary>
        /// <param name="collection">Die SubmodelElementCollection mit Registrierungsdaten</param>
        /// <returns>RegisterMessage-Instanz</returns>
        public static RegisterMessage FromSubmodelElementCollection(SubmodelElementCollection collection)
        {
            var message = new RegisterMessage();

            foreach (var element in collection.Value?.Value ?? Enumerable.Empty<ISubmodelElement>())
            {
                switch (element.IdShort?.ToLower())
                {
                    case "timestamp":
                        if (element is Property timestampProp)
                        {
                            var rawObj = timestampProp.Value?.Value;
                            string? raw = null;
                            if (rawObj is BaSyx.Models.AdminShell.IValue inner)
                            {
                                raw = inner.Value?.ToString();
                            }
                            else
                            {
                                raw = rawObj?.ToString() ?? timestampProp.Value?.ToString();
                            }

                            if (!string.IsNullOrWhiteSpace(raw) && DateTime.TryParse(raw, out var timestamp))
                            {
                                message.Timestamp = timestamp;
                            }
                        }
                        break;

                    case "agentid":
                        if (element is Property agentIdProp)
                        {
                            var rawObj = agentIdProp.Value?.Value;
                            string? raw = null;
                            if (rawObj is BaSyx.Models.AdminShell.IValue inner)
                            {
                                raw = inner.Value?.ToString();
                            }
                            else
                            {
                                raw = rawObj?.ToString() ?? agentIdProp.Value?.ToString();
                            }
                            message.AgentId = raw ?? string.Empty;
                        }
                        break;

                    case "subagents":
                        if (element is SubmodelElementCollection subagentsCollection)
                        {
                            message.Subagents = ExtractStringList(subagentsCollection);
                        }
                        break;

                    case "capabilities":
                        if (element is SubmodelElementCollection capabilitiesCollection)
                        {
                            message.Capabilities = ExtractStringList(capabilitiesCollection);
                        }
                        break;
                }
            }

            return message;
        }

        /// <summary>
        /// Konvertiert diese RegisterMessage in eine BaSyx SubmodelElementCollection
        /// </summary>
        /// <returns>SubmodelElementCollection mit allen Registrierungsdaten</returns>
        public SubmodelElementCollection ToSubmodelElementCollection()
        {
            var collection = new SubmodelElementCollection("RegisterMessage");

            // Timestamp
            var timestampProp = new Property("Timestamp", new DataType(DataObjectType.DateTime));
            timestampProp.Value = new PropertyValue<string>(Timestamp.ToString("o"));
            collection.Add(timestampProp);

            // AgentId
            var agentIdProp = new Property("AgentId", new DataType(DataObjectType.String));
            agentIdProp.Value = new PropertyValue<string>(AgentId);
            collection.Add(agentIdProp);

            // Subagents
            var subagentsCollection = new SubmodelElementCollection("Subagents");
            for (int i = 0; i < Subagents.Count; i++)
            {
                var subagentProp = new Property($"Subagent_{i}", new DataType(DataObjectType.String));
                subagentProp.Value = new PropertyValue<string>(Subagents[i]);
                subagentsCollection.Add(subagentProp);
            }
            collection.Add(subagentsCollection);

            // Capabilities
            var capabilitiesCollection = new SubmodelElementCollection("Capabilities");
            for (int i = 0; i < Capabilities.Count; i++)
            {
                var capabilityProp = new Property($"Capability_{i}", new DataType(DataObjectType.String));
                capabilityProp.Value = new PropertyValue<string>(Capabilities[i]);
                capabilitiesCollection.Add(capabilityProp);
            }
            collection.Add(capabilitiesCollection);

            return collection;
        }

        /// <summary>
        /// Hilfsmethode zum Extrahieren einer String-Liste aus einer SubmodelElementCollection
        /// </summary>
        private static List<string> ExtractStringList(SubmodelElementCollection collection)
        {
            var list = new List<string>();

            foreach (var element in collection.Value?.Value ?? Enumerable.Empty<ISubmodelElement>())
            {
                if (element is Property property)
                {
                    var rawObj = property.Value?.Value;
                    string? value = null;
                    if (rawObj is BaSyx.Models.AdminShell.IValue inner)
                    {
                        value = inner.Value?.ToString();
                    }
                    else
                    {
                        value = rawObj?.ToString() ?? property.Value?.ToString();
                    }

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        list.Add(value);
                    }
                }
            }

            return list;
        }
    }
}
