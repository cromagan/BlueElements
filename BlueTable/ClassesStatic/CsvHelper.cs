// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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
            sb.AppendJoin(separator, headerFields).AppendLine();
        }

        foreach (var row in table.Row) {
            if (row.IsDisposed) { continue; }

            var escapedFields = EscapeCSVFields(columns.Select(row.CellGetString).ToList(), separator);
            sb.AppendJoin(separator, escapedFields).AppendLine();
        }

        return sb.ToString();
    }

    public static string ImportCsv(Table table, string importText, bool zeileZuordnen, string splitChar, bool eliminateMultipleSplitter, bool eleminateSplitterAtStart) {
        return ImportCsv(table, importText, zeileZuordnen, splitChar.Length > 0 ? splitChar[0] : ';', eliminateMultipleSplitter, eleminateSplitterAtStart);
    }

    public static string ImportCsv(Table table, string importText, bool zeileZuordnen, char separator = ';', bool eliminateMultipleSplitter = false, bool eleminateSplitterAtStart = false) {
        var f = table.IsGenericEditable(false);

        if (!string.IsNullOrEmpty(f)) {
            Develop.Message(ErrorType.Warning, table, table.Caption, ImageCode.Tabelle, $"Abbruch, {f}", 0);
            return $"Abbruch, {f}";
        }

        #region Text vorbereiten

        importText = importText.Replace("\r\n", "\r").Trim('\r');

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
            zeil.Add([.. ParseCSVLine(line, separator)]);
        }

        if (zeil.Count == 0) {
            Develop.Message(ErrorType.Warning, table, table.Caption, ImageCode.Tabelle, "Abbruch, keine Zeilen zum Importieren erkannt.", 0);
            return "Abbruch, keine Zeilen zum Importieren erkannt.";
        }

        #endregion

        #region Spaltenreihenfolge (columns) ermitteln

        List<ColumnItem> columns = [];
        var startZ = 1;

        for (var spaltNo = 0; spaltNo < zeil[0].Count; spaltNo++) {
            if (string.IsNullOrEmpty(zeil[0][spaltNo])) {
                Develop.Message(ErrorType.Warning, table, table.Caption, ImageCode.Tabelle, "Abbruch, leerer Spaltenname.", 0);
                return "Abbruch,<br>leerer Spaltenname.";
            }
            zeil[0][spaltNo] = ColumnItem.MakeValidColumnKey(zeil[0][spaltNo]);

            var col = table.Column[zeil[0][spaltNo]];
            if (col == null) {
                if (!ColumnItem.IsValidColumnKey(zeil[0][spaltNo])) {
                    Develop.Message(ErrorType.Warning, table, table.Caption, ImageCode.Tabelle, "Abbruch, ungültiger Spaltenname.", 0);
                    return "Abbruch,<br>ungültiger Spaltenname.";
                }

                col = table.Column.GenerateAndAdd(zeil[0][spaltNo]);
                col?.Caption = zeil[0][spaltNo];
            }

            if (col == null) {
                Develop.Message(ErrorType.Warning, table, table.Caption, ImageCode.Tabelle, "Abbruch, Spaltenfehler.", 0);
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
                    Develop.Message(ErrorType.Warning, table, table.Caption, ImageCode.Tabelle, "Abbruch, vorhandene Zeilen der Tabelle '" + table.Caption + "' sind nicht eindeutig.", 0);
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
                Develop.Message(ErrorType.Warning, table, table.Caption, ImageCode.Tabelle, "Abbruch, Leere Zeile im Import.", 0);
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
                Develop.Message(ErrorType.Warning, table, table.Caption, ImageCode.Tabelle, "Abbruch, Import-Fehler.", 0);
                return "Abbruch, Import-Fehler.";
            }

            #endregion

            #region Werte in die Spalten schreiben

            for (var colNo = 0; colNo < maxColCount; colNo++) {
                row.CellSet(columns[colNo], string.Join('\r', thisD.Value[colNo].SplitAndCutBy("|")), "CSV-Import");
            }

            #endregion

            #region Speichern und Ausgabe

            if (DateTime.Now.Subtract(d2).TotalSeconds > 4.5) {
                Develop.Message(ErrorType.Info, table, table.Caption, ImageCode.Tabelle, "Import: Zeile " + no + " von " + zeil.Count, 0);
                d2 = DateTime.Now;
            }

            #endregion
        }

        #endregion

        Develop.Message(ErrorType.Info, table, table.Caption, ImageCode.Tabelle, "<b>Import abgeschlossen.</b>\r\n" + neuZ + " neue Zeilen erstellt.", 0);
        return string.Empty;
    }

    public static IEnumerable<string> ParseCSVLine(string line, char separator) {
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
                    yield return currentField.ToString();
                    currentField.Clear();
                } else {
                    currentField.Append(c);
                }
            }
        }

        yield return currentField.ToString();
    }

    #endregion
}