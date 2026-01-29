// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using BlueControls.Controls;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.Abstract;
using BlueTable;
using BlueTable.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad.FunktionsItems_ColumnArrangement_Editor;

public class DummyHeadPadItem : FixedRectanglePadItem, IHasTable {

    #region Constructors

    public DummyHeadPadItem(Table table) : base(string.Empty) {
        Table = table;
        CanvasSize = new SizeF(10, 10);
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-DummyHead";
    public ReadOnlyCollection<string> Ausführbare_Skripte { get; set; } = new([]);

    public string Chapter_Column { get; set; } = string.Empty;
    public override string Description => string.Empty;
    public ReadOnlyCollection<string> Filter_immer_Anzeigen { get; set; } = new([]);
    public int FilterRows { get; set; }
    public ReadOnlyCollection<string> Kontextmenu_Skripte { get; set; } = new([]);
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

        var chapterColumns = ItemsOf(tb.Column, true);
        chapterColumns.Add(ItemOf("Keine Überschriften", "#ohne", ImageCode.Kreuz, true, "!!!"));

        var filterColumns = ItemsOf(tb.Column, true);

        var scriptAll = new List<AbstractListItem>();
        var scriptRow = new List<AbstractListItem>();

        foreach (var thisScript in tb.EventScript.Where(s => s.UserGroups.Count > 0)) {
            scriptAll.Add(ItemOf(thisScript));

            if (thisScript.NeedRow) {
                scriptRow.Add(ItemOf(thisScript));
            }
        }

        List<GenericControl> result =
        [
            new FlexiControlForDelegate(tb),
            new FlexiControl(),
            new FlexiControlForProperty<bool>(() => ShowHead),
            new FlexiControlForProperty<int>(() => FilterRows),
            new FlexiControlForProperty<string>(() => Chapter_Column, chapterColumns ),
            new FlexiControlForProperty<string>(() => QuickInfo, 3 ),
            new FlexiControlForProperty<ReadOnlyCollection<string>>(() => Filter_immer_Anzeigen, "Filter immer anzeigen von", 6, filterColumns, Enums.CheckBehavior.AllSelected, Enums.AddType.OnlySuggests, System.Windows.Forms.ComboBoxStyle.DropDownList, false ),
            new FlexiControlForProperty<ReadOnlyCollection<string>>(() => Ausführbare_Skripte, "Ausführbare Skripte",6, scriptAll, Enums.CheckBehavior.AllSelected,Enums.AddType.OnlySuggests, System.Windows.Forms.ComboBoxStyle.DropDownList, false ),
            new FlexiControlForProperty<ReadOnlyCollection<string>>(() => Kontextmenu_Skripte, "Kontextmenu ersetzen mit",6, scriptRow, Enums.CheckBehavior.AllSelected,Enums.AddType.OnlySuggests, System.Windows.Forms.ComboBoxStyle.DropDownList, false )
            ];

        return result;
    }

    public override string ReadableText() => "ColumnArrangement";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Spalte, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) { }

    #endregion
}