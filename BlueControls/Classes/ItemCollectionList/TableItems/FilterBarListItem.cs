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
using BlueControls.Enums;
using BlueTable;
using BlueTable.Enums;
using System.Drawing;

namespace BlueControls.ItemCollectionList;

public sealed class FilterBarListItem : RowBackgroundListItem {

    #region Fields

    public const string Identifier = "FilterBarListItem";

    public static readonly int AutoFilterSize = 22;

    #endregion

    #region Constructors

    public FilterBarListItem(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) => IgnoreYOffset = true;

    #endregion

    #region Properties

    public FilterCollection? FilterCombined { get; set; }

    public BlueFont Font_Numbers {
        get {
            var baseFont = Skin.GetBlueFont(SheetStyle, PadStyles.Hervorgehoben);
            return BlueFont.Get(baseFont.FontName, baseFont.Size, false, false, false, false, Color.Black, Color.White, Color.Transparent);
        }
    }

    public BlueFont Font_TextInFilter {
        get {
            var baseFont = Skin.GetBlueFont(SheetStyle, PadStyles.Hervorgehoben);
            return BlueFont.Get(baseFont.FontName, baseFont.Size - 2, true, false, false, false, Color.White, Color.Red, Color.Transparent);
        }
    }

    public override string QuickInfo => string.Empty;

    public int RowsFilteredCount { get; set; }

    public bool ShowNumber { get; set; }

    #endregion

    #region Methods

    public override void Draw_LowerLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) => base.Draw_LowerLine(gr, ColumnLineStyle.Dick, left, right, bottom);

    public override void DrawColumn(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.DrawColumn(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        #region Trichter-Text && trichterState

        var trichterText = string.Empty;
        var trichterState = States.Undefiniert;
        var fi = FilterCombined?[viewItem.Column];

        if (viewItem.AutoFilterSymbolPossible) {
            if (fi != null) {
                trichterState = States.Checked;
                var anz = Autofilter_Text(viewItem);
                trichterText = anz > -100 ? (anz * -1).ToStringInt1() : "∞";
            } else {
                trichterState = States.Standard;
            }
        }

        #endregion

        #region Filter-Knopf mit Trichter

        QuickImage? trichterIcon = null;

        var siz = (int)positionControl.Height - 2;

        // Anpassen der Autofilter-CanvasPosition
        var origAutoFilterLocation = new Rectangle((int)positionControl.Right - siz, (int)positionControl.Top, siz, siz);

        var paf = positionControl.Height;

        var pts = (int)(positionControl.Height * 0.8);

        if (FilterCombined != null) {
            if (FilterCombined.HasAlwaysFalse() && viewItem.AutoFilterSymbolPossible) {
                trichterIcon = QuickImage.Get("Trichter|" + pts + "|||FF0000||170");
            } else if (fi != null) {
                trichterIcon = QuickImage.Get("Trichter|" + pts + "|||FF0000");
            } else if (FilterCombined.MayHaveRowFilter(viewItem.Column)) {
                trichterIcon = QuickImage.Get("Trichter|" + pts + "|||227722");
            } else if (viewItem.AutoFilterSymbolPossible) {
                trichterIcon = QuickImage.Get("Trichter|" + pts);
            }
        }

        if (trichterState != States.Undefiniert) {
            Skin.Draw_Back(gr, Design.Button_AutoFilter, trichterState, origAutoFilterLocation, null, false);
            Skin.Draw_Border(gr, Design.Button_AutoFilter, trichterState, origAutoFilterLocation);
        }

        if (trichterIcon != null) {
            var p2 = 2.CanvasToControl(scale);
            gr.DrawImage(trichterIcon, origAutoFilterLocation.Left + p2, origAutoFilterLocation.Top + p2);
        }

        if (!string.IsNullOrEmpty(trichterText)) {
            var Font_TextInFilter_Scaled = Font_TextInFilter.Scale(scale);
            var s = Font_TextInFilter_Scaled.MeasureString(trichterText);

            Font_TextInFilter_Scaled.DrawString(gr, trichterText,
                origAutoFilterLocation.Left + ((paf - s.Width) / 2),
                origAutoFilterLocation.Top + ((paf - s.Height) / 2));
        }

        #endregion

        #region LaufendeNummer

        if (ShowNumber && Arrangement != null) {
            Font_Numbers.Scale(scale).DrawString(gr, "#" + Arrangement.IndexOf(viewItem), positionControl.X, positionControl.Top);
        }

        #endregion
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => AutoFilterSize + 2;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(AutoFilterSize, AutoFilterSize + 2);

    private int Autofilter_Text(ColumnViewItem viewItem) {
        if (IsDisposed) { return 0; }

        // Cache nutzen für bessere Performance
        if (viewItem.TmpIfFilterRemoved != null) { return (int)viewItem.TmpIfFilterRemoved; }

        // Optimierung: FilterCombined nur klonen, wenn notwendig
        // Überprüfen, ob überhaupt ein Filter für die Spalte existiert
        if (FilterCombined?[viewItem.Column] is not { }) {
            viewItem.TmpIfFilterRemoved = 0;
            return 0;
        }

        using var fc = (FilterCollection)FilterCombined.Clone("Autofilter_Text");
        fc.Remove(viewItem.Column);

        var filterDifference = RowsFilteredCount - fc.Rows.Count;
        viewItem.TmpIfFilterRemoved = filterDifference;
        return filterDifference;
    }

    #endregion
}