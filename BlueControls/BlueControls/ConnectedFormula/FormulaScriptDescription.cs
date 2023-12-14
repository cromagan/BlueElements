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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueScript;
using static BlueBasics.Converter;

namespace BlueControls.ConnectedFormula;

public static class FormulaScriptExtension {

    #region Methods

    public static List<FormulaScriptDescription> Get(this ReadOnlyCollection<FormulaScriptDescription> scripts, ScriptEventTypes type) {
        var l = new List<FormulaScriptDescription>();

        foreach (var thisScript in scripts) {
            if (thisScript.EventTypes.HasFlag(type)) { l.Add(thisScript); }
        }

        return l;
    }

    #endregion
}

public sealed class FormulaScriptDescription : ScriptDescription, IParseable, IReadableTextWithChangingAndKey, IDisposableExtended, ICloneable, IErrorCheckable, IHasKeyName, IChangedFeedback {

    #region Fields

    private ScriptEventTypes _eventTypes = 0;

    #endregion

    #region Constructors

    public FormulaScriptDescription(ConnectedFormula formula, string name, string script) : base(name, script) => Formula = formula;

    public FormulaScriptDescription(ConnectedFormula? formula, string toParse) : this(formula) => this.Parse(toParse);

    public FormulaScriptDescription(ConnectedFormula? formula) : base(string.Empty, string.Empty) => Formula = formula;

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~FormulaScriptDescription() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Properties

    public ScriptEventTypes EventTypes {
        get => _eventTypes;
        set {
            if (IsDisposed) { return; }
            if (_eventTypes == value) { return; }
            _eventTypes = value;
            OnChanged();
        }
    }

    public ConnectedFormula? Formula { get; private set; }

    #endregion

    #region Methods

    public override List<string> Attributes() {
        var s = new List<string>();
        if (!ChangeValues) { s.Add("NeverChangesValues"); }
        return s;
    }

    public object Clone() => new FormulaScriptDescription(Formula, ToString());

    public override int CompareTo(object obj) {
        if (obj is FormulaScriptDescription v) {
            return string.Compare(CompareKey, v.CompareKey, StringComparison.Ordinal);
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Falscher Objecttyp!");
        return 0;
    }

    public override string ErrorReason() {
        if (Formula?.IsDisposed ?? true) { return "Formular verworfen"; }

        var b = base.ErrorReason();

        if (!string.IsNullOrEmpty(b)) { return b; }

        return string.Empty;
    }

    public override bool ParseThis(string key, string value) {
        if (base.ParseThis(key, value)) { return true; }

        switch (key) {
            case "events":
                _eventTypes = (ScriptEventTypes)IntParse(value);
                break;
        }

        return false;
    }

    public override QuickImage SymbolForReadableText() {
        var i = base.SymbolForReadableText();
        if (i != null) { return i; }

        var symb = ImageCode.Formel;
        var c = Color.Transparent;

        if (ManualExecutable) {
            c = Color.Yellow;
            symb = ImageCode.Person;
        }

        if (_eventTypes.HasFlag(ScriptEventTypes.export)) { symb = ImageCode.Layout; }
        if (_eventTypes.HasFlag(ScriptEventTypes.loaded)) { symb = ImageCode.Diskette; }
        if (_eventTypes.HasFlag(ScriptEventTypes.new_row)) { symb = ImageCode.Zeile; }
        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed)) { symb = ImageCode.Stift; }
        if (_eventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread)) { symb = ImageCode.Wolke; }
        if (_eventTypes.HasFlag(ScriptEventTypes.prepare_formula)) { symb = ImageCode.Textfeld; }

        return QuickImage.Get(symb, 16, c, Color.Transparent);
    }

    public override string ToString() {
        try {
            if (IsDisposed) { return string.Empty; }
            List<string> result = [];
            result.ParseableAdd("Events", _eventTypes);
            return result.Parseable(base.ToString());
        } catch {
            return ToString();
        }
    }

    protected override void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            //if (Database != null && !Database.IsDisposed) { Database.DisposingEvent -= Database_DisposingEvent; }
            Formula = null;
        }

        base.Dispose(disposing);
    }

    #endregion
}