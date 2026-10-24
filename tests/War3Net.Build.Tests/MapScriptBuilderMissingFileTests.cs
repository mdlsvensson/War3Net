namespace War3Net.Build.Tests
{
    [TestClass]
    public class MapScriptBuilderMissingFileTests
    {
        private const string MapFolder = "MapFiles/TestGenerateUnitData";

        [TestMethod]
        public void TestBuildWritesMainForAMapWithInfoAndEnvironment()
        {
            var map = OpenMap();

            var script = new MapScriptBuilder().Build(map);

            StringAssert.Contains(script, "function main takes nothing returns nothing");
            StringAssert.Contains(script, "function config takes nothing returns nothing");
        }

        [TestMethod]
        public void TestBuildThrowsForAMapWithoutEnvironment()
        {
            var map = OpenMap();
            map.Environment = null;

            var exception = Assert.ThrowsException<InvalidOperationException>(() => new MapScriptBuilder().Build(map));

            StringAssert.Contains(exception.Message, "war3map.w3e");
        }

        [TestMethod]
        public void TestBuildThrowsForAMapWithoutInfo()
        {
            var map = OpenMap();
            map.Info = null;

            var exception = Assert.ThrowsException<InvalidOperationException>(() => new MapScriptBuilder().Build(map));

            StringAssert.Contains(exception.Message, "war3map.w3i");
        }

        [TestMethod]
        public void TestBuildThrowsForAMapWithNeitherFile()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new MapScriptBuilder().Build(new Map()));
        }

        [TestMethod]
        public void TestBuildThrowsBeforeItWritesAnything()
        {
            var map = OpenMap();
            map.Environment = null;
            using var stringWriter = new StringWriter();
            using var writer = new IndentedTextWriter(stringWriter);

            Assert.ThrowsException<InvalidOperationException>(() => new MapScriptBuilder().Build(map, writer));

            Assert.AreEqual(string.Empty, stringWriter.ToString());
        }

        private static Map OpenMap()
        {
            return Map.Open(TestDataProvider.GetPath(MapFolder), MapFiles.Info | MapFiles.Environment);
        }
    }
}
