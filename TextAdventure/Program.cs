using AdventureLib;

namespace CsAdventure
{
    class Program
    {
        GameState m_game;

        Program(string inputFile)
        {
            m_game = new GameState();
            var output = m_game.LoadGame(inputFile);
            WriteOutput(output);
        }

        void WriteOutput(IList<string> output)
        {
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
            int lineStart = startPos;

            while (para.Length - lineStart > ColumnWidth)
            {
                int maxLength = ColumnWidth - (currentLinePrefix.Length + lineSuffix.Length);
                int lastBreakStart = lineStart;
                int lastBreakEnd = lineStart;

                for (int i = lineStart; i < para.Length; i++)
                {
                    if (para[i] == ' ')
                    {
                        int breakStart = i++;
                        while (i < para.Length && para[i] == ' ')
                            i++;

                        if (lastBreakStart > lineStart && i - lineStart >= maxLength)
                        {
                            WriteLine(para, lineStart, lastBreakStart - lineStart, currentLinePrefix, lineSuffix);
                            currentLinePrefix = linePrefix;
                            lineStart = lastBreakEnd;
                            break;
                        }

                        lastBreakStart = breakStart;
                        lastBreakEnd = i;
                    }
                }
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

        void Run()
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            while (input != null && input != "q")
            {
                var output = m_game.InvokeCommand(input);
                WriteOutput(output);

                Console.Write("> ");
                input = Console.ReadLine();
            }
        }

        public void Save(string fileName)
        {
            m_game.Save(fileName);
        }

        public static void Main(string[] args)
        {
            try
            {
                bool parseFlag = false;
                string inputFile = string.Empty;
                string outputFile = string.Empty ;

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-parse")
                    {
                        parseFlag = true;
                    }
                    else if (inputFile == string.Empty)
                    {
                        inputFile = args[i];
                    }
                    else if (outputFile == string.Empty)
                    {
                        outputFile = args[i];
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: Too many command line arguments.");
                    }
                }

                if (inputFile == string.Empty)
                {
                    Console.Error.WriteLine("Error: No input file specified.");
                    return;
                }

                var program = new Program(inputFile);

                if (!parseFlag)
                {
                    program.Run();
                }

                if (outputFile != string.Empty)
                {
                    program.Save(outputFile);
                }
            }
            catch (ParseException e)
            {
                Console.Error.WriteLine($"Error: {e.Message}");
            }
        }
    }
}
