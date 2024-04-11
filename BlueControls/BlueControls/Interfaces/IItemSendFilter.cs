// Authors:
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
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueDatabase;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static BlueDatabase.Database;
using BlueControls.ItemCollectionList;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Interfaces;

public interface IItemSendFilter : IPropertyChangedFeedback, IReadableTextWithPropertyChangingAndKey, IHasVersion, IHasKeyName, IItemToControl, IErrorCheckable {

    #region Properties

    public ReadOnlyCollection<string> ChildIds { get; set; }
    public Database? DatabaseOutput { get; set; }
    int OutputColorId { get; set; }
    public string Page { get; }

    public ItemCollectionPad.ItemCollectionPad? Parent { get; }

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add);

    public void RemoveChild(IItemAcceptFilter remove);

    public void UpdateSideOptionMenu();

    #endregion

    //public void RemoveAllConnections();
}

public static class ItemSendSomethingExtension {

    #region Methods

    public static void DoChilds(this IItemSendFilter item) {
        //if (_childIds == null) { return; }

        if (item.Parent == null) { return; }

        foreach (var thisChild in item.ChildIds) {
            var item2 = item.Parent[thisChild];

            if (item2 is IItemAcceptFilter ias) {
                ias.CalculateInputColorIds();
            }
        }
    }

    #endregion
}

public sealed class ItemSendSomething {

    #region Fields

    private readonly List<string> _childIds = [];

    private Database? _databaseOutput;
    private int _outputColorId = -1;

    private string tempDatabaseNametoLoad = string.Empty;

    #endregion

    #region Methods

    public void AddChild(IItemSendFilter item, IHasKeyName add) {
        var l = new List<string>();
        l.AddRange(item.ChildIds);
        l.Add(add.KeyName);
        l = l.SortedDistinctList();

        ChildIdsSet(l.AsReadOnly(), item);
    }

    public ReadOnlyCollection<string> ChildIdsGet() => new(_childIds);

    public void ChildIdsSet(ReadOnlyCollection<string> value, IItemSendFilter item) {
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }
        if (!_childIds.IsDifferentTo(value)) { return; }

        _childIds.Clear();
        _childIds.AddRange(value);
        item.RaiseVersion();
        item.DoChilds();
        item.OnPropertyChanged();
    }

    public Database? DatabaseOutputGet() {
        if (string.IsNullOrEmpty(tempDatabaseNametoLoad)) { return _databaseOutput; }
        _databaseOutput = GetById(new ConnectionInfo(tempDatabaseNametoLoad, null, string.Empty), false, null, true);

        tempDatabaseNametoLoad = string.Empty;
        return _databaseOutput;
    }

    public void DatabaseOutputSet(Database? value, IItemSendFilter item) {
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }
        if (value == _databaseOutput) { return; }

        _databaseOutput = value;
        item.RaiseVersion();
        item.DoChilds();
        item.OnPropertyChanged();
        item.UpdateSideOptionMenu();
    }

    public void DoCreativePadAddedToCollection(IItemSendFilter item) {
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }
        if (item.Parent != null) {
            item.OutputColorId = -1;
            item.OutputColorId = item.Parent.GetFreeColorId(item.Page);
        }
        item.DoChilds();
        item.OnPropertyChanged();
    }

    public List<GenericControl> GetStyleOptions(IItemSendFilter item, int widthOfControl) {
        var l = new List<GenericControl> {
            new FlexiControl("Ausgang:", widthOfControl, true)
        };

        var ld = AllAvailableTables(string.Empty);

        var ld2 = new List<AbstractListItem>();

        foreach (var thisd in ld) {
            ld2.Add(ItemOf(thisd));
        }

        l.Add(new FlexiControlForProperty<Database?>(() => item.DatabaseOutput, ld2));

        return l;
    }

    public int OutputColorIdGet() => _outputColorId;

    public void OutputColorIdSet(int value, IItemSendFilter item) {
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }
        if (_outputColorId == value) { return; }

        _outputColorId = value;
        item.OnPropertyChanged();
    }

    public List<string> ParsableTags() {
        List<string> result = [];

        result.ParseableAdd("OutputDatabase", _databaseOutput);

        result.ParseableAdd("SentToChildIds", _childIds, false);

        return result;
    }

    public void ParseFinished(IItemSendFilter item) { }

    public bool ParseThis(string tag, string value) {
        switch (tag) {
            case "database":
            case "outputdatabase":
                tempDatabaseNametoLoad = value.FromNonCritical();

                if (tempDatabaseNametoLoad.IsFormat(FormatHolder.FilepathAndName)) {
                    tempDatabaseNametoLoad = tempDatabaseNametoLoad.FilePath() + MakeValidTableName(tempDatabaseNametoLoad.FileNameWithoutSuffix()) + "." + tempDatabaseNametoLoad.FileSuffix();
                }

                return true;

            case "senttochildids":
                value = value.Replace("\r", "|");

                var tmp = value.FromNonCritical().SplitBy("|");
                _childIds.Clear();
                foreach (var thiss in tmp) {
                    _childIds.Add(thiss.FromNonCritical());
                }
                return true;
        }
        return false;
    }

    public void RemoveChild(IItemAcceptFilter remove, IItemSendFilter item) {
        var l = new List<string>();
        l.AddRange(_childIds);
        _ = l.Remove(remove.KeyName);

        ChildIdsSet(l.AsReadOnly(), item);
        remove.CalculateInputColorIds();
    }

    internal string ErrorReason(IItemSendFilter item) {
        var d = item.DatabaseOutput;
        if (d == null || d.IsDisposed) {
            return "Ausgehende Datenbank nicht angegeben.";
        }

        return string.Empty;
    }

    #endregion
}