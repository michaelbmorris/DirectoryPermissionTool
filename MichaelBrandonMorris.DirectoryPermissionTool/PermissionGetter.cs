using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using MichaelBrandonMorris.Extensions.CollectionExtensions;

namespace MichaelBrandonMorris.DirectoryPermissionTool
{
    /// <summary>
    ///     Enum SearchDepth
    /// </summary>
    /// TODO Edit XML Comment Template for SearchDepth
    internal enum SearchDepth
    {
        /// <summary>
        ///     All
        /// </summary>
        /// TODO Edit XML Comment Template for All
        All,

        /// <summary>
        ///     The children
        /// </summary>
        /// TODO Edit XML Comment Template for Children
        Children,

        /// <summary>
        ///     The current
        /// </summary>
        /// TODO Edit XML Comment Template for Current
        Current,

        /// <summary>
        ///     The none
        /// </summary>
        /// TODO Edit XML Comment Template for None
        None
    }

    /// <summary>
    ///     Class PermissionGetter.
    /// </summary>
    /// TODO Edit XML Comment Template for PermissionGetter
    internal class PermissionGetter
    {
        /// <summary>
        ///     The root level
        /// </summary>
        /// TODO Edit XML Comment Template for RootLevel
        private const int RootLevel = 0;

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="PermissionGetter" /> class.
        /// </summary>
        /// <param name="searchPaths">The search paths.</param>
        /// <param name="excludePaths">The exclude paths.</param>
        /// <param name="searchDepth">The search depth.</param>
        /// <param name="shouldIncludeFiles">
        ///     if set to <c>true</c>
        ///     [should include files].
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="log">The log.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentException">
        ///     Search Depth must not
        ///     be null.
        /// </exception>
        /// TODO Edit XML Comment Template for #ctor
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

        /// <summary>
        ///     Gets the maximum path levels.
        /// </summary>
        /// <value>The maximum path levels.</value>
        /// TODO Edit XML Comment Template for MaxPathLevels
        public int MaxPathLevels
        {
            get;
            private set;
        }

        /// <summary>
        ///     Gets the cancellation token.
        /// </summary>
        /// <value>The cancellation token.</value>
        /// TODO Edit XML Comment Template for CancellationToken
        private CancellationToken CancellationToken
        {
            get;
        }

        /// <summary>
        ///     Gets the exclude paths.
        /// </summary>
        /// <value>The exclude paths.</value>
        /// TODO Edit XML Comment Template for ExcludePaths
        private IEnumerable<string> ExcludePaths
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
        ///     Gets the root directories.
        /// </summary>
        /// <value>The root directories.</value>
        /// TODO Edit XML Comment Template for RootDirectories
        private HashSet<DirectoryInfo> RootDirectories
        {
            get;
        }

        /// <summary>
        ///     Gets the search depth.
        /// </summary>
        /// <value>The search depth.</value>
        /// TODO Edit XML Comment Template for SearchDepth
        private SearchDepth SearchDepth
        {
            get;
        }

        /// <summary>
        ///     Gets a value indicating whether [should include files].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [should include files]; otherwise,
        ///     <c>false</c>.
        /// </value>
        /// TODO Edit XML Comment Template for ShouldIncludeFiles
        private bool ShouldIncludeFiles
        {
            get;
        }

        /// <summary>
        ///     Gets or sets the permission infos.
        /// </summary>
        /// <value>The permission infos.</value>
        /// TODO Edit XML Comment Template for PermissionInfos
        private HashSet<PermissionInfo> PermissionInfos
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets the permission infos.
        /// </summary>
        /// <returns>HashSet&lt;PermissionInfo&gt;.</returns>
        /// TODO Edit XML Comment Template for GetPermissionInfos
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

        /// <summary>
        ///     Gets the directories.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="currentLevel">The current level.</param>
        /// TODO Edit XML Comment Template for GetDirectories
        private void GetDirectories(DirectoryInfo directory, int currentLevel)
        {
            try
            {
                if (!ExcludePaths.IsNullOrEmpty()
                    && ExcludePaths.ContainsIgnoreCase(directory.FullName))
                {
                    return;
                }

                var permissionInfo = new PermissionInfo(directory);

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
                    directory.Parent != null,
                    "directory.Parent != null");

                Log.Add(
                    "The path of directory with name '"
                    + $"{directory.Parent.FullName}\\"
                    + $"{directory.Name}' is too long.");
            }
            catch (UnauthorizedAccessException)
            {
                Log.Add(
                    "Could not get permissions for directory '"
                    + $"{directory.FullName}'");
            }

            if (SearchDepth == SearchDepth.Current
                || SearchDepth == SearchDepth.Children
                && currentLevel > RootLevel)
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
                    directory.Parent != null,
                    "directory.Parent != null");

                Log.Add(
                    "The path of directory with name '"
                    + $"{directory.Parent.FullName}\\"
                    + $"{directory.Name}' is too long.");
            }
            catch (UnauthorizedAccessException e)
            {
                Log.Add(e.Message);
            }
        }
    }
}