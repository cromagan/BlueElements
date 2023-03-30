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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms.Design;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace BlueControls.Interfaces;

public interface IItemSendSomething : IHasColorId, IChangedFeedback, IReadableTextWithChangingAndKey, IHasVersion {

    #region Properties

    public ReadOnlyCollection<string>? ChildIds { get; set; }
    public DatabaseAbstract? OutputDatabase { get; set; }
    public string Page { get; }
    public ItemCollectionPad? Parent { get; }

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add);

    public void RemoveChild(IHasKeyName remove);

    #endregion

    //public void RemoveAllConnections();
}

public class ItemSendSomething {

    #region Fields

    private readonly List<string> _childIds = new();

    private DatabaseAbstract? _outputDatabase;

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add, IItemSendSomething item) {
        var l = new List<string>();
        l.AddRange(_childIds);
        l.Add(add.KeyName);

        ChildIdsSet(new ReadOnlyCollection<string>(l), item);
    }

    public ReadOnlyCollection<string> ChildIdsGet() => new ReadOnlyCollection<string>(_childIds);

    public void ChildIdsSet(ReadOnlyCollection<string> value, IItemSendSomething item) {
        if (!_childIds.IsDifferentTo(value)) { return; }

        _childIds.Clear();
        _childIds.AddRange(value);
        item.RaiseVersion();
        DoChilds(item);
        item.OnChanged();
    }

    public void Datenbank_wählen(IItemSendSomething item) {
        var db = CommonDialogs.ChooseKnownDatabase();

        if (db == null) { return; }

        OutputDatabaseSet(db, item);
    }

    public void Datenbankkopf() {
        if (_outputDatabase == null || _outputDatabase.IsDisposed) { return; }
        TableView.OpenDatabaseHeadEditor(_outputDatabase);
    }

    public void DoChilds(IItemSendSomething item) {
        //if (_childIds == null) { return; }

        if (item.Parent == null) { return; }

        foreach (var thisChild in _childIds) {
            var item2 = item.Parent[thisChild];

            if (item2 is IItemAcceptSomething ias) {
                ias.InputColorId = item.InputColorId;
            }
        }
    }

    public void DoParentChanged(IItemSendSomething item) {
        if (item.Parent != null) {
            item.InputColorId = -1;
            item.InputColorId = item.Parent.GetFreeColorId(item.Page);
        }
        DoChilds(item);

        item.OnChanged();
    }

    public DatabaseAbstract? OutputDatabaseGet() => _outputDatabase;

    public void OutputDatabaseSet(DatabaseAbstract? value, IItemSendSomething item) {
        if (value == _outputDatabase) { return; }

        _outputDatabase = value;
        item.RaiseVersion();
        DoChilds(item);
        item.OnChanged();
    }

    public void RemoveChild(IHasKeyName remove, IItemSendSomething item) {
        var l = new List<string>();
        l.AddRange(_childIds);
        l.Remove(remove.KeyName);
        ChildIdsSet(new ReadOnlyCollection<string>(l), item);
    }

    #endregion
}