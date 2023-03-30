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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms.Design;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace BlueControls.Interfaces;

public interface IItemSendSomething : IDisposableExtended, IItemToControl, IHasKeyName, IHasColorId, IChangedFeedback, IReadableTextWithChangingAndKey, IHasVersion {

    #region Properties

    public ReadOnlyCollection<string>? ChildIds { get; set; }
    public DatabaseAbstract? OutputDatabase { get; set; }
    public string Page { get; }
    public ItemCollectionPad? Parent { get; }

    #endregion

    //public void RemoveAllConnections();
}

public static class IItemSendSomethingExtensions {

    #region Methods

    public static void AddChild(this IItemSendSomething item, IHasKeyName add) {
        var l = new List<string>();

        if (item.ChildIds != null) { l.AddRange(item.ChildIds); }

        l.AddIfNotExists(add.KeyName);

        item.ChildIds = new ReadOnlyCollection<string>(l);
    }

    public static void DoChilds(this IItemSendSomething item) {
        if (item.ChildIds == null) { return; }

        if (item.Parent == null) { return; }

        foreach (var thisChild in item.ChildIds) {
            var item2 = item.Parent[thisChild];

            if (item2 is IItemAcceptSomething ias) {
                ias.SetInputColorId(item.ColorId);
            }
        }
    }

    public static void DoParentChanged(this IItemSendSomething item) {
        if (item.Parent != null) {
            item.ColorId = -1;
            item.ColorId = item.Parent.GetFreeColorId(item.Page);
        }
        DoChilds(item);

        item.OnChanged();
    }

    public static void RemoveChild(this IItemSendSomething item, IHasKeyName remove) {
        var l = new List<string>();

        if (item.ChildIds != null) {
            l.AddRange(item.ChildIds);

            l.Remove(remove.KeyName);
        }
        item.ChildIds = new ReadOnlyCollection<string>(l);
    }

    #endregion

    //public static void RepairConnections(this IItemSendSomething item) {
    //    item.RemoveAllConnections();
    //    var item1 = item.Parent[item.KeyName];
    //    foreach (var thisChild in item.ChildIds) {
    //        var item2 = item.Parent[thisChild];

    //        item1.Parent.Connections.Add(new ItemConnection(item1, ConnectionType.Bottom, false, item2, ConnectionType.Top, true, false));
    //    }
    //}
}