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

using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using BlueControls.ItemCollectionList;
using static BlueBasics.Extensions;
using static BlueBasics.IO;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public sealed partial class EasyPic : GenericControl, IContextMenu, IBackgroundNone, IControlUsesRow {

    #region Fields

    private Bitmap? _bitmap;

    private string _filename = string.Empty;

    private FilterCollection? _filterInput;
    private string _originalText = string.Empty;

    private int _panelMoveDirection;

    #endregion

    #region Constructors

    public EasyPic() : base(false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        SetNotFocusable();
        //((IControlSendFilter)this).RegisterEvents();
        this.RegisterEvents();
    }

    #endregion

    #region Events

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    #endregion

    #region Properties

    [DefaultValue("")]
    public string FileName {
        get => _filename;
        set {
            if (value.Equals(_filename, StringComparison.OrdinalIgnoreCase)) { return; }

            _filename = value;

            if (!FileExists(_filename)) {
                _bitmap = null;
            } else {
                _bitmap = (Bitmap?)Image_FromFile(_filename);
            }
            ZoomFitInvalidateAndCheckButtons();
        }
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput {
        get => _filterInput;
        set {
            if (_filterInput == value) { return; }
            this.UnRegisterEventsAndDispose();
            _filterInput = value;
            this.RegisterEvents();
        }
    }

    public bool FilterInputChangedHandled { get; set; }

    public string OriginalText {
        get => _originalText;
        set {
            _originalText = value;
            ZoomFitInvalidateAndCheckButtons();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlSendFilter> Parents { get; } = [];

    public List<RowItem>? RowsInput { get; set; }

    public bool RowsInputChangedHandled { get; set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool RowsInputManualSeted { get; set; } = false;

    [DefaultValue(0)]
    public new int TabIndex {
        get => 0;
        // ReSharper disable once ValueParameterNotUsed
        set => base.TabIndex = 0;
    }

    [DefaultValue(false)]
    public new bool TabStop {
        get => false;
        // ReSharper disable once ValueParameterNotUsed
        set => base.TabStop = false;
    }

    #endregion

    #region Methods

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
        switch (e.ClickedCommand) {
            case "ExF":
                PictureView epv = new(_bitmap);
                epv.Show();
                return true;

                //case "Speichern":
                //    System.Windows.Forms.FolderBrowserDialog savOrt = new();
                //    savOrt.ShowDialog();
                //    if (!DirectoryExists(savOrt.SelectedPath)) {
                //        MessageBox.Show("Abbruch!", ImageCode.Warnung, "OK");
                //        return true;
                //    }
                //    var ndt = TempFile(savOrt.SelectedPath + "\\Bild.png");
                //    _bitmap.Save(ndt, ImageFormat.Png);
                //    ExecuteFile(ndt);
                //    return true;
        }
        return false;
    }

    public bool DeleteImageInFileSystem() {
        if (string.IsNullOrEmpty(_filename)) { return true; }
        if (!FileExists(_filename)) { return true; }

        if (MessageBox.Show("Vorhandenes Bild löschen?", ImageCode.Warnung, "Löschen", "Abbruch") != 0) { return false; }

        if (DeleteFile(_filename, false)) {
            _bitmap = null;
            ZoomFitInvalidateAndCheckButtons();
            return true;
        }

        return false;
    }

    public void FilterInput_DispodingEvent(object sender, System.EventArgs e) => this.FilterInput_DispodingEvent();

    public void FilterInput_RowsChanged(object sender, System.EventArgs e) => this.FilterInput_RowsChanged();

    public void GetContextMenuItems(MouseEventArgs? e, List<AbstractListItem> items, out object? hotItem) {
        hotItem = null;
        if (_bitmap != null) {
            _ = items.Add("Externes Fenster öffnen", "ExF");
        }
    }

    public void HandleChangesNow() {
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        if (!FilterInputChangedHandled) {
            FilterInputChangedHandled = true;
            this.DoInputFilter(null, false);
        }

        RowsInputChangedHandled = true;

        this.DoRows();

        var ct = string.Empty;

        if (this.RowSingleOrNull()?.LastCheckedEventArgs?.Variables is VariableCollection list) {
            ct = list.ReplaceInText(OriginalText);
        }

        FileName = ct;
    }

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    public void ParentFilterOutput_Changed() { }

    public void RowsInput_Changed() { }

    //Inherits Windows.Forms.UserControl
    //UserControl überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
    [DebuggerNonUserCode]
    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                this.DoDispose();
                components?.Dispose();
            }
        } finally {
            base.Dispose(disposing);
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        HandleChangesNow();

        if (state.HasFlag(States.Standard_MouseOver)) { state ^= States.Standard_MouseOver; }
        if (state.HasFlag(States.Standard_MousePressed)) { state ^= States.Standard_MousePressed; }

        Skin.Draw_Back(gr, Design.EasyPic, state, DisplayRectangle, this, true);

        if (_bitmap != null) {
            gr.DrawImageInRectAspectRatio(_bitmap, 1, 1, Width - 2, Height - 2);
        }

        Skin.Draw_Border(gr, Design.EasyPic, state, DisplayRectangle);
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        if (!Enabled) {
            EditPanelFrame.Visible = false;
            _PanelMover.Enabled = false;
            _panelMoveDirection = 0;
        }
    }

    protected override void OnMouseEnter(System.EventArgs e) {
        base.OnMouseEnter(e);
        _panelMoveDirection = 1;
        _PanelMover.Enabled = true;
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        _PanelMover.Enabled = true;
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Right) {
            FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
        }
    }

    protected override void OnResize(System.EventArgs e) {
        base.OnResize(e);
        ZoomFitInvalidateAndCheckButtons();
    }

    private void btnScreenshot_Click(object sender, System.EventArgs e) {
        if (!DeleteImageInFileSystem()) { return; }
        if (!HasFileName()) { return; }
        _bitmap = ScreenShot.GrabArea(ParentForm()).CloneOfBitmap();

        SaveNewPicToDisc();
        ZoomFitInvalidateAndCheckButtons();
    }

    private void DelP_Click(object sender, System.EventArgs e) => DeleteImageInFileSystem();

    private void EditPanel_Tick(object sender, System.EventArgs e) {
        if (_panelMoveDirection == 0) {
            if (!EditPanelFrame.Visible) {
                _PanelMover.Enabled = false;
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

    private bool HasFileName() {
        if (string.IsNullOrEmpty(_filename)) { return false; }

        if (!_filename.FileSuffix().Equals("PNG", StringComparison.OrdinalIgnoreCase)) { return false; }

        if (string.IsNullOrEmpty(_filename.FilePath())) { return false; }

        if (string.IsNullOrEmpty(_filename.FileNameWithoutSuffix())) { return false; }

        return true;
    }

    private void Lade_Click(object sender, System.EventArgs e) {
        if (!DeleteImageInFileSystem()) { return; }
        if (!HasFileName()) { return; }
        _ = OpenDia.ShowDialog();
    }

    private void OpenDia_FileOk(object sender, CancelEventArgs e) {
        if (!HasFileName()) { return; }
        _bitmap = Image_FromFile(OpenDia.FileName) as Bitmap;
        SaveNewPicToDisc();
        ZoomFitInvalidateAndCheckButtons();
    }

    private void SaveNewPicToDisc() {
        if (!HasFileName()) { return; }
        _bitmap?.Save(_filename, ImageFormat.Png);
    }

    private void ZoomFitInvalidateAndCheckButtons() {
        _panelMoveDirection = -1;
        _PanelMover.Enabled = true;
        if (_bitmap == null) {
            btnDeleteImage.Enabled = false;
            Invalidate();
            return;
        }
        btnDeleteImage.Enabled = true;
        Invalidate();
    }

    #endregion
}