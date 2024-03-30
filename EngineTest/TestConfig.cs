namespace EngineTest
{
    class TestConfig
    {
        string m_testFilesDir;
        string m_baselineDir;
        string m_gamesDir;
        string m_outputDir;

        public TestConfig(string[] args)
        {
            for (int i = 0; i + 1 < args.Length; i += 2)
            {
                switch (args[i])
                {
                    case "-testfiles":
                        m_testFilesDir = args[i + 1];
                        break;

                    case "-baseline":
                        m_baselineDir = args[i + 1];
                        break;

                    case "-games":
                        m_gamesDir = args[i + 1];
                        break;

                    case "-output":
                        m_outputDir = args[i + 1];
                        break;

                    default:
                        throw new ArgumentException($"Unknown argument: {args[i]}.");
                }
            }

            // Ensure all the paths were specified.
            if (m_testFilesDir == null)
                throw new ArgumentException($"-testfiles not specified.");
            if (m_baselineDir == null)
                throw new ArgumentException($"-baseline not specified.");
            if (m_gamesDir == null)
                throw new ArgumentException($"-games not specified.");
            if (m_outputDir == null)
                throw new ArgumentException($"-output not specified.");

            // Ensure the output directory exists.
            Directory.CreateDirectory(m_outputDir);
        }

        public string TestFilesDir => m_testFilesDir;
        public string BaselineDir => m_baselineDir;
        public string GamesDir => m_gamesDir;
        public string OutputDir => m_outputDir;

        public string BaselinePath(string fileName) => Path.Combine(BaselineDir, fileName);
        public string TestFilePath(string fileName) => Path.Combine(TestFilesDir, fileName);
        public string GamePath(string fileName) => Path.Combine(GamesDir, fileName);
        public string OutputPath(string fileName) => Path.Combine(OutputDir, fileName);

        public void CompareMaster(string fileName)
        {
            string baselinePath = BaselinePath(fileName);
            string outputPath = OutputPath(fileName);

            using (var baselineReader = new StreamReader(baselinePath))
            {
                using (var outputReader = new StreamReader(outputPath))
                {
                    string? baselineLine = baselineReader.ReadLine();
                    string? outputLine = outputReader.ReadLine();
                    int lineNumber = 1;

                    while (baselineLine != null && baselineLine == outputLine)
                    {
                        baselineLine = baselineReader.ReadLine();
                        outputLine = outputReader.ReadLine();
                        lineNumber++;
                    }

                    if (baselineLine != outputLine)
                    {
                        Console.Error.WriteLine($"Error: {fileName}({lineNumber}): Output differs from baseline.");
                        Console.Error.WriteLine($"  Baseline: {baselinePath}");
                        Console.Error.WriteLine($"  Output: {outputPath}");
                    }
                }
            }
        }
    }
}
