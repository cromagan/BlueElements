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

using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueControls.Classes;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using WndMsg = BlueControls.Enums.WndProc;

namespace BlueControls.Forms;

public partial class FloatingForm : Form {

    #region Fields

    protected readonly Control? _connectedControl;
    private static readonly List<FloatingForm> AllBoxes = [];

    #endregion

    #region Constructors

    public FloatingForm() : this(Design.Form_QuickInfo) { }

    protected FloatingForm(Design design) : base(design) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        SetStyle(ControlStyles.Selectable, false);
        SetStyle(ControlStyles.StandardClick, false);
        SetStyle(ControlStyles.StandardDoubleClick, false);
        // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
        SetStyle(ControlStyles.ResizeRedraw, false);
        SetStyle(ControlStyles.SupportsTransparentBackColor, false);
        SetStyle(ControlStyles.Opaque, false);
        //The next 3 styles are allefor double buffering
        // Bei FloatingForms muss immer der Hinetergunrd gezeichnet haben. wir wollen ja einen schönen Rahmen haben.
        DoubleBuffered = true;
        SetStyle(ControlStyles.DoubleBuffer, true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.UserPaint, true);
        UpdateStyles();
        AllBoxes.Add(this);
        OutsideClicked += (_, _) => Close();
    }

    protected FloatingForm(Control? connectedControl, Design design) : this(design) => _connectedControl = connectedControl;

    #endregion

    #region Properties

    /// <summary>
    /// Bestimmt, ob die FloatingForm automatisch geschlossen wird, wenn außerhalb geklickt wird.
    /// </summary>
    public DismissMode DismissMode { get; protected set; } = DismissMode.OnOutsideClick;

    /// <summary>
    /// Floating Forms sind immer Topmost, darf aber hier nicht gesetzt werden und wird über
    /// CreateParams gesteuert. Wenn TopMost true wäre, würde das Form den Focus bekommen.
    /// </summary>
    public new bool TopMost {
        get => false;

        set => base.TopMost = false;
    }

    protected override CreateParams CreateParams {
        get {
            var oParam = base.CreateParams;
            oParam.ExStyle |= (int)ExStyle.EX_NOACTIVATE | (int)ExStyle.EX_TOOLWINDOW | (int)ExStyle.EX_TOPMOST;
            oParam.Parent = IntPtr.Zero;
            return oParam;
        }
    }

    #endregion

    #region Methods

    public new void Close() {
        try {
            if (InvokeRequired) {
                Invoke(new Action(Close));
                return;
            }

            AllBoxes.Remove(this);
            base.Close();
        } catch {
            Develop.AbortAppIfStackOverflow();
            Close();
        }
    }

    public void Position_LocateToMouse() {
        var screenNr = Generic.PointOnScreenNr(Cursor.Position);
        CheckMaxSize(screenNr);
        var ypos = Cursor.Position.Y + 15;
        var xpos = Cursor.Position.X + 15;
        if (xpos + Width > Screen.AllScreens[screenNr].Bounds.Right) {
            xpos = Cursor.Position.X - 5 - Width;
        }
        if (ypos + Height > Screen.AllScreens[screenNr].Bounds.Bottom) {
            ypos = Cursor.Position.Y - 5 - Height;
        }
        Position_SetWindowIntoScreen(screenNr, xpos, ypos);
    }

    public void Position_LocateToPosition(Point bestPosition) {
        var screenNr = Generic.PointOnScreenNr(bestPosition);
        CheckMaxSize(screenNr);
        Position_SetWindowIntoScreen(screenNr, bestPosition.X, bestPosition.Y);
    }

    public void Position_SetWindowIntoScreen(int screenNr, int xpos, int ypos) {
        //  Dim ScreenNr As Integer = PointOnScreenNr(BestPosition)
        CheckMaxSize(screenNr);
        if (xpos < Screen.AllScreens[screenNr].WorkingArea.Left) { xpos = Screen.AllScreens[screenNr].WorkingArea.Left; }
        if (ypos < Screen.AllScreens[screenNr].WorkingArea.Top) { ypos = Screen.AllScreens[screenNr].WorkingArea.Top; }
        if (xpos + Width > Screen.AllScreens[screenNr].WorkingArea.Right) { xpos = Screen.AllScreens[screenNr].WorkingArea.Right - Width; }
        if (ypos + Height > Screen.AllScreens[screenNr].WorkingArea.Bottom) { ypos = Screen.AllScreens[screenNr].WorkingArea.Bottom - Height; }
        Location = new Point(xpos, ypos);
    }

    public new void Show() {
        if (IsDisposed || Disposing) { return; }

        try {
            _ = WindowsRemoteControl.ShowWindow(Handle, (int)Sw.ShowNoActivate);
            lock (AllBoxes) {
                AllBoxes.Remove(this);
                AllBoxes.Add(this);
            }
        } catch (ObjectDisposedException) {
            // kommt vor, wenn der Aufbau zu lange dauert. Ignorierbar.
        } catch (Exception ex) {
            Develop.DebugPrint("Floating Form konnte nicht angezeigt werden", ex);
        }
    }

    internal static void Close(object? connectedControl, Design design) {
        if (design == Design.Undefined) {
            for (var i = AllBoxes.Count - 1; i >= 0; i--) {
                var thisForm = AllBoxes[i];
                if (thisForm.IsDisposed) { continue; }
                if (connectedControl != null && connectedControl != thisForm._connectedControl) { continue; }
                try {
                    thisForm.Close();
                } catch (Exception ex) {
                    Develop.DebugPrint("Fehler beim Schließen der Floating Form", ex);
                }
                return;
            }
            return;
        }

        foreach (var thisForm in AllBoxes.Where(f => !f.IsDisposed).ToList()) {
            if (connectedControl != null && connectedControl != thisForm._connectedControl) { continue; }
            if (thisForm.Design != design) { continue; }
            try {
                thisForm.Close();
            } catch (Exception ex) {
                Develop.DebugPrint("Fehler beim Schließen der Floating Form", ex);
            }
        }
    }

    internal static void Close(Design design) => Close(null, design);

    internal static void Close(object? connectedControl) => Close(connectedControl, Design.Undefined);

    internal static bool IsShowing(object connectedControl) => AllBoxes.Exists(thisForm => !thisForm.IsDisposed && connectedControl == thisForm._connectedControl);

    public event EventHandler? OutsideClicked;

    protected static List<FloatingForm> GetActiveForms() => AllBoxes;

    protected override void OnPaint(PaintEventArgs? e) {
        // MyBase.OnPaint(e) - comment out - do not call  http://stackoverflow.com/questions/592538/how-to-create-a-transparent-control-which-works-when-on-top-of-other-controls
        if (IsClosed || IsDisposed) { return; }
        if (BackgroundImage == null || Width != BackgroundImage.Width || Height != BackgroundImage.Height) {
            BackgroundImage = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
        }
        var tmpgr = Graphics.FromImage(BackgroundImage);
        Skin.Draw_Back(tmpgr, Design, States.Standard, DisplayRectangle, null, false);
        Skin.Draw_Border(tmpgr, Design, States.Standard, DisplayRectangle);
    }

    private void CheckMaxSize(int screenNr) {
        Width = Math.Min(Width, (int)(Screen.AllScreens[screenNr].WorkingArea.Width * 0.9));
        Height = Math.Min(Height, (int)(Screen.AllScreens[screenNr].WorkingArea.Height * 0.9));
    }

    #endregion

    #region Classes

    private sealed class MessageFilter : IMessageFilter {

        #region Methods

        public bool PreFilterMessage(ref Message m) {
            if (m.Msg is not ((int)WndMsg.WM_LBUTTONDOWN or (int)WndMsg.WM_RBUTTONDOWN or (int)WndMsg.WM_MBUTTONDOWN)) {
                return false;
            }

            var pt = Cursor.Position;
            FloatingForm? hitForm = null;

            lock (AllBoxes) {
                for (var i = AllBoxes.Count - 1; i >= 0; i--) {
                    var form = AllBoxes[i];
                    if (form.IsDisposed || form.IsClosed) { continue; }
                    if (new Rectangle(form.Location, form.Size).Contains(pt)) {
                        hitForm = form;
                        break;
                    }
                }

                if (hitForm != null) {
                    AllBoxes.Remove(hitForm);
                    AllBoxes.Add(hitForm);
                } else {
                    for (var i = AllBoxes.Count - 1; i >= 0; i--) {
                        var form = AllBoxes[i];
                        if (form.IsDisposed || form.IsClosed) { continue; }
                        if (form.DismissMode == DismissMode.OnOutsideClick) {
                            form.OutsideClicked?.Invoke(form, System.EventArgs.Empty);
                            break;
                        }
                    }
                }
            }

            return false;
        }

        #endregion
    }

    #endregion
}