using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

    internal class PermissionGetter
    {
        private const int RootLevel = 0;
        private readonly CancellationToken _cancellationToken;
        private readonly IEnumerable<string> _excludePaths;
        private readonly bool _includeFiles;
        private readonly List<string> _log;
        private readonly List<DirectoryInfo> _rootDirectories;
        private readonly SearchDepth _searchDepth;

        private List<PermissionInfo> _permissionInfos;

        internal PermissionGetter(
            IEnumerable<string> searchPaths,
            IEnumerable<string> excludePaths,
            SearchDepth searchDepth,
            bool includeFiles,
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
            _includeFiles = includeFiles;
            _cancellationToken = cancellationToken;
            _log = log;
        }

        public int MaxPathLevels
        {
            get;
            private set;
        }

        internal List<PermissionInfo> GetPermissionInfos()
        {
            _permissionInfos = new List<PermissionInfo>();
            foreach (var rootDirectory in _rootDirectories)
            {
                try
                {
                    GetDirectories(rootDirectory, RootLevel);
                }
                catch (OperationCanceledException)
                {
                    _permissionInfos = null;
                    throw;
                }
            }
            return _permissionInfos;
        }

        private void GetDirectories(
            DirectoryInfo directory,
            int currentLevel)
        {
            try
            {
                if (!_excludePaths.IsNullOrEmpty() &&
                    _excludePaths.ContainsIgnoreCase(directory.FullName))
                {
                    return;
                }
                var permissionInfo = new PermissionInfo(
                    directory);

                if (permissionInfo.PathLevels > MaxPathLevels)
                {
                    MaxPathLevels = permissionInfo.PathLevels;
                }

                _permissionInfos.Add(permissionInfo);
                if (_includeFiles)
                {
                    foreach (var fileInfo in directory.GetFiles())
                    {
                        permissionInfo = new PermissionInfo(fileInfo);
                        if (permissionInfo.PathLevels > MaxPathLevels)
                        {
                            MaxPathLevels = permissionInfo.PathLevels;
                        }

                        _permissionInfos.Add(permissionInfo);
                    }
                }
            }
            catch (PathTooLongException)
            {
                Debug.Assert(
                    directory.Parent != null, "directory.Parent != null");
                _log.Add(
                    "The path of directory with name '" +
                    $"{directory.Parent.FullName}\\{directory.Name}' is too long.");
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
                    GetDirectories(subDirectory, currentLevel + 1);
                }
            }
            catch (PathTooLongException)
            {
                Debug.Assert(
                    directory.Parent != null, "directory.Parent != null");
                _log.Add(
                    "The path of directory with name '" +
                    $"{directory.Parent.FullName}\\{directory.Name}' is too long.");
            }
            catch (UnauthorizedAccessException e)
            {
                _log.Add(e.Message);
            }
        }
    }
}