using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Extensions.CollectionExtensions;

namespace DirectoryPermissionTool
{
    internal enum SearchDepth
    {
        All,
        Children,
        Current,
        None
    }

    internal class DirectoryInfoGetter
    {
        private const int RootLevel = 0;
        private readonly CancellationToken _cancellationToken;
        private readonly List<string> _log;
        private readonly List<DirectoryInfo> _rootDirectories;
        private readonly SearchDepth _searchDepth;
        private readonly IEnumerable<string> _excludePaths;

        public int MaxFolderLevels { get; private set; }

        internal DirectoryInfoGetter(
            IEnumerable<string> searchPaths,
            IEnumerable<string> excludePaths,
            SearchDepth searchDepth,
            CancellationToken cancellationToken,
            List<string> log)
        {
            _rootDirectories = new List<DirectoryInfo>();
            foreach (var searchPath in searchPaths)
            {
                if (!Directory.Exists(searchPath))
                {
                    throw new DirectoryNotFoundException(
                        $"Could not find directory '{searchPath}'");
                }
                _rootDirectories.Add(new DirectoryInfo(searchPath));
            }

            if (searchDepth == SearchDepth.None)
            {
                throw new ArgumentException("Search Depth must not be null.");
            }

            _excludePaths = excludePaths;
            _searchDepth = searchDepth;
            _cancellationToken = cancellationToken;
            _log = log;
        }

        internal List<DirectoryPermissionInfo> GetDirectories()
        {
            var directoryPermissionInfos = new List<DirectoryPermissionInfo>();
            foreach (var rootDirectory in _rootDirectories)
            {
                try
                {
                    GetDirectories(
                        rootDirectory, directoryPermissionInfos, RootLevel);
                }
                catch (OperationCanceledException)
                {
                    directoryPermissionInfos = null;
                    throw;
                }
            }
            return directoryPermissionInfos;
        }

        private void GetDirectories(
            DirectoryInfo directory,
            ICollection<DirectoryPermissionInfo> directories,
            int currentLevel)
        {
            try
            {
                if (_excludePaths.ContainsIgnoreCase(directory.FullName))
                {
                    Debug.WriteLine("Skipped " + directory.FullName);
                    return;
                }
                var directoryPermissionInfo = new DirectoryPermissionInfo(
                    directory);

                if (directoryPermissionInfo.FolderLevels > MaxFolderLevels)
                {
                    MaxFolderLevels = directoryPermissionInfo.FolderLevels;
                }

                directories.Add(directoryPermissionInfo);
            }
            catch (PathTooLongException)
            {
                _log.Add(
                    "The path of directory with name '" +
                    $"{directory.Name}' is too long.");
            }
            catch (UnauthorizedAccessException)
            {
                _log.Add(
                    "Could not get permissions for directory '" +
                    $"{directory.FullName}'");
            }

            if (_searchDepth == SearchDepth.Current)
            {
                return;
            }

            if (_searchDepth == SearchDepth.Children &&
                currentLevel > RootLevel)
            {
                return;
            }

            try
            {
                foreach (var subDirectory in directory.GetDirectories())
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    GetDirectories(
                        subDirectory, directories, currentLevel + 1);
                }
            }
            catch (PathTooLongException)
            {
                _log.Add(
                    "The path of directory with name '" +
                    $"{directory.Name}' is too long.");
            }
            catch (UnauthorizedAccessException e)
            {
                _log.Add(e.Message);
            }  
        }
    }
}