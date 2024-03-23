using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OxbowCastle
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            InitializeGameList();
        }

        void InitializeGameList()
        {
            var gameList = new List<GameInfo>();

            // Add saved games.
            var savedGamesDir = App.SavedGamesDir;
            if (Directory.Exists(savedGamesDir))
            {
                foreach (var dir in new DirectoryInfo(savedGamesDir).GetDirectories())
                {
                    gameList.Add(new SavedGameInfo(dir.Name));
                }
            }

            // Add new games.
            foreach (var dir in new DirectoryInfo(App.GamesDir).GetDirectories())
            {
                gameList.Add(new NewGameInfo(dir.Name));
            }

            m_gameListControl.ItemsSource = gameList;
        }

        private void GameButton_Click(object sender, RoutedEventArgs e)
        {
            var gameInfo = ((Button)sender).DataContext as GameInfo;
            if (gameInfo != null)
            {
                gameInfo.Invoke(this);
            }
        }

        internal void LaunchNewGame(NewGameInfo gameInfo)
        {
            var savedGamesDir = App.SavedGamesDir;
            var destDir = Path.Combine(savedGamesDir, gameInfo.Name);

            if (!Directory.Exists(savedGamesDir))
            {
                Directory.CreateDirectory(savedGamesDir);
            }
            else if (Directory.Exists(destDir))
            {
                // TODO - prompt to confirm overwrite of saved game

                Directory.Delete(destDir, /*recursive*/ true);
            }

            Directory.CreateDirectory(destDir);

            foreach (var file in new DirectoryInfo(Path.Combine(App.GamesDir, gameInfo.Name)).GetFiles())
            {
                File.Copy(
                    file.FullName,
                    Path.Combine(destDir, file.Name)
                    );
            }

            StartGame(Path.Combine(destDir, App.GameFileName));
        }

        internal void LoadSavedGame(SavedGameInfo gameInfo)
        {
            var gameDir = Path.Combine(App.SavedGamesDir, gameInfo.Name);

            StartGame(Path.Combine(gameDir, App.GameFileName));
        }

        void StartGame(string filePath)
        {
            // TODO
        }
    }
}
