using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using MichaelBrandonMorris.Extensions.CollectionExtensions;
using MichaelBrandonMorris.Extensions.PrimitiveExtensions;

namespace MichaelBrandonMorris.DirectoryPermissionTool
{
    internal class PermissionInfoFormatter
    {
        private const string AccessControlTypeHeader = "Access Control Type";
        private const char Comma = ',';
        private const string FileSystemRightsHeader = "File System Rights";
        private const string IdentityReferenceHeader = "Identity";
        private const string IsInheritedHeader = "Is Inherited?";
        private const string LevelHeader = "Level";
        private const int MaxResults = 1048574;
        private const string OwnerHeader = "Owner";
        private const string PathHeader = "Path";
        private const char Quote = '"';
        private const string ResultNumberHeader = "#";

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

        private CancellationToken CancellationToken
        {
            get;
        }

        private IEnumerable<string> ExcludedGroups
        {
            get;
        }

        private int MaxPathLevels
        {
            get;
        }

        private bool ShouldSplitPathLevels
        {
            get;
        }

        private IEnumerable<PermissionInfo> PermissionInfos
        {
            get;
            set;
        }

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
                                        .Wrap(
                                            Quote)
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


                    foreach (FileSystemAccessRule accessRule in
                        permissionInfo.AccessRules)
                    {
                        if (resultsCount == MaxResults)
                        {
                            continue;
                        }

                        var identityReference =
                            accessRule.IdentityReference.Value;

                        if (ExcludedGroups.ContainsIgnoreCase(
                            identityReference))
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