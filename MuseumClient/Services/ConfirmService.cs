using MuseumClient.Views.Windows;
using System.Windows;

namespace MuseumClient.Services
{
    public static class ConfirmService
    {
        public static bool ConfirmDelete(string itemName)
        {
            var window = new ConfirmWindow(
                $"Вы действительно хотите удалить {itemName}?"
            );

            window.Owner = Application.Current.MainWindow;

            window.ShowDialog();

            return window.Result;
        }
    }
}