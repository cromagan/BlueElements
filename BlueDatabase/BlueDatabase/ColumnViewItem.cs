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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Constants;
using static BlueBasics.Converter;

namespace BlueDatabase;

public sealed class ColumnViewItem : IParseable {

    #region Constructors

    public ColumnViewItem(ColumnItem column, ViewType type, ColumnViewCollection parent) : this(parent) {
        Column = column;
        ViewType = type;
    }

    public ColumnViewItem(ColumnViewCollection parent, string toParse) : this(parent) => this.Parse(toParse);

    private ColumnViewItem(ColumnViewCollection parent) : base() {
        Parent = parent;
        ViewType = ViewType.None;
        Column = null;
        X_WithSlider = null;
        X = null;
        TmpAutoFilterLocation = Rectangle.Empty;
        TmpReduceLocation = Rectangle.Empty;
        TmpDrawWidth = null;
        TmpReduced = false;
    }

    #endregion

    #region Properties

    public ColumnItem? Column { get; private set; }

    public Rectangle TmpAutoFilterLocation { get; set; }

    public int? TmpDrawWidth { get; set; }

    public bool TmpReduced { get; set; }

    public Rectangle TmpReduceLocation { get; set; }

    public ViewType ViewType { get; set; }

    /// <summary>
    /// Koordinate der Spalte ohne Slider
    public int? X { get; set; }

    /// <summary>
    /// Koordinate der Spalte mit einbrechneten Slider
    /// </summary>
    public int? X_WithSlider { get; set; }

    private ColumnViewCollection Parent { get; }

    #endregion

    #region Methods

    public static int CalculateColumnContentWidth(ColumnItem column, Font cellFont, int pix16) {
        if (column.IsDisposed) { return 16; }
        if (column.Database is not { IsDisposed: false } db) { return 16; }
        if (column.FixedColumnWidth > 0) { return column.FixedColumnWidth; }
        if (column.Contentwidth is { } v) { return v; }

        column.RefreshColumnsData();

        var newContentWidth = 16; // Wert muss gesetzt werden, dass er am Ende auch gespeichert wird

        try {
            //  Parallel.ForEach führt ab und zu zu DeadLocks
            foreach (var thisRowItem in db.Row) {
                var wx = CellItem.ContentSize(column, thisRowItem, cellFont, pix16).Width;
                newContentWidth = Math.Max(newContentWidth, wx);
            }
        } catch {
            Develop.CheckStackForOverflow();
            return CalculateColumnContentWidth(column, cellFont, pix16);
        }

        column.Contentwidth = newContentWidth;
        return newContentWidth;
    }

    public int DrawWidth(Rectangle displayRectangleWoSlider, int pix16, Font cellFont) {
        // Hier wird die ORIGINAL-Spalte gezeichnet, nicht die FremdZelle!!!!

        if (Column == null) { return 0; }
        if (TmpDrawWidth is { } v) { return v; }

        if (Parent.Count == 1) {
            TmpDrawWidth = displayRectangleWoSlider.Width;
            return displayRectangleWoSlider.Width;
        }

        if (TmpReduced) {
            TmpDrawWidth = pix16;
        } else {
            TmpDrawWidth = ViewType == ViewType.PermanentColumn
                ? Math.Min(CalculateColumnContentWidth(Column, cellFont, pix16), (int)(displayRectangleWoSlider.Width * 0.3))
                : Math.Min(CalculateColumnContentWidth(Column, cellFont, pix16), (int)(displayRectangleWoSlider.Width * 0.6));
        }

        TmpDrawWidth = Math.Max((int)TmpDrawWidth, AutoFilterSize); // Mindestens so groß wie der Autofilter;

        //TmpDrawWidth = Math.Max((int)TmpDrawWidth, (int)Column.ColumnCaptionText_Size(columnFont).Width);
        return (int)TmpDrawWidth;
    }

    public void Invalidate_DrawWidth() => TmpDrawWidth = null;

    public ColumnViewItem? NextVisible() => Parent.NextVisible(this);

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        if (Parent.Database is not { IsDisposed: false } db) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbank unbekannt");
            return false;
        }

        switch (key) {
            case "column":
            case "columnname":// ColumnName wichtg, wegen CopyLayout
                Column = db.Column[value];
                //Column?.Repair(); // Alte Formate reparieren
                return true;

            case "columnkey":
                //Column = database.Column.SearchByKey(LongParse(value));
                //Column?.Repair(); // Alte Formate reparieren
                return true;

            case "permanent": // Todo: Alten Code Entfernen, Permanent wird nicht mehr verstringt 06.09.2019
                ViewType = ViewType.PermanentColumn;
                return true;

            case "type":
                ViewType = (ViewType)IntParse(value);
                if (Column != null && ViewType == ViewType.None) { ViewType = ViewType.Column; }
                return true;

            case "edittype":
                //    _editType = (EditTypeFormula)IntParse(value);
                return true;
        }

        return false;
    }

    public ColumnViewItem? PreviewsVisible() => Parent.PreviousVisible(this);

    public string ToParseableString() {
        var result = new List<string>();
        result.ParseableAdd("Type", ViewType);
        result.ParseableAdd("ColumnName", Column);
        return result.Parseable();
    }

    public override string ToString() => ToParseableString();

    #endregion
}