using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MichaelBrandonMorris.Extensions.CollectionExtensions;

namespace MichaelBrandonMorris.DirectoryPermissionTool
{
    internal class PermissionChecker
    {
        public static readonly string OutputPath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                "DirectoryPermissionsChecker");

        private const string CsvExtension = ".csv";
        private const string DateTimeFormat = "yyyyMMddTHHmmss";
        private const string LogFileName = "Log";
        private const string OutputFileName = "DirectoryPermissions";
        private const string TxtExtension = ".txt";

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

        public string Result
        {
            get;
            private set;
        }

        private CancellationTokenSource CancellationTokenSource
        {
            get;
        }

        private IEnumerable<string> ExcludedGroups
        {
            get;
        }

        private IList<string> Log
        {
            get;
        }

        private PermissionGetter PermissionGetter
        {
            get;
        }

        private bool ShouldSplitPathLevels
        {
            get;
        }

        internal void Cancel()
        {
            CancellationTokenSource.Cancel();
        }

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
                            CancellationTokenSource.Token)
                        .FormatDirectories();
                },
                CancellationTokenSource.Token);

            await task;

            var now = DateTime.Now;
            WriteAndOpenResult(now);
            WriteAndOpenLog(now);
        }

        private void WriteAndOpenLog(DateTime timestamp)
        {
            if (Log.IsNullOrEmpty())
            {
                return;
            }
            var logFileName = Path.Combine(
                OutputPath,
                $"{LogFileName} - "
                + $"{timestamp.ToString(DateTimeFormat)}{TxtExtension}");

            File.WriteAllLines(logFileName, Log);
            Process.Start(logFileName);
        }

        private void WriteAndOpenResult(DateTime timestamp)
        {
            var resultFileName = Path.Combine(
                OutputPath,
                $"{OutputFileName} - "
                + $"{timestamp.ToString(DateTimeFormat)}{CsvExtension}");

            File.WriteAllText(resultFileName, Result);
            Process.Start(resultFileName);
        }
    }
}