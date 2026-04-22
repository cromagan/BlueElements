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
using static BlueBasics.ClassesStatic.IO;

namespace BlueControls.Classes;

public static class PrivateNotesManager {

    #region Fields

    private static readonly string _filename = $"%appdocumentpath%\\{Generic.UserName}_PrivateNotes.json".NormalizeFile();
    private static readonly object _lock = new();
    private static readonly Dictionary<string, PrivateNoteEntry> _notes = [];

    #endregion

    #region Events

    public static event EventHandler? NotesChanged;

    #endregion

    #region Methods

    public static PrivateNoteEntry? GetNote(string key) {
        lock (_lock) {
            return _notes.TryGetValue(key, out var entry) ? entry : null;
        }
    }

    public static List<PrivateNoteEntry> GetNotesByKeyPrefix(string prefix) {
        lock (_lock) {
            return _notes.Where(kvp => kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                         .Select(kvp => kvp.Value)
                         .ToList();
        }
    }

    public static void Initialize() {
        lock (_lock) {
            if (_notes.Count > 0) { return; }

            var file = CachedFileSystem.Get<CachedTextFile>(_filename);
            if (file != null) {
                var json = file.GetContentAsString();
                if (!string.IsNullOrEmpty(json)) {
                    ParseJson(json);
                }
            }
        }
    }

    public static void RemoveNote(string key) {
        lock (_lock) {
            _notes.Remove(key);
            Save();
        }
    }

    public static void RemoveNotesByKeyPrefix(string prefix) {
        lock (_lock) {
            var keysToRemove = _notes.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var key in keysToRemove) {
                _notes.Remove(key);
            }
            Save();
        }
    }

    public static void SetNote(string key, string symbol, string note) => SetNote(key, symbol, note, -1, -1);

    public static void SetNote(string key, string symbol, string note, float x, float y) {
        lock (_lock) {
            if (_notes.TryGetValue(key, out var existing)) {
                existing.Symbol = symbol;
                existing.Note = note;
                existing.X = x;
                existing.Y = y;
            } else {
                _notes[key] = new PrivateNoteEntry(key) {
                    Symbol = symbol,
                    Note = note,
                    X = x,
                    Y = y
                };
            }

            Save();
        }
    }

    private static void ParseJson(string json) {
        try {
            var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.IsObject()) { return; }
            if (doc.RootElement.GetJson("notes") is not { } notesArray) { return; }

            foreach (var item in notesArray.EnumerateArray()) {
                var entry = PrivateNoteEntry.Parse(item);
                if (entry != null && !string.IsNullOrEmpty(entry.KeyName)) {
                    _notes[entry.KeyName] = entry;
                }
            }
        } catch { }
    }

    private static void Save() {
        try {
            var arr = new JsonArray();
            foreach (var entry in _notes.Values) {
                var obj = new JsonObject {
                    { "keyName", entry.KeyName },
                    { "symbol", entry.Symbol },
                    { "note", entry.Note },
                    { "x", (double)entry.X },
                    { "y", (double)entry.Y }
                };
                arr.Add(obj);
            }

            var json = new JsonObject { ["notes"] = arr };

            var file = CachedFileSystem.Get<CachedTextFile>(_filename) ?? CachedFileSystem.Register(new CachedTextFile(_filename));
            file.Content = Encoding.UTF8.GetBytes(json.ToJsonString());
            file.Save();
        } catch { }

        NotesChanged?.Invoke(null, System.EventArgs.Empty);
    }

    #endregion
}