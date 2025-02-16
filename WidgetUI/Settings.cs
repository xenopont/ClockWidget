using System.Text.Json;
using System.IO;

namespace WidgetUI
{
    public record SettingsRecord
    {
        public bool AlwaysOnTop;
        public double Top;
        public double Left;
    }

    public class ApplicationSettings
    {
        public SettingsRecord Values { get; private set; } = new();

        public void Load(string filename)
        {
            if (!File.Exists(filename))
            {
                Values = new SettingsRecord();
                return;
            }
            var bytes = File.ReadAllBytes(filename);
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

        private static string _appFileName = "";
        private static string _appBasePath = "";
        private static string ApplicationFileName
        {
            get
            {
                if (_appFileName == "")
                {
                    _appFileName = Environment.ProcessPath ?? "";
                }

                return _appFileName;
            }
        }
        private static string ApplicationBasePath { 
            get {
                if (_appBasePath == "") {
                    _appBasePath = Path.GetDirectoryName(ApplicationFileName) ?? Directory.GetCurrentDirectory();
                }

                return _appBasePath;
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
            return ApplicationFileName != "" &&
                   Registry.SetValue(AutostartRegistryKeyName, ClockWidgetValueName, ApplicationFileName);
        }

        public static bool DeleteAutostart()
        {
            return Registry.DeleteValue(AutostartRegistryKeyName, ClockWidgetValueName);
        }
    }
}
