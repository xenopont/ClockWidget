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
            return;
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

        public static string FileName()
        {
            string appLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = Path.GetDirectoryName(appLocation) ?? Directory.GetCurrentDirectory();
            return path + @"\settings.json";
        }
    }
}
