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

using System;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.EventArgs;
using BluePaint.EventArgs;

namespace BluePaint;

public abstract partial class GenericTool : GroupBox // System.Windows.Forms.UserControl //
{
    #region Fields

    protected static readonly SolidBrush BrushRedTransp = new(Color.FromArgb(128, 255, 0, 0));
    protected static readonly Pen PenLightWhite = new(Color.FromArgb(150, 255, 255, 255), 3);
    protected static readonly Pen PenRedTransp = new(ColorRedTransp);
    protected static Color ColorRedTransp = Color.FromArgb(50, 255, 0, 0);

    #endregion

    #region Constructors

    protected GenericTool() : base() => InitializeComponent();

    #endregion

    #region Events

    public event EventHandler? DoInvalidate;

    public event EventHandler? ForceUndoSaving;

    public event EventHandler? HideMainWindow;

    public event EventHandler<BitmapEventArgs>? NeedCurrentPic;

    public event EventHandler<ZoomBitmapEventArgs>? OverridePic;

    public event EventHandler? ShowMainWindow;

    public event EventHandler? ZoomFit;

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
    /// <param name="originalPic"></param>
    public virtual void DoAdditionalDrawing(AdditionalDrawing e, Bitmap? originalPic) { }

    /// <summary>
    ///
    /// </summary>
    /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
    /// <param name="originalPic"></param>
    public virtual new void MouseDown(MouseEventArgs1_1 e, Bitmap? originalPic) { }

    /// <summary>
    ///
    /// </summary>
    /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
    public virtual new void MouseMove(MouseEventArgs1_1DownAndCurrent e, Bitmap? originalPic) { }

    /// <summary>
    ///
    /// </summary>
    /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
    public virtual new void MouseUp(MouseEventArgs1_1DownAndCurrent e, Bitmap? originalPic) { }

    /// <summary>
    ///
    /// </summary>
    public virtual void OnToolChanging() { }

    /// <summary>
    /// Z.B: bei Undo
    /// </summary>
    /// <returns></returns>
    internal virtual void PictureChangedByMainWindow() { }

    internal virtual void ToolFirstShown() { }

    protected void OnDoInvalidate() => DoInvalidate?.Invoke(this, System.EventArgs.Empty);

    protected void OnForceUndoSaving() => ForceUndoSaving?.Invoke(this, System.EventArgs.Empty);

    protected void OnHideMainWindow() => HideMainWindow?.Invoke(this, System.EventArgs.Empty);

    protected Bitmap? OnNeedCurrentPic() {
        BitmapEventArgs e = new(null);
        NeedCurrentPic?.Invoke(this, e);
        return e.Bmp;
    }

    /// <summary>
    /// Es wird automatisch OnForceUndoSaving in der MainForm ausgelöst.
    /// Wird benutzt, wenn ein neues Bild erstellt wurde und dieses in den Speicher soll.
    /// </summary>
    /// <param name="bmp"></param>
    protected void OnOverridePic(Bitmap? bmp, bool zoomfit) => OverridePic?.Invoke(this, new ZoomBitmapEventArgs(bmp, zoomfit));

    protected void OnShowMainWindow() => ShowMainWindow?.Invoke(this, System.EventArgs.Empty);

    protected void OnZoomFit() => ZoomFit?.Invoke(this, System.EventArgs.Empty);

    #endregion
}