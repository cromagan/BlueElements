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
using BlueBasics.Classes;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Classes.ItemCollectionList;

public class DropDownListItem : AbstractListItem {

    #region Fields

    public readonly List<AbstractListItem> DropDownItems = [];

    #endregion

    #region Constructors

    public DropDownListItem(string keyName, bool enabled, string userDefCompareKey) : base(keyName, enabled) => UserDefCompareKey = userDefCompareKey;

    #endregion

    #region Methods

    public override bool FilterMatch(string filterText) => false;

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) {
        var he = 16;

        foreach (var item in DropDownItems) {
            var s = item.HeightInControl(style, columnWidth, itemdesign);

            he = Math.Max(he, s);
        }
        return he;
    }

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) {
        var wi = 16 * 3;
        var he = 16;

        foreach (var item in DropDownItems) {
            var s = item.UntrimmedCanvasSize(itemdesign);

            wi = Math.Max(wi, s.Width);
            he = Math.Max(he, s.Height);
        }
        return new Size(wi, he);
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float zoom) {
        //var tmpd = TempDesign(design);
        if (drawBorderAndBack) {
            Skin.Draw_Back(gr, Design.ComboBox_Textbox, state, positionControl.ToRect(), null, false);
        }

        //Selected?.Draw(gr, positionControl.ControlX, positionControl.Y, design, design, vState, false, string.Empty, translate, Design.Undefiniert);

        //Skin.Draw_FormatedText(gr, Text, tmpd, vState, Symbol, Alignment.VerticalCenter_Left, positionControl, null, false, translate);
        if (drawBorderAndBack) {
            Skin.Draw_Border(gr, Design.ComboBox_Textbox, state, positionControl.ToRect());
            var but = new Rectangle((int)positionControl.Right - 16, (int)positionControl.Top, 16, 16);

            var qi = QuickImage.Get("Pfeil_Unten_Scrollbar|8|||||0");

            Button.DrawButton(null, gr, Design.Button_ComboBox, state, qi, Alignment.Horizontal_Vertical_Center, false, null, string.Empty, but, false);

            //Skin.Draw_Back(gr, Design.Button_ComboBox, vState, but, null, false);
            //Skin.Draw_Border(gr, Design.Button_ComboBox, vState, but);
        }
    }

    protected override string GetCompareKey() => KeyName.CompareKey(SortierTyp.Sprachneutral_String);

    #endregion
}