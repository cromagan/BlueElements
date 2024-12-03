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

using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using BlueBasics;
using BlueControls.Forms;
using static BlueBasics.BitmapExt;
using Form = System.Windows.Forms.Form;

namespace BlueControls;

/// <summary>
/// Eine Klasse, die alle möglichen Arten von Screenshots zurückgibt.
/// </summary>
public sealed partial class ScreenShot {

    #region Fields

    private const int DrawSize = 20;
    private readonly string _drawText = string.Empty;
    private readonly ScreenData _feedBack;
    private readonly bool _onlyMouseDown;
    private bool _mousesWasUp;

    #endregion

    #region Constructors

    private ScreenShot() : base() {
        InitializeComponent();
        _feedBack = new ScreenData();
    }

    private ScreenShot(string text, bool onlyMouseDown) : this() {
        _drawText = text;
        _onlyMouseDown = onlyMouseDown;
    }

    #endregion

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

    /// <summary>
    /// Erstellt einen Screenshot, dann kann der User einen Bereich wählen - und gibt diesen zurück.
    /// </summary>
    /// <param name="frm">Diese Form wird automatisch minimiert und wieder maximiert.</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static ScreenData GrabArea(Form? frm) {
        using ScreenShot x = new("Bitte ziehen sie einen Rahmen\r\num den gewünschten Bereich.", false);
        return x.Start(frm);
    }

    internal static ScreenData GrabAndClick(string txt, Form? frm) {
        using ScreenShot x = new(txt, true);
        return x.Start(frm);
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        if (!_mousesWasUp) { return; }
        _feedBack.Point1 = new Point(e.X, e.Y);

        if (_onlyMouseDown) {
            Close();
        }
    }

    protected override void OnMouseMove(MouseEventArgs? e) {
        if (e == null || _feedBack.Screen == null) { return; }

        base.OnMouseMove(e);
        if (e.Button == MouseButtons.None && !_mousesWasUp) {
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

        gr.DrawImage(_feedBack.Screen, 0, 0);

        PrintText(gr, e);
        Magnify(_feedBack.Screen, new Point(e.X, e.Y), gr, false);
        if (e.Button != MouseButtons.None) {
            gr.DrawLine(new Pen(Color.Red), 0, _feedBack.Point1.Y, Width, _feedBack.Point1.Y);
            gr.DrawLine(new Pen(Color.Red), _feedBack.Point1.X, 0, _feedBack.Point1.X, Height);
            gr.DrawLine(new Pen(Color.Red), 0, e.Y, Width, e.Y);
            gr.DrawLine(new Pen(Color.Red), e.X, 0, e.X, Height);
        } else {
            gr.DrawLine(new Pen(Color.Red), 0, e.Y, Width, e.Y);
            gr.DrawLine(new Pen(Color.Red), e.X, 0, e.X, Height);
        }

        Refresh();
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        if (!_mousesWasUp) {
            _mousesWasUp = true;
            return;
        }
        _feedBack.Point2 = new Point(e.X, e.Y);

        var r = _feedBack.AreaRectangle();
        if (r.Width < 2 || r.Height < 2) { return; }

        _feedBack.Area = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppPArgb);

        if (_feedBack.Screen != null) {
            using var gr = Graphics.FromImage(_feedBack.Area);
            gr.Clear(Color.Black);
            gr.DrawImage(_feedBack.Screen, 0, 0, r, GraphicsUnit.Pixel);
        }

        Close();
    }

    private void PrintText(Graphics gr, MouseEventArgs? e) {
        Brush bs = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
        Brush bf = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
        Font fn = new("Arial", DrawSize, FontStyle.Bold);
        var f = gr.MeasureString(_drawText, fn);
        var yPos = e == null ? 0 : e.Y > f.Height + 50 ? 0 : (int)(Height - f.Height - 5);
        gr.FillRectangle(bs, 0, yPos - 5, Width, f.Height + 10);
        BlueFont.DrawString(gr, _drawText, fn, bf, 2, yPos + 2);
    }

    private ScreenData Start(Form? frm) {
        try {
            var op = 0d;

            QuickInfo.Close();

            if (frm != null) {
                op = frm.Opacity;
                frm.Opacity = 0;
                frm.Refresh();
            }

            _feedBack.Screen = GrabAllScreens();
            BackgroundImage = new Bitmap(_feedBack.Screen.Width, _feedBack.Screen.Height, PixelFormat.Format32bppPArgb);
            using var gr = Graphics.FromImage(BackgroundImage);
            gr.DrawImage(_feedBack.Screen, 0, 0);

            var r = Generic.RectangleOfAllScreens();
            Left = r.Left;
            Top = r.Top;
            Width = r.Width;
            Height = r.Height;
            OnMouseMove(null);
            _ = ShowDialog();

            if (frm != null) { frm.Opacity = op; }

            return _feedBack;
        } catch {
            return new ScreenData();
        }
    }

    #endregion
}