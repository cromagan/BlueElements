using BlueBasics;
using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Drawing;

namespace BlueControls.ItemCollection.ItemCollectionList
{
    // LinenKollision
    //http://www.vb-fun.de/cgi-bin/loadframe.pl?ID=vb/tipps/tip0294.shtml

    //'Imports Microsoft.VisualBasic


    public class LineListItem : BasicListItem
        {
            public override void DesignOrStyleChanged()
            {
                // Keine Variablen zum Reseten, ein Invalidate reicht
            }


            #region  Variablen-Deklarationen 

            #endregion


            #region  Event-Deklarationen + Delegaten 

            #endregion


            #region  Construktor + Initialize 

            public LineListItem(string cUserDefCompareKey)
            {
                UserDefCompareKey = cUserDefCompareKey;
            }


            public LineListItem()
            { }


            protected override void InitializeLevel2() { }

            #endregion


            #region  Properties 


            #endregion

            public override string Internal()
            {
                return _Internal;
            }


            public override SizeF SizeUntouchedForListBox()
            {

                if (Pos.X == 0 && Pos.X == 0 && Pos.Width == 0 && Pos.Height == 0)
                {

                    return new SizeF(4, 4);


                }

                //  DebugPrint_NichtImplementiert()
                return Pos.Size;
            }

            protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enStates vState, bool DrawBorderAndBack)
            {

                GR.DrawLine(GenericControl.Skin.GetBlueFont(Parent.ItemDesign, enStates.Standard).Pen(1f), PositionModified.Left, Convert.ToInt32(PositionModified.Top + PositionModified.Height / 2.0), PositionModified.Right, Convert.ToInt32(PositionModified.Top + PositionModified.Height / 2.0));
            }



            public override bool IsClickable()
            {
                return false;
            }


            public override void ComputePositionForListBox(enBlueListBoxAppearance IsIn, float X, float Y, float MultiX, int SliderWidth, int MaxWidth)
            {
                SetCoordinates(new Rectangle(Convert.ToInt32(X), Convert.ToInt32(Y) + 2, Convert.ToInt32(MultiX - SliderWidth), 4));
            }

            public override SizeF QuickAndDirtySize(int PreferedHeigth)
            {
                return SizeUntouchedForListBox();
            }


            protected override string GetCompareKey()
            {
                return Pos.ToString();
            }


            protected override BasicListItem CloneLVL2()
            {
                return new LineListItem();
            }


            protected override bool FilterMatchLVL2(string FilterText)
            {
                Develop.DebugPrint_NichtImplementiert();
                return false;
            }

        }
    }
