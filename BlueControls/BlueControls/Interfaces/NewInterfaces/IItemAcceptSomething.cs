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

using BlueBasics.Enums;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueDatabase;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das Steuerelement etwas empfangen kann
/// </summary>
public interface IItemAcceptSomething : IHasKeyName, IChangedFeedback, IHasVersion, IItemToControl {

    #region Properties

    public DatabaseAbstract? DatabaseInput { get; }

    /// <summary>
    /// Welcher Datenbank die eingehenden Filter entsprechen müssen.
    /// Wenn NULL zurück gegeben wird, ist freie Datenbankwahl
    /// </summary>
    public DatabaseAbstract? DatabaseInputMustBe { get; }

    List<int> InputColorId { get; }
    public string Page { get; }

    public ItemCollectionPad.ItemCollectionPad? Parent { get; }

    /// <summary>
    /// Enthält die Schlüssel der Items
    /// </summary>
    public ReadOnlyCollection<string> Parents { get; set; }

    /// <summary>
    /// Wenn DatabaseInputMustBe null zurück gibt, ob schon Filter gewählt werden dürfen.
    /// Typischerweise false, wenn auf die Output-Database gewartet werden soll.
    /// </summary>
    public bool WaitForDatabase { get; }

    #endregion

    #region Methods

    public void CalculateInputColorIds();

    public void UpdateSideOptionMenu();

    #endregion
}

public static class ItemAcceptSomethingExtensions {

    #region Methods

    public static List<int> CalculateColorIds(this IItemAcceptSomething item) {
        var l = new List<int>();

        foreach (var thisId in item.Parents) {
            if (item?.Parent?[thisId] is IItemSendSomething i) {
                l.Add(i.OutputColorId);
            }
        }

        if (l.Count == 0) { l.Add(-1); }

        return l;
    }

    [Description("Wählt ein Filter-Objekt, aus der die Werte kommen.")]
    public static void Datenquellen_bearbeiten(this IItemAcceptSomething item) {
        if (item.Parent is null) { return; }

        var matchDB = item.DatabaseInputMustBe;

        if (item.WaitForDatabase && matchDB == null) {
            return;
        }

        //if (sameDatabase && item.GetFilterFrom.Count > 0) {
        //    if (item.Parent[item.GetFilterFrom[0]] is IItemSendSomething ir) {
        //        matchDB = ir.DatabaseOutput;
        //    }
        //}

        var x = new ItemCollectionList.ItemCollectionList(false);

        // Die Items, die man noch wählen könnte
        foreach (var thisR in item.Parent) {
            if (thisR.IsVisibleOnPage(item.Page) && thisR is IItemSendSomething rfp) {
                if (!item.Parents.Contains(rfp.KeyName) && item != rfp) {
                    if (matchDB == null || matchDB == rfp.DatabaseOutput) {
                        _ = x.Add("Hinzu: " + rfp.ReadableText(), "+|" + rfp.KeyName, rfp.SymbolForReadableText(), true, "1");
                    }
                }
            }
        }

        // Die Items, die entfernt werden können
        if (item.Parents.Count > 0) {
            x.AddSeparator();

            foreach (var thisIt in item.Parents) {
                var name = thisIt;
                var im = QuickImage.Get(ImageCode.Warnung, 16);

                if (item.Parent[name] is IReadableText ir) {
                    name = ir.ReadableText();
                    im = ir.SymbolForReadableText();
                }

                _ = x.Add("Entf.: " + name, "-|" + thisIt, im, true, "3");
            }
        }

        x.AddSeparator();
        _ = x.Add(ContextMenuCommands.Abbruch);

        var it = InputBoxListBoxStyle.Show("Aktion wählen:", x, AddType.None, true);

        if (it == null || it.Count != 1) {
            return;
        }

        var ak = it[0].SplitBy("|");
        if (ak.Length != 2) { return; }

        if (ak[0] == "+") {
            var t = item.Parent[ak[1]];

            if (t is IItemSendSomething rfp2) {
                var l = new List<string>();
                l.AddRange(item.Parents);
                l.Add(rfp2.KeyName);
                l = l.SortedDistinctList();
                item.Parents = l.AsReadOnly();
            }
        }

        if (ak[0] == "-") {
            var l = new List<string>();
            l.AddRange(item.Parents);
            l.Remove(ak[1]);
            l = l.SortedDistinctList();
            item.Parents = l.AsReadOnly();
        }
    }

    #endregion
}

public class ItemAcceptSomething {
    //public void InputColorIdSet(IItemAcceptSomething item, List<int> value) {
    //    if (!_inputColorId.IsDifferentTo(value)) { return; }

