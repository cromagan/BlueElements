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

using System;
using System.Collections.Generic;
using System.Text;

namespace BlueBasics.Classes.FileHelpers;

/// <summary>
/// Text-Format-Helfer für INI-Dateien.
/// Format: [Sektion] gefolgt von Key=Value-Paaren.
/// Kommentarzeilen beginnen mit ; oder #.
/// Reine In-Memory-Verarbeitung ohne Dateisystem.
/// </summary>
public class IniHelper : TextFileHelper {

    #region Fields

    /// <summary>
    /// Sektionsname für Schlüssel außerhalb jeder Sektion.
    /// </summary>
    public const string DefaultSection = "";

    private readonly Dictionary<string, Dictionary<string, string>> _data =
        new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Methods

    public override bool ParseContent(string content) {
        _data.Clear();

        if (string.IsNullOrEmpty(content)) { return true; }

        content = content.Replace("\r\n", "\r").Replace("\n", "\r");
        var lines = content.Split('\r');
        var currentSection = DefaultSection;

        foreach (var rawLine in lines) {
            var line = rawLine.Trim();

            if (string.IsNullOrEmpty(line)) { continue; }
            if (line.StartsWith(";", StringComparison.Ordinal) ||
                line.StartsWith("#", StringComparison.Ordinal)) { continue; }

            if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal)) {
                currentSection = line.Substring(1, line.Length - 2).Trim();
                if (!_data.ContainsKey(currentSection)) {
                    _data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                continue;
            }

            var eqIndex = line.IndexOf('=');
            if (eqIndex < 0) { continue; }

            var key = line.Substring(0, eqIndex).Trim();
            var value = line.Substring(eqIndex + 1).Trim();

            if (!_data.ContainsKey(currentSection)) {
                _data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            _data[currentSection][key] = value;
        }

        return true;
    }

    public override string SerializeContent() {
        var sb = new StringBuilder();

        // Erst Schlüssel ohne Sektion
        if (_data.TryGetValue(DefaultSection, out var defaultKeys) && defaultKeys.Count > 0) {
            foreach (var kv in defaultKeys) {
                sb.AppendLine(kv.Key + "=" + kv.Value);
            }
            sb.AppendLine();
        }

        foreach (var section in _data) {
            if (string.Equals(section.Key, DefaultSection, StringComparison.Ordinal)) { continue; }
            sb.AppendLine("[" + section.Key + "]");
            foreach (var kv in section.Value) {
                sb.AppendLine(kv.Key + "=" + kv.Value);
            }
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd('\r', '\n');
    }

    /// <summary>
    /// Gibt den Wert eines Schlüssels zurück.
    /// </summary>
    public string Get(string section, string key, string defaultValue = "") {
        if (!_data.TryGetValue(section, out var sectionDict)) { return defaultValue; }
        return sectionDict.TryGetValue(key, out var val) ? val : defaultValue;
    }

    /// <summary>
    /// Gibt den Wert eines Schlüssels aus der Default-Sektion zurück.
    /// </summary>
    public string Get(string key, string defaultValue = "") => Get(DefaultSection, key, defaultValue);

    /// <summary>
    /// Gibt alle Sektionsnamen zurück (ohne Default-Sektion).
    /// </summary>
    public IEnumerable<string> GetSections() {
        foreach (var key in _data.Keys) {
            if (!string.Equals(key, DefaultSection, StringComparison.Ordinal)) { yield return key; }
        }
    }

    /// <summary>
    /// Gibt alle Schlüssel einer Sektion zurück.
    /// </summary>
    public IEnumerable<string> GetKeys(string section) {
        if (!_data.TryGetValue(section, out var sectionDict)) { yield break; }
        foreach (var key in sectionDict.Keys) { yield return key; }
    }

    /// <summary>
    /// Setzt einen Wert.
    /// </summary>
    public void Set(string section, string key, string value) {
        if (!_data.ContainsKey(section)) {
            _data[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        _data[section][key] = value;
    }

    /// <summary>
    /// Setzt einen Wert in der Default-Sektion.
    /// </summary>
    public void Set(string key, string value) => Set(DefaultSection, key, value);

    #endregion
}
