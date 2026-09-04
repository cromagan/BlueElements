// Licensed under AGPL-3.0; see License.md for disclaimer and details.


// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;
using System.Windows.Forms;

namespace BlueControls.TableElements;

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public sealed class NewRowTableElement : TableElement {

    #region Fields

    public const string Identifier = "NewRowTableElement";
    public static readonly Brush NewRowOverlayBrush = new SolidBrush(Color.FromArgb(150, 255, 255, 255));

    #endregion

    #region Constructors

    public NewRowTableElement(ColumnViewCollection? arrangement) : base(Identifier, arrangement, string.Empty) => IgnoreYOffset = true;

    #endregion

    #region Properties

    public FilterCollection? FilterCombined { get; set; }
    protected override bool DoSpezialOrder => true;

    #endregion

    #region Methods

    public override void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state, Brush? rowcolor) {
        base.Draw_ColumnBackGround(gr, viewItem, positionControl, state, rowcolor);

        RowTableElement.ColumnBackGround(gr, viewItem, positionControl);
    }

    public override void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
        base.Draw_ColumnContent(gr, viewItem, positionControl, scale, translate, offsetX, offsetY, state);

        if (viewItem.Column is null) { return; }

        //var p14 = 14.CanvasToControl(scale);
        //var p1 = 1.CanvasToControl(scale);

        string toDrawd;
        var doWhiteAfter = true;
        //QuickImage? plusszeichen;
        if (viewItem.Column.IsFirst) {
            toDrawd = "[Neue Zeile]";
            //plusszeichen = QuickImage.Get(ImageCode.PlusZeichen, p14);
            doWhiteAfter = false;
        } else {
            toDrawd = FilterCollection.InitValue(viewItem.Column, false, false, FilterCombined is { } fc ? [.. fc] : []) ?? string.Empty;
            //plusszeichen = QuickImage.Get(ImageCode.PlusZeichen, p14, Color.Transparent, Color.Transparent, 200);
        }

        if (!doWhiteAfter) {
            gr.FillRectangle(NewRowOverlayBrush, positionControl);
        }

        if (!string.IsNullOrEmpty(toDrawd)) {
            gr.SetClip(positionControl);
            viewItem.GetRenderer(SheetStyle).Draw(gr, toDrawd, null, positionControl.ToRect(), translate, (Alignment)viewItem.Column.Align, scale, Design.Item_ListBox, States.Standard);

            //gr.DrawImage(plusszeichen, new Point((int)positionControl.Left + p1, (int)positionControl.Top + p1));
            gr.ResetClip();
        }

        if (doWhiteAfter) {
            gr.FillRectangle(NewRowOverlayBrush, positionControl);
        }
    }

    public override void Draw_ColumnOverlay(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        base.Draw_ColumnOverlay(gr, viewItem, positionControl, state);

        RowTableElement.ColumnOverlay(gr, viewItem, positionControl);
    }

    /// <summary>
    /// Startet die Inline-Editierung für das Anlegen einer neuen Zeile.
    /// Ist die ChunkValue-Spalte die erste Spalte, wird der ChunkValue null
    /// übergeben (neue Zeile wird später erzeugt); sonst der aktuelle
    /// FilterChunkValue, damit die neue Zeile im aktuellen Chunk landet.
    /// Die eigentliche Edit-Logik liegt in
    /// <see cref="TableElement.BeginCellEdit" />.
    /// </summary>
    public override bool HandleDoubleClick(ColumnViewItem? mouseOverColumn, TableView tableView) {
        if (mouseOverColumn is null) { return false; }
        if (Arrangement?.Table is not { IsDisposed: false } tb) { return false; }

        var chunkValue = tb.Column.ChunkValueColumn == tb.Column.First
            ? null
            : FilterCombined?.ChunkVal;
        return BeginCellEdit(tableView, mouseOverColumn, this, null, chunkValue);
    }

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => UntrimmedCanvasSize(itemdesign).Height;

    public override void HandleMouseMove(ColumnViewItem? mouseOverColumn, TableView tableView, CanvasMouseEventArgs e) {
        if (mouseOverColumn is not { IsDisposed: false } cvi || e.Button != MouseButtons.None) {
            base.HandleMouseMove(mouseOverColumn, tableView, e);
            return;
        }

        tableView.QuickInfo = cvi.Column is not { IsDisposed: false }
            ? string.Empty
            : cvi.Column.IsFirst ? "Neue Zeile anlegen" : RowTableElement.QuickInfoText(cvi.Column, "Neue Zeile");
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => new(18, 18);

    #endregion
}