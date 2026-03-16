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

using System.Collections.Generic;
using System.Text;

namespace BlueBasics.Classes.FileHelpers;

/// <summary>
/// Text-Format-Helfer für CSV-Dateien (Comma-Separated Values).
/// Unterstützt konfigurierbaren Trenner und optionale Kopfzeile.
/// Reine In-Memory-Verarbeitung ohne Dateisystem.
/// </summary>
public class CSVHelper : TextFileHelper {

    #region Fields

    private List<List<string>> _rows = [];

    #endregion

    #region Properties

    /// <summary>
    /// Erste Zeile als Kopfzeile interpretieren.
    /// </summary>
    public bool FirstLineIsHeader { get; set; } = true;

    /// <summary>
    /// Die Kopfzeile (Spaltennamen), falls <see cref="FirstLineIsHeader"/> aktiv.
    /// </summary>
    public List<string> Headers { get; private set; } = [];

    /// <summary>
    /// Alle Datenzeilen (ohne Kopfzeile).
    /// </summary>
    public List<List<string>> Rows => _rows;

    /// <summary>
    /// Trennzeichen zwischen Feldern. Standard: Semikolon.
    /// </summary>
    public char Separator { get; set; } = ';';

    #endregion

    #region Methods

    public override bool ParseContent(string content) {
        _rows = [];
        Headers = [];

        if (string.IsNullOrEmpty(content)) { return true; }

        content = content.Replace("\r\n", "\r").Replace("\n", "\r");
        var lines = content.Split('\r');

        var startLine = 0;

        if (FirstLineIsHeader && lines.Length > 0) {
            Headers = ParseLine(lines[0], Separator);
            startLine = 1;
        }

        for (var i = startLine; i < lines.Length; i++) {
            var line = lines[i];
            if (string.IsNullOrEmpty(line)) { continue; }
            _rows.Add(ParseLine(line, Separator));
        }

        return true;
    }

    public override string SerializeContent() {
        var sb = new StringBuilder();

        if (FirstLineIsHeader && Headers.Count > 0) {
            sb.AppendLine(EscapeFields(Headers, Separator));
        }

        foreach (var row in _rows) {
            sb.AppendLine(EscapeFields(row, Separator));
        }

        return sb.ToString().TrimEnd('\r', '\n');
    }

    /// <summary>
    /// Gibt den Wert einer Zelle anhand von Zeile und Spalte zurück.
    /// </summary>
    public string GetCell(int row, int col) {
        if (row < 0 || row >= _rows.Count) { return string.Empty; }
        var r = _rows[row];
        if (col < 0 || col >= r.Count) { return string.Empty; }
        return r[col];
    }

    /// <summary>
    /// Gibt den Wert einer Zelle anhand von Zeile und Spaltenname zurück.
    /// Setzt <see cref="FirstLineIsHeader"/> = true voraus.
    /// </summary>
    public string GetCell(int row, string columnName) {
        var col = Headers.IndexOf(columnName);
        if (col < 0) { return string.Empty; }
        return GetCell(row, col);
    }

    private static string EscapeFields(List<string> fields, char separator) {
        var sb = new StringBuilder();
        for (var i = 0; i < fields.Count; i++) {
            if (i > 0) { sb.Append(separator); }
            var field = fields[i];
            if (field.IndexOf(separator) >= 0 || field.IndexOf('"') >= 0 || field.IndexOf('\r') >= 0 || field.IndexOf('\n') >= 0) {
                sb.Append('"');
                sb.Append(field.Replace("\"", "\"\""));
                sb.Append('"');
            } else {
                sb.Append(field);
            }
        }
        return sb.ToString();
    }

    private static List<string> ParseLine(string line, char separator) {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++) {
            var c = line[i];

            if (inQuotes) {
                if (c == '"') {
                    if (i + 1 < line.Length && line[i + 1] == '"') {
                        current.Append('"');
                        i++;
                    } else {
                        inQuotes = false;
                    }
                } else {
                    current.Append(c);
                }
            } else {
                if (c == '"') {
                    inQuotes = true;
                } else if (c == separator) {
                    fields.Add(current.ToString());
                    current.Clear();
                } else {
                    current.Append(c);
                }
            }
        }

        fields.Add(current.ToString());
        return fields;
    }

    #endregion
}
