// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes.FileSystemCaching;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BlueControls.Classes;

public static class ViewManager {

    #region Fields

    public const string Last = "Letzte Ansicht";
    public const string Standard = "Standard";
    private static readonly string _filename = $"%appdocumentpath%\\{Generic.UserName}_TableViews.json".NormalizeFile();
    private static readonly object _lock = new();
    private static readonly Dictionary<string, bool> _settings = [];
    private static readonly Dictionary<string, List<JsonEntry>> _views = [];

    #endregion

    #region Methods

    public static void DeleteView(string tableKey, string viewName) {
        lock (_lock) {
            if (_views.TryGetValue(tableKey, out var list)) {
                list.RemoveAll(v => string.Equals(v.KeyName, viewName, StringComparison.OrdinalIgnoreCase));
                if (list.Count == 0) { _views.Remove(tableKey); }
            }
            Save();
        }
    }

    public static bool GetAutoLoadLastView(string tableKey) {
        lock (_lock) {
            InitializeIfNeeded();
            return _settings.TryGetValue(tableKey, out var value) && value;
        }
    }

    public static List<JsonEntry> GetViews(string tableKey) {
        lock (_lock) {
            InitializeIfNeeded();
            return _views.TryGetValue(tableKey, out var list) ? list.ToList() : [];
        }
    }

    public static bool HasView(string tableKey, string viewName) {
        lock (_lock) {
            InitializeIfNeeded();
            if (!_views.TryGetValue(tableKey, out var list)) { return false; }
            return list.Exists(v => string.Equals(v.KeyName, viewName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static void InitializeIfNeeded() {
        lock (_lock) {
            if (_views.Count > 0) { return; }

            if (CachedFileSystem.Get<CachedTextFile>(_filename) is { } file) {
                var json = file.GetContentAsString();
                if (!string.IsNullOrEmpty(json)) {
                    ParseJson(json);
                }
            }
        }
    }

    public static void SaveView(string tableKey, string viewName, JsonNode? viewData) {
        lock (_lock) {
            InitializeIfNeeded();

            var element = viewData is { } ? JsonSerializer.SerializeToElement(viewData) : default;

            if (!_views.TryGetValue(tableKey, out var list)) {
                list = [];
                _views[tableKey] = list;
            }

            if (list.FirstOrDefault(v => string.Equals(v.KeyName, viewName, StringComparison.OrdinalIgnoreCase)) is { } existing) {
                existing.JsonData = element;
                existing.Modified = DateTime.Now;
            } else {
                list.Add(new JsonEntry(viewName, element));
            }

            Save();
        }
    }

    public static void SetAutoLoadLastView(string tableKey, bool value) {
        lock (_lock) {
            InitializeIfNeeded();
            _settings[tableKey] = value;
            Save();
        }
    }

    private static void ParseJson(string json) {
        try {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.IsObject()) { return; }

            var viewsObj = root.GetJson("views");
            if (viewsObj is not null && viewsObj.Value.IsObject()) {
                foreach (var tableEntry in viewsObj.Value.EnumerateObject()) {
                    if (!tableEntry.Value.IsArray()) { continue; }

                    var viewList = new List<JsonEntry>();
                    foreach (var viewEl in tableEntry.Value.EnumerateArray()) {
                        var entry = JsonEntry.Parse(viewEl);
                        if (entry is not null && !string.IsNullOrEmpty(entry.KeyName)) {
                            viewList.Add(entry);
                        }
                    }
                    if (viewList.Count > 0) {
                        _views[tableEntry.Name] = viewList;
                    }
                }
            }

            var settingsObj = root.GetJson("settings");
            if (settingsObj is not null && settingsObj.Value.IsObject()) {
                foreach (var settingEntry in settingsObj.Value.EnumerateObject()) {
                    if (settingEntry.Value.ValueKind == JsonValueKind.True || settingEntry.Value.ValueKind == JsonValueKind.False) {
                        _settings[settingEntry.Name] = settingEntry.Value.GetBoolean();
                    }
                }
            }
        } catch { }
    }

    private static void Save() {
        try {
            var viewsObj = new JsonObject();
            foreach (var kvp in _views) {
                var arr = new JsonArray();
                foreach (var view in kvp.Value) {
                    var viewObj = new JsonObject {
                        { "name", view.KeyName },
                        { "data", JsonSerializer.Deserialize<JsonNode>(view.JsonData) },
                        { "modified", view.Modified.ToString("o") }
                    };
                    arr.Add(viewObj);
                }
                viewsObj.Add(kvp.Key, arr);
            }

            var settingsObj = new JsonObject();
            foreach (var kvp in _settings) {
                settingsObj.Add(kvp.Key, kvp.Value);
            }

            var json = new JsonObject {
                ["views"] = viewsObj,
                ["settings"] = settingsObj
            };

            var file = CachedFileSystem.Get<CachedTextFile>(_filename) ?? new CachedTextFile(_filename);
            file.EnsureContentLoaded();
            file.Content = Encoding.UTF8.GetBytes(json.ToJsonString());
            file.Save();
        } catch { }
    }

    #endregion
}