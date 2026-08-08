// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.EventArgs;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class PadEditor : FormWithStatusBar {

    #region Fields

    private ItemCollectionPadItem? _currentItemsForSideMenu;

    #endregion

    #region Constructors

    public PadEditor() : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        PadDesign.ItemClear();
        PadDesign.ItemAddRange(ItemsOf(Skin.AllStyles()));
        PadDesign.Text = PadDesign[0]?.KeyName ?? string.Empty;

        if (Pad?.Items is not null && Skin.HasStyles) {
            Pad.Items.SheetStyle = PadDesign.Text;
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Ungleich leer: Das Pad ist schreibgeschützt und der Text wird als
    /// Hinweis über allen Items im <see cref="CreativePad"/> eingeblendet.
    /// Jegliche Bearbeitung wird unterbunden. Zoom und Verschieben bleiben möglich.
    /// </summary>
    public string NotEditableReason {
        get;
        set {
            value ??= string.Empty;
            if (field == value) { return; }
            field = value;
            if (Pad is { IsDisposed: false }) { Pad.NotEditableReason = value; }
            OnNotEditableReasonChanged();
        }
    } = string.Empty;

    #endregion

    #region Methods

    /// <summary>
    /// Wird aufgerufen, nachdem sich <see cref="NotEditableReason"/> geändert hat.
    /// Ableitungen können hier z.B. Bearbeitungs-Buttons (de)aktivieren.
    /// </summary>
    protected virtual void OnNotEditableReasonChanged() { }

    /// <summary>
    /// Wenn true, werden bei nicht angewähltem Item die Eigenschaften des
    /// Pads (ItemCollectionPadItem) in der Seitenleiste angezeigt — z.B. um
    /// eine Referenztabelle auszuwählen. Der ConnectedFormulaEditor
    /// überschreibt dies mit false.
    /// </summary>
    protected virtual bool ShowPadPropertiesWhenNoItemSelected => true;

    protected virtual void Pad_ClickedItemChanged(object sender, System.EventArgs e) {
        Pad.LastClickedItem?.DoUpdateSideOptionMenu += LastClickedItem_DoUpdateSideOptionMenu;

        LastClickedItem_DoUpdateSideOptionMenu(this, System.EventArgs.Empty);
    }

    protected virtual void Pad_GotNewItemCollection(object sender, System.EventArgs e) {
        btnVorschauModus.Checked = Pad.ShowInPrintMode;

        ckbRaster.Enabled = Pad.Items is not null;
        txbRasterAnzeige.Enabled = Pad.Items is not null;
        txbRasterFangen.Enabled = Pad.Items is not null;

        if (Pad.Items is not null) {
            ckbRaster.Checked = Pad.Items.SnapMode == SnapMode.SnapToGrid;
            txbRasterAnzeige.Text = Pad.Items.GridShow.ToString1_2();
            txbRasterFangen.Text = Pad.Items.GridSnap.ToString1_2();
            PadDesign.Text = Pad.Items.SheetStyle;
        }

        SubscribeItemsForSideMenu();
    }

    private void btnAlsBildSpeichern_Click(object sender, System.EventArgs e) => Pad.OpenSaveDialog(string.Empty);

    private void btnArbeitsbreichSetup_Click(object sender, System.EventArgs e) => Pad.ShowWorkingAreaSetup();

    private void btnDruckerDialog_Click(object sender, System.EventArgs e) => Pad.Print();

    private void btnHintergrundFarbe_Click(object sender, System.EventArgs e) {
        if (Pad?.Items is null) { return; }

        ColorDia.Color = Pad.Items.BackColor;
        ColorDia.ShowDialog();
        Pad.Items.BackColor = ColorDia.Color;
        Pad.Invalidate();
    }

    private void btnKeinHintergrund_Click(object sender, System.EventArgs e) {
        if (Pad?.Items is null) { return; }

        Pad.Items.BackColor = Color.Transparent;
        Pad.Invalidate();
    }

    private void btnNoArea_Click(object sender, System.EventArgs e) {
        if (Pad?.Items is null) { return; }

        Pad.Items.Endless = true;
        Pad.Invalidate();
    }

    private void btnPageSetup_Click(object sender, System.EventArgs e) => Pad.ShowPrinterPageSetup();

    private void btnVorschau_Click(object sender, System.EventArgs e) => Pad.ShowPrintPreview();

    private void btnVorschauModus_CheckedChanged(object sender, System.EventArgs e) => Pad.ShowInPrintMode = btnVorschauModus.Checked;

    private void btnZoom11_Click(object sender, System.EventArgs e) => Pad.Zoom = 1f;

    private void btnZoomFit_Click(object sender, System.EventArgs e) => Pad.ZoomFit();

    private void ckbRaster_CheckedChanged(object sender, System.EventArgs e) {
        if (Pad?.Items is null) { return; }
        Pad.Items.SnapMode = ckbRaster.Checked ? SnapMode.SnapToGrid : SnapMode.Ohne;
    }

    private void LastClickedItem_DoUpdateSideOptionMenu(object? sender, System.EventArgs e) {
        if (Pad.LastClickedItem is { IsDisposed: false } item) {
            item.DoForm(tabElementEigenschaftenPanel);
            return;
        }

        if (ShowPadPropertiesWhenNoItemSelected) {
            Pad.Items?.DoForm(tabElementEigenschaftenPanel);
        } else {
            ((ISimpleEditor?)null).DoForm(tabElementEigenschaftenPanel);
        }
    }

    private void Pad_ClickedItemChanging(object sender, System.EventArgs e) => Pad.LastClickedItem?.DoUpdateSideOptionMenu -= LastClickedItem_DoUpdateSideOptionMenu;

    /// <summary>
    /// Abonniert das DoUpdateSideOptionMenu-Event der aktuellen
    /// ItemCollection, damit Änderungen (z.B. Referenztabelle) die
    /// Seitenleiste aktualisieren, wenn kein Item angewählt ist.
    /// </summary>
    private void SubscribeItemsForSideMenu() {
        if (_currentItemsForSideMenu is { IsDisposed: false } old && !ReferenceEquals(old, Pad.Items)) {
            old.DoUpdateSideOptionMenu -= LastClickedItem_DoUpdateSideOptionMenu;
        }

        _currentItemsForSideMenu = Pad.Items;

        if (_currentItemsForSideMenu is { IsDisposed: false } icpi) {
            icpi.DoUpdateSideOptionMenu += LastClickedItem_DoUpdateSideOptionMenu;
        }

        LastClickedItem_DoUpdateSideOptionMenu(this, System.EventArgs.Empty);
    }

    private void Pad_DrawModChanged(object sender, System.EventArgs e) => btnVorschauModus.Checked = Pad.ShowInPrintMode;

    private void Pad_MouseUp(object sender, MouseEventArgs e) {
        if (btnZoomIn.Checked) { Pad.ZoomIn(e); }
        if (btnZoomOut.Checked) { Pad.ZoomOut(e); }
    }

    private void PadDesign_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (Pad?.Items is not null && Skin.HasStyles) {
            Pad.Items.SheetStyle = e.Item.KeyName;
        }
    }

    private void txbRasterAnzeige_TextChanged(object sender, System.EventArgs e) {
        if (!txbRasterAnzeige.Text.IsNumeral()) { return; }
        if (!txbRasterAnzeige.Visible) { return; }
        if (Pad?.Items is null) { return; }
        Pad.Items.GridShow = FloatParse(txbRasterAnzeige.Text);
    }

    private void txbRasterFangen_TextChanged(object sender, System.EventArgs e) {
        if (!txbRasterFangen.Text.IsNumeral()) { return; }
        if (!txbRasterFangen.Visible) { return; }
        if (Pad?.Items is null) { return; }
        Pad.Items.GridSnap = FloatParse(txbRasterFangen.Text);
    }

    #endregion

}