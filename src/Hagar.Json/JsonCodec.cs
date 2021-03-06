﻿using System;
using System.Text;
using Hagar.Buffers;
using Hagar.Codecs;
using Hagar.Serializers;
using Hagar.WireProtocol;
using Newtonsoft.Json;

namespace Hagar.Json
{
    public class JsonCodec : IGeneralizedCodec
    {
        private static readonly Type SelfType = typeof(JsonCodec);
        private readonly Func<Type, bool> isSupportedFunc;
        private readonly JsonSerializerSettings settings;
        
        public JsonCodec(
            JsonSerializerSettings settings = null,
            Func<Type, bool> isSupportedFunc = null)
        {
            this.settings = settings ?? new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
            };
            this.isSupportedFunc = isSupportedFunc ?? (_ => true);
        }

        void IFieldCodec<object>.WriteField<TBufferWriter>(ref Writer<TBufferWriter> writer, uint fieldIdDelta, Type expectedType, object value)
        {
            if (ReferenceCodec.TryWriteReferenceField(ref writer, fieldIdDelta, expectedType, value)) return;
            var result = JsonConvert.SerializeObject(value, this.settings);
            
            // The schema type when serializing the field is the type of the codec.
            // In practice it could be any unique type as long as this codec is registered as the handler.
            // By checking against the codec type in IsSupportedType, the codec could also just be registered as an IGenericCodec.
            // Note that the codec is responsible for serializing the type of the value itself.
            writer.WriteFieldHeader(fieldIdDelta, expectedType, SelfType, WireType.LengthPrefixed);

            // TODO: NoAlloc
            var bytes = Encoding.UTF8.GetBytes(result);
            writer.WriteVarInt((uint)bytes.Length);
            writer.Write(bytes);
        }

        object IFieldCodec<object>.ReadValue(ref Reader reader, Field field)
        {
            if (field.WireType == WireType.Reference)
                return ReferenceCodec.ReadReference<object>(ref reader, field);
            
            if (field.WireType != WireType.LengthPrefixed) ThrowUnsupportedWireTypeException(field);
            var length = reader.ReadVarUInt32();
            var bytes = reader.ReadBytes(length);

            // TODO: NoAlloc
            var resultString = Encoding.UTF8.GetString(bytes);
            var result = JsonConvert.DeserializeObject(resultString, this.settings);
            ReferenceCodec.RecordObject(reader.Session, result);
            return result;
        }

        public bool IsSupportedType(Type type) => type == SelfType || this.isSupportedFunc(type);

        private static void ThrowUnsupportedWireTypeException(Field field) => throw new UnsupportedWireTypeException(
            $"Only a {nameof(WireType)} value of {WireType.LengthPrefixed} is supported for JSON fields. {field}");
    }
}
