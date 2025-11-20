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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using System.Drawing;

namespace BlueControls.ItemCollectionList;

public class TextListItem : AbstractListItem {

    #region Fields

    private QuickImage? _symbol;

    private string _text;

    #endregion

    #region Constructors

    public TextListItem(string readableText, string keyName, QuickImage? symbol, bool isCaption, bool enabled, string userDefCompareKey) : base(keyName, enabled) {
        _isCaption = isCaption;
        _text = readableText;
        _symbol = symbol;
        UserDefCompareKey = userDefCompareKey;
    }

    #endregion

    #region Properties

    public override string QuickInfo => Text.CreateHtmlCodes();

    public QuickImage? Symbol {
        get => _symbol;
        set {
            if (_symbol == value) { return; }
            _symbol = value;
            OnPropertyChanged();
        }
    }

    public string Text {
        get => _text;
        set {
            if (_text == value) { return; }
            _text = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override bool FilterMatch(string filterText) => base.FilterMatch(filterText) || Text.ToUpperInvariant().Contains(filterText.ToUpperInvariant());

    public override int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign) => SizeUntouchedForListBox(itemdesign).Height;

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) => Skin.GetBlueFont(TempDesign(itemdesign), States.Standard).FormatedText_NeededSize(Text, Symbol, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle positionModified, Design design, States vState, bool drawBorderAndBack, bool translate) {
        var tmpd = TempDesign(design);
        if (drawBorderAndBack) {
            Skin.Draw_Back(gr, tmpd, vState, positionModified, null, false);
        }
        Skin.Draw_FormatedText(gr, Text, Symbol, Alignment.VerticalCenter_Left, positionModified, tmpd, vState, null, false, translate);
        if (drawBorderAndBack) {
            Skin.Draw_Border(gr, tmpd, vState, positionModified);
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