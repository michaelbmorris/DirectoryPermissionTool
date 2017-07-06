using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using MichaelBrandonMorris.DynamicText;
using MichaelBrandonMorris.Extensions.PrimitiveExtensions;
using static System.IO.Path;
using static System.Deployment.Application.ApplicationDeployment;

namespace MichaelBrandonMorris.DirectoryPermissionTool
{
    /// <summary>
    ///     Class ViewModel.
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    /// TODO Edit XML Comment Template for ViewModel
    internal class ViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     The help file
        /// </summary>
        /// TODO Edit XML Comment Template for HelpFile
        private static readonly string HelpFile =
            Combine(Combine("Resources", "Help"), "Help.chm");

        /// <summary>
        ///     The about window
        /// </summary>
        /// TODO Edit XML Comment Template for _aboutWindow
        private AboutWindow _aboutWindow;

        /// <summary>
        ///     The cancel buton visibility
        /// </summary>
        /// TODO Edit XML Comment Template for _cancelButonVisibility
        private Visibility _cancelButonVisibility;

        /// <summary>
        ///     The combined path levels is checked
        /// </summary>
        /// TODO Edit XML Comment Template for _combinedPathLevelsIsChecked
        private bool _combinedPathLevelsIsChecked;

        /// <summary>
        ///     The directory permissions checker
        /// </summary>
        /// TODO Edit XML Comment Template for _directoryPermissionsChecker
        private PermissionChecker _directoryPermissionsChecker;

        /// <summary>
        ///     The include files is checked
        /// </summary>
        /// TODO Edit XML Comment Template for _includeFilesIsChecked
        private bool _includeFilesIsChecked;

        /// <summary>
        ///     The is busy
        /// </summary>
        /// TODO Edit XML Comment Template for _isBusy
        private bool _isBusy;

        /// <summary>
        ///     The message
        /// </summary>
        /// TODO Edit XML Comment Template for _message
        private string _message;

        /// <summary>
        ///     The message is visible
        /// </summary>
        /// TODO Edit XML Comment Template for _messageIsVisible
        private bool _messageIsVisible;

        /// <summary>
        ///     The message z index
        /// </summary>
        /// TODO Edit XML Comment Template for _messageZIndex
        private int _messageZIndex;

        /// <summary>
        ///     The progress bar visibility
        /// </summary>
        /// TODO Edit XML Comment Template for _progressBarVisibility
        private Visibility _progressBarVisibility;

        /// <summary>
        ///     The search depth all is checked
        /// </summary>
        /// TODO Edit XML Comment Template for _searchDepthAllIsChecked
        private bool _searchDepthAllIsChecked;

        /// <summary>
        ///     The search depth children is checked
        /// </summary>
        /// TODO Edit XML Comment Template for _searchDepthChildrenIsChecked
        private bool _searchDepthChildrenIsChecked;

        /// <summary>
        ///     The search depth current is checked
        /// </summary>
        /// TODO Edit XML Comment Template for _searchDepthCurrentIsChecked
        private bool _searchDepthCurrentIsChecked;

        /// <summary>
        ///     The split path levels is checked
        /// </summary>
        /// TODO Edit XML Comment Template for _splitPathLevelsIsChecked
        private bool _splitPathLevelsIsChecked;

        /// <summary>
        ///     The user guide
        /// </summary>
        /// TODO Edit XML Comment Template for _userGuide
        private Process _userGuide;

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="ViewModel" /> class.
        /// </summary>
        /// TODO Edit XML Comment Template for #ctor
        internal ViewModel()
        {
            SetUpProperties();
        }

        /// <summary>
        ///     Gets the add excluded group.
        /// </summary>
        /// <value>The add excluded group.</value>
        /// TODO Edit XML Comment Template for AddExcludedGroup
        public ICommand AddExcludedGroup => new RelayCommand(
            ExecuteAddExcludedGroup,
            CanAddExcludedGroup);

        /// <summary>
        ///     Gets the add excluded path.
        /// </summary>
        /// <value>The add excluded path.</value>
        /// TODO Edit XML Comment Template for AddExcludedPath
        public ICommand AddExcludedPath => new RelayCommand(
            ExecuteAddExcludedPath,
            CanAddExcludedPath);

        /// <summary>
        ///     Gets the add search path.
        /// </summary>
        /// <value>The add search path.</value>
        /// TODO Edit XML Comment Template for AddSearchPath
        public ICommand AddSearchPath => new RelayCommand(
            ExecuteAddSearchPath,
            CanAddSearchPath);

