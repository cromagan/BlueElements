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
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;
using System;
using System.Drawing;

namespace BlueControls.ItemCollection
{
    public class ObjectListItem : BasicListItem
    {

        #region  Variablen-Deklarationen 


        private IObjectWithDialog _Obj;
        public IObjectWithDialog Obj
        {
            get
            {
                return _Obj;
            }
            set
            {
                if (_Obj != null) { _Obj.Changed -= Obj_Changed; }
                _Obj = value;
                if (value != null) { _Obj.Changed += Obj_Changed; }
            }
        }

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 

        public ObjectListItem(IObjectWithDialog cobject)
        {

            Obj = cobject;

        }

        public override string Internal()
        {
            if (Obj is ColumnItem item) { return item.Name; }
            return Obj.ToString();
        }




        protected override void Initialize()
        {
            base.Initialize();
            Obj = null;
        }


        #endregion


        #region  Properties 


        //public IReadableText ObjectReadable
        //{
        //    get
        //    {
        //        return Obj;
        //    }
        //}


        //public IParseable ObjectParseable
        //{
        //    get
        //    {
        //        if (!(Obj is IParseable))
        //        {
        //            Develop.DebugPrint(enFehlerArt.Fehler, "Zugriffsverletzung, nicht Parseable");
        //        }

        //        return (IParseable)Obj;
        //    }
        //}

        #endregion


        public override SizeF SizeUntouchedForListBox()
        {
            return GenericControl.Skin.FormatedText_NeededSize(Obj.ReadableText(), Obj.SymbolForReadableText(), GenericControl.Skin.GetBlueFont(Parent.ItemDesign, enStates.Standard), 16);
        }

        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enStates vState, bool DrawBorderAndBack, bool Translate)
        {

            if (DrawBorderAndBack)
            {
                GenericControl.Skin.Draw_Back(GR, Parent.ItemDesign, vState, PositionModified, null, false);
            }

            if (Obj == null)
            {
                GenericControl.Skin.Draw_FormatedText(GR, "Objekt nicht vorhanden", Parent.ItemDesign, vState, QuickImage.Get(enImageCode.Kritisch, 16), enAlignment.VerticalCenter_Left, PositionModified, null, false, Translate);
            }
            else
            {
                GenericControl.Skin.Draw_FormatedText(GR, Obj.ReadableText(), Parent.ItemDesign, vState, Obj.SymbolForReadableText(), enAlignment.VerticalCenter_Left, PositionModified, null, false, false);
            }

            if (DrawBorderAndBack)
            {
                GenericControl.Skin.Draw_Border(GR, Parent.ItemDesign, vState, PositionModified);
            }
        }



        protected override string GetCompareKey()
        {
            if (Obj is ICompareKey key) { return key.CompareKey(); }
            return DataFormat.CompareKey(_Internal, enDataFormat.Text);
        }




        public override bool IsClickable()
        {
            return true;
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
        private void Obj_Changed(object sender, System.EventArgs e)
        {
            OnChanged();
        }

        protected override BasicListItem CloneLVL2()
        {
            return new ObjectListItem(Obj);
        }

        protected override bool FilterMatchLVL2(string FilterText)
        {
            return Obj.ReadableText().ToUpper().Contains(FilterText.ToUpper());
        }

    }
}
