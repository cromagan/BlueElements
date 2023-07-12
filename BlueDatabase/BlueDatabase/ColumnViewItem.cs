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

using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using static BlueBasics.Converter;

namespace BlueDatabase;

public sealed class ColumnViewItem {

    #region Fields

    private ViewType _viewType;

    #endregion

    #region Constructors

    public ColumnViewItem(ColumnItem column, ViewType type, ColumnViewCollection parent) : this(parent) {
        Column = column;
        _viewType = type;
    }

    public ColumnViewItem(DatabaseAbstract database, string toParse, ColumnViewCollection parent) : this(parent) {
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "column":

                case "columnname":// ColumnName wichtg, wegen CopyLayout
                    Column = database.Column.Exists(pair.Value);
                    Column?.Repair(); // Alte Formate reparieren
                    break;

                case "columnkey":
                    //Column = database.Column.SearchByKey(LongParse(pair.Value));
                    //Column?.Repair(); // Alte Formate reparieren
                    break;

                case "permanent": // Todo: Alten Code Entfernen, Permanent wird nicht mehr verstringt 06.09.2019
                    _viewType = ViewType.PermanentColumn;
                    break;

                case "type":
                    _viewType = (ViewType)IntParse(pair.Value);
                    break;

                case "edittype":
                    //    _editType = (EditTypeFormula)IntParse(pair.Value);
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }

        if (Column != null && _viewType == ViewType.None) { _viewType = ViewType.Column; }
    }

    private ColumnViewItem(ColumnViewCollection parent) : base() {
        Parent = parent;
        _viewType = ViewType.None;
        Column = null;
        OrderTmpSpalteX1 = null;
        TmpAutoFilterLocation = Rectangle.Empty;
        TmpReduceLocation = Rectangle.Empty;
        TmpDrawWidth = null;
        TmpReduced = false;
    }

    #endregion

    #region Properties

    public ColumnItem? Column { get; }
    public int? OrderTmpSpalteX1 { get; set; }
    public ColumnViewCollection Parent { get; }

    public Rectangle TmpAutoFilterLocation { get; set; }
    public int? TmpDrawWidth { get; set; }
    public bool TmpReduced { get; set; }
    public Rectangle TmpReduceLocation { get; set; }

    public ViewType ViewType {
        get => _viewType;
        set {
            if (value == _viewType) { return; }
            _viewType = value;
        }
    }

    #endregion

    #region Methods

    public void Invalidate_DrawWidth() => TmpDrawWidth = null;

    public ColumnViewItem? NextVisible() => Parent.NextVisible(this);

    public ColumnViewItem? PreviewsVisible() => Parent.PreviousVisible(this);

    public override string ToString() {
        var result = "{Type=" + (int)_viewType;
        if (Column != null && !Column.IsDisposed) { result = result + ", ColumnName=" + Column.Name; }
        return result + "}";
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