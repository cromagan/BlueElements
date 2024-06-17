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
using BlueControls.Interfaces;
using static BlueDatabase.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static BlueBasics.Converter;

using BlueDatabase;
using BlueControls.Editoren;
using BlueControls.Enums;
using BlueControls.EventArgs;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using BlueScript;
using BlueScript.Methods;
using BlueDatabase.Enums;
using System.Collections.ObjectModel;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class RowAdderSingle : IParseable, IReadableTextWithKey, IErrorCheckable, IHasKeyName, IReadableText, IEditable, ISimpleEditor {

    #region Fields

    private List<RowAdderSingleRow> _addersinglerow = new();
    private Database? _database;
    private FilterCollection _filterCollection;
    private string tempDatabaseNametoLoad = string.Empty;
    private string tmpFiltercollection = string.Empty;

    #endregion

    #region Constructors

    public RowAdderSingle(RowAdderPadItem parent, string toParse) : this(parent, -1) => this.Parse(toParse);

    //public RowAdderSingle() : base(Generic.GetUniqueKey()) { }
    public RowAdderSingle(RowAdderPadItem parent, int count) : base() {
        KeyName = Generic.GetUniqueKey();
        Parent = parent;

        _filterCollection = new FilterCollection("RowAdderSingle");

        if (count >= 0) {
            Count = count;
        }
    }

    #endregion

    #region Properties

    public ReadOnlyCollection<RowAdderSingleRow> AdderSingleRows => _addersinglerow.AsReadOnly();
    public string CaptionForEditor => "Import Element";
    public int Count { get; private set; } = 0;

    public Database? Database {
        get {
            if (string.IsNullOrEmpty(tempDatabaseNametoLoad)) { return _database; }
            _database = GetById(new ConnectionInfo(tempDatabaseNametoLoad, null, string.Empty), false, null, true);

            tempDatabaseNametoLoad = string.Empty;
            return _database;
        }

        set {
            _database = value;
            _filterCollection = new FilterCollection(_database, "RowAdder");
            tempDatabaseNametoLoad = string.Empty;
        }
    }

    public string Description => "Ein Element, das beschreibt, wie die Daten zusammengetragen werden.";
    public Type? Editor { get; set; }

    public FilterCollection Filter {
        get {
            if (string.IsNullOrEmpty(tmpFiltercollection)) {
                _filterCollection ??= new FilterCollection(_database, "RowAdderSingle");
                _filterCollection.Database = _database;
                _filterCollection.Editor = typeof(FilterCollectionEditor);
                return _filterCollection;
            }

            _ = Database;
            _filterCollection = new FilterCollection("RowAdderSingle");
            _filterCollection.Database = _database;
            _filterCollection.Parse(tmpFiltercollection);

            tmpFiltercollection = string.Empty;

            _filterCollection.Editor = typeof(FilterCollectionEditor);
            return _filterCollection;
        }

        set {
            if (_filterCollection == value) { return; }
            if (_filterCollection != null) { _filterCollection.Dispose(); }

            _filterCollection = value;
            _filterCollection.Editor = typeof(FilterCollectionEditor);
            tmpFiltercollection = string.Empty;
        }
    }

    public string KeyName { get; private set; } = string.Empty;
    public RowAdderPadItem? Parent { get; private set; } = null;
    public string QuickInfo => ReadableText();

    #endregion

    #region Methods

    public string ErrorReason() {
        if (Database is not Database db || db.IsDisposed) { return "Datenbank fehlt."; }

        //if (string.IsNullOrEmpty(_textKey)) { return "TextKey-Id-Generierung fehlt"; }
        //if (!_textKey.Contains("~")) { return "TextKey-ID-Generierung muss mit Variablen definiert werden."; }

        return string.Empty;
    }

    public List<GenericControl> GetProperties(int widthOfControl) {
        var l = new List<GenericControl>();
        //new FlexiControl("Ausgang:", widthOfControl, true),
        l.Add(new FlexiControlForProperty<Database?>(() => Database, ItemSendFilter.AllAvailableTables()));

        if (Database != null && !Database.IsDisposed) {
            l.Add(new FlexiControlForProperty<FilterCollection>(() => Filter));

            //l.Add(new FlexiControlForProperty<string>(() => TextKey, 5));
            //l.Add(new FlexiControlForProperty<string>(() => AdditionalText, 5));

            l.Add(new FlexiControl("Zeilen:", widthOfControl, true));
            l.Add(Childs());
        }

        return l;
    }

    public AbstractListItem? NewChild() {
        if (Parent is not RowAdderPadItem rapi || rapi.IsDisposed) { return null; }

        //if (ToEdit is not FilterCollection fc || fc.IsDisposed) { return null; }

        //if (fc.Database is not Database db || db.IsDisposed) { return null; }

        var l = new RowAdderSingleRow(this);
        //l.Editor = typeof(FilterEditor);
        _addersinglerow.Add(l);
        return ItemOf(l);
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "database":
                tempDatabaseNametoLoad = value.FromNonCritical();

                if (tempDatabaseNametoLoad.IsFormat(FormatHolder.FilepathAndName)) {
                    tempDatabaseNametoLoad = tempDatabaseNametoLoad.FilePath() + MakeValidTableName(tempDatabaseNametoLoad.FileNameWithoutSuffix()) + "." + tempDatabaseNametoLoad.FileSuffix();
                }

                return true;

            case "filter":
                tmpFiltercollection = value.FromNonCritical();
                return true;

            case "rows":
                foreach (var pair2 in value.GetAllTags()) {
                    _addersinglerow.Add(new RowAdderSingleRow(this, pair2.Value.FromNonCritical()));
                }

                return true;

            case "count":
                Count = IntParse(value);
                return true;
        }
        return false;
    }

    public string ReadableText() {
        var b = ErrorReason();
        if (!string.IsNullOrEmpty(b) || Database == null) { return b; }
        return Database.Caption;
    }

    public QuickImage? SymbolForReadableText() => null;

    public override string ToString() {
        List<string> result = [];

        result.ParseableAdd("Database", Database); // Nicht _database, weil sie evtl. noch nicht geladen ist
        result.ParseableAdd("Filter", Filter);
        result.ParseableAdd("Rows", "Item", _addersinglerow);
        result.ParseableAdd("Count", Count);

        return result.Parseable();
    }

    private ListBox Childs() {
        var childs = new ListBox {
            AddAllowed = AddType.UserDef,
            RemoveAllowed = true,
            MoveAllowed = true,
            AutoSort = false,
            ItemEditAllowed = true,
            CheckBehavior = CheckBehavior.AllSelected,
            AddMethod = NewChild,
            Height = 240
        };

        //CFormula?.AddChilds(childs.Suggestions, CFormula.NotAllowedChilds);

        foreach (var thisf in _addersinglerow) {
            //if (File.Exists(thisf)) {
            //    childs.AddAndCheck(new TextListItem(thisf.FileNameWithoutSuffix(), thisf, QuickImage.Get(ImageCode.Diskette, 16), false, true, string.Empty));
            //} else {
            childs.AddAndCheck(ItemOf(thisf));
            //}
        }

        childs.ItemCheckedChanged += Childs_ItemCheckedChanged;
        childs.Disposed += Childs_Disposed;

        return childs;
    }

    private void Childs_Disposed(object sender, System.EventArgs e) {
        if (sender is ListBox childs) {
            childs.ItemCheckedChanged -= Childs_ItemCheckedChanged;
            childs.Disposed -= Childs_Disposed;
        }
    }

    private void Childs_ItemCheckedChanged(object sender, System.EventArgs e) {
        //if (IsDisposed) { return; }
        _addersinglerow.Clear();

        foreach (var item in ((ListBox)sender).CheckedItems()) {
            if (item is ReadableListItem rli && rli.Item is RowAdderSingleRow ras) {
                _addersinglerow.Add(ras);
            }
        }
        //OnPropertyChanged();
        //this.RaiseVersion();
        //UpdateSideOptionMenu();
    }

    #endregion
}