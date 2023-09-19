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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Interfaces;
using static BlueBasics.Converter;

namespace BlueDatabase;

public static class EventScriptExtension {

    #region Methods

    public static List<DatabaseScript> Get(this ReadOnlyCollection<DatabaseScript> scripts, ScriptEventTypes type) {
        var l = new List<DatabaseScript>();

        foreach (var thisScript in scripts) {
            if (thisScript.EventTypes.HasFlag(type)) { l.Add(thisScript); }
        }

        return l;
    }

    #endregion
}

public sealed class DatabaseScript : IParseable, IReadableTextWithChangingAndKey, IDisposableExtended, ICloneable, IErrorCheckable, IHasKeyName, IHasDatabase, IComparable, IChangedFeedback {

    #region Fields

    private string _admininfo;
    private bool _changeValues;
    private ScriptEventTypes _eventTypes = 0;
    private string _imagecode;
    private bool _manualexecutable;
    private bool _needRow;
    private string _quickinfo;
    private string _scripttext;
    private List<string> _usergroups;

    #endregion

    #region Constructors

    public DatabaseScript(DatabaseAbstract database, string name, string script) : this(database) {
        KeyName = name;
        _scripttext = script;
    }

    public DatabaseScript(DatabaseAbstract? database, string toParse) : this(database) => Parse(toParse);

    public DatabaseScript(DatabaseAbstract? database) {
        Database = database;

        if (Database != null && !Database.IsDisposed) {
            Database.Disposing += Database_Disposing;
        }

        KeyName = string.Empty;
        _scripttext = string.Empty;
        _manualexecutable = false;
        _needRow = false;
    }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~DatabaseScript() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    public string AdminInfo {
        get => _admininfo;
        set {
            if (IsDisposed) { return; }
            if (_admininfo == value) { return; }
            _admininfo = value;
            OnChanged();
        }
    }

    public bool ChangeValues {
        get => _changeValues;
        set {
            if (IsDisposed) { return; }
            if (_changeValues == value) { return; }
            _changeValues = value;
            OnChanged();
        }
    }

    public string CompareKey => Name.ToString();
    public DatabaseAbstract? Database { get; private set; }

    public ScriptEventTypes EventTypes {
        get => _eventTypes;
        set {
            if (IsDisposed) { return; }
            if (_eventTypes == value) { return; }
            _eventTypes = value;
            OnChanged();
        }
    }

    public string Image {
        get => _imagecode;
        set {
            if (IsDisposed) { return; }
            if (_imagecode == value) { return; }
            _imagecode = value;
            OnChanged();
        }
    }

    public bool IsDisposed { get; private set; }
    public string KeyName { get; private set; }

    public bool ManualExecutable {
        get => _manualexecutable;
        set {
            if (IsDisposed) { return; }
            if (_manualexecutable == value) { return; }
            _manualexecutable = value;
            OnChanged();
        }
    }

    public string Name {
        get => KeyName;
        set {
            if (IsDisposed) { return; }
            if (KeyName == value) { return; }
            KeyName = value;
            OnChanged();
        }
    }

    public bool NeedRow {
        get => _needRow;
        set {
            if (IsDisposed) { return; }
            if (_needRow == value) { return; }
            _needRow = value;
            OnChanged();
        }
    }

    public string QuickInfo {
        get => _quickinfo;
        set {
            if (IsDisposed) { return; }
            if (_quickinfo == value) { return; }
            _quickinfo = value;
            OnChanged();
        }
    }

    public string ScriptText {
        get => _scripttext;
        set {
            if (IsDisposed) { return; }
            if (_scripttext == value) { return; }
            _scripttext = value;
            OnChanged();
        }
    }

    public List<string> UserGroups {
        get => _usergroups;
        set {
            if (IsDisposed) { return; }
            if (!_usergroups.IsDifferentTo(value)) { return; }
            _usergroups = value;
            OnChanged();
        }
    }

    #endregion

    #region Methods

    public object Clone() => new DatabaseScript(Database, ToString());

