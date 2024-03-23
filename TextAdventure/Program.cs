using AdventureLib;
using System.Diagnostics;

namespace TextAdventure
{
    class Program
    {
        static void Run(
            string inputFileName,
            string? outputFileName,
            TextWriter? traceFile,
            bool parseFlag
            )
        {
            try
            {
                var program = new Game(inputFileName, traceFile);

                if (!parseFlag)
                {
                    program.Run();
                }

                if (outputFileName != null)
                {
                    program.Save(outputFileName);
                }
            }
            catch (ParseException e)
            {
                Console.Error.WriteLine($"Error: {e.Message}");
            }
        }

        public static void Main(string[] args)
        {
            bool parseFlag = false;
            string? traceFileName = null;
            string? inputFileName = null;
            string? outputFileName = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-parse")
                {
                    parseFlag = true;
                }
                else if (args[i] == "-trace")
                {
                    if (++i < args.Length)
                    {
                        traceFileName = args[i];
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: Expected file name after -trace.");
                        return;
                    }
                }
                else if (inputFileName == null)
                {
                    inputFileName = args[i];
                }
                else if (outputFileName == null)
                {
                    outputFileName = args[i];
                }
                else
                {
                    Console.Error.WriteLine("Error: Too many command line arguments.");
                }
            }

            if (inputFileName == null)
            {
                Console.Error.WriteLine("Error: No input file specified.");
                return;
            }

            if (traceFileName != null)
            {
                using (var traceFile = new StreamWriter(traceFileName))
                {
                    Run(inputFileName, outputFileName, traceFile, parseFlag);
                }
            }
            else
            {
                Run(inputFileName, outputFileName, null, parseFlag);
            }
        }
    }
}
