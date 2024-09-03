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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using BlueControls.CellRenderer;

namespace BlueControls.ItemCollectionList;

public class DropDownListItem : AbstractListItem {

    #region Fields

    public readonly List<AbstractListItem> DDItems = new();
    public AbstractListItem? Selected;

    #endregion

    #region Constructors

    public DropDownListItem(string keyName, bool enabled, string userDefCompareKey) : base(keyName, enabled) {
        IsCaption = false;
        UserDefCompareKey = userDefCompareKey;
    }

    #endregion

    #region Properties

    public override string QuickInfo => string.Empty;

    #endregion

    #region Methods

    public override bool FilterMatch(string filterText) => false;

    public override int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign, AbstractRenderer renderer) {
        var he = 16;

        foreach (var item in DDItems) {
            var s = item.HeightForListBox(style, columnWidth, itemdesign, renderer);

            he = Math.Max(he, s);
        }
        return he;
    }

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) {
        var wi = 16 * 3;
        var he = 16;

        foreach (var item in DDItems) {
            var s = item.SizeUntouchedForListBox(itemdesign);

            wi = Math.Max(wi, s.Width);
            he = Math.Max(he, s.Height);
        }
        return new Size(wi, he);
    }

    protected override void DrawExplicit(Graphics gr, Rectangle positionModified, Design design, States vState, bool drawBorderAndBack, bool translate) {
        //var tmpd = TempDesign(design);
        if (drawBorderAndBack) {
            Skin.Draw_Back(gr, Design.ComboBox_Textbox, vState, positionModified, null, false);
        }

        Selected?.Draw(gr, positionModified.X, positionModified.Y, design, design, vState, false, string.Empty, translate, Design.Undefiniert);

        //Skin.Draw_FormatedText(gr, Text, tmpd, vState, Symbol, Alignment.VerticalCenter_Left, positionModified, null, false, translate);
        if (drawBorderAndBack) {
            Skin.Draw_Border(gr, Design.ComboBox_Textbox, vState, positionModified);
            var but = new Rectangle(positionModified.Right - 16, positionModified.Top, 16, 16);

            var qi = QuickImage.Get("Pfeil_Unten_Scrollbar|8|||||0");

            Controls.Button.DrawButton(null, gr, Design.Button_ComboBox, vState, qi, Alignment.Horizontal_Vertical_Center, false, null, null, but, false);

            //Skin.Draw_Back(gr, Design.Button_ComboBox, vState, but, null, false);
            //Skin.Draw_Border(gr, Design.Button_ComboBox, vState, but);
        }
    }

    protected override string GetCompareKey() => KeyName.CompareKey(SortierTyp.Sprachneutral_String);

    #endregion
}