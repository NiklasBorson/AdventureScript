using Microsoft.UI.Xaml;

namespace OxbowCastle
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            m_frame.Navigate(typeof(StartPage));
        }
    }
}
