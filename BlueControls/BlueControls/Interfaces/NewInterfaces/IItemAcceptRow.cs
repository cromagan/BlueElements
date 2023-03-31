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
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Interfaces;
using System.Collections.Generic;
using System.Security;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das Steuerelement einen Zeilen empfangen kann
/// </summary>
public interface IItemAcceptRow : IItemAcceptSomething, IChangedFeedback, IHasVersion {

    #region Properties

    public string Datenquelle_wählen { get; set; }
    public IItemSendRow? GetRowFrom { get; set; }

    #endregion
}

public class ItemAcceptRow : ItemAcceptSomething {

    #region Fields

    private string? _getValueFromkey;
    private IItemSendRow? _tmpgetValueFrom;

    #endregion

    #region Methods

    public void Datenquelle_wählen(IItemAcceptRow item) {
        if (item.Parent is null) { return; }

        var x = new ItemCollectionList(true);
        foreach (var thisR in item.Parent) {
            if (thisR.IsVisibleOnPage(item.Page) && thisR is IItemSendRow rfp) {
                _ = x.Add(rfp);
            }
        }

        _ = x.Add("<Keine Quelle>");

        var it = InputBoxListBoxStyle.Show("Quelle wählen:", x, AddType.None, true);

        if (it == null || it.Count != 1) { return; }

        var newGetRowFrom = item.Parent[it[0]];

        if (newGetRowFrom is IItemSendRow rfp2) {
            item.GetRowFrom = rfp2;
        } else {
            item.GetRowFrom = null;
        }
        item.RaiseVersion();
        item.OnChanged();
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
    }

    public DatabaseAbstract? InputDatabase(IItemAcceptRow item) => GetRowFromGet(item)?.OutputDatabase;

    public override List<string> ParsableTags() {
        var result = base.ParsableTags();

        result.ParseableAdd("GetValueFromKey", _getValueFromkey ?? string.Empty);

        return result;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "getvaluefrom":
            case "getvaluefromkey":
                _getValueFromkey = value.FromNonCritical();
                return true;
        }
        return false;
    }

    #endregion
}