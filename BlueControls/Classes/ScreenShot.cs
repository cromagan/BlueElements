// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.DrawingHelpers;
using BlueControls.EventArgs;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace BlueControls;

public sealed partial class ScreenShot : Form {

    #region Fields

    private readonly string _drawText = string.Empty;

    private readonly ScreenData _feedBack;

    private readonly List<DrawingHelper> _helpers = [];

    private readonly bool _onlyMouseDown;

    #endregion

    #region Constructors

    public ScreenShot() : base() {
        InitializeComponent();
        _feedBack = new ScreenData();
    }

    private ScreenShot(string text, bool onlyMouseDown, IEnumerable<DrawingHelper> helpers) : this() {
        _drawText = text;
        _onlyMouseDown = onlyMouseDown;
        _helpers = [.. helpers];
    }

    #endregion

    #region Methods

    public static Bitmap GrabAllScreens() {
        do {
            try {
                // Virtuelles Rechteck aller Bildschirme in logischen Pixeln
                var r = SystemInformation.VirtualScreen;
                var bmp = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppPArgb);
                using var gr = Graphics.FromImage(bmp);
                gr.CopyFromScreen(r.X, r.Y, 0, 0, bmp.Size);
                return bmp;
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
        using var x = new ScreenShot("Bitte ziehen sie einen Rahmen\r\num den gewünschten Bereich.", false, [DrawingHelper_DrawRectangle.Instance, DrawingHelper_Magnifier.Instance]);
        return x.Start(frm);
    }

    internal static ScreenData GrabAndClick(string txt, Form? frm, IEnumerable<DrawingHelper> helpers) {
        using var x = new ScreenShot(txt, true, helpers);
        return x.Start(frm);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            zoomPic?.Dispose();
        }
        base.Dispose(disposing);
    }

    private ScreenData Start(Form? frm) {
        try {
            var op = 0d;

            QuickInfo.Close();

            if (frm is not null) {
                op = frm.Opacity;
                frm.Opacity = 0;
                frm.Refresh();
            }

            // Hole das Rectangle aller Screens in logischen Pixeln
            var r = SystemInformation.VirtualScreen;

            // Setze die Form-CanvasPosition und Größe VOR dem Screen-Grab
            StartPosition = FormStartPosition.Manual;
            Left = r.Left;
            Top = r.Top;
            Width = r.Width;
            Height = r.Height;

            // Führe den Screenshot durch
            _feedBack.Screen = GrabAllScreens();
            zoomPic.Bmp = _feedBack.Screen;
            zoomPic.CanvasMargin = 0;
            zoomPic.InfoText = _drawText;
            zoomPic.Helpers.Clear();
            zoomPic.Helpers.AddRange(_helpers);

            // Zeige die Form
            ShowDialog();

            frm?.Opacity = op;

            return _feedBack;
        } catch {
            return new ScreenData();
        }
    }

    private void ZoomPic_ImageMouseDown(object sender, TrimmedCanvasMouseEventArgs e) {
        _feedBack.Point1 = new Point(e.TrimmedCanvasX, e.TrimmedCanvasY);

        if (_onlyMouseDown) {
            Close();
        }
    }

    private void ZoomPic_ImageMouseUp(object sender, TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e) {
        _feedBack.Point2 = new Point(e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY);

        var r = _feedBack.AreaRectangle();
        if (r.Width < 2 || r.Height < 2) { return; }

        _feedBack.Area = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppPArgb);

        if (_feedBack.Screen is not null) {
            using var gr = Graphics.FromImage(_feedBack.Area);
            gr.Clear(Color.Black);
            gr.DrawImage(_feedBack.Screen, 0, 0, r, GraphicsUnit.Pixel);
        }

        Close();
    }

    #endregion
}