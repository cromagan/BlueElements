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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using System.Drawing;

namespace BlueControls.ItemCollection {
    public class TextListItem : BasicListItem {

        #region  Variablen-Deklarationen 
        private string _ReadableText;
        private QuickImage _Symbol;
        //private readonly enDataFormat _Format = enDataFormat.Text;

        #endregion

        #region  Event-Deklarationen + Delegaten 

        #endregion

        #region  Construktor + Initialize 

        public TextListItem(string readableText, string internalname, QuickImage symbol, bool isCaption, bool enabled, string userDefCompareKey) : base(internalname) {
            IsCaption = isCaption;
            _ReadableText = readableText;
            _Symbol = symbol;
            _Enabled = enabled;
            //_Format = format;
            UserDefCompareKey = userDefCompareKey;
        }

        #endregion

        #region  Properties 

        public override string QuickInfo => _ReadableText.CreateHtmlCodes(true);

        public string Text {
            get => _ReadableText;
            set {
                if (value == _ReadableText) { return; }
                _ReadableText = value;
                //OnChanged();
            }
        }

        public QuickImage Symbol {
            get => _Symbol;
            set {
                if (value == _Symbol) { return; }
                _Symbol = value;
                //OnChanged();
            }
        }

        #endregion

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

        protected override Size ComputeSizeUntouchedForListBox() {
            return Skin.FormatedText_NeededSize(_ReadableText, _Symbol, Skin.GetBlueFont(tempDesign(Parent.ItemDesign), enStates.Standard), 16);
        }

        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enDesign design, enStates vState, bool DrawBorderAndBack, bool Translate) {

            var tmpd = tempDesign(design);

            if (DrawBorderAndBack) {
                Skin.Draw_Back(GR, tmpd, vState, PositionModified, null, false);
            }

            Skin.Draw_FormatedText(GR, _ReadableText, tmpd, vState, _Symbol, enAlignment.VerticalCenter_Left, PositionModified, null, false, Translate);

            if (DrawBorderAndBack) {
                Skin.Draw_Border(GR, tmpd, vState, PositionModified);
            }
        }

        protected override string GetCompareKey() {
            return DataFormat.CompareKey(Internal, enDataFormat.Text);
        }

        public override int HeightForListBox(enBlueListBoxAppearance style, int columnWidth) {
            return SizeUntouchedForListBox().Height;
        }

        public override void CloneToNewCollection(ItemCollectionList newParent) {
            CloneToNewCollection(newParent, new TextListItem(_ReadableText, Internal, _Symbol, IsCaption, _Enabled, UserDefCompareKey));
        }

        public override bool FilterMatch(string FilterText) {
            return base.FilterMatch(FilterText) || _ReadableText.ToUpper().Contains(FilterText.ToUpper());
        }
    }
}
