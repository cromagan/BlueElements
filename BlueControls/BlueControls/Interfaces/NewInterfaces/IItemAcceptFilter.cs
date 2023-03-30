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
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.Converter;
using static BlueControls.Interfaces.IItemSendSomethingExtensions;
using static BlueControls.Interfaces.IHasVersionExtensions;
using System.Collections.ObjectModel;
using BlueBasics.Interfaces;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das Steuerelement einen Filter empfangen kann
/// </summary>
public interface IItemAcceptFilter : IDisposableExtended, IItemAcceptSomething {

    #region Properties

    public string Datenquelle_hinzufügen { get; set; }
    public ReadOnlyCollection<IItemSendFilter>? GetFilterFrom { get; set; }

    #endregion
}

public static class IItemAcceptFilterExtension {

    #region Methods

    public static void ChangeFilterTo(this IItemAcceptFilter item, List<IItemSendFilter> current, ReadOnlyCollection<IItemSendFilter>? newvalue) {
        if (!current.IsDifferentTo(newvalue)) { return; }

        foreach (var thisItem in current) {
            thisItem.RemoveChild(item);
        }

        current.Clear();
        current.AddRange(newvalue);

        foreach (var thisItem in current) {
            thisItem.AddChild(item);
        }

        item.RaiseVersion();
        item.OnChanged();
    }

    public static DatabaseAbstract? InputDatabase(this IItemAcceptFilter item) {
        if (item.GetFilterFrom == null || item.GetFilterFrom.Count == 0) { return null; }
        return item.GetFilterFrom[0].OutputDatabase;
    }

    #endregion
}