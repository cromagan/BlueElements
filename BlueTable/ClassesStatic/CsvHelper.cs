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
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueTable.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlueTable.ClassesStatic;

public static class CsvHelper {

    #region Methods

    public static string EscapeCSVField(string field, char separator) {
        if (string.IsNullOrEmpty(field)) { return string.Empty; }

        var needsQuoting = field.Contains(separator) ||
                           field.Contains('"') ||
                           field.Contains('\r') ||
                           field.Contains('\n');

        if (!needsQuoting) { return field; }

        return "\"" + field.Replace("\"", "\"\"") + "\"";
    }

    public static List<string> EscapeCSVFields(List<string> fields, char separator) =>
        fields.Select(field => EscapeCSVField(field, separator)).ToList();

    public static string ExportCSV(Table table, char separator, bool firstLineIsHeader) {
        var sb = new StringBuilder();

        var columns = new List<ColumnItem>();
        foreach (var col in table.Column) {
            if (!col.IsDisposed && col.SaveContent) {
                columns.Add(col);
            }
        }

        if (columns.Count == 0) { return string.Empty; }

        if (firstLineIsHeader) {
            var headerFields = columns.Select(col => EscapeCSVField(col.KeyName, separator));
            sb.AppendLine(string.Join(separator.ToString(), headerFields));
        }

        foreach (var row in table.Row) {
            if (row.IsDisposed) { continue; }

            var escapedFields = EscapeCSVFields(columns.Select(col => row.CellGetString(col)).ToList(), separator);
            sb.AppendLine(string.Join(separator.ToString(), escapedFields));
        }

        return sb.ToString();
    }

    public static string ImportCsv(Table table, string importText, bool zeileZuordnen, string splitChar, bool eliminateMultipleSplitter, bool eleminateSplitterAtStart) {
        return ImportCsv(table, importText, zeileZuordnen, splitChar.Length > 0 ? splitChar[0] : ';', eliminateMultipleSplitter, eleminateSplitterAtStart);
    }

