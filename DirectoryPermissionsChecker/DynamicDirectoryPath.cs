using System.Windows.Input;
using Extensions.PrimitiveExtensions;
using GalaSoft.MvvmLight.CommandWpf;
using Ookii.Dialogs.Wpf;

namespace DirectoryPermissionTool
{
    internal class DynamicDirectoryPath : DynamicText
    {
        public ICommand BrowseFolderCommand => new RelayCommand(BrowseFolder);

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
    }
}