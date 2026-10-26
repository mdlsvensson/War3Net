namespace War3Net.Build.Script
{
    public sealed partial class TriggerString
    {
        internal TriggerString(TextReader reader)
        {
            ReadFrom(reader);
        }

        internal void ReadFrom(TextReader reader)
        {
            while (true)
            {
                var line = reader.ReadLine();
                if (line is null)
                {
                    break;
                }

                if (line.StartsWith("STRING ", StringComparison.Ordinal))
                {
                    // Read key
                    var keyString = line[7..].Trim();
                    Key = uint.TryParse(keyString, out var result) ? result : 0;
                    KeyPrecision = (uint)keyString.Length;

                    // Read comment
                    var isFirstLine = true;
                    while (true)
                    {
                        line = reader.ReadLine();
                        if (line is null)
                        {
                            throw new InvalidDataException("Expected opening brace or comment.");
                        }
                        else if (string.Equals(line, "{", StringComparison.Ordinal))
                        {
                            break;
                        }
                        else if (line.StartsWith("//", StringComparison.Ordinal) || Comment is not null)
                        {
                            if (isFirstLine)
                            {
                                Comment = line[2..];
                                isFirstLine = false;
                            }
                            else
                            {
                                Comment += $"\r\n{line}";
                            }
                        }
                        else
                        {
                            throw new InvalidDataException("Expected opening brace or comment.");
                        }
                    }

                    // Read value
                    isFirstLine = true;
                    while (true)
                    {
                        line = reader.ReadLine();
                        if (string.Equals(line, "}", StringComparison.Ordinal))
                        {
                            break;
                        }

                        if (isFirstLine)
                        {
                            Value = line;
                            isFirstLine = false;
                        }
                        else
                        {
                            Value += $"\r\n{line}";
                        }
                    }

                    break;
                }
                else
                {
                    EmptyLineCount++;
                }
            }
        }

        internal void WriteTo(TextWriter writer)
        {
            for (nuint i = 0; i < EmptyLineCount; i++)
            {
                writer.WriteLine();
            }

            if (Value is not null)
            {
                writer.WriteLine($"STRING {Key.ToString($"D{KeyPrecision}")}");
                if (Comment is not null)
                {
                    writer.WriteLine($"//{Comment.Replace("\r\n", writer.NewLine, StringComparison.Ordinal)}");
                }

                // The reader joins the lines of a comment or a value with \r\n; they are written with the writer's line break.
                writer.WriteLine("{");
                writer.WriteLine(Value.Replace("\r\n", writer.NewLine, StringComparison.Ordinal));
                writer.WriteLine("}");
            }
        }
    }
}