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

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using static BlueBasics.Extensions;

namespace BlueControls.ItemCollection;

public class ItemConnection : IStringable, IChangedFeedback {

    #region Fields

    internal readonly bool ArrowOnMyItem;
    internal readonly bool ArrowOnOtherItem;
    internal readonly ConnectionType MyItemType;
    internal readonly BasicPadItem OtherItem;
    internal readonly ConnectionType OtherItemType;

    private bool _beiExportSichtbar;

    #endregion

    #region Constructors

    public ItemConnection(ConnectionType myItemType, bool arrowOnMyItem, BasicPadItem otheritem, ConnectionType otherItemType, bool arrowOnOtherItem, bool showinPrintMode) {
        OtherItem = otheritem;
        OtherItemType = otherItemType;
        MyItemType = myItemType;
        ArrowOnMyItem = arrowOnMyItem;
        ArrowOnOtherItem = arrowOnOtherItem;
        _beiExportSichtbar = showinPrintMode;
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    [Description("Wird bei einem Export (wie z. B. Drucken) nur angezeigt, wenn das Häkchen gesetzt ist.")]
    public bool Bei_Export_sichtbar {
        get => _beiExportSichtbar;
        set {
            if (_beiExportSichtbar == value) { return; }
            _beiExportSichtbar = value;
            OnChanged();
        }
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

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    internal string ToString(BasicPadItem myItem) {
        List<string> result = new();
        result.ParseableAdd("Item1", myItem.KeyName);
        result.ParseableAdd("Arrow1", ArrowOnMyItem);
        result.ParseableAdd("Type1", MyItemType);
        result.ParseableAdd("Item2", OtherItem.KeyName);
        result.ParseableAdd("Arrow2", ArrowOnOtherItem);
        result.ParseableAdd("Type2", OtherItemType);
        result.ParseableAdd("Print", _beiExportSichtbar);
        return result.Parseable();
    }

    #endregion
}