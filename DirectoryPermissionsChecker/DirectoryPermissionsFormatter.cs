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
    internal class DirectoryPermissionsFormatter
    {
        private const char Comma = ',';
        private const char Quote = '"';

        private readonly CancellationToken _cancellationToken;
        private IEnumerable<DirectoryPermissionInfo> _directoryPermissionInfos;
        private readonly int _maxFolderLevels;

        internal DirectoryPermissionsFormatter(
            IEnumerable<DirectoryPermissionInfo> directoryPermissionInfos,
            int maxFolderLevels,
            CancellationToken cancellationToken)
        {
            _directoryPermissionInfos = directoryPermissionInfos;
            _maxFolderLevels = maxFolderLevels;
            _cancellationToken = cancellationToken;
        }

        internal string FormatDirectories()
        {
            var stringBuilder = new StringBuilder();

            for (var i = 0; i < _maxFolderLevels; i++)
            {
                stringBuilder.Append($"Level {i}".Wrap(Quote) + Comma);
            }

            stringBuilder.AppendLine(
                string.Join(
                    Comma.ToString(),
                    "Identity".Wrap(Quote),
                    "File System Rights".Wrap(Quote),
                    "Access Control Type".Wrap(Quote),
                    "Is Inherited?".Wrap(Quote)));
            try
            {
                foreach (var directoryPermissionInfo in _directoryPermissionInfos)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    var pathStringBuilder = new StringBuilder();
                    for (var i = 0; i < _maxFolderLevels; i++)
                    {
                        _cancellationToken.ThrowIfCancellationRequested();
                        if (i < directoryPermissionInfo.FullNameSplitPath.Length)
                        {
                            pathStringBuilder.Append(
                                directoryPermissionInfo.FullNameSplitPath[i].Wrap(
                                    Quote) + Comma);
                        }
                        else
                        {
                            pathStringBuilder.Append(
                                string.Empty.Wrap(Quote) + Comma);
                        }
                    }

                    foreach (FileSystemAccessRule accessRule in
                        directoryPermissionInfo.AccessRules)
                    {
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
                    }
                }
            }
            catch (OperationCanceledException)
            {
                stringBuilder = null;
                _directoryPermissionInfos = null;
                throw;
            }
            

            return stringBuilder.ToString();
        }
    }

    internal struct DirectoryPermissionInfo
    {
        internal string[] FullNameSplitPath { get; }
        internal AuthorizationRuleCollection AccessRules { get; }

        internal int FolderLevels => FullNameSplitPath.Length;


        internal DirectoryPermissionInfo(DirectoryInfo directoryInfo)
        {
            FullNameSplitPath = directoryInfo.FullName.Split(
                Path.DirectorySeparatorChar).Where(
                    s => !s.IsNullOrWhiteSpace()).ToArray();

            AccessRules =
                directoryInfo.GetAccessControl()
                    .GetAccessRules(true, true, typeof(NTAccount));
        }
    }
}