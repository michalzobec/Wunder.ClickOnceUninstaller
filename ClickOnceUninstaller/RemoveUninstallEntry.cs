using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Wunder.ClickOnceUninstaller
{
    public class RemoveUninstallEntry : IUninstallStep
    {
        private const string CurrentUserPathPrefix = @"HKEY_CURRENT_USER\";
        private readonly UninstallInfo _uninstallInfo;
        private string _path;

        public RemoveUninstallEntry(UninstallInfo uninstallInfo)
        {
            _uninstallInfo = uninstallInfo;
        }

        public void Prepare(List<string> componentsToRemove)
        {
            if (_uninstallInfo?.KeyPath.StartsWith(CurrentUserPathPrefix, StringComparison.OrdinalIgnoreCase) == true)
            {
                _path = _uninstallInfo.KeyPath.Substring(CurrentUserPathPrefix.Length);
            }
        }

        public void PrintDebugInformation()
        {
            if (_path == null)
                throw new InvalidOperationException("Call Prepare() first.");

            Console.WriteLine("Remove uninstall info from " + Registry.CurrentUser.OpenSubKey(_path).Name);
            Console.WriteLine();
        }

        public void Execute()
        {
            if (_path == null)
                throw new InvalidOperationException("Call Prepare() first.");

            Registry.CurrentUser.DeleteSubKey(_path);
        }

        public void Dispose()
        {
            if (_path != null)
            {
                _path = null;
            }
        }
    }
}