    public static string ImportCsv(Table table, string importText, bool zeileZuordnen, char separator = ';', bool eliminateMultipleSplitter = false, bool eleminateSplitterAtStart = false) {
        if (!table.IsEditable(false)) {
            table.DropMessage(ErrorType.Warning, "Abbruch, " + table.IsGenericEditable(false));
            return "Abbruch, " + table.IsGenericEditable(false);
        }

        #region Text vorbereiten

        importText = importText.Replace("\r\n", "\r").Trim("\r");

        #endregion

        #region Die Zeilen (zeil) vorbereiten

        var ein = importText.SplitAndCutByCr();
        List<List<string>> zeil = [];
        var neuZ = 0;
        for (var z = 0; z <= ein.GetUpperBound(0); z++) {
            var line = ein[z];
            if (eliminateMultipleSplitter) {
                line = line.Replace(separator.ToString() + separator.ToString(), separator.ToString());
            }
            if (eleminateSplitterAtStart) {
                line = line.TrimStart(separator);
            }
            line = line.TrimEnd(separator);
            zeil.Add(ParseCSVLine(line, separator));
        }

        if (zeil.Count == 0) {
            table.DropMessage(ErrorType.Warning, "Abbruch, keine Zeilen zum Importieren erkannt.");
            return "Abbruch, keine Zeilen zum Importieren erkannt.";
        }

        #endregion

        #region Spaltenreihenfolge (columns) ermitteln

        List<ColumnItem> columns = [];
        var startZ = 1;

        for (var spaltNo = 0; spaltNo < zeil[0].Count; spaltNo++) {
            if (string.IsNullOrEmpty(zeil[0][spaltNo])) {
                table.DropMessage(ErrorType.Warning, "Abbruch, leerer Spaltenname.");
                return "Abbruch,<br>leerer Spaltenname.";
            }
            zeil[0][spaltNo] = ColumnItem.MakeValidColumnName(zeil[0][spaltNo]);

            var col = table.Column[zeil[0][spaltNo]];
            if (col == null) {
                if (!ColumnItem.IsValidColumnName(zeil[0][spaltNo])) {
                    table.DropMessage(ErrorType.Warning, "Abbruch, ungültiger Spaltenname.");
                    return "Abbruch,<br>ungültiger Spaltenname.";
                }

                col = table.Column.GenerateAndAdd(zeil[0][spaltNo]);
                col?.Caption = zeil[0][spaltNo];
            }

            if (col == null) {
                table.DropMessage(ErrorType.Warning, "Abbruch, Spaltenfehler.");
                return "Abbruch,<br>Spaltenfehler.";
            }

            columns.Add(col);
        }

        #endregion

        #region Neue Werte in ein Dictionary schreiben (dictNeu)

        var dictNeu = new Dictionary<string, List<string>>();

        for (var rowNo = startZ; rowNo < zeil.Count; rowNo++) {
            if (zeileZuordnen) {
                if (zeil[rowNo].Count > 0 && !string.IsNullOrEmpty(zeil[rowNo][0]) && !dictNeu.ContainsKey(zeil[rowNo][0].ToUpperInvariant())) {
                    dictNeu.Add(zeil[rowNo][0].ToUpperInvariant(), zeil[rowNo]);
                }
            } else {
                dictNeu.Add(rowNo.ToString1(), zeil[rowNo]);
            }
        }

        #endregion

        #region Zeilen, die BEACHTET werden sollen, in ein Dictionary schreiben (dictVorhanden)

        var dictVorhanden = new Dictionary<string, RowItem>();

        if (zeileZuordnen) {
            foreach (var thisR in table.Row) {
                var cv = thisR.CellFirstString().ToUpperInvariant();
                if (!string.IsNullOrEmpty(cv) && !dictVorhanden.ContainsKey(cv)) {
                    dictVorhanden.Add(cv, thisR);
                } else {
                    table.DropMessage(ErrorType.Warning, "Abbruch, vorhandene Zeilen der Tabelle '" + table.Caption + "' sind nicht eindeutig.");
                    return "Abbruch, vorhandene Zeilen sind nicht eindeutig.";
                }
            }
        }

        #endregion

        #region Der eigentliche Import

        var d2 = DateTime.Now;

        var no = 0;
        foreach (var thisD in dictNeu) {
            no++;

            #region Spaltenanzahl zum Import ermitteln (maxColCount)

            var maxColCount = Math.Min(thisD.Value.Count, columns.Count);

            if (maxColCount == 0) {
                table.DropMessage(ErrorType.Warning, "Abbruch, Leere Zeile im Import.");
                return "Abbruch, Leere Zeile im Import.";
            }

            #endregion

            #region Row zum schreiben ermitteln (row)

            RowItem? row;

            if (dictVorhanden.ContainsKey(thisD.Key)) {
                dictVorhanden.TryGetValue(thisD.Key, out row);
                dictVorhanden.Remove(thisD.Key); // Speedup
            } else {
                neuZ++;
                row = table.Row.GenerateAndAdd(thisD.Value[0], "Import, fehlende Zeile");
            }

            if (row == null) {
                table.DropMessage(ErrorType.Warning, "Abbruch, Import-Fehler.");
                return "Abbruch, Import-Fehler.";
            }

            #endregion

            #region Werte in die Spalten schreiben

            for (var colNo = 0; colNo < maxColCount; colNo++) {
                row.CellSet(columns[colNo], thisD.Value[colNo].SplitAndCutBy("|").JoinWithCr(), "CSV-Import");
            }

            #endregion

            #region Speichern und Ausgabe

            if (DateTime.Now.Subtract(d2).TotalSeconds > 4.5) {
                table.DropMessage(ErrorType.Info, "Import: Zeile " + no + " von " + zeil.Count);
                d2 = DateTime.Now;
            }
            Develop.SetUserDidSomething();

            #endregion
        }

        #endregion

        table.DropMessage(ErrorType.Info, "<b>Import abgeschlossen.</b>\r\n" + neuZ + " neue Zeilen erstellt.");
        return string.Empty;
    }

    public static List<string> ParseCSVLine(string line, char separator) {
        var result = new List<string>();
        var currentField = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++) {
            var c = line[i];

            if (inQuotes) {
                if (c == '"') {
                    if (i + 1 < line.Length && line[i + 1] == '"') {
                        currentField.Append('"');
                        i++;
                    } else {
                        inQuotes = false;
                    }
                } else {
                    currentField.Append(c);
                }
            } else {
                if (c == '"') {
                    inQuotes = true;
                } else if (c == separator) {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                } else {
                    currentField.Append(c);
                }
            }
        }

        result.Add(currentField.ToString());
        return result;
    }

    #endregion
}