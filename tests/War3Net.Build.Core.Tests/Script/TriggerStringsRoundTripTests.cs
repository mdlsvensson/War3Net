namespace War3Net.Build.Core.Tests.Script
{
    /// <summary>
    /// A trigger strings file that is read and written back unchanged comes back byte for byte: its byte order mark, its line
    /// endings and whether its last line ends with one are kept.
    /// </summary>
    [TestClass]
    public class TriggerStringsRoundTripTests
    {
        [TestMethod]
        public void TestWorldEditorFileIsWrittenBackAsItWasRead()
        {
            // Saved by World Editor 3.00: a UTF-8 byte order mark and CRLF line endings.
            var input = File.ReadAllBytes(TestDataProvider.GetPath("RoundTrip/WorldEditorObjects/war3map.wts"));

            CollectionAssert.AreEqual(input, RoundTrip(input));
        }

        [DataTestMethod]
        [DataRow("STRING 1\r\n{\r\nA\r\n}\r\n\r\nSTRING 2\r\n{\r\nB\r\nC\r\n}\r\n", DisplayName = "CRLF")]
        [DataRow("STRING 1\n{\nA\n}\n\nSTRING 2\n{\nB\nC\n}\n", DisplayName = "LF")]
        [DataRow("STRING 1\r\n// A comment\r\n{\r\nA\r\n}\r\n", DisplayName = "CRLF with a comment")]
        [DataRow("STRING 1\n// A comment\n// on two lines\n{\nA\n}\n", DisplayName = "LF with a comment on two lines")]
        [DataRow("STRING 1\r\n{\r\nA\r\n}", DisplayName = "CRLF, no line break at the end")]
        [DataRow("STRING 1\n{\nA\n}", DisplayName = "LF, no line break at the end")]
        [DataRow("STRING 1\n{\nA\n}\n\n\n", DisplayName = "LF, empty lines at the end")]
        public void TestFileIsWrittenBackAsItWasRead(string text)
        {
            var withoutByteOrderMark = Encoding.UTF8.GetBytes(text);
            var withByteOrderMark = Encoding.UTF8.GetPreamble().Concat(withoutByteOrderMark).ToArray();

            CollectionAssert.AreEqual(withoutByteOrderMark, RoundTrip(withoutByteOrderMark));
            CollectionAssert.AreEqual(withByteOrderMark, RoundTrip(withByteOrderMark));
        }

        [TestMethod]
        public void TestAReaderWhoseEncodingHasAByteOrderMarkDoesNotAddOne()
        {
            var input = Encoding.UTF8.GetBytes("STRING 1\r\n{\r\nA\r\n}\r\n");

            using var reader = new StreamReader(new MemoryStream(input), Encoding.UTF8);
            var triggerStrings = reader.ReadTriggerStrings();

            CollectionAssert.AreEqual(input, Write(triggerStrings));
        }

        [TestMethod]
        public void TestAValueOnSeveralLinesIsStillJoinedWithCrlf()
        {
            using var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes("STRING 1\n{\nB\nC\n}\n")), UTF8EncodingProvider.StrictUTF8);

            Assert.AreEqual("B\r\nC", reader.ReadTriggerStrings().Strings.Single().Value);
        }

        [TestMethod]
        public void TestAChangedValueIsWrittenWithTheFileLineEndings()
        {
            var input = Encoding.UTF8.GetBytes("STRING 1\n{\nA\n}\n");
            using var reader = new StreamReader(new MemoryStream(input), UTF8EncodingProvider.StrictUTF8);
            var triggerStrings = reader.ReadTriggerStrings();
            triggerStrings.Strings.Single().Value = "B\r\nC";

            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("STRING 1\n{\nB\nC\n}\n"), Write(triggerStrings));
        }

        [TestMethod]
        public void TestANewFileIsWrittenAsBefore()
        {
            var triggerStrings = new TriggerStrings();
            triggerStrings.Strings.Add(new TriggerString { Key = 1, KeyPrecision = 1, Value = "A" });

            var newLine = System.Environment.NewLine;
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes($"STRING 1{newLine}{{{newLine}A{newLine}}}{newLine}"), Write(triggerStrings));
        }

        /// <summary>
        /// Reads and writes the file as Map does, with a strict UTF-8 encoding that has no byte order mark.
        /// </summary>
        private static byte[] RoundTrip(byte[] input)
        {
            using var reader = new StreamReader(new MemoryStream(input), UTF8EncodingProvider.StrictUTF8);
            return Write(reader.ReadTriggerStrings());
        }

        private static byte[] Write(TriggerStrings triggerStrings)
        {
            using var output = new MemoryStream();
            using var writer = new StreamWriter(output, UTF8EncodingProvider.StrictUTF8);
            writer.WriteTriggerStrings(triggerStrings);
            writer.Flush();
            return output.ToArray();
        }
    }
}