namespace War3Net.Build.Object
{
    public sealed partial class ObjectData
    {
        internal ObjectData(BinaryReader reader)
        {
            ReadFrom(reader);
        }

        internal void ReadFrom(BinaryReader reader)
        {
            FormatVersion = reader.ReadInt32<ObjectDataFormatVersion>();

            UnitData = reader.ReadBool() ? new UnitObjectData(reader, false) : null;
            ItemData = reader.ReadBool() ? new ItemObjectData(reader, false) : null;
            DestructableData = reader.ReadBool() ? new DestructableObjectData(reader, false) : null;
            DoodadData = reader.ReadBool() ? new DoodadObjectData(reader, false) : null;
            AbilityData = reader.ReadBool() ? new AbilityObjectData(reader, false) : null;
            BuffData = reader.ReadBool() ? new BuffObjectData(reader, false) : null;
            UpgradeData = reader.ReadBool() ? new UpgradeObjectData(reader, false) : null;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write((int)FormatVersion);

            writer.WriteBool(UnitData is not null);
            UnitData?.WriteTo(writer, false);

            writer.WriteBool(ItemData is not null);
            ItemData?.WriteTo(writer, false);

            writer.WriteBool(DestructableData is not null);
            DestructableData?.WriteTo(writer, false);

            writer.WriteBool(DoodadData is not null);
            DoodadData?.WriteTo(writer, false);

            writer.WriteBool(AbilityData is not null);
            AbilityData?.WriteTo(writer, false);

            writer.WriteBool(BuffData is not null);
            BuffData?.WriteTo(writer, false);

            writer.WriteBool(UpgradeData is not null);
            UpgradeData?.WriteTo(writer, false);
        }
    }
}