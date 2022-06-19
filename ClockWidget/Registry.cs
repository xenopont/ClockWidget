using System.Reflection;

namespace ClockWidget
{
    internal class Registry
    {
        public static string ReadString(string keyName, string valueName, string defaultValue)
        {
            try { 
                Microsoft.Win32.RegistryKey? key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyName, false);
                object? value = key?.GetValue(valueName);
                if (value != null)
                {
                    return value.ToString() ?? defaultValue;
                }
            }
            catch { }

            return defaultValue;
        }

        public static bool SetValue(string keyName, string valueName, object value)
        {
            try
            {
                Microsoft.Win32.RegistryKey? key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyName, true);
                key?.SetValue(valueName, value);
                return true;
            }
            catch {
                return false;
            }
        }

        public static bool DeleteValue(string keyName, string valueName)
        {
            try
            {
                Microsoft.Win32.RegistryKey? key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyName, true);
                key?.DeleteValue(valueName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
