namespace War3Net.Build.Object
{
    public sealed partial class UpgradeObjectData
    {
        internal UpgradeObjectData(BinaryReader reader)
            : this(reader, true)
        {
        }

        // Inside an ObjectData file (.w3o), the next tables follow, so readTrailingData is false.
        internal UpgradeObjectData(BinaryReader reader, bool readTrailingData)
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

            nint baseUpgradesCount = reader.ReadInt32();
            for (nint i = 0; i < baseUpgradesCount; i++)
            {
                BaseUpgrades.Add(reader.ReadLevelObjectModification(FormatVersion));
            }

            nint newUpgradesCount = reader.ReadInt32();
            for (nint i = 0; i < newUpgradesCount; i++)
            {
                NewUpgrades.Add(reader.ReadLevelObjectModification(FormatVersion));
            }
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteTo(writer, true);
        }

        internal void WriteTo(BinaryWriter writer, bool writeTrailingData)
        {
            writer.Write((int)FormatVersion);

            writer.Write(BaseUpgrades.Count);
            foreach (var upgrade in BaseUpgrades)
            {
                writer.Write(upgrade, FormatVersion);
            }

            writer.Write(NewUpgrades.Count);
            foreach (var upgrade in NewUpgrades)
            {
                writer.Write(upgrade, FormatVersion);
            }

            if (writeTrailingData && TrailingData is not null)
            {
                writer.Write(TrailingData);
            }
        }
    }
}