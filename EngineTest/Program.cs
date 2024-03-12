using AdventureLib;
using System.Text.RegularExpressions;

static class Program
{
    const string BaselineDir = "Baseline";
    const string InputDir = "TestFiles";
    const string OutputDir = "Output";

    static string BaselinePath(string fileName) => Path.Combine(BaselineDir, fileName);
    static string InputPath(string fileName) => Path.Combine(InputDir, fileName);
    static string OutputPath(string fileName) => Path.Combine(OutputDir, fileName);

    static void CompareMaster(string fileName)
    {
        using (var baselineReader = new StreamReader(BaselinePath(fileName)))
        {
            using (var outputReader = new StreamReader(OutputPath(fileName)))
            {
                string? baselineLine = baselineReader.ReadLine();
                string? outputLine = outputReader.ReadLine();
                int lineNumber = 1;

                while (baselineLine != null && baselineLine == outputLine)
                {
                    baselineLine = baselineReader.ReadLine();
                    outputLine = outputReader.ReadLine();
                    lineNumber++;
                }

                if (baselineLine != outputLine)
                {
                    Console.Error.WriteLine($"{fileName}({lineNumber}): Output differs from baseline.");
                }
            }
        }
    }

    static void WriteTokens(Lexer lexer, TextWriter writer)
    {
        for (lexer.Advance(); lexer.HaveToken; lexer.Advance())
        {
            writer.Write($"{lexer.SourcePos}: ");

            switch (lexer.TokenType)
            {
                case TokenType.Int:
                    writer.Write($"Int: {lexer.IntValue}");
                    break;

                case TokenType.Name:
                    writer.Write($"Name: {lexer.NameValue}");
                    break;

                case TokenType.Variable:
                    writer.Write($"Variable: {lexer.NameValue}");
                    break;

                case TokenType.String:
                    writer.Write($"String: '{lexer.StringValue}'");
                    break;

                case TokenType.FormatString:
                    writer.Write($"FormatString: '{lexer.FormatStringValue}'");
                    break;

                case TokenType.Symbol:
                    writer.Write($"Symbol: {lexer.SymbolValue}");
                    break;
            }

            writer.WriteLine();
        }

        writer.WriteLine(lexer.TokenType);
    }

    static string TestStringLexer(string fileName)
    {
        using (var writer = new StreamWriter(OutputPath(fileName)))
        {
            var lexer = new StringLexer("", "$varName", 1, 0);
            WriteTokens(lexer, writer);
        }
        return fileName;
    }

    static string TestLexer(string fileName)
    {
        using (var writer = new StreamWriter(OutputPath(fileName)))
        {
            using (var lexer = new FileLexer(InputPath(fileName)))
            {
                WriteTokens(lexer, writer);
            }
        }
        return fileName;
    }

    static string TestParser(string fileName)
    {
        try
        {
            var game = new GameState();
            game.LoadGame(InputPath(fileName));
            game.Save(OutputPath(fileName));
        }
        catch (ParseException e)
        {
            Console.Error.WriteLine($"Error: {e.Message}");
        }
        return fileName;
    }

    record struct StringTestCase(string Input, string ExpectedOutput);

    static void TestStringHelpers()
    {
        var testCases = new StringTestCase[]
        {
            new StringTestCase("", ""),
            new StringTestCase("input", "input"),
            new StringTestCase(" input", "input"),
            new StringTestCase("  input", "input"),
            new StringTestCase("input", "input"),
            new StringTestCase("  input  ", "input"),
            new StringTestCase("one two three", "one two three"),
            new StringTestCase("one   two  three", "one two three"),
            new StringTestCase("  one   two  three  ", "one two three"),
        };

        foreach (var testCase in testCases)
        {
            var output = StringHelpers.NormalizeSpaces(testCase.Input);
            if (output != testCase.ExpectedOutput)
            {
                Console.Error.WriteLine("Error: Incorrect output from NormalizedSpaces.");
                Console.Error.WriteLine("       Input:    '{0}'", testCase.Input);
                Console.Error.WriteLine("       Output:   '{0}'", output);
                Console.Error.WriteLine("       Expected: '{0}'", testCase.ExpectedOutput);
            }
        }
    }

    static void CodeGenEscapeRegex()
    {
        const string escapeChars = @"\().?*+[]";

        var lowChars = new List<char>();
        var highChars = new List<char>();
        foreach (char ch in escapeChars)
        {
            ((ch < 64) ? lowChars : highChars).Add(ch);
        }

        Console.WriteLine("const ulong lowMask =");
        for (int i = 0; i < lowChars.Count; i++)
        {
            char ch = lowChars[i];
            Console.WriteLine(
                "    0x{0:X16}ul{1} // 1ul << '{2}'",
                1ul << ch,
                i + 1 < lowChars.Count ? " |" : "; ",
                ch
                );
        }
        Console.WriteLine("const ulong highMask =");
        for (int i = 0; i < highChars.Count; i++)
        {
            char ch = highChars[i];
            Console.WriteLine(
                "    0x{0:X16}ul{1} // 1ul << ('{2}' - 64)",
                1ul << (ch - 64),
                i + 1 < highChars.Count ? " |" : "; ",
                ch
                );
        }
    }
    static void TestEscapeRegexSpecialChars()
    {
        var testCases = new StringTestCase[]
        {
            new StringTestCase("", ""),
            new StringTestCase("input", "input"),
            new StringTestCase(@"input?", @"input\?"),
            new StringTestCase(@"one\two\three", @"one\\two\\three"),
            new StringTestCase(@"one[two].three(four)", @"one\[two\]\.three\(four\)"),
        };

        foreach (var testCase in testCases)
        {
            var output = StringHelpers.EscapeRegexSpecialChars(testCase.Input);
            if (output != testCase.ExpectedOutput)
            {
                Console.Error.WriteLine("Error: Incorrect output from EscapeRegexSpecialChars.");
                Console.Error.WriteLine("       Input:    '{0}'", testCase.Input);
                Console.Error.WriteLine("       Output:   '{0}'", output);
                Console.Error.WriteLine("       Expected: '{0}'", testCase.ExpectedOutput);
            }
        }
    }

    static void Main()
    {
        // Ensure the output directory exists.
        Directory.CreateDirectory(OutputDir);

        CompareMaster(TestStringLexer("StringLexer.txt"));
        CompareMaster(TestLexer("LexerTest.txt"));
        CompareMaster(TestParser("ParserTest.txt"));
        TestStringHelpers();
        TestEscapeRegexSpecialChars();
        //CodeGenEscapeRegex();
    }
}
