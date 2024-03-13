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
            const int colWidth = 80;

            bool inList = false;

            foreach (var para in output)
            {
                if (para.StartsWith("- "))
                {
                    WriteWrapped(
                        para,
                        /*colWidth*/ colWidth - 2,
                        /*startPos*/ 2,
                        /*firstLinePrefix*/ "- ",
                        /*linePrefix*/ "  "
                        );
                    inList = true;
                }
                else
                {
                    if (inList)
                    {
                        Console.WriteLine();
                        inList = false;
                    }
                    WriteWrapped(
                        para,
                        /*colWidth*/ colWidth,
                        /*startPos*/ 0,
                        /*firstLinePrefix*/ string.Empty,
                        /*linePrefix*/ string.Empty
                        );
                    Console.WriteLine();
                }
            }
            if (inList)
            {
                Console.WriteLine();
            }
        }

        void WriteWrapped(
            string para,
            int colWidth,
            int startPos,
            string firstLinePrefix,
            string linePrefix
            )
        {
            if (para.Length <= startPos)
                return;

            int lineStart = startPos;

            while (para.Length - lineStart > colWidth)
            {
                int lastBreakStart = lineStart;
                int lastBreakEnd = lineStart;

                for (int i = lineStart; i < para.Length; i++)
                {
                    if (para[i] == ' ')
                    {
                        int breakStart = i++;
                        while (i < para.Length && para[i] == ' ')
                            i++;

                        if (lastBreakStart > lineStart && i - lineStart >= colWidth)
                        {
                            var line = para.Substring(lineStart, lastBreakStart - lineStart);
                            Console.Write(lineStart == startPos ? firstLinePrefix : linePrefix);
                            Console.WriteLine(line);
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
                Console.Write(lineStart == startPos ? firstLinePrefix : linePrefix);
                Console.WriteLine(para.Substring(lineStart));
            }
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
