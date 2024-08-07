﻿// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BlueControls.ItemCollectionList;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das Steuerelement etwas empfangen kann
/// </summary>
public interface IItemAcceptFilter : IHasKeyName, IPropertyChangedFeedback, IHasVersion, IErrorCheckable {

    #region Properties

    /// <summary>
    /// Bestimmt die Filterberechung und bestimmt, ob der Filter der Parents weiterverwendet werden kann.
    /// 'One' spart immens Rechenleistung
    /// </summary>
    public AllowedInputFilter AllowedInputFilter { get; }

    public Database? DatabaseInput { get; }

    public bool DatabaseInputMustMatchOutputDatabase { get; }

    // ReSharper disable once UnusedMemberInSuper.Global
    List<int> InputColorId { get; }

    /// <summary>
    /// Wenn True, können Variablen einer Zeile benutzt werden.
    /// Das heißt aber auch, dass die eingehenden Filter nur eine Zeile ergeben dürfen.
    /// Wird aktuell nur für die Description des Skript-Editors benutzt
    /// </summary>
    public bool InputMustBeOneRow { get; }

    public string Page { get; }

    public ItemCollectionPad.ItemCollectionPad? Parent { get; }

    /// <summary>
    /// Enthält die Schlüssel der Items
    /// </summary>
    public ReadOnlyCollection<string> Parents { get; set; }

    #endregion

    #region Methods

    public void CalculateInputColorIds();

    public void UpdateSideOptionMenu();

    #endregion
}

public static class ItemAcceptFilterExtensions {

    #region Methods

    public static List<int> CalculateColorIds(this IItemAcceptFilter item) {
        var l = new List<int>();

        foreach (var thisId in item.Parents) {
            if (item?.Parent?[thisId] is IItemSendFilter i) {
                l.Add(i.OutputColorId);
            }
        }

        if (l.Count == 0) { l.Add(-1); }

        return l;
    }

    #endregion
}

public sealed class ItemAcceptFilter {
    //public void InputColorIdSet(IItemAcceptFilter item, List<int> value) {
    //    if (!_inputColorId.IsDifferentTo(value)) { return; }

    //    _inputColorId = value;
    //    item.OnPropertyChanged();
    //}

    #region Fields

    private readonly List<string> _getFilterFromKeys = [];

    private ReadOnlyCollection<IItemSendFilter>? _getFilterFrom;

    private List<int> _inputColorId = [];

    #endregion

    #region Methods

    public void CalculateInputColorIds(IItemAcceptFilter item) {
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }
        var nl = item.CalculateColorIds();

        if (nl.IsDifferentTo(_inputColorId)) {
            _inputColorId = nl;
            //item.on;
        }
    }

    /// <summary>
    /// Holt die Datebank aus dem erst Parent, da das Output
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Database? DatabaseInputGet(IItemAcceptFilter item) {
        //if (item.DatabaseInputMustMatchOutputDatabase) {
        //    return item is IItemSendFilter iiss ? iiss.DatabaseOutput : null;
        //}

        var g = GetFilterFromGet(item);

        return g.Count == 0 ? null : g[0].DatabaseOutput;
    }

    public void DoCreativePadAddedToCollection(IItemAcceptFilter item) {
        var l = GetFilterFromGet(item);

        foreach (var thiss in l) {
            thiss.DoChilds();
        }
        item.OnPropertyChanged();
    }

    public string ErrorReason(IItemAcceptFilter item) =>
        //if (item.DatabaseInputGet is not Database db || db.IsDisposed) {
        //    return "Eingehende Datenbank unbekannt";
        //}
        string.Empty;

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

    public void GetFilterFromKeysSet(IEnumerable<string>? value, IItemAcceptFilter item) {
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
        item.OnPropertyChanged();
        item.UpdateSideOptionMenu();
    }

    public List<GenericControl> GetProperties(IItemAcceptFilter item, int widthOfControl) {
        var l = new List<GenericControl> {
            new FlexiControl("Eingang:", widthOfControl, true)
        };

        if (item.Parent is null) { return l; }

        Database? outp = null;

        if (item is IItemSendFilter iiss) {
            outp = iiss.DatabaseOutput;
        }

        if (item.DatabaseInputMustMatchOutputDatabase && outp == null) {
            l.Add(new FlexiControl("<ImageCode=Information|16> Bevor Filter gewählt werden können muss die Ausgangsdatenbank gewählt werden.", widthOfControl, false));
        } else {
            //l.Add(new FlexiControlForProperty<>(item.Datenquellen_bearbeiten, "Eingehende Filter wählen", ImageCode.Trichter));

            var x = new List<AbstractListItem>();

            // Die Items, die man noch wählen könnte
            foreach (var thisR in item.Parent) {
                if (thisR.IsOnPage(item.Page) && thisR is IItemSendFilter rfp) {
                    //if (outp == null || outp == rfp.DatabaseOutput) {
                    if (rfp != item) {
                        x.Add(ItemOf(rfp.ReadableText(), rfp.KeyName, rfp.SymbolForReadableText(), true, "1"));
                    }

                    //}
                }
            }

            switch (item.AllowedInputFilter) {
                case AllowedInputFilter.One:
                    l.Add(new FlexiControlForProperty<ReadOnlyCollection<string>>(() => item.Parents, string.Empty, 3, x, CheckBehavior.SingleSelection, AddType.None));
                    break;

                case AllowedInputFilter.More:
                    l.Add(new FlexiControlForProperty<ReadOnlyCollection<string>>(() => item.Parents, string.Empty, 3, x, CheckBehavior.MultiSelection, AddType.None));
                    break;

                case AllowedInputFilter.More | AllowedInputFilter.None:
                    l.Add(new FlexiControlForProperty<ReadOnlyCollection<string>>(() => item.Parents, string.Empty, 3, x, CheckBehavior.MultiSelection, AddType.None));
                    break;

                case AllowedInputFilter.One | AllowedInputFilter.None:
                    l.Add(new FlexiControlForProperty<ReadOnlyCollection<string>>(() => item.Parents, string.Empty, 3, x, CheckBehavior.SingleSelection, AddType.None));
                    break;

                default:

                    //case AllowedInputFilter.None:
                    break;
            }
        }

        return l;
    }

    public List<int> InputColorIdGet(IItemAcceptFilter item) {
        if (_inputColorId.Count == 0) {
            CalculateInputColorIds(item);
        }
        return _inputColorId;
    }

    public List<string> ParsableTags() {
        List<string> result = [];
        result.ParseableAdd("GetFilterFromKeys", _getFilterFromKeys, false);
        //result.ParseableAdd("GetValueFromKey", _getValueFromkey ?? string.Empty);

        return result;
    }

    public void ParseFinished(IItemAcceptFilter item) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
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

    #endregion
}