namespace War3Net.Build.Object
{
    public sealed partial class AbilityObjectData
    {
        internal AbilityObjectData(BinaryReader reader)
            : this(reader, true)
        {
        }

        // Inside an ObjectData file (.w3o), the next tables follow, so readTrailingData is false.
        internal AbilityObjectData(BinaryReader reader, bool readTrailingData)
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

            nint baseAbilitiesCount = reader.ReadInt32();
            for (nint i = 0; i < baseAbilitiesCount; i++)
            {
                BaseAbilities.Add(reader.ReadLevelObjectModification(FormatVersion));
            }

            nint newAbilitiesCount = reader.ReadInt32();
            for (nint i = 0; i < newAbilitiesCount; i++)
            {
                NewAbilities.Add(reader.ReadLevelObjectModification(FormatVersion));
            }
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteTo(writer, true);
        }

        internal void WriteTo(BinaryWriter writer, bool writeTrailingData)
        {
            writer.Write((int)FormatVersion);

            writer.Write(BaseAbilities.Count);
            foreach (var ability in BaseAbilities)
            {
                writer.Write(ability, FormatVersion);
            }

            writer.Write(NewAbilities.Count);
            foreach (var ability in NewAbilities)
            {
                writer.Write(ability, FormatVersion);
            }

            if (writeTrailingData && TrailingData is not null)
            {
                writer.Write(TrailingData);
            }
        }
    }
}