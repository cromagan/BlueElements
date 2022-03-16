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

        public bool ArrowOnMyItem = false;
        public bool ArrowOnOtherItem = false;
        public enConnectionType MyItemType;
        public FixedConnectibleRectangleBitmapPadItem OtherItem;
        public enConnectionType OtherItemType;

        #endregion

        #region Constructors

        public ItemConnection(FixedConnectibleRectangleBitmapPadItem otheritem, enConnectionType otherItemType, enConnectionType myItemType, bool arrowOnMyItem, bool arrowOnOtherItem) {
            OtherItem = otheritem;
            OtherItemType = otherItemType;
            MyItemType = myItemType;
            ArrowOnMyItem = arrowOnMyItem;
            ArrowOnOtherItem = arrowOnOtherItem;
        }

        #endregion

        #region Methods

        public static PointF GetConnectionPoint(FixedConnectibleRectangleBitmapPadItem item, enConnectionType itemc, FixedConnectibleRectangleBitmapPadItem otherItem) {
            switch (itemc) {
                case enConnectionType.Top:
                    return item.UsedArea.PointOf(enAlignment.Top_HorizontalCenter);

                case enConnectionType.Bottom:
                    return item.UsedArea.PointOf(enAlignment.Bottom_HorizontalCenter);

                case enConnectionType.Left:
                    return item.UsedArea.PointOf(enAlignment.VerticalCenter_Left);

                case enConnectionType.Right:
                    return item.UsedArea.PointOf(enAlignment.VerticalCenter_Right);

                default:
                    var m1 = otherItem.UsedArea.PointOf(enAlignment.Horizontal_Vertical_Center);
                    return item.UsedArea.NearestLineMiddle(m1);
            }
        }

        #endregion
    }
}