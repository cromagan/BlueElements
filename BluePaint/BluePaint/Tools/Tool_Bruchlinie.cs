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

using BlueControls.Controls;
using System;
using System.Drawing;

namespace BluePaint;

public partial class Tool_Bruchlinie {

    #region Constructors

    public Tool_Bruchlinie() : base() => InitializeComponent();

    #endregion

    #region Methods

    private void Bruch_Click(object sender, System.EventArgs e) {
        var pic = OnNeedCurrentPic();
        if (pic == null) { return; }
        var xRi = Convert.ToInt32(pic.Width / 10.0);
        var yri = Convert.ToInt32(pic.Height / 10.0);
        var changeY = false;
        var changeX = false;
        var modX = 0;
        var modY = 0;
        OnForceUndoSaving();
        Point nach;
        switch (((Button)sender).Name.ToLowerInvariant()) {
            case "bruch_oben":
                nach = new Point(0, 5);
                yri = -5;
                modY = -5;
                changeY = true;
                break;

            case "bruch_unten":
                nach = new Point(0, pic.Height - 6);
                yri = 5;
                modY = 5;
                changeY = true;
                break;

            case "bruch_links":
                nach = new Point(5, 0);
                xRi = -5;
                modX = -5;
                changeX = true;
                break;

            case "bruch_rechts":
                nach = new Point(pic.Width - 6, 0);
                xRi = 5;
                modX = 5;
                changeX = true;
                break;

            default:
                return;
        }
        var gr = Graphics.FromImage(pic);
        for (var z = 0; z <= 10; z++) {
            var von = nach;
            nach.X += xRi;
            nach.Y += yri;
            for (var x1 = -1; x1 <= 1; x1++) {
                for (var y1 = -1; y1 <= 1; y1++) {
                    gr.DrawLine(new Pen(Color.FromArgb(255, 255, 255), 8), von.X + modX + x1, von.Y + modY + y1, nach.X + modX + x1, nach.Y + modY + y1);
                }
            }
            gr.DrawLine(new Pen(Color.FromArgb(128, 128, 128)), von, nach);
            if (changeX) { xRi *= -1; }
            if (changeY) { yri *= -1; }
        }
        OnDoInvalidate();
    }

    #endregion
}