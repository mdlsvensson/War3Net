namespace War3Net.IO.Slk
{
    public sealed class SylkParser
    {
        private readonly bool _keepText;

        private SylkTable _table;
        private int? _lastX;
        private int? _lastY;

        public SylkParser()
        {
            _lastX = null;
            _lastY = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SylkParser"/> class.
        /// </summary>
        /// <param name="keepText">
        /// If <see langword="true"/>, every cell is kept as the text the file spells (quoted text without its quotes, and a doubled quote as one),
        /// instead of being read as a <see cref="string"/>, <see cref="int"/>, <see cref="float"/> or <see cref="bool"/> or rejected.
        /// A cell outside the size the <c>B</c> record gives makes the table larger, a record other than <c>ID</c>, <c>B</c>, <c>C</c> and <c>E</c> is
        /// skipped, and the file may end without an <c>E</c> record.
        /// </param>
        public SylkParser(bool keepText)
            : this()
        {
            _keepText = keepText;
        }

        public SylkTable Parse(Stream input, bool leaveOpen = false)
        {
            using var reader = new StreamReader(input, Encoding.UTF8, true, 1024, leaveOpen);

            if (_keepText)
            {
                return ParseText(reader);
            }

            var isOnFirstLine = true;
            while (true)
            {
                var line = reader.ReadLine();
                var fields = line.Split(';');
                var recordType = fields[0];

                string GetField(string fieldName)
                {
                    foreach (var field in fields)
                    {
                        if (field.StartsWith(fieldName, StringComparison.Ordinal))
                        {
                            return field.Substring(fieldName.Length);
                        }
                    }

                    throw new InvalidDataException($"Record does not contain mandatory field of type '{fieldName}'.");
                }

                string? GetOptionalField(string fieldName)
                {
                    foreach (var field in fields)
                    {
                        if (field.StartsWith(fieldName, StringComparison.Ordinal))
                        {
                            return field.Substring(fieldName.Length);
                        }
                    }

                    return null;
                }

                if (isOnFirstLine)
                {
                    isOnFirstLine = false;
                    if (recordType != "ID")
                    {
                        throw new InvalidDataException("SYLK file must start with 'ID'.");
                    }

                    GetField("P");
                }
                else
                {
                    switch (recordType)
                    {
                        case "ID":
                            throw new InvalidDataException("Record type 'ID' can only occur on the first line.");

                        case "B":
                            if (_table != null)
                            {
                                throw new InvalidDataException("Only one record of type 'B' may be present.");
                            }

                            _table = new SylkTable(
                                int.Parse(GetField("X"), NumberStyles.Integer, CultureInfo.InvariantCulture),
                                int.Parse(GetField("Y"), NumberStyles.Integer, CultureInfo.InvariantCulture));

                            break;

                        case "C":
                            if (_table == null)
                            {
                                throw new InvalidDataException("Unable to parse record of type 'C' before encountering a record of type 'B'.");
                            }

                            SetCellContent(
                                GetOptionalField("X"),
                                GetOptionalField("Y"),
                                GetOptionalField("K") ?? string.Empty);

                            break;

                        case "E":
                            return _table;

                        default:
                            throw new NotSupportedException($"Support for record type '{recordType}' is not implemented. Only records of type 'ID', 'B', 'C', and 'E' are supported.");
                    }
                }
            }
        }

        /// <param name="x">The cell's 1-indexed X position.</param>
        /// <param name="y">The cell's 1-indexed Y position.</param>
        private void SetCellContent(string? x, string? y, string value)
        {
            var xi = x is not null
                ? (int.Parse(x, NumberStyles.Integer, CultureInfo.InvariantCulture) - 1)
                : _lastX ?? throw new InvalidDataException("Column for cell is not defined.");

            var yi = y is not null
                ? (int.Parse(y, NumberStyles.Integer, CultureInfo.InvariantCulture) - 1)
                : _lastY ?? throw new InvalidDataException("Row for cell is not defined.");

            if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
            {
                _table[xi, yi] = value[1..^1];
            }
            else if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var @int))
            {
                _table[xi, yi] = @int;
            }
            else if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var @float))
            {
                _table[xi, yi] = @float;
            }
            else if (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase))
            {
                _table[xi, yi] = true;
            }
            else if (string.Equals(value, bool.FalseString, StringComparison.OrdinalIgnoreCase))
            {
                _table[xi, yi] = false;
            }
            else if (string.Equals(value, "#VALUE!", StringComparison.Ordinal) || string.Equals(value, "#REF!", StringComparison.Ordinal))
            {
                _table[xi, yi] = 0;
            }
            else
            {
                throw new NotSupportedException($"Unable to parse value '{value}'. Can only parse strings, integers, floats, and booleans.");
            }

            _lastX = xi;
            _lastY = yi;
        }

        private static SylkTable ParseText(StreamReader reader)
        {
            var cells = new List<(int X, int Y, string Text)>();
            var width = 0;
            var height = 0;
            var lastX = 0;
            var lastY = 0;

            var isOnFirstLine = true;
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                var fields = SplitRecord(line);
                var recordType = fields[0];

                if (isOnFirstLine)
                {
                    isOnFirstLine = false;
                    if (recordType != "ID")
                    {
                        throw new InvalidDataException("SYLK file must start with 'ID'.");
                    }

                    continue;
                }

                if (recordType == "E")
                {
                    break;
                }

                if (recordType == "B")
                {
                    foreach (var field in fields.Skip(1))
                    {
                        if (field.Length > 1 && field[0] == 'X')
                        {
                            width = Math.Max(width, int.Parse(field.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture));
                        }
                        else if (field.Length > 1 && field[0] == 'Y')
                        {
                            height = Math.Max(height, int.Parse(field.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture));
                        }
                    }
                }
                else if (recordType == "C")
                {
                    string? text = null;
                    foreach (var field in fields.Skip(1))
                    {
                        if (field.Length == 0)
                        {
                            continue;
                        }

                        switch (field[0])
                        {
                            case 'X':
                                lastX = int.Parse(field.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture);
                                break;

                            case 'Y':
                                lastY = int.Parse(field.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture);
                                break;

                            case 'K':
                                text = field.Length >= 3 && field[1] == '"' && field[^1] == '"'
                                    ? field.Substring(2, field.Length - 3).Replace("\"\"", "\"")
                                    : field.Substring(1);
                                break;
                        }
                    }

                    if (text is not null)
                    {
                        if (lastX < 1)
                        {
                            throw new InvalidDataException("Column for cell is not defined.");
                        }

                        if (lastY < 1)
                        {
                            throw new InvalidDataException("Row for cell is not defined.");
                        }

                        cells.Add((lastX, lastY, text));
                        width = Math.Max(width, lastX);
                        height = Math.Max(height, lastY);
                    }
                }
            }

            var table = new SylkTable(width, height);
            foreach (var (x, y, text) in cells)
            {
                table[x - 1, y - 1] = text;
            }

            return table;
        }

        /// <summary>
        /// Splits a record on the semicolons outside quotes.
        /// </summary>
        private static List<string> SplitRecord(string record)
        {
            var fields = new List<string>();
            var start = 0;
            var quoted = false;
            for (var i = 0; i < record.Length; i++)
            {
                if (record[i] == '"')
                {
                    quoted = !quoted;
                }
                else if (record[i] == ';' && !quoted)
                {
                    fields.Add(record.Substring(start, i - start));
                    start = i + 1;
                }
            }

            fields.Add(record.Substring(start));
            return fields;
        }
    }
}