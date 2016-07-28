using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Extensions.PrimitiveExtensions;
using GalaSoft.MvvmLight.CommandWpf;
using Ookii.Dialogs.Wpf;

namespace DirectoryPermissionTool
{
    internal class DynamicDirectoryPath : INotifyPropertyChanged
    {
        private Visibility _addButtonVisibility;
        private string _text;

        public Visibility AddButtonVisibility
        {
            get
            {
                return _addButtonVisibility;
            }
            set
            {
                if (_addButtonVisibility == value)
                {
                    return;
                }

                _addButtonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand BrowseFolderCommand =>
            new RelayCommand(BrowseFolder);

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text != null && value != null && _text.Equals(value))
                {
                    return;
                }

                _text = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void BrowseFolder()
        {
            var folderDialog = new VistaFolderBrowserDialog();
            folderDialog.ShowDialog();
            if (!folderDialog.SelectedPath.IsNullOrWhiteSpace() &&
                !folderDialog.SelectedPath.Equals(Text))
            {
                Text = folderDialog.SelectedPath;
            }
        }

        private void NotifyPropertyChanged(
            [CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}