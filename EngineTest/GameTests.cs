using AdventureLib;

namespace EngineTest
{
    static class GameTests
    {
        static bool MatchOutput(IList<string> output, TextReader reader, TextWriter writer)
        {
            foreach (string line in output)
            {
                writer.WriteLine(line);
                if (reader.ReadLine() != line)
                    return false;
            }
            return true;
        }

        static bool TestGame(TextReader reader, TextWriter writer, string gamePath)
        {
            var game = new GameState();
            var output = game.LoadGame(gamePath);
            if (!MatchOutput(output, reader, writer))
                return false;

            for (var input = reader.ReadLine();
                input != null;
                input = reader.ReadLine())
            {
                if (!input.StartsWith("> "))
                    return false;

                writer.WriteLine(input);

                output = game.InvokeCommand(input.Substring(2));

                if (!MatchOutput(output, reader, writer))
                    return false;
            }
            return true;
        }

        static void TestGame(TestConfig config, string name)
        {
            string gamePath = config.GamePath($"{name}.txt");
            string traceFilePath = config.TestFilePath($"{name}-trace.txt");
            string outputPath = config.OutputPath($"{name}-trace.txt");

            using (var reader = new StreamReader(traceFilePath))
            {
                using (var writer = new StreamWriter(outputPath))
                {
                    if (!TestGame(reader, writer, gamePath))
                    {
                        Console.Error.WriteLine($"TestGame.{name}: Game output does not match trace.");
                        Console.Error.WriteLine($"  Baseline: {traceFilePath}");
                        Console.Error.WriteLine($"  Output: {outputPath}");
                    }
                }
            }
        }

        public static void Run(TestConfig config)
        {
            TestGame(config, "Oxbow-Castle");

        }
    }
}
