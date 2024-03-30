using System.Text;

namespace AdventureScript
{
    public static class StringHelpers
    {
        static char[]? m_buffer = null;

        static char[] GetBuffer(int size)
        {
            var buffer = Interlocked.Exchange(ref m_buffer, null);
            if (buffer != null && buffer.Length >= size)
            {
                return buffer;
            }
            else
            {
                return new char[size];
            }
        }

        static void ReleaseBuffer(char[] buffer)
        {
            Interlocked.MemoryBarrierProcessWide();
            m_buffer = buffer;
        }

        static bool IsRegexSpecialChar(char ch)
        {
            const ulong lowMask =
                0x0000010000000000ul | // 1ul << '('
                0x0000020000000000ul | // 1ul << ')'
                0x0000400000000000ul | // 1ul << '.'
                0x8000000000000000ul | // 1ul << '?'
                0x0000040000000000ul | // 1ul << '*'
                0x0000080000000000ul;  // 1ul << '+'
            const ulong highMask =
                0x0000000010000000ul | // 1ul << ('\' - 64)
                0x0000000008000000ul | // 1ul << ('[' - 64)
                0x0000000020000000ul;  // 1ul << (']' - 64)

            return (ch < 64) ? (lowMask & (1ul << ch)) != 0 :
                ch < 128 && (highMask & (1ul << (ch - 64))) != 0;
        }

        public static string EscapeRegexSpecialChars(string input)
        {
            // Scan for special characters.
            int i = 0;
            while (i < input.Length && IsRegexSpecialChar(input[i]))
                i++;

            // Return the input string if there are not special characters.
            if (i == input.Length)
                return input;

            // Get a buffer of the maximum size we need for the escaped string,
            // which is twice the input length (i.e., to escape every character).
            var buffer = GetBuffer(input.Length * 2);

            // Copy the characters that don't require escaping.
            int di; // destination index
            for (di = 0; di < i; di++)
            {
                buffer[di] = input[di];
            }

            // Scan the remainder of the input string.
            for (; i < input.Length; i++)
            {
                char ch = input[i];
                if (IsRegexSpecialChar(ch))
                {
                    buffer[di++] = '\\';
                }
                buffer[di++] = ch;
            }

            string result = new string(buffer, 0, di);
            ReleaseBuffer(buffer);

            return result;
        }

        public static string NormalizeSpaces(string input)
        {
            // Early out for empty string.
            if (input.Length == 0)
                return input;

            // Scan the string to see if normalization is needed.
            // Break at leading space or second consecutive space.
            char lastChar = ' ';
            int blackLength = 0; // index one past last non-space character
            int i;
            for (i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                if (ch == ' ')
                {
                    if (lastChar == ' ')
                        break;
                }
                else
                {
                    blackLength = i + 1;
                }
                lastChar = ch;
            }

            // Did we scan the whole string without breaking?
            if (i == input.Length)
            {
                // Check for trailing spaces.
                return (blackLength == i) ?
                    input :
                    input.Substring(0, blackLength);
            }

            // Get a buffer to build the normalized string.
            var buffer = GetBuffer(input.Length);

            // Copy the characters that don't require normalization.
            int di; // destination index
            for (di = 0; di < i; di++)
            {
                buffer[di] = input[di];
            }

            // Scan the remainder of the input string.
            for (; i < input.Length; i++)
            {
                char ch = input[i];
                if (ch == ' ')
                {
                    if (lastChar != ' ')
                    {
                        buffer[di++] = ch;
                    }
                }
                else
                {
                    buffer[di++] = ch;
                    blackLength = di;
                }
                lastChar = ch;
            }

            var result = new string(buffer, 0, blackLength);

            ReleaseBuffer(buffer);

            return result;
        }

        public static string NormalizeInputString(string input, string[] ignoreWords)
        {
            var chars = input.ToLowerInvariant().ToCharArray();

            // Replace certain punctuation with spaces.
            for (int i = 0; i < chars.Length; i++)
            {
                if ("\"'.,?;:".Contains(chars[i]))
                {
                    chars[i] = ' ';
                }
            }

            int resultLength = 0;

            void AddWord(int index, int length)
            {
                var span = chars.AsSpan(index, length);
                for (int i = 0; i < ignoreWords.Length; i++)
                {
                    if (span.SequenceEqual(ignoreWords[i]))
                        return;
                }

                if (resultLength != 0)
                {
                    chars[resultLength++] = ' ';
                }

                if (index > resultLength)
                {
                    for (int i = 0; i < length; i++)
                    {
                        chars[resultLength + i] = chars[index + i];
                    }
                }

                resultLength += length;
            }

            int wordPos = 0;
            while (wordPos < chars.Length && chars[wordPos] == ' ')
            {
                wordPos++;
            }

            for (int i = wordPos; i < chars.Length; i++)
            {
                if (chars[i] == ' ')
                {
                    if (i > wordPos)
                    {
                        AddWord(wordPos, i - wordPos);
                    }
                    wordPos = i + 1;
                }
            }

            if (chars.Length > wordPos)
            {
                AddWord(wordPos, chars.Length - wordPos);
            }

            return new string(chars, 0, resultLength);
        }

        static readonly char[] m_escapedChars = new char[] { '\"', '\n', '\\' };

        public static string ToStringLiteral(string input)
        {
            // Allocate a buffer equal to the maximum possible size.
            var buffer = GetBuffer(input.Length * 2 + 2);
            int length = 0;

            // Add the opening quotation mark.
            buffer[length++] = '\"';

            // Copy the remaining characters, escaping those that require it.
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                switch (ch)
                {
                    case '\t':
                        // Replace tabs with spaces
                        buffer[length++] = ' ';
                        break;

                    case '\n':

                        // Replace newline with \n
                        buffer[length++] = '\\';
                        buffer[length++] = 'n';
                        break;

                    case '\\':
                    case '\"':
                        // Escape other special characters
                        buffer[length++] = '\\';
                        buffer[length++] = ch;
                        break;

                    default:
                        if (ch >= 0x20)
                        {
                            // Add literal, non-control character.
                            buffer[length++] = ch;
                        }
                        else
                        {
                            // Elide control character. This includes the return
                            // character, which is always followed by newline anyway.
                        }
                        break;
                }
            }

            // Add the closing quotation mark.
            buffer[length++] = '\"';

            string result = new string(buffer, 0, length);

            ReleaseBuffer(buffer);

            return result;
        }

        // Returns the length of the token if valid or -1 if not.
        public static int ParseStringLiteral(ReadOnlySpan<char> input, out string value)
        {
            value = string.Empty;
            if (input.Length == 0 || input[0] != '\"')
                return -1;

            var buffer = GetBuffer(input.Length);
            int length = 0;

            for (int i = 1; i < input.Length; i++)
            {
                char ch = input[i];

                if (ch == '\"')
                {
                    // Success!
                    value = new string(buffer, 0, length);
                    ReleaseBuffer(buffer);
                    return i + 1;
                }
                else if (ch == '\\')
                {
                    if (++i < input.Length)
                    {
                        ch = input[i];
                        switch (ch)
                        {
                            case 'n':
                                buffer[length++] = '\n';
                                break;

                            case '\\':
                            case '\"':
                                buffer[length++] = ch;
                                break;

                            default:
                                // Error
                                i = input.Length;
                                break;
                        }
                    }
                    else
                    {
                        // Error
                        break;
                    }
                }
                else
                {
                    // Unescaped character.
                    buffer[length++] = ch;
                }
            }

            // Invalid token.
            ReleaseBuffer(buffer);
            return -1;
        }
    }
}
