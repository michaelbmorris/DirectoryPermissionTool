using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Extensions.CollectionExtensions;

namespace MichaelBrandonMorris.DirectoryPermissionTool
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

        internal PermissionGetter(
            IEnumerable<string> searchPaths,
            IEnumerable<string> excludePaths,
            SearchDepth searchDepth,
            bool shouldIncludeFiles,
            CancellationToken cancellationToken,
            IList<string> log)
        {
            RootDirectories = new HashSet<DirectoryInfo>();
            foreach (var searchPath in searchPaths)
            {
                if (!Directory.Exists(searchPath))
                {
                    throw new DirectoryNotFoundException(
                        $"Could not find directory '{searchPath}'");
                }

                RootDirectories.Add(new DirectoryInfo(searchPath));
            }

            if (searchDepth == SearchDepth.None)
            {
                throw new ArgumentException("Search Depth must not be null.");
            }

            ExcludePaths = excludePaths;
            SearchDepth = searchDepth;
            ShouldIncludeFiles = shouldIncludeFiles;
            CancellationToken = cancellationToken;
            Log = log;
        }

        public int MaxPathLevels
        {
            get;
            private set;
        }

        private CancellationToken CancellationToken
        {
            get;
        }

        private IEnumerable<string> ExcludePaths
        {
            get;
        }

        private IList<string> Log
        {
            get;
        }

        private HashSet<PermissionInfo> PermissionInfos
        {
            get;
            set;
        }

        private HashSet<DirectoryInfo> RootDirectories
        {
            get;
        }

        private SearchDepth SearchDepth
        {
            get;
        }

        private bool ShouldIncludeFiles
        {
            get;
        }

        internal HashSet<PermissionInfo> GetPermissionInfos()
        {
            PermissionInfos = new HashSet<PermissionInfo>();
            foreach (var rootDirectory in RootDirectories)
            {
                try
                {
                    GetDirectories(rootDirectory, RootLevel);
                }
                catch (OperationCanceledException)
                {
                    PermissionInfos = null;
                    throw;
                }
            }
            return PermissionInfos;
        }

        private void GetDirectories(
            DirectoryInfo directory,
            int currentLevel)
        {
            try
            {
                if (!ExcludePaths.IsNullOrEmpty() &&
                    ExcludePaths.ContainsIgnoreCase(directory.FullName))
                {
                    return;
                }

                var permissionInfo = new PermissionInfo(
                    directory);

                if (permissionInfo.PathLevels > MaxPathLevels)
                {
                    MaxPathLevels = permissionInfo.PathLevels;
                }

                PermissionInfos.Add(permissionInfo);

                if (ShouldIncludeFiles)
                {
                    foreach (var fileInfo in directory.GetFiles())
                    {
                        permissionInfo = new PermissionInfo(fileInfo);

                        if (permissionInfo.PathLevels > MaxPathLevels)
                        {
                            MaxPathLevels = permissionInfo.PathLevels;
                        }

                        PermissionInfos.Add(permissionInfo);
                    }
                }
            }
            catch (PathTooLongException)
            {
                Debug.Assert(
                    directory.Parent != null, "directory.Parent != null");

                Log.Add(
                    "The path of directory with name '" +
                    $"{directory.Parent.FullName}\\" +
                    $"{directory.Name}' is too long.");
            }
            catch (UnauthorizedAccessException)
            {
                Log.Add(
                    "Could not get permissions for directory '" +
                    $"{directory.FullName}'");
            }

            if (SearchDepth == SearchDepth.Current ||
                (SearchDepth == SearchDepth.Children &&
                 currentLevel > RootLevel))
            {
                return;
            }

            try
            {
                foreach (var subDirectory in directory.GetDirectories())
                {
                    CancellationToken.ThrowIfCancellationRequested();
                    GetDirectories(subDirectory, currentLevel + 1);
                }
            }
            catch (PathTooLongException)
            {
                Debug.Assert(
                    directory.Parent != null, "directory.Parent != null");

                Log.Add(
                    "The path of directory with name '" +
                    $"{directory.Parent.FullName}\\" +
                    $"{directory.Name}' is too long.");
            }
            catch (UnauthorizedAccessException e)
            {
                Log.Add(e.Message);
            }
        }
    }
}