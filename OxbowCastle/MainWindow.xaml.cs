using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace OxbowCastle
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.AppWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;

            this.InitializeComponent();

            m_frame.Navigate(typeof(StartPage));
        }
    }
}
