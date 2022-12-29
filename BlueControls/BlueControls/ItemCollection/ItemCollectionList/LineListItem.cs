// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using BlueControls.Enums;
using System.Drawing;

namespace BlueControls.ItemCollection.ItemCollectionList;

// LinenKollision
//http://www.vb-fun.de/cgi-bin/loadframe.pl?ID=vb/tipps/tip0294.shtml
//'Imports Microsoft.VisualBasic
public class LineListItem : BasicListItem {

    #region Constructors

    public LineListItem(string internalname, string userDefCompareKey) : base(internalname, true) => UserDefCompareKey = userDefCompareKey;

    #endregion

    #region Properties

    public override string QuickInfo => string.Empty;

    #endregion

    #region Methods

    public override object Clone() {
        var x = new LineListItem(Internal, UserDefCompareKey);
        x.CloneBasicStatesFrom(this);
        return x;
    }

    public override int HeightForListBox(BlueListBoxAppearance style, int columnWidth) => 4;

    public override bool IsClickable() => false;

    protected override Size ComputeSizeUntouchedForListBox() => Pos.X == 0 && Pos.Y == 0 && Pos.Width == 0 && Pos.Height == 0 ? new Size(4, 4) : Pos.Size;

    protected override void DrawExplicit(Graphics gr, Rectangle positionModified, Design itemdesign, States vState, bool drawBorderAndBack, bool translate) => gr.DrawLine(Skin.GetBlueFont(itemdesign, States.Standard).Pen(1f), positionModified.Left, (int)(positionModified.Top + (positionModified.Height / 2.0)), positionModified.Right, (int)(positionModified.Top + (positionModified.Height / 2.0)));

    protected override string GetCompareKey() => Pos.ToString();

    #endregion
}