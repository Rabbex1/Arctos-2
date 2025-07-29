using System.Text.Json;
using System.Text.Json.Nodes;

namespace Arctos
{
    public class AppSettings
    {
        private readonly Dictionary<string, JsonNode?> _values = new();
        private readonly string _path;

        private static AppSettings? _current;
        public static AppSettings Current => _current ?? throw new InvalidOperationException("AppSettings not initialized.");

        public static void Initialize(string path)
        {
            _current = Load(path);
        }

        private AppSettings(string path, Dictionary<string, JsonNode?>? initial = null)
        {
            _path = path;
            if (initial != null)
                _values = initial;
        }

        public static AppSettings Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, JsonNode?>>(json);
                    return new AppSettings(path, dict ?? new());
                }
            }
            catch { }

            return new AppSettings(path);
        }

        public void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(_path)!;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(_values, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_path, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AppSettings save failed: {ex.Message}");
            }
        }

        public void Set<T>(string key, T value)
        {
            _values[key] = JsonSerializer.SerializeToNode(value);
        }

        public T? Get<T>(string key, T? fallback = default)
        {
            if (_values.TryGetValue(key, out var node) && node != null)
            {
                try
                {
                    return node.Deserialize<T>();
                }
                catch { }
            }
            return fallback;
        }

        public JsonNode? GetRaw(string key) => _values.TryGetValue(key, out var node) ? node : null;
    }
}