    public int CompareTo(object obj) {
        if (obj is DatabaseScript v) {
            return CompareKey.CompareTo(v.CompareKey);
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Falscher Objecttyp!");
        return 0;
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason() {
        if (Database?.IsDisposed ?? true) { return "Datenbank verworfen"; }

        if (string.IsNullOrEmpty(KeyName)) { return "Kein Name angegeben."; }

        if (!KeyName.IsFormat(FormatHolder.Text)) { return "Ungültiger Name"; }

        if (_eventTypes.HasFlag(ScriptEventTypes.prepare_formula)) {
            if (_changeValues) { return "Routinen, die das Formular vorbereiten, können keine Werte ändern."; }
            if (!_needRow) { return "Routinen, die das Formular vorbereiten, müssen sich auf Zeilen beziehen."; }
            if (_manualexecutable) { return "Routinen, die das Formular vorbereiten, können nicht so von außerhalb benutzt werden."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.export)) {
            if (_changeValues) { return "Routinen für Export können keine Werte ändern."; }
            if (!_needRow) { return "Routinen für Export müssen sich auf Zeilen beziehen."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread)) {
            if (_changeValues) { return "Routinen aus einem ExtraThread, können keine Werte ändern."; }
            if (!_needRow) { return "Routinen aus einem ExtraThread, müssen sich auf Zeilen beziehen."; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed)) {
            if (!_needRow) { return "Routinen, die Werteänderungen überwachen, müssen sich auf Zeilen beziehen."; }
            if (!_changeValues) { return "Routinen, die Werteänderungen überwachen, müssen auch Werte ändern dürfen"; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.new_row)) {
            if (!_needRow) { return "Routinen, die neue Zeilen überwachen, müssen sich auf Zeilen beziehen."; }
            if (!_changeValues) { return "Routinen, die neue Zeilen überwachen, müssen auch Werte ändern dürfen"; }
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.loaded)) {
            if (_needRow) { return "Routinen nach dem Laden einer Datenbank, dürfen sich nicht auf Zeilen beziehen."; }
        }

        if (_needRow && !Database.isRowScriptPossible(false)) { return "Zeilenskripte in dieser Datenbank nicht möglich"; }

        return string.Empty;
    }

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void Parse(string toParse) {
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "name":
                    KeyName = pair.Value.FromNonCritical();
                    break;

                case "script":

                    _scripttext = pair.Value.FromNonCritical();
                    break;

                case "needrow":

                    _needRow = pair.Value.FromPlusMinus();
                    break;

                case "manualexecutable":

                    _manualexecutable = pair.Value.FromPlusMinus();
                    break;

                case "changevalues":
                    _changeValues = pair.Value.FromPlusMinus();
                    break;

                case "events":
                    _eventTypes = (ScriptEventTypes)IntParse(pair.Value);
                    break;

                case "quickinfo":
                    _quickinfo = pair.Value.FromNonCritical();
                    break;

                case "admininfo":
                    _admininfo = pair.Value.FromNonCritical();
                    break;

                case "image":
                    _imagecode = pair.Value.FromNonCritical();
                    break;

                case "usergroups":
                    _usergroups = pair.Value.FromNonCritical().SplitAndCutByCrToList();
                    break;

                //case "lastdone":

                //    _lastUsed = pair.Value.FromNonCritical();
                //    break;

                case "database":
                    //Database = DatabaseAbstract.GetById(new ConnectionInfo(pair.Value.FromNonCritical(), null), null);
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
    }

    public string ReadableText() {
        var t = ErrorReason();
        if (!string.IsNullOrEmpty(t)) {
            return "Fehler: " + t;
        }

        return Name;
    }

    public QuickImage SymbolForReadableText() {
        if (!this.IsOk()) { return QuickImage.Get(ImageCode.Kritisch); }

        var symb = ImageCode.Formel;
        var c = Color.Transparent;

        if (_manualexecutable) {
            c = Color.Yellow;
            symb = ImageCode.Person;
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.export)) { symb = ImageCode.Layout; }
        if (_eventTypes.HasFlag(ScriptEventTypes.loaded)) { symb = ImageCode.Diskette; }
        if (_eventTypes.HasFlag(ScriptEventTypes.new_row)) { symb = ImageCode.Zeile; }
        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed)) { symb = ImageCode.Stift; }
        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread)) { symb = ImageCode.Wolke; }
        if (_eventTypes.HasFlag(ScriptEventTypes.prepare_formula)) { symb = ImageCode.Textfeld; }

        if (!_changeValues) { }

        return QuickImage.Get(symb, 16, c, Color.Transparent);
    }

    public override string ToString() {
        try {
            if (IsDisposed) { return string.Empty; }
            List<string> result = new();

            result.ParseableAdd("Name", Name);
            result.ParseableAdd("Script", ScriptText);
            result.ParseableAdd("ManualExecutable", ManualExecutable);
            result.ParseableAdd("NeedRow", NeedRow);
            result.ParseableAdd("ChangeValues", ChangeValues);
            result.ParseableAdd("Events", EventTypes);
            result.ParseableAdd("QuickInfo", QuickInfo);
            result.ParseableAdd("AdminInfo", AdminInfo);
            result.ParseableAdd("Image", Image);
            result.ParseableAdd("UserGroups", UserGroups);

            return result.Parseable();
        } catch {
            Develop.CheckStackForOverflow();
            return ToString();
        }
    }

    internal List<string> Attributes() {
        var s = new List<string>();
        if (!NeedRow) { s.Add("Rowless"); }
        if (!ChangeValues) { s.Add("NeverChangesValues"); }
        return s;
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            if (Database != null && !Database.IsDisposed) { Database.Disposing -= Database_Disposing; }
            Database = null;

            IsDisposed = true;
        }
    }

    #endregion
}