using AdventureScript;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OxbowCastle
{
    class ActiveGame
    {
        public ActiveGame(GameState game, string filePath, string[] lastOutput)
        {
            this.Game = game;
            this.FilePath = filePath;
            this.LastOutput = lastOutput;
        }

        public GameState Game { get; }
        public string FilePath { get; }

        public string[] LastOutput { get; set; }

        public void Save()
        {
            using (var writer = new StreamWriter(FilePath))
            {
                writer.WriteLine("game {");
                foreach (var para in LastOutput)
                {
                    writer.WriteLine("    Message(\"{0}\");", para);
                }
                writer.WriteLine('}');

                Game.Save(writer);
            }
        }
    }
}
