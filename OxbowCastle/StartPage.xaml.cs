using AdventureScript;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.IO;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Navigation;

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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
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
            foreach (var file in new DirectoryInfo(App.GamesDir).GetFiles())
            {
                gameList.Add(new NewGameReference(Path.GetFileNameWithoutExtension(file.Name)));
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

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var gameInfo = ((Button)sender).DataContext as GameReference;
            if (gameInfo != null)
            {
                if (await ShowYesNoMessage($"Are you sure you want to delete {gameInfo.Name}?"))
                {
                    var folderPath = Path.Combine(App.SavedGamesDir, gameInfo.Name);
                    Directory.Delete(folderPath, true);
                    InitializeGameList();
                }
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
            LaunchNewGame(Path.Combine(App.GamesDir, $"{gameInfo.Name}.adventure"));
        }

        internal void LoadSavedGame(SavedGameReference gameInfo)
        {
            var folderPath = Path.Combine(App.SavedGamesDir, gameInfo.Name);
            var filePath = Path.Combine(folderPath, App.GameFileName);

            var game = new GameState();
            game.LoadGame(filePath);

            StartGame(new ActiveGame(game, gameInfo.Name, folderPath));
        }

        internal async void BrowseForGame()
        {
            // Create the file picker
            var filePicker = new FileOpenPicker();

            // Associate the app's HWND with the file picker
            App.Current.InitializeWithWindow(filePicker);

            // Browser for .adventure files.
            filePicker.FileTypeFilter.Add(".adventure");
            var file = await filePicker.PickSingleFileAsync();

            // If the user picked a file then launch the game.
            if (file != null)
            {
                LaunchNewGame(file.Path);
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

            // Associate the app's HWND with the message dialog.
            App.Current.InitializeWithWindow(messageDialog);

            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        async Task<bool> ShowYesNoMessage(string message)
        {
            var messageDialog = new MessageDialog(message);

            var yesCommand = new UICommand("Yes");
            var noCommand = new UICommand("No");

            messageDialog.Commands.Add(yesCommand);
            messageDialog.Commands.Add(noCommand);

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;

            // Associate the app's HWND with the message dialog.
            App.Current.InitializeWithWindow(messageDialog);

            // Show the message dialog
            var command = await messageDialog.ShowAsync();

            return object.ReferenceEquals(command, yesCommand);
        }
    }
}
