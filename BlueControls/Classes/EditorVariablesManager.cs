// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Text;

namespace BlueControls.Classes;

public static class EditorVariablesManager {

    #region Fields

    private static readonly string _filename = $"%appdocumentpath%\\{Generic.UserName}_EditorVariables.json".NormalizeFile();
    private static readonly object _lock = new();
    private static readonly Dictionary<string, List<JsonEntry>> _sets = [];
    private static readonly Dictionary<string, bool> _settings = [];
    private static bool _initialized;

    #endregion

    #region Methods

    public static void DeleteSet(string storageKey, string setName) {
        lock (_lock) {
            if (_sets.TryGetValue(storageKey, out var list)) {
                list.RemoveAll(v => string.Equals(v.KeyName, setName, StringComparison.OrdinalIgnoreCase));
                if (list.Count == 0) { _sets.Remove(storageKey); }
            }
            Save();
        }
    }

    public static List<JsonEntry> GetSets(string storageKey) {
        lock (_lock) {
            InitializeIfNeeded();
            return _sets.TryGetValue(storageKey, out var list) ? list.ToList() : [];
        }
    }

    public static bool HasSet(string storageKey, string setName) {
        lock (_lock) {
            InitializeIfNeeded();
            if (!_sets.TryGetValue(storageKey, out var list)) { return false; }
            return list.Exists(v => string.Equals(v.KeyName, setName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static void InitializeIfNeeded() {
        lock (_lock) {
            if (_initialized) { return; }
            _initialized = true;

            if (IO.FileExists(_filename)) {
                var json = IO.ReadAllText(_filename, Encoding.UTF8);
                if (!string.IsNullOrEmpty(json)) {
                    ParseJson(json);
                }
            }
        }
    }

    public static void SaveSet(string storageKey, string setName, JsonNode? variableData) {
        lock (_lock) {
            InitializeIfNeeded();

            var element = variableData is { } ? JsonSerializer.SerializeToElement(variableData) : default;

            if (!_sets.TryGetValue(storageKey, out var list)) {
                list = [];
                _sets[storageKey] = list;
            }

            if (list.FirstOrDefault(v => string.Equals(v.KeyName, setName, StringComparison.OrdinalIgnoreCase)) is { } existing) {
                existing.JsonData = element;
                existing.Modified = DateTime.Now;
            } else {
                list.Add(new JsonEntry(setName, element));
            }

            Save();
        }
    }

    private static void ParseJson(string json) {
        try {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.IsObject()) { return; }

            var setsObj = root.GetJson("sets");
            if (setsObj is not null && setsObj.Value.IsObject()) {
                foreach (var storageEntry in setsObj.Value.EnumerateObject()) {
                    if (!storageEntry.Value.IsArray()) { continue; }

                    var setList = new List<JsonEntry>();
                    foreach (var setEl in storageEntry.Value.EnumerateArray()) {
                        var entry = JsonEntry.Parse(setEl);
                        if (entry is not null && !string.IsNullOrEmpty(entry.KeyName)) {
                            setList.Add(entry);
                        }
                    }
                    if (setList.Count > 0) {
                        _sets[storageEntry.Name] = setList;
                    }
                }
            }

            var settingsObj = root.GetJson("settings");
            if (settingsObj is not null && settingsObj.Value.IsObject()) {
                foreach (var settingEntry in settingsObj.Value.EnumerateObject()) {
                    if (settingEntry.Value.ValueKind is JsonValueKind.True or JsonValueKind.False) {
                        _settings[settingEntry.Name] = settingEntry.Value.GetBoolean();
                    }
                }
            }
        } catch (Exception ex) { Develop.DebugPrint("Fehler beim Parsen der Editor-Variablen", ex); }
    }

    private static void Save() {
        try {
            var setsObj = new JsonObject();
            foreach (var kvp in _sets) {
                JsonArray arr = [];
                foreach (var set in kvp.Value) {
                    var setObj = new JsonObject()
                        .Set("name", set.KeyName)
                        .Set("data", set.JsonData.ToJsonNode())
                        .Set("modified", set.Modified.ToString("o"));
                    arr.Add(setObj);
                }
                setsObj.Add(kvp.Key, arr);
            }

            var settingsObj = new JsonObject();
            foreach (var kvp in _settings) {
                settingsObj.Add(kvp.Key, kvp.Value);
            }

            var json = new JsonObject()
                .Set("sets", setsObj)
                .Set("settings", settingsObj);

            _ = IO.WriteAllBytes(_filename, Encoding.UTF8.GetBytes(json.ToJsonString()));
        } catch (Exception ex) { Develop.DebugPrint("Fehler beim Speichern der Editor-Variablen", ex); }
    }

    #endregion
}