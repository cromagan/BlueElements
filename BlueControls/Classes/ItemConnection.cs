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

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.ItemCollectionPad.Abstract;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using static BlueBasics.Extensions;

namespace BlueControls.ItemCollection;

public class ItemConnection : IStringable, INotifyPropertyChanged {

    #region Fields

    internal readonly bool ArrowOnItem1;
    internal readonly bool ArrowOnItem2;
    internal readonly AbstractPadItem Item1;
    internal readonly ConnectionType Item1Type;
    internal readonly AbstractPadItem Item2;
    internal readonly ConnectionType Item2Type;

    private bool _beiExportSichtbar;

    #endregion

    #region Constructors

    public ItemConnection(AbstractPadItem item1, ConnectionType item1Type, bool arrowOnItem1, AbstractPadItem item2, ConnectionType item2Type, bool arrowOnItem2, bool showinPrintMode) {
        Item2 = item2;
        Item2Type = item2Type;
        Item1 = item1;
        Item1Type = item1Type;
        ArrowOnItem1 = arrowOnItem1;
        ArrowOnItem2 = arrowOnItem2;
        _beiExportSichtbar = showinPrintMode;
    }

    #endregion

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    [Description("Wird bei einem Export (wie z. B. Drucken) nur angezeigt, wenn das Häkchen gesetzt ist.")]
    public bool Bei_Export_sichtbar {
        get => _beiExportSichtbar;
        set {
            if (_beiExportSichtbar == value) { return; }
            _beiExportSichtbar = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public static PointF GetConnectionPoint(AbstractPadItem item, ConnectionType itemc, AbstractPadItem otherItem) {
        switch (itemc) {
            case ConnectionType.Top:
                return item.CanvasUsedArea.PointOf(Alignment.Top_HorizontalCenter);

            case ConnectionType.Bottom:
                return item.CanvasUsedArea.PointOf(Alignment.Bottom_HorizontalCenter);

            case ConnectionType.Left:
                return item.CanvasUsedArea.PointOf(Alignment.VerticalCenter_Left);

            case ConnectionType.Right:
                return item.CanvasUsedArea.PointOf(Alignment.VerticalCenter_Right);

            default:
                var m1 = otherItem.CanvasUsedArea.PointOf(Alignment.Horizontal_Vertical_Center);
                return item.CanvasUsedArea.NearestLineMiddle(m1);
        }
    }

    public List<string> ParseableItems() {
        List<string> result = [];
        result.ParseableAdd("Item1", Item1.KeyName);
        result.ParseableAdd("Arrow1", ArrowOnItem1);
        result.ParseableAdd("Type1", Item1Type);
        result.ParseableAdd("Item2", Item2.KeyName);
        result.ParseableAdd("Arrow2", ArrowOnItem2);
        result.ParseableAdd("Type2", Item2Type);
        result.ParseableAdd("Print", _beiExportSichtbar);
        return result;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}