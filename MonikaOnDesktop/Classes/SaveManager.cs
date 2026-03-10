using System.Drawing;
using System.IO;
using System.Text.Json;

namespace MonikaOnDesktop {
    public class AppSettings {
        public string Username { get; set; } = "__fistrun__";
        public string PCname { get; set; } = "PC";
        public string CurrentHair { get; set; } = "def";
        public string CurrentWear { get; set; } = "def";
        public string CurrentHat { get; set; } = "null";
        public string CurrentRibbon { get; set; } = "def";
        public string CurrentAhoge { get; set; } = "null";
        public List<string> Acs { get; set; } = new List<string>();

        public Dictionary<string, bool> Parsing { get; set; } = new Dictionary<string, bool>{
            {"Processes", false},
            {"Sites", true},
            {"Google", true},
            {"Youtube", true}
            };
        public string Lang { get; set; } = "en-US";
        public bool AutoUpdate { get; set; } = false;
        public bool DialogAutoUpdate { get; set; } = false;
        public bool AutoStart { get; set; } = false;
        public bool MouseHide { get; set; } = false;
        public bool Filter { get; set; } = true;
        public float Scale { get; set; } = 1;
        public float IdleRarity { get; set; } = 1;

        public bool AI { get; set; } = false;
        public string token { get; set; } = "null";
        public string port { get; set; } = "3000";
        public Point MonikaPosition { get; set; } = new Point();
        public bool IsAuto { get; set; } = true;


    }
    public class SaveManager {
        static string mainPath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string Path = System.IO.Path.Combine(mainPath, "characters", "monika.chr");

        public static void Save(AppSettings settings) {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path, json);
        }

        public static AppSettings Load() {
            if (!File.Exists(Path)) return new AppSettings();
            string json = File.ReadAllText(Path);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
    }
}
