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

    public AdderItem(ColumnItem entityIDColumn, string generatedentityID, ColumnItem? originIDColumn, ColumnItem textKeyColumn, string generatedTextKeyWOAsterix, ColumnItem? additinalTextColumn) {
        EntityIDColumn = entityIDColumn;
        GeneratedentityID = generatedentityID;
        OriginIDColumn = originIDColumn;
        TextKeyColumn = textKeyColumn;
        KeyName = generatedTextKeyWOAsterix.ToUpper();
        KeyNameOri = generatedTextKeyWOAsterix;
        AdditinalTextColumn = additinalTextColumn;
    }

    #endregion

    #region Properties

    public ColumnItem? AdditinalTextColumn { get; }

    public ColumnItem EntityIDColumn { get; }

    public string GeneratedentityID { get; set; }

    /// <summary>
    /// Enstpricht TextKey  (ZUTATEN\\MEHL\\) OHNE *
    /// </summary>
    public string KeyName { get; }



    /// <summary>
    /// Enstpricht TextKey  (Zutaten\\Mehl\\) OHNE *
    /// </summary>
    public string KeyNameOri { get; }

    public ColumnItem? OriginIDColumn { get; }

    public string QuickInfo => KeyName;

    public List<AdderItemSingle> Rows { get; } = new List<AdderItemSingle>();

    public ColumnItem TextKeyColumn { get; }

    #endregion

    #region Methods

    public void AddRowsToDatabase() {
        if (EntityIDColumn?.Database is not Database db || db.IsDisposed) { return; }
        if(OriginIDColumn ==  null) { return; }
        if (TextKeyColumn == null) { return; }

        foreach (var row in Rows) {
            var oriid = OriginId(row);

            if (!string.IsNullOrEmpty(oriid)) {
                var r = db.Row.GenerateAndAdd(GeneratedentityID, null, "Zeilengenerator im Formular");

                if (r != null) {

                    EntityIDColumn.MaxCellLenght = Math.Max(EntityIDColumn.MaxCellLenght, GeneratedentityID.Length);
                    r.CellSet(EntityIDColumn, GeneratedentityID, "Zeilengenerator im Formular");

                    OriginIDColumn.MaxCellLenght = Math.Max(OriginIDColumn.MaxCellLenght, oriid.Length);
                    r.CellSet(OriginIDColumn, oriid, "Zeilengenerator im Formular");

                    TextKeyColumn.MaxCellLenght = Math.Max(TextKeyColumn.MaxCellLenght, row.GeneratedTextKey.Length);
                    r.CellSet(TextKeyColumn, row.GeneratedTextKey, "Zeilengenerator im Formular");

                    if(AdditinalTextColumn != null) {
                        AdditinalTextColumn.MaxCellLenght = Math.Max(AdditinalTextColumn.MaxCellLenght, row.Additionaltext.Length);
                        r.CellSet(AdditinalTextColumn, row.Additionaltext, "Zeilengenerator im Formular");

                    }

                }
            }
        }
    }

    public string ReadableText() {
        var t = Math.Max(KeyNameOri.CountString("\\") - 1, 0);

        return new string(' ', t * 6) + KeyNameOri.TrimEnd("\\").FileNameWithSuffix();
    }

    public QuickImage? SymbolForReadableText() => null;

    internal void RemoveRowsFromDatabase() {
        if (EntityIDColumn.Database is not Database db || db.IsDisposed) { return; }

        foreach (var row in Rows) {
            var fi = new FilterCollection(db, "Zeilengenerator im Formular");

            fi.Add(new FilterItem(EntityIDColumn, FilterType.Istgleich_GroßKleinEgal, GeneratedentityID));
            fi.Add(new FilterItem(TextKeyColumn, FilterType.Istgleich_GroßKleinEgal, row.GeneratedTextKey));

            db.Row.Remove(fi, null, "Zeilengenerator im Formular");
            fi.Dispose();

            //var oriid = OriginId(row);

            //if (!string.IsNullOrEmpty(oriid)) {
            //    var r = db.Row.GenerateAndAdd(GeneratedentityID, null, "Zeilengenerator im Formular");

            //    if (r != null) {
            //        r.CellSet(EntityIDColumn, GeneratedentityID, "Zeilengenerator im Formular");
            //        r.CellSet(OriginIDColumn, oriid, "Zeilengenerator im Formular");
            //        r.CellSet(TextKeyColumn, row.GeneratedTextKey, "Zeilengenerator im Formular");
            //        r.CellSet(AdditinalTextColumn, row.Additionaltext, "Zeilengenerator im Formular");
            //    }
            //}
        }
    }

    private string OriginId(AdderItemSingle ais) {
        if (EntityIDColumn.Database is not Database db || db.IsDisposed) { return string.Empty; }

        var hash = ais.ThisRow.Hash();

        if (string.IsNullOrEmpty(hash)) { return string.Empty; }

        var id = EntityIDColumn.Database.Caption + ">";
        id = id + ais.Count.ToString() + ">";
        id = id + ais.GeneratedTextKey + ">";
        id = id + hash;

        return id;
    }

    #endregion
}