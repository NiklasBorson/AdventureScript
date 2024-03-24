using AdventureLib;
using System.Diagnostics;

namespace TextAdventure
{
    class Program
    {
        static void Run(string inputFileName, TextWriter? traceFile)
        {
            try
            {
                var game = new Game(inputFileName, traceFile);
                game.Run();
            }
            catch (ParseException e)
            {
                Console.Error.WriteLine($"Error: {e.Message}");
            }
        }

        static void Trace(string inputFileName, string traceFileName)
        {
            using (var traceFile = new StreamWriter(traceFileName))
            {
                Run(inputFileName, traceFile);
            }
        }

        static void Compile(string inputFileName, string outputFileName)
        {
            var game = new GameState();
            var output = game.LoadGame(inputFileName);

            using (var writer = new StreamWriter(outputFileName))
            {
                if (output.Count != 0)
                {
                    writer.WriteLine("game {");
                    foreach (var message in output)
                    {
                        writer.WriteLine(
                            "    Message(\"{0}\");",
                            message
                            );
                    }
                    writer.WriteLine("}");
                }
                game.Save(writer);
            }
        }

        const string Usage =
            "TextAdventure <inputFile>\n" +
            "TextAdventure -compile <inputFile> <outputFile>\n" +
            "TextAdventure -trace <inputFile> <traceFile>\n";

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(Usage);
                return;
            }

            string firstArg = args[0];
            if (firstArg[0] == '-')
            {
                if (firstArg == "-?" || firstArg == "--help")
                {
                    Console.WriteLine(Usage);
                }
                else if (firstArg == "-compile" && args.Length == 3)
                {
                    Compile(args[1], args[2]);
                }
                else if (firstArg == "-trace" && args.Length == 3)
                {
                    Trace(args[1], args[2]);
                }
                else
                {
                    Console.Error.WriteLine("Error: Invalid command line.");
                }
            }
            else if (args.Length == 1)
            {
                Run(args[0], null);
            }
            else
            {
                Console.Error.WriteLine("Error: Invalid command line.");
            }
        }
    }
}
