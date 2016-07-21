using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace DirectoryPermissionsChecker
{
    internal class Program
    {
        private const string DateTimeFormat = "yyyyMMddTHHmmss";
        private const string TxtExtension = ".txt";

        private static void Main()
        {
            var rootDirectory = new DirectoryInfo(
                @"\\kng1fsmf01\sharedfolders");

            var directories = rootDirectory.GetDirectories();
            var log = new List<string>();
            var results = new List<string>();

            var myDocuments = Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments);

            var resultsPath = Path.Combine(
                myDocuments,
                "DirectoryPermissionsResults" +
                DateTime.Now.ToString(DateTimeFormat) +
                TxtExtension);

            var logPath = Path.Combine(
                myDocuments,
                "DirectoryPermissionsLogs" +
                DateTime.Now.ToString(DateTimeFormat) +
                TxtExtension);

            foreach (var directory in directories)
            {
                try
                {
                    var accessControl = directory.GetAccessControl();

                    var accessRules = accessControl.GetAccessRules(
                        true, true, typeof(NTAccount));

                    results.Add(directory.Name);
                    foreach (FileSystemAccessRule accessRule in accessRules)
                    {
                        var result = string.Join(
                            " : ", accessRule.IdentityReference.Value,
                            accessRule.FileSystemRights.ToString(),
                            accessRule.AccessControlType.ToString());

                        results.Add(result);
                    }
                    results.Add("\n");
                }
                catch (Exception e)
                {
                    log.Add(e.Message + " " + directory.Name);
                }
            }
            File.WriteAllLines(resultsPath, results);
            File.WriteAllLines(logPath, log);
            Process.Start(resultsPath);
        }
    }
}