using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Extensions.PrimitiveExtensions;

namespace DirectoryPermissionTool
{
    internal class PermissionInfoFormatter
    {
        private const char Comma = ',';
        private const char Quote = '"';
        private const int MaxResults = 1048576;
        private const string IdentityReferenceHeader = "Identity";
        private const string FileSystemRightsHeader = "File System Rights";
        private const string AccessControlTypeHeader = "Access Control Type";
        private const string IsInheritedHeader = "Is Inherited?";

        private readonly CancellationToken _cancellationToken;
        private IEnumerable<PermissionInfo> _permissionInfos;
        private readonly int _maxFolderLevels;

        internal PermissionInfoFormatter(
            IEnumerable<PermissionInfo> permissionInfos,
            int maxFolderLevels,
            CancellationToken cancellationToken)
        {
            _permissionInfos = permissionInfos;
            _maxFolderLevels = maxFolderLevels;
            _cancellationToken = cancellationToken;
        }

        internal string FormatDirectories()
        {
            var stringBuilder = new StringBuilder();
            var resultsCount = 0;

            for (var i = 0; i < _maxFolderLevels; i++)
            {
                stringBuilder.Append($"Level {i}".Wrap(Quote) + Comma);
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

                    foreach (FileSystemAccessRule accessRule in
                        permissionInfo.AccessRules)
                    {
                        if (resultsCount == MaxResults)
                        {
                            continue;
                        }

                        var identityReference = accessRule.IdentityReference.Value;

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

    internal struct PermissionInfo
    {
        internal string[] FullNameSplitPath { get; }
        internal AuthorizationRuleCollection AccessRules { get; }

        internal int PathLevels => FullNameSplitPath.Length;


        internal PermissionInfo(DirectoryInfo directoryInfo)
        {
            FullNameSplitPath = directoryInfo.FullName.Split(
                Path.DirectorySeparatorChar).Where(
                    s => !s.IsNullOrWhiteSpace()).ToArray();

            AccessRules = directoryInfo.GetAccessControl().GetAccessRules(
                true, true, typeof(NTAccount));
        }

        internal PermissionInfo(FileInfo fileInfo)
        {
            FullNameSplitPath = fileInfo.FullName.Split(
                Path.DirectorySeparatorChar).Where(
                    s => !s.IsNullOrWhiteSpace()).ToArray();

            AccessRules = fileInfo.GetAccessControl().GetAccessRules(
                true, true, typeof(NTAccount));
        }
    }
}