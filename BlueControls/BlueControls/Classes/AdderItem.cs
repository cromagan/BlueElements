// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Interfaces;
using BlueTable;
using BlueTable.Enums;
using System;
using System.Collections.Generic;

namespace BlueControls.Controls;

/// <summary>
/// Diese Element kennt den Schlüssel und alle Zeilen, die erzeugt werden müssen.
/// </summary>
internal class AdderItem : IReadableTextWithKey {

    #region Constructors

    public AdderItem(string generatedTextKey) {
        //GeneratedEntityID = generatedentityID;
        //OriginIDColumn = originIDColumn;

        //AdditionalInfoColumn = additionalInfoColumn;

        Last = generatedTextKey.TrimEnd("\\").FileNameWithSuffix().Trim("+");
        KeyName = generatedTextKey;
    }

    #endregion

    #region Properties

    //public ColumnItem? OriginIDColumn { get; }
    public string ColumnQuickInfo => KeyName;

    public bool KeyIsCaseSensitive => false;

    //public ColumnItem? AdditionalInfoColumn { get; }
    //public string GeneratedEntityID { get; set; }
    /// <summary>
    /// Enstpricht TextKey  (ZUTATEN\\MEHL\\) OHNE #
    /// </summary>
    public string KeyName { get; }

    public List<string> KeysAndInfo { get; } = [];

    public string Last { get; }

    #endregion

    #region Methods

    public static void AddRowsToTable(ColumnItem? originIdColumn, List<string> keysAndInfo, string generatedEntityId, ColumnItem? additionalInfoColumn) {
        if (originIdColumn?.Table is not { IsDisposed: false } db) { return; }

        foreach (var thisKeyAndInfo in keysAndInfo) {
            var key = OriginId(thisKeyAndInfo, originIdColumn, generatedEntityId);

            var keyName = key.TrimStart(generatedEntityId + "\\");

            if (!string.IsNullOrEmpty(keyName)) {
                var r = db.Row.GenerateAndAdd(key, "Zeilengenerator im Formular");

                if (r != null) {
                    originIdColumn.MaxCellLength = Math.Max(originIdColumn.MaxCellLength, key.Length);
                    originIdColumn.MultiLine = false;
                    originIdColumn.AfterEditAutoCorrect = false;
                    originIdColumn.AfterEditQuickSortRemoveDouble = false;
                    originIdColumn.ScriptType = ScriptType.String_Readonly;
                    originIdColumn.AdminInfo = "Diese Spalte wird als Erkennung für den Textgenerator benutzt.";
                    r.CellSet(originIdColumn, key, "Zeilengenerator im Formular");

                    if (additionalInfoColumn != null) {
                        var info = thisKeyAndInfo.SplitBy("#")[1];

                        additionalInfoColumn.MaxCellLength = Math.Max(additionalInfoColumn.MaxCellLength, info.Length);
                        additionalInfoColumn.MultiLine = false;
                        additionalInfoColumn.AfterEditAutoCorrect = false;
                        additionalInfoColumn.AfterEditQuickSortRemoveDouble = false;
                        additionalInfoColumn.ScriptType = ScriptType.String_Readonly;
                        additionalInfoColumn.AdminInfo = "Diese Spalte wird für Zusatzinfos de Textgenerators benutzt.";
                        r.CellSet(additionalInfoColumn, info, "Zeilengenerator im Formular");
                    }
                    _ = r.UpdateRow(true, "Zeile erstellt");
                }
            }
        }
    }

    public static void RemoveRowsFromTable(ColumnItem? originIdColumn, string generatedEntityId, string keyName) {
        if (originIdColumn?.Table is not { IsDisposed: false } db) { return; }

        var fi = new FilterCollection(db, "Zeilengenerator im Formular");
        var key = OriginId(keyName + "#", originIdColumn, generatedEntityId);
        fi.Add(new FilterItem(originIdColumn, FilterType.Istgleich_UND_GroßKleinEgal, key));
        _ = RowCollection.Remove(fi, null, "Zeilengenerator im Formular");
        fi.Dispose();

        //fi = new FilterCollection(db, "Zeilengenerator im Formular");
        //key = OriginId(KeyName + "#", OriginIDColumn, GeneratedEntityID);
        //fi.Add(new FilterItem(OriginIDColumn, FilterType.Istgleich_UND_GroßKleinEgal, key));
        //db.Row.Remove(fi, null, "Zeilengenerator im Formular");
        //fi.Dispose();
    }

    public string ReadableText() => Last;

    public QuickImage? SymbolForReadableText() => null;

    private static string OriginId(string keyAndInfos, ColumnItem? originIdColumn, string generatedEntityId) {
        if (originIdColumn?.Table is not { IsDisposed: false }) { return string.Empty; }
        return generatedEntityId + "\\" + keyAndInfos.SplitBy("#")[0];
    }

    #endregion
}