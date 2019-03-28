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
using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Drawing;

namespace BlueControls.ItemCollection
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

                GR.DrawLine(GenericControl.Skin.GetBlueFont(Parent.ItemDesign, enStates.Standard).Pen(1f), PositionModified.Left, (int)(PositionModified.Top + PositionModified.Height / 2.0), PositionModified.Right, (int)(PositionModified.Top + PositionModified.Height / 2.0));
            }



            public override bool IsClickable()
            {
                return false;
            }


            public override void ComputePositionForListBox(enBlueListBoxAppearance IsIn, float X, float Y, float MultiX, int SliderWidth, int MaxWidth)
            {
                SetCoordinates(new Rectangle((int)(X), (int)(Y) + 2, (int)(MultiX - SliderWidth), 4));
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
