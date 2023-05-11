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

using System.Collections.Generic;
using BlueBasics;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueDatabase;
using System.Collections.ObjectModel;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueBasics.Enums;
using System.ComponentModel;
using BlueDatabase.AdditionalScriptComands;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das Steuerelement einen Filter empfangen kann
/// </summary>
public interface IItemAcceptFilter : IItemAcceptSomething {

    #region Properties

    /// <summary>
    /// Enthält die Schlüssel der Items
    /// </summary>
    public ReadOnlyCollection<string>? GetFilterFrom { get; set; }

    #endregion
}

public static class ItemAcceptFilterExtensions {

    #region Methods

    public static List<int> CalculateColorIds(this IItemAcceptFilter item) {
        var l = new List<int>();

        if (item.GetFilterFrom == null) {
            foreach (var thisID in item.GetFilterFrom) {
                if (item.Parent[thisID] is IItemSendFilter i) {
                    l.Add(i.OutputColorId);
                }
            }
        }

        if (l.Count == 0) { l.Add(-1); }

        return l;
    }

    [Description("Wählt ein Filter-Objekt, aus der die Werte kommen.")]
    public static void Datenquelle_hinzufügen(this IItemAcceptFilter item) {
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
            var l = new List<string>();
            if (item.GetFilterFrom != null) { l.AddRange(item.GetFilterFrom); }
            l.Add(rfp2.KeyName);
            item.GetFilterFrom = l.AsReadOnly();

            //_getFilterFrom = null;
            //_getFilterFromKeys.AddIfNotExists(rfp2.KeyName);
        }
    }

    #endregion
}

public class ItemAcceptFilter : ItemAcceptSomething {

    #region Fields

    public readonly List<string> _getFilterFromKeys = new();

    private ReadOnlyCollection<IItemSendFilter>? _getFilterFrom;

    private List<int> _inputColorId = new();

    #endregion

    #region Methods

    public void CalculateInputColorIds(IItemAcceptFilter item) {
        var nl = item.CalculateColorIds();

        if (nl.IsDifferentTo(_inputColorId)) {
            _inputColorId = nl;
            item.OnChanged();
        }
    }

    public void DoCreativePadAddedToCollection(IItemAcceptFilter item) {
        var l = GetFilterFromGet(item);

        foreach (var thiss in l) {
            thiss.DoChilds();
        }
        item.OnChanged();
    }

    public ReadOnlyCollection<IItemSendFilter> GetFilterFromGet(IItemAcceptFilter item) {
        if (_getFilterFrom == null) {
            var l = new List<IItemSendFilter>();

            foreach (var thisk in _getFilterFromKeys) {
                if (item.Parent[thisk] is IItemSendFilter isf) {
                    l.Add(isf);
                }
            }

            _getFilterFrom = new(l);
        }

        return _getFilterFrom;
    }

    public ReadOnlyCollection<string> GetFilterFromKeysGet() => new(_getFilterFromKeys);

    public void GetFilterFromKeysSet(ICollection<string>? value, IItemAcceptFilter item) {
        {
            if (!_getFilterFromKeys.IsDifferentTo(value)) { return; }

            var g = GetFilterFromGet(item);

            foreach (var thisItem in g) {
                thisItem.RemoveChild(item);
            }

            _getFilterFrom = null;
            _getFilterFromKeys.Clear();

            if (value != null) {
                _getFilterFromKeys.AddRange(value);
            }

            g = GetFilterFromGet(item);
            foreach (var thisItem in g) {
                thisItem.AddChild(item);
            }

            item.RaiseVersion();
            item.OnChanged();
        }
    }

    public List<int> InputColorIdGet(IItemAcceptFilter item) {
        if (_inputColorId.Count == 0) {
            this.CalculateInputColorIds(item);
        }
        return _inputColorId;
    }

    public DatabaseAbstract? InputDatabase(IItemAcceptFilter item) {
        var g = GetFilterFromGet(item);

        if (g == null || g.Count == 0) { return null; }
        return g[0].OutputDatabase;
    }

    public override List<string> ParsableTags() {
        var result = base.ParsableTags();

        result.ParseableAdd("GetFilterFromKeys", _getFilterFromKeys);

        return result;
    }

    public void ParseFinished(IItemAcceptFilter item) { }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "getfilterfromkeys":
                var tmp = value.FromNonCritical().SplitBy("|");
                _getFilterFromKeys.Clear();
                foreach (var thiss in tmp) {
                    _getFilterFromKeys.Add(thiss.FromNonCritical());
                }
                _getFilterFrom = null;
                return true;
        }
        return false;
    }

    internal List<GenericControl> GetStyleOptions(IItemAcceptFilter item) {
        var l = new List<GenericControl>();
        l.AddRange(base.GetStyleOptions(item));

        l.Add(new FlexiControlForDelegate(item.Datenquelle_hinzufügen, "Datenquelle hinzufügen", ImageCode.Trichter));

        return l;
    }

    #endregion
}