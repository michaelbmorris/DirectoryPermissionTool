using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Extensions.PrimitiveExtensions;
using GalaSoft.MvvmLight.CommandWpf;

namespace DirectoryPermissionTool
{
    internal class ViewModel : INotifyPropertyChanged
    {
        private AboutWindow _aboutWindow;
        private Visibility _cancelButonVisibility;

        private DirectoryPermissionsChecker _directoryPermissionsChecker;
        private bool _isBusy;

        private string _message;

        private Visibility _messageVisibility;
        private int _messageZIndex;
        private Visibility _progressBarVisibility;
        private string _rootPath;
        private bool _searchDepthAllIsChecked;
        private bool _searchDepthChildrenIsChecked;
        private bool _searchDepthCurrentIsChecked;

        internal ViewModel()
        {
            SetUpCommands();
            SetUpProperties();
        }

        public ICommand AddExcludedPathCommand =>
            new RelayCommand(AddExcludedPath, CanAddExcludedPath);

        public ICommand AddSearchPathCommand =>
            new RelayCommand(AddSearchPath, CanAddSearchPath);

        public Visibility CancelButtonVisibility
        {
            get
            {
                return _cancelButonVisibility;
            }
            set
            {
                _cancelButonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand CancelCommand { get; private set; }

        public ObservableCollection<DynamicTextBox> ExcludedPaths { get; private set; }

        public ObservableCollection<DynamicTextBox> SearchPaths { get; private set; }

        public ICommand GetDirectoryPermissionsCommand { get; private set; }

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility MessageVisibility
        {
            get
            {
                return _messageVisibility;
            }
            set
            {
                _messageVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public int MessageZIndex
        {
            get
            {
                return _messageZIndex;
            }
            set
            {
                _messageZIndex = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand OpenAboutWindowCommand { get; private set; }
        public ICommand OpenHelpWindowCommand { get; private set; }

        public Visibility ProgressBarVisibility
        {
            get
            {
                return _progressBarVisibility;
            }
            set
            {
                _progressBarVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand RemoveExcludedPathCommand =>
            new RelayCommand<DynamicTextBox>(
                RemoveExcludedPath, CanRemoveExcludedPath);

        public ICommand RemoveSearchPathCommand => 
            new RelayCommand<DynamicTextBox>(
            RemoveSearchPath, CanRemoveSearchPath);

        public bool SearchDepthAllIsChecked
        {
            get
            {
                return _searchDepthAllIsChecked;
            }
            set
            {
                _searchDepthAllIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool SearchDepthChildrenIsChecked
        {
            get
            {
                return _searchDepthChildrenIsChecked;
            }
            set
            {
                _searchDepthChildrenIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool SearchDepthCurrentIsChecked
        {
            get
            {
                return _searchDepthCurrentIsChecked;
            }
            set
            {
                _searchDepthCurrentIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public string Version { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void AddSearchPath()
        {
            SearchPaths.Add(new DynamicTextBox());
        }

        private void AddExcludedPath()
        {
            ExcludedPaths.Add(new DynamicTextBox());
        }

        private bool CanAddExcludedPath()
        {
            return ExcludedPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        private bool CanAddSearchPath()
        {
            return SearchPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        private bool CancelCommandCanExecute()
        {
            return _directoryPermissionsChecker != null;
        }

        private void CancelCommandExecute()
        {
            _directoryPermissionsChecker.Cancel();
        }

        private bool CanRemoveExcludedPath(DynamicTextBox excludedPath = null)
        {
            return ExcludedPaths.Count > 1;
        }

        private bool CanRemoveSearchPath(DynamicTextBox searchPath = null)
        {
            return SearchPaths.Count > 1;
        }

        private bool GetDirectoryPermissionsCommandCanExecute()
        {
            return SearchPaths.Any(x => !x.Text.IsNullOrWhiteSpace()) &&
                   !_isBusy &&
                   GetSearchDepth() != SearchDepth.None;
        }

        private async void GetDirectoryPermissionsCommandExecute()
        {
            _isBusy = true;
            CancelButtonVisibility = Visibility.Visible;
            ProgressBarVisibility = Visibility.Visible;
            HideMessage();
            try
            {
                _directoryPermissionsChecker = new DirectoryPermissionsChecker(
                    SearchPaths.Select(x => x.Text),
                    ExcludedPaths.Select(x => x.Text),
                    GetSearchDepth());
                await _directoryPermissionsChecker.Execute();
                _directoryPermissionsChecker = null;
            }
            catch (DirectoryNotFoundException e)
            {
                ShowMessage(e.Message);
            }
            catch (OperationCanceledException e)
            {
                ShowMessage(e.Message);
            }
            _isBusy = false;
            CancelButtonVisibility = Visibility.Hidden;
            ProgressBarVisibility = Visibility.Hidden;
        }

        private SearchDepth GetSearchDepth()
        {
            if (SearchDepthAllIsChecked)
                return SearchDepth.All;
            if (SearchDepthChildrenIsChecked)
                return SearchDepth.Children;
            return SearchDepthCurrentIsChecked
                ? SearchDepth.Current
                : SearchDepth.None;
        }

        private void HideMessage()
        {
            Message = string.Empty;
            MessageVisibility = Visibility.Hidden;
            MessageZIndex = -1;
        }

        private void NotifyPropertyChanged(
            [CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        private void OnExcludedPathsChanged(
            object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var excludedPath in ExcludedPaths)
            {
                excludedPath.AddButtonVisibility = Visibility.Hidden;
            }
            ExcludedPaths.Last().AddButtonVisibility = Visibility.Visible;
        }

        private void OnSearchPathsChanged(
            object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var searchPath in SearchPaths)
            {
                searchPath.AddButtonVisibility = Visibility.Hidden;
            }
            SearchPaths.Last().AddButtonVisibility = Visibility.Visible;
        }

        private void OpenAboutWindowCommandExecute()
        {
            if (_aboutWindow == null || !_aboutWindow.IsVisible)
            {
                _aboutWindow = new AboutWindow();
                _aboutWindow.Show();
            }
            else
                _aboutWindow.Activate();
        }

        private void OpenHelpWindowCommandExecute()
        {
            try
            {
                Process.Start("help.chm");
            }
            catch (Exception e)
            {
                ShowMessage($"Could not load help - {e} - {e.Message}");
            }
        }

        private void RemoveExcludedPath(DynamicTextBox excludedPath)
        {
            ExcludedPaths.Remove(excludedPath);
        }

        private void RemoveSearchPath(DynamicTextBox searchPath)
        {
            SearchPaths.Remove(searchPath);
        }

        private void SetUpCommands()
        {
            OpenAboutWindowCommand = new RelayCommand(
                OpenAboutWindowCommandExecute);

            OpenHelpWindowCommand = new RelayCommand(
                OpenHelpWindowCommandExecute);

            GetDirectoryPermissionsCommand = new RelayCommand(
                GetDirectoryPermissionsCommandExecute,
                GetDirectoryPermissionsCommandCanExecute);

            CancelCommand = new RelayCommand(
                CancelCommandExecute, CancelCommandCanExecute);
        }

        private void SetUpProperties()
        {
            _isBusy = false;
            CancelButtonVisibility = Visibility.Hidden;
            ProgressBarVisibility = Visibility.Hidden;
            MessageVisibility = Visibility.Hidden;
            MessageZIndex = -1;
            ExcludedPaths = new ObservableCollection<DynamicTextBox>();
            SearchPaths = new ObservableCollection<DynamicTextBox>();
            ExcludedPaths.CollectionChanged += OnExcludedPathsChanged;
            SearchPaths.CollectionChanged += OnSearchPathsChanged;
            ExcludedPaths.Add(new DynamicTextBox());
            SearchPaths.Add(new DynamicTextBox());
            try
            {
                Version =
                    ApplicationDeployment.CurrentDeployment.CurrentVersion
                        .ToString();
            }
            catch (InvalidDeploymentException)
            {
                Version = "(Development Build)";
            }
        }

        private void ShowMessage(string message)
        {
            Message = message + "\n\nDouble-click to dismiss.";
            MessageVisibility = Visibility.Visible;
            MessageZIndex = 1;
        }

        public class DynamicTextBox : INotifyPropertyChanged
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
}