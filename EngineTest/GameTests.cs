using AdventureLib;

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
            string gamePath = config.GamePath($"{name}.txt");
            string inputPath = config.TestFilePath($"{name}-input.txt");
            string traceFileName = $"{name}-trace.txt";
            string outputPath = config.OutputPath(traceFileName);

            using (var reader = new StreamReader(inputPath))
            {
                using (var writer = new StreamWriter(outputPath))
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
            TestGame(config, "Oxbow-Castle");

        }
    }
}
