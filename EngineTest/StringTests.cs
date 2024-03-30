using AdventureScript;
using System.Runtime.CompilerServices;

namespace EngineTest
{
    static class StringTests
    {
        record struct StringTestCase(string Input, string ExpectedOutput);

        static void TestNormalizeSpaces()
        {
            Console.WriteLine("StringTests.TestNormalizeSpaces");

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

        static void TestEscapeRegexSpecialChars()
        {
            Console.WriteLine("StringTests.TestEscapeRegexSpecialChars");

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

        // This function isn't really a test, but it's a
        // convenient place to put this code.
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

        static void TestToStringLiteral()
        {
            Console.WriteLine("StringTests.TestToStringLiteral");

            var testCases = new StringTestCase[]
            {
                new StringTestCase("input", "\"input\""),
                new StringTestCase("Say, \"Hello!\"", "\"Say, \\\"Hello!\\\"\""),
                new StringTestCase("line one\nline two", "\"line one\\nline two\""),
                new StringTestCase("line one\r\nline two", "\"line one\\nline two\""),
            };

            foreach (var testCase in testCases)
            {
                string output = StringHelpers.ToStringLiteral(testCase.Input);

                if (output != testCase.ExpectedOutput)
                {
                    Console.Error.WriteLine("Error: Incorrect output from ParseStringLiteral.");
                    Console.Error.WriteLine("       Input:    {0}", testCase.Input);
                    Console.Error.WriteLine("       Output:   {0}", output);
                    Console.Error.WriteLine("       Expected: {0}", testCase.ExpectedOutput);
                }
            }
        }

        static void TestParseStringLiteral()
        {
            Console.WriteLine("StringTests.ParseStringLiteral");

            var testCases = new StringTestCase[]
            {
                // Invalid inputs
                new StringTestCase("", ""),
                new StringTestCase("input", ""),
                new StringTestCase("\"input", ""),

                // Valid inputs with and without escapes
                new StringTestCase("\"input\"$end", "input"),
                new StringTestCase("\"line one\\nline two\"$end", "line one\nline two"),
                new StringTestCase("\"Say, \\\"Hello!\\\"\"$end", "Say, \"Hello!\""),
            };

            foreach (var testCase in testCases)
            {
                string output;
                int tokenLength = StringHelpers.ParseStringLiteral(testCase.Input.AsSpan(), out output);

                if (output != testCase.ExpectedOutput)
                {
                    Console.Error.WriteLine("Error: Incorrect output from ParseStringLiteral.");
                    Console.Error.WriteLine("       Input:    {0}", StringHelpers.ToStringLiteral(testCase.Input));
                    Console.Error.WriteLine("       Output:   {0}", StringHelpers.ToStringLiteral(output));
                    Console.Error.WriteLine("       Expected: {0}", StringHelpers.ToStringLiteral(testCase.ExpectedOutput));
                }

                int expectedLength = testCase.Input.IndexOf('$');
                if (tokenLength != expectedLength)
                {
                    Console.Error.WriteLine("Error: Incorrect token length from ParseString.");
                    Console.Error.WriteLine("       Input:    {0}", StringHelpers.ToStringLiteral(testCase.Input));
                    Console.Error.WriteLine("       Actual:   {0}", tokenLength);
                    Console.Error.WriteLine("       Expected: {0}", expectedLength);
                }
            }
        }

        public static void Run()
        {
            TestNormalizeSpaces();
            TestEscapeRegexSpecialChars();
            //CodeGenEscapeRegex();
            TestToStringLiteral();
            TestParseStringLiteral();
        }
    }
}
