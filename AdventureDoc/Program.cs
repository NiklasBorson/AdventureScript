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
                string outputDir = args[1];

                // Load the game.
                var game = new GameState();
                game.LoadGame(inputFile);

                // Get APIs.
                var apiSet = new ApiSet(game);

                // Write the output.
                apiSet.Write(outputDir);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error: {0}", e.Message);
            }
        }
    }
}
