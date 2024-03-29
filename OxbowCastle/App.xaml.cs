﻿using Microsoft.UI.Xaml;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OxbowCastle
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        internal static new App Current => (App)(Application.Current);

        internal ActiveGame ActiveGame { get; set; }

        // Associate the application's HWND with an object such as a
        // folder picker or message box.
        internal void InitializeWithWindow(object obj)
        {
            // Get the current window's HWND by passing in the Window object
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(m_window);

            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(obj, hwnd);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
            m_window.AppWindow.Closing += AppWindow_Closing;
        }

        private void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
        {
            ActiveGame?.Save();
        }

        private Window m_window;

        public static string GamesDir => $"{AppDomain.CurrentDomain.BaseDirectory}\\Assets\\Games";

        public static string SavedGamesDir = $"{ApplicationData.Current.LocalFolder.Path}\\SavedGames";

        public const string GameFileName = "adventure.txt";
    }
}
