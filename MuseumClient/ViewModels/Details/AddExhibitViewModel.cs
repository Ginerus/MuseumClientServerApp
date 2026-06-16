using MuseumClient.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MuseumClient.ViewModels.Details
{
    public class AddExhibitViewModel : INotifyPropertyChanged
    {
        private readonly ContentHubViewModel _hub;

        public AddExhibitViewModel(ContentHubViewModel hub)
        {
            _hub = hub;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(name));

        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private string _materials = "";
        public string Materials
        {
            get => _materials;
            set
            {
                _materials = value;
                OnPropertyChanged(nameof(Materials));
            }
        }

        private bool _isPermanent;
        public bool IsPermanent
        {
            get => _isPermanent;
            set
            {
                _isPermanent = value;
                OnPropertyChanged(nameof(IsPermanent));
            }
        }

        private string? _selectedImagePath;
        public string? SelectedImagePath
        {
            get => _selectedImagePath;
            set
            {
                _selectedImagePath = value;
                OnPropertyChanged(nameof(SelectedImagePath));
            }
        }

        private BitmapImage? _image;
        public BitmapImage? Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

        public RelayCommand SelectImageCommand { get; }
        public RelayCommand SaveCommand { get; }
    }
}
