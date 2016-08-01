using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Extensions.CollectionExtensions;

namespace DirectoryPermissionTool
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

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly PermissionGetter _permissionGetter;
        private readonly List<string> _log;

        private bool ShouldSplitPathLevels
        {
            get;
        }

        internal PermissionChecker(
            IEnumerable<string> searchPaths,
            IEnumerable<string> excludePaths,
            SearchDepth searchDepth,
            bool includeFiles,
            bool shouldSplitPathLevels)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _log = new List<string>();
            ShouldSplitPathLevels = shouldSplitPathLevels;

            _permissionGetter = new PermissionGetter(
                searchPaths,
                excludePaths,
                searchDepth,
                includeFiles,
                _cancellationTokenSource.Token,
                _log);

            Directory.CreateDirectory(OutputPath);
        }

        public string Result { get; private set; }

        internal void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        internal async Task Execute()
        {
            var task = Task.Run(
                () =>
                {
                    var permissionInfos = _permissionGetter.GetPermissionInfos();

                    Result = new PermissionInfoFormatter(
                        permissionInfos, 
                        _permissionGetter.MaxPathLevels,
                        ShouldSplitPathLevels,
                        _cancellationTokenSource.Token)
                        .FormatDirectories();
                },
                _cancellationTokenSource.Token);

            await task;

            var now = DateTime.Now;
            WriteAndOpenResult(now);
            WriteAndOpenLog(now);
        }

        private void WriteAndOpenResult(DateTime timestamp)
        {
            var resultFileName = Path.Combine(
                OutputPath,
                $"{OutputFileName} - " +
                $"{timestamp.ToString(DateTimeFormat)}{CsvExtension}");

            File.WriteAllText(resultFileName, Result);
            Process.Start(resultFileName);
        }

        private void WriteAndOpenLog(DateTime timestamp)
        {
            if (_log.IsNullOrEmpty()) return;
            var logFileName = Path.Combine(
                OutputPath,
                $"{LogFileName} - " +
                $"{timestamp.ToString(DateTimeFormat)}{TxtExtension}");

            File.WriteAllLines(logFileName, _log);
            Process.Start(logFileName);
        }
    }
}