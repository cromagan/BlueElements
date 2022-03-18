// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueBasics.Enums;
using BlueControls.Enums;
using System.Drawing;
using static BlueBasics.Extensions;

namespace BlueControls.ItemCollection {

    public class ItemConnection {

        #region Fields

        internal readonly bool ArrowOnMyItem = false;
        internal readonly bool ArrowOnOtherItem = false;
        internal readonly ConnectionType MyItemType;
        internal readonly BasicPadItem OtherItem;
        internal readonly ConnectionType OtherItemType;

        #endregion

        #region Constructors

        public ItemConnection(BasicPadItem otheritem, ConnectionType otherItemType, ConnectionType myItemType, bool arrowOnMyItem, bool arrowOnOtherItem) {
            OtherItem = otheritem;
            OtherItemType = otherItemType;
            MyItemType = myItemType;
            ArrowOnMyItem = arrowOnMyItem;
            ArrowOnOtherItem = arrowOnOtherItem;
        }

        #endregion

        #region Methods

        public static PointF GetConnectionPoint(BasicPadItem item, ConnectionType itemc, BasicPadItem otherItem) {
            switch (itemc) {
                case ConnectionType.Top:
                    return item.UsedArea.PointOf(Alignment.Top_HorizontalCenter);

                case ConnectionType.Bottom:
                    return item.UsedArea.PointOf(Alignment.Bottom_HorizontalCenter);

                case ConnectionType.Left:
                    return item.UsedArea.PointOf(Alignment.VerticalCenter_Left);

                case ConnectionType.Right:
                    return item.UsedArea.PointOf(Alignment.VerticalCenter_Right);

                default:
                    var m1 = otherItem.UsedArea.PointOf(Alignment.Horizontal_Vertical_Center);
                    return item.UsedArea.NearestLineMiddle(m1);
            }
        }

        #endregion
    }
}