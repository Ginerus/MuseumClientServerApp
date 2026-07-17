using System.Windows;
using System.Windows.Input;

namespace MuseumClient.Views
{
    public partial class ConfirmWindow : Window
    {
        public bool Result { get; private set; }

        public string Message { get; }


        public ConfirmWindow(string message)
        {
            InitializeComponent();

            Message = message;

            DataContext = this;
        }


        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }


        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }


    }
}