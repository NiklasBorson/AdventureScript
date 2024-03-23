using AdventureLib;

namespace TextAdventure
{
    class Game
    {
        GameState m_game;
        TextWriter? m_traceFile;

        public Game(string inputFile, TextWriter? traceFile)
        {
            m_game = new GameState();
            m_traceFile = traceFile;
            var output = m_game.LoadGame(inputFile);
            WriteOutput(output);
        }

        void WriteOutput(IList<string> output)
        {
            if (m_traceFile != null)
            {
                foreach (var line in output)
                {
                    m_traceFile.WriteLine(line);
                }
            }

            bool inList = false;

            foreach (var para in output)
            {
                if (para.StartsWith("- "))
                {
                    // Special handling for list items.
                    WriteWrapped(
                        para,
                        /*startPos*/ 2,
                        /*firstLinePrefix*/ "- ",
                        /*linePrefix*/ "  ",
                        /*lineSuffix*/ string.Empty
                        );
                    inList = true;
                }
                else
                {
                    // Not a list item, so end the list if we're on one.
                    if (inList)
                    {
                        Console.WriteLine();
                        inList = false;
                    }

                    if (para.StartsWith("# "))
                    {
                        Console.WriteLine();
                        Console.WriteLine("+------------------------------------------------------------------------------+");
                        WriteWrapped(
                            para,
                            /*startPos*/ 2,
                            /*firstLinePrefix*/ "| ",
                            /*linePrefix*/ "| ",
                            /*lineSuffix*/ " |"
                            );
                        Console.WriteLine("+------------------------------------------------------------------------------+");
                    }
                    else
                    {
                        WriteWrapped(
                            para,
                            /*startPos*/ 0,
                            /*firstLinePrefix*/ string.Empty,
                            /*linePrefix*/ string.Empty,
                            /*lineSuffix*/ string.Empty
                            );
                    }
                    Console.WriteLine();
                }
            }
            if (inList)
            {
                Console.WriteLine();
            }
        }

        const int ColumnWidth = 80;

        void WriteWrapped(
            string para,
            int startPos,
            string firstLinePrefix,
            string linePrefix,
            string lineSuffix
            )
        {
            if (para.Length <= startPos)
                return;

            string currentLinePrefix = firstLinePrefix;
            int maxLength = ColumnWidth - (currentLinePrefix.Length + lineSuffix.Length);
            int lineStart = startPos;

            while (para.Length - lineStart > maxLength)
            {
                int endIndex = Math.Min(para.Length, lineStart + maxLength);

                int breakStart = endIndex;
                int breakEnd = endIndex;

                for (int i = lineStart; i < endIndex; i++)
                {
                    if (para[i] == ' ')
                    {
                        breakStart = i++;
                        while (i < para.Length && para[i] == ' ')
                            i++;
                        breakEnd = i;
                    }
                }

                WriteLine(para, lineStart, breakStart - lineStart, currentLinePrefix, lineSuffix);
                lineStart = breakEnd;
                currentLinePrefix = linePrefix;
                maxLength = ColumnWidth - (currentLinePrefix.Length + lineSuffix.Length);
            }

            if (lineStart < para.Length)
            {
                WriteLine(para, lineStart, para.Length - lineStart, currentLinePrefix, lineSuffix);
            }
        }

        void WriteLine(string input, int startPos, int length, string linePrefix, string lineSuffix)
        {
            Console.Write(linePrefix);
            Console.Write(input.Substring(startPos, length));
            if (lineSuffix.Length != 0)
            {
                int padding = ColumnWidth - (linePrefix.Length + length + lineSuffix.Length);
                for (int i = 0; i < padding; i++)
                {
                    Console.Write(' ');
                }
                Console.Write(lineSuffix);
            }
            Console.WriteLine();
        }

        public void Run()
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            while (input != null && input != "q")
            {
                if (m_traceFile != null)
                {
                    m_traceFile.WriteLine("> {0}", input);
                }

                var output = m_game.InvokeCommand(input);

                WriteOutput(output);

                if (m_game.IsGameOver)
                {
                    break;
                }

                Console.Write("> ");
                input = Console.ReadLine();
            }
        }

        public void Save(string fileName)
        {
            m_game.Save(fileName);
        }
    }
}
