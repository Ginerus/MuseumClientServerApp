using Microsoft.Xaml.Behaviors;
using MuseumClient.Models;
using MuseumClient.Services;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MuseumClient.Behaviors
{
    public class VideoHoverBehavior : Behavior<MediaElement>
    {
        private readonly ApiService _apiService;
        private FrameworkElement? _parent;

        public VideoHoverBehavior()
        {
            var config = new ConfigService().Server;
            _apiService = new ApiService(config, AuthService.Instance());
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _parent = FindParent(AssociatedObject);

            if (_parent != null)
            {
                _parent.MouseEnter += OnMouseEnter;
                _parent.MouseLeave += OnMouseLeave;
            }
        }

        protected override void OnDetaching()
        {
            if (_parent != null)
            {
                _parent.MouseEnter -= OnMouseEnter;
                _parent.MouseLeave -= OnMouseLeave;
            }
        }
    
        private async void OnMouseEnter(object sender, MouseEventArgs e)
        {
            var media = AssociatedObject;

            if (media.DataContext is not MediaFileDto video)
                return;

            try
            {
                if (string.IsNullOrEmpty(video.PreviewTempPath))
                {
                    var bytes = await _apiService.GetBytesAsync(
                        $"MediaFile/stream/{video.MediaFileId}?size=preview"
                    );

                    var path = Path.Combine(
                        Path.GetTempPath(),
                        $"video_preview_{video.MediaFileId}.mp4"
                    );

                    await File.WriteAllBytesAsync(path, bytes);

                    video.PreviewTempPath = path;
                }

                media.Source = new Uri(video.PreviewTempPath);
                media.Position = TimeSpan.Zero;
                media.Opacity = 1;
                media.Play();
            }
            catch { }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            var media = AssociatedObject;

            try
            {
                media.Pause();              // сначала пауза (быстрее Stop)
                media.Stop();               // полный стоп
                media.Position = TimeSpan.Zero; // сброс времени
                media.Source = null;        // ВАЖНО: обрыв потока
                media.Opacity = 0;
            }
            catch { }
        }

        private FrameworkElement? FindParent(DependencyObject child)
        {
            while (child != null)
            {
                child = VisualTreeHelper.GetParent(child);

                if (child is FrameworkElement fe)
                    return fe;
            }

            return null;
        }
    }
}