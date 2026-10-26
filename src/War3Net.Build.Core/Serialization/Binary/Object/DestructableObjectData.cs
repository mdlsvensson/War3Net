namespace War3Net.Build.Object
{
    public sealed partial class DestructableObjectData
    {
        internal DestructableObjectData(BinaryReader reader)
            : this(reader, true)
        {
        }

        // Inside an ObjectData file (.w3o), the next tables follow, so readTrailingData is false.
        internal DestructableObjectData(BinaryReader reader, bool readTrailingData)
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

            nint baseDestructablesCount = reader.ReadInt32();
            for (nint i = 0; i < baseDestructablesCount; i++)
            {
                BaseDestructables.Add(reader.ReadSimpleObjectModification(FormatVersion));
            }

            nint newDestructablesCount = reader.ReadInt32();
            for (nint i = 0; i < newDestructablesCount; i++)
            {
                NewDestructables.Add(reader.ReadSimpleObjectModification(FormatVersion));
            }
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteTo(writer, true);
        }

        internal void WriteTo(BinaryWriter writer, bool writeTrailingData)
        {
            writer.Write((int)FormatVersion);

            writer.Write(BaseDestructables.Count);
            foreach (var destructable in BaseDestructables)
            {
                writer.Write(destructable, FormatVersion);
            }

            writer.Write(NewDestructables.Count);
            foreach (var destructable in NewDestructables)
            {
                writer.Write(destructable, FormatVersion);
            }

            if (writeTrailingData && TrailingData is not null)
            {
                writer.Write(TrailingData);
            }
        }
    }
}