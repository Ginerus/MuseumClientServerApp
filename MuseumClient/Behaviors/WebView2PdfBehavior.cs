using Microsoft.Xaml.Behaviors;
using Microsoft.Web.WebView2.Wpf;
using System;

namespace MuseumClient.Behaviors
{
    public class WebView2PdfBehavior : Behavior<WebView2>
    {
        protected override async void OnAttached()
        {
            base.OnAttached();

            await AssociatedObject.EnsureCoreWebView2Async();

            SetSource();
        }

        private void SetSource()
        {
            if (AssociatedObject.DataContext is ViewModels.Details.DocumentViewerViewModel vm &&
                !string.IsNullOrEmpty(vm.LocalPdfPath))
            {
                AssociatedObject.Source = new Uri(vm.LocalPdfPath, UriKind.Absolute);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}