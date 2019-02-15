using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using System;
using System.Drawing;

namespace BlueControls.ItemCollection.ItemCollectionList
{
    public class ObjectListItem : BasicListItem
        {

            #region  Variablen-Deklarationen 


            private IReadableText _Obj;
            private IReadableText Obj
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

            public ObjectListItem(IReadableText cobject)
            {

                Obj = cobject;

            }

            public override string Internal()
            {
                if (Obj is ColumnItem item) { return item.Name; }
                return ObjectParseable.ToString();
            }




            protected override void InitializeLevel2()
            {
                Obj = null;
            }


            #endregion


            #region  Properties 


            public IReadableText ObjectReadable
            {
                get
                {
                    return Obj;
                }
            }


            public IParseable ObjectParseable
            {
                get
                {
                    if (!(Obj is IParseable))
                    {
                        Develop.DebugPrint(enFehlerArt.Fehler, "Zugriffsverletzung, nicht Parseable");
                    }

                    return (IParseable)Obj;
                }
            }

            #endregion


            public override SizeF SizeUntouchedForListBox()
            {
                return GenericControl.Skin.FormatedText_NeededSize(Obj.ReadableText(), Obj.SymbolForReadableText(), GenericControl.Skin.GetBlueFont(Parent.ItemDesign, enStates.Standard));
            }

            protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enStates vState, bool DrawBorderAndBack)
            {

                if (DrawBorderAndBack)
                {
                    GenericControl.Skin.Draw_Back(GR, Parent.ItemDesign, vState, PositionModified, null, false);
                }

                if (Obj == null)
                {
                    GenericControl.Skin.Draw_FormatedText(GR, "Objekt nicht vorhanden", Parent.ItemDesign, vState, QuickImage.Get(enImageCode.Kritisch, 16), enAlignment.VerticalCenter_Left, PositionModified, null, false);
                }
                else
                {
                    GenericControl.Skin.Draw_FormatedText(GR, Obj.ReadableText(), Parent.ItemDesign, vState, Obj.SymbolForReadableText(), enAlignment.VerticalCenter_Left, PositionModified, null, false);
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
                SetCoordinates(new Rectangle(Convert.ToInt32(X), Convert.ToInt32(Y), Convert.ToInt32(MultiX - SliderWidth), (int)SizeUntouchedForListBox().Height));
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
