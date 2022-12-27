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

#nullable enable

using BlueBasics;
using BlueControls.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using static BlueBasics.BitmapExt;

namespace BlueControls;

/// <summary>
/// Eine Klasse, die alle möglichen Arten von Screenshots zurückgibt.
/// </summary>
public sealed partial class ScreenShot {
    //[AccessedThroughProperty(nameof(Hook))]
    //private SystemInputHook _Hook;

    //private List<clsScreenData> AllS = new();

    #region Fields

    private const int DrawSize = 20;
    private readonly string _drawText = string.Empty;
    private readonly ScreenData _feedBack;
    private Bitmap? _clipedArea;
    private bool _mousesWasUp;

    private Bitmap? _screenShotBmp;

    #endregion

    #region Constructors

    private ScreenShot() : base() {
        InitializeComponent();
        _feedBack = new ScreenData();
    }

    private ScreenShot(string text) : this() => _drawText = text;

    #endregion

    //private SystemInputHook Hook {
    //    [DebuggerNonUserCode]
    //    get => _Hook;
    //    [MethodImpl(MethodImplOptions.Synchronized), DebuggerNonUserCode]
    //    set {
    //        if (_Hook != null) {
    //            _Hook.MouseUp -= Hook_MouseUp;
    //            _Hook.MouseDown -= Hook_MouseDown;
    //            _Hook.MouseMove -= Hook_MouseMove;
    //        }
    //        _Hook = value;
    //        if (value != null) {
    //            _Hook.MouseUp += Hook_MouseUp;
    //            _Hook.MouseDown += Hook_MouseDown;
    //            _Hook.MouseMove += Hook_MouseMove;
    //        }
    //    }
    //}

    #region Methods

    public static Bitmap GrabAllScreens() {
        do {
            try {
                var r = Generic.RectangleOfAllScreens();
                Bitmap b = new(r.Width, r.Height, PixelFormat.Format32bppPArgb);
                using var gr = Graphics.FromImage(b);
                gr.CopyFromScreen(r.X, r.Y, 0, 0, b.Size);
                return b;
            } catch {
                Generic.CollectGarbage();
            }
        } while (true);
    }

    //public static Bitmap GrabArea(Rectangle r) => r.Width < 2 || r.Height < 2 ? null : Area(GrabAllScreens(), r);

    public static ScreenData GrabArea() => GrabArea(null);

