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
using Form = System.Windows.Forms.Form;
using BlueControls.Enums;

namespace BlueControls {

    public sealed partial class ScreenShot : Form {

        #region Fields

        private readonly string _drawText = string.Empty;
        private readonly ScreenData _feedBack;
        private readonly bool _onlyMouseDown;

        private Helpers _helpers = Helpers.None;

        #endregion

        #region Constructors

        public ScreenShot() : base() {
            InitializeComponent();
            _feedBack = new ScreenData();
        }

        private ScreenShot(string text, bool onlyMouseDown, Helpers helper) : this() {
            _drawText = text;
            _onlyMouseDown = onlyMouseDown;
            _helpers = helper;
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
        /// Erstellt einen Screenshot, dann kann der User einen Bereich w�hlen - und gibt diesen zur�ck.
        /// </summary>
        /// <param name="frm">Diese Form wird automatisch minimiert und wieder maximiert.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static ScreenData GrabArea(Form? frm) {
            using ScreenShot x = new("Bitte ziehen sie einen Rahmen\r\num den gew�nschten Bereich.", false, Helpers.DrawRectangle | Helpers.Magnifier);
            return x.Start(frm);
        }

        internal static ScreenData GrabAndClick(string txt, Form? frm, Helpers helper) {
            using ScreenShot x = new(txt, true, helper);
            return x.Start(frm);
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

                // Hole das Rectangle aller Screens
                var r = Generic.RectangleOfAllScreens();

                // Setze die Form-Position und Gr��e VOR dem Screen-Grab
                this.StartPosition = FormStartPosition.Manual;
                this.Left = r.Left;
                this.Top = r.Top;
                this.Width = r.Width;
                this.Height = r.Height;

                // F�hre den Screenshot durch
                _feedBack.Screen = GrabAllScreens();
                zoomPic.Bmp = _feedBack.Screen;
                zoomPic.InfoText = _drawText;
                zoomPic.Helper = _helpers;

                // Zeige die Form
                _ = ShowDialog();

                if (frm != null) { frm.Opacity = op; }

                return _feedBack;
            } catch {
                return new ScreenData();
            }
        }

        private void zoomPic_MouseDown(object sender, MouseEventArgs e) {
            _feedBack.Point1 = new Point(e.X, e.Y);

            if (_onlyMouseDown) {
                Close();
            }
        }

        private void zoomPic_MouseUp(object sender, MouseEventArgs e) {
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

        #endregion
    }
}