        /// <summary>
        ///     Gets the cancel.
        /// </summary>
        /// <value>The cancel.</value>
        /// TODO Edit XML Comment Template for Cancel
        public ICommand Cancel => new RelayCommand(ExecuteCancel, CanCancel);

        /// <summary>
        ///     Gets the excluded groups.
        /// </summary>
        /// <value>The excluded groups.</value>
        /// TODO Edit XML Comment Template for ExcludedGroups
        public DynamicTextCollection ExcludedGroups
        {
            get;
        } = new DynamicTextCollection();

        /// <summary>
        ///     Gets the excluded paths.
        /// </summary>
        /// <value>The excluded paths.</value>
        /// TODO Edit XML Comment Template for ExcludedPaths
        public DynamicDirectoryPathCollection ExcludedPaths
        {
            get;
        } = new DynamicDirectoryPathCollection();

        /// <summary>
        ///     Gets the get directory permissions.
        /// </summary>
        /// <value>The get directory permissions.</value>
        /// TODO Edit XML Comment Template for GetDirectoryPermissions
        public ICommand GetDirectoryPermissions => new RelayCommand(
            ExecuteGetDirectoryPermissions,
            CanGetDirectoryPermissions);

        /// <summary>
        ///     Gets the open about window command.
        /// </summary>
        /// <value>The open about window command.</value>
        /// TODO Edit XML Comment Template for OpenAboutWindowCommand
        public ICommand OpenAboutWindowCommand => new RelayCommand(
            OpenAboutWindowCommandExecute);

        /// <summary>
        ///     Gets the open help window command.
        /// </summary>
        /// <value>The open help window command.</value>
        /// TODO Edit XML Comment Template for OpenHelpWindowCommand
        public ICommand OpenHelpWindowCommand => new RelayCommand(
            OpenHelpWindowCommandExecute);

        /// <summary>
        ///     Gets the remove excluded group.
        /// </summary>
        /// <value>The remove excluded group.</value>
        /// TODO Edit XML Comment Template for RemoveExcludedGroup
        public ICommand RemoveExcludedGroup => new
            RelayCommand<DynamicText.DynamicText>(
                ExecuteRemoveExcludedGroup,
                CanRemoveExcludedGroup);

        /// <summary>
        ///     Gets the remove excluded path.
        /// </summary>
        /// <value>The remove excluded path.</value>
        /// TODO Edit XML Comment Template for RemoveExcludedPath
        public ICommand RemoveExcludedPath => new
            RelayCommand<DynamicDirectoryPath>(
                ExecuteRemoveExcludedPath,
                CanRemoveExcludedPath);

        /// <summary>
        ///     Gets the remove search path.
        /// </summary>
        /// <value>The remove search path.</value>
        /// TODO Edit XML Comment Template for RemoveSearchPath
        public ICommand RemoveSearchPath => new
            RelayCommand<DynamicDirectoryPath>(
                ExecuteRemoveSearchPath,
                CanRemoveSearchPath);

        /// <summary>
        ///     Gets the search paths.
        /// </summary>
        /// <value>The search paths.</value>
        /// TODO Edit XML Comment Template for SearchPaths
        public DynamicDirectoryPathCollection SearchPaths
        {
            get;
        } = new DynamicDirectoryPathCollection();

        /// <summary>
        ///     Gets the version.
        /// </summary>
        /// <value>The version.</value>
        /// TODO Edit XML Comment Template for Version
        public string Version
        {
            get
            {
                string version;

                try
                {
                    version = CurrentDeployment.CurrentVersion.ToString();
                }
                catch (InvalidDeploymentException)
                {
                    version = "Development Build";
                }

                return version;
            }
        }

        /// <summary>
        ///     Gets or sets the cancel button visibility.
        /// </summary>
        /// <value>The cancel button visibility.</value>
        /// TODO Edit XML Comment Template for CancelButtonVisibility
        public Visibility CancelButtonVisibility
        {
            get => _cancelButonVisibility;
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

        /// <summary>
        ///     Gets or sets a value indicating whether [combined path
        ///     levels is checked].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [combined path levels is checked];
        ///     otherwise, <c>false</c>.
        /// </value>
        /// TODO Edit XML Comment Template for CombinedPathLevelsIsChecked
        public bool CombinedPathLevelsIsChecked
        {
            get => _combinedPathLevelsIsChecked;
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

        /// <summary>
        ///     Gets or sets a value indicating whether [include files
        ///     is checked].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [include files is checked];
        ///     otherwise, <c>false</c>.
        /// </value>
        /// TODO Edit XML Comment Template for IncludeFilesIsChecked
        public bool IncludeFilesIsChecked
        {
            get => _includeFilesIsChecked;
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

        /// <summary>
        ///     Gets or sets a value indicating whether this instance
        ///     is busy.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is busy; otherwise,
        ///     <c>false</c>.
        /// </value>
        /// TODO Edit XML Comment Template for IsBusy
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value)
                {
                    return;
                }

                _isBusy = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        /// TODO Edit XML Comment Template for Message
        public string Message
        {
            get => _message;
            set
            {
                if (_message != null
                    && value != null
                    && _message.Equals(value))
                {
                    return;
                }

                _message = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [message is
        ///     visible].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [message is visible]; otherwise,
        ///     <c>false</c>.
        /// </value>
        /// TODO Edit XML Comment Template for MessageIsVisible
        public bool MessageIsVisible
        {
            get => _messageIsVisible;
            set
            {
                if (_messageIsVisible == value)
                {
                    return;
                }

                _messageIsVisible = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the index of the message z.
        /// </summary>
        /// <value>The index of the message z.</value>
        /// TODO Edit XML Comment Template for MessageZIndex
        public int MessageZIndex
        {
            get => _messageZIndex;
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

        /// <summary>
        ///     Gets or sets the progress bar visibility.
        /// </summary>
        /// <value>The progress bar visibility.</value>
        /// TODO Edit XML Comment Template for ProgressBarVisibility
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
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

        /// <summary>
        ///     Gets or sets a value indicating whether [search depth
        ///     all is checked].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [search depth all is checked];
        ///     otherwise, <c>false</c>.
        /// </value>
        /// TODO Edit XML Comment Template for SearchDepthAllIsChecked
        public bool SearchDepthAllIsChecked
        {
            get => _searchDepthAllIsChecked;
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

        /// <summary>
        ///     Gets or sets a value indicating whether [search depth
        ///     children is checked].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [search depth children is checked];
        ///     otherwise, <c>false</c>.
        /// </value>
        /// TODO Edit XML Comment Template for SearchDepthChildrenIsChecked
        public bool SearchDepthChildrenIsChecked
        {
            get => _searchDepthChildrenIsChecked;
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

        /// <summary>
        ///     Gets or sets a value indicating whether [search depth
        ///     current is checked].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [search depth current is checked];
        ///     otherwise, <c>false</c>.
        /// </value>
        /// TODO Edit XML Comment Template for SearchDepthCurrentIsChecked
        public bool SearchDepthCurrentIsChecked
        {
            get => _searchDepthCurrentIsChecked;
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

        /// <summary>
        ///     Gets or sets a value indicating whether [split path
        ///     levels is checked].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [split path levels is checked];
        ///     otherwise, <c>false</c>.
        /// </value>
        /// TODO Edit XML Comment Template for SplitPathLevelsIsChecked
        public bool SplitPathLevelsIsChecked
        {
            get => _splitPathLevelsIsChecked;
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

        /// <summary>
        ///     Gets the about window.
        /// </summary>
        /// <value>The about window.</value>
        /// TODO Edit XML Comment Template for AboutWindow
        private AboutWindow AboutWindow
        {
            get
            {
                if (_aboutWindow == null
                    || !_aboutWindow.IsVisible)
                {
                    _aboutWindow = new AboutWindow();
                }

                return _aboutWindow;
            }
        }

        /// <summary>
        ///     Gets the user guide.
        /// </summary>
        /// <value>The user guide.</value>
        /// TODO Edit XML Comment Template for UserGuide
        private Process UserGuide
        {
            get
            {
                if (_userGuide != null
                    && !_userGuide.HasExited)
                {
                    _userGuide.Kill();
                }

                _userGuide = new Process
                {
                    StartInfo =
                    {
                        FileName = HelpFile
                    }
                };

                return _userGuide;
            }
        }

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        /// TODO Edit XML Comment Template for PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Determines whether this instance [can add excluded
        ///     group].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance [can add excluded
        ///     group]; otherwise, <c>false</c>.
        /// </returns>
        /// TODO Edit XML Comment Template for CanAddExcludedGroup
        private bool CanAddExcludedGroup()
        {
            return ExcludedGroups.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        /// <summary>
        ///     Determines whether this instance [can add excluded
        ///     path].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance [can add excluded
        ///     path]; otherwise, <c>false</c>.
        /// </returns>
        /// TODO Edit XML Comment Template for CanAddExcludedPath
        private bool CanAddExcludedPath()
        {
            return ExcludedPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        /// <summary>
        ///     Determines whether this instance [can add search path].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance [can add search
        ///     path]; otherwise, <c>false</c>.
        /// </returns>
        /// TODO Edit XML Comment Template for CanAddSearchPath
        private bool CanAddSearchPath()
        {
            return SearchPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        /// <summary>
        ///     Determines whether this instance can cancel.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can cancel;
        ///     otherwise, <c>false</c>.
        /// </returns>
        /// TODO Edit XML Comment Template for CanCancel
        private bool CanCancel()
        {
            return _directoryPermissionsChecker != null;
        }

        /// <summary>
        ///     Determines whether this instance [can get directory
        ///     permissions].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance [can get directory
        ///     permissions]; otherwise, <c>false</c>.
        /// </returns>
        /// TODO Edit XML Comment Template for CanGetDirectoryPermissions
        private bool CanGetDirectoryPermissions()
        {
            return SearchPaths.Any(x => !x.Text.IsNullOrWhiteSpace())
                   && !_isBusy
                   && GetSearchDepth() != SearchDepth.None
                   && PathDisplayOptionIsChecked();
        }

        /// <summary>
        ///     Determines whether this instance [can remove excluded
        ///     group] the specified excluded group.
        /// </summary>
        /// <param name="excludedGroup">The excluded group.</param>
        /// <returns>
        ///     <c>true</c> if this instance [can remove excluded
        ///     group] the specified excluded group; otherwise,
        ///     <c>false</c>.
        /// </returns>
        /// TODO Edit XML Comment Template for CanRemoveExcludedGroup
        private bool CanRemoveExcludedGroup(
            DynamicText.DynamicText excludedGroup = null)
        {
            return ExcludedGroups.Count > 1
                   || ExcludedGroups.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        /// <summary>
        ///     Determines whether this instance [can remove excluded
        ///     path] the specified excluded path.
        /// </summary>
        /// <param name="excludedPath">The excluded path.</param>
        /// <returns>
        ///     <c>true</c> if this instance [can remove excluded path]
        ///     the specified excluded path; otherwise, <c>false</c>.
        /// </returns>
        /// TODO Edit XML Comment Template for CanRemoveExcludedPath
        private bool CanRemoveExcludedPath(
            DynamicDirectoryPath excludedPath = null)
        {
            return ExcludedPaths.Count > 1
                   || ExcludedPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        /// <summary>
        ///     Determines whether this instance [can remove search
        ///     path] the specified search path.
        /// </summary>
        /// <param name="searchPath">The search path.</param>
        /// <returns>
        ///     <c>true</c> if this instance [can remove search path]
        ///     the specified search path; otherwise, <c>false</c>.
        /// </returns>
        /// TODO Edit XML Comment Template for CanRemoveSearchPath
        private bool CanRemoveSearchPath(DynamicDirectoryPath searchPath = null)
        {
            return SearchPaths.Count > 1
                   || SearchPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        /// <summary>
        ///     Executes the add excluded group.
        /// </summary>
        /// TODO Edit XML Comment Template for ExecuteAddExcludedGroup
        private void ExecuteAddExcludedGroup()
        {
            ExcludedGroups.Add(new DynamicText.DynamicText());
        }

        /// <summary>
        ///     Executes the add excluded path.
        /// </summary>
        /// TODO Edit XML Comment Template for ExecuteAddExcludedPath
        private void ExecuteAddExcludedPath()
        {
            ExcludedPaths.Add(new DynamicDirectoryPath());
        }

        /// <summary>
        ///     Executes the add search path.
        /// </summary>
        /// TODO Edit XML Comment Template for ExecuteAddSearchPath
        private void ExecuteAddSearchPath()
        {
            SearchPaths.Add(new DynamicDirectoryPath());
        }

        /// <summary>
        ///     Executes the cancel.
        /// </summary>
        /// TODO Edit XML Comment Template for ExecuteCancel
        private void ExecuteCancel()
        {
            _directoryPermissionsChecker.Cancel();
        }

        /// <summary>
        ///     Executes the get directory permissions.
        /// </summary>
        /// TODO Edit XML Comment Template for ExecuteGetDirectoryPermissions
        private async void ExecuteGetDirectoryPermissions()
        {
            IsBusy = true;
            CancelButtonVisibility = Visibility.Visible;
            ProgressBarVisibility = Visibility.Visible;
            HideMessage();

            try
            {
                _directoryPermissionsChecker = new PermissionChecker(
                    from x in SearchPaths
                    where !x.Text.IsNullOrWhiteSpace()
                    select x.Text,
                    from x in ExcludedPaths
                    where !x.Text.IsNullOrWhiteSpace()
                    select x.Text,
                    GetSearchDepth(),
                    IncludeFilesIsChecked,
                    SplitPathLevelsIsChecked,
                    from x in ExcludedGroups
                    where !x.Text.IsNullOrWhiteSpace()
                    select x.Text);

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

            IsBusy = false;
            CancelButtonVisibility = Visibility.Hidden;
            ProgressBarVisibility = Visibility.Hidden;
        }

        /// <summary>
        ///     Executes the remove excluded group.
        /// </summary>
        /// <param name="excludedGroup">The excluded group.</param>
        /// TODO Edit XML Comment Template for ExecuteRemoveExcludedGroup
        private void ExecuteRemoveExcludedGroup(
            DynamicText.DynamicText excludedGroup)
        {
            if (ExcludedGroups.Count == 1)
            {
                excludedGroup.Text = string.Empty;
            }
            else
            {
                ExcludedGroups.Remove(excludedGroup);
            }
        }

        /// <summary>
        ///     Executes the remove excluded path.
        /// </summary>
        /// <param name="excludedPath">The excluded path.</param>
        /// TODO Edit XML Comment Template for ExecuteRemoveExcludedPath
        private void ExecuteRemoveExcludedPath(
            DynamicDirectoryPath excludedPath)
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

        /// <summary>
        ///     Executes the remove search path.
        /// </summary>
        /// <param name="searchPath">The search path.</param>
        /// TODO Edit XML Comment Template for ExecuteRemoveSearchPath
        private void ExecuteRemoveSearchPath(DynamicDirectoryPath searchPath)
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

        /// <summary>
        ///     Gets the search depth.
        /// </summary>
        /// <returns>SearchDepth.</returns>
        /// TODO Edit XML Comment Template for GetSearchDepth
        private SearchDepth GetSearchDepth()
        {
            if (SearchDepthAllIsChecked)
            {
                return SearchDepth.All;
            }

            if (SearchDepthChildrenIsChecked)
            {
                return SearchDepth.Children;
            }

            return SearchDepthCurrentIsChecked
                ? SearchDepth.Current
                : SearchDepth.None;
        }

        /// <summary>
        ///     Hides the message.
        /// </summary>
        /// TODO Edit XML Comment Template for HideMessage
        private void HideMessage()
        {
            Message = string.Empty;
            MessageIsVisible = false;
            MessageZIndex = -1;
        }

        /// <summary>
        ///     Notifies the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// TODO Edit XML Comment Template for NotifyPropertyChanged
        private void NotifyPropertyChanged(
            [CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Opens the about window command execute.
        /// </summary>
        /// TODO Edit XML Comment Template for OpenAboutWindowCommandExecute
        private void OpenAboutWindowCommandExecute()
        {
            AboutWindow.Show();
            AboutWindow.Activate();
        }

        /// <summary>
        ///     Opens the help window command execute.
        /// </summary>
        /// TODO Edit XML Comment Template for OpenHelpWindowCommandExecute
        private void OpenHelpWindowCommandExecute()
        {
            try
            {
                UserGuide.Start();
            }
            catch (Exception)
            {
                ShowMessage("Could not load help.");
            }
        }

        /// <summary>
        ///     Pathes the display option is checked.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// TODO Edit XML Comment Template for PathDisplayOptionIsChecked
        private bool PathDisplayOptionIsChecked()
        {
            return CombinedPathLevelsIsChecked || SplitPathLevelsIsChecked;
        }

        /// <summary>
        ///     Sets up properties.
        /// </summary>
        /// TODO Edit XML Comment Template for SetUpProperties
        private void SetUpProperties()
        {
            IsBusy = false;
            MessageZIndex = -1;
        }

        /// <summary>
        ///     Shows the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// TODO Edit XML Comment Template for ShowMessage
        private void ShowMessage(string message)
        {
            Message = message + "\n\nDouble-click to dismiss.";
            MessageIsVisible = true;
            MessageZIndex = 1;
        }
    }
}