    /// <summary>
    /// Erstellt einen Screenshot, dann kann der User einen Bereich wählen - und gibt diesen zurück.
    /// </summary>
    /// <param name="frm">Diese Form wird automatisch minimiert und wieder maximiert.</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static ScreenData GrabArea(System.Windows.Forms.Form? frm) {
        using ScreenShot x = new("Bitte ziehen sie einen Rahmen\r\num den gewünschten Bereich.");
        return x.GrabAreaInternal(frm);
    }

    protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) {
        base.OnMouseDown(e);
        if (!_mousesWasUp) { return; }
        _feedBack.Point1 = new Point(e.X, e.Y);
    }

    //public static Bitmap GrabContinuus() {
    //    ScreenShot x = new();
    //    var im = x.GrabContinuusIntern();
    //    x.Dispose();
    //    Generic.CollectGarbage();
    //    return im;
    //}
    protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs? e) {
        base.OnMouseMove(e);
        if (e != null && e.Button == System.Windows.Forms.MouseButtons.None && !_mousesWasUp) {
            _mousesWasUp = true;
            return;
        }
        var r = Generic.RectangleOfAllScreens();
        Left = r.Left;
        Top = r.Top;
        Width = r.Width;
        Height = r.Height;
        using var gr = Graphics.FromImage(BackgroundImage);
        gr.Clear(Color.Black);

        gr.DrawImage(_screenShotBmp, 0, 0);

        if (e != null) {
            PrintText(gr, e);
            Magnify(_screenShotBmp, new Point(e.X, e.Y), gr, false);
            if (e.Button != System.Windows.Forms.MouseButtons.None) {
                gr.DrawLine(new Pen(Color.Red), 0, _feedBack.Point1.Y, Width, _feedBack.Point1.Y);
                gr.DrawLine(new Pen(Color.Red), _feedBack.Point1.X, 0, _feedBack.Point1.X, Height);
                gr.DrawLine(new Pen(Color.Red), 0, e.Y, Width, e.Y);
                gr.DrawLine(new Pen(Color.Red), e.X, 0, e.X, Height);
            } else {
                gr.DrawLine(new Pen(Color.Red), 0, e.Y, Width, e.Y);
                gr.DrawLine(new Pen(Color.Red), e.X, 0, e.X, Height);
            }
        }
        Refresh();
    }

    protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
        base.OnMouseUp(e);
        if (!_mousesWasUp) {
            _mousesWasUp = true;
            return;
        }
        _feedBack.Point2 = new Point(e.X, e.Y);

        var r = _feedBack.GrabedArea();
        if (r.Width < 2 || r.Height < 2) { return; }

        _clipedArea = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppPArgb);
        using (var gr = Graphics.FromImage(_clipedArea)) {
            gr.Clear(Color.Black);
            gr.DrawImage(_screenShotBmp, 0, 0, r, GraphicsUnit.Pixel);
        }
        Close();
    }

    private ScreenData GrabAreaInternal(System.Windows.Forms.Form? frm) {
        try {
            //System.Windows.Forms.FormWindowState ws = 0;
            var op = 0d;

            QuickInfo.Close();

            if (frm != null) {
                //ws = frm.WindowState;
                op = frm.Opacity;
                frm.Opacity = 0;
                frm.Refresh();
                //Develop.DoEvents();
                //frm.WindowState = System.Windows.Forms.FormWindowState.Minimized;
                //Generic.Pause(0.5, true); // 0.3 ist zu wenig!
            }

            _screenShotBmp = GrabAllScreens();
            BackgroundImage = new Bitmap(_screenShotBmp.Width, _screenShotBmp.Height, PixelFormat.Format32bppPArgb);
            using var gr = Graphics.FromImage(BackgroundImage);
            gr.DrawImage(_screenShotBmp, 0, 0);

            var r = Generic.RectangleOfAllScreens();
            Left = r.Left;
            Top = r.Top;
            Width = r.Width;
            Height = r.Height;
            OnMouseMove(null);
            ShowDialog();

            if (frm != null) {
                frm.Opacity = op;
                //frm.WindowState = ws;
            }
            // New Bitmap davor, um die Bitmaptiefe zu korrigiern
            //if (MaxW > 0 && MaxH > 0) {
            //    // Auch hier NEW Bitmap, da evtl. das Original-Bild zurück gegeben wird.
            //    FeedBack.Pic = new Bitmap(BitmapExt.Resize(ClipedArea, MaxW, MaxH, enSizeModes.Breite_oder_Höhe_Anpassen_OhneVergrößern, InterpolationMode.HighQualityBicubic, true));
            //    FeedBack.IsResized = true;
            //} else {
            _feedBack.CloneFromBitmap(_clipedArea);
            //FeedBack.IsResized = false;
            //}
            _clipedArea.Dispose();
            return _feedBack;
        } catch {
            return new ScreenData();
        }
    }

    //private Bitmap GrabContinuusIntern() {
    //    AllS = new List<clsScreenData>();
    //    DrawText = "Bitte ziehen sie einen Rahmen um den gewünschten Bereich.\r\nDieser wird anschließend nach jedem Mauszug abfotografiert.\r\nBeendet wird der Modus mit der rechten Maustaste.";
    //    DrawSize = 10;
    //    AllS.GenerateAndAdd(GrabAreaInternal(null, -1, -1));
    //    Rahm = new Overlay();
    //    Hook = new SystemInputHook();
    //    Hook.InstallHook();
    //    HookStartPoint = new Point(int.MinValue, int.MinValue);
    //    HookEndPoint = new Point(int.MinValue, int.MinValue);
    //    LastMouse = new Point(int.MinValue, int.MinValue);
    //    Rahm.Show();
    //    do {
    //        Develop.DoEvents();
    //        if (HookFinish) { break; }
    //        if (HookStartPoint.X > int.MinValue && HookEndPoint.X > int.MinValue) {
    //            Hook.RemoveHook();
    //            if (HookStartPoint.ToString() != HookEndPoint.ToString()) {
    //                Rahm.Visible = false;
    //                clsScreenData l = new() {
    //                    Pic = GrabArea(AllS[0].GrabedArea()),
    //                    HookP1 = HookStartPoint,
    //                    HookP2 = HookEndPoint
    //                };
    //                AllS.GenerateAndAdd(l);
    //            }
    //            HookStartPoint = new Point(int.MinValue, int.MinValue);
    //            HookEndPoint = new Point(int.MinValue, int.MinValue);
    //            LastMouse = new Point(int.MinValue, int.MinValue);
    //            Generic.Pause(0.5, false);
    //            Rahm.Visible = true;
    //            Hook.InstallHook();
    //        }
    //    } while (true);
    //    Hook.RemoveHook();
    //    Rahm.Dispose();
    //    var MinX = 0;
    //    var MinY = 0;
    //    var maxx = AllS[0].GrabedArea().Width;
    //    var maxy = AllS[0].GrabedArea().Height;
    //    var VersX = 0;
    //    var VersY = 0;
    //    for (var z = 1; z < AllS.Count; z++) {
    //        VersX = VersX + AllS[z].HookP1.X - AllS[z].HookP2.X;
    //        VersY = VersY + AllS[z].HookP1.Y - AllS[z].HookP2.Y;
    //        MinX = Math.Min(VersX, MinX);
    //        MinY = Math.Min(VersY, MinY);
    //        maxx = Math.Max(VersX + AllS[0].GrabedArea().Width, maxx);
    //        maxy = Math.Max(VersY + AllS[0].GrabedArea().Height, maxy);
    //    }
    //    Generic.CollectGarbage();
    //    Bitmap bmp = new(maxx - MinX, maxy - MinY, PixelFormat.Format32bppPArgb);
    //    var gr = Graphics.FromImage(bmp);
    //    gr.Clear(Color.White);
    //    VersX = MinX * -1;
    //    VersY = MinY * -1;
    //    for (var z = 0; z < AllS.Count; z++) {
    //        VersX = VersX + AllS[z].HookP1.X - AllS[z].HookP2.X;
    //        VersY = VersY + AllS[z].HookP1.Y - AllS[z].HookP2.Y;
    //        gr.DrawImage(AllS[z].Pic, VersX, VersY);
    //        AllS[z].Pic.Dispose();
    //    }
    //    return bmp;
    //}

    //private void Hook_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
    //    if (e.Button == System.Windows.Forms.MouseButtons.Right) {
    //        HookFinish = true;
    //        return;
    //    }
    //    if (HookStartPoint.X > int.MinValue) { return; }
    //    if (e.Button == System.Windows.Forms.MouseButtons.Left) { HookStartPoint = new Point(LastMouse.X, LastMouse.Y); }
    //}

    //private void Hook_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
    //    LastMouse = new Point(e.X, e.Y);
    //    if (Rahm != null) {
    //        Rahm.Left = (int)(e.X - (Rahm.Width / 2.0));
    //        Rahm.Top = (int)(e.Y - (Rahm.Width / 2.0));
    //    }
    //}

    //private void Hook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
    //    if (e.Button == System.Windows.Forms.MouseButtons.Right) {
    //        HookFinish = true;
    //        return;
    //    }
    //    if (HookEndPoint.X > int.MinValue) { return; }
    //    if (e.Button == System.Windows.Forms.MouseButtons.Left) { HookEndPoint = new Point(LastMouse.X, LastMouse.Y); }
    //}

    private void PrintText(Graphics gr, System.Windows.Forms.MouseEventArgs? e) {
        Brush bs = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
        Brush bf = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
        Font fn = new("Arial", DrawSize, FontStyle.Bold);
        var f = gr.MeasureString(_drawText, fn);
        var yPos = e == null ? 0 : e.Y > f.Height + 50 ? 0 : (int)(Height - f.Height - 5);
        gr.FillRectangle(bs, 0, yPos - 5, Width, f.Height + 10);
        BlueFont.DrawString(gr, _drawText, fn, bf, 2, yPos + 2);
    }

    #endregion
}