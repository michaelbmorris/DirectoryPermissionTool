using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DirectoryPermissionTool
{
    internal class DynamicText : INotifyPropertyChanged
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

        private void NotifyPropertyChanged(
            [CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
