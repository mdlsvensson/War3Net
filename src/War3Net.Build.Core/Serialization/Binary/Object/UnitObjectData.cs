namespace War3Net.Build.Object
{
    public sealed partial class UnitObjectData
    {
        internal UnitObjectData(BinaryReader reader)
            : this(reader, true)
        {
        }

        // Inside an ObjectData file (.w3o), the next tables follow, so readTrailingData is false.
        internal UnitObjectData(BinaryReader reader, bool readTrailingData)
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

            nint baseUnitsCount = reader.ReadInt32();
            for (nint i = 0; i < baseUnitsCount; i++)
            {
                BaseUnits.Add(reader.ReadSimpleObjectModification(FormatVersion));
            }

            nint newUnitsCount = reader.ReadInt32();
            for (nint i = 0; i < newUnitsCount; i++)
            {
                NewUnits.Add(reader.ReadSimpleObjectModification(FormatVersion));
            }
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteTo(writer, true);
        }

        internal void WriteTo(BinaryWriter writer, bool writeTrailingData)
        {
            writer.Write((int)FormatVersion);

            writer.Write(BaseUnits.Count);
            foreach (var unit in BaseUnits)
            {
                writer.Write(unit, FormatVersion);
            }

            writer.Write(NewUnits.Count);
            foreach (var unit in NewUnits)
            {
                writer.Write(unit, FormatVersion);
            }

            if (writeTrailingData && TrailingData is not null)
            {
                writer.Write(TrailingData);
            }
        }
    }
}