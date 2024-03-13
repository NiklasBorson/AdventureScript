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
            foreach (string para in output)
            {
                WriteMessage(para);
            }
        }

        void WriteMessage(string para)
        {
            const int colWidth = 80;

            while (para.Length > colWidth)
            {
                int lastBreakStart = 0;
                int lastBreakEnd = 0;

                for (int i = 0; i < para.Length; i++)
                {
                    if (para[i] == ' ')
                    {
                        int breakStart = i++;
                        while (i < para.Length && para[i] == ' ')
                            i++;

                        if (lastBreakStart > 0 && i >= colWidth)
                        {
                            Console.WriteLine(para.Substring(0, lastBreakStart));
                            para = para.Substring(lastBreakEnd);
                            break;
                        }

                        lastBreakStart = breakStart;
                        lastBreakEnd = i;
                    }
                }
            }

            if (para.Length != 0)
            {
                Console.WriteLine(para);
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
