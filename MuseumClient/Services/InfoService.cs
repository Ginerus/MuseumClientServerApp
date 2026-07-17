using MuseumClient.Views.Windows;
using System.Windows;

namespace MuseumClient.Services
{
    public static class InfoService
    {
        public static void Show(string message)
        {
            var window = new InfoWindow(message);

            window.Owner = Application.Current.MainWindow;

            window.ShowDialog();
        }
    }
}