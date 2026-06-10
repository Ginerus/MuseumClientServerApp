using Microsoft.Win32;
using MuseumClient.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using MuseumClient.Commands;
using System.Threading.Tasks;
using System.IO;

namespace MuseumClient.ViewModels.Details
{
    public class DocumentViewerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly ApiService _apiService;
        private readonly int _id;

        public string Title { get; set; } = "";
        public string FileType { get; set; } = "";

        private string? _text;
        public string? Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
        }

        private byte[]? _rawFile;

        public DocumentViewerViewModel(int id, string fileType)
        {
            _id = id;
            FileType = fileType;

            _apiService = new ApiService(
                new ConfigService().Server,
                AuthService.Instance()
            );

            DownloadCommand = new RelayCommand(async _ => await DownloadAsync());

            _ = LoadAsync();
        }

        public RelayCommand DownloadCommand { get; }

        private async Task LoadAsync()
        {
            var bytes = await _apiService.GetBytesAsync($"Document/stream/{_id}");

            _rawFile = bytes;

            switch (FileType.ToLower())
            {
                case "txt":
                case "md":
                    Text = Encoding.UTF8.GetString(bytes);
                    break;

                case "pdf":
                    // позже можно WebView2
                    Text = "PDF preview (todo WebView2)";
                    break;

                default:
                    Text = "Формат откроется через внешнее приложение";
                    break;
            }
        }

        private async Task DownloadAsync()
        {
            var dialog = new SaveFileDialog
            {
                FileName = Title,
                Filter = "All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true && _rawFile != null)
            {
                File.WriteAllBytes(dialog.FileName, _rawFile);
            }
        }
    }


}
