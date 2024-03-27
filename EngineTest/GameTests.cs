using AdventureScript;
using System.Net.Http.Headers;
using System.Xml.Linq;

namespace EngineTest
{
    static class GameTests
    {
        static void WriteOutput(IList<string> output, TextWriter writer)
        {
            foreach (var line in output)
            {
                writer.WriteLine(line);
            }
        }

        static void TestGame(TestConfig config, string name)
        {
            string gamePath = config.GamePath($"{name}/adventure.txt");

            // The test file names contain hyphens in place of spaces.
            name = name.Replace(' ', '-');

            string inputFileName = $"{name}-input.txt";
            string traceFileName = $"{name}-trace.txt";

            using (var reader = new StreamReader(config.TestFilePath(inputFileName)))
            {
                using (var writer = new StreamWriter(config.OutputPath(traceFileName)))
                {
                    var game = new GameState();
                    WriteOutput(game.LoadGame(gamePath), writer);

                    for (var input = reader.ReadLine();
                        input != null;
                        input = reader.ReadLine())
                    {
                        writer.WriteLine("> {0}", input);

                        WriteOutput(game.InvokeCommand(input), writer);

                        if (game.IsGameOver)
                            break;
                    }
                }
            }

            config.CompareMaster(traceFileName);
        }

        public static void Run(TestConfig config)
        {
            foreach (var gameDir in new DirectoryInfo(config.GamesDir).GetDirectories())
            {
                string gameName = gameDir.Name;
                string gamePath = Path.Combine(gameDir.FullName, "adventure.txt");

                // The test file names contain hyphens in place of spaces.
                string testName = gameName.Replace(' ', '-');
                string inputFileName = $"{testName}-input.txt";
                string traceFileName = $"{testName}-trace.txt";

                // Skip this game if we don't have test data for it.
                string inputFilePath = config.TestFilePath(inputFileName);
                string traceFilePath = config.OutputPath(traceFileName);

                if (!File.Exists(inputFilePath))
                    continue;

                // Output the name of this test case.
                Console.WriteLine($"GameTests.{testName}");

                // Open the input and output files.
                using (var reader = new StreamReader(inputFilePath))
                {
                    using (var writer = new StreamWriter(traceFilePath))
                    {
                        // Load the game.
                        var game = new GameState();
                        WriteOutput(game.LoadGame(gamePath), writer);

                        // "Play" the game by invoking the commands in the input file.
                        for (var input = reader.ReadLine();
                            input != null;
                            input = reader.ReadLine())
                        {
                            // Write the input command and game output to the trace file.
                            writer.WriteLine("> {0}", input);
                            WriteOutput(game.InvokeCommand(input), writer);

                            if (game.IsGameOver)
                                break;
                        }
                    }
                }

                // Compare the generated trace file with the baseline version.
                config.CompareMaster(traceFileName);
            }
        }
    }
}
