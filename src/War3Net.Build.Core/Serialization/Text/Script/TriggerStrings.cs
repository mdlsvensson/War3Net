namespace War3Net.Build.Script
{
    public sealed partial class TriggerStrings
    {
        private const char ByteOrderMark = '﻿';

        internal TriggerStrings(StreamReader reader)
        {
            ReadFrom(reader);
        }

        // How the file that was read is written, so that it is written back as it was. A new file is written as the writer
        // writes text: with its line break and its encoding's byte order mark, if any.
        internal bool HasByteOrderMark { get; set; }

        internal string? NewLine { get; set; }

        internal bool EndsWithNewLine { get; set; } = true;

        internal void ReadFrom(StreamReader reader)
        {
            // A StreamReader removes the byte order mark, unless it is told not to look for one.
            HasByteOrderMark = StartsWithByteOrderMark(reader.BaseStream);

            var text = reader.ReadToEnd();
            if (text.Length > 0 && text[0] == ByteOrderMark)
            {
                HasByteOrderMark = true;
                text = text[1..];
            }

            NewLine = GetFirstNewLine(text);
            EndsWithNewLine = text.Length == 0 || text[^1] == '\n' || text[^1] == '\r';

            using var textReader = new StringReader(text);
            while (textReader.Peek() != -1)
            {
                Strings.Add(new TriggerString(textReader));
            }
        }

        internal void WriteTo(StreamWriter writer)
        {
            // A writer whose encoding has a byte order mark writes it itself.
            if (HasByteOrderMark && writer.Encoding.GetPreamble().Length == 0)
            {
                writer.Write(ByteOrderMark);
            }

            using var textWriter = new StringWriter(CultureInfo.InvariantCulture)
            {
                NewLine = NewLine ?? writer.NewLine,
            };

            foreach (var triggerString in Strings)
            {
                triggerString.WriteTo(textWriter);
            }

            var text = textWriter.ToString();
            if (!EndsWithNewLine && text.EndsWith(textWriter.NewLine, StringComparison.Ordinal))
            {
                text = text[..^textWriter.NewLine.Length];
            }

            writer.Write(text);
        }

        private static bool StartsWithByteOrderMark(Stream stream)
        {
            // Nothing has been read from the stream yet if it is still at its start.
            if (!stream.CanSeek || stream.Position != 0)
            {
                return false;
            }

            var bytes = new byte[3];
            var count = 0;
            int read;
            while (count < bytes.Length && (read = stream.Read(bytes, count, bytes.Length - count)) > 0)
            {
                count += read;
            }

            stream.Position = 0;
            return count == bytes.Length && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        }

        private static string? GetFirstNewLine(string text)
        {
            var index = text.IndexOfAny(new[] { '\r', '\n' });
            if (index == -1)
            {
                return null;
            }

            if (text[index] == '\n')
            {
                return "\n";
            }

            return index + 1 < text.Length && text[index + 1] == '\n' ? "\r\n" : "\r";
        }
    }
}