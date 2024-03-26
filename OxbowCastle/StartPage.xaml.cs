using AdventureScript;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.IO;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using System;
using System.Linq;

namespace OxbowCastle
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartPage : Page
    {
        public StartPage()
        {
            this.InitializeComponent();
            InitializeGameList();
        }

        void InitializeGameList()
        {
            var gameList = new List<GameReference>();

            // Add saved games.
            var savedGamesDir = App.SavedGamesDir;
            if (Directory.Exists(savedGamesDir))
            {
                foreach (var dir in new DirectoryInfo(savedGamesDir).GetDirectories())
                {
                    gameList.Add(new SavedGameReference(dir.Name));
                }
            }

            // Add new games.
            foreach (var dir in new DirectoryInfo(App.GamesDir).GetDirectories())
            {
                gameList.Add(new NewGameReference(dir.Name));
            }

            // Add the browse option.
            gameList.Add(new BrowseGameReference());

            m_gameListControl.ItemsSource = gameList;
        }

        private void GameButton_Click(object sender, RoutedEventArgs e)
        {
            var gameInfo = ((Button)sender).DataContext as GameReference;
            if (gameInfo != null)
            {
                gameInfo.Invoke(this);
            }
        }

        void LaunchNewGame(string sourceDir)
        {
            try
            {
                // Get the source file info.
                var sourceDirInfo = new DirectoryInfo(sourceDir);
                string sourceFilePath = Path.Combine(sourceDirInfo.FullName, App.GameFileName);
                string gameName = sourceDirInfo.Name;

                // Load the game.
                var game = new GameState();
                var output = game.LoadGame(sourceFilePath);

                // Get the destination directory and file.
                var savedGamesDir = App.SavedGamesDir;
                var destDir = Path.Combine(savedGamesDir, gameName);
                var destFilePath = Path.Combine(destDir, App.GameFileName);

                // Make sure the saved games directory exists.
                // Delete any old destination directory.
                if (!Directory.Exists(savedGamesDir))
                {
                    Directory.CreateDirectory(savedGamesDir);
                }
                else if (Directory.Exists(destDir))
                {
                    Directory.Delete(destDir, /*recursive*/ true);
                }

                // Create the destination directory.
                Directory.CreateDirectory(destDir);

                // Copy all from the source directory except *.txt and *.md.
                foreach (var file in sourceDirInfo.GetFiles())
                {
                    if (file.Extension != ".txt" && file.Extension != ".md")
                    {
                        File.Copy(file.FullName, Path.Combine(destDir, file.Name));
                    }
                }

                // Start the game.
                StartGame(game, destFilePath, output.ToArray());
            }
            catch (ParseException e)
            {
                ShowErrorMessage(e.Message);
            }
        }

        internal void LaunchNewGame(NewGameReference gameInfo)
        {
            LaunchNewGame(Path.Combine(App.GamesDir, gameInfo.Name));
        }

        internal void LoadSavedGame(SavedGameReference gameInfo)
        {
            var gameDir = Path.Combine(App.SavedGamesDir, gameInfo.Name);

            StartGame(Path.Combine(gameDir, App.GameFileName));
        }

        internal async void BrowseForGame()
        {
            // Create the file picker
            var folderPicker = new FolderPicker();

            // Get the current window's HWND by passing in the Window object
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            // Use file picker like normal!
            var folder = await folderPicker.PickSingleFolderAsync().AsTask();

            if (folder != null)
            {
                string dirPath = folder.Path;

                string filePath = Path.Combine(dirPath, App.GameFileName);
                if (File.Exists(filePath))
                {
                    LaunchNewGame(dirPath);
                }
                else
                {
                    ShowErrorMessage($"File not found in directory: {App.GameFileName}.");
                }
            }
        }

        void StartGame(GameState game, string filePath, string[] initialOutput)
        {
            var app = (App)(Application.Current);
            app.ActiveGame = new ActiveGame(game, filePath, initialOutput);
            this.Frame.Navigate(typeof(GamePage));
        }


        void StartGame(string filePath)
        {
            try
            {
                var game = new GameState();
                var output = game.LoadGame(filePath);
                StartGame(game, filePath, output.ToArray());
            }
            catch (ParseException e)
            {
                ShowErrorMessage(e.Message);
            }
        }

        async void ShowErrorMessage(string message)
        {
            var messageDialog = new MessageDialog(message);

            // Get the current window's HWND by passing in the Window object
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // Associate the HWND with the message dialog
            WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, hwnd);

            // Show the message dialog
            await messageDialog.ShowAsync();
        }
    }
}
