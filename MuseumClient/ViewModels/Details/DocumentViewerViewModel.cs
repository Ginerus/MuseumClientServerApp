using Microsoft.Win32;
using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static MuseumClient.Models.MediaFileDto;

namespace MuseumClient.ViewModels.Details
{
    public class DocumentViewerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly ApiService _apiService;
        private readonly int _id;

        public string FileType { get; set; } = "";
        public string? LocalPdfPath { get; set; }

        private string? _text;
        public string? Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
        }

        private bool _isPdfReady;
        public bool IsPdfReady
        {
            get => _isPdfReady;
            set
            {
                _isPdfReady = value;
                OnPropertyChanged(nameof(IsPdfReady));
            }
        }

        private byte[]? _rawFile;

        public bool IsPdf => FileType?.ToLower() == "pdf";
        public bool IsText => FileType?.ToLower() is "txt" or "md";

        private async Task InitializeAsync()
        {
            await LoadMetadataAsync();
            await LoadAsync();
        }

        private string _title = "";
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public DocumentViewerViewModel(int id, string fileType)
        {
            _id = id;
            FileType = fileType;

            RefreshFileTypeFlags();

            _apiService = new ApiService(
                new ConfigService().Server,
                AuthService.Instance()
            );

            DownloadCommand = new RelayCommand(async _ => await DownloadAsync());

            _ = InitializeAsync();
        }

        private async Task LoadMetadataAsync()
        {
            var response = await _apiService.GetAsync<ApiResponse<MediaFileDto>>($"MediaFile/{_id}");

            Title = response.data.Title;
        }

        private void RefreshFileTypeFlags()
        {
            OnPropertyChanged(nameof(IsPdf));
            OnPropertyChanged(nameof(IsText));
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
                    {
                        var path = Path.Combine(Path.GetTempPath(), $"{_id}.pdf");
                        File.WriteAllBytes(path, _rawFile!);

                        LocalPdfPath = path;
                        OnPropertyChanged(nameof(LocalPdfPath));
                        break;
                    }
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
