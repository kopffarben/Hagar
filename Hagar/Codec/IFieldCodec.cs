using System;
using Hagar.Session;
using Hagar.Utilities;
using Hagar.WireProtocol;

namespace Hagar.Codec
{
    public interface IFieldCodec<T>
    {
        void WriteField(Writer writer, SerializerSession session, uint fieldId, Type expectedType, T value);
        T ReadValue(Reader reader, SerializerSession session, Field field);
    }
}