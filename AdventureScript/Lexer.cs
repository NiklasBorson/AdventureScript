using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AdventureLib
{
    public enum TokenType
    {
        Error = -1,
        None = 0,
        Int,
        Name,
        Variable,
        String,
        FormatString,
        Symbol
    }

    public enum SymbolId
    {
        None,
        Plus,
        Minus,
        Times,
        Divide,
        LeftParen,
        RightParen,
        LeftBrace,
        RightBrace,
        Period,
        Comma,
        Or,
        And,
        Not,
        Semicolon,
        Colon,
        Assign,
        Less,
        Greater,
        Equals,
        NotEquals,
        LessEquals,
        GreaterEquals,
        QuestionMark,
        Lambda,
        RightArrow,
        MAX_VALUE = RightArrow
    }

    public abstract class Lexer
    {
        // Input file.
        string m_filePath;
        int m_lineNumber;
        int m_colOffset;
        bool m_eof = false;

        // Input string and position.
        string m_inputLine = string.Empty;
        Match m_match = Match.Empty;
        int m_matchPos = 0;
        int m_matchLength = 0;

        // Properties of the current token.
        TokenType m_tokenType = TokenType.None;
        SymbolId m_symbolId = SymbolId.None;
        int m_intValue = 0;

        protected Lexer(string filePath, int lineNumber, int colOffset)
        {
            m_filePath = filePath;
            m_lineNumber = lineNumber;
            m_colOffset = colOffset;
        }

        // Getters.
        public string FilePath => m_filePath;
        public SourcePos SourcePos => new SourcePos(
            m_filePath, 
            m_lineNumber, 
            m_match.Index + m_colOffset
            );
        public bool HaveToken => m_tokenType > TokenType.None;
        public TokenType TokenType => m_tokenType;
        public SymbolId SymbolValue => m_symbolId;
        public int IntValue => m_intValue;
        public ReadOnlySpan<char> NameValue => GetCapture();
        public ReadOnlySpan<char> StringValue => m_inputLine.AsSpan(m_matchPos + 1, m_matchLength - 2);
        public ReadOnlySpan<char> FormatStringValue => m_inputLine.AsSpan(m_matchPos + 2, m_matchLength - 3);

        protected abstract string? ReadLine();

        // Advance to the first or next token in the input string.
        public void Advance()
        {
            ClearToken();

            while (m_tokenType == TokenType.None && !m_eof)
            {
                if (m_match != Match.Empty)
                {
                    m_match = m_match.NextMatch();
                }
                else
                {
                    var line = ReadLine();
                    if (line == null)
                    {
                        m_eof = true;
                        return;
                    }

                    m_inputLine = line.Replace('\t', ' ');
                    m_lineNumber++;

                    m_matchPos = 0;
                    m_matchLength = 0;

                    m_match = m_tokenRegex.Match(m_inputLine);
                }
                InitializeToken();
            }
        }

        void ClearToken()
        {
            m_tokenType = TokenType.None;
            m_symbolId = SymbolId.None;
            m_intValue = 0;
        }

        ReadOnlySpan<char> GetCapture()
        {
            return m_inputLine.AsSpan(m_matchPos, m_matchLength);
        }

        void InitializeToken()
        {
            // Initialize the match position to the end of the previous token.
            m_matchPos += m_matchLength;
            m_matchLength = 0;

            if (m_match.Index > m_matchPos)
            {
                // Unexpected characters between matches.
                m_tokenType = TokenType.Error;
                m_matchLength = m_match.Index - m_matchPos;
            }
            else if (IsGroupMatch(1, TokenType.Int))
            {
                if (!int.TryParse(GetCapture(), out m_intValue))
                {
                    m_tokenType = TokenType.Error;
                }
            }
            else if (IsGroupMatch(2, TokenType.Name))
            {
                if (m_inputLine[m_matchPos] == '$')
                {
                    m_tokenType = TokenType.Variable;
                }
            }
            else if (IsGroupMatch(3, TokenType.String))
            {
                if (m_inputLine[m_matchPos] == '$')
                {
                    m_tokenType = TokenType.FormatString;
                }
            }
            else if (IsGroupMatch(4, TokenType.Symbol))
            {
                m_symbolId = StringToSymbol(GetCapture());
                Debug.Assert(m_symbolId != SymbolId.None);
            }
            else if (IsGroupMatch(5, TokenType.None))
            {
                // End of input line.
                m_match = Match.Empty;
            }
            else
            {
                // No match.
                m_tokenType = TokenType.Error;
                m_matchPos = m_match.Index;
                m_matchLength = 0;
            }
        }

        bool IsGroupMatch(int groupIndex, TokenType tokenType)
        {
            var group = m_match.Groups[groupIndex];
            if (group.Success)
            {
                m_tokenType = tokenType;
                m_matchPos = group.Index;
                m_matchLength = group.Length;
                return true;
            }
            else
            {
                return false;
            }
        }

        static SymbolId StringToSymbol(ReadOnlySpan<char> chars)
        {
            if (chars.Length == 1)
            {
                switch (chars[0])
                {
                    case '+': return SymbolId.Plus;
                    case '-': return SymbolId.Minus;
                    case '*': return SymbolId.Times;
                    case '/': return SymbolId.Divide;
                    case '(': return SymbolId.LeftParen;
                    case ')': return SymbolId.RightParen;
                    case '{': return SymbolId.LeftBrace;
                    case '}': return SymbolId.RightBrace;
                    case '.': return SymbolId.Period;
                    case ',': return SymbolId.Comma;
                    case '!': return SymbolId.Not;
                    case ';': return SymbolId.Semicolon;
                    case ':': return SymbolId.Colon;
                    case '=': return SymbolId.Assign;
                    case '<': return SymbolId.Less;
                    case '>': return SymbolId.Greater;
                    case '?': return SymbolId.QuestionMark;
                }
            }
            else if (chars.Length == 2)
            {
                char first = chars[0];
                char second = chars[1];

                switch (first)
                {
                    case '|':
                        return
                            second == '|' ? SymbolId.Or :
                            SymbolId.None;
                    case '&':
                        return
                            second == '&' ? SymbolId.And :
                            SymbolId.None;
                    case '=':
                        return
                            second == '=' ? SymbolId.Equals :
                            second == '>' ? SymbolId.Lambda :
                            SymbolId.None;
                    case '<':
                        return
                            second == '=' ? SymbolId.LessEquals :
                            SymbolId.None;
                    case '>':
                        return
                            second == '=' ? SymbolId.GreaterEquals :
                            SymbolId.None;
                    case '!':
                        return
                            second == '=' ? SymbolId.NotEquals :
                            SymbolId.None;
                    case '-':
                        return
                            second == '>' ? SymbolId.RightArrow :
                            SymbolId.None;
                }
            }

            return SymbolId.None;
        }

        const string m_intPattern = "[0-9]+";
        const string m_namePattern = "\\$?[A-Za-z_][A-Za-z_0-9]*";
        const string m_stringPattern = "\\$?\"[^\"]*\"";
        const string m_symbolPattern = @"[\?+\*/(){}.,;:]|&&|\|\||=[=>]?|<=?|>=?|!=?|->?";

        // Regular expression that matches one token.
        // Each capture group cooresponds to a token type.
        static readonly Regex m_tokenRegex = new Regex(
            " *(?:" +
            $"({m_intPattern})" +       // 1 -> Int
            $"|({m_namePattern})" +     // 2 -> Name/Variable
            $"|({m_stringPattern})" +   // 3 -> String/FormatString
            $"|({m_symbolPattern})" +   // 4 -> Symbol
            "|(#|$)" +                  // 5 -> end of input
            ")"
            );

        static readonly Regex m_nameRegex = new Regex(
            $"^{m_namePattern}$"
            );

        public static bool IsName(string name)
        {
            return m_nameRegex.IsMatch(name);
        }

        public static string Stringize(string value)
        {
            return $"\"{value}\"";
        }
    }

    public sealed class FileLexer : Lexer, IDisposable
    {
        TextReader m_reader;
        bool m_isMarkdown;
        bool m_inCodeBlock;

        static Regex m_markdownLinkRegex = new Regex(
            @"<([^>]+.md)>|" +
            @"\[[^]]+\]\(([^>]+.md)\)"
            );

        public FileLexer(string filePath) : base(filePath, 0, 0)
        {
            m_reader = new StreamReader(filePath);
            m_isMarkdown = Path.GetExtension(filePath) == ".md";
            m_inCodeBlock = !m_isMarkdown;
        }

        protected override string? ReadLine()
        {
            // Read the next input line.
            string? line = m_reader.ReadLine();

            // Return the raw input if not in markdown mode or EOF.
            if (!m_isMarkdown || line == null)
                return line;

            // In markdown mode, behavior depends on whether we're in a
            // code block. Return string.Empty for ignorable lines rather
            // than skipping them entirely so line numbers are correct.
            if (line.StartsWith("```"))
            {
                m_inCodeBlock = !m_inCodeBlock;
                return string.Empty;
            }

            // Return the raw input string if we're in a code block.
            if (m_inCodeBlock)
            {
                return line;
            }

            // In markdown blocks, for links to included .md files.
            var match = m_markdownLinkRegex.Match(line);
            if (match.Success)
            {
                for (int i = 1; i <= match.Groups.Count; i++)
                {
                    var group = match.Groups[i];
                    if (group.Success)
                    {
                        // Convert the markdown link to an include statement.
                        return $"include \"{group.Value}\";";
                    }
                }
            }
            return string.Empty;
        }

        public void Dispose()
        {
            m_reader.Dispose();
        }
    }

    public sealed class StringLexer : Lexer
    {
        string m_inputString;
        int m_lineIndex = 0;

        public StringLexer(string filePath, string inputString, int lineNumber, int colOffset) :
            base(filePath, lineNumber - 1, colOffset)
        {
            m_inputString = inputString;
        }
        protected override string? ReadLine()
        {
            return m_lineIndex++ == 0 ? m_inputString : null;
        }
    }
}
