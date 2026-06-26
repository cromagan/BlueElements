// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.ObjectModel;
using System.Drawing;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueTable.Classes;

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

    #region Fields

    public const string CellValuesReadOnly = "CellValuesReadOnly";

    private bool? _mayAffectUser;

    #endregion

    #region Constructors

    public TableScriptDescription(Table? table, string keyName, string script, string image, string quickInfo, string adminInfo, ReadOnlyCollection<string> userGroups, ScriptEventTypes eventTypes, bool needRow, bool readOnly, string failedReason, List<Variable>? savedVariables, int stoppedtimecount, long averageruntime) : base(adminInfo, image, keyName, quickInfo, script, userGroups, failedReason, savedVariables) {
        Table = table;
        EventTypes = eventTypes;
        NeedRow = needRow;
        ValuesReadOnly = readOnly;
        StoppedTimeCount = stoppedtimecount;
        AverageRunTime = averageruntime;
        _mayAffectUser = null;
    }

    public TableScriptDescription(Table? table, string name, string script) : base(name, script) {
        Table = table;
        NeedRow = false;
    }

    public TableScriptDescription(Table? table, string toParse) : this(table) => this.Parse(toParse);

    public TableScriptDescription(Table? table) : this(table, string.Empty, string.Empty) { }

    #endregion

    #region Properties

    /// <summary>
    /// Die Durschnittliche Laufzeit eines Scripts in Millisekunden
    /// </summary>
    public long AverageRunTime {
        get;
        private set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public ScriptEventTypes EventTypes {
        get;
        private set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool MayAffectUser {
        get {
            if (_mayAffectUser is { } b) { return b; }

            if (AllowedMethodsMaxLevel(true) == MethodType.Standard) {
                _mayAffectUser = false;
                return false;
            }

            var a = false;

            if (StoppedTimeCount < 20) { a = true; }
            if (AverageRunTime > 5000) { a = true; }

            if (!a) {
                foreach (var thisc in Method.AllMethods.Instances) {
                    if (thisc.MethodLevel >= MethodType.ManipulatesUser) {
                        if (Script?.IndexOfWord(thisc.Command, 0, System.Text.RegularExpressions.RegexOptions.IgnoreCase) >= 0) { a = true; break; }
                    }
                }
            }
            _mayAffectUser = a;

            return a;
        }
    }

    public bool NeedRow {
        get;
        private set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// So viele Laufzeit-Messnungen wurden abgespeichert.
    /// </summary>
    public int StoppedTimeCount {
        get;
        private set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            field?.DisposingEvent -= _table_Disposing;
            field = value;

            field?.DisposingEvent += _table_Disposing;
            OnPropertyChanged();
        }
    }

    public bool ValuesReadOnly {
        get;
        private set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Nichtspeicherbare Spalten werden bei FormularVorbereiten benötigt.
    /// </summary>
    public bool VirtalColumns => EventTypes.HasFlag(ScriptEventTypes.prepare_formula);

    #endregion

    #region Methods

    public static bool MustBeReadonly(ScriptEventTypes type) {
        if (type.HasFlag(ScriptEventTypes.prepare_formula)) { return true; }
        if (type.HasFlag(ScriptEventTypes.export)) { return true; }
        if (type.HasFlag(ScriptEventTypes.row_deleting)) { return true; }
        if (type.HasFlag(ScriptEventTypes.value_changed_extra_thread)) { return true; }
        return false;
    }

    public MethodType AllowedMethodsMaxLevel(bool extended) {
        if (EventTypes == ScriptEventTypes.Ohne_Auslöser) { return MethodType.GUI; }

        if (EventTypes.HasFlag(ScriptEventTypes.prepare_formula)) { return MethodType.Standard; }

        if (EventTypes == ScriptEventTypes.value_changed && extended) { return MethodType.ManipulatesUser; }

        //if (EventTypes == ScriptEventTypes.loaded) { return MethodType.ManipulatesUser; }

        return MethodType.Sub;
    }

    public override List<string> Attributes() {
        var s = new List<string>();
        if (!NeedRow) { s.Add("Rowless"); }
        if (ValuesReadOnly) { s.Add(CellValuesReadOnly); }
        return s;
    }

    public override int CompareTo(object obj) {
        if (obj is TableScriptDescription v) {
            return string.Compare(CompareKey, v.CompareKey, StringComparison.Ordinal);
        }

        Develop.DebugError("Falscher Objecttyp!");
        return 0;
    }

    /// <summary>
    /// Vergleicht diese Skript-Beschreibung mit einer anderen auf inhaltliche Gleichheit.
    /// Reine Laufzeit-Statistiken (AverageRunTime, StoppedTimeCount) werden bewusst
    /// ignoriert, da sie das Prüf- bzw. Ausführungsergebnis einer Zeile nicht beeinflussen.
    /// </summary>
    public bool ContentEquals(TableScriptDescription? other) {
        if (ReferenceEquals(this, other)) { return true; }
        if (other is null) { return false; }

        return KeyName == other.KeyName
            && Script == other.Script
            && Image == other.Image
            && QuickInfo == other.QuickInfo
            && AdminInfo == other.AdminInfo
            && EventTypes == other.EventTypes
            && NeedRow == other.NeedRow
            && ValuesReadOnly == other.ValuesReadOnly
            && UserGroups.SequenceEqual(other.UserGroups)
            && FailedReason == other.FailedReason;
    }

    public override string ErrorReason() {
        if (Table is not { IsDisposed: false } tb) { return "Tabelle verworfen"; }

        if (tb is TableChunk) {
            if (!NeedRow && !ValuesReadOnly && EventTypes != ScriptEventTypes.Ohne_Auslöser) { return "Gechunkte Tabellen unterstützen nur Zeilenskripte."; }
        }

        if (EventTypes.HasFlag(ScriptEventTypes.prepare_formula)) {
            if (!ValuesReadOnly) { return "Routinen, die das Formular vorbereiten, können keine Werte ändern."; }
            if (!NeedRow) { return "Routinen, die das Formular vorbereiten, müssen sich auf Zeilen beziehen."; }
            if (UserGroups.Count > 0) { return "Routinen, die das Formular vorbereiten, können nicht von außerhalb benutzt werden."; }
            if (EventTypes != ScriptEventTypes.prepare_formula) { return "Routinen für den Export müssen für sich alleine stehen."; }
        }

        if (EventTypes.HasFlag(ScriptEventTypes.export)) {
            if (!ValuesReadOnly) { return "Routinen für Export können keine Werte ändern."; }
            if (!NeedRow) { return "Routinen für Export müssen sich auf Zeilen beziehen."; }
            if (UserGroups.Count > 0) { return "Routinen, die den Export vorbereiten, können nicht von außerhalb benutzt werden."; }
            if (EventTypes != ScriptEventTypes.export) { return "Routinen für den Export müssen für sich alleine stehen."; }
        }

        if (EventTypes.HasFlag(ScriptEventTypes.row_deleting)) {
            if (!ValuesReadOnly) { return "Routinen für das Löschen einer Zeile können keine Werte ändern."; }
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
            if (!ValuesReadOnly) { return "Routinen aus einem ExtraThread, können keine Werte ändern."; }
            if (!NeedRow) { return "Routinen aus einem ExtraThread, müssen sich auf Zeilen beziehen."; }
        }

        if (EventTypes.HasFlag(ScriptEventTypes.value_changed)) {
            if (!NeedRow) { return "Routinen, die Werteänderungen überwachen, müssen sich auf Zeilen beziehen."; }
            if (ValuesReadOnly) { return "Routinen, die Werteänderungen überwachen, müssen auch Werte ändern dürfen."; }
            if (EventTypes != ScriptEventTypes.value_changed) { return "Routinen, die Werteänderungen überwachen, dürfen keine weitern Auslöser haben."; }
        }

        if (EventTypes.HasFlag(ScriptEventTypes.InitialValues)) {
            if (!NeedRow) { return "Routinen, die neue Zeilen überwachen, müssen sich auf Zeilen beziehen."; }
            if (ValuesReadOnly) { return "Routinen, die neue Zeilen überwachen, müssen auch Werte ändern dürfen."; }
            if (UserGroups.Count > 0) { return "Routinen, die die Zeilen initialsieren, können nicht von außerhalb benutzt werden."; }
            if (EventTypes != ScriptEventTypes.InitialValues) { return "Routinen zum Initialisieren dürfen keine weitern Auslöser haben."; }
        }

        //if (EventTypes.HasFlag(ScriptEventTypes.loaded)) {
        //    if (NeedRow) { return "Routinen nach dem Laden einer Tabelle, dürfen sich nicht auf Zeilen beziehen."; }
        //}

        if (NeedRow && !Table.IsRowScriptPossible()) { return "Zeilenskripte in dieser Tabelle nicht möglich"; }

        if (EventTypes.ToString() == ((int)EventTypes).ToString1()) { return "Skripte öffnen und neu speichern."; }

        // Bei jeder Aktualisierung der Collection werden alle Objekte per Deserialisierung
        // neu erstellt (SetValueInternal → case EventScript). Hält eine UI-Komponente noch
        // eine Referenz auf ein veraltetes Objekt, würde die Referenzprüfung (script != this)
        // für jeden Eintrag einen False Positive beim Duplikat-Check auslösen.
        if (!tb.EventScript.Contains(this)) { return base.ErrorReason(); }

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
            result.ParseableAdd("ValuesReadOnly", ValuesReadOnly);
            result.ParseableAdd("Events", EventTypes);
            result.ParseableAdd("StoppedTimeCount", StoppedTimeCount);
            result.ParseableAdd("AverageRunTime", AverageRunTime);
            return result;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return ParseableItems();
        }
    }

    public override void ParseFinished(string parsed) {
        if (MustBeReadonly(EventTypes)) {
            ValuesReadOnly = true;
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "name":
                value = value.FromNonCritical();
                break;

            case "needrow":
                NeedRow = value.FromPlusMinus();
                return true;

            case "valuesreadonly":
                ValuesReadOnly = value.FromPlusMinus();
                return true;

            case "events":
                EventTypes = (ScriptEventTypes)IntParse(value);
                return true;

            case "database":
            case "table":
                //Table = Table.GetById(new ConnectionInfo(pair.Value.FromNonCritical(), null), null);
                return true;

            case "stoppedtimecount":
                StoppedTimeCount = IntParse(value.FromNonCritical());
                _mayAffectUser = null;
                return true;

            case "averageruntime":
                AverageRunTime = LongParse(value.FromNonCritical());
                return true;
        }

        return base.ParseThis(key, value);
    }

    public override QuickImage SymbolForReadableText() {
        if (base.SymbolForReadableText() is { } i) { return i; }

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

    private void _table_Disposing(object? sender, System.EventArgs e) => Dispose();

    #endregion
}