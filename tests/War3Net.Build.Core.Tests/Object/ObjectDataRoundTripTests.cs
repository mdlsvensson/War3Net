namespace War3Net.Build.Core.Tests.Object
{
    /// <summary>
    /// An object data file that is read and written back unchanged comes back byte for byte.
    /// The files in RoundTrip/WorldEditorObjects were saved by World Editor 3.00 (Warcraft III 3.0.0.24268): one new object on each
    /// Object Editor tab, with only its name changed.
    /// </summary>
    [TestClass]
    public class ObjectDataRoundTripTests
    {
        private const string FixtureFolder = "RoundTrip/WorldEditorObjects";

        [DataTestMethod]
        [DataRow("war3map.w3a")]
        [DataRow("war3map.w3b")]
        [DataRow("war3map.w3d")]
        [DataRow("war3map.w3h")]
        [DataRow("war3map.w3q")]
        [DataRow("war3map.w3t")]
        [DataRow("war3map.w3u")]
        [DataRow("war3mapSkin.w3a")]
        [DataRow("war3mapSkin.w3b")]
        [DataRow("war3mapSkin.w3d")]
        [DataRow("war3mapSkin.w3h")]
        [DataRow("war3mapSkin.w3q")]
        [DataRow("war3mapSkin.w3t")]
        [DataRow("war3mapSkin.w3u")]
        public void TestWorldEditorFileIsWrittenBackAsItWasRead(string fileName)
        {
            var input = ReadFixture(fileName);

            CollectionAssert.AreEqual(input, RoundTrip(fileName, input));
        }

        [DataTestMethod]
        [DataRow("war3map.w3a")]
        [DataRow("war3map.w3b")]
        [DataRow("war3map.w3d")]
        [DataRow("war3map.w3h")]
        [DataRow("war3map.w3q")]
        [DataRow("war3map.w3t")]
        [DataRow("war3map.w3u")]
        [DataRow("war3mapSkin.w3a")]
        [DataRow("war3mapSkin.w3b")]
        [DataRow("war3mapSkin.w3d")]
        [DataRow("war3mapSkin.w3h")]
        [DataRow("war3mapSkin.w3q")]
        [DataRow("war3mapSkin.w3t")]
        [DataRow("war3mapSkin.w3u")]
        public void TestAThirdEmptyTableIsWrittenBack(string fileName)
        {
            var input = ReadFixture(fileName).Concat(new byte[] { 0, 0, 0, 0 }).ToArray();

            CollectionAssert.AreEqual(input, RoundTrip(fileName, input));
        }

        [TestMethod]
        public void TestAnyBytesAfterTheTablesAreWrittenBack()
        {
            var input = ReadFixture("war3mapSkin.w3u").Concat(new byte[] { 1, 2, 3 }).ToArray();

            CollectionAssert.AreEqual(input, RoundTrip("war3mapSkin.w3u", input));
        }

        [DataTestMethod]
        [DataRow("war3mapSkin.w3a")]
        [DataRow("war3mapSkin.w3b")]
        [DataRow("war3map.w3d")]
        [DataRow("war3mapSkin.w3h")]
        [DataRow("war3mapSkin.w3q")]
        [DataRow("war3mapSkin.w3t")]
        [DataRow("war3mapSkin.w3u")]
        public void TestAStringThatIsNotUtf8IsWrittenBackAsItWasRead(string fileName)
        {
            var input = WithLatin1Name(ReadFixture(fileName));

            CollectionAssert.AreEqual(input, RoundTrip(fileName, input));
        }

        [TestMethod]
        public void TestAStringThatIsNotUtf8IsReadWithReplacementCharacters()
        {
            var unitData = ReadUnitData(WithLatin1Name(ReadFixture("war3mapSkin.w3u")));

            Assert.AreEqual("B�gskytt", unitData.NewUnits[0].Modifications[0].ValueAsString);
        }

        [TestMethod]
        public void TestAStringThatIsNotUtf8IsWrittenAsUtf8OnceChanged()
        {
            var unitData = ReadUnitData(WithLatin1Name(ReadFixture("war3mapSkin.w3u")));
            unitData.NewUnits[0].Modifications[0].Value = "Bågskytt";

            using var output = new MemoryStream();
            using var writer = new BinaryWriter(output, UTF8EncodingProvider.StrictUTF8);
            writer.Write(unitData);
            writer.Flush();

            var expected = ReplaceName(ReadFixture("war3mapSkin.w3u"), Encoding.UTF8.GetBytes("Bågskytt"));
            CollectionAssert.AreEqual(expected, output.ToArray());
        }

        private static byte[] ReadFixture(string fileName)
        {
            return File.ReadAllBytes(TestDataProvider.GetPath($"{FixtureFolder}/{fileName}"));
        }

        /// <summary>
        /// The fixture with its object's name, a TRIGSTR_ reference, replaced by "Bågskytt" in Latin-1: the 0xE5 is not valid UTF-8.
        /// </summary>
        private static byte[] WithLatin1Name(byte[] file)
        {
            return ReplaceName(file, new byte[] { 0x42, 0xE5, 0x67, 0x73, 0x6B, 0x79, 0x74, 0x74 });
        }

        private static byte[] ReplaceName(byte[] file, byte[] name)
        {
            var reference = Encoding.ASCII.GetBytes("TRIGSTR_");
            var start = Enumerable.Range(0, file.Length - reference.Length).First(i => file.AsSpan(i, reference.Length).SequenceEqual(reference));
            var end = Array.IndexOf(file, (byte)0, start);

            return file[..start].Concat(name).Concat(file[end..]).ToArray();
        }

        private static UnitObjectData ReadUnitData(byte[] file)
        {
            using var reader = new BinaryReader(new MemoryStream(file), UTF8EncodingProvider.StrictUTF8);
            return reader.ReadUnitObjectData();
        }

        /// <summary>
        /// Reads and writes the file as Map does, with a strict UTF-8 encoding.
        /// </summary>
        private static byte[] RoundTrip(string fileName, byte[] input)
        {
            using var reader = new BinaryReader(new MemoryStream(input), UTF8EncodingProvider.StrictUTF8);
            using var output = new MemoryStream();
            using var writer = new BinaryWriter(output, UTF8EncodingProvider.StrictUTF8);

            switch (Path.GetExtension(fileName))
            {
                case AbilityObjectData.FileExtension: writer.Write(reader.ReadAbilityObjectData()); break;
                case BuffObjectData.FileExtension: writer.Write(reader.ReadBuffObjectData()); break;
                case DestructableObjectData.FileExtension: writer.Write(reader.ReadDestructableObjectData()); break;
                case DoodadObjectData.FileExtension: writer.Write(reader.ReadDoodadObjectData()); break;
                case ItemObjectData.FileExtension: writer.Write(reader.ReadItemObjectData()); break;
                case UnitObjectData.FileExtension: writer.Write(reader.ReadUnitObjectData()); break;
                case UpgradeObjectData.FileExtension: writer.Write(reader.ReadUpgradeObjectData()); break;
                default: throw new ArgumentException($"Not an object data file: {fileName}", nameof(fileName));
            }

            writer.Flush();
            return output.ToArray();
        }
    }
}