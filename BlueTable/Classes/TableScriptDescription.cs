// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueScript;
using BlueScript.Enums;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using static BlueBasics.Converter;

namespace BlueTable;

public static class TableScriptDescriptionExtension {

    #region Methods

    public static List<TableScriptDescription> Get(this ReadOnlyCollection<TableScriptDescription> scripts, ScriptEventTypes type) {
        try {
            return [.. scripts.Where(script => script.EventTypes.HasFlag(type))];
        } catch {
            Develop.AbortAppIfStackOverflow();
            return scripts.Get(type);
        }
    }

    #endregion
}

public sealed class TableScriptDescription : ScriptDescription, IHasTable {

    #region Constructors

    public TableScriptDescription(Table? table, string keyName, string script, string image, string quickInfo, string adminInfo, ReadOnlyCollection<string> userGroups, ScriptEventTypes eventTypes, bool needRow, string failedReason) : base(adminInfo, image, keyName, quickInfo, script, userGroups, failedReason) {
        Table = table;
        EventTypes = eventTypes;
        NeedRow = needRow;
    }

    public TableScriptDescription(Table? table, string name, string script) : base(name, script) {
        Table = table;
        NeedRow = false;
    }

    public TableScriptDescription(Table? table, string toParse) : this(table) => this.Parse(toParse);

    public TableScriptDescription(Table? table) : this(table, string.Empty, string.Empty) { }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~TableScriptDescription() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Properties

    public bool ChangeValuesAllowed {
        get {
            if (EventTypes.HasFlag(ScriptEventTypes.prepare_formula)) { return false; }
            if (EventTypes.HasFlag(ScriptEventTypes.export)) { return false; }
            if (EventTypes.HasFlag(ScriptEventTypes.row_deleting)) { return false; }
            if (EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread)) { return false; }
            return true;
        }
    }

    public ScriptEventTypes EventTypes { get; private set; }

    public bool NeedRow { get; private set; }

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            if (field != null) {
                field.DisposingEvent -= _table_Disposing;
            }
            field = value;

            if (field != null) {
                field.DisposingEvent += _table_Disposing;
            }
        }
    }

    /// <summary>
    /// Nichtspeicherbare Spalten werden bei FormularVorbereiten benötigt.
    /// </summary>
    public bool VirtalColumns => EventTypes.HasFlag(ScriptEventTypes.prepare_formula);

    #endregion

    #region Methods

    public MethodType AllowedMethodsMaxLevel(bool extended) {
        if (EventTypes == ScriptEventTypes.Ohne_Auslöser) { return MethodType.GUI; }

        if (EventTypes.HasFlag(ScriptEventTypes.prepare_formula)) { return MethodType.Standard; }

        if (EventTypes == ScriptEventTypes.value_changed && extended) { return MethodType.ManipulatesUser; }

        //if (EventTypes == ScriptEventTypes.loaded) { return MethodType.ManipulatesUser; }

        return MethodType.LongTime;
    }

    public override List<string> Attributes() {
        var s = new List<string>();
        if (!NeedRow) { s.Add("Rowless"); }
        //if (!ChangeValuesAllowed) { s.Add("NeverChangesValues"); }
        return s;
    }

    public override int CompareTo(object obj) {
        if (obj is TableScriptDescription v) {
            return string.Compare(CompareKey, v.CompareKey, StringComparison.Ordinal);
        }

        Develop.DebugPrint(ErrorType.Error, "Falscher Objecttyp!");
        return 0;
    }

    public override string ErrorReason() {
        if (Table is not { IsDisposed: false } tb) { return "Tabelle verworfen"; }

        if (tb is TableChunk) {
            if (!NeedRow && ChangeValuesAllowed && EventTypes != ScriptEventTypes.Ohne_Auslöser) { return "Gechunkte Tabellen unterstütze nur Zeilenskripte."; }
        }

        if (EventTypes.HasFlag(ScriptEventTypes.prepare_formula)) {
            //if (ChangeValuesAllowed) { return "Routinen, die das Formular vorbereiten, können keine Werte ändern."; }
            if (!NeedRow) { return "Routinen, die das Formular vorbereiten, müssen sich auf Zeilen beziehen."; }
            if (UserGroups.Count > 0) { return "Routinen, die das Formular vorbereiten, können nicht von außerhalb benutzt werden."; }
            if (EventTypes != ScriptEventTypes.prepare_formula) { return "Routinen für den Export müssen für sich alleine stehen."; }
        }

        if (EventTypes.HasFlag(ScriptEventTypes.export)) {
            //if (ChangeValuesAllowed) { return "Routinen für Export können keine Werte ändern."; }
            if (!NeedRow) { return "Routinen für Export müssen sich auf Zeilen beziehen."; }
            if (UserGroups.Count > 0) { return "Routinen, die den Export vorbereiten, können nicht von außerhalb benutzt werden."; }
            if (EventTypes != ScriptEventTypes.export) { return "Routinen für den Export müssen für sich alleine stehen."; }
        }

        if (EventTypes.HasFlag(ScriptEventTypes.row_deleting)) {
            //if (ChangeValuesAllowed) { return "Routinen für das Löschen einer Zeile können keine Werte ändern."; }
            if (!NeedRow) { return "Routinen für das Löschen einer Zeile müssen sich auf Zeilen beziehen."; }
            if (UserGroups.Count > 0) { return "Routinen, für das Löschen einer Zeile, können nicht von außerhalb benutzt werden."; }
            if (EventTypes != ScriptEventTypes.row_deleting) { return "Routinen für für das Löschen einer Zeile müssen für sich alleine stehen."; }
        }

        //if (EventTypes.HasFlag(ScriptEventTypes.correct_changed)) {
        //    //if (ChangeValuesAllowed) { return "Routinen für das Löschen einer Zeile können keine Werte ändern."; }
        //    if (!NeedRow) { return "Routinen, die den Fehlerfrei-Status überwachen, einer Zeile müssen sich auf Zeilen beziehen."; }
        //    //if (UserGroups.Count > 0) { return "Routinen, die den Fehlerfrei-Status überwachen, können nicht von außerhalb benutzt werden."; }
        //    if (EventTypes != ScriptEventTypes.correct_changed) { return "Routinen, die den Fehlerfrei-Status überwachen, müssen für sich alleine stehen."; }
        //}

        if (EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread)) {
            //if (ChangeValuesAllowed) { return "Routinen aus einem ExtraThread, können keine Werte ändern."; }
            if (!NeedRow) { return "Routinen aus einem ExtraThread, müssen sich auf Zeilen beziehen."; }
        }

        if (EventTypes.HasFlag(ScriptEventTypes.value_changed)) {
            if (!NeedRow) { return "Routinen, die Werteänderungen überwachen, müssen sich auf Zeilen beziehen."; }
            //if (!ChangeValuesAllowed) { return "Routinen, die Werteänderungen überwachen, müssen auch Werte ändern dürfen."; }
        }

        if (EventTypes.HasFlag(ScriptEventTypes.InitialValues)) {
            if (!NeedRow) { return "Routinen, die neue Zeilen überwachen, müssen sich auf Zeilen beziehen."; }
            //if (!ChangeValuesAllowed) { return "Routinen, die neue Zeilen überwachen, müssen auch Werte ändern dürfen."; }
            if (UserGroups.Count > 0) { return "Routinen, die die Zeilen initialsieren, können nicht von außerhalb benutzt werden."; }
            if (EventTypes != ScriptEventTypes.InitialValues) { return "Routinen zum Initialisieren müssen für sich alleine stehen."; }
        }

        //if (EventTypes.HasFlag(ScriptEventTypes.loaded)) {
        //    if (NeedRow) { return "Routinen nach dem Laden einer Tabelle, dürfen sich nicht auf Zeilen beziehen."; }
        //}

        if (NeedRow && !Table.IsRowScriptPossible()) { return "Zeilenskripte in dieser Tabelle nicht möglich"; }

        if (EventTypes.ToString() == ((int)EventTypes).ToString1()) { return "Skripte öffnen und neu speichern."; }

        foreach (var script in tb.EventScript) {
            if (script != this) {
                if (string.Equals(script.KeyName, KeyName, StringComparison.OrdinalIgnoreCase)) { return $"Skriptname '{script.KeyName}' mehrfach vorhanden"; }

                var gemeinsame = script.EventTypes & EventTypes;
                if (gemeinsame != 0) { return $"Skript-Typ '{gemeinsame}' mehrfach vorhanden"; }
            }
        }

        return base.ErrorReason();
    }

    public override List<string> ParseableItems() {
        try {
            if (IsDisposed) { return []; }
            List<string> result = [.. base.ParseableItems()];
            result.ParseableAdd("NeedRow", NeedRow);
            result.ParseableAdd("Events", EventTypes);
            return result;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return ParseableItems();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "needrow":
                NeedRow = value.FromPlusMinus();
                return true;

            case "events":
                EventTypes = (ScriptEventTypes)IntParse(value);
                return true;

            case "database":
            case "table":
                //Table = Table.GetById(new ConnectionInfo(pair.Value.FromNonCritical(), null), null);
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

        if (EventTypes.HasFlag(ScriptEventTypes.export)) { symb = ImageCode.Layout; }
        //if (EventTypes.HasFlag(ScriptEventTypes.loaded)) { symb = ImageCode.Diskette; }
        if (EventTypes.HasFlag(ScriptEventTypes.InitialValues)) { symb = ImageCode.Zeile; }
        if (EventTypes.HasFlag(ScriptEventTypes.value_changed)) { symb = ImageCode.Stift; }
        if (EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread)) { symb = ImageCode.Wolke; }
        if (EventTypes.HasFlag(ScriptEventTypes.prepare_formula)) { symb = ImageCode.Textfeld; }

        return QuickImage.Get(symb, 16, c, Color.Transparent, h);
    }

    protected override void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            Table = null;
        }

        base.Dispose(disposing);
    }

    private void _table_Disposing(object sender, System.EventArgs e) => Dispose();

    #endregion
}