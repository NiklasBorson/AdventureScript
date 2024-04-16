using AdventureScript;
using System.IO;
using System.IO.Compression;

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

        static public ActiveGame CreateNew(string sourceFilePath)
        {
            string gameName = Path.GetFileNameWithoutExtension(sourceFilePath);

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

            // Extract the file to the destination directory.
            ZipFile.ExtractToDirectory(sourceFilePath, destFolderPath);

            // Load the game.
            var game = new GameState();
            game.LoadGame(destFilePath);

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
