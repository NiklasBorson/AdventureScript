namespace AdventureLib
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

            return new string(buffer, 0, di);
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

    }
}
