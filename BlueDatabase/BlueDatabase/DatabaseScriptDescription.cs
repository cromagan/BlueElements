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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Interfaces;
using BlueScript;
using BlueScript.Enums;
using static BlueBasics.Converter;

namespace BlueDatabase;

public static class EventScriptExtension {

    #region Methods

    public static List<DatabaseScriptDescription> Get(this ReadOnlyCollection<DatabaseScriptDescription> scripts, ScriptEventTypes type) {
        try {
            var l = new List<DatabaseScriptDescription>();

            foreach (var thisScript in scripts) {
                if (thisScript.EventTypes.HasFlag(type)) { l.Add(thisScript); }
            }

            return l;
        } catch {
            Develop.CheckStackForOverflow();
            return scripts.Get(type);
        }
    }

    #endregion
}

public sealed class DatabaseScriptDescription : ScriptDescription, ICloneable, IHasDatabase {

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

    public bool AddSysCorrect => _eventTypes.HasFlag(ScriptEventTypes.correct_changed);

    public bool AllVariabelsReadOnly {
        get {
            if (!ChangeValues) { return true; }
            if (_eventTypes.HasFlag(ScriptEventTypes.correct_changed)) { return true; }
            return false;
        }
    }

    public bool ChangeValues {
        get {
            if (_eventTypes.HasFlag(ScriptEventTypes.prepare_formula)) { return false; }
            if (_eventTypes.HasFlag(ScriptEventTypes.export)) { return false; }
            if (_eventTypes.HasFlag(ScriptEventTypes.row_deleting)) { return false; }
            if (_eventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread)) { return false; }
            return true;
        }
    }

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

    /// <summary>
    /// Virtuelle Spalten werden bei FormularVorbereiten benötigt.
    /// </summary>
    public bool VirtalColumns => _eventTypes.HasFlag(ScriptEventTypes.prepare_formula);

    #endregion

    #region Methods

    public override List<string> Attributes() {
        var s = new List<string>();
        if (!NeedRow) { s.Add("Rowless"); }
        //if (!ChangeValues) { s.Add("NeverChangesValues"); }
        return s;
    }

    public object Clone() => new DatabaseScriptDescription(Database, ParseableItems().FinishParseable());

    public override int CompareTo(object obj) {
        if (obj is DatabaseScriptDescription v) {
            return string.Compare(CompareKey, v.CompareKey, StringComparison.Ordinal);
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Falscher Objecttyp!");
        return 0;
    }

    public override string ErrorReason() {
        if (Database is not { IsDisposed: false }) { return "Datenbank verworfen"; }

        if (_eventTypes.HasFlag(ScriptEventTypes.prepare_formula)) {
            //if (ChangeValues) { return "Routinen, die das Formular vorbereiten, können keine Werte ändern."; }
            if (!_needRow) { return "Routinen, die das Formular vorbereiten, müssen sich auf Zeilen beziehen."; }
            if (UserGroups.Count > 0) { return "Routinen, die das Formular vorbereiten, können nicht von außerhalb benutzt werden."; }
            if (_eventTypes != ScriptEventTypes.prepare_formula) { return "Routinen für den Export müssen für sich alleine stehen."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.export)) {
            //if (ChangeValues) { return "Routinen für Export können keine Werte ändern."; }
            if (!_needRow) { return "Routinen für Export müssen sich auf Zeilen beziehen."; }
            if (UserGroups.Count > 0) { return "Routinen, die den Export vorbereiten, können nicht von außerhalb benutzt werden."; }
            if (_eventTypes != ScriptEventTypes.export) { return "Routinen für den Export müssen für sich alleine stehen."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.row_deleting)) {
            //if (ChangeValues) { return "Routinen für das Löschen einer Zeile können keine Werte ändern."; }
            if (!_needRow) { return "Routinen für das Löschen einer Zeile müssen sich auf Zeilen beziehen."; }
            if (UserGroups.Count > 0) { return "Routinen, für das Löschen einer Zeile, können nicht von außerhalb benutzt werden."; }
            if (_eventTypes != ScriptEventTypes.row_deleting) { return "Routinen für für das Löschen einer Zeile müssen für sich alleine stehen."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.correct_changed)) {
            //if (ChangeValues) { return "Routinen für das Löschen einer Zeile können keine Werte ändern."; }
            if (!_needRow) { return "Routinen, die den Fehlerfrei-Status überwachen, einer Zeile müssen sich auf Zeilen beziehen."; }
            //if (UserGroups.Count > 0) { return "Routinen, die den Fehlerfrei-Status überwachen, können nicht von außerhalb benutzt werden."; }
            if (_eventTypes != ScriptEventTypes.correct_changed) { return "Routinen, die den Fehlerfrei-Status überwachen, müssen für sich alleine stehen."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread)) {
            //if (ChangeValues) { return "Routinen aus einem ExtraThread, können keine Werte ändern."; }
            if (!_needRow) { return "Routinen aus einem ExtraThread, müssen sich auf Zeilen beziehen."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed)) {
            if (!_needRow) { return "Routinen, die Werteänderungen überwachen, müssen sich auf Zeilen beziehen."; }
            //if (!ChangeValues) { return "Routinen, die Werteänderungen überwachen, müssen auch Werte ändern dürfen."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.InitialValues)) {
            if (!_needRow) { return "Routinen, die neue Zeilen überwachen, müssen sich auf Zeilen beziehen."; }
            //if (!ChangeValues) { return "Routinen, die neue Zeilen überwachen, müssen auch Werte ändern dürfen."; }
            if (UserGroups.Count > 0) { return "Routinen, die die Zeilen initialsieren, können nicht von außerhalb benutzt werden."; }
            if (_eventTypes != ScriptEventTypes.InitialValues) { return "Routinen zum Initialisieren müssen für sich alleine stehen."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.loaded)) {
            if (_needRow) { return "Routinen nach dem Laden einer Datenbank, dürfen sich nicht auf Zeilen beziehen."; }
        }

        if (_needRow && !Database.IsRowScriptPossible(false)) { return "Zeilenskripte in dieser Datenbank nicht möglich"; }

        if (_eventTypes.ToString() == ((int)_eventTypes).ToString()) { return "Skripte öffnen und neu speichern."; }

        return base.ErrorReason();
    }

    public override List<string> ParseableItems() {
        try {
            if (IsDisposed) { return []; }
            List<string> result = [.. base.ParseableItems()];
            result.ParseableAdd("NeedRow", _needRow);
            result.ParseableAdd("Events", _eventTypes);
            return result;
        } catch {
            Develop.CheckStackForOverflow();
            return ParseableItems();
        }
    }

    public override bool ParseThis(string key, string value) {
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

        return base.ParseThis(key, value);
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
        if (_eventTypes.HasFlag(ScriptEventTypes.InitialValues)) { symb = ImageCode.Zeile; }
        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed)) { symb = ImageCode.Stift; }
        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread)) { symb = ImageCode.Wolke; }
        if (_eventTypes.HasFlag(ScriptEventTypes.prepare_formula)) { symb = ImageCode.Textfeld; }

        return QuickImage.Get(symb, 16, c, Color.Transparent, h);
    }

    internal MethodType AllowedMethods(RowItem? row, bool extended) {

        #region  Erlaubte Methoden ermitteln

        var allowedMethods = MethodType.Standard | MethodType.Database | MethodType.SpecialVariables | MethodType.Math | MethodType.DrawOnBitmap;

        if (row is { IsDisposed: false }) { allowedMethods |= MethodType.MyDatabaseRow; }

        if (_eventTypes is ScriptEventTypes.Ohne_Auslöser or ScriptEventTypes.correct_changed || extended) {
            allowedMethods |= MethodType.ManipulatesUser;
        }

        #endregion

        return allowedMethods;
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