    //    _inputColorId = value;
    //    item.OnChanged();
    //}

    #region Fields

    private readonly List<string> _getFilterFromKeys = new();

    private ReadOnlyCollection<IItemSendSomething>? _getFilterFrom;

    private List<int> _inputColorId = new();

    #endregion

    #region Methods

    public void CalculateInputColorIds(IItemAcceptSomething item) {
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }
        var nl = item.CalculateColorIds();

        if (nl.IsDifferentTo(_inputColorId)) {
            _inputColorId = nl;
            item.OnChanged();
        }
    }

    public DatabaseAbstract? DatabaseInput(IItemAcceptSomething item) {
        if (item.DatabaseInputMustBe is DatabaseAbstract db) {
            return db;
        }

        var g = GetFilterFromGet(item);

        if (g.Count == 0) { return null; }
        return g[0].DatabaseOutput;
    }

    public void DoCreativePadAddedToCollection(IItemAcceptSomething item) {
        var l = GetFilterFromGet(item);

        foreach (var thiss in l) {
            thiss.DoChilds();
        }
        item.OnChanged();
    }

    public string ErrorReason(IItemAcceptSomething item) =>
        //if (item.DatabaseInput is not DatabaseAbstract db || db.IsDisposed) {
        //    return "Eingehende Datenbank unbekannt";
        //}
        string.Empty;

    public ReadOnlyCollection<IItemSendSomething> GetFilterFromGet(IItemAcceptSomething item) {
        if (item.Parent == null) {
            Develop.DebugPrint(FehlerArt.Warnung, "Parent nicht initialisiert!");
            return new ReadOnlyCollection<IItemSendSomething>(new List<IItemSendSomething>());
        }

        if (_getFilterFrom == null || _getFilterFrom.Count != _getFilterFromKeys.Count) {
            var l = new List<IItemSendSomething>();

            foreach (var thisk in _getFilterFromKeys) {
                if (item.Parent[thisk] is IItemSendSomething isf) {
                    l.Add(isf);
                }
            }

            _getFilterFrom = new(l);
        }

        return _getFilterFrom;
    }

    public ReadOnlyCollection<string> GetFilterFromKeysGet() => new(_getFilterFromKeys);

    public void GetFilterFromKeysSet(ICollection<string>? value, IItemAcceptSomething item) {
        if (!_getFilterFromKeys.IsDifferentTo(value)) { return; }

        var g = GetFilterFromGet(item);

        // Zuerst die gaanzen Verknüpfungen auflösen.
        foreach (var thisItem in g) {
            thisItem.RemoveChild(item);
        }

        // Dann die Collection leeren
        _getFilterFrom = null;
        _getFilterFromKeys.Clear();

        // Die Collection befüllen
        if (value != null) {
            _getFilterFromKeys.AddRange(value);
        }

        // Und den Eltern bescheid geben, dass ein neues Kind da ist
        g = GetFilterFromGet(item);
        foreach (var thisItem in g) {
            thisItem.AddChild(item);
        }

        item.CalculateInputColorIds();
        item.RaiseVersion();
        item.OnChanged();
        item.UpdateSideOptionMenu();
    }

    public List<int> InputColorIdGet(IItemAcceptSomething item) {
        if (_inputColorId.Count == 0) {
            CalculateInputColorIds(item);
        }
        return _inputColorId;
    }

    public virtual List<string> ParsableTags() {
        List<string> result = new();
        result.ParseableAdd("GetFilterFromKeys", _getFilterFromKeys);
        //result.ParseableAdd("GetValueFromKey", _getValueFromkey ?? string.Empty);

        return result;
    }

    public void ParseFinished(IItemAcceptSomething item) { }

    public bool ParseThis(string tag, string value) {
        switch (tag) {
            case "getvaluefrom":
            case "getvaluefromkey":
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

    internal List<GenericControl> GetStyleOptions(IItemAcceptSomething item, int widthOfControl) {
        var l = new List<GenericControl>();

        l.Add(new FlexiControl("Eingang:", widthOfControl));

        if (item.WaitForDatabase && item.DatabaseInputMustBe == null) {
            l.Add(new FlexiControl("<ImageCode=Information|16> Bevor Filter gewählt werden können muss die Ausgangsdatenbank gewählt werden.", widthOfControl));
        } else {
            l.Add(new FlexiControlForDelegate(item.Datenquellen_bearbeiten, "Eingehende Filter wählen", ImageCode.Trichter));
        }

        return l;
    }

    #endregion
}