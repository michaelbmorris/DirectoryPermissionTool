using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using MichaelBrandonMorris.Extensions.PrimitiveExtensions;

namespace MichaelBrandonMorris.DirectoryPermissionTool
{
    internal class PermissionInfo : IEquatable<PermissionInfo>
    {
        internal PermissionInfo(DirectoryInfo directoryInfo)
        {
            FullNameSplitPath = directoryInfo.FullName.Split(
                    Path.DirectorySeparatorChar)
                .Where(
                    s => !s.IsNullOrWhiteSpace())
                .ToArray();

            FullName = directoryInfo.FullName;

            AccessRules = directoryInfo.GetAccessControl()
                .GetAccessRules(
                    true,
                    true,
                    typeof(NTAccount));
            try
            {
                Owner = directoryInfo.GetAccessControl()
                    .GetOwner(
                        typeof(NTAccount))
                    .ToString();
            }
            catch (IdentityNotMappedException)
            {
                Owner = string.Empty;
            }
        }

        internal PermissionInfo(FileInfo fileInfo)
        {
            FullNameSplitPath = fileInfo.FullName.Split(
                    Path.DirectorySeparatorChar)
                .Where(
                    s => !s.IsNullOrWhiteSpace())
                .ToArray();

            FullName = fileInfo.FullName;

            AccessRules = fileInfo.GetAccessControl()
                .GetAccessRules(
                    true,
                    true,
                    typeof(NTAccount));

            try
            {
                Owner = fileInfo.GetAccessControl()
                    .GetOwner(
                        typeof(NTAccount))
                    .ToString();
            }
            catch (IdentityNotMappedException)
            {
                Owner = string.Empty;
            }
        }

        internal AuthorizationRuleCollection AccessRules
        {
            get;
        }

        internal string FullName
        {
            get;
        }

        internal string[] FullNameSplitPath
        {
            get;
        }

        internal string Owner
        {
            get;
        }

        internal int PathLevels
        {
            get
            {
                return FullNameSplitPath.Length;
            }
        }

        public bool Equals(PermissionInfo other)
        {
            return other != null
                   && FullName.EqualsOrdinalIgnoreCase(other.FullName);
        }
    }
}