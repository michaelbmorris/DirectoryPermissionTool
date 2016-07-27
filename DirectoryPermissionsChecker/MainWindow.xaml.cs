using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DirectoryPermissionTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Label_MouseDoubleClick(
            object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2) return;
            var message = sender as TextBlock;
            if (message == null) return;
            message.Visibility = Visibility.Hidden;
            message.Text = string.Empty;
            Panel.SetZIndex(message, -1);
        }
    }
}
