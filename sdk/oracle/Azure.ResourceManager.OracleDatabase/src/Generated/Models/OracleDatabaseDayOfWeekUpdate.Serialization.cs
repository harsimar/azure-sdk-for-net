// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Text.Json;
using Azure.Core;

namespace Azure.ResourceManager.OracleDatabase.Models
{
    public partial class OracleDatabaseDayOfWeekUpdate : IUtf8JsonSerializable, IJsonModel<OracleDatabaseDayOfWeekUpdate>
    {
        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer) => ((IJsonModel<OracleDatabaseDayOfWeekUpdate>)this).Write(writer, ModelSerializationExtensions.WireOptions);

        void IJsonModel<OracleDatabaseDayOfWeekUpdate>.Write(Utf8JsonWriter writer, ModelReaderWriterOptions options)
        {
            writer.WriteStartObject();
            JsonModelWriteCore(writer, options);
            writer.WriteEndObject();
        }

        /// <param name="writer"> The JSON writer. </param>
        /// <param name="options"> The client options for reading and writing models. </param>
        protected virtual void JsonModelWriteCore(Utf8JsonWriter writer, ModelReaderWriterOptions options)
        {
            var format = options.Format == "W" ? ((IPersistableModel<OracleDatabaseDayOfWeekUpdate>)this).GetFormatFromOptions(options) : options.Format;
            if (format != "J")
            {
                throw new FormatException($"The model {nameof(OracleDatabaseDayOfWeekUpdate)} does not support writing '{format}' format.");
            }

            writer.WritePropertyName("name"u8);
            writer.WriteStringValue(Name.ToString());
            if (options.Format != "W" && _serializedAdditionalRawData != null)
            {
                foreach (var item in _serializedAdditionalRawData)
                {
                    writer.WritePropertyName(item.Key);
#if NET6_0_OR_GREATER
				writer.WriteRawValue(item.Value);
#else
                    using (JsonDocument document = JsonDocument.Parse(item.Value, ModelSerializationExtensions.JsonDocumentOptions))
                    {
                        JsonSerializer.Serialize(writer, document.RootElement);
                    }
#endif
                }
            }
        }

        OracleDatabaseDayOfWeekUpdate IJsonModel<OracleDatabaseDayOfWeekUpdate>.Create(ref Utf8JsonReader reader, ModelReaderWriterOptions options)
        {
            var format = options.Format == "W" ? ((IPersistableModel<OracleDatabaseDayOfWeekUpdate>)this).GetFormatFromOptions(options) : options.Format;
            if (format != "J")
            {
                throw new FormatException($"The model {nameof(OracleDatabaseDayOfWeekUpdate)} does not support reading '{format}' format.");
            }

            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            return DeserializeOracleDatabaseDayOfWeekUpdate(document.RootElement, options);
        }

        internal static OracleDatabaseDayOfWeekUpdate DeserializeOracleDatabaseDayOfWeekUpdate(JsonElement element, ModelReaderWriterOptions options = null)
        {
            options ??= ModelSerializationExtensions.WireOptions;

            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            OracleDatabaseDayOfWeekName name = default;
            IDictionary<string, BinaryData> serializedAdditionalRawData = default;
            Dictionary<string, BinaryData> rawDataDictionary = new Dictionary<string, BinaryData>();
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("name"u8))
                {
                    name = new OracleDatabaseDayOfWeekName(property.Value.GetString());
                    continue;
                }
                if (options.Format != "W")
                {
                    rawDataDictionary.Add(property.Name, BinaryData.FromString(property.Value.GetRawText()));
                }
            }
            serializedAdditionalRawData = rawDataDictionary;
            return new OracleDatabaseDayOfWeekUpdate(name, serializedAdditionalRawData);
        }

        BinaryData IPersistableModel<OracleDatabaseDayOfWeekUpdate>.Write(ModelReaderWriterOptions options)
        {
            var format = options.Format == "W" ? ((IPersistableModel<OracleDatabaseDayOfWeekUpdate>)this).GetFormatFromOptions(options) : options.Format;

            switch (format)
            {
                case "J":
                    return ModelReaderWriter.Write(this, options, AzureResourceManagerOracleDatabaseContext.Default);
                default:
                    throw new FormatException($"The model {nameof(OracleDatabaseDayOfWeekUpdate)} does not support writing '{options.Format}' format.");
            }
        }

        OracleDatabaseDayOfWeekUpdate IPersistableModel<OracleDatabaseDayOfWeekUpdate>.Create(BinaryData data, ModelReaderWriterOptions options)
        {
            var format = options.Format == "W" ? ((IPersistableModel<OracleDatabaseDayOfWeekUpdate>)this).GetFormatFromOptions(options) : options.Format;

            switch (format)
            {
                case "J":
                    {
                        using JsonDocument document = JsonDocument.Parse(data, ModelSerializationExtensions.JsonDocumentOptions);
                        return DeserializeOracleDatabaseDayOfWeekUpdate(document.RootElement, options);
                    }
                default:
                    throw new FormatException($"The model {nameof(OracleDatabaseDayOfWeekUpdate)} does not support reading '{options.Format}' format.");
            }
        }

        string IPersistableModel<OracleDatabaseDayOfWeekUpdate>.GetFormatFromOptions(ModelReaderWriterOptions options) => "J";
    }
}
