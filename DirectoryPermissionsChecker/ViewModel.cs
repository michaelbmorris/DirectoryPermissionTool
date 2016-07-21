using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Extensions.PrimitiveExtensions;
using GalaSoft.MvvmLight.CommandWpf;
using static System.Deployment.Application.ApplicationDeployment;

namespace DirectoryPermissionsChecker
{
    internal class ViewModel : INotifyPropertyChanged
    {
        public static readonly string OutputPath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                "DirectoryPermissionsChecker");

        private const string CsvExtension = ".csv";
        private const string DateTimeFormat = "yyyyMMddTHHmmss";
        private AboutWindow _aboutWindow;
        private Visibility _cancelButonVisibility;

        private string _data;
        private PermissionsChecker _directoryPermissionsChecker;
        private bool _isBusy;
        private string _rootPath;
        private List<int> _searchDepths;
        private int _selectedSearchDepthIndex;

        private string _message;

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

        private Visibility _messageVisibility;
        private Visibility _progressBarVisibility;

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

        internal ViewModel()
        {
            SetUpCommands();
            SetUpProperties();
        }

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

        public string Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand GetDirectoryPermissionsCommand { get; private set; }

        public ICommand OpenAboutWindowCommand { get; private set; }
        public ICommand OpenHelpWindowCommand { get; private set; }

        public string RootPath
        {
            get
            {
                return _rootPath;
            }
            set
            {
                _rootPath = value;
                NotifyPropertyChanged();
            }
        }

        public List<int> SearchDepths
        {
            get
            {
                return _searchDepths;
            }
            set
            {
                _searchDepths = value;
                NotifyPropertyChanged();
            }
        }

        public int SelectedSearchDepthIndex
        {
            get
            {
                return _selectedSearchDepthIndex;
            }
            set
            {
                _selectedSearchDepthIndex = value;
                NotifyPropertyChanged();
            }
        }

        public string Version { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OpenHelpWindowCommandExecute()
        {
            //Process.Start("help.chm");
            ShowMessage("Help currently unavailable.");
        }

        private bool CancelCommandCanExecute()
        {
            return _directoryPermissionsChecker != null;
        }

        private void CancelCommandExecute()
        {
            _directoryPermissionsChecker.Cancel();
        }

        private bool GetDirectoryPermissionsCommandCanExecute()
        {
            return !RootPath.IsNullOrWhiteSpace() && !_isBusy;
        }

        private async void GetDirectoryPermissionsCommandExecute()
        {
            _isBusy = true;
            CancelButtonVisibility = Visibility.Visible;
            ProgressBarVisibility = Visibility.Visible;
            try
            {
                _directoryPermissionsChecker = new PermissionsChecker(
                    RootPath,
                    SearchDepths[SelectedSearchDepthIndex]);
                await _directoryPermissionsChecker.Execute();
                Data = _directoryPermissionsChecker.Data;
                _directoryPermissionsChecker = null;
                var fileName = Path.Combine(
                    OutputPath,
                    "DirectoryPermissionsResults - " +
                    DateTime.Now.ToString(DateTimeFormat) + CsvExtension);
                File.WriteAllText(fileName, Data);
                Process.Start(fileName);
            }
            catch (DirectoryNotFoundException e)
            {
                ShowMessage(e.Message);
            }
            _isBusy = false;
            CancelButtonVisibility = Visibility.Hidden;
            ProgressBarVisibility = Visibility.Hidden;
        }

        private void NotifyPropertyChanged(
            [CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
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
            SearchDepths = new List<int>
            {
                -1,
                0,
                1,
                2,
                3,
                4,
                5,
                6,
                7,
                8,
                9,
                10
            };
            try
            {
                Version = CurrentDeployment.CurrentVersion.ToString();
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
        }

        private void HideMessage()
        {
            Message = string.Empty;
            MessageVisibility = Visibility.Hidden;
        }
    }
}