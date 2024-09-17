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
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.Temporär;
using BlueControls.ItemCollectionPad;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class PadEditor : FormWithStatusBar {

    #region Constructors

    protected PadEditor() : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        PadDesign.ItemClear();
        PadDesign.ItemAddRange(ItemsOf(Skin.AllStyles()));
        PadDesign.Text = PadDesign[0]?.KeyName ?? string.Empty;

        if (Pad?.Items != null && Skin.StyleDb != null) {
            Pad.Items.SheetStyle = Skin.StyleDb.Row[PadDesign.Text];
        }

        cbxSchriftGröße.ItemAdd(ItemOf("30%", "030"));
        cbxSchriftGröße.ItemAdd(ItemOf("40%", "040"));
        cbxSchriftGröße.ItemAdd(ItemOf("50%", "050"));
        cbxSchriftGröße.ItemAdd(ItemOf("60%", "060"));
        cbxSchriftGröße.ItemAdd(ItemOf("70%", "070"));
        cbxSchriftGröße.ItemAdd(ItemOf("80%", "080"));
        cbxSchriftGröße.ItemAdd(ItemOf("90%", "090"));
        cbxSchriftGröße.ItemAdd(ItemOf("100%", "100"));
        cbxSchriftGröße.ItemAdd(ItemOf("110%", "110"));
        cbxSchriftGröße.ItemAdd(ItemOf("120%", "120"));
        cbxSchriftGröße.ItemAdd(ItemOf("130%", "130"));
        cbxSchriftGröße.ItemAdd(ItemOf("140%", "140"));
        cbxSchriftGröße.ItemAdd(ItemOf("150%", "150"));
        cbxSchriftGröße.Text = "100";
    }

    #endregion

    #region Methods

    public void ItemChanged() {
        Pad.ZoomFit();

        if (Pad?.Items?.SheetStyle != null) {
            PadDesign.Text = Pad.Items.SheetStyle.CellFirstString();
            cbxSchriftGröße.Text = ((int)(Pad.Items.SheetStyleScale * 100)).ToStringInt3();
        }
    }

    private void btnAddDimension_Click(object sender, System.EventArgs e) {
        DimensionPadItem b = new(new PointF(300, 300), new PointF(400, 300), 30);
        Pad.AddCentered(b);
    }

    private void btnAddDynamicSymbol_Click(object sender, System.EventArgs e) {
        DynamicSymbolPadItem b = new();
        b.SetCoordinates(new RectangleF(100, 100, 300, 300), true);
        Pad.AddCentered(b);
    }

    private void btnAddImage_Click(object sender, System.EventArgs e) {
        BitmapPadItem b = new(string.Empty, QuickImage.Get(ImageCode.Fragezeichen), new Size(1000, 1000));
        Pad.AddCentered(b);
    }

    private void btnAddLine_Click(object sender, System.EventArgs e) {
        var p = Pad.MiddleOfVisiblesScreen();
        var w = (int)(300 / Pad.Zoom);
        LinePadItem b = new(PadStyles.Style_Standard, p with { X = p.X - w }, p with { X = p.X + w });
        Pad.AddCentered(b);
    }

    private void btnAddPhsyik_Click(object sender, System.EventArgs e) {
        PhysicPadItem b = new();
        //b.SetCoordinates(new RectangleF(100, 100, 300, 300));
        Pad.AddCentered(b);
    }

    private void btnAddSymbol_Click(object sender, System.EventArgs e) {
        SymbolPadItem b = new();
        b.SetCoordinates(new RectangleF(100, 100, 300, 300), true);
        Pad.AddCentered(b);
    }

    private void btnAddText_Click(object sender, System.EventArgs e) {
        TextPadItem b = new() {
            Text = string.Empty,
            Stil = PadStyles.Style_Standard
        };
        Pad.AddCentered(b);
        b.SetCoordinates(new RectangleF(10, 10, 200, 200), true);
    }

    private void btnAddUnterStufe_Click(object sender, System.EventArgs e) {
        ChildPadItem b = new();
        b.SetCoordinates(new RectangleF(100, 100, 300, 300), true);
        Pad.AddCentered(b);
    }

    private void btnArbeitsbreichSetup_Click(object sender, System.EventArgs e) => Pad.ShowWorkingAreaSetup();

    private void btnHintergrundFarbe_Click(object sender, System.EventArgs e) {
        if (Pad?.Items == null) { return; }

        ColorDia.Color = Pad.Items.BackColor;
        _ = ColorDia.ShowDialog();
        Pad.Items.BackColor = ColorDia.Color;
        Pad.Invalidate();
    }

    private void btnKeinHintergrund_Click(object sender, System.EventArgs e) {
        if (Pad?.Items == null) { return; }

        Pad.Items.BackColor = Color.Transparent;
        Pad.Invalidate();
    }

    private void cbxSchriftGröße_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (Pad?.Items == null) { return; }
        Pad.Items.SheetStyleScale = FloatParse(cbxSchriftGröße.Text) / 100f;
    }

    private void ckbRaster_CheckedChanged(object sender, System.EventArgs e) {
        if (Pad?.Items == null) { return; }
        Pad.Items.SnapMode = ckbRaster.Checked ? SnapMode.SnapToGrid : SnapMode.Ohne;
    }

    private void LastClickedItem_DoUpdateSideOptionMenu(object sender, System.EventArgs e) => Pad.LastClickedItem.DoForm(tabElementEigenschaften);

    private void Pad_ClickedItemChanged(object sender, System.EventArgs e) {
        // FALLS ein PadEditor doppelt offen ist, kann ein Control Element aber nur
        // einem Parent zugeordnet werden.
        // Deswegen müssen die Element einzigartig sein (also extra für das Menü generiert werden)
        // Und deswegen können sie auch disposed werden.

        if (Pad.LastClickedItem != null) {
            Pad.LastClickedItem.DoUpdateSideOptionMenu += LastClickedItem_DoUpdateSideOptionMenu;
        }

        LastClickedItem_DoUpdateSideOptionMenu(this, System.EventArgs.Empty);
    }

    private void Pad_ClickedItemChanging(object sender, System.EventArgs e) {
        if (Pad.LastClickedItem != null) {
            Pad.LastClickedItem.DoUpdateSideOptionMenu -= LastClickedItem_DoUpdateSideOptionMenu;
        }
    }

    private void Pad_GotNewItemCollection(object sender, System.EventArgs e) {
        btnVorschauModus.Checked = Pad.ShowInPrintMode;

        DoPages();


        ckbRaster.Enabled = Pad.Items != null;
        txbRasterAnzeige.Enabled = Pad.Items != null;
        txbRasterFangen.Enabled = Pad.Items != null;

        if (Pad.Items != null) {
            ckbRaster.Checked = Pad.Items.SnapMode == SnapMode.SnapToGrid;
            txbRasterAnzeige.Text = Pad.Items.GridShow.ToStringFloat2();
            txbRasterFangen.Text = Pad.Items.GridSnap.ToStringFloat2();
            if (Pad.Items.SheetStyle != null) {
                PadDesign.Text = Pad.Items.SheetStyle.CellFirstString();
            }

            cbxSchriftGröße.Text = ((int)(Pad.Items.SheetStyleScale * 100)).ToStringInt3();
        }
    }

    private void PadDesign_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (Pad?.Items != null && Skin.StyleDb?.Row != null) {
            Pad.Items.SheetStyle = Skin.StyleDb.Row[e.Item.KeyName];
        }
    }

    private void txbRasterAnzeige_TextChanged(object sender, System.EventArgs e) {
        if (!txbRasterAnzeige.Text.IsNumeral()) { return; }
        if (!txbRasterAnzeige.Visible) { return; }
        if (Pad?.Items == null) { return; }
        Pad.Items.GridShow = FloatParse(txbRasterAnzeige.Text);
    }

    private void txbRasterFangen_TextChanged(object sender, System.EventArgs e) {
        if (!txbRasterFangen.Text.IsNumeral()) { return; }
        if (!txbRasterFangen.Visible) { return; }
        if (Pad?.Items == null) { return; }
        Pad.Items.GridSnap = FloatParse(txbRasterFangen.Text);
    }

    #endregion

    private void btnWeitereAllItem_Click(object sender, System.EventArgs e) {


        var l = Generic.GetInstaceOfType<AbstractPadItem>();

        if (l.Count == 0) { return; }

        var i = new List<AbstractListItem>();

        foreach (var thisl in l) {
            i.Add(ItemOf(thisl));
        }

        var x = InputBoxListBoxStyle.Show("Hinzufügen:", i, Enums.CheckBehavior.SingleSelection, null, Enums.AddType.None);

        if (x is not { Count: 1 }) { return; }

        var toadd = i.Get(x[0]);

        if (toadd is not ReadableListItem { Item: AbstractPadItem api }) { return; }

        //if (toadd is not AbstractPadItem api) {  return; }

        //var x = new FileExplorerPadItem(string.Empty);

        Pad.AddCentered(api);


    }


    #region Constructors


    #endregion

    #region Methods

    private void btnAlsBildSpeichern_Click(object sender, System.EventArgs e) => Pad.OpenSaveDialog(string.Empty);

    private void btnDruckerDialog_Click(object sender, System.EventArgs e) => Pad.Print();

    private void btnPageSetup_Click(object sender, System.EventArgs e) => Pad.ShowPrinterPageSetup();

    private void btnVorschau_Click(object sender, System.EventArgs e) => Pad.ShowPrintPreview();

    private void btnVorschauModus_CheckedChanged(object sender, System.EventArgs e) => Pad.ShowInPrintMode = btnVorschauModus.Checked;

    private void btnZoom11_Click(object sender, System.EventArgs e) => Pad.Zoom = 1f;

    private void btnZoomFit_Click(object sender, System.EventArgs e) => Pad.ZoomFit();

    private void DoPages() {
        if (InvokeRequired) {
            _ = Invoke(new Action(DoPages));
            return;
        }

        try {
            if (IsDisposed) { return; }

            var x = new List<string>();

            if (Pad?.Items != null) { x.AddRange(Pad.Items.AllPages()); }

            if (Pad != null) { _ = x.AddIfNotExists(Pad.CurrentPage); }

            TabPage? later = null;

            if (x.Count == 1 && string.IsNullOrEmpty(x[0])) { x.Clear(); }

            if (x.Count > 0) {
                tabSeiten.Visible = true;

                foreach (var thisTab in tabSeiten.TabPages) {
                    var tb = (TabPage)thisTab;

                    if (!x.Contains(tb.Text)) {
                        tabSeiten.TabPages.Remove(tb);
                        DoPages();
                        return;
                    }

                    _ = x.Remove(tb.Text);
                    if (Pad != null && tb.Text == Pad.CurrentPage) { later = tb; }
                }

                foreach (var thisn in x) {
                    var t = new TabPage(thisn) {
                        Name = "Seite_" + thisn
                    };
                    tabSeiten.TabPages.Add(t);

                    if (Pad != null && t.Text == Pad.CurrentPage) { later = t; }
                }
            } else {
                tabSeiten.Visible = false;
                if (tabSeiten.TabPages.Count > 0) {
                    tabSeiten.TabPages.Clear();
                }
            }

            tabSeiten.SelectedTab = later;
        } catch { }
    }

    private void Pad_DrawModChanged(object sender, System.EventArgs e) {
        btnVorschauModus.Checked = Pad.ShowInPrintMode;

        DoPages();
    }



    private void Pad_MouseUp(object sender, MouseEventArgs e) {
        if (btnZoomIn.Checked) { Pad.ZoomIn(e); }
        if (btnZoomOut.Checked) { Pad.ZoomOut(e); }
    }

    private void Pad_PropertyChanged(object sender, System.EventArgs e) => DoPages();

    private void tabSeiten_Selected(object sender, TabControlEventArgs e) {
        var s = string.Empty;

        if (tabSeiten.SelectedTab != null) {
            s = tabSeiten.SelectedTab.Text;
        }

        Pad.CurrentPage = s;
    }

    #endregion
}