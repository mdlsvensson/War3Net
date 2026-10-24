namespace War3Net.Build.Core.Tests.Info
{
    [TestClass]
    public class GameDataVersionTests
    {
        [TestMethod]
        public void TestForsakenKingdomIsTheValueUnsetHas()
        {
            Assert.AreEqual(GameDataVersion.Unset, GameDataVersion.ForsakenKingdom);
            Assert.AreEqual(2, (int)GameDataVersion.ForsakenKingdom);
        }

        [TestMethod]
        public void TestTheValueIsStillNamedUnsetByDefault()
        {
            Assert.AreEqual("Unset", GameDataVersion.ForsakenKingdom.ToString());
            Assert.AreEqual("Unset", Enum.GetName(typeof(GameDataVersion), 2));
        }

        [TestMethod]
        public void TestJsonStringEnumsWriteUnsetAndReadBothNames()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());

            Assert.AreEqual("\"Unset\"", JsonSerializer.Serialize(GameDataVersion.ForsakenKingdom, options));
            Assert.AreEqual(GameDataVersion.ForsakenKingdom, JsonSerializer.Deserialize<GameDataVersion>("\"Unset\"", options));
            Assert.AreEqual(GameDataVersion.ForsakenKingdom, JsonSerializer.Deserialize<GameDataVersion>("\"ForsakenKingdom\"", options));
            Assert.AreEqual(GameDataVersion.ForsakenKingdom, JsonSerializer.Deserialize<GameDataVersion>("2", options));
        }

        [TestMethod]
        public void TestAMapInfoWithTheValueIsWrittenWithTheNumber()
        {
            using var input = File.OpenRead(TestDataProvider.GetPath("Info/Reforged/CustSkinFalse-AccProbFalse-HD-FrozenThrone.w3i"));
            using var reader = new BinaryReader(input);
            var info = reader.ReadMapInfo();
            info.GameDataVersion = GameDataVersion.ForsakenKingdom;

            using var output = new MemoryStream();
            using var writer = new BinaryWriter(output);
            writer.Write(info);
            writer.Flush();
            output.Position = 0;
            var read = new BinaryReader(output).ReadMapInfo();

            Assert.AreEqual(2, (int)read.GameDataVersion);
            Assert.AreEqual(GameDataVersion.Unset, read.GameDataVersion);
        }
    }
}
