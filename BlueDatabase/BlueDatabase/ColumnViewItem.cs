// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
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
        OrderTmpSpalteX1 = null;
        TmpAutoFilterLocation = Rectangle.Empty;
        TmpReduceLocation = Rectangle.Empty;
        TmpDrawWidth = null;
        TmpReduced = false;
    }

    #endregion

    #region Properties

    public ColumnItem? Column { get; private set; }
    public int? OrderTmpSpalteX1 { get; set; }
    public ColumnViewCollection Parent { get; }
    public Rectangle TmpAutoFilterLocation { get; set; }
    public int? TmpDrawWidth { get; set; }
    public bool TmpReduced { get; set; }
    public Rectangle TmpReduceLocation { get; set; }

    public ViewType ViewType { get; set; }

    #endregion

    #region Methods

    public void Invalidate_DrawWidth() => TmpDrawWidth = null;

    public ColumnViewItem? NextVisible() => Parent.NextVisible(this);

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        if (Parent.Database is not DatabaseAbstract db) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbank unbekannt");
            return false;
        }

        switch (key) {
            case "column":
            case "columnname":// ColumnName wichtg, wegen CopyLayout
                Column = db.Column.Exists(value);
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

    public override string ToString() {
        var result = new List<string>();
        result.ParseableAdd("Type", ViewType);
        result.ParseableAdd("ColumnName", Column);
        return result.Parseable();
    }

    internal bool NonPermanentPossible() {
        //if (_arrangementNr < 1) {
        //    return !thisViewItem.Column.IsFirst();
        //}
        var nx = NextVisible();
        return nx == null || nx.ViewType != ViewType.PermanentColumn;
    }

    internal bool PermanentPossible() {
        //if (_arrangementNr < 1) {
        //    return thisViewItem.Column.IsFirst();
        //}
        var prev = PreviewsVisible();
        return prev == null || prev.ViewType == ViewType.PermanentColumn;
    }

    #endregion
}