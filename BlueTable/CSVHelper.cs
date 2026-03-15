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

namespace BlueTable;

/// <summary>
/// Text-Format-Helfer für CSV-Dateien (Comma-Separated Values).
/// Unterstützt konfigurierbaren Trenner und optionale Kopfzeile.
/// Reine In-Memory-Verarbeitung ohne Dateisystem.
/// </summary>
public static class CSVHelper {

    #region Methods

    /// <summary>
    /// Gibt ein einzelnes CSV-Feld korrekt escaped zurück.
    /// </summary>
    public static string EscapeField(string field, char separator) {
        if (field.IndexOf(separator) >= 0 || field.IndexOf('"') >= 0 || field.IndexOf('\r') >= 0 || field.IndexOf('\n') >= 0) {
            return '"' + field.Replace("\"", "\"\"") + '"';
        }
        return field;
    }

    /// <summary>
    /// Gibt alle Felder einer Zeile als CSV-Zeile zurück (public für externe Nutzung).
    /// </summary>
    public static string EscapeFields(List<string> fields, char separator) {
        var sb = new StringBuilder();
        for (var i = 0; i < fields.Count; i++) {
            if (i > 0) { sb.Append(separator); }
            sb.Append(EscapeField(fields[i], separator));
        }
        return sb.ToString();
    }

    public static List<string> ParseLine(string line, char separator) {
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