namespace War3Net.Build.Object
{
    public abstract partial class ObjectDataModification
    {
        // A string whose bytes are not valid UTF-8 is read with replacement characters. Its bytes are kept, and written back as
        // long as Value is still the text they were read as.
        private byte[]? _invalidUtf8Value;
        private string? _invalidUtf8ValueText;

        protected object ReadValue(BinaryReader reader, ObjectDataFormatVersion formatVersion)
        {
            return Type switch
            {
                ObjectDataType.Int => reader.ReadInt32(),
                ObjectDataType.Real => reader.ReadSingle(),
                ObjectDataType.Unreal => reader.ReadSingle(),
                ObjectDataType.String => ReadStringValue(reader),
                ObjectDataType.Bool => reader.ReadBoolean(),
                ObjectDataType.Char => reader.ReadChar(),

                _ => throw new InvalidEnumArgumentException(nameof(Type), (int)Type, typeof(ObjectDataType)),
            };
        }

        protected void WriteValue(BinaryWriter writer, ObjectDataFormatVersion formatVersion)
        {
            switch (Type)
            {
                case ObjectDataType.Int:
                    writer.Write((int)Value);
                    break;

                case ObjectDataType.Real:
                case ObjectDataType.Unreal:
                    writer.Write((float)Value);
                    break;

                case ObjectDataType.String:
                    if (_invalidUtf8Value is not null && string.Equals((string)Value, _invalidUtf8ValueText, StringComparison.Ordinal))
                    {
                        writer.Write(_invalidUtf8Value);
                        writer.Write(byte.MinValue);
                    }
                    else
                    {
                        writer.WriteString((string)Value);
                    }

                    break;

                case ObjectDataType.Bool:
                    writer.Write((bool)Value);
                    break;

                case ObjectDataType.Char:
                    writer.Write((char)Value);
                    break;

                default: throw new InvalidEnumArgumentException(nameof(Type), (int)Type, typeof(ObjectDataType));
            }
        }

        private string ReadStringValue(BinaryReader reader)
        {
            using var bytes = new MemoryStream();
            while (true)
            {
                var value = reader.BaseStream.ReadByte();
                if (value == -1)
                {
                    throw new InvalidDataException("Reached end of the stream without encountering a \0 character to mark the end of the string.");
                }

                if (value == byte.MinValue)
                {
                    break;
                }

                bytes.WriteByte((byte)value);
            }

            var raw = bytes.ToArray();
            var text = Encoding.UTF8.GetString(raw);
            var isValidUtf8 = Encoding.UTF8.GetBytes(text).AsSpan().SequenceEqual(raw);
            _invalidUtf8Value = isValidUtf8 ? null : raw;
            _invalidUtf8ValueText = isValidUtf8 ? null : text;

            return text;
        }
    }
}