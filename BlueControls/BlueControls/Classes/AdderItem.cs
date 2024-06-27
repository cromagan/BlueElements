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

/// <summary>
/// Diese Element kennt den Schlüssel und alle Zeilen, die erzeugt werden müssen.
/// </summary>
internal class AdderItem : IReadableTextWithKey {

    #region Constructors

    public AdderItem(string generatedentityID, ColumnItem? originIDColumn, ColumnItem? additionalInfoColumn, string generatedTextKey) {
        GeneratedEntityID = generatedentityID;
        OriginIDColumn = originIDColumn;

        AdditionalInfoColumn = additionalInfoColumn;

        Indent = Math.Max(generatedTextKey.CountString("\\") - 1, 0);
        Last = generatedTextKey.TrimEnd("\\").FileNameWithSuffix();
        KeyName = generatedTextKey;
    }

    #endregion

    #region Properties

    public ColumnItem? AdditionalInfoColumn { get; }
    public string GeneratedEntityID { get; set; }
    public int Indent { get; private set; }

    public List<string> Infos { get; } = new List<string>();

    /// <summary>
    /// Enstpricht TextKey  (ZUTATEN\\MEHL\\) OHNE *
    /// </summary>
    public string KeyName { get; }

    public List<string> Keys { get; } = new List<string>();

    public string Last { get; private set; }
    public ColumnItem? OriginIDColumn { get; }
    public string QuickInfo => KeyName;

    #endregion

    #region Methods

    public void AddRowsToDatabase() {
        if (OriginIDColumn?.Database is not Database db || db.IsDisposed) { return; }

        for (var z = 0; z < Keys.Count; z++) {
            var oriid = OriginId(Keys[z]);
            if (!string.IsNullOrEmpty(oriid)) {
                var r = db.Row.GenerateAndAdd(GeneratedEntityID, null, "Zeilengenerator im Formular");

                if (r != null) {
                    OriginIDColumn.MaxCellLenght = Math.Max(OriginIDColumn.MaxCellLenght, oriid.Length);
                    OriginIDColumn.MultiLine = false;
                    OriginIDColumn.AfterEditAutoCorrect = false;
                    OriginIDColumn.AfterEditQuickSortRemoveDouble = false;
                    OriginIDColumn.ScriptType = ScriptType.String_Readonly;
                    OriginIDColumn.AdminInfo = "Diese Spalte wird als Erkennung für den Textgenerator benutzt.";
                    r.CellSet(OriginIDColumn, oriid, "Zeilengenerator im Formular");

                    if (AdditionalInfoColumn != null) {
                        AdditionalInfoColumn.MaxCellLenght = Math.Max(AdditionalInfoColumn.MaxCellLenght, Infos[z].Length);
                        AdditionalInfoColumn.MultiLine = false;
                        AdditionalInfoColumn.AfterEditAutoCorrect = false;
                        AdditionalInfoColumn.AfterEditQuickSortRemoveDouble = false;
                        AdditionalInfoColumn.ScriptType = ScriptType.String_Readonly;
                        AdditionalInfoColumn.AdminInfo = "Diese Spalte wird für Zusatzinfos de Textgenerators benutzt.";
                        r.CellSet(AdditionalInfoColumn, Infos[z], "Zeilengenerator im Formular");
                    }
                }
            }
        }
    }

    public string ReadableText() => new string(' ', Indent * 6) + Last;

    public QuickImage? SymbolForReadableText() => null;

    internal void RemoveRowsFromDatabase() {
        if (OriginIDColumn?.Database is not Database db || db.IsDisposed) { return; }

        foreach (var row in Keys) {
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