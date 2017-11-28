using System;
using System.Linq;
using Microsoft.Win32;
using System.Text;
using System.Security.Policy;
using System.Collections.Generic;

namespace Wunder.ClickOnceUninstaller
{
    public class UninstallInfo
    {
        public const string UninstallRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
        public const string DisplayIconKey = "DisplayIcon";
        public const string DisplayNameKey = "DisplayName";
        public const string DisplayVersionKey = "DisplayVersion";
        public const string PublisherKey = "Publisher";
        public const string ShortcutAppIdKey = "ShortcutAppId";
        public const string ShortcutFileNameKey = "ShortcutFileName";
        public const string ShortcutFolderNameKey = "ShortcutFolderName";
        public const string ShortcutSuiteNameKey = "ShortcutSuiteName";
        public const string SupportShortcutFileNameKey = "SupportShortcutFileName";
        public const string UninstallStringKey = "UninstallString";
        public const string UrlUpdateInfoKey = "UrlUpdateInfo";

        private UninstallInfo(RegistryKey key)
        {
            KeyPath = key.Name;
            Icon = key.GetValue(DisplayIconKey) as string;
            Name = key.GetValue(DisplayNameKey) as string;
            Version = key.GetValue(DisplayVersionKey) as string;
            Publisher = key.GetValue(PublisherKey) as string;
            ShortcutAppId = key.GetValue(ShortcutAppIdKey) as string;
            ShortcutFileName = key.GetValue(ShortcutFileNameKey) as string;
            ShortcutFolderName = key.GetValue(ShortcutFolderNameKey) as string;
            ShortcutSuiteName = key.GetValue(ShortcutSuiteNameKey) as string;
            SupportShortcutFileName = key.GetValue(SupportShortcutFileNameKey) as string;
            UninstallString = key.GetValue(UninstallStringKey) as string;
            UpdateUrl = key.GetValue(UrlUpdateInfoKey) as string;
        }

        public static IEnumerable<RegistryKey> FindAllRegKeys()
        {
            var reg = Registry.CurrentUser.OpenSubKey(UninstallRegistryPath);
            if (reg != null)
            {
                foreach (var appSubKeyName in reg.GetSubKeyNames())
                {
                    var appReg = reg.OpenSubKey(appSubKeyName);
                    if (appReg != null)
                    {
                        var uninstallString = appReg.GetValue(UninstallStringKey) as string;
                        if (uninstallString.StartsWith("rundll32.exe dfshim.dll,ShArpMaintain", StringComparison.OrdinalIgnoreCase))
                        {
                            yield return appReg;
                        }
                    }
                }
            }
        }

        public static IEnumerable<UninstallInfo> FindAll()
        {
            return FindAllRegKeys().Select(reg => new UninstallInfo(reg));
        }

        public static UninstallInfo FindByKeyValue(string key, string value, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            foreach (var reg in FindAllRegKeys())
            {
                if (reg.GetValue(key) is string keyValue && keyValue.Equals(value, comparison))
                {
                    return new UninstallInfo(reg);
                }
            }
            return null;
        }

        public static UninstallInfo FindByName(string appName)
        {
            return FindByKeyValue(DisplayNameKey, appName);
        }

        public static UninstallInfo FindByUpdateUrl(string updateUrl)
        {
            return FindByKeyValue(UrlUpdateInfoKey, updateUrl);
        }

        public static string GetPublicKeyTokenFromAppDomain()
        {
            var pkt = new StringBuilder();
            var asi = new ApplicationSecurityInfo(AppDomain.CurrentDomain.ActivationContext);
            byte[] pk = asi.ApplicationId.PublicKeyToken;
            for (int i = 0; i < pk.GetLength(0); i++)
                pkt.AppendFormat("{0:x}", pk[i]);

            return pkt.ToString();
        }

        public string KeyPath { get; }
        public string Icon { get; }
        public string Name { get; }
        public string Version { get; }
        public string Publisher { get; }
        public string ShortcutAppId { get; }
        public string ShortcutFileName { get; }
        public string ShortcutFolderName { get; }
        public string ShortcutSuiteName { get; }
        public string SupportShortcutFileName { get; }
        public string UninstallString { get; }
        public string UpdateUrl { get; }

        public string GetPublicKeyToken()
        {
            const string keyName = "PublicKeyToken=";
            var result = UninstallString.Split(',')
                .Select(s => s.Trim())
                .First(s => s.StartsWith(keyName))
                .Substring(keyName.Length);
            if (result.Length != 16) throw new FormatException($"Public Key Token not found in uninstall string {UninstallString}");
            return result;
        }

        public string GetApplicationName()
        {
            const string keyName = "ShArpMaintain ";
            var result = UninstallString.Split(',')
                .Select(s => s.Trim())
                .First(s => s.StartsWith(keyName))
                .Substring(keyName.Length);
            if (string.IsNullOrEmpty(result)) throw new FormatException($"Application name not found in uninstall string {UninstallString}");
            return result;
        }

        public string GetApplicationNameAbbreviation()
        {
            var appName = GetApplicationName();
            if (appName.Length <= 10) return appName;
            return appName.Substring(0, 4) + ".." + appName.Substring(appName.Length - 4);
        }
    }
}
