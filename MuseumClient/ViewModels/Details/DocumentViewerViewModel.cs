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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private string _status = "";
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        private string? _localPdfPath;
        public string? LocalPdfPath
        {
            get => _localPdfPath;
            set
            {
                _localPdfPath = value;
                OnPropertyChanged(nameof(LocalPdfPath));
            }
        }

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
        public bool IsText => FileType?.ToLower() is "txt";
        public bool IsHtml => FileType?.ToLower() is "pdf" or "docx" or "md";

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                Status = "Загрузка документа...";

                await LoadMetadataAsync();

                Status = "Загрузка содержимого...";
                await LoadAsync();

                Status = "";
            }
            catch
            {
                Status = "Ошибка загрузки документа";
            }
            finally
            {
                IsLoading = false;
            }
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
                    Text = Encoding.UTF8.GetString(bytes);
                    break;
                case "md":
                    {
                        var markdown = Encoding.UTF8.GetString(bytes);
                        var html = MarkdownConverter.ConvertToHtml(markdown);

                        var htmlPath = Path.Combine(Path.GetTempPath(), $"{_id}_md.html");
                        File.WriteAllText(htmlPath, html, Encoding.UTF8);

                        HtmlPath = new Uri(htmlPath).AbsoluteUri;
                        break;
                    }
                case "pdf":
                    {
                        var path = Path.Combine(Path.GetTempPath(), $"{_id}.pdf");
                        File.WriteAllBytes(path, _rawFile!);

                        LocalPdfPath = path;
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
                    "doc" => "Документ Word 97-2003 (*.doc)|*.doc|Все файлы (*.*)|*.*",
                    "docx" => "Документ Word(*.docx)|*.docx|Все файлы (*.*)|*.*",
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
