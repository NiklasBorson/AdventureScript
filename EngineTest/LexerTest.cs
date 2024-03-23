using AdventureLib;

namespace EngineTest
{
    static class LexerTest
    {
        static void WriteTokens(Lexer lexer, TextWriter writer)
        {
            for (lexer.Advance(); lexer.HaveToken; lexer.Advance())
            {
                writer.Write(
                    "{0} ({1},{2}): ",
                    Path.GetFileName(lexer.SourcePos.FileName),
                    lexer.SourcePos.LineNumber,
                    lexer.SourcePos.ColumnNumber
                    );

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

        static string TestStringLexer(TestConfig config, string fileName)
        {
            using (var writer = new StreamWriter(config.OutputPath(fileName)))
            {
                var lexer = new StringLexer("", "$varName", 1, 0);
                WriteTokens(lexer, writer);
            }
            return fileName;
        }

        static string TestLexer(TestConfig config, string fileName)
        {
            using (var writer = new StreamWriter(config.OutputPath(fileName)))
            {
                using (var lexer = new FileLexer(config.TestFilePath(fileName)))
                {
                    WriteTokens(lexer, writer);
                }
            }
            return fileName;
        }

        public static void Run(TestConfig config)
        {
            config.CompareMaster(TestStringLexer(config, "StringLexer.txt"));
            config.CompareMaster(TestLexer(config, "LexerTest.txt"));
        }
    }
}
