namespace ClockWidget
{
    internal static class Registry
    {
        public static string ReadString(string keyName, string valueName, string defaultValue)
        {
            try { 
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyName, false);
                var value = key?.GetValue(valueName);
                if (value != null)
                {
                    return value.ToString() ?? defaultValue;
                }
            }
            catch
            {
                // ignored
            }

            return defaultValue;
        }

        public static bool SetValue(string keyName, string valueName, object value)
        {
            try
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyName, true);
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
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyName, true);
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
