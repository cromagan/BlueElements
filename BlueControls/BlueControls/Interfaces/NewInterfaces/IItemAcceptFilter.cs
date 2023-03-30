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
using static BlueControls.Interfaces.IHasVersionExtensions;
using System.Collections.ObjectModel;
using System.Windows.Forms.VisualStyles;
using BlueBasics.Interfaces;
using BlueControls.ItemCollection;
using BlueControls.ItemCollection.ItemCollectionList;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das Steuerelement einen Filter empfangen kann
/// </summary>
public interface IItemAcceptFilter : IItemAcceptSomething {

    #region Properties

    public string Datenquelle_hinzufügen { get; set; }
    public ReadOnlyCollection<IItemSendFilter>? GetFilterFrom { get; set; }

    #endregion
}

public class ItemAcceptFilter : ItemAcceptSomething {

    #region Fields

    private readonly List<IItemSendFilter> _getFilterFrom = new();

    #endregion

    #region Methods

    public void Datenquelle_hinzufügen(IItemAcceptSomething item) {
        if (item.Parent is null) { return; }

        var x = new ItemCollectionList(true);
        foreach (var thisR in item.Parent) {
            if (thisR.IsVisibleOnPage(item.Page) && thisR is IItemSendFilter rfp) {
                _ = x.Add(rfp);
            }
        }

        _ = x.Add("<Abbruch>");

        var it = InputBoxListBoxStyle.Show("Quelle hinzufügen:", x, AddType.None, true);

        if (it == null || it.Count != 1) { return; }

        var t = item.Parent[it[0]];

        if (t is IItemSendFilter rfp2) {
            _getFilterFrom.AddIfNotExists(rfp2);
        }
    }

    public ReadOnlyCollection<IItemSendFilter>? GetFilterFromGet() => new(_getFilterFrom);

    public void GetFilterFromSet(ICollection<IItemSendFilter>? value, IItemAcceptSomething item) {
        {
            if (!_getFilterFrom.IsDifferentTo(value)) { return; }

            foreach (var thisItem in _getFilterFrom) {
                thisItem.RemoveChild(item);
            }

            _getFilterFrom.Clear();

            if (value != null) {
                _getFilterFrom.AddRange(value);
            }

            foreach (var thisItem in _getFilterFrom) {
                thisItem.AddChild(item);
            }

            item.RaiseVersion();
            item.OnChanged();
        }
    }

    public DatabaseAbstract? InputDatabase() {
        if (_getFilterFrom.Count == 0) { return null; }
        return _getFilterFrom[0].OutputDatabase;
    }

    #endregion
}