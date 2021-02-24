#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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

using BlueControls.Enums;
using System.Drawing;

namespace BlueControls.ItemCollection
{
    // LinenKollision
    //http://www.vb-fun.de/cgi-bin/loadframe.pl?ID=vb/tipps/tip0294.shtml

    //'Imports Microsoft.VisualBasic


    public class LineListItem : BasicListItem
    {


        #region  Variablen-Deklarationen 

        #endregion


        #region  Event-Deklarationen + Delegaten 

        #endregion


        #region  Construktor + Initialize 

        public LineListItem(string internalname, string userDefCompareKey) : base(internalname)
        {
            UserDefCompareKey = userDefCompareKey;
        }




        #endregion


        #region  Properties 

        public override string QuickInfo
        {
            get
            {
                return string.Empty;
            }
        }
        #endregion




        public override Size SizeUntouchedForListBox()
        {
            if (Pos.X == 0 && Pos.X == 0 && Pos.Width == 0 && Pos.Height == 0)
            {

                return new Size(4, 4);
            }

            return Pos.Size;
        }

        protected override void DrawExplicit(Graphics GR, Rectangle PositionModified, enDesign itemdesign, enStates vState, bool DrawBorderAndBack, bool Translate)
        {
            GR.DrawLine(Skin.GetBlueFont(itemdesign, enStates.Standard).Pen(1f), PositionModified.Left, (int)(PositionModified.Top + PositionModified.Height / 2.0), PositionModified.Right, (int)(PositionModified.Top + PositionModified.Height / 2.0));
        }



        public override bool IsClickable()
        {
            return false;
        }


        public override int HeightForListBox(enBlueListBoxAppearance style, int columnWidth)
        {
            return 4;
        }

        protected override string GetCompareKey()
        {
            return Pos.ToString();
        }

        public override void CloneToNewCollection(ItemCollectionList newParent)
        {
            CloneToNewCollection(newParent, new LineListItem(Internal, UserDefCompareKey));
        }
    }
}
