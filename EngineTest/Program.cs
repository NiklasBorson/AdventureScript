﻿using AdventureLib;
using EngineTest;

static class Program
{

    static void Main(string[] args)
    {
        try
        {
            var config = new TestConfig(args);

            MiscTests.Run();
            LexerTest.Run(config);
            GameTests.Run(config);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error: {0}", e.Message);
        }
    }
}
