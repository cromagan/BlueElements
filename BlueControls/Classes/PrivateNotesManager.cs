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

using BlueBasics.Classes.FileSystemCaching;
using BlueBasics.ClassesStatic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
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

    public static void SetNote(string key, string image, string note) {
        lock (_lock) {
            if (_notes.TryGetValue(key, out var existing)) {
                existing.Image = image;
                existing.Note = note;
            } else {
                _notes[key] = new PrivateNoteEntry(key) {
                    Image = image,
                    Note = note
                };
            }

            Save();
        }
    }

    private static CachedTextFile? GetOrCreateFile() {
        var file = CachedFileSystem.Get<CachedTextFile>(_filename);
        if (file != null) { return file; }

        if (!DirectoryExists(_filename.FilePath())) {
            Directory.CreateDirectory(_filename.FilePath());
        }

        return CachedFileSystem.Register(new CachedTextFile(_filename));
    }

    private static void ParseJson(string json) {
        try {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) { return; }

            if (!doc.RootElement.TryGetProperty("notes", out var notesArray)) { return; }
            if (notesArray.ValueKind != JsonValueKind.Array) { return; }

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
            var sb = new StringBuilder();
            sb.Append("{\"notes\":[");

            var first = true;
            foreach (var entry in _notes.Values) {
                if (!first) { sb.Append(","); }
                first = false;
                sb.Append("{\"keyName\":");
                sb.Append(JsonSerializer.Serialize(entry.KeyName));
                sb.Append(",\"image\":");
                sb.Append(JsonSerializer.Serialize(entry.Image));
                sb.Append(",\"note\":");
                sb.Append(JsonSerializer.Serialize(entry.Note));
                sb.Append('}');
            }

            sb.Append("]}");

            var file = GetOrCreateFile();
            if (file != null) {
                var json = sb.ToString();
                file.Content = Encoding.UTF8.GetBytes(json);
                file.Save();
            }
        } catch { }

        NotesChanged?.Invoke(null, System.EventArgs.Empty);
    }

    #endregion
}