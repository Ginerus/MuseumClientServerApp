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

namespace MuseumClient.ViewModels.Details
{
    public class DocumentViewerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly ApiService _apiService;
        private readonly int _id;

        private string _fileType = "";
        public string FileType
        {
            get => _fileType;
            set
            {
                _fileType = value;
                OnPropertyChanged(nameof(FileType));
                OnPropertyChanged(nameof(IsPdf));
                OnPropertyChanged(nameof(IsText));
            }
        }


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
        public bool IsDoc => FileType?.ToLower() is "doc" or "docx";
        public bool IsPdfOrDoc => FileType?.ToLower() is "pdf" or "docx";

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

        private string? _htmlPath;
        public string? HtmlPath
        {
            get => _htmlPath;
            set
            {
                _htmlPath = value;
                OnPropertyChanged(nameof(HtmlPath));
            }
        }

        public DocumentViewerViewModel(int id, string fileType)
        {
            _id = id;
            FileType = fileType;

            _apiService = new ApiService(
                new ConfigService().Server,
                AuthService.Instance()
            );

            DownloadCommand = new RelayCommand(async _ => await DownloadAsync());

            _ = InitializeAsync();
        }

        private async Task LoadMetadataAsync()
        {
            var response = await _apiService.GetAsync<DocumentResponse>($"Document/{_id}");

            if (response?.Data != null)
            {
                Title = response.Data.Title;
                FileType = response.Data.FileType;
            }
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
                case "docx":
                    {
                        var path = Path.Combine(Path.GetTempPath(), $"{_id}.docx");
                        File.WriteAllBytes(path, _rawFile!);

                        var html = DocxToHtmlConverter.Convert(path);

                        var htmlPath = Path.Combine(Path.GetTempPath(), $"{_id}.html");
                        File.WriteAllText(htmlPath, html, Encoding.UTF8);

                        HtmlPath = new Uri(htmlPath).AbsoluteUri;
                        OnPropertyChanged(nameof(HtmlPath));
                        break;
                    }
                default:
                    Text = "Формат откроется через внешнее приложение";
                    break;
            }
        }
        private async Task DownloadAsync()
        {
            string extension = FileType?.ToLower() switch
            {
                "pdf" => ".pdf",
                "txt" => ".txt",
                "md" => ".md",
                "doc" => ".doc",
                "docx" => ".docx",
                _ => ""
            };

            var dialog = new SaveFileDialog
            {
                FileName = string.IsNullOrWhiteSpace(Title)
                    ? $"document{extension}"
                    : $"{Title}{extension}",

                Filter = FileType?.ToLower() switch
                {
                    "txt" => "Текстовый документ (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    "md" => "Markdown файл (*.md)|*.md|Все файлы (*.*)|*.*",
                    "pdf" => "PDF документ (*.pdf)|*.pdf|Все файлы (*.*)|*.*",
                    _ => "Все файлы (*.*)|*.*"
                }
            };

            if (dialog.ShowDialog() == true && _rawFile != null)
            {
                string path = dialog.FileName;

                if (!Path.HasExtension(path))
                    path += extension;

                await File.WriteAllBytesAsync(path, _rawFile);
            }
        }
    }
}
