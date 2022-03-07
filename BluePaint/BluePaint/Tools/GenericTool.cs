// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueControls.Controls;
using BluePaint.EventArgs;
using System.Drawing;

namespace BluePaint {

    public abstract partial class GenericTool : GroupBox // System.Windows.Forms.UserControl //
    {
        #region Fields

        protected static SolidBrush Brush_RedTransp = new(Color.FromArgb(128, 255, 0, 0));
        protected static Color ColorRedTransp = Color.FromArgb(50, 255, 0, 0);
        protected static Pen Pen_LightWhite = new(Color.FromArgb(150, 255, 255, 255), 3);
        protected static Pen Pen_RedTransp = new(ColorRedTransp);

        #endregion

        #region Constructors

        protected GenericTool() : base() => InitializeComponent();

        #endregion

        #region Events

        public event System.EventHandler<CommandForMacroArgs> CommandForMacro;

        public event System.EventHandler DoInvalidate;

        public event System.EventHandler ForceUndoSaving;

        public event System.EventHandler HideMainWindow;

        public event System.EventHandler<BitmapEventArgs> NeedCurrentPic;

        public event System.EventHandler<BitmapEventArgs> OverridePic;

        public event System.EventHandler ShowMainWindow;

        public event System.EventHandler ZoomFit;

        #endregion

        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual void DoAdditionalDrawing(BlueControls.EventArgs.AdditionalDrawing e, Bitmap? OriginalPic) { }

        public virtual void ExcuteCommand(string command) => BlueBasics.Develop.DebugPrint_RoutineMussUeberschriebenWerden();

        /// <summary>
        /// Falls es während einer Makro aufzeichnung benutzt werden kann, gibt es eine eindeutige Kennung zurück.
        /// Wenn keine Benutzung möglich ist, wird string.empty zurückgegebenm
        /// </summary>
        /// <returns></returns>
        public virtual string MacroKennung() => string.Empty;

        /// <summary>
        ///
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual new void MouseDown(BlueControls.EventArgs.MouseEventArgs1_1 e, Bitmap? OriginalPic) { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual new void MouseMove(BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e, Bitmap? OriginalPic) { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e">Pixel-Koordinaten auf dem Bitmap</param>
        public virtual new void MouseUp(BlueControls.EventArgs.MouseEventArgs1_1DownAndCurrent e, Bitmap? OriginalPic) { }

        /// <summary>
        ///
        /// </summary>
        public virtual void OnToolChanging() { }

        /// <summary>
        /// Z.B: bei Undo
        /// </summary>
        /// <returns></returns>
        public virtual void PictureChangedByMainWindow() {
        }

        public virtual void ToolFirstShown() {
        }

        protected virtual void OnCommandForMacro(string command) => CommandForMacro?.Invoke(this, new CommandForMacroArgs(command));

        protected virtual void OnDoInvalidate() => DoInvalidate?.Invoke(this, System.EventArgs.Empty);

        protected virtual void OnForceUndoSaving() => ForceUndoSaving?.Invoke(this, System.EventArgs.Empty);

        protected virtual void OnHideMainWindow() => HideMainWindow?.Invoke(this, System.EventArgs.Empty);

        protected virtual Bitmap? OnNeedCurrentPic() {
            BitmapEventArgs e = new(null);
            NeedCurrentPic?.Invoke(this, e);
            return e.Bmp;
        }

        /// <summary>
        /// Es wird automatisch OnForceUndoSaving in der MainForm ausgelöst.
        /// Wird benutzt, wenn ein neues Bild erstellt wurde und dieses in den Speicher soll.
        /// </summary>
        /// <param name="Bmp"></param>
        protected virtual void OnOverridePic(Bitmap? Bmp) => OverridePic?.Invoke(this, new BitmapEventArgs(Bmp));

        protected virtual void OnShowMainWindow() => ShowMainWindow?.Invoke(this, System.EventArgs.Empty);

        protected virtual void OnZoomFit() => ZoomFit?.Invoke(this, System.EventArgs.Empty);

        #endregion
    }
}