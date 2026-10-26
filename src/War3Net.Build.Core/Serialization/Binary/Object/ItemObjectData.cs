namespace War3Net.Build.Object
{
    public sealed partial class ItemObjectData
    {
        internal ItemObjectData(BinaryReader reader)
            : this(reader, true)
        {
        }

        // Inside an ObjectData file (.w3o), the next tables follow, so readTrailingData is false.
        internal ItemObjectData(BinaryReader reader, bool readTrailingData)
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

            nint baseItemsCount = reader.ReadInt32();
            for (nint i = 0; i < baseItemsCount; i++)
            {
                BaseItems.Add(reader.ReadSimpleObjectModification(FormatVersion));
            }

            nint newItemsCount = reader.ReadInt32();
            for (nint i = 0; i < newItemsCount; i++)
            {
                NewItems.Add(reader.ReadSimpleObjectModification(FormatVersion));
            }
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteTo(writer, true);
        }

        internal void WriteTo(BinaryWriter writer, bool writeTrailingData)
        {
            writer.Write((int)FormatVersion);

            writer.Write(BaseItems.Count);
            foreach (var item in BaseItems)
            {
                writer.Write(item, FormatVersion);
            }

            writer.Write(NewItems.Count);
            foreach (var item in NewItems)
            {
                writer.Write(item, FormatVersion);
            }

            if (writeTrailingData && TrailingData is not null)
            {
                writer.Write(TrailingData);
            }
        }
    }
}