using AdventureScript;

namespace AdventureDoc
{

    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 5)
                {
                    Console.WriteLine("Usage: AdventureDoc <input-file> <output-file> <index-title> <heading-text> <heading-url>");
                    return;
                }

                string inputFile = args[0];
                string outputDir = args[1];
                string indexTitle = args[2];
                string headingText = args[3];
                string headingUrl = args[4];

                // Load the game.
                var game = new GameState();
                game.LoadGame(inputFile);

                // Get APIs.
                var apiSet = new ApiSet(game, indexTitle, headingText, headingUrl);

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
