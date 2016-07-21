using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Extensions.CollectionExtensions;
using Extensions.PrimitiveExtensions;

namespace DirectoryPermissionsChecker
{
    internal class PermissionsChecker
    {
        public static readonly string OutputPath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                "DirectoryPermissionsChecker");

        private const string CsvExtension = ".csv";
        private const string DateTimeFormat = "yyyyMMddTHHmmss";
        
        private readonly List<string> _log;
        private readonly string _rootPath;
        private readonly int _searchDepth;
        private CancellationTokenSource _cancellationTokenSource;

        internal PermissionsChecker(
            string rootPath,
            int searchDepth)
        {
            if (!Directory.Exists(rootPath))
            {
                throw new DirectoryNotFoundException(
                    rootPath + " does not exist!");
            }
            _rootPath = rootPath;
            _searchDepth = searchDepth;
            _log = new List<string>();
            Directory.CreateDirectory(OutputPath);
        }

        internal string Data { get; private set; }

        internal void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        internal async Task Execute()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var task = Task.Run(
                () =>
                {
                    Data = GetDirectoryPermissions();
                },
                _cancellationTokenSource.Token);
            await task;
            if (_log.IsEmpty()) return;
            var fileName = Path.Combine(
                OutputPath,
                "DirectoryPermissionsLog - " +
                DateTime.Now.ToString(DateTimeFormat) + CsvExtension);
            File.WriteAllLines(fileName, _log);
        }

        private string GetDirectoryPermissions()
        {
            var rootDirectory = new DirectoryInfo(_rootPath);
            return GetDirectoryPermissions(rootDirectory, 0);
        }

        private string GetDirectoryPermissions(
            DirectoryInfo directory, int currentLevel)
        {
            _cancellationTokenSource.Token.ThrowIfCancellationRequested();
            var results = string.Empty;
            results += directory.GetDirectoryPermissionsString(_log);
            if (currentLevel >= _searchDepth && _searchDepth >= 0)
                return results;
            DirectoryInfo[] directories = null;
            try
            {
                directories = directory.GetDirectories();
                
            }
            catch (UnauthorizedAccessException e)
            {
                _log.Add(e.Message);
            }
            return directories.IsNullOrEmpty()
                    ? results
                // ReSharper disable once AssignNullToNotNullAttribute
                    : directories.Aggregate(
                        results,
                        (current, childDirectory) =>
                            current + GetDirectoryPermissions(
                                childDirectory, ++currentLevel));
        }
    }

    internal static class DirectoryInfoExtensions
    {
        private const char Quote = '"';
        private const char Comma = ',';

        internal static string GetDirectoryPermissionsString(
            this DirectoryInfo directoryInfo, List<string> log)
        {
            var result = string.Empty;
            try
            {
                var fullPath = directoryInfo.FullName;
                var fullPathAsArray = fullPath.Split(
                    Path.DirectorySeparatorChar);
                fullPath = fullPathAsArray.Where(
                    s => !s.IsNullOrWhiteSpace()).Aggregate(
                    string.Empty,
                    (current, s) => current + (Quote + s + Quote + Comma));
                var accessControl = directoryInfo.GetAccessControl();
                var accessRules = accessControl.GetAccessRules(
                    true, true, typeof(NTAccount));
                foreach (FileSystemAccessRule accessRule in accessRules)
                {
                    var identity = accessRule.IdentityReference.Value.Wrap('"');
                    var rights =
                        accessRule.FileSystemRights.ToString().Wrap('"');
                    var type = accessRule.AccessControlType.ToString()
                        .Wrap('"');
                    result += fullPath + string.Join(
                        ",", identity, rights, type) + "\n";
                }
            }
            catch (Exception e)
            {
                log.Add(e.Message);
            }
            return result;
        }
    }
}