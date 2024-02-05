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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueDatabase;
using static BlueDatabase.Database;

namespace BlueControls.Interfaces;

public interface IItemSendSomething : IChangedFeedback, IReadableTextWithChangingAndKey, IHasVersion, IHasKeyName, IItemToControl {

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

    public static void Datenbank_wählen(this IItemSendSomething item) {
        var db = CommonDialogs.ChooseKnownDatabase("Ausgangs-Datenbank wählen: ", string.Empty);
        if (db == null) { return; }
        if (db == item.DatabaseOutput) { return; }

        item.DatabaseOutput = db;

        if (item is IItemAcceptFilter ias && ias.DatabaseInputMustMatchOutputDatabase) {
            ias.Parents = new List<string>().AsReadOnly();
        }
    }

    public static void Datenbankkopf(this IItemSendSomething item) {
        if (item.DatabaseOutput is not Database db || db.IsDisposed) { return; }
        TableView.OpenDatabaseHeadEditor(db);
    }

    public static void DoChilds(this IItemSendSomething item) {
        //if (_childIds == null) { return; }

        if (item.Parent == null) { return; }

        if (item.ChildIds != null) {
            foreach (var thisChild in item.ChildIds) {
                var item2 = item.Parent[thisChild];

                if (item2 is IItemAcceptFilter ias) {
                    ias.CalculateInputColorIds();
                }
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

    #endregion

    #region Methods

    public void AddChild(IItemSendSomething item, IHasKeyName add) {
        var l = new List<string>();
        l.AddRange(item.ChildIds);
        l.Add(add.KeyName);
        l = l.SortedDistinctList();

        ChildIdsSet(l.AsReadOnly(), item);
    }

    public ReadOnlyCollection<string> ChildIdsGet() => new(_childIds);

    public void ChildIdsSet(ReadOnlyCollection<string> value, IItemSendSomething item) {
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }
        if (!_childIds.IsDifferentTo(value)) { return; }

        _childIds.Clear();
        _childIds.AddRange(value);
        item.RaiseVersion();
        item.DoChilds();
        item.OnChanged();
    }

    public Database? DatabaseOutputGet() => _databaseOutput;

    public void DatabaseOutputSet(Database? value, IItemSendSomething item) {
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }
        if (value == _databaseOutput) { return; }

        _databaseOutput = value;
        item.RaiseVersion();
        item.DoChilds();
        item.OnChanged();
        item.UpdateSideOptionMenu();
    }

    public void DoCreativePadAddedToCollection(IItemSendSomething item) {
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }
        if (item.Parent != null) {
            item.OutputColorId = -1;
            item.OutputColorId = item.Parent.GetFreeColorId(item.Page);
        }
        item.DoChilds();
        item.OnChanged();
    }

    public List<GenericControl> GetStyleOptions(IItemSendSomething item, int widthOfControl) {
        var l = new List<GenericControl> {
            new FlexiControl("Ausgang:", widthOfControl)
        };

        if (item.DatabaseOutput is not Database db || db.IsDisposed) {
            l.Add(new FlexiControlForDelegate(item.Datenbank_wählen, "Datenbank wählen", ImageCode.Datenbank));
            return l;
        }

        l.Add(new FlexiControlForDelegate(item.Datenbank_wählen, "Datenbank ändern (gewählt: " + db.Caption + ")", ImageCode.Datenbank));

        l.Add(new FlexiControlForDelegate(item.Datenbankkopf, "Datenbank-Kopf von '" + db.Caption + "'", ImageCode.Stift));

        return l;
    }

    public int OutputColorIdGet() => _outputColorId;

    public void OutputColorIdSet(int value, IItemSendSomething item) {
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }
        if (_outputColorId == value) { return; }

        _outputColorId = value;
        item.OnChanged();
    }

    public List<string> ParsableTags() {
        List<string> result = [];

        result.ParseableAdd("OutputDatabase", _databaseOutput);

        result.ParseableAdd("SentToChildIds", _childIds);

        return result;
    }

    public void ParseFinished(IItemSendSomething item) { }

    public bool ParseThis(string tag, string value) {
        switch (tag) {
            case "database":
            case "outputdatabase":
                var na = value.FromNonCritical();

                if (na.IsFormat(FormatHolder.FilepathAndName)) {
                    na = na.FilePath() + MakeValidTableName(na.FileNameWithoutSuffix()) + "." + na.FileSuffix();
                }

                _databaseOutput = GetById(new ConnectionInfo(na, null, string.Empty), false, null, true);
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

    public void RemoveChild(IItemAcceptFilter remove, IItemSendSomething item) {
        var l = new List<string>();
        l.AddRange(_childIds);
        _ = l.Remove(remove.KeyName);

        ChildIdsSet(l.AsReadOnly(), item);
        remove.CalculateInputColorIds();
    }

    internal string ErrorReason(IItemSendSomething item) {
        var d = item.DatabaseOutput;
        if (d == null || d.IsDisposed) {
            return "Ausgehende Datenbank nicht angegeben.";
        }

        return string.Empty;
    }

    #endregion
}