namespace War3Net.IO.Slk.Tests
{
    [TestClass]
    public class SylkParserTests
    {
        [TestMethod]
        [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
        public void TestParseSlkFile(string filePath)
        {
            using var stream = File.OpenRead(filePath);

            new SylkParser().Parse(stream);
        }

        [TestMethod]
        public void TestDefaultParserTypesCells()
        {
            var table = Parse(
                "ID;PWXL;N;E",
                "B;X7;Y1",
                "C;X1;Y1;K\"text\"",
                "C;X2;K12",
                "C;X3;K0.666666666666667",
                "C;X4;KTRUE",
                "C;X5;KFALSE",
                "C;X6;K#VALUE!",
                "C;X7;K#REF!",
                "E");

            Assert.AreEqual("text", table[0, 0]);
            Assert.AreEqual(12, table[1, 0]);
            Assert.AreEqual(0.6666667f, table[2, 0]);
            Assert.AreEqual(true, table[3, 0]);
            Assert.AreEqual(false, table[4, 0]);
            Assert.AreEqual(0, table[5, 0]);
            Assert.AreEqual(0, table[6, 0]);
        }

        [TestMethod]
        public void TestDefaultParserThrowsOnACellItDoesNotKnow()
        {
            Assert.ThrowsException<NotSupportedException>(() => Parse("ID;PWXL;N;E", "B;X1;Y1", "C;X1;Y1;K#N/A", "E"));
        }

        [TestMethod]
        public void TestDefaultParserThrowsOnACellOutsideTheTable()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Parse("ID;PWXL;N;E", "B;X1;Y1", "C;X2;Y1;K1", "E"));
        }

        [TestMethod]
        public void TestDefaultParserThrowsOnARecordItDoesNotKnow()
        {
            Assert.ThrowsException<NotSupportedException>(() => Parse("ID;PWXL;N;E", "B;X1;Y1", "F;P0;FG0G;X1", "E"));
        }

        [TestMethod]
        public void TestKeepTextKeepsEveryCellAsWritten()
        {
            var table = Parse(
                true,
                "ID;PWXL;N;E",
                "B;X8;Y1",
                "C;X1;Y1;K\"text\"",
                "C;X2;K12",
                "C;X3;K0.666666666666667",
                "C;X4;KTRUE",
                "C;X5;KFALSE",
                "C;X6;K#VALUE!",
                "C;X7;K#REF!",
                "C;X8;K#N/A",
                "E");

            CollectionAssert.AreEqual(
                new object[] { "text", "12", "0.666666666666667", "TRUE", "FALSE", "#VALUE!", "#REF!", "#N/A" },
                new[] { table[0, 0], table[1, 0], table[2, 0], table[3, 0], table[4, 0], table[5, 0], table[6, 0], table[7, 0] });
        }

        [TestMethod]
        public void TestKeepTextUnquotesAQuotedCell()
        {
            var table = Parse(
                true,
                "ID;PWXL;N;E",
                "B;X4;Y1",
                "C;X1;Y1;K\"say \"\"hi\"\"\"",
                "C;X2;K\"a;b\"",
                "C;X3;K\"\"",
                "C;X4;K\"12\"",
                "E");

            Assert.AreEqual("say \"hi\"", table[0, 0]);
            Assert.AreEqual("a;b", table[1, 0]);
            Assert.AreEqual(string.Empty, table[2, 0]);
            Assert.AreEqual("12", table[3, 0]);
        }

        [TestMethod]
        public void TestKeepTextTakesACellOutsideTheDeclaredSize()
        {
            var table = Parse(
                true,
                "ID;PWXL;N;E",
                "B;X2;Y2",
                "C;X1;Y1;K\"a\"",
                "C;X3;Y1;K\"c\"",
                "C;X1;Y3;K\"z\"",
                "E");

            Assert.AreEqual(3, table.Width);
            Assert.AreEqual(3, table.Height);
            Assert.AreEqual("a", table[0, 0]);
            Assert.AreEqual("c", table[2, 0]);
            Assert.AreEqual("z", table[0, 2]);
            Assert.AreEqual(2, table.Columns);
            Assert.AreEqual(2, table.Rows);
        }

        [TestMethod]
        public void TestKeepTextSkipsARecordItDoesNotKnow()
        {
            var table = Parse(
                true,
                "ID;PWXL;N;E",
                "P;PGeneral",
                "B;X2;Y1",
                "F;P0;FG0G;X1",
                string.Empty,
                "O;L;D;V0;K47;G100 0.001",
                "C;X1;Y1;K\"a\"",
                "C;X2;K\"b\"",
                "E");

            Assert.AreEqual("a", table[0, 0]);
            Assert.AreEqual("b", table[1, 0]);
        }

        [TestMethod]
        public void TestKeepTextTakesTheColumnAndRowOfThePreviousCell()
        {
            var table = Parse(
                true,
                "ID;PWXL;N;E",
                "B;X2;Y2",
                "C;X1;Y1;K\"a\"",
                "C;Y2;K\"b\"",
                "C;X2;K\"c\"",
                "E");

            Assert.AreEqual("a", table[0, 0]);
            Assert.AreEqual("b", table[0, 1]);
            Assert.AreEqual("c", table[1, 1]);
        }

        [TestMethod]
        public void TestKeepTextLeavesACellWithoutContentEmpty()
        {
            var table = Parse(true, "ID;PWXL;N;E", "B;X2;Y1", "C;X1;Y1", "C;X2;K\"b\"", "E");

            Assert.IsNull(table[0, 0]);
            Assert.AreEqual("b", table[1, 0]);
        }

        [TestMethod]
        public void TestKeepTextEndsAtTheEndOfTheFile()
        {
            var table = Parse(true, "ID;PWXL;N;E", "B;X1;Y1", "C;X1;Y1;K\"a\"");

            Assert.AreEqual("a", table[0, 0]);
        }

        [TestMethod]
        public void TestKeepTextStillWantsTheFirstRecordToBeID()
        {
            Assert.ThrowsException<InvalidDataException>(() => Parse(true, "B;X1;Y1", "C;X1;Y1;K\"a\"", "E"));
        }

        [TestMethod]
        public void TestKeepTextIsOffByDefault()
        {
            var table = new SylkParser().Parse(ToStream("ID;PWXL;N;E", "B;X1;Y1", "C;X1;Y1;K12", "E"));

            Assert.AreEqual(12, table[0, 0]);
        }

        private static SylkTable Parse(params string[] lines)
        {
            return new SylkParser().Parse(ToStream(lines));
        }

        private static SylkTable Parse(bool keepText, params string[] lines)
        {
            return new SylkParser(keepText).Parse(ToStream(lines));
        }

        private static MemoryStream ToStream(params string[] lines)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(string.Join("\r\n", lines) + "\r\n"));
        }

        private static IEnumerable<object[]> GetTestData()
        {
            return TestDataProvider.GetDynamicData("*.slk", SearchOption.AllDirectories, "Slk");
        }
    }
}