// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueDatabase;
using System.Collections.Generic;
using System;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;

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

    //public ColumnItem? AdditionalInfoColumn { get; }
    //public string GeneratedEntityID { get; set; }

    #region Properties

    /// <summary>
    /// Enstpricht TextKey  (ZUTATEN\\MEHL\\) OHNE #
    /// </summary>
    public string KeyName { get; }

    public List<string> KeysAndInfo { get; } = new List<string>();

    public string Last { get; private set; }

    //public ColumnItem? OriginIDColumn { get; }
    public string QuickInfo => KeyName;

    #endregion

    #region Methods

    public static void AddRowsToDatabase(ColumnItem? OriginIDColumn, List<string> KeysAndInfo, string GeneratedEntityID, ColumnItem? AdditionalInfoColumn) {
        if (OriginIDColumn?.Database is not { IsDisposed: false } db) { return; }

        for (var z = 0; z < KeysAndInfo.Count; z++) {
            var key = OriginId(KeysAndInfo[z], OriginIDColumn, GeneratedEntityID);

            var KeyName = key.TrimStart(GeneratedEntityID + "\\");

            if (!string.IsNullOrEmpty(KeyName)) {
                var r = db.Row.GenerateAndAdd(key, null, "Zeilengenerator im Formular");

                if (r != null) {
                    OriginIDColumn.MaxCellLenght = Math.Max(OriginIDColumn.MaxCellLenght, key.Length);
                    OriginIDColumn.MultiLine = false;
                    OriginIDColumn.AfterEditAutoCorrect = false;
                    OriginIDColumn.AfterEditQuickSortRemoveDouble = false;
                    OriginIDColumn.ScriptType = ScriptType.String_Readonly;
                    OriginIDColumn.AdminInfo = "Diese Spalte wird als Erkennung für den Textgenerator benutzt.";
                    r.CellSet(OriginIDColumn, key, "Zeilengenerator im Formular");

                    if (AdditionalInfoColumn != null) {
                        var info = KeysAndInfo[z].SplitBy("#")[1];

                        AdditionalInfoColumn.MaxCellLenght = Math.Max(AdditionalInfoColumn.MaxCellLenght, info.Length);
                        AdditionalInfoColumn.MultiLine = false;
                        AdditionalInfoColumn.AfterEditAutoCorrect = false;
                        AdditionalInfoColumn.AfterEditQuickSortRemoveDouble = false;
                        AdditionalInfoColumn.ScriptType = ScriptType.String_Readonly;
                        AdditionalInfoColumn.AdminInfo = "Diese Spalte wird für Zusatzinfos de Textgenerators benutzt.";
                        r.CellSet(AdditionalInfoColumn, info, "Zeilengenerator im Formular");
                    }
                    r.UpdateRow(true, true, "Zeile erstellt");
                }
            }
        }
    }

    public static void RemoveRowsFromDatabase(ColumnItem? OriginIDColumn, string GeneratedEntityId, string KeyName) {
        if (OriginIDColumn?.Database is not { IsDisposed: false } db) { return; }

        var fi = new FilterCollection(db, "Zeilengenerator im Formular");
        var key = OriginId(KeyName + "#", OriginIDColumn, GeneratedEntityId);
        fi.Add(new FilterItem(OriginIDColumn, FilterType.Istgleich_UND_GroßKleinEgal, key));
        db.Row.Remove(fi, null, "Zeilengenerator im Formular");
        fi.Dispose();

        //fi = new FilterCollection(db, "Zeilengenerator im Formular");
        //key = OriginId(KeyName + "#", OriginIDColumn, GeneratedEntityID);
        //fi.Add(new FilterItem(OriginIDColumn, FilterType.Istgleich_UND_GroßKleinEgal, key));
        //db.Row.Remove(fi, null, "Zeilengenerator im Formular");
        //fi.Dispose();
    }

    public string ReadableText() => Last;

    public QuickImage? SymbolForReadableText() => null;

    private static string OriginId(string keyAndInfos, ColumnItem OriginIDColumn, string GeneratedEntityID) {
        if (OriginIDColumn?.Database is not { IsDisposed: false }) { return string.Empty; }
        return GeneratedEntityID + "\\" + keyAndInfos.SplitBy("#")[0];
    }

    #endregion
}