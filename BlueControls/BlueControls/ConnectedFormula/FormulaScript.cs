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
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.Converter;

namespace BlueControls.ConnectedFormula;

public static class FormulaScriptExtension {

    #region Methods

    public static List<FormulaScript> Get(this ReadOnlyCollection<FormulaScript> scripts, DatabaseEventTypes type) {
        var l = new List<FormulaScript>();

        foreach (var thisScript in scripts) {
            if (thisScript.EventTypes.HasFlag(type)) { l.Add(thisScript); }
        }

        return l;
    }

    #endregion
}

public sealed class FormulaScript : IParseable, IReadableTextWithChangingAndKey, IDisposableExtended, ICloneable, IErrorCheckable, IHasKeyName, IComparable, IChangedFeedback {

    #region Fields

    private bool _changeValues;
    private DatabaseEventTypes _eventTypes = 0;
    private bool _executable;
    private string _script;

    #endregion

    #region Constructors

    public FormulaScript(ConnectedFormula formula, string name, string script) : this(formula) {
        KeyName = name;
        _script = script;
    }

    public FormulaScript(ConnectedFormula? formula, string toParse) : this(formula) => Parse(toParse);

    public FormulaScript(ConnectedFormula? formula) {
        Formula = formula;

        //if (Database != null && !Database.IsDisposed) {
        //    Database.Disposing += Database_Disposing;
        //}

        KeyName = string.Empty;
        _script = string.Empty;
        _executable = false;
    }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~FormulaScript() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    public bool ChangeValues {
        get => _changeValues;
        set {
            if (_changeValues == value) { return; }
            _changeValues = value;
            OnChanged();
        }
    }

    public string CompareKey => Name.ToString();

    public DatabaseEventTypes EventTypes {
        get => _eventTypes;
        set {
            if (_eventTypes == value) { return; }
            _eventTypes = value;
            OnChanged();
        }
    }

    public ConnectedFormula? Formula { get; private set; }
    public bool IsDisposed { get; private set; }

    public string KeyName { get; private set; }

    public bool ManualExecutable {
        get => _executable;
        set {
            if (_executable == value) { return; }
            _executable = value;
            OnChanged();
        }
    }

    public string Name {
        get => KeyName;
        set {
            if (KeyName == value) { return; }
            KeyName = value;
            OnChanged();
        }
    }

    public string Script {
        get => _script;
        set {
            if (_script == value) { return; }
            _script = value;
            OnChanged();
        }
    }

    #endregion

    #region Methods

    public object Clone() => new FormulaScript(Formula, ToString());

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
        if (Formula?.IsDisposed ?? true) { return "Formular verworfen"; }

        if (string.IsNullOrEmpty(KeyName)) { return "Kein Name angegeben."; }

        if (!KeyName.IsFormat(FormatHolder.Text)) { return "Ungültiger Name"; }

        //if (_eventTypes.HasFlag(DatabaseEventTypes.prepare_formula)) {
        //    if (_changeValues) { return "Routinen, die das Formular vorbereiten, können keine Werte ändern."; }
        //    if (!_needRow) { return "Routinen, die das Formular vorbereiten, müssen sich auf Zeilen beziehen."; }
        //    if (_executable) { return "Routinen, die das Formular vorbereiten, können nicht so von außerhalb benutzt werden."; }
        //}

        //if (_eventTypes.HasFlag(DatabaseEventTypes.export)) {
        //    if (_changeValues) { return "Routinen für Export können keine Werte ändern."; }
        //    if (!_needRow) { return "Routinen für Export müssen sich auf Zeilen beziehen."; }
        //}

        //if (_eventTypes.HasFlag(DatabaseEventTypes.value_changed_extra_thread)) {
        //    if (_changeValues) { return "Routinen aus einem ExtraThread, können keine Werte ändern."; }
        //    if (!_needRow) { return "Routinen aus einem ExtraThread, müssen sich auf Zeilen beziehen."; }
        //}

        //if (_eventTypes.HasFlag(DatabaseEventTypes.value_changed)) {
        //    if (!_needRow) { return "Routinen, die Werteänderungen überwachen, müssen sich auf Zeilen beziehen."; }
        //    if (!_changeValues) { return "Routinen, die Werteänderungen überwachen, müssen auch Werte ändern dürfen"; }
        //}

        //if (_eventTypes.HasFlag(DatabaseEventTypes.new_row)) {
        //    if (!_needRow) { return "Routinen, die neue Zeilen überwachen, müssen sich auf Zeilen beziehen."; }
        //    if (!_changeValues) { return "Routinen, die neue Zeilen überwachen, müssen auch Werte ändern dürfen"; }
        //}

        //if (_eventTypes.HasFlag(DatabaseEventTypes.database_loaded)) {
        //    if (_needRow) { return "Routinen nach dem Laden einer Datenbank, dürfen sich nicht auf Zeilen beziehen."; }
        //}

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

                    _script = pair.Value.FromNonCritical();
                    break;

                case "manualexecutable":

                    _executable = pair.Value.FromPlusMinus();
                    break;

                case "changevalues":
                    _changeValues = pair.Value.FromPlusMinus();
                    break;

                case "events":
                    _eventTypes = (DatabaseEventTypes)IntParse(pair.Value);
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

        if (_executable) {
            c = Color.Yellow;
            symb = ImageCode.Person;
        }

        if (_eventTypes.HasFlag(DatabaseEventTypes.export)) { symb = ImageCode.Layout; }
        if (_eventTypes.HasFlag(DatabaseEventTypes.database_loaded)) { symb = ImageCode.Diskette; }
        if (_eventTypes.HasFlag(DatabaseEventTypes.new_row)) { symb = ImageCode.Zeile; }
        if (_eventTypes.HasFlag(DatabaseEventTypes.value_changed)) { symb = ImageCode.Stift; }
        if (_eventTypes.HasFlag(DatabaseEventTypes.value_changed_extra_thread)) { symb = ImageCode.Wolke; }
        if (_eventTypes.HasFlag(DatabaseEventTypes.prepare_formula)) { symb = ImageCode.Textfeld; }

        if (!_changeValues) { }

        return QuickImage.Get(symb, 16, c, Color.Transparent);
    }

    public override string ToString() {
        try {
            var result = new List<string>();

            //result.ParseableAdd("Database", Database);
            result.ParseableAdd("Name", Name);
            result.ParseableAdd("Script", Script);
            result.ParseableAdd("ManualExecutable", ManualExecutable);
            result.ParseableAdd("ChangeValues", ChangeValues);
            result.ParseableAdd("Events", EventTypes);

            return result.Parseable();
        } catch {
            return ToString();
        }
    }

    internal string Attributes() {
        var s = string.Empty;
        if (ChangeValues) { s += "ChangeValues"; }
        return s;
    }

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            //if (Database != null && !Database.IsDisposed) { Database.Disposing -= Database_Disposing; }
            Formula = null;

            IsDisposed = true;
        }
    }

    #endregion
}