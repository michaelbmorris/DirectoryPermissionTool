using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using MichaelBrandonMorris.Extensions.PrimitiveExtensions;

namespace MichaelBrandonMorris.DirectoryPermissionTool
{
    /// <summary>
    ///     Class PermissionInfo.
    /// </summary>
    /// <seealso
    ///     cref="System.IEquatable{MichaelBrandonMorris.DirectoryPermissionTool.PermissionInfo}" />
    /// TODO Edit XML Comment Template for PermissionInfo
    internal class PermissionInfo : IEquatable<PermissionInfo>
    {
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="PermissionInfo" /> class.
        /// </summary>
        /// <param name="directoryInfo">The directory information.</param>
        /// TODO Edit XML Comment Template for #ctor
        internal PermissionInfo(DirectoryInfo directoryInfo)
        {
            FullNameSplitPath = directoryInfo.FullName
                .Split(Path.DirectorySeparatorChar)
                .Where(s => !s.IsNullOrWhiteSpace())
                .ToArray();

            FullName = directoryInfo.FullName;

            AccessRules = directoryInfo.GetAccessControl()
                .GetAccessRules(true, true, typeof(NTAccount));
            try
            {
                Owner = directoryInfo.GetAccessControl()
                    .GetOwner(typeof(NTAccount))
                    .ToString();
            }
            catch (IdentityNotMappedException)
            {
                Owner = string.Empty;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="PermissionInfo" /> class.
        /// </summary>
        /// <param name="fileInfo">The file information.</param>
        /// TODO Edit XML Comment Template for #ctor
        internal PermissionInfo(FileInfo fileInfo)
        {
            FullNameSplitPath = fileInfo.FullName
                .Split(Path.DirectorySeparatorChar)
                .Where(s => !s.IsNullOrWhiteSpace())
                .ToArray();

            FullName = fileInfo.FullName;

            AccessRules = fileInfo.GetAccessControl()
                .GetAccessRules(true, true, typeof(NTAccount));

            try
            {
                Owner = fileInfo.GetAccessControl()
                    .GetOwner(typeof(NTAccount))
                    .ToString();
            }
            catch (IdentityNotMappedException)
            {
                Owner = string.Empty;
            }
        }

        /// <summary>
        ///     Gets the access rules.
        /// </summary>
        /// <value>The access rules.</value>
        /// TODO Edit XML Comment Template for AccessRules
        internal AuthorizationRuleCollection AccessRules
        {
            get;
        }

        /// <summary>
        ///     Gets the full name.
        /// </summary>
        /// <value>The full name.</value>
        /// TODO Edit XML Comment Template for FullName
        internal string FullName
        {
            get;
        }

        /// <summary>
        ///     Gets the full name split path.
        /// </summary>
        /// <value>The full name split path.</value>
        /// TODO Edit XML Comment Template for FullNameSplitPath
        internal string[] FullNameSplitPath
        {
            get;
        }

        /// <summary>
        ///     Gets the owner.
        /// </summary>
        /// <value>The owner.</value>
        /// TODO Edit XML Comment Template for Owner
        internal string Owner
        {
            get;
        }

        /// <summary>
        ///     Gets the path levels.
        /// </summary>
        /// <value>The path levels.</value>
        /// TODO Edit XML Comment Template for PathLevels
        internal int PathLevels => FullNameSplitPath.Length;

        /// <summary>
        ///     Indicates whether the current object is equal to
        ///     another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///     true if the current object is equal to the
        ///     <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// TODO Edit XML Comment Template for Equals
        public bool Equals(PermissionInfo other)
        {
            return other != null
                   && FullName.EqualsOrdinalIgnoreCase(other.FullName);
        }
    }
}