using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MichaelBrandonMorris.Extensions.CollectionExtensions;

namespace MichaelBrandonMorris.DirectoryPermissionTool
{
    /// <summary>
    ///     Class PermissionChecker.
    /// </summary>
    /// TODO Edit XML Comment Template for PermissionChecker
    internal class PermissionChecker
    {
        /// <summary>
        ///     The output path
        /// </summary>
        /// TODO Edit XML Comment Template for OutputPath
        public static readonly string OutputPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "DirectoryPermissionsChecker");

        /// <summary>
        ///     The CSV extension
        /// </summary>
        /// TODO Edit XML Comment Template for CsvExtension
        private const string CsvExtension = ".csv";

        /// <summary>
        ///     The log file name
        /// </summary>
        /// TODO Edit XML Comment Template for LogFileName
        private const string LogFileName = "Log";

        /// <summary>
        ///     The output file name
        /// </summary>
        /// TODO Edit XML Comment Template for OutputFileName
        private const string OutputFileName = "DirectoryPermissions";

        /// <summary>
        ///     The text extension
        /// </summary>
        /// TODO Edit XML Comment Template for TxtExtension
        private const string TxtExtension = ".txt";

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="PermissionChecker" /> class.
        /// </summary>
        /// <param name="searchPaths">The search paths.</param>
        /// <param name="excludedPaths">The excluded paths.</param>
        /// <param name="searchDepth">The search depth.</param>
        /// <param name="shouldIncludeFiles">
        ///     if set to <c>true</c>
        ///     [should include files].
        /// </param>
        /// <param name="shouldSplitPathLevels">
        ///     if set to <c>true</c>
        ///     [should split path levels].
        /// </param>
        /// <param name="excludedGroups">The excluded groups.</param>
        /// TODO Edit XML Comment Template for #ctor
        internal PermissionChecker(
            IEnumerable<string> searchPaths,
            IEnumerable<string> excludedPaths,
            SearchDepth searchDepth,
            bool shouldIncludeFiles,
            bool shouldSplitPathLevels,
            IEnumerable<string> excludedGroups)
        {
            CancellationTokenSource = new CancellationTokenSource();
            Log = new List<string>();
            ShouldSplitPathLevels = shouldSplitPathLevels;
            ExcludedGroups = excludedGroups;

            PermissionGetter = new PermissionGetter(
                searchPaths,
                excludedPaths,
                searchDepth,
                shouldIncludeFiles,
                CancellationTokenSource.Token,
                Log);

            Directory.CreateDirectory(OutputPath);
        }

        /// <summary>
        ///     Gets the result.
        /// </summary>
        /// <value>The result.</value>
        /// TODO Edit XML Comment Template for Result
        public string Result
        {
            get;
            private set;
        }

        /// <summary>
        ///     Gets the cancellation token source.
        /// </summary>
        /// <value>The cancellation token source.</value>
        /// TODO Edit XML Comment Template for CancellationTokenSource
        private CancellationTokenSource CancellationTokenSource
        {
            get;
        }

        /// <summary>
        ///     Gets the excluded groups.
        /// </summary>
        /// <value>The excluded groups.</value>
        /// TODO Edit XML Comment Template for ExcludedGroups
        private IEnumerable<string> ExcludedGroups
        {
            get;
        }

        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>The log.</value>
        /// TODO Edit XML Comment Template for Log
        private IList<string> Log
        {
            get;
        }

        /// <summary>
        ///     Gets the permission getter.
        /// </summary>
        /// <value>The permission getter.</value>
        /// TODO Edit XML Comment Template for PermissionGetter
        private PermissionGetter PermissionGetter
        {
            get;
        }

        /// <summary>
        ///     Gets a value indicating whether [should split path
        ///     levels].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [should split path levels];
        ///     otherwise, <c>false</c>.
        /// </value>
        /// TODO Edit XML Comment Template for ShouldSplitPathLevels
        private bool ShouldSplitPathLevels
        {
            get;
        }

        /// <summary>
        ///     Cancels this instance.
        /// </summary>
        /// TODO Edit XML Comment Template for Cancel
        internal void Cancel()
        {
            CancellationTokenSource.Cancel();
        }

        /// <summary>
        ///     Executes this instance.
        /// </summary>
        /// <returns>Task.</returns>
        /// TODO Edit XML Comment Template for Execute
        internal async Task Execute()
        {
            var task = Task.Run(
                () =>
                {
                    var permissionInfos = PermissionGetter.GetPermissionInfos();

                    Result = new PermissionInfoFormatter(
                        permissionInfos,
                        PermissionGetter.MaxPathLevels,
                        ShouldSplitPathLevels,
                        ExcludedGroups,
                        CancellationTokenSource.Token).FormatDirectories();
                },
                CancellationTokenSource.Token);

            await task;

            var now = DateTime.Now;
            WriteAndOpenResult(now);
            WriteAndOpenLog(now);
        }

        /// <summary>
        ///     Writes the and open log.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// TODO Edit XML Comment Template for WriteAndOpenLog
        private void WriteAndOpenLog(DateTime timestamp)
        {
            if (Log.IsNullOrEmpty())
            {
                return;
            }

            var logFileName = Path.Combine(
                OutputPath,
                $"{LogFileName} - "
                + $"{timestamp:yyyyMMddTHHmmss}{TxtExtension}");

            File.WriteAllLines(logFileName, Log);
            Process.Start(logFileName);
        }

        /// <summary>
        ///     Writes the and open result.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// TODO Edit XML Comment Template for WriteAndOpenResult
        private void WriteAndOpenResult(DateTime timestamp)
        {
            var resultFileName = Path.Combine(
                OutputPath,
                $"{OutputFileName} - "
                + $"{timestamp:yyyyMMddTHHmmss}{CsvExtension}");

            File.WriteAllText(resultFileName, Result);
            Process.Start(resultFileName);
        }
    }
}