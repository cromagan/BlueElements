// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

#nullable enable

using BlueControls.Enums;
using System.Drawing;

namespace BlueControls.ItemCollectionList;

// LinenKollision
//http://www.vb-fun.de/cgi-bin/loadframe.pl?ID=vb/tipps/tip0294.shtml
//'Imports Microsoft.VisualBasic
public class LineListItem : AbstractListItem {

    #region Constructors

    public LineListItem(string keyName, string userDefCompareKey) : base(keyName, true) => UserDefCompareKey = userDefCompareKey;

    #endregion

    #region Properties

    public override string QuickInfo => string.Empty;

    #endregion

    #region Methods

    public override int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign) => 4;

    public override bool IsClickable() => false;

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) => Position is { X: 0, Y: 0 } and { Width: 0, Height: 0 } ? new Size(4, 4) : Position.Size;

    protected override void DrawExplicit(Graphics gr, Rectangle positionModified, Design itemdesign, States vState, bool drawBorderAndBack, bool translate) => gr.DrawLine(Skin.GetBlueFont(itemdesign, States.Standard).Pen(1f), positionModified.Left, (int)(positionModified.Top + (positionModified.Height / 2.0)), positionModified.Right, (int)(positionModified.Top + (positionModified.Height / 2.0)));

    protected override string GetCompareKey() => Position.ToString();

    #endregion
}