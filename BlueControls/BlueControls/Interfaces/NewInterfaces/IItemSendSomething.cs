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
using BlueControls.Forms;
using BlueDatabase;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static BlueDatabase.DatabaseAbstract;

namespace BlueControls.Interfaces;

public interface IItemSendSomething : IChangedFeedback, IReadableTextWithChangingAndKey, IHasVersion, IHasKeyName, IItemToControl {

    #region Properties

    public ReadOnlyCollection<string> ChildIds { get; set; }
    int OutputColorId { get; set; }

    public DatabaseAbstract? OutputDatabase { get; set; }

    public string Page { get; }

    public ItemCollectionPad.ItemCollectionPad? Parent { get; }

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add);

    public void RemoveChild(IItemAcceptSomething remove);

    public void UpdateSideOptionMenu();

    #endregion

    //public void RemoveAllConnections();
}

public static class ItemSendSomethingExtension {

    #region Methods

    public static void Datenbank_wählen(this IItemSendSomething item) {
        var db = CommonDialogs.ChooseKnownDatabase("Ausgangs-Datenbank wählen:");
        if (db == null) { return; }
        item.OutputDatabase = db;

        if (item is IItemAcceptSomething ias) {
            if (ias.InputDatabase != ias.InputDatabaseMustBe && ias.InputDatabaseMustBe != null) {
                if (ias is IItemAcceptFilter iaf) {
                    iaf.GetFilterFrom = new List<string>().AsReadOnly();
                }
                if (ias is IItemAcceptRow iar) {
                    iar.GetRowFrom = null;
                }
            }
        }
    }

    public static void Datenbankkopf(this IItemSendSomething item) {
        if (item.OutputDatabase == null || item.OutputDatabase.IsDisposed) { return; }
        TableView.OpenDatabaseHeadEditor(item.OutputDatabase);
    }

    public static void DoChilds(this IItemSendSomething item) {
        //if (_childIds == null) { return; }

        if (item.Parent == null) { return; }

        if (item.ChildIds != null) {
            foreach (var thisChild in item.ChildIds) {
                var item2 = item.Parent[thisChild];

                if (item2 is IItemAcceptSomething ias) {
                    ias.CalculateInputColorIds();
                }
            }
        }
    }

    #endregion
}

public class ItemSendSomething {

    #region Fields

    private readonly List<string> _childIds = new();

    private int _outputColorId = -1;

    private DatabaseAbstract? _outputDatabase;

    #endregion

    #region Methods

    public void AddChild(IItemSendSomething item, IHasKeyName add) {
        var l = new List<string>();
        if (item.ChildIds != null) { l.AddRange(item.ChildIds); }
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

    public void DoCreativePadAddedToCollection(IItemSendSomething item) {
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }
        if (item.Parent != null) {
            item.OutputColorId = -1;
            item.OutputColorId = item.Parent.GetFreeColorId(item.Page);
        }
        item.DoChilds();
        item.OnChanged();
    }

    public int OutputColorIdGet() => _outputColorId;

    public void OutputColorIdSet(int value, IItemSendSomething item) {
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }
        if (_outputColorId == value) { return; }

        _outputColorId = value;
        item.OnChanged();
    }

    public DatabaseAbstract? OutputDatabaseGet() => _outputDatabase;

    public void OutputDatabaseSet(DatabaseAbstract? value, IItemSendSomething item) {
        if (item is IDisposableExtended ds && ds.IsDisposed) { return; }
        if (value == _outputDatabase) { return; }

        _outputDatabase = value;
        item.RaiseVersion();
        item.DoChilds();
        item.OnChanged();
        item.UpdateSideOptionMenu();
    }

    public virtual List<string> ParsableTags() {
        List<string> result = new();

        result.ParseableAdd("OutputDatabase", _outputDatabase);

        result.ParseableAdd("SentToChildIds", _childIds);

        return result;
    }

    public void ParseFinished(IItemSendSomething item) { }

    public virtual bool ParseThis(string tag, string value) {
        switch (tag) {
            case "database":
            case "outputdatabase":
                var na = value.FromNonCritical();

                if (na.IsFormat(FormatHolder.FilepathAndName)) {
                    na = na.FilePath() + MakeValidTableName(na.FileNameWithoutSuffix()) + "." + na.FileSuffix();
                }

                _outputDatabase = GetById(new ConnectionInfo(na, null), false, string.Empty, null);
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

    public void RemoveChild(IItemAcceptSomething remove, IItemSendSomething item) {
        var l = new List<string>();
        l.AddRange(_childIds);
        _ = l.Remove(remove.KeyName);

        ChildIdsSet(l.AsReadOnly(), item);
        remove.CalculateInputColorIds();
    }

    protected List<GenericControl> GetStyleOptions(IItemSendSomething item, int widthOfControl) {
        var l = new List<GenericControl>();
        l.Add(new FlexiControl("Ausgang:", widthOfControl));

        l.Add(new FlexiControlForDelegate(item.Datenbank_wählen, "Datenbank wählen", ImageCode.Datenbank));

        if (item.OutputDatabase == null || item.OutputDatabase.IsDisposed) { return l; }

        l.Add(new FlexiControlForDelegate(item.Datenbankkopf, "Datenbank: '" + item.OutputDatabase.Caption + "'", ImageCode.Stift));

        return l;
    }

    #endregion
}