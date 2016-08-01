using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Extensions.PrimitiveExtensions;

namespace DirectoryPermissionTool
{
    internal class PermissionInfo
    {
        internal string[] FullNameSplitPath
        {
            get;
        }

        internal string FullName
        {
            get;
        }

        internal AuthorizationRuleCollection AccessRules
        {
            get;
        }

        internal int PathLevels => FullNameSplitPath.Length;

        internal PermissionInfo(DirectoryInfo directoryInfo)
        {
            FullNameSplitPath = directoryInfo.FullName.Split(
                Path.DirectorySeparatorChar).Where(
                    s => !s.IsNullOrWhiteSpace()).ToArray();

            FullName = directoryInfo.FullName;

            AccessRules = directoryInfo.GetAccessControl().GetAccessRules(
                true, true, typeof(NTAccount));
        }

        internal PermissionInfo(FileInfo fileInfo)
        {
            FullNameSplitPath = fileInfo.FullName.Split(
                Path.DirectorySeparatorChar).Where(
                    s => !s.IsNullOrWhiteSpace()).ToArray();

            FullName = fileInfo.FullName;

            AccessRules = fileInfo.GetAccessControl().GetAccessRules(
                true, true, typeof(NTAccount));
        }
    }
}
