// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics;
using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.ClassesStatic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BlueControls.Classes;

public static class ViewManager {

    #region Fields

    private static readonly string _filename = $"%appdocumentpath%\\{Generic.UserName}_TableViews.json".NormalizeFile();
    private static readonly object _lock = new();
    private static readonly Dictionary<string, List<SavedViewEntry>> _views = [];

    #endregion

    #region Methods

    public static void DeleteView(string tableKey, string viewName) {
        lock (_lock) {
            if (_views.TryGetValue(tableKey, out var list)) {
                list.RemoveAll(v => string.Equals(v.Name, viewName, StringComparison.OrdinalIgnoreCase));
                if (list.Count == 0) { _views.Remove(tableKey); }
            }
            Save();
        }
    }

    public static List<SavedViewEntry> GetViews(string tableKey) {
        lock (_lock) {
            InitializeIfNeeded();
            return _views.TryGetValue(tableKey, out var list) ? list.ToList() : [];
        }
    }

    public static void InitializeIfNeeded() {
        lock (_lock) {
            if (_views.Count > 0) { return; }

            var file = CachedFileSystem.Get<CachedTextFile>(_filename);
            if (file != null) {
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

            var element = viewData != null ? JsonSerializer.SerializeToElement(viewData) : default(JsonElement);

            if (!_views.TryGetValue(tableKey, out var list)) {
                list = [];
                _views[tableKey] = list;
            }

            var existing = list.FirstOrDefault(v => string.Equals(v.Name, viewName, StringComparison.OrdinalIgnoreCase));
            if (existing != null) {
                existing.ViewData = element;
                existing.Modified = DateTime.Now;
            } else {
                list.Add(new SavedViewEntry(viewName, element));
            }

            Save();
        }
    }

    private static void ParseJson(string json) {
        try {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.IsObject()) { return; }

            var viewsObj = root.GetJson("views");
            if (viewsObj == null || !viewsObj.Value.IsObject()) { return; }

            foreach (var tableEntry in viewsObj.Value.EnumerateObject()) {
                if (!tableEntry.Value.IsArray()) { continue; }

                var viewList = new List<SavedViewEntry>();
                foreach (var viewEl in tableEntry.Value.EnumerateArray()) {
                    var entry = SavedViewEntry.Parse(viewEl);
                    if (entry != null && !string.IsNullOrEmpty(entry.Name)) {
                        viewList.Add(entry);
                    }
                }
                if (viewList.Count > 0) {
                    _views[tableEntry.Name] = viewList;
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
                        { "name", view.Name },
                        { "data", JsonSerializer.Deserialize<JsonNode>(view.ViewData) },
                        { "modified", view.Modified.ToString("o") }
                    };
                    arr.Add(viewObj);
                }
                viewsObj.Add(kvp.Key, arr);
            }

            var json = new JsonObject { ["views"] = viewsObj };

            var file = CachedFileSystem.Get<CachedTextFile>(_filename) ?? CachedFileSystem.Register(new CachedTextFile(_filename));
            file.Content = Encoding.UTF8.GetBytes(json.ToJsonString());
            file.Save();
        } catch { }
    }

    #endregion

    #region Classes

    public sealed class SavedViewEntry {

        #region Constructors

        public SavedViewEntry(string name, JsonElement viewData) {
            Name = name;
            ViewData = viewData;
            Modified = DateTime.Now;
        }

        #endregion

        #region Properties

        public DateTime Modified { get; set; }
        public string Name { get; set; }
        public JsonElement ViewData { get; set; }

        #endregion

        #region Methods

        public static SavedViewEntry? Parse(JsonElement element) {
            if (!element.IsObject()) { return null; }

            var name = element.GetString("name");
            if (string.IsNullOrEmpty(name)) { return null; }

            var data = element.GetJson("data");
            return new SavedViewEntry(name, data ?? default);
        }

        #endregion
    }

    #endregion
}