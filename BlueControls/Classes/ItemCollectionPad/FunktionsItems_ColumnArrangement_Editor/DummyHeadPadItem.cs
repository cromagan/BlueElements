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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.BlueTableDialogs;
using BlueControls.CellRenderer;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.ItemCollectionPad.Abstract;
using BlueTable;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.ItemCollectionPad.FunktionsItems_ColumnArrangement_Editor;

/// <summary>
/// Zum Darstellen einer Spalte. Im ViewEditpt benutzt
/// </summary>
public class DummyHeadPadItem : FixedRectanglePadItem, IHasTable {

    #region Constructors

    public DummyHeadPadItem(Table table) : base(string.Empty) {
        Table = table;
        CanvasSize = new SizeF(10, 10);
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-DummyHead";

    public override string Description => string.Empty;

    public int FilterRows { get; set; }
    public bool ShowHead { get; set; }
    public Table? Table { get; private set; }

    /// <summary>
    /// Wird von Flexoptions aufgerufen
    /// </summary>
    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    //        if (value == Permanent) { return; }
    public override List<GenericControl> GetProperties(int widthOfControl) {
        if (Table is not { IsDisposed: false } tb) { return []; }

        tb.Editor = typeof(TableHeadEditor);

        List<GenericControl> result =
        [
            new FlexiDelegateControl(tb.Edit, "Tabelle: " + tb.Caption, ImageCode.Tabelle),
            new FlexiControl(),
            new FlexiControlForProperty<bool>(() => ShowHead),
            new FlexiControlForProperty<int>(() => FilterRows)
        ];

        return result;
    }

    public override string ReadableText() => "ColumnArrangement";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Spalte, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY) { }

    #endregion
}