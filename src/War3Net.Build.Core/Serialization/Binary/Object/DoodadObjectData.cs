namespace War3Net.Build.Object
{
    public sealed partial class DoodadObjectData
    {
        internal DoodadObjectData(BinaryReader reader)
            : this(reader, true)
        {
        }

        // Inside an ObjectData file (.w3o), the next tables follow, so readTrailingData is false.
        internal DoodadObjectData(BinaryReader reader, bool readTrailingData)
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

            nint baseDoodadsCount = reader.ReadInt32();
            for (nint i = 0; i < baseDoodadsCount; i++)
            {
                BaseDoodads.Add(reader.ReadVariationObjectModification(FormatVersion));
            }

            nint newDoodadsCount = reader.ReadInt32();
            for (nint i = 0; i < newDoodadsCount; i++)
            {
                NewDoodads.Add(reader.ReadVariationObjectModification(FormatVersion));
            }
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteTo(writer, true);
        }

        internal void WriteTo(BinaryWriter writer, bool writeTrailingData)
        {
            writer.Write((int)FormatVersion);

            writer.Write(BaseDoodads.Count);
            foreach (var doodad in BaseDoodads)
            {
                writer.Write(doodad, FormatVersion);
            }

            writer.Write(NewDoodads.Count);
            foreach (var doodad in NewDoodads)
            {
                writer.Write(doodad, FormatVersion);
            }

            if (writeTrailingData && TrailingData is not null)
            {
                writer.Write(TrailingData);
            }
        }
    }
}