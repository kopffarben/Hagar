using System;
using Hagar.Buffers;
using Hagar.Codecs;
using Hagar.WireProtocol;

namespace Hagar.Serializers
{
    /// <summary>
    /// Serializer for value types.
    /// </summary>
    /// <typeparam name="TField">The field type.</typeparam>
    /// <typeparam name="TValueSerializer">The value-type serializer implementation type.</typeparam>
    public sealed class ValueSerializer<TField, TValueSerializer> : IFieldCodec<TField> where TField : struct where TValueSerializer : IValueSerializer<TField>
    {
        private static readonly Type CodecFieldType = typeof(TField);
        private readonly TValueSerializer serializer;

        public ValueSerializer(TValueSerializer serializer)
        {
            this.serializer = serializer;
        }

        void IFieldCodec<TField>.WriteField<TBufferWriter>(ref Writer<TBufferWriter> writer, uint fieldIdDelta, Type expectedType, TField value)
        {
            ReferenceCodec.MarkValueField(writer.Session);
            writer.WriteStartObject(fieldIdDelta, expectedType, CodecFieldType);
            this.serializer.Serialize(ref writer, in value);
            writer.WriteEndObject();
        }

        TField IFieldCodec<TField>.ReadValue(ref Reader reader, Field field)
        {
            ReferenceCodec.MarkValueField(reader.Session);
            var value = default(TField);
            this.serializer.Deserialize(ref reader, ref value);
            return value;
        }
    }
}