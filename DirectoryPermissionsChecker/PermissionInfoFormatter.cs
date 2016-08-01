using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using Extensions.PrimitiveExtensions;

namespace DirectoryPermissionTool
{
    internal class PermissionInfoFormatter
    {
        private const string AccessControlTypeHeader = "Access Control Type";
        private const char Comma = ',';
        private const string FileSystemRightsHeader = "File System Rights";
        private const string IdentityReferenceHeader = "Identity";
        private const string IsInheritedHeader = "Is Inherited?";
        private const string PathHeader = "Path";
        private const string LevelHeader = "Level";
        private const int MaxResults = 1048576;
        private const char Quote = '"';

        private readonly CancellationToken _cancellationToken;
        private readonly int _maxFolderLevels;
        private IEnumerable<PermissionInfo> _permissionInfos;

        private bool ShouldSplitPathLevels
        {
            get;
        }

        internal PermissionInfoFormatter(
            IEnumerable<PermissionInfo> permissionInfos,
            int maxFolderLevels,
            bool shouldSplitPathLevels,
            CancellationToken cancellationToken)
        {
            _permissionInfos = permissionInfos;
            _maxFolderLevels = maxFolderLevels;
            ShouldSplitPathLevels = shouldSplitPathLevels;
            _cancellationToken = cancellationToken;
        }

        internal string FormatDirectories()
        {
            var stringBuilder = new StringBuilder();
            var resultsCount = 0;

            if (ShouldSplitPathLevels)
            {
                for (var i = 0; i < _maxFolderLevels; i++)
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
                    IsInheritedHeader.Wrap(Quote)));
            try
            {
                foreach (var permissionInfo in _permissionInfos)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    if (resultsCount == MaxResults)
                    {
                        continue;
                    }

                    var pathStringBuilder = new StringBuilder();
                    if (ShouldSplitPathLevels)
                    {
                        for (var i = 0; i < _maxFolderLevels; i++)
                        {
                            _cancellationToken.ThrowIfCancellationRequested();
                            if (i < permissionInfo.FullNameSplitPath.Length)
                            {
                                pathStringBuilder.Append(
                                    permissionInfo.FullNameSplitPath[i].Wrap(
                                        Quote) + Comma);
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

                        var fileSystemRights =
                            accessRule.FileSystemRights.ToString();

                        var accessControlType =
                            accessRule.AccessControlType.ToString();

                        var isInherited = accessRule.IsInherited.ToString();
                        _cancellationToken.ThrowIfCancellationRequested();
                        stringBuilder.Append(pathStringBuilder);

                        stringBuilder.AppendLine(
                            string.Join(
                                Comma.ToString(),
                                identityReference.Wrap(Quote),
                                fileSystemRights.Wrap(Quote),
                                accessControlType.Wrap(Quote),
                                isInherited.Wrap(Quote)));

                        resultsCount++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                stringBuilder = null;
                _permissionInfos = null;
                throw;
            }

            return stringBuilder.ToString();
        }
    }
}