namespace War3Net.Build.Object
{
    public sealed partial class BuffObjectData
    {
        internal BuffObjectData(BinaryReader reader)
            : this(reader, true)
        {
        }

        // Inside an ObjectData file (.w3o), the next tables follow, so readTrailingData is false.
        internal BuffObjectData(BinaryReader reader, bool readTrailingData)
        {
            ReadFrom(reader);

            if (readTrailingData)
            {
                var trailingData = reader.ReadRemainingBytes();
                TrailingData = trailingData.Length > 0 ? trailingData : null;
            }
        }

        // The bytes after the two tables, written back as they were read: some writers add a third, empty table.
        internal byte[]? TrailingData { get; set; }

        internal void ReadFrom(BinaryReader reader)
        {
            FormatVersion = reader.ReadInt32<ObjectDataFormatVersion>();

            nint baseBuffsCount = reader.ReadInt32();
            for (nint i = 0; i < baseBuffsCount; i++)
            {
                BaseBuffs.Add(reader.ReadSimpleObjectModification(FormatVersion));
            }

            nint newBuffsCount = reader.ReadInt32();
            for (nint i = 0; i < newBuffsCount; i++)
            {
                NewBuffs.Add(reader.ReadSimpleObjectModification(FormatVersion));
            }
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteTo(writer, true);
        }

        internal void WriteTo(BinaryWriter writer, bool writeTrailingData)
        {
            writer.Write((int)FormatVersion);

            writer.Write(BaseBuffs.Count);
            foreach (var buff in BaseBuffs)
            {
                writer.Write(buff, FormatVersion);
            }

            writer.Write(NewBuffs.Count);
            foreach (var buff in NewBuffs)
            {
                writer.Write(buff, FormatVersion);
            }

            if (writeTrailingData && TrailingData is not null)
            {
                writer.Write(TrailingData);
            }
        }
    }
}