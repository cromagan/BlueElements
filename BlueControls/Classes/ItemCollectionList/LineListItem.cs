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

    public override int HeightInControl(ListBoxAppearance style, int columnWidth, Design itemdesign) => 4;

    public override bool IsClickable() => false;

    protected override Size ComputeUntrimmedCanvasSize(Design itemdesign) => CanvasPosition is { X: 0, Y: 0 } and { Width: 0, Height: 0 } ? new Size(4, 4) : CanvasPosition.Size;

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionInControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float scale) => gr.DrawLine(Skin.GetBlueFont(itemdesign, States.Standard).Pen(1f), positionInControl.Left, (int)(positionInControl.Top + (positionInControl.Height / 2.0)), positionInControl.Right, (int)(positionInControl.Top + (positionInControl.Height / 2.0)));

    protected override string GetCompareKey() => CanvasPosition.ToString();

    #endregion
}