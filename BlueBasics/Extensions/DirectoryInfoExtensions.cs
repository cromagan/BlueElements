using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace BlueBasics
{
    public static partial class Extensions
    {

        public static bool IsReadable(this DirectoryInfo di)
        {
            AuthorizationRuleCollection rules;
            WindowsIdentity identity;
            try
            {
                rules = di.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
                identity = WindowsIdentity.GetCurrent();
            }
            catch (UnauthorizedAccessException)
            {
                //Develop.DebugPrint(uae);
                return false;
            }

            var isAllow = false;
            var userSID = identity.User.Value;

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.IdentityReference.ToString() == userSID || identity.Groups.Contains(rule.IdentityReference))
                {
                    if ((rule.FileSystemRights.HasFlag(FileSystemRights.Read) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadAttributes) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadData)) && rule.AccessControlType == AccessControlType.Deny)
                    { return false; }
                    else if ((rule.FileSystemRights.HasFlag(FileSystemRights.Read) &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadAttributes) &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadData)) && rule.AccessControlType == AccessControlType.Allow)
                    { isAllow = true; }

                }
            }
            return isAllow;
        }


        public static bool IsWriteable(this DirectoryInfo me)
        {
            AuthorizationRuleCollection rules;
            WindowsIdentity identity;
            try
            {
                rules = me.GetAccessControl().GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
                identity = WindowsIdentity.GetCurrent();
            }
            catch (UnauthorizedAccessException uae)
            {
                Develop.DebugPrint(uae);
                //Debug.WriteLine(uae.ToString());
                return false;
            }

            var isAllow = false;
            var userSID = identity.User.Value;

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.IdentityReference.ToString() == userSID || identity.Groups.Contains(rule.IdentityReference))
                {
                    if ((rule.FileSystemRights.HasFlag(FileSystemRights.Write) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteAttributes) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteData) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.CreateDirectories) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.CreateFiles)) && rule.AccessControlType == AccessControlType.Deny)
                    { return false; }
                    else if ((rule.FileSystemRights.HasFlag(FileSystemRights.Write) &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteAttributes) &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteData) &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.CreateDirectories) &&
                        rule.FileSystemRights.HasFlag(FileSystemRights.CreateFiles)) && rule.AccessControlType == AccessControlType.Allow)
                    { isAllow = true; }

                }
            }
            return isAllow;
        }
    }
}