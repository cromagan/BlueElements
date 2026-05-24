// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.BlueTableDialogs;
using BlueControls.Designer_Support;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueTable;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;


namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ConnectedCreativePad : GenericControlReciver, IOpenScriptEditor //System.Windows.Forms.UserControl //,
    {
    #region Fields

    private DateTime _lastRefresh = DateTime.UtcNow;
    private int _panelMoveDirection;
    private System.Threading.Timer? _panelMover;
    private System.Threading.Timer? _autoRefresh;

    #endregion

    #region Constructors

    public ConnectedCreativePad(ItemCollectionPadItem? itemCollectionPad) : base(false, false, false) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
        SetNotFocusable();
        pad.Items = itemCollectionPad;
        pad.Unselect();
        pad.ShowInPrintMode = true;
        _panelMover = new System.Threading.Timer(_ => {
            if (IsHandleCreated) { BeginInvoke(new Action(PanelMover_Tick)); }
        }, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        _autoRefresh = new System.Threading.Timer(_ => {
            if (IsHandleCreated) { BeginInvoke(new Action(AutoRefresh_Tick)); }
        }, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
    }

    public ConnectedCreativePad() : this(null) { }

    #endregion

    #region Properties

    public int AutoRefresh {
        get;
        set {
            field = value;

            if (field > 0) { _autoRefresh?.Change(1000, 1000); } else { _autoRefresh?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite); }
        }
    } = -1;

    public float DefaultCopyScale { get; set; } = 1f;

    public string DefaultDesign { get; set; } = string.Empty;
    public string ExecuteScriptAtRowChange { get; internal set; } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowItem? LastRow {
        get;

        set {
            if (value?.Table == null || value.IsDisposed) { value = null; }

            if (field == value) { return; }

            field = value;

            RefreshPad();
        }
    }

    public string LoadAtRowChange { get; internal set; } = string.Empty;

    #endregion

    #region Methods

    public void OpenScriptEditor() {
        if (IsDisposed || GeneratedFrom is not CreativePadItem { IsDisposed: false } it) { return; }

        var se = IUniqueWindowExtension.ShowOrCreate<CreativePadScriptEditor>(it);

        se.Row = LastRow;
    }

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            _panelMover?.Dispose();
            _autoRefresh?.Dispose();
            pad.Items?.Clear();
            pad.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);
        RowsInputChangedHandled = true;
        LastRow = RowSingleOrNull();
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        if (!Enabled) {
            EditPanelFrame.Visible = false;
            _panelMover?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            _panelMoveDirection = 0;
        } else {
            RefreshPad();
        }
    }

    private void AutoRefresh_Tick() {
        if (Generic.Ending || IsDisposed || Disposing) { return; }

        if (DateTime.UtcNow.Subtract(_lastRefresh).TotalSeconds > AutoRefresh) { RefreshPad(); }
    }

    private void PanelMover_Tick() {
        if (Generic.Ending || IsDisposed || Disposing) { return; }

        if (_panelMoveDirection == 0) {
            if (!EditPanelFrame.Visible) {
                _panelMover?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                return;
            }
        }
        if (_panelMoveDirection >= 0) {
            if (!ContainsMouse()) { _panelMoveDirection = -1; }
        }
        if (_panelMoveDirection > 0) {
            if (!EditPanelFrame.Visible) {
                EditPanelFrame.Top = -EditPanelFrame.Height;
                EditPanelFrame.Visible = true;
                return;
            }
            if (EditPanelFrame.Top >= 0) {
                EditPanelFrame.Top = 0;
                _panelMoveDirection = 0;
                return;
            }
            EditPanelFrame.Top += 4;
            return;
        }
        if (_panelMoveDirection < 0) {
            if (EditPanelFrame.Top < -EditPanelFrame.Height) {
                EditPanelFrame.Visible = false;
                _panelMoveDirection = 0;
                return;
            }
            EditPanelFrame.Top -= 4;
        }
    }

    private void btnAktualisieren_Click(object sender, System.EventArgs e) => RefreshPad();

    private void btnCopy_Click(object sender, System.EventArgs e) {
        RefreshPad();

        try {
            if (ZoomPad.ScaleWarnung()) { return; }
            //GenerateBottleFromRow(tblAlleBehaelter.CursorPosRow.Row);
            //var sk = FloatParse(GetSetting("Solid Edge Skalierung").Replace(".", ","));
            var i = pad?.Items?.ToBitmap(DefaultCopyScale);
            Clipboard.SetImage(i);
            QuickNote.Show(NoteSymbols.Ok, "Kopiert");
        } catch {
            QuickNote.Show(NoteSymbols.Critical, "Fehlgeschlagen");
        }
    }

    private void pad_MouseEnter(object sender, System.EventArgs e) {
        _panelMoveDirection = 1;
        _panelMover?.Change(5, 5);
    }

    private void pad_MouseLeave(object sender, System.EventArgs e) => _panelMover?.Change(5, 5);

    private void pad_MouseMove(object sender, MouseEventArgs e) {
        _panelMoveDirection = 1;
        _panelMover?.Change(5, 5);
    }

    private void RefreshPad() {
        pad.SuspendLayout();

        _lastRefresh = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(LoadAtRowChange)) {
            if (LoadAtRowChange.FileType() is FileFormat.BlueCreativeFile) {
                var newItems = new ItemCollectionPadItem(LoadAtRowChange);
                newItems.ResetVariables();
                newItems.ReplaceVariables(LastRow);
                pad.Items = newItems;
            } else {
                pad.Items = null;
            }
        } else if (!string.IsNullOrEmpty(ExecuteScriptAtRowChange)) {
            var newItems = new ItemCollectionPadItem();
            newItems.Endless = true;

            if (Skin.HasStyles) {
                newItems.SheetStyle = DefaultDesign;
            }

            if (LastRow != null) {
                newItems.ExecuteScript(ExecuteScriptAtRowChange, Mode, LastRow);
            }

            pad.Items = newItems;
        } else {
            pad.Items = null;
        }

        pad.ZoomFit();
        pad.ResumeLayout();
    }

    #endregion

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ConnectedFormulaView()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}