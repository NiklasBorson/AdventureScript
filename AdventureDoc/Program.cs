using AdventureScript;

namespace AdventureDoc
{

    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("Usage: AdventureDoc <input-file> <output-file>");
                    return;
                }

                string inputFile = args[0];
                string outputFile = args[1];

                // Parse the input.
                var sink = new ParserSink();
                var game = new GameState();
                game.LoadGame(inputFile, sink);

                // Write the output.
                sink.Write(outputFile);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error: {0}", e.Message);
            }
        }
    }
}
