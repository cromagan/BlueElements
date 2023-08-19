﻿// Authors:
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
using BlueBasics.Enums;
using System.ComponentModel;
using BlueBasics.Interfaces;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das Steuerelement einen Filter empfangen kann
/// </summary>
public interface IItemAcceptFilter : IItemAcceptSomething {

    #region Properties

    /// <summary>
    /// Enthält die Schlüssel der Items
    /// </summary>
    public ReadOnlyCollection<string> GetFilterFrom { get; set; }

    #endregion
}

public static class ItemAcceptFilterExtensions {

    #region Methods

    public static List<int> CalculateColorIds(this IItemAcceptFilter item) {
        var l = new List<int>();

        if (item.GetFilterFrom != null) {
            foreach (var thisID in item.GetFilterFrom) {
                if (item?.Parent?[thisID] is IItemSendFilter i) {
                    l.Add(i.OutputColorId);
                }
            }
        }

        if (l.Count == 0) { l.Add(-1); }

        return l;
    }

    [Description("Wählt ein Filter-Objekt, aus der die Werte kommen.")]
    public static void Datenquellen_bearbeiten(this IItemAcceptFilter item) {
        if (item.Parent is null) { return; }

        var matchDB = item.InputDatabaseMustBe;

        if (item.WaitForDatabase && matchDB == null) {
            return;
        }

        //if (sameDatabase && item.GetFilterFrom.Count > 0) {
        //    if (item.Parent[item.GetFilterFrom[0]] is IItemSendSomething ir) {
        //        matchDB = ir.OutputDatabase;
        //    }
        //}

        var x = new ItemCollectionList.ItemCollectionList(false);

        // Die Items, die man noch wählen könnte
        foreach (var thisR in item.Parent) {
            if (thisR.IsVisibleOnPage(item.Page) && thisR is IItemSendFilter rfp) {
                if (!item.GetFilterFrom.Contains(rfp.KeyName)) {
                    if (matchDB == null || matchDB == rfp.OutputDatabase) {
                        _ = x.Add("Hinzu: " + rfp.ReadableText(), "+|" + rfp.KeyName, rfp.SymbolForReadableText(), true, "1");
                    }
                }
            }
        }

        // Die Items, die entfernt werden können
        if (item.GetFilterFrom.Count > 0) {
            x.AddSeparator();

            foreach (var thisIt in item.GetFilterFrom) {
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
        _ = x.Add(ContextMenuComands.Abbruch);

        var it = InputBoxListBoxStyle.Show("Aktion wählen:", x, AddType.None, true);

        if (it == null || it.Count != 1) {
            return;
        }

        var ak = it[0].SplitBy("|");
        if (ak.Length != 2) { return; }

        if (ak[0] == "+") {
            var t = item.Parent[ak[1]];

            if (t is IItemSendFilter rfp2) {
                var l = new List<string>();
                l.AddRange(item.GetFilterFrom);
                l.Add(rfp2.KeyName);
                l = l.SortedDistinctList();
                item.GetFilterFrom = l.AsReadOnly();
            }
        }

        if (ak[0] == "-") {
            var l = new List<string>();
            l.AddRange(item.GetFilterFrom);
            l.Remove(ak[1]);
            l = l.SortedDistinctList();
            item.GetFilterFrom = l.AsReadOnly();
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
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }

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

    public string ErrorReason(IItemAcceptFilter item) {
        var d = InputDatabase(item);

        if (d != null && d != item.InputDatabaseMustBe) {
            return "Eingehende Filter fehlen";
        }

        return string.Empty;
    }

    public ReadOnlyCollection<IItemSendFilter> GetFilterFromGet(IItemAcceptFilter item) {
        if (item.Parent == null) {
            Develop.DebugPrint(FehlerArt.Warnung, "Parent nicht initialisiert!");
            return new ReadOnlyCollection<IItemSendFilter>(new List<IItemSendFilter>());
        }

        if (_getFilterFrom == null || _getFilterFrom.Count != _getFilterFromKeys.Count) {
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

    public List<int> InputColorIdGet(IItemAcceptFilter item) {
        if (_inputColorId.Count == 0) {
            CalculateInputColorIds(item);
        }
        return _inputColorId;
    }

    public DatabaseAbstract? InputDatabase(IItemAcceptFilter item) {
        if (item.InputDatabaseMustBe is DatabaseAbstract db) {
            return db;
        }

        var g = GetFilterFromGet(item);

        if (g.Count == 0) { return null; }
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

    internal List<GenericControl> GetStyleOptions(IItemAcceptFilter item, int widthOfControl) {
        var l = new List<GenericControl>();
        l.AddRange(base.GetStyleOptions(item, widthOfControl));

        if (item.WaitForDatabase && item.InputDatabaseMustBe == null) {
            l.Add(new FlexiControl("<ImageCode=Information|16> Bevor Filter gewählt werden können muss die Ausgangsdatenbank gewählt werden.", widthOfControl));
        } else {
            l.Add(new FlexiControlForDelegate(item.Datenquellen_bearbeiten, "Eingehende Filter wählen", ImageCode.Trichter));
        }

        return l;
    }

    #endregion
}