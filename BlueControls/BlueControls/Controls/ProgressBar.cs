#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
#endregion
using BlueControls.Designer_Support;
using BlueControls.Enums;
using System;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls {
    [Designer(typeof(BasicDesigner))]
    public class ProgressBar : GenericControl {
        private int wProzent = 100;

        #region Constructor
        public ProgressBar() : base(false, false) { }
        #endregion

        [DefaultValue(100)]
        public int Prozent {
            get => wProzent;
            set {
                if (value < 0) {
                    value = 0;
                }
                if (value > 100) {
                    value = 100;
                }
                if (wProzent == value) {
                    return;
                }
                wProzent = value;
                Invalidate();
            }
        }
        //Friend Overrides Sub PrepareForShowing()
        //    'Stop
        //End Sub
        // Private Sub EventDrawControl(GR as graphics, vState As enStates) Handles MyBase.DrawControl
        protected override void DrawControl(Graphics gr, enStates state) {
            Skin.Draw_Back(gr, enDesign.Progressbar, state, DisplayRectangle, this, true);
            if (wProzent > 0) {
                Rectangle r = new(DisplayRectangle.X, DisplayRectangle.Y, (int)Math.Truncate(DisplayRectangle.Width * wProzent / 100.0), DisplayRectangle.Height);
                //r = New Rectangle(DisplayRectangle)
                //r.Width = CInt(r.Width * wProzent / 100)
                Skin.Draw_Back(gr, enDesign.Progressbar_Füller, state, r, this, true);
                Skin.Draw_Border(gr, enDesign.Progressbar_Füller, state, r);
            }
            Skin.Draw_Border(gr, enDesign.Progressbar, state, DisplayRectangle);
        }
    }
}
