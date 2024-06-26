﻿// Authors:
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
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;

namespace BlueControls.Controls;

/// <summary>
/// Diese Element kennt den Schlüssel und alle Zeilen, die erzeugt werden müssen.
/// </summary>
internal class AdderItem : IReadableTextWithKey {

    #region Constructors

    public AdderItem(string generatedentityID, ColumnItem? originIDColumn, string generatedTextKey, RowItem uniqueRow) {
        GeneratedEntityID = generatedentityID;
        OriginIDColumn = originIDColumn;
        UniqueRow = uniqueRow;

        Indent = Math.Max(generatedTextKey.CountString("\\") - 1, 0);
        Last = generatedTextKey.TrimEnd("\\").FileNameWithSuffix();
        KeyName = generatedTextKey.ToUpper();
    }

    #endregion

    #region Properties

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
    public RowItem UniqueRow { get; set; }

    #endregion

    #region Methods

    public void AddRowsToDatabase() {
        if (OriginIDColumn?.Database is not Database db || db.IsDisposed) { return; }

        foreach (var row in Rows) {
            if (row.Columns == null || row.Columns.Count == 0) { continue; }

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
                        "Sie darf weder gelesen noch verändert werden.";
                    r.CellSet(OriginIDColumn, oriid, "Zeilengenerator im Formular");

                    foreach (var thisColumn in row.Columns) {
                        if (!string.IsNullOrWhiteSpace(thisColumn.ReplaceableText)) {
                            r.CellSet(thisColumn.Column, thisColumn.ReplacedText(row.Row, UniqueRow), "Zeilengenerator im Formular");
                        }
                    }
                }
            }
        }
    }

    public string ReadableText() => new string(' ', Indent * 6) + Last;

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
            //        r.CellSet(AdditionalTextColumn, row.Additionaltext, "Zeilengenerator im Formular");
            //    }
            //}
        }
    }

    private string OriginId(AdderItemSingle ais) {
        if (OriginIDColumn?.Database is not Database db || db.IsDisposed) { return string.Empty; }

        var id = "<ID>" + GeneratedEntityID + "\r";
        id = id + "<TK>" + ais.GeneratedTextKey + "\r";
        id = id + "<CT>" + ais.Count + "\r";
        id = id + "<RK>" + ais.Row.KeyName + "\r";
        id = id + "<RH>" + ais.Row.Hash() + "\r";

        return id;
    }

    #endregion
}