#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
using BlueBasics.Interfaces;
using BlueControls.Enums;
using System.Drawing;

namespace BlueControls.ItemCollection
{
    public class TextListItem : BasicListItem
    {

        #region  Variablen-Deklarationen 
        private string _ReadableText;
        private QuickImage _Symbol;
        private readonly enDataFormat _Format = enDataFormat.Text;
        private bool _IsCaption;


        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 

        public TextListItem(IReadableText obj) : this(string.Empty, obj.ReadableText(), obj.SymbolForReadableText())
        {
            Tags = obj;
        }
        public TextListItem(string internalname, IReadableText obj) : this(internalname, obj.ReadableText(), obj.SymbolForReadableText())
        {
            Tags = obj;
        }

        public TextListItem(string internalname, string readableText, bool isCaption, string userDefCompareKey) : this(internalname, readableText, null, isCaption, true, enDataFormat.Text, userDefCompareKey) { }



        public TextListItem(string internalAndReadableText, bool isCaption) : this(internalAndReadableText, internalAndReadableText, null, isCaption, true, enDataFormat.Text, string.Empty) { }





        public TextListItem(string internalAndReadableText) : this(internalAndReadableText, internalAndReadableText, null, false, true, enDataFormat.Text, string.Empty) { }



        public TextListItem(string internalAndReadableText, enDataFormat format) : this(internalAndReadableText, internalAndReadableText, null, false, true, format, string.Empty) { }


        public TextListItem(string internalAndReadableText, enImageCode symbol) : this(internalAndReadableText, internalAndReadableText, symbol, false, true, enDataFormat.Text, string.Empty) { }



        public TextListItem(string internalname, string readableText, bool enabled) : this(internalname, readableText, null, false, enabled, enDataFormat.Text, string.Empty) { }

        public TextListItem(string internalname, string readableText, enImageCode symbol, bool enabled) : this(internalname, readableText, symbol, false, enabled, enDataFormat.Text, string.Empty) { }

        public TextListItem(string internalname, string readableText, enImageCode symbol, bool enabled, string userDefCompareKey) : this(internalname, readableText, symbol, false, enabled, enDataFormat.Text, userDefCompareKey) { }

        public TextListItem(string internalname, string readableText, QuickImage symbol, bool enabled) : this(internalname, readableText, symbol, false, enabled, enDataFormat.Text, string.Empty) { }

        public TextListItem(string internalname, string readableText, QuickImage symbol, bool enabled, string userDefCompareKey) : this(internalname, readableText, symbol, false, enabled, enDataFormat.Text, userDefCompareKey) { }





        public TextListItem(string internalname, string readableText) : this(internalname, readableText, null, false, true, enDataFormat.Text, string.Empty) { }

        public TextListItem(string internalname, string readableText, enImageCode symbol) : this(internalname, readableText, symbol, false, true, enDataFormat.Text, string.Empty) { }


        public TextListItem(string internalname, string readableText, QuickImage symbol) : this(internalname, readableText, symbol, false, true, enDataFormat.Text, string.Empty) { }




        public TextListItem(string internalname, string readableText, enImageCode symbol, bool isCaption, bool enabled, enDataFormat format, string userDefCompareKey) : this(internalname, readableText, null, isCaption, enabled, format, userDefCompareKey)
        {
            _Symbol = QuickImage.Get(symbol, 16);
            //if (_Symbol != null) { _SymbolDisabled = QuickImage.Get(_Symbol, Skin.AdditionalState(enStates.Standard_Disabled)); }
        }

        public TextListItem(string internalname, string readableText, QuickImage symbol, bool isCaption, bool enabled, enDataFormat format, string userDefCompareKey) : base(internalname)
        {
            _IsCaption = isCaption;
            _ReadableText = readableText;
            _Symbol = symbol;
            _Enabled = enabled;
            _Format = format;
            UserDefCompareKey = userDefCompareKey;
        }





        #endregion


        #region  Properties 


        public string Text
        {
            get
            {
                return _ReadableText;
            }
            set
            {
                if (value == _ReadableText) { return; }
                _ReadableText = value;
                OnChanged();
            }
        }


        public QuickImage Symbol
        {
            get
            {
                return _Symbol;
            }
            //set
            //{
            //    if (value == _Symbolx) { return; }
            //    _Symbolx = value;
            //    OnChanged();
            //}
        }


        #endregion



        private enDesign tempDesign(enDesign itemdesign)
        {
            if (_IsCaption)
            {
                switch (itemdesign)
                {
                    case enDesign.Item_KontextMenu:
                        return enDesign.Item_KontextMenu_Caption;
                    case enDesign.Item_Listbox:
                        return enDesign.Item_Listbox_Caption;
                }
            }


            return itemdesign;
        }


        public override SizeF SizeUntouchedForListBox()
        {
            return Skin.FormatedText_NeededSize(_ReadableText, _Symbol, Skin.GetBlueFont(tempDesign(((ItemCollectionList)Parent).ItemDesign), enStates.Standard), 16);
        }

        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enDesign design, enStates vState, bool DrawBorderAndBack, bool Translate)
        {

            var tmpd = tempDesign(design);

            if (DrawBorderAndBack)
            {
                Skin.Draw_Back(GR, tmpd, vState, PositionModified, null, false);
            }

            Skin.Draw_FormatedText(GR, _ReadableText, tmpd, vState, _Symbol, enAlignment.VerticalCenter_Left, PositionModified, null, false, Translate);

            if (DrawBorderAndBack)
            {
                Skin.Draw_Border(GR, tmpd, vState, PositionModified);
            }
        }



        protected override string GetCompareKey()
        {
            return DataFormat.CompareKey(Internal, _Format);
        }


        public override bool IsClickable()
        {
            return !_IsCaption;
        }


        public override void ComputePositionForListBox(enBlueListBoxAppearance IsIn, float X, float Y, float MultiX, int SliderWidth, int MaxWidth)
        {
            SetCoordinates(new Rectangle((int)X, (int)Y, (int)(MultiX - SliderWidth), (int)SizeUntouchedForListBox().Height));
        }


        public override SizeF QuickAndDirtySize(int PreferedHeigth)
        {
            return SizeUntouchedForListBox();
        }

        public override void DesignOrStyleChanged()
        {

        }

        public override object Clone()
        {
            return GetCloneData(new TextListItem(Internal, _ReadableText, _Symbol, _IsCaption, _Enabled, _Format, UserDefCompareKey));
        }

        public override bool FilterMatch(string FilterText)
        {
            if (base.FilterMatch(FilterText)) { return true; }
            return _ReadableText.ToUpper().Contains(FilterText.ToUpper());
        }
    }
}
