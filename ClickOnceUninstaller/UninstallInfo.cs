using System;
using System.Linq;
using Microsoft.Win32;
using System.Text;
using System.Security.Policy;

namespace Wunder.ClickOnceUninstaller
{
    public class UninstallInfo
    {
        public const string UninstallRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

        private UninstallInfo()
        {
        }

        public static UninstallInfo Find(string appName)
        {
            var uninstall = Registry.CurrentUser.OpenSubKey(UninstallRegistryPath);
            if (uninstall != null)
            {
                foreach (var app in uninstall.GetSubKeyNames())
                {
                    var sub = uninstall.OpenSubKey(app);
                    if (sub != null && sub.GetValue("DisplayName") as string == appName)
                    {
                        return new UninstallInfo
                                   {
                                       Key = app,
                                       UninstallString = sub.GetValue("UninstallString") as string,
                                       ShortcutFolderName = sub.GetValue("ShortcutFolderName") as string,
                                       ShortcutSuiteName = sub.GetValue("ShortcutSuiteName") as string,
                                       ShortcutFileName = sub.GetValue("ShortcutFileName") as string,
                                       SupportShortcutFileName = sub.GetValue("SupportShortcutFileName") as string,
                                       Version = sub.GetValue("DisplayVersion") as string
                                   };
                    }
                }
            }

            return null;
        }

        public static string GetPublicKeyTokenFromAppDomain()
        {
            StringBuilder pkt = new StringBuilder();

            ApplicationSecurityInfo asi = new ApplicationSecurityInfo(AppDomain.CurrentDomain.ActivationContext);
            byte[] pk = asi.ApplicationId.PublicKeyToken;

            for (int i = 0; i < pk.GetLength(0); i++)
                pkt.Append(String.Format("{0:x}", pk[i]));

            return pkt.ToString();
        }

        public static UninstallInfo FindByInstallerUrl(string urlName)
        {
            var publicKey = GetPublicKeyTokenFromAppDomain();

            var uninstall = Registry.CurrentUser.OpenSubKey(UninstallRegistryPath);
            if (uninstall != null)
            {
                foreach (var app in uninstall.GetSubKeyNames())
                {
                    var sub = uninstall.OpenSubKey(app);
                    if (sub != null && sub.GetValue("UrlUpdateInfo") as string == urlName)
                    {
                        string tmpUninstallString = sub.GetValue("UninstallString").ToString();

                        if (tmpUninstallString.Contains(publicKey) == true)
                        {
                            // found correct uninstall instance!
                            return new UninstallInfo
                            {
                                Key = app,
                                UninstallString = sub.GetValue("UninstallString") as string,
                                ShortcutFolderName = sub.GetValue("ShortcutFolderName") as string,
                                ShortcutSuiteName = sub.GetValue("ShortcutSuiteName") as string,
                                ShortcutFileName = sub.GetValue("ShortcutFileName") as string,
                                SupportShortcutFileName = sub.GetValue("SupportShortcutFileName") as string,
                                Version = sub.GetValue("DisplayVersion") as string
                            };
                        }
                    }
                }
            }

            return null;
        }

        public string Key { get; set; }

        public string UninstallString { get; private set; }

        public string ShortcutFolderName { get; set; }

        public string ShortcutSuiteName { get; set; }

        public string ShortcutFileName { get; set; }

        public string SupportShortcutFileName { get; set; }

        public string Version { get; set; }

        public string GetPublicKeyToken()
        {
            var token = UninstallString.Split(',').First(s => s.Trim().StartsWith("PublicKeyToken=")).Substring(16);
            if (token.Length != 16) throw new ArgumentException();
            return token;
        }
    }
}
