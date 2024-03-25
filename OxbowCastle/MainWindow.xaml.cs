using ABI.System;
using AdventureScript;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Windows.Media;
using Windows.Media.AppBroadcasting;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace OxbowCastle
{
    public sealed partial class MainWindow : Window
    {
        string m_gamePath = string.Empty;
        GameState m_game = null;
        string[] m_lastOutput = null;


        public MainWindow()
        {
            this.InitializeComponent();

            InitializeGameList();

            this.AppWindow.Closing += AppWindow_Closing;
        }

        private void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
        {
            SaveGame();
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

            // Add the browse option.
            gameList.Add(new BrowseGameInfo());

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
                StartGame(game, output.ToArray(), destFilePath);
            }
            catch (ParseException e)
            {
                ShowErrorMessage(e.Message);
            }
        }

        internal void LaunchNewGame(NewGameInfo gameInfo)
        {
            LaunchNewGame(Path.Combine(App.GamesDir, gameInfo.Name));
        }

        internal void LoadSavedGame(SavedGameInfo gameInfo)
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

                string filePath = Path.Combine(dirPath, "adventure.txt");
                if (File.Exists(filePath))
                {
                    LaunchNewGame(dirPath);
                }
                else
                {
                    ShowErrorMessage("The folder does not have an adventure.txt file.");
                }
            }
        }

        void SaveGame(GameState game, IList<string> lastOutput, string gamePath)
        {
            using (var writer = new StreamWriter(gamePath))
            {
                writer.WriteLine("game {");
                foreach (var para in lastOutput)
                {
                    writer.WriteLine("    Message(\"{0}\");", para);
                }
                writer.WriteLine('}');

                game.Save(writer);
            }
        }

        void SaveGame()
        {
            if (m_game != null && m_lastOutput != null)
            {
                SaveGame(m_game, m_lastOutput, m_gamePath);
            }
        }

        void AddOutputText(string text, Style style)
        {
            m_outputStackPanel.Children.Add(new TextBlock
            {
                Text = text,
                Style = style
            });
        }

        void AddOutputListItem(string text)
        {
            var grid = new Grid();
            grid.Children.Add(new TextBlock
            {
                Text = "\x2022",
                Style = m_listBulletStyle
            });
            grid.Children.Add(new TextBlock
            {
                Text = text,
                Style = m_listParaStyle
            });
            m_outputStackPanel.Children.Add(grid);
        }

        void AddOutputImage(string fileName)
        {
            var path = Path.Combine(
                Path.GetDirectoryName(m_gamePath),
                fileName
                );

            var bitmapImage = new BitmapImage();
            bitmapImage.UriSource = new System.Uri(path);
            var image = new Image 
            {
                Stretch = Microsoft.UI.Xaml.Media.Stretch.None,
                Source = bitmapImage 
            };

            m_outputStackPanel.Children.Add(image);
        }

        void AddOutput(IList<string> output)
        {
            m_lastOutput = output.ToArray();

            foreach (var para in output)
            {
                if (para.StartsWith("# "))
                {
                    AddOutputText(para.Substring(2), m_headingParaStyle);
                }
                else if (para.StartsWith("- "))
                {
                    AddOutputListItem(para.Substring(2));
                }
                else if (para.StartsWith('[') && para.EndsWith(']'))
                {
                    AddOutputImage(para.Substring(1, para.Length - 2));
                }
                else
                {
                    AddOutputText(para, m_bodyParaStyle);
                }
            }
        }

        void StartGame(GameState game, string[] output, string filePath)
        {
            m_game = game;
            m_gamePath = filePath;
            m_lastOutput = output;

            m_outputStackPanel.Children.Clear();
            AddOutput(output);

            m_gameControl.Visibility = Visibility.Visible;
            m_gameListControl.Visibility = Visibility.Collapsed;
        }

        void StartGame(string filePath)
        {
            try
            {
                var game = new GameState();
                var output = game.LoadGame(filePath);
                StartGame(game, output.ToArray(), filePath);
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

        void InvokeCommand(string input)
        {
            if (input != string.Empty)
            {
                AddOutputText(input, m_commandStyle);
                AddOutput(m_game.InvokeCommand(input));

                m_outputScrollViewer.Measure(m_outputScrollViewer.RenderSize);

                m_outputScrollViewer.ChangeView(null, m_outputScrollViewer.ScrollableHeight, null);
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
