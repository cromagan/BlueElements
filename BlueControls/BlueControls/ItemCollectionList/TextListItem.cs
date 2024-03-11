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
using System.Drawing;

namespace BlueControls.ItemCollectionList;

public class TextListItem : AbstractListItem {

    #region Constructors

    public TextListItem(string readableText, string internalname, QuickImage? symbol, bool isCaption, bool enabled, string userDefCompareKey) : base(internalname, enabled) {
        IsCaption = isCaption;
        Text = readableText;
        Symbol = symbol;
        UserDefCompareKey = userDefCompareKey;
    }

    #endregion

    #region Properties

    public override string QuickInfo => Text.CreateHtmlCodes(true);

    public QuickImage? Symbol { get; set; }

    public string Text { get; set; }

    #endregion

    #region Methods

    public override object Clone() {
        var l = new TextListItem(Text, Internal, Symbol, IsCaption, Enabled, UserDefCompareKey);
        l.CloneBasicStatesFrom(this);
        return l;
    }

    public override bool FilterMatch(string filterText) => base.FilterMatch(filterText) || Text.ToUpper().Contains(filterText.ToUpper());

    public override int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign) => SizeUntouchedForListBox(itemdesign).Height;

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) => ((Font)Skin.GetBlueFont(TempDesign(itemdesign), States.Standard)).FormatedText_NeededSize(Text, Symbol, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle positionModified, Design design, States vState, bool drawBorderAndBack, bool translate) {
        var tmpd = TempDesign(design);
        if (drawBorderAndBack) {
            Skin.Draw_Back(gr, tmpd, vState, positionModified, null, false);
        }
        Skin.Draw_FormatedText(gr, Text, tmpd, vState, Symbol, Alignment.VerticalCenter_Left, positionModified, null, false, translate);
        if (drawBorderAndBack) {
            Skin.Draw_Border(gr, tmpd, vState, positionModified);
        }
    }

    protected override string GetCompareKey() => Internal.CompareKey(SortierTyp.Sprachneutral_String);

    private Design TempDesign(Design itemdesign) {
        if (IsCaption) {
            switch (itemdesign) {
                case Design.Item_KontextMenu:
                    return Design.Item_KontextMenu_Caption;

                case Design.Item_Listbox:
                    return Design.Item_Listbox_Caption;
            }
        }
        return itemdesign;
    }

    #endregion
}