using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using MichaelBrandonMorris.Extensions.CollectionExtensions;
using MichaelBrandonMorris.Extensions.PrimitiveExtensions;

namespace MichaelBrandonMorris.DirectoryPermissionTool
{
    /// <summary>
    ///     Class PermissionInfoFormatter.
    /// </summary>
    /// TODO Edit XML Comment Template for PermissionInfoFormatter
    internal class PermissionInfoFormatter
    {
        /// <summary>
        ///     The access control type header
        /// </summary>
        /// TODO Edit XML Comment Template for AccessControlTypeHeader
        private const string AccessControlTypeHeader = "Access Control Type";

        /// <summary>
        ///     The comma
        /// </summary>
        /// TODO Edit XML Comment Template for Comma
        private const char Comma = ',';

        /// <summary>
        ///     The file system rights header
        /// </summary>
        /// TODO Edit XML Comment Template for FileSystemRightsHeader
        private const string FileSystemRightsHeader = "File System Rights";

        /// <summary>
        ///     The identity reference header
        /// </summary>
        /// TODO Edit XML Comment Template for IdentityReferenceHeader
        private const string IdentityReferenceHeader = "Identity";

        /// <summary>
        ///     The is inherited header
        /// </summary>
        /// TODO Edit XML Comment Template for IsInheritedHeader
        private const string IsInheritedHeader = "Is Inherited?";

        /// <summary>
        ///     The level header
        /// </summary>
        /// TODO Edit XML Comment Template for LevelHeader
        private const string LevelHeader = "Level";

        /// <summary>
        ///     The maximum results
        /// </summary>
        /// TODO Edit XML Comment Template for MaxResults
        private const int MaxResults = 1048574;

        /// <summary>
        ///     The owner header
        /// </summary>
        /// TODO Edit XML Comment Template for OwnerHeader
        private const string OwnerHeader = "Owner";

        /// <summary>
        ///     The path header
        /// </summary>
        /// TODO Edit XML Comment Template for PathHeader
        private const string PathHeader = "Path";

        /// <summary>
        ///     The quote
        /// </summary>
        /// TODO Edit XML Comment Template for Quote
        private const char Quote = '"';

        /// <summary>
        ///     The result number header
        /// </summary>
        /// TODO Edit XML Comment Template for ResultNumberHeader
        private const string ResultNumberHeader = "#";

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="PermissionInfoFormatter" /> class.
        /// </summary>
        /// <param name="permissionInfos">The permission infos.</param>
        /// <param name="maxPathLevels">The maximum path levels.</param>
        /// <param name="shouldSplitPathLevels">
        ///     if set to <c>true</c>
        ///     [should split path levels].
        /// </param>
        /// <param name="excludedGroups">The excluded groups.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// TODO Edit XML Comment Template for #ctor
        internal PermissionInfoFormatter(
            IEnumerable<PermissionInfo> permissionInfos,
            int maxPathLevels,
            bool shouldSplitPathLevels,
            IEnumerable<string> excludedGroups,
            CancellationToken cancellationToken)
        {
            PermissionInfos = permissionInfos;
            MaxPathLevels = maxPathLevels;
            ShouldSplitPathLevels = shouldSplitPathLevels;
            ExcludedGroups = excludedGroups;
            CancellationToken = cancellationToken;
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
        ///     Gets the excluded groups.
        /// </summary>
        /// <value>The excluded groups.</value>
        /// TODO Edit XML Comment Template for ExcludedGroups
        private IEnumerable<string> ExcludedGroups
        {
            get;
        }

        /// <summary>
        ///     Gets the maximum path levels.
        /// </summary>
        /// <value>The maximum path levels.</value>
        /// TODO Edit XML Comment Template for MaxPathLevels
        private int MaxPathLevels
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
        ///     Gets or sets the permission infos.
        /// </summary>
        /// <value>The permission infos.</value>
        /// TODO Edit XML Comment Template for PermissionInfos
        private IEnumerable<PermissionInfo> PermissionInfos
        {
            get;
            set;
        }

        /// <summary>
        ///     Formats the directories.
        /// </summary>
        /// <returns>System.String.</returns>
        /// TODO Edit XML Comment Template for FormatDirectories
        internal string FormatDirectories()
        {
            var stringBuilder = new StringBuilder();
            var resultsCount = 0;

            stringBuilder.Append(ResultNumberHeader.Wrap(Quote) + Comma);
            if (ShouldSplitPathLevels)
            {
                for (var i = 0; i < MaxPathLevels; i++)
                {
                    stringBuilder.Append(
                        $"{LevelHeader} {i}".Wrap(Quote) + Comma);
                }
            }
            else
            {
                stringBuilder.Append(PathHeader.Wrap(Quote) + Comma);
            }

            stringBuilder.AppendLine(
                string.Join(
                    Comma.ToString(),
                    IdentityReferenceHeader.Wrap(Quote),
                    FileSystemRightsHeader.Wrap(Quote),
                    AccessControlTypeHeader.Wrap(Quote),
                    IsInheritedHeader.Wrap(Quote),
                    OwnerHeader.Wrap(Quote)));
            try
            {
                foreach (var permissionInfo in PermissionInfos)
                {
                    CancellationToken.ThrowIfCancellationRequested();

                    if (resultsCount == MaxResults)
                    {
                        continue;
                    }

                    var pathStringBuilder = new StringBuilder();

                    if (ShouldSplitPathLevels)
                    {
                        for (var i = 0; i < MaxPathLevels; i++)
                        {
                            CancellationToken.ThrowIfCancellationRequested();

                            if (i < permissionInfo.PathLevels)
                            {
                                pathStringBuilder.Append(
                                    permissionInfo.FullNameSplitPath[i]
                                        .Wrap(Quote)
                                    + Comma);
                            }
                            else
                            {
                                pathStringBuilder.Append(
                                    string.Empty.Wrap(Quote) + Comma);
                            }
                        }
                    }
                    else
                    {
                        pathStringBuilder.Append(
                            permissionInfo.FullName.Wrap(Quote) + Comma);
                    }


                    foreach (FileSystemAccessRule accessRule in permissionInfo
                        .AccessRules)
                    {
                        if (resultsCount == MaxResults)
                        {
                            continue;
                        }

                        var identityReference =
                            accessRule.IdentityReference.Value;

                        if (ExcludedGroups.ContainsIgnoreCase(identityReference)
                        )
                        {
                            continue;
                        }

                        var fileSystemRights =
                            accessRule.FileSystemRights.ToString();

                        var accessControlType =
                            accessRule.AccessControlType.ToString();

                        var isInherited = accessRule.IsInherited.ToString();
                        CancellationToken.ThrowIfCancellationRequested();
                        stringBuilder.Append(
                            resultsCount.ToString().Wrap(Quote) + Comma);
                        stringBuilder.Append(pathStringBuilder);

                        stringBuilder.AppendLine(
                            string.Join(
                                Comma.ToString(),
                                identityReference.Wrap(Quote),
                                fileSystemRights.Wrap(Quote),
                                accessControlType.Wrap(Quote),
                                isInherited.Wrap(Quote),
                                permissionInfo.Owner.Wrap(Quote)));

                        resultsCount++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // ReSharper disable once RedundantAssignment
                stringBuilder = null;
                PermissionInfos = null;
                throw;
            }

            return stringBuilder.ToString();
        }
    }
}