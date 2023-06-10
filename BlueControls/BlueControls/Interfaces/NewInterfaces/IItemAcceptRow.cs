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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das Steuerelement einen Zeilen empfangen kann
/// </summary>
public interface IItemAcceptRow : IItemAcceptSomething, IChangedFeedback, IHasVersion {

    #region Properties

    public IItemSendRow? GetRowFrom { get; set; }

    #endregion
}

public static class ItemAcceptRowExtensions {

    #region Methods

    public static List<int> CalculateColorIds(this IItemAcceptRow item) {
        var l = new List<int>();

        if (item.GetRowFrom is IItemSendRow i) {
            l.Add(i.OutputColorId);
        }

        if (l.Count == 0) { l.Add(-1); }

        return l;
    }

    [Description("Wählt ein Zeilen-Objekt, aus der die Werte kommen.")]
    public static void Datenquelle_wählen(this IItemAcceptRow item) {
        if (item.Parent is null) { return; }

        var x = new ItemCollectionList(false);
        foreach (var thisR in item.Parent) {
            if (thisR.IsVisibleOnPage(item.Page) && thisR is IItemSendRow rfp) {
                _ = x.Add(rfp);
            }
        }

        _ = x.Add("<Keine Quelle>");

        var it = InputBoxListBoxStyle.Show("Quelle wählen:", x, AddType.None, true);

        if (it == null || it.Count != 1) { return; }

        var newGetRowFrom = item.Parent[it[0]];

        if (item.GetRowFrom == newGetRowFrom) { return; }

        if (newGetRowFrom is IItemSendRow rfp2) {
            item.GetRowFrom = rfp2;
        } else {
            item.GetRowFrom = null;
        }
        item.RaiseVersion();
        item.OnChanged();
        item.UpdateSideOptionMenu();
    }

    #endregion
}

public class ItemAcceptRow : ItemAcceptSomething {

    #region Fields

    private string? _getValueFromkey;

    private List<int> _inputColorId = new();

    private IItemSendRow? _tmpgetValueFrom;

    #endregion

    #region Methods

    public void CalculateInputColorIds(IItemAcceptRow item) {
        var nl = item.CalculateColorIds();

        if (nl.IsDifferentTo(_inputColorId)) {
            _inputColorId = nl;
            item.OnChanged();
        }
    }

    public void DoCreativePadAddedToCollection(IItemAcceptRow item) {
        GetRowFromGet(item)?.DoChilds();
        item.OnChanged();
    }

    public string ErrorReason(IItemAcceptRow item) {
        var d = InputDatabase(item);

        if (d == null || d.IsDisposed) {
            return "Eingehende Zeile (Quelle) fehlt";
        }

        return string.Empty;
    }

    public IItemSendRow? GetRowFromGet(IItemAcceptRow item) {
        if (item.Parent == null || _getValueFromkey == null) { return null; }

        _tmpgetValueFrom ??= item.Parent[_getValueFromkey] as IItemSendRow;

        return _tmpgetValueFrom;
    }

    public void GetRowFromSet(IItemSendRow? value, IItemAcceptRow item) {
        var f = GetRowFromGet(item);

        if (f == value) { return; }

        if (f is IItemSendRow old) { old.RemoveChild(item); }
        _tmpgetValueFrom = value;
        _getValueFromkey = value?.KeyName ?? string.Empty;
        if (_tmpgetValueFrom is IItemSendRow ne) { ne.AddChild(item); }

        item.RaiseVersion();
        item.OnChanged();
        item.UpdateSideOptionMenu();
    }

    public List<int> InputColorIdGet(IItemAcceptRow item) {
        if (_inputColorId.Count == 0) {
            CalculateInputColorIds(item);
        }
        return _inputColorId;
    }

    public DatabaseAbstract? InputDatabase(IItemAcceptRow item) => GetRowFromGet(item)?.OutputDatabase;

    public override List<string> ParsableTags() {
        var result = base.ParsableTags();

        result.ParseableAdd("GetValueFromKey", _getValueFromkey ?? string.Empty);

        return result;
    }

    public void ParseFinished(IItemAcceptRow item) { }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "getvaluefrom":
            case "getvaluefromkey":
                _getValueFromkey = value.FromNonCritical();

                //var l = GetRowFromGet(this);

                return true;
        }
        return false;
    }

    internal List<GenericControl> GetStyleOptions(IItemAcceptRow item) {
        var l = new List<GenericControl>();
        l.AddRange(base.GetStyleOptions(item));

        l.Add(new FlexiControlForDelegate(item.Datenquelle_wählen, "Datenquelle wählen", ImageCode.Zeile));
        //l.Add(new FlexiControl());

        return l;
    }

    #endregion
}