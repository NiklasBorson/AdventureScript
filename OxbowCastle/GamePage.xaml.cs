using AdventureScript;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OxbowCastle
{
    public sealed partial class GamePage : Page
    {
        ActiveGame m_game;
        string m_lastCommand = null;

        public GamePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            m_game = App.Current.ActiveGame;
            m_lastCommand = null;

            m_titleTextBlock.Text = m_game.Title;

            m_outputStackPanel.Children.Clear();
            AddOutput(m_game.LastOutput);

            m_commandTextBox.IsEnabled = !m_game.Game.IsGameOver;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            m_game?.Save();

            App.Current.ActiveGame = null;
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
            var path = Path.Combine(m_game.FolderPath, fileName);

            var bitmapImage = new BitmapImage();
            bitmapImage.UriSource = new System.Uri(path);
            var image = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Stretch = Microsoft.UI.Xaml.Media.Stretch.None,
                Source = bitmapImage
            };

            m_outputStackPanel.Children.Add(image);
        }

        void AddOutput(string[] output)
        {
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
            m_outputScrollViewer.Measure(m_outputScrollViewer.RenderSize);
            m_outputScrollViewer.ChangeView(null, m_outputScrollViewer.ScrollableHeight, null);
        }
        void InvokeCommand(string input)
        {
            if (input != string.Empty)
            {
                // Add the command itself to the output.
                AddOutputText(input, m_commandStyle);

                // Process the command get its output.
                var output = m_game.Game.InvokeCommand(input).ToArray();

                // Remember the output of the last command.
                m_game.LastOutput = output;

                // Display the output.
                AddOutput(output);

                // Disable the text box if the game is over.
                if (m_game.Game.IsGameOver)
                {
                    m_commandTextBox.IsEnabled = false;
                }
            }
        }

        void TextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Enter:
                    m_lastCommand = m_commandTextBox.Text;
                    InvokeCommand(m_lastCommand);
                    m_commandTextBox.Text = string.Empty;
                    e.Handled = true;
                    break;

                case VirtualKey.Up:
                    if (m_lastCommand != null)
                    {
                        m_commandTextBox.Text = m_lastCommand;
                    }
                    e.Handled = true;
                    break;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
