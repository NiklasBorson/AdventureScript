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
                var game = ActiveGame.CreateNew(sourceDir);
                StartGame(game);
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
            var folderPath = Path.Combine(App.SavedGamesDir, gameInfo.Name);
            var filePath = Path.Combine(folderPath, App.GameFileName);

            var game = new GameState();
            var output = game.LoadGame(filePath);

            StartGame(new ActiveGame(game, gameInfo.Name, folderPath, output.ToArray()));
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

        void StartGame(ActiveGame game)
        {
            App.Current.ActiveGame = game;
            this.Frame.Navigate(typeof(GamePage));
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
