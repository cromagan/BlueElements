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
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueDatabase;
using System;
using System.ComponentModel;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ConnectedCreativePad : GenericControlReciver, IOpenScriptEditor //System.Windows.Forms.UserControl //,
    {
    #region Fields

    private int _autoRefreshx = -1;
    private DateTime _lastRefresh = DateTime.UtcNow;
    private RowItem? _lastRow;
    private int _panelMoveDirection;

    #endregion

    #region Constructors

    public ConnectedCreativePad(ItemCollectionPad.ItemCollectionPad? itemCollectionPad) : base(false, false) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
        SetNotFocusable();
        pad.Items = itemCollectionPad;
        pad.Unselect();
        pad.ShowInPrintMode = true;
        pad.EditAllowed = false;
        MouseHighlight = false;
    }

    public ConnectedCreativePad() : this(null) { }

    #endregion

    #region Properties

    public int AutoRefresh {
        get => _autoRefreshx;
        set {
            _autoRefreshx = value;

            _autoRefresh.Enabled = _autoRefreshx > 0;
        }
    }

    public float DefaultCopyScale { get; set; } = 1f;

    public string DefaultDesign { get; set; } = string.Empty;
    public float DefaultScale { get; set; } = 1f;
    public string ExecuteScriptAtRowChange { get; internal set; } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowItem? LastRow {
        get => _lastRow;

        set {
            if (value?.Database == null || value.IsDisposed) { value = null; }

            if (_lastRow == value) { return; }

            _lastRow = value;

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
        DoRows();

        LastRow = RowSingleOrNull();
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        if (!Enabled) {
            EditPanelFrame.Visible = false;
            _panelMover.Enabled = false;
            _panelMoveDirection = 0;
        } else {
            RefreshPad();
        }
    }

    private void _autoRefresh_Tick(object sender, System.EventArgs e) {
        if (DateTime.UtcNow.Subtract(_lastRefresh).TotalSeconds > _autoRefreshx) { RefreshPad(); }
    }

    private void _panelMover_Tick(object sender, System.EventArgs e) {
        if (_panelMoveDirection == 0) {
            if (!EditPanelFrame.Visible) {
                _panelMover.Enabled = false;
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

    private void btnAktualisieren_Click(object sender, System.EventArgs e) {
        RefreshPad();
    }

    private void btnCopy_Click(object sender, System.EventArgs e) {
        RefreshPad();

        try {
            if (ZoomPad.ScaleWarnung()) { return; }
            //GenerateBottleFromRow(tblAlleBehaelter.CursorPosRow.Row);
            //var sk = FloatParse(GetSetting("Solid Edge Skalierung").Replace(".", ","));
            var i = pad?.Items?.ToBitmap(DefaultCopyScale);
            System.Windows.Forms.Clipboard.SetImage(i);
            Notification.Show("Kopiert!", ImageCode.Smiley);
        } catch {
            MessageBox.Show("Fehler beim Kopieren!");
        }
    }

    private void pad_MouseEnter(object sender, System.EventArgs e) {
        _panelMoveDirection = 1;
        _panelMover.Enabled = true;
    }

    private void pad_MouseLeave(object sender, System.EventArgs e) {
        _panelMover.Enabled = true;
    }

    private void pad_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
        _panelMoveDirection = 1;
        _panelMover.Enabled = true;
    }

    private void RefreshPad() {
        pad.SuspendLayout();

        _lastRefresh = DateTime.UtcNow;

        pad.Items = null;

        if (!string.IsNullOrEmpty(LoadAtRowChange)) {
            if (LoadAtRowChange.FileType() is FileFormat.BlueCreativeFile) {
                pad.Items = new ItemCollectionPad.ItemCollectionPad(LoadAtRowChange);
                pad.Items.ResetVariables();
                pad.Items.ReplaceVariables(_lastRow);
            }
        } else if (!string.IsNullOrEmpty(ExecuteScriptAtRowChange)) {
            pad.Items = new ItemCollectionPad.ItemCollectionPad();
            pad.Items.SheetStyleScale = DefaultScale;

            if (Skin.StyleDb?.Row != null) {
                pad.Items.SheetStyle = Skin.StyleDb.Row[DefaultDesign];
            }

            if (_lastRow != null) {
                pad.Items.ExecuteScript(ExecuteScriptAtRowChange, Mode, _lastRow);
            }
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