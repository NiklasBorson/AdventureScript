using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AdventureScript
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
        Modulo,
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
        string m_stringValue = string.Empty;

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
        public string StringValue => m_stringValue;

        protected abstract string? ReadLine();

        // Advance to the first or next token in the input string.
        public void Advance()
        {
            ClearToken();

            while (m_tokenType == TokenType.None && !m_eof)
            {
                if (m_match != Match.Empty)
                {
                    m_match = m_tokenRegex.Match(m_inputLine, m_matchPos + m_matchLength);
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
            m_stringValue = string.Empty;
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
            else if (IsGroupMatch(CaptureIndex.Int, TokenType.Int))
            {
                if (!int.TryParse(GetCapture(), out m_intValue))
                {
                    m_tokenType = TokenType.Error;
                }
            }
            else if (IsGroupMatch(CaptureIndex.Name, TokenType.Name))
            {
                if (m_inputLine[m_matchPos] == '$')
                {
                    m_tokenType = TokenType.Variable;
                }
            }
            else if (IsGroupMatch(CaptureIndex.String, TokenType.String))
            {
                InitializeStringToken(m_matchPos);
            }
            else if (IsGroupMatch(CaptureIndex.FString, TokenType.FormatString))
            {
                InitializeStringToken(m_matchPos + 1);
            }
            else if (IsGroupMatch(CaptureIndex.Symbol, TokenType.Symbol))
            {
                m_symbolId = StringToSymbol(GetCapture());
                Debug.Assert(m_symbolId != SymbolId.None);
            }
            else if (IsGroupMatch(CaptureIndex.End, TokenType.None))
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

        void InitializeStringToken(int startPos)
        {
            int tokenLength = StringHelpers.ParseStringLiteral(
                m_inputLine.AsSpan(startPos),
                out m_stringValue
                );

            if (tokenLength > 0)
            {
                m_matchLength = startPos + tokenLength - m_matchPos;
            }
            else
            {
                m_tokenType = TokenType.Error;
                m_matchLength = 0;
            }
        }

        bool IsGroupMatch(CaptureIndex groupIndex, TokenType tokenType)
        {
            var group = m_match.Groups[(int) groupIndex];
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
                    case '%': return SymbolId.Modulo;
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
        const string m_stringPattern = "\"";
        const string m_fstringPattern = "\\$\"";
        const string m_symbolPattern = @"[\?+\*%/(){}.,;:]|&&|\|\||=[=>]?|<=?|>=?|!=?|->?";

        enum CaptureIndex : int
        {
            Token,
            Int,
            Name,
            String,
            FString,
            Symbol,
            End
        }

        // Regular expression that matches one token.
        // Each capture group cooresponds to a token type.
        static readonly Regex m_tokenRegex = new Regex(
            " *(?:" +
            $"({m_intPattern})" +       // 1 -> Int
            $"|({m_namePattern})" +     // 2 -> Name/Variable
            $"|({m_stringPattern})" +   // 3 -> String
            $"|({m_fstringPattern})" +  // 4 -> FormatString
            $"|({m_symbolPattern})" +   // 5 -> Symbol
            "|(#|$)" +                  // 6 -> end of input
            ")"
            );

        static readonly Regex m_nameRegex = new Regex(
            $"^{m_namePattern}$"
            );

        public static bool IsName(string name)
        {
            return m_nameRegex.IsMatch(name);
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
