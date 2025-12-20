// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueBasics.Enums;
using BlueControls.Enums;
using System.Drawing;

namespace BlueControls.ItemCollectionList;

public class TextListItem : AbstractListItem {

    #region Constructors

    public TextListItem(string readableText, string keyName, QuickImage? symbol, bool isCaption, bool enabled, string userDefCompareKey) : base(keyName, enabled) {
        IsCaption = isCaption;
        Text = readableText;
        Symbol = symbol;
        UserDefCompareKey = userDefCompareKey;
    }

    #endregion

    #region Properties

    public bool IsCaption {
        get;
        protected set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public override string QuickInfo => Text.CreateHtmlCodes();

    public QuickImage? Symbol {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public string Text {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override bool FilterMatch(string filterText) => base.FilterMatch(filterText) || Text.ContainsIgnoreCase(filterText);

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => UntrimmedCanvasSize(itemdesign).Height;

    public override bool IsClickable() => !IsCaption && base.IsClickable();

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => Skin.GetBlueFont(TempDesign(itemdesign), States.Standard).FormatedText_NeededSize(Text, Symbol, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float scale) {
        var tmpd = TempDesign(itemdesign);
        if (drawBorderAndBack) {
            Skin.Draw_Back(gr, tmpd, state, positionControl.ToRect(), null, false);
        }
        Skin.Draw_FormatedText(gr, Text, Symbol, Alignment.VerticalCenter_Left, positionControl.ToRect(), tmpd, state, null, false, translate);
        if (drawBorderAndBack) {
            Skin.Draw_Border(gr, tmpd, state, positionControl.ToRect());
        }
    }

    protected override string GetCompareKey() => KeyName.CompareKey(SortierTyp.Sprachneutral_String);

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