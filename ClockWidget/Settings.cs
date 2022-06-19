using System.Text.Json;
using System.IO;

namespace ClockWidget
{
    public record SettingsRecord
    {
        public bool AlwaysOnTop = false;
    }

    public class ApplicationSettings
    {
        public SettingsRecord Values { get; set; } = new();

        public void Load(string filename)
        {
            if (!File.Exists(filename))
            {
                Values = new SettingsRecord();
                return;
            }
            byte[] bytes = File.ReadAllBytes(filename);
            JsonSerializerOptions options = new()
            {
                IncludeFields = true,
            };
            Values = JsonSerializer.Deserialize<SettingsRecord>(bytes, options) ?? new SettingsRecord();
        }

        public void Save(string filename)
        {
            JsonSerializerOptions options = new()
            {
                IncludeFields = true,
                WriteIndented = true,
            };
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes<SettingsRecord>(Values, options);
            File.WriteAllBytes(filename, bytes);
        }

        private static string appFileName = "";
        private static string appBasePath = "";
        private static string ApplicationFileName
        {
            get
            {
                if (appFileName == "")
                {
                    appFileName = System.Environment.ProcessPath ?? "";
                }

                return appFileName;
            }
        }
        private static string ApplicationBasePath { 
            get {
                if (appBasePath == "") {
                    appBasePath = Path.GetDirectoryName(ApplicationFileName) ?? Directory.GetCurrentDirectory();
                }

                return appBasePath;
            }
        }

        public static string FileName()
        {
            return ApplicationBasePath + @"\settings.json";
        }

        private static readonly string AutostartRegistryKeyName = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private static readonly string ClockWidgetValueName = "ClockWidget";

        public static bool IsAutostart()
        {
            return Registry.ReadString(AutostartRegistryKeyName, ClockWidgetValueName, "") == ApplicationFileName;
        }

        public static bool SetAutostart()
        {
            if (ApplicationFileName == "")
            {
                return false;
            }
            return Registry.SetValue(AutostartRegistryKeyName, ClockWidgetValueName, ApplicationFileName);
        }

        public static bool DeleteAutostart()
        {
            return Registry.DeleteValue(AutostartRegistryKeyName, ClockWidgetValueName);
        }
    }
}
