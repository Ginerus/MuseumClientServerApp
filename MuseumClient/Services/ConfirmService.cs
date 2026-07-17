using System.Windows;

namespace MuseumClient.Services
{
    public static class ConfirmService
    {
        public static bool ConfirmDelete(string itemName = "эту статью")
        {
            var result = MessageBox.Show(
                $"Вы действительно хотите удалить {itemName}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }
    }
}