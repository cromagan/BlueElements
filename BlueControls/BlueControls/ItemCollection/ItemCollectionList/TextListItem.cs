// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

namespace BlueControls.ItemCollection {

    public class TextListItem : BasicListItem {

        #region Constructors

        public TextListItem(string readableText, string internalname, QuickImage? symbol, bool isCaption, bool enabled, string userDefCompareKey) : base(internalname) {
            IsCaption = isCaption;
            Text = readableText;
            Symbol = symbol;
            _Enabled = enabled;
            //_Format = format;
            UserDefCompareKey = userDefCompareKey;
        }

        #endregion

        #region Properties

        public override string QuickInfo => Text.CreateHtmlCodes(true);

        public QuickImage? Symbol { get; set; }

        public string Text { get; set; }

        #endregion

        #region Methods

        public override object Clone() => new TextListItem(Text, Internal, Symbol, IsCaption, _Enabled, UserDefCompareKey);

        public override bool FilterMatch(string FilterText) => base.FilterMatch(FilterText) || Text.ToUpper().Contains(FilterText.ToUpper());

        public override int HeightForListBox(enBlueListBoxAppearance style, int columnWidth) => SizeUntouchedForListBox().Height;

        protected override Size ComputeSizeUntouchedForListBox() => Skin.FormatedText_NeededSize(Text, Symbol, Skin.GetBlueFont(tempDesign(Parent.ItemDesign), enStates.Standard), 16);

        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enDesign design, enStates vState, bool DrawBorderAndBack, bool Translate) {
            var tmpd = tempDesign(design);
            if (DrawBorderAndBack) {
                Skin.Draw_Back(GR, tmpd, vState, PositionModified, null, false);
            }
            Skin.Draw_FormatedText(GR, Text, tmpd, vState, Symbol, enAlignment.VerticalCenter_Left, PositionModified, null, false, Translate);
            if (DrawBorderAndBack) {
                Skin.Draw_Border(GR, tmpd, vState, PositionModified);
            }
        }

        protected override string GetCompareKey() => Internal.CompareKey(enSortierTyp.Sprachneutral_String);

        private enDesign tempDesign(enDesign itemdesign) {
            if (IsCaption) {
                switch (itemdesign) {
                    case enDesign.Item_KontextMenu:
                        return enDesign.Item_KontextMenu_Caption;

                    case enDesign.Item_Listbox:
                        return enDesign.Item_Listbox_Caption;
                }
            }
            return itemdesign;
        }

        #endregion
    }
}