using AdventureLib;

namespace CsAdventure
{
    class Program
    {
        GameState m_game;

        Program(string inputFile)
        {
            m_game = new GameState();
            m_game.MessageEvent += MessageHandler;
            m_game.LoadGame(inputFile);
        }

        static void MessageHandler(object? sender, MessageEventArgs args)
        {
            if (args.MessageType == MessageType.Heading)
            {
                Console.WriteLine("+---------------------");
                Console.WriteLine($"| {args.Message}");
                Console.WriteLine("+---------------------");
            }
            else
            {
                Console.WriteLine(args.Message);
            }
        }

        void Run()
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            while (input != null && input != "q")
            {
                if (!m_game.InvokeCommand(input))
                {
                    Console.WriteLine("I don't understand that.");
                }

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
