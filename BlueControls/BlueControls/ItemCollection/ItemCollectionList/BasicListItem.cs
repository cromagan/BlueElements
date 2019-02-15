using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.ItemCollection.Basics;
using System;
using System.Drawing;

namespace BlueControls.ItemCollection.ItemCollectionList
{
    public abstract class BasicListItem : BasicItem, ICompareKey, IComparable, ICloneable
        {
            public abstract SizeF SizeUntouchedForListBox();

            public abstract bool IsClickable();

            public abstract void ComputePositionForListBox(enBlueListBoxAppearance IsIn, float X, float Y, float MultiX, int SliderWidth, int MaxWidth);

            public abstract SizeF QuickAndDirtySize(int PreferedHeigth);


            protected abstract string GetCompareKey();

            protected abstract void DrawExplicit(Graphics GR, Rectangle PositionModified, enStates vState, bool DrawBorderAndBack);


            protected abstract bool FilterMatchLVL2(string FilterText);

            public ItemCollectionList Parent;


            protected abstract BasicListItem CloneLVL2();

            /// <summary>
            /// Ist das Item markiert/selektiert?
            /// </summary>
            /// <remarks></remarks>
            private bool _Checked;

            public Rectangle Pos;
            private string _UserDefCompareKey = "";


            /// <summary>
            /// Ist das Item enabled?
            /// </summary>
            /// <remarks></remarks>
            protected bool _Enabled = true;


            protected BasicListItem()
            {
                Initialize();
            }


            public new void Initialize()
            {
                base.Initialize();

                _Checked = false;
                Pos = new Rectangle(0, 0, 0, 0);
                _UserDefCompareKey = string.Empty;
            }


            public bool Contains(int x, int Y)
            {
                return Pos.Contains(x, Y);
            }





            public void SetCoordinates(Rectangle r)
            {

                Pos = r;
            }


            public bool Enabled
            {
                get
                {
                    return _Enabled;
                }
                set
                {
                    _Enabled = value;
                }
            }


            public string CompareKey()
            {

                if (!string.IsNullOrEmpty(_UserDefCompareKey))
                {
                    if (Convert.ToChar(_UserDefCompareKey.Substring(0, 1)) < 32) { Develop.DebugPrint("Sortierung inkorrekt: " + _UserDefCompareKey); }
                    return _UserDefCompareKey + Constants.FirstSortChar + Parent.IndexOf(this).Nummer(6);
                }

                return GetCompareKey();
            }


            public string UserDefCompareKey
            {
                get
                {
                    return _UserDefCompareKey;
                }
                set
                {
                    if (value == _UserDefCompareKey) { return; }
                    _UserDefCompareKey = value;

                    OnChanged();
                }
            }


            public bool Checked
            {
                get
                {
                    return _Checked;
                }
                set
                {
                    Parent.SetNewCheckState(this, value, ref _Checked);
                    OnChanged();
                }
            }


            //protected enStates ModifyState(enStates vState)
            //{
            //    enStates ItemState = 0;


            //    if (Convert.ToBoolean(vState & enStates.Standard_Disabled) || !_Enabled)
            //    {
            //        ItemState = enStates.Standard_Disabled;
            //    }
            //    else
            //    {
            //        ItemState = enStates.Standard;
            //        if (Convert.ToBoolean(vState & enStates.Standard_MouseOver)) { ItemState |= enStates.Standard_MouseOver; }
            //    }


            //    if (_Checked) { ItemState |= enStates.Checked; }


            //    return ItemState;
            //}


            public void Draw(Graphics GR, int xModifier, int YModifier, enStates vState, bool DrawBorderAndBack, string FilterText)
            {


                if (Parent == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Parent nicht definiert"); }
                if (Parent.ItemDesign == enDesign.Undefiniert) { return; }


                var PositionModified = new Rectangle(Pos.X - xModifier, Pos.Y - YModifier, Pos.Width, Pos.Height);




                DrawExplicit(GR, PositionModified, vState, DrawBorderAndBack);


                if (DrawBorderAndBack)
                {
                    if (!string.IsNullOrEmpty(FilterText) && !FilterMatch(FilterText))
                    {
                        var c1 = GenericControl.Skin.Color_Back(Parent.ControlDesign, enStates.Standard); // Standard asl notlösung, um nicnt doppelt checken zu müssen
                        c1 = c1.SetAlpha(160);
                        GR.FillRectangle(new SolidBrush(c1), PositionModified);
                    }
                }

            }


            public int CompareTo(object obj)
            {
                if (!(obj is BasicListItem)) { Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Objecttyp!"); }
                return CompareKey().CompareTo(((BasicListItem)obj).CompareKey());
            }

            public object Clone()
            {
                var x = CloneLVL2();
                x._Checked = _Checked;
                x._Enabled = _Enabled;
                x._Internal = _Internal;
                x._Tags.AddRange(_Tags);
                x._UserDefCompareKey = _UserDefCompareKey;
                return x;
            }


            public bool FilterMatch(string FilterText)
            {
                if (_Internal.ToUpper().Contains(FilterText.ToUpper())) { return true; }
                return FilterMatchLVL2(FilterText);
            }

        }




    }
