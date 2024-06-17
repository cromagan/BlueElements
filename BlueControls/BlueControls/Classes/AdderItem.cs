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
using static BlueBasics.IO;

namespace BlueControls.Controls;

internal class AdderItem : IReadableTextWithKey {

    #region Constructors

    public AdderItem(string generatedentityID, ColumnItem? originIDColumn, string generatedTextKeyWOAsterix, ColumnItem? additinalTextColumn) {
        GeneratedEntityID = generatedentityID;
        OriginIDColumn = originIDColumn;

        Indent = Math.Max(generatedTextKeyWOAsterix.CountString("\\") - 1, 0);
        Last = generatedTextKeyWOAsterix.TrimEnd("\\").FileNameWithSuffix();
        KeyName = generatedTextKeyWOAsterix.ToUpper();
        AdditinalTextColumn = additinalTextColumn;
    }

    #endregion

    #region Properties

    public ColumnItem? AdditinalTextColumn { get; }
    public string GeneratedEntityID { get; set; }
    public int Indent { get; private set; }

    /// <summary>
    /// Enstpricht TextKey  (ZUTATEN\\MEHL\\) OHNE *
    /// </summary>
    public string KeyName { get; }

    public string Last { get; private set; }
    public ColumnItem? OriginIDColumn { get; }

    public string QuickInfo => KeyName;

    public List<AdderItemSingle> Rows { get; } = new List<AdderItemSingle>();

    #endregion

    #region Methods

    public void AddRowsToDatabase() {
        if (OriginIDColumn?.Database is not Database db || db.IsDisposed) { return; }
        if (AdditinalTextColumn == null) { return; }

        foreach (var row in Rows) {
            var oriid = OriginId(row);
            if (!string.IsNullOrEmpty(oriid)) {
                if (!row.RealAdder) { continue; }

                var r = db.Row.GenerateAndAdd(GeneratedEntityID, null, "Zeilengenerator im Formular");

                if (r != null) {
                    OriginIDColumn.MaxCellLenght = Math.Max(OriginIDColumn.MaxCellLenght, oriid.Length);
                    OriginIDColumn.MultiLine = true;
                    OriginIDColumn.AfterEditAutoCorrect = false;
                    OriginIDColumn.AfterEditQuickSortRemoveDouble = false;
                    OriginIDColumn.ScriptType = ScriptType.Nicht_vorhanden;
                    OriginIDColumn.AdminInfo = "Diese Spalte wird als Erkennung für den Textgenerator benutzt.\r\n" +
                        "Sie darf weder gelesen noch verändert werden. Dafür ist die Zusatz-Text-Spalte vorhanden.";
                    r.CellSet(OriginIDColumn, oriid, "Zeilengenerator im Formular");

                    AdditinalTextColumn.MaxCellLenght = Math.Max(AdditinalTextColumn.MaxCellLenght, row.Additionaltext.Length);
                    r.CellSet(AdditinalTextColumn, row.Additionaltext, "Zeilengenerator im Formular");
                }
            }
        }
    }

    public string ReadableText() {
        return new string(' ', Indent * 6) + Last;
    }

    public QuickImage? SymbolForReadableText() => null;

    internal void RemoveRowsFromDatabase() {
        if (OriginIDColumn?.Database is not Database db || db.IsDisposed) { return; }

        foreach (var row in Rows) {
            var fi = new FilterCollection(db, "Zeilengenerator im Formular");

            fi.Add(new FilterItem(OriginIDColumn, FilterType.Istgleich_UND_GroßKleinEgal, ["<ID>" + GeneratedEntityID, "<TK>" + row.GeneratedTextKey]));
            //fi.Add(new FilterItem(origin, FilterType.Istgleich_GroßKleinEgal, row.GeneratedTextKey));

            db.Row.Remove(fi, null, "Zeilengenerator im Formular");
            fi.Dispose();

            //var oriid = OriginId(row);

            //if (!string.IsNullOrEmpty(oriid)) {
            //    var r = db.Row.GenerateAndAdd(GeneratedEntityID, null, "Zeilengenerator im Formular");

            //    if (r != null) {
            //        r.CellSet(EntityIDColumn, GeneratedEntityID, "Zeilengenerator im Formular");
            //        r.CellSet(OriginIDColumn, oriid, "Zeilengenerator im Formular");
            //        r.CellSet(TextKeyColumn, row.GeneratedTextKey, "Zeilengenerator im Formular");
            //        r.CellSet(AdditinalTextColumn, row.Additionaltext, "Zeilengenerator im Formular");
            //    }
            //}
        }
    }

    private string OriginId(AdderItemSingle ais) {
        if (OriginIDColumn?.Database is not Database db || db.IsDisposed) { return string.Empty; }

        var id = "<ID>" + GeneratedEntityID + "\r";
        id = id + "<TK>" + ais.GeneratedTextKey + "\r";
        id = id + "<CT>" + ais.Count.ToString() + "\r";
        id = id + "<RK>" + ais.RowKey + "\r";
        id = id + "<RH>" + ais.RowHash + "\r";

        return id;
    }

    #endregion
}