using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.IO;
using AdventureLib;

namespace OxbowCastle
{
    public sealed partial class MainWindow : Window
    {
        string m_gamePath = string.Empty;
        GameState m_game = null;

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

        void AddOutputText(string text, Style style)
        {
            m_outputStackPanel.Children.Add(new TextBlock
            {
                Text = text,
                Style = style
            });
        }

        void AddOutput(IList<string> output)
        {
            foreach (var para in output)
            {
                if (para.StartsWith("# "))
                {
                    AddOutputText(para.Substring(2), m_headingParaStyle);
                }
                else
                {
                    AddOutputText(para, m_bodyParaStyle);
                }
            }
        }

        void StartGame(string filePath)
        {
            var game = new GameState();
            var output = game.LoadGame(filePath);

            m_game = game;
            m_gamePath = filePath;

            m_outputStackPanel.Children.Clear();
            AddOutput(output);

            m_gameControl.Visibility = Visibility.Visible;
            m_gameListControl.Visibility = Visibility.Collapsed;
        }

        void InvokeCommand(string input)
        {
            if (input != string.Empty)
            {
                AddOutputText(input, m_commandStyle);
                AddOutput(m_game.InvokeCommand(input));

                m_outputScrollViewer.Measure(m_outputScrollViewer.RenderSize);
                m_outputScrollViewer.ScrollToVerticalOffset(m_outputScrollViewer.ScrollableHeight);
            }
        }

        void TextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                InvokeCommand(m_commandTextBox.Text);
                m_commandTextBox.Text = string.Empty;
                e.Handled = true;
            }
        }
    }
}
