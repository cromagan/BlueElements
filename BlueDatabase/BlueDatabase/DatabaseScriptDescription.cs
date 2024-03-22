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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Interfaces;
using BlueScript;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueDatabase;

public static class EventScriptExtension {

    #region Methods

    public static List<DatabaseScriptDescription> Get(this ReadOnlyCollection<DatabaseScriptDescription> scripts, ScriptEventTypes type) {
        var l = new List<DatabaseScriptDescription>();

        foreach (var thisScript in scripts) {
            if (thisScript.EventTypes.HasFlag(type)) { l.Add(thisScript); }
        }

        return l;
    }

    #endregion
}

public sealed class DatabaseScriptDescription : ScriptDescription, IParseable, IReadableTextWithPropertyChangingAndKey, IDisposableExtended, ICloneable, IErrorCheckable, IHasKeyName, IHasDatabase, IPropertyChangedFeedback {

    #region Fields

    private Database? _database;
    private ScriptEventTypes _eventTypes = 0;
    private bool _needRow;

    #endregion

    #region Constructors

    public DatabaseScriptDescription(Database? database, string name, string script) : base(name, script) {
        Database = database;
        _needRow = false;
    }

    public DatabaseScriptDescription(Database? database, string toParse) : this(database) => this.Parse(toParse);

    public DatabaseScriptDescription(Database? database) : this(database, string.Empty, string.Empty) { }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~DatabaseScriptDescription() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Properties

    public Database? Database {
        get => _database;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
            }
        }
    }

    public ScriptEventTypes EventTypes {
        get => _eventTypes;
        set {
            if (IsDisposed) { return; }
            if (_eventTypes == value) { return; }
            _eventTypes = value;
            OnPropertyChanged();
        }
    }

    public bool NeedRow {
        get => _needRow;
        set {
            if (IsDisposed) { return; }
            if (_needRow == value) { return; }
            _needRow = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override List<string> Attributes() {
        var s = new List<string>();
        if (!NeedRow) { s.Add("Rowless"); }
        if (!ChangeValues) { s.Add("NeverChangesValues"); }
        return s;
    }

    public object Clone() => new DatabaseScriptDescription(Database, ToString());

    public override int CompareTo(object obj) {
        if (obj is DatabaseScriptDescription v) {
            return string.Compare(CompareKey, v.CompareKey, StringComparison.Ordinal);
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Falscher Objecttyp!");
        return 0;
    }

    public override string ErrorReason() {
        if (Database is not Database db || db.IsDisposed) { return "Datenbank verworfen"; }

        var b = base.ErrorReason();

        if (!string.IsNullOrEmpty(b)) { return b; }

        if (_eventTypes.HasFlag(ScriptEventTypes.prepare_formula)) {
            if (ChangeValues) { return "Routinen, die das Formular vorbereiten, können keine Werte ändern."; }
            if (!_needRow) { return "Routinen, die das Formular vorbereiten, müssen sich auf Zeilen beziehen."; }
            if (UserGroups.Count > 0) { return "Routinen, die das Formular vorbereiten, können nicht von außerhalb benutzt werden."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.export)) {
            if (ChangeValues) { return "Routinen für Export können keine Werte ändern."; }
            if (!_needRow) { return "Routinen für Export müssen sich auf Zeilen beziehen."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread)) {
            if (ChangeValues) { return "Routinen aus einem ExtraThread, können keine Werte ändern."; }
            if (!_needRow) { return "Routinen aus einem ExtraThread, müssen sich auf Zeilen beziehen."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed)) {
            if (!_needRow) { return "Routinen, die Werteänderungen überwachen, müssen sich auf Zeilen beziehen."; }
            if (!ChangeValues) { return "Routinen, die Werteänderungen überwachen, müssen auch Werte ändern dürfen."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.keyvalue_changed)) {
            if (!_needRow) { return "Routinen, die Werteänderungen überwachen, müssen sich auf Zeilen beziehen."; }
            if (!ChangeValues) { return "Routinen, die Werteänderungen überwachen, müssen auch Werte ändern dürfen."; }
            if (!db.Column.HasKeyColumns()) { return "Routinen wird nie ausgelöst, da keine Schlüsselspalten definiert sind."; }
            if (_eventTypes.HasFlag(ScriptEventTypes.value_changed)) { return "Nach einer Schlüsselwert-Änderung wird sowieso 'Wert geändert' ausgelöst."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.new_row)) {
            if (!_needRow) { return "Routinen, die neue Zeilen überwachen, müssen sich auf Zeilen beziehen."; }
            if (!ChangeValues) { return "Routinen, die neue Zeilen überwachen, müssen auch Werte ändern dürfen."; }

            if (_eventTypes.HasFlag(ScriptEventTypes.keyvalue_changed)) { return "Nach einer neuen Zeile wird sowieso 'Schlüsselwert geändert' ausgelöst."; }
            if (_eventTypes.HasFlag(ScriptEventTypes.value_changed)) { return "Nach einer neuen Zeile wird sowieso 'Wert geändert' ausgelöst."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.loaded)) {
            if (_needRow) { return "Routinen nach dem Laden einer Datenbank, dürfen sich nicht auf Zeilen beziehen."; }
        }

        if (_needRow && !Database.IsRowScriptPossible(false)) { return "Zeilenskripte in dieser Datenbank nicht möglich"; }

        return string.Empty;
    }

    public override bool ParseThis(string key, string value) {
        if (base.ParseThis(key, value)) { return true; }

        switch (key) {
            case "needrow":
                _needRow = value.FromPlusMinus();
                return true;

            case "events":
                _eventTypes = (ScriptEventTypes)IntParse(value);
                return true;

            case "database":
                //Database = Database.GetById(new ConnectionInfo(pair.Value.FromNonCritical(), null), null);
                return true;
        }

        return false;
    }

    public override QuickImage SymbolForReadableText() {
        var i = base.SymbolForReadableText();
        if (i != null) { return i; }

        var symb = ImageCode.Formel;
        var c = Color.Transparent;

        var h = 100;

        if (UserGroups.Count > 0) {
            //c = Color.Yellow;
            symb = ImageCode.Person;
        } else {
            h = 170;
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.export)) { symb = ImageCode.Layout; }
        if (_eventTypes.HasFlag(ScriptEventTypes.loaded)) { symb = ImageCode.Diskette; }
        if (_eventTypes.HasFlag(ScriptEventTypes.new_row)) { symb = ImageCode.Zeile; }
        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed)) { symb = ImageCode.Stift; }
        if (_eventTypes.HasFlag(ScriptEventTypes.keyvalue_changed)) { symb = ImageCode.Schlüssel; }
        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread)) { symb = ImageCode.Wolke; }
        if (_eventTypes.HasFlag(ScriptEventTypes.prepare_formula)) { symb = ImageCode.Textfeld; }

        return QuickImage.Get(symb, 16, c, Color.Transparent, h);
    }

    public override string ToString() {
        try {
            if (IsDisposed) { return string.Empty; }
            List<string> result = [];
            result.ParseableAdd("NeedRow", _needRow);
            result.ParseableAdd("Events", _eventTypes);
            return result.Parseable(base.ToString());
        } catch {
            Develop.CheckStackForOverflow();
            return ToString();
        }
    }

    protected override void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            Database = null;
        }

        base.Dispose(disposing);
    }

    private void _database_Disposing(object sender, System.EventArgs e) => Dispose();

    #endregion
}