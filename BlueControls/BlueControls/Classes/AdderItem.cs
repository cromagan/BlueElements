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
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;

namespace BlueControls.Controls;

/// <summary>
/// Diese Element kennt den Schlüssel und alle Zeilen, die erzeugt werden müssen.
/// </summary>
internal class AdderItem : IReadableTextWithKey {

    #region Constructors

    public AdderItem(string generatedentityID, ColumnItem? originIDColumn, string generatedTextKey) {
        GeneratedEntityID = generatedentityID;
        OriginIDColumn = originIDColumn;

        Indent = Math.Max(generatedTextKey.CountString("\\") - 1, 0);
        Last = generatedTextKey.TrimEnd("\\").FileNameWithSuffix();
        KeyName = generatedTextKey;
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
    public List<string> Rows { get; } = new List<string>();

    #endregion

    #region Methods

    public void AddRowsToDatabase() {
        if (OriginIDColumn?.Database is not Database db || db.IsDisposed) { return; }

        foreach (var row in Rows) {
            var oriid = OriginId(row);
            if (!string.IsNullOrEmpty(oriid)) {
                var r = db.Row.GenerateAndAdd(GeneratedEntityID, null, "Zeilengenerator im Formular");

                if (r != null) {
                    OriginIDColumn.MaxCellLenght = Math.Max(OriginIDColumn.MaxCellLenght, oriid.Length);
                    OriginIDColumn.MultiLine = true;
                    OriginIDColumn.AfterEditAutoCorrect = false;
                    OriginIDColumn.AfterEditQuickSortRemoveDouble = false;
                    OriginIDColumn.ScriptType = ScriptType.String_Readonly;
                    OriginIDColumn.AdminInfo = "Diese Spalte wird als Erkennung für den Textgenerator benutzt.";
                    r.CellSet(OriginIDColumn, oriid, "Zeilengenerator im Formular");
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
            fi.Add(new FilterItem(OriginIDColumn, FilterType.Istgleich_UND_GroßKleinEgal, OriginId(row)));
            db.Row.Remove(fi, null, "Zeilengenerator im Formular");
            fi.Dispose();
        }
    }

    private string OriginId(string ais) {
        if (OriginIDColumn?.Database is not Database db || db.IsDisposed) { return string.Empty; }
        return GeneratedEntityID + "\\" + ais;
    }

    #endregion
}