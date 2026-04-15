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
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.IO;
using static BlueBasics.Extensions;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public sealed partial class EasyPic : GenericControlReciver, IContextMenu //  UserControl //
                                                  {
    #region Fields

    private Bitmap? _bitmap;
    private int _panelMoveDirection;
    private System.Threading.Timer? _panelMover;

    #endregion

    #region Constructors

    public EasyPic() : base(true, false, false) {
        InitializeComponent();
        SetNotFocusable();
        _panelMover = new System.Threading.Timer(_ => {
            if (IsHandleCreated) { BeginInvoke(new Action(PanelMover_Tick)); }
        }, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
    }

    #endregion

    #region Properties

    public object? ContextMenuHotItem { get; set; }

    public MouseEventArgs? ContextMenuMouseEventArgs { get; set; }

    public ReadOnlyCollection<AbstractListItem>? CustomMenuItems { get; set; }

    [DefaultValue(true)]
    public bool Editable {
        get;
        set {
            if (field == value) { return; }
            field = value;
            InvalidateAndCheckButtons();
        }
    } = true;

    [DefaultValue("")]
    public string FileName {
        get;
        set {
            if (value.Equals(field, StringComparison.OrdinalIgnoreCase)) { return; }

            field = value;

            _bitmap = !FileExists(field) ? null : (Bitmap?)Image_FromFile(field);
            InvalidateAndCheckButtons();
        }
    } = string.Empty;

    public string OriginalText {
        get;
        set {
            if (field == value) { return; }
            field = value;
            InvalidateAndCheckButtons();
        }
    } = string.Empty;

    [DefaultValue(0)]
    public new int TabIndex {
        get => 0;

        set => base.TabIndex = 0;
    }

    [DefaultValue(false)]
    public new bool TabStop {
        get => false;

        set => base.TabStop = false;
    }

    #endregion

    #region Methods

    public bool DeleteImageInFileSystem() {
        if (string.IsNullOrEmpty(FileName)) { return true; }
        if (!FileExists(FileName)) { return true; }

        if (Forms.MessageBox.Show("Vorhandenes Bild löschen?", ImageCode.Warnung, "Löschen", "Abbruch") != 0) { return false; }

        if (DeleteFile(FileName, 20)) {
            _bitmap = null;
            InvalidateAndCheckButtons();
            return true;
        }

        return false;
    }

    public List<AbstractListItem>? GetContextMenuItems() {
        List<AbstractListItem> contextMenu = [];
        if (_bitmap != null) {
            contextMenu.Add(ItemOf("Externes Fenster öffnen", null, PictureView.Contextmenu_OpenImage, true, string.Empty));
        }
        return contextMenu;
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);

        if (state.HasFlag(States.Standard_MouseOver)) { state ^= States.Standard_MouseOver; }
        if (state.HasFlag(States.Standard_MousePressed)) { state ^= States.Standard_MousePressed; }

        Skin.Draw_Back(gr, Design.EasyPic, state, DisplayRectangle, this, true);

        if (_bitmap != null) {
            gr.DrawImageInRectAspectRatio(_bitmap, 1, 1, Width - 2, Height - 2);
        }

        Skin.Draw_Border(gr, Design.EasyPic, state, DisplayRectangle);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);
        RowsInputChangedHandled = true;

        var ct = string.Empty;

        if (RowSingleOrNull()?.CheckRow().PrepareFormulaFeedback.Variables is { } list) {
            ct = list.ReplaceInText(OriginalText);
        }

        FileName = ct;
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        if (!Enabled) {
            EditPanelFrame.Visible = false;
            _panelMover?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            _panelMoveDirection = 0;
        }
    }

    protected override void OnMouseEnter(System.EventArgs e) {
        base.OnMouseEnter(e);
        if (Editable) {
            _panelMoveDirection = 1;
            _panelMover?.Change(5, 5);
        }
    }

    protected override void OnMouseLeave(System.EventArgs e) {
        base.OnMouseLeave(e);
        _panelMover?.Change(5, 5);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Right) {
            FloatingInputBoxListBoxStyle.ContextMenuShow(this, this, e);
        }
    }

    protected override void OnResize(System.EventArgs e) {
        base.OnResize(e);
        InvalidateAndCheckButtons();
    }

    private void PanelMover_Tick() {
        if (_panelMoveDirection == 0) {
            if (!EditPanelFrame.Visible) {
                _panelMover?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                return;
            }
        }
        if (_panelMoveDirection >= 0) {
            if (!Editable || !ContainsMouse()) { _panelMoveDirection = -1; }
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

    private void btnScreenshot_Click(object sender, System.EventArgs e) {
        if (!DeleteImageInFileSystem()) { return; }
        if (!HasFileName()) { return; }
        _bitmap = ScreenShot.GrabArea(ParentForm()).Area;

        SaveNewPicToDisc();
        InvalidateAndCheckButtons();
    }

    private void DelP_Click(object sender, System.EventArgs e) => DeleteImageInFileSystem();

    private bool HasFileName() {
        if (string.IsNullOrEmpty(FileName)) { return false; }

        if (!FileName.FileSuffix().Equals("PNG", StringComparison.OrdinalIgnoreCase) &&
            !FileName.FileSuffix().Equals("JPG", StringComparison.OrdinalIgnoreCase)) { return false; }

        if (string.IsNullOrEmpty(FileName.FilePath())) { return false; }

        if (string.IsNullOrEmpty(FileName.FileNameWithoutSuffix())) { return false; }

        return true;
    }

    private void InvalidateAndCheckButtons() {
        _panelMoveDirection = -1;
        _panelMover?.Change(5, 5);
        btnDeleteImage.Enabled = _bitmap != null && Editable;
        btnLoad.Enabled = Editable;
        btnScreenshot.Enabled = Editable;
        //Invalidate();
    }

    private void Lade_Click(object sender, System.EventArgs e) {
        if (!DeleteImageInFileSystem()) { return; }
        if (!HasFileName()) { return; }
        OpenDia.ShowDialog();
    }

    private void OpenDia_FileOk(object sender, CancelEventArgs e) {
        if (!HasFileName()) { return; }
        _bitmap = Image_FromFile(OpenDia.FileName) as Bitmap;
        SaveNewPicToDisc();
        InvalidateAndCheckButtons();
    }

    private void SaveNewPicToDisc() {
        if (!HasFileName() || _bitmap == null || !Editable) { return; }

        try {
            using var bmp = new Bitmap(_bitmap);
            using var memory = new System.IO.MemoryStream();
            bmp.Save(memory, ImageFormat.Png);
            WriteAllBytes(FileName, memory.ToArray());
        } catch (Exception ex) {
            Forms.MessageBox.Show($"Fehler beim Speichern des Bildes: {ex.Message}");
        }
    }

    #endregion
}