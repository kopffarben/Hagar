﻿using System;
using System.Buffers;
using Hagar.Buffers;

namespace Hagar.ISerializable
{
    internal interface ISerializableSerializer
    {
        void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value) where TBufferWriter : IBufferWriter<byte>;
        object ReadValue(ref Reader reader, Type type, uint placeholderReferenceId);
    }
}