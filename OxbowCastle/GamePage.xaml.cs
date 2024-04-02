using AdventureScript;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System.IO;
using System.Collections.Generic;
using Windows.System;
using System;

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

            m_outputControl.Blocks.Clear();
            AddOutput(m_game.Game.LastOutput);

            m_commandTextBox.IsEnabled = !m_game.Game.IsGameOver;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            m_game?.Save();

            App.Current.ActiveGame = null;
        }

        public string FolderPath => m_game.FolderPath;

        void AddOutput(IList<string> output)
        {
            MarkdownParser.AddContent(
                this,
                output,
                m_outputControl
                );

            ScrollToEnd();
        }

        public void ScrollToEnd()
        {
            m_outputScrollViewer.Measure(m_outputScrollViewer.RenderSize);
            m_outputScrollViewer.ChangeView(null, m_outputScrollViewer.ScrollableHeight, null);
        }

        void InvokeCommand(string input)
        {
            if (input != string.Empty)
            {
                // Add the command itself to the output.
                // TODO - specify command style
                MarkdownParser.AddCommandParagraph(input, m_outputControl);

                // Process the command get its output.
                var output = m_game.Game.InvokeCommand(input);

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
