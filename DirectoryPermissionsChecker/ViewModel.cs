using System;
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
        private PermissionChecker _directoryPermissionsChecker;
        private bool _includeFilesIsChecked;
        private bool _isBusy;
        private string _message;
        private Visibility _messageVisibility;
        private int _messageZIndex;
        private Visibility _progressBarVisibility;
        private bool _searchDepthAllIsChecked;
        private bool _searchDepthChildrenIsChecked;
        private bool _searchDepthCurrentIsChecked;

        internal ViewModel()
        {
            SetUpProperties();
        }

        public ICommand AddExcludedPath =>
            new RelayCommand(ExecuteAddExcludedPath, CanAddExcludedPath);

        public ICommand AddSearchPath =>
            new RelayCommand(ExecuteAddSearchPath, CanAddSearchPath);

        public ICommand Cancel => new RelayCommand(ExecuteCancel, CanCancel);

        public Visibility CancelButtonVisibility
        {
            get
            {
                return _cancelButonVisibility;
            }
            set
            {
                if (_cancelButonVisibility == value)
                {
                    return;
                }

                _cancelButonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<DynamicDirectoryPath> ExcludedPaths
        {
            get;
            private set;
        }

        public ICommand GetDirectoryPermissions => new RelayCommand(
            ExecuteGetDirectoryPermissions,
            CanGetDirectoryPermissions);

        public bool IncludeFilesIsChecked
        {
            get
            {
                return _includeFilesIsChecked;
            }
            set
            {
                if (_includeFilesIsChecked == value)
                {
                    return;
                }

                _includeFilesIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (_message != null &&
                    value != null &&
                    _message.Equals(value))
                {
                    return;
                }

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
                if (_messageVisibility == value)
                {
                    return;
                }

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
                if (_messageZIndex == value)
                {
                    return;
                }

                _messageZIndex = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand OpenAboutWindowCommand => new RelayCommand(
            OpenAboutWindowCommandExecute);

        public ICommand OpenHelpWindowCommand => new RelayCommand(
            OpenHelpWindowCommandExecute);

        public Visibility ProgressBarVisibility
        {
            get
            {
                return _progressBarVisibility;
            }
            set
            {
                if (_progressBarVisibility == value)
                {
                    return;
                }

                _progressBarVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand RemoveExcludedPathCommand =>
            new RelayCommand<DynamicDirectoryPath>(
                RemoveExcludedPath, CanRemoveExcludedPath);

        public ICommand RemoveSearchPathCommand =>
            new RelayCommand<DynamicDirectoryPath>(
                RemoveSearchPath, CanRemoveSearchPath);

        public bool SearchDepthAllIsChecked
        {
            get
            {
                return _searchDepthAllIsChecked;
            }
            set
            {
                if (_searchDepthAllIsChecked == value)
                {
                    return;
                }

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
                if (_searchDepthChildrenIsChecked == value)
                {
                    return;
                }

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
                if (_searchDepthCurrentIsChecked == value)
                {
                    return;
                }

                _searchDepthCurrentIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<DynamicDirectoryPath> SearchPaths
        {
            get;
            private set;
        }

        public string Version
        {
            get;
            private set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool CanAddExcludedPath()
        {
            return ExcludedPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        private bool CanAddSearchPath()
        {
            return SearchPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        private bool CanCancel()
        {
            return _directoryPermissionsChecker != null;
        }

        private bool CanGetDirectoryPermissions()
        {
            return SearchPaths.Any(x => !x.Text.IsNullOrWhiteSpace()) &&
                   !_isBusy &&
                   GetSearchDepth() != SearchDepth.None;
        }

        private bool CanRemoveExcludedPath(
            DynamicDirectoryPath excludedPath = null)
        {
            return ExcludedPaths.Count > 1 || 
                ExcludedPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        private bool CanRemoveSearchPath(
            DynamicDirectoryPath searchPath = null)
        {
            return SearchPaths.Count > 1 ||
                SearchPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        private void ExecuteAddExcludedPath()
        {
            ExcludedPaths.Add(new DynamicDirectoryPath());
        }

        private void ExecuteAddSearchPath()
        {
            SearchPaths.Add(new DynamicDirectoryPath());
        }

        private void ExecuteCancel()
        {
            _directoryPermissionsChecker.Cancel();
        }

        private bool SplitPathLevels()
        {
            return _splitPathLevelsIsChecked;
        }

        private async void ExecuteGetDirectoryPermissions()
        {
            _isBusy = true;
            CancelButtonVisibility = Visibility.Visible;
            ProgressBarVisibility = Visibility.Visible;
            HideMessage();
            try
            {
                _directoryPermissionsChecker = new PermissionChecker(
                    SearchPaths.Select(x => x.Text),
                    ExcludedPaths.Select(x => x.Text),
                    GetSearchDepth(),
                    IncludeFilesIsChecked,
                    SplitPathLevelsIsChecked);
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
                excludedPath.AddButtonVisibility = excludedPath ==
                                                   ExcludedPaths.Last()
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }

        private void OnSearchPathsChanged(
            object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var searchPath in SearchPaths)
            {
                searchPath.AddButtonVisibility = searchPath ==
                                                 SearchPaths.Last()
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }

        private void OpenAboutWindowCommandExecute()
        {
            if (_aboutWindow == null || !_aboutWindow.IsVisible)
            {
                _aboutWindow = new AboutWindow();
                _aboutWindow.Show();
            }
            else
            {
                _aboutWindow.Activate();
            }
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

        private void RemoveExcludedPath(DynamicDirectoryPath excludedPath)
        {
            if (ExcludedPaths.Count == 1)
            {
                excludedPath.Text = string.Empty;
            }
            else
            {
                ExcludedPaths.Remove(excludedPath);
            }
        }

        private void RemoveSearchPath(DynamicDirectoryPath searchPath)
        {
            if (SearchPaths.Count == 1)
            {
                searchPath.Text = string.Empty;
            }
            else
            {
                SearchPaths.Remove(searchPath);
            }
        }

        private bool _splitPathLevelsIsChecked;

        public bool SplitPathLevelsIsChecked
        {
            get
            {
                return _splitPathLevelsIsChecked;
            }
            set
            {
                if (_splitPathLevelsIsChecked == value)
                {
                    return;
                }

                _splitPathLevelsIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        private bool _combinedPathLevelsIsChecked;

        public bool CombinedPathLevelsIsChecked
        {
            get
            {
                return _combinedPathLevelsIsChecked;
            }
            set
            {
                if (_combinedPathLevelsIsChecked == value)
                {
                    return;
                }
                _combinedPathLevelsIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        private void SetUpProperties()
        {
            _isBusy = false;
            CancelButtonVisibility = Visibility.Hidden;
            ProgressBarVisibility = Visibility.Hidden;
            MessageVisibility = Visibility.Hidden;
            MessageZIndex = -1;
            ExcludedPaths = new ObservableCollection<DynamicDirectoryPath>();
            SearchPaths = new ObservableCollection<DynamicDirectoryPath>();
            ExcludedPaths.CollectionChanged += OnExcludedPathsChanged;
            SearchPaths.CollectionChanged += OnSearchPathsChanged;
            ExcludedPaths.Add(new DynamicDirectoryPath());
            SearchPaths.Add(new DynamicDirectoryPath());

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
    }
}