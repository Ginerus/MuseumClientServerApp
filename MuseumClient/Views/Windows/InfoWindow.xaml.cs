using System.Windows;
using System.Windows.Input;

namespace MuseumClient.Views.Windows
{
    public partial class InfoWindow : Window
    {
        public string Message { get; }


        public InfoWindow(string message)
        {
            InitializeComponent();

            Message = message;

            DataContext = this;
        }


        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void Window_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}