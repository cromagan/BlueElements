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


        public TextListItem()
        { }


        public TextListItem(string vInternalAndReadableText, bool vEnabled = true)
        {
            _Internal = vInternalAndReadableText;
            _ReadableText = vInternalAndReadableText;
            _Enabled = vEnabled;
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }


        public TextListItem(bool IsCaption, string vInternalAndReadableText)
        {
            _Internal = vInternalAndReadableText;
            _ReadableText = vInternalAndReadableText;

            _IsCaption = IsCaption;
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }

        public TextListItem(bool IsCaption, string vInternal, string vReadableText, string cUserDefCompareKey)
        {
            _Internal = vInternal;
            _ReadableText = vReadableText;
            _IsCaption = IsCaption;
            UserDefCompareKey = cUserDefCompareKey;
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }


        public TextListItem(string vInternal, string vReadableText, enDataFormat cFormat)
        {
            _Internal = vInternal;
            _ReadableText = vReadableText;
            _Format = cFormat;
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }

        public TextListItem(string vInternalAndReadableText, enDataFormat cFormat, bool vEnabled = true)
        {
            _Internal = vInternalAndReadableText;
            _ReadableText = vInternalAndReadableText;
            _Enabled = vEnabled;
            _Format = cFormat;
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }


        public TextListItem(string vInternalAndReadableText, enImageCode vSymbol, bool vEnabled = true)
        {
            _Internal = vInternalAndReadableText;
            _ReadableText = vInternalAndReadableText;
            _Symbol = QuickImage.Get(vSymbol);
            _Enabled = vEnabled;
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }


        public TextListItem(string vInternal, string vReadableText, bool vEnabled = true)
        {
            _Internal = vInternal;
            _ReadableText = vReadableText;
            _Enabled = vEnabled;
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }


        public TextListItem(string vInternal, string vReadableText, enImageCode vSymbol, bool vEnabled = true)
        {
            _Internal = vInternal;
            _ReadableText = vReadableText;
            _Symbol = QuickImage.Get(vSymbol);
            _Enabled = vEnabled;
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }


        public TextListItem(string vInternal, string vReadableText, enImageCode vSymbol, bool vEnabled, string cUserDefCompareKey)
        {
            _Internal = vInternal;
            _ReadableText = vReadableText;
            _Symbol = QuickImage.Get(vSymbol);
            _Enabled = vEnabled;
            UserDefCompareKey = cUserDefCompareKey;
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }


        public TextListItem(string vInternal, string vReadableText, QuickImage vSymbol, bool vEnabled, string cUserDefCompareKey)
        {
            _Internal = vInternal;
            _ReadableText = vReadableText;
            _Symbol = vSymbol;
            _Enabled = vEnabled;
            UserDefCompareKey = cUserDefCompareKey;
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }


        public TextListItem(string vInternal, string vReadableText, QuickImage vSymbol, bool vEnabled = true)
        {
            _Internal = vInternal;
            _ReadableText = vReadableText;
            _Symbol = vSymbol;
            _Enabled = vEnabled;
            if (string.IsNullOrEmpty(_Internal)) { Develop.DebugPrint(enFehlerArt.Fehler, "Interner Name nicht vergeben."); }
        }





        protected override void Initialize()
        {
            base.Initialize();
            base.Initialize();
            _ReadableText = string.Empty;
            _Symbol = null;
            _IsCaption = false;
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
            set
            {
                if (value == _Symbol) { return; }
                _Symbol = value;
                OnChanged();
            }
        }


        #endregion

        public override string Internal()
        {
            return _Internal;
        }



        private enDesign tempDesign()
        {
            if (_IsCaption)
            {
                switch (Parent.ItemDesign)
                {
                    case enDesign.Item_KontextMenu:
                        return enDesign.Item_KontextMenu_Caption;
                    case enDesign.Item_Listbox:
                        return enDesign.Item_Listbox_Caption;
                }
            }


            return Parent.ItemDesign;
        }


        public override SizeF SizeUntouchedForListBox()
        {
            return Skin.FormatedText_NeededSize(_ReadableText, _Symbol, Skin.GetBlueFont(tempDesign(), enStates.Standard), 16);
        }

        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enStates vState, bool DrawBorderAndBack, bool Translate)
        {

            var tmpd = tempDesign();

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
            return DataFormat.CompareKey(_Internal, _Format);
        }


        public override bool IsClickable()
        {
            return !_IsCaption;
        }


        public override void ComputePositionForListBox(enBlueListBoxAppearance IsIn, float X, float Y, float MultiX, int SliderWidth, int MaxWidth)
        {
            SetCoordinates(new Rectangle((int)(X), (int)(Y), (int)(MultiX - SliderWidth), (int)SizeUntouchedForListBox().Height));
        }


        public override SizeF QuickAndDirtySize(int PreferedHeigth)
        {
            return SizeUntouchedForListBox();
        }

        public override void DesignOrStyleChanged()
        {

        }

        protected override BasicListItem CloneLVL2()
        {
            var x = new TextListItem(_Internal, _ReadableText, _Format);
            x._Symbol = _Symbol;
            x._IsCaption = _IsCaption;
            return x;
        }

        protected override bool FilterMatchLVL2(string FilterText)
        {
            return _ReadableText.ToUpper().Contains(FilterText.ToUpper());
        }


    }
}
