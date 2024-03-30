using AdventureScript;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OxbowCastle
{
    class ActiveGame
    {
        public ActiveGame(GameState game, string title, string folderPath)
        {
            this.Game = game;
            this.Title = title;
            this.FolderPath = folderPath;
        }

        public GameState Game { get; }

        public string Title { get; }
        public string FolderPath { get; }

        public string FilePath => Path.Combine(FolderPath, App.GameFileName);

        static public ActiveGame CreateNew(string sourceFolderPath)
        {
            // Get the source file info.
            var sourceDir = new DirectoryInfo(sourceFolderPath);
            string sourceFilePath = Path.Combine(sourceDir.FullName, App.GameFileName);
            string gameName = sourceDir.Name;

            // Load the game.
            var game = new GameState();
            game.LoadGame(sourceFilePath);

            // Get the destination directory and file.
            var savedGamesDir = App.SavedGamesDir;
            var destFolderPath = Path.Combine(savedGamesDir, gameName);
            var destFilePath = Path.Combine(destFolderPath, App.GameFileName);

            // Make sure the saved games directory exists.
            // Delete any old destination directory.
            if (!Directory.Exists(savedGamesDir))
            {
                Directory.CreateDirectory(savedGamesDir);
            }
            else if (Directory.Exists(destFolderPath))
            {
                Directory.Delete(destFolderPath, /*recursive*/ true);
            }

            // Create the destination directory.
            Directory.CreateDirectory(destFolderPath);

            // Copy all from the source directory except *.txt and *.md.
            foreach (var file in sourceDir.GetFiles())
            {
                if (file.Extension != ".txt" && file.Extension != ".md")
                {
                    File.Copy(file.FullName, Path.Combine(destFolderPath, file.Name));
                }
            }

            return new ActiveGame(game, gameName, destFolderPath);
        }

        public void Save()
        {
            if (!Game.IsGameOver)
            {
                Game.Save(FilePath);
            }
            else
            {
                Directory.Delete(FolderPath, true);
            }
        }
    }
}
