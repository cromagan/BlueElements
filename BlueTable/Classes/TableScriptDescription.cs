// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Classes;
using System.Collections.ObjectModel;
using System.IO;

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

public sealed class TableScriptDescription : ScriptDescription, IHasTable, IFragmentStoreable {

    #region Enums

    /// <summary>
    /// Empfänger-Typen des Skripts im binären Fragment-Format. Nur dem Skript selbst bekannt.
    /// </summary>
    private enum ScriptFragmentType : byte {

        /// <summary>
        /// Der Skript-Text selbst.
        /// </summary>
        Script = 1,

        QuickInfo = 2,

        AdminInfo = 3,

        Image = 4,

        /// <summary>
        /// Benutzergruppen; Content = Gruppennamen, durch „|“ getrennt.
        /// </summary>
        UserGroups = 5,

        FailedReason = 6,

        /// <summary>
        /// Gespeicherte Skript-Variablen (nur für alte Dateien; neue Dateien
        /// adressieren SavedVariables über die Sub*-Navigation).
        /// </summary>
        SavedVariables = 7,

        NeedRow = 8,

        ValuesReadOnly = 9,

        /// <summary>
        /// Auslösende Ereignisse; Content = int-Wert von ScriptEventTypes.
        /// </summary>
        Events = 10,

        StoppedTimeCount = 11,

        AverageRunTime = 12,

        /// <summary>
        /// Name des Skripts; erster Satz des Blocks, die Empfänger-Fabrik baut das
        /// Objekt daraus.
        /// </summary>
        ScriptName = 13
    }

    /// <summary>
    /// Pfad-Typen, unter denen das Skript Unter-Objekte adressiert.
    /// </summary>
    private enum ScriptNavigationType : byte {

        /// <summary>
        /// Navigation in die gespeicherten Skript-Variablen.
        /// </summary>
        SubVariableCollection = 94
    }

    #endregion

    #region Fields

    public const string CellValuesReadOnly = "CellValuesReadOnly";

    private bool? _mayAffectUser;

    #endregion

    #region Constructors

    public TableScriptDescription(Table? table, string keyName, string script, string image, string quickInfo, string adminInfo, ReadOnlyCollection<string> userGroups, ScriptEventTypes eventTypes, bool needRow, bool readOnly, string failedReason, IEnumerable<ScriptVariable>? savedVariables, int stoppedtimecount, long averageruntime) : base(adminInfo, image, keyName, quickInfo, script, userGroups, failedReason, savedVariables) {
        Table = table;
        EventTypes = eventTypes;
        NeedRow = needRow;
        ValuesReadOnly = readOnly;
        StoppedTimeCount = stoppedtimecount;
        AverageRunTime = averageruntime;
        _mayAffectUser = null;
        ObserveFragmentChain();
    }

    public TableScriptDescription(Table? table, string name, string script) : base(name, script) {
        Table = table;
        NeedRow = false;
        ObserveFragmentChain();
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
        set {
            // Beim Setzen auf 500 ms runden — so wird auch beim Parsen alter,
            // ungerundeter Daten normalisiert. Siehe RoundRunTime.
            value = RoundRunTime(value);
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Bei welchen Änderungen das Skript automatisch ausgeführt wird.
    /// </summary>
    public ScriptEventTypes EventTypes {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool MayAffectUser {
        get {
            if (_mayAffectUser is { } b) { return b; }

            if (AllowedMethodsMaxLevel(true) == ScriptCommandType.Standard) {
                _mayAffectUser = false;
                return false;
            }

            var a = false;

            if (StoppedTimeCount < 20) { a = true; }
            if (AverageRunTime > 5000) { a = true; }

            if (!a) {
                foreach (var thisc in ScriptCommand.AllMethods.Instances) {
                    if (thisc.ScriptCommandLevel >= ScriptCommandType.ManipulatesUser) {
                        if (Script?.IndexOfWord(thisc.Command, 0, System.Text.RegularExpressions.RegexOptions.IgnoreCase) >= 0) { a = true; break; }
                    }
                }
            }
            _mayAffectUser = a;

            return a;
        }
    }

    /// <summary>
    /// Das Skript arbeitet mit den Werten einer einzelnen Zeile.
    /// </summary>
    public bool NeedRow {
        get;
        set {
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
        set {
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

            field?.Disposed -= _table_Disposed;
            field = value;

            field?.Disposed += _table_Disposed;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Das Skript kann Werte nur lesen, nicht ändern.
    /// </summary>
    public bool ValuesReadOnly {
        get;
        set {
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

    public ScriptCommandType AllowedMethodsMaxLevel(bool extended) {
        if (EventTypes == ScriptEventTypes.Ohne_Auslöser) { return ScriptCommandType.GUI; }

        if (EventTypes.HasFlag(ScriptEventTypes.prepare_formula)) { return ScriptCommandType.Standard; }

        if (EventTypes == ScriptEventTypes.value_changed && extended) { return ScriptCommandType.ManipulatesUser; }

        //if (EventTypes == ScriptEventTypes.loaded) { return MethodType.ManipulatesUser; }

        if (EventTypes == ScriptEventTypes.ShownInTableView) { return ScriptCommandType.GUI; }

        return ScriptCommandType.Sub;
    }

    public override List<string> Attributes() {
        var s = new List<string>();
        if (!NeedRow) { s.Add("Rowless"); }
        if (ValuesReadOnly) { s.Add(CellValuesReadOnly); }
        return s;
    }

    public override int CompareTo(object? obj) {
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

        if (EventTypes.HasFlag(ScriptEventTypes.ShownInTableView)) {
            if (NeedRow) { return "Routinen beim Anzeigen der Tabelle dürfen sich nicht auf Zeilen beziehen."; }
            if (EventTypes != ScriptEventTypes.ShownInTableView) { return "Routinen für die Tabellenanzeige müssen für sich alleine stehen."; }
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

    public override bool ParseFinished() {
        if (MustBeReadonly(EventTypes)) {
            ValuesReadOnly = true;
        }

        return true;
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
        var c = System.Drawing.Color.Transparent;

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

        return QuickImage.Get(symb, 16, c, System.Drawing.Color.Transparent, h);
    }

    /// <summary>
    /// Rundet eine Laufzeit auf 500 ms, um Fragment-Schreibvorgänge zu reduzieren.
    /// Die Statistik dient nur der Anzeige (Sekunden) und MayAffectUser (&gt; 5000 ms),
    /// daher ist eine Auflösung von 500 ms mehr als ausreichend.
    /// </summary>
    internal static long RoundRunTime(long milliseconds) => ((milliseconds + 250) / 500) * 500;

    /// <summary>
    /// Verdrahtet die FragmentChanged-Bubbling-Kette: Das Skript meldet sich bei der
    /// Tabelle (Crumb = SubScript + eigener KeyName, je Event neu gelesen) und die
    /// gespeicherten Skript-Variablen beim Skript (Crumb = SubVariableCollection,
    /// leerer Key).
    /// </summary>
    private void ObserveFragmentChain() {
        if (Table is { IsDisposed: false } tb) { tb.Reregister(this); }

        this.Register(SavedVariables);
    }

    /// <summary>
    /// Übernimmt alle Werte von <paramref name="other"/> (Basis + Skript-spezifisch).
    /// Wird beim Recycling in Table.SetValueInternal (case EventScript)
    /// verwendet: Existierende Instanzen behalten ihre Identität, nur die Felder
    /// werden aktualisiert. Der MayAffectUser-Cache wird invalidiert.
    /// </summary>
    internal void UpdateFrom(TableScriptDescription other) {
        UpdateBaseFrom(other);
        EventTypes = other.EventTypes;
        NeedRow = other.NeedRow;
        ValuesReadOnly = other.ValuesReadOnly;
        StoppedTimeCount = other.StoppedTimeCount;
        AverageRunTime = other.AverageRunTime;
        _mayAffectUser = null;
    }

    protected override void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (Table is { IsDisposed: false } tb) { tb.Unregister(this); }

            this.Unregister(SavedVariables);

            Table = null;
        }

        base.Dispose(disposing);
    }

    private void _table_Disposed(object? sender, System.EventArgs e) => Dispose();

    #region IFragmentStoreable

    /// <summary>
    /// Bubbling-Event für Fragment-Änderungen (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public event EventHandler<FragmentChangedEventArgs>? FragmentChanged;

    /// <summary>
    /// Schreibt den Skript-Block: Name (erster Satz), Skript-Eigenschaften und die
    /// gespeicherten Skript-Variablen.
    /// </summary>
    public bool SerializeCore(Stream stream, int depth) {
        if (IsDisposed) { return true; }

        var allok = true;

        // Der Name ist der erste Satz: Die Empfänger-Fabrik baut das Objekt daraus
        allok &= FragmentRecord.Append(stream, depth, ScriptFragmentType.ScriptName, KeyName);

        // Immer alles hinzufügen, keine Ausschlüsse
        allok &= FragmentRecord.Append(stream, depth, ScriptFragmentType.Script, Script.Replace("\r\n", "\r").TrimEnd(' '));
        allok &= FragmentRecord.Append(stream, depth, ScriptFragmentType.QuickInfo, QuickInfo.Replace("\r\n", "\r").TrimEnd(' '));
        allok &= FragmentRecord.Append(stream, depth, ScriptFragmentType.AdminInfo, AdminInfo.Replace("\r\n", "\r").TrimEnd(' '));
        allok &= FragmentRecord.Append(stream, depth, ScriptFragmentType.Image, Image);
        allok &= FragmentRecord.Append(stream, depth, ScriptFragmentType.UserGroups, UserGroups);
        allok &= FragmentRecord.Append(stream, depth, ScriptFragmentType.FailedReason, FailedReason.Replace("\r\n", "\r").TrimEnd(' '));

        allok &= FragmentRecord.Append(stream, depth, ScriptNavigationType.SubVariableCollection, SavedVariables);

        allok &= FragmentRecord.Append(stream, depth, ScriptFragmentType.NeedRow, NeedRow);
        allok &= FragmentRecord.Append(stream, depth, ScriptFragmentType.ValuesReadOnly, ValuesReadOnly);
        allok &= FragmentRecord.Append(stream, depth, ScriptFragmentType.Events, (int)EventTypes);
        allok &= FragmentRecord.Append(stream, depth, ScriptFragmentType.StoppedTimeCount, StoppedTimeCount);
        allok &= FragmentRecord.Append(stream, depth, ScriptFragmentType.AverageRunTime, AverageRunTime);

        return allok;
    }

    /// <summary>
    /// Verarbeitet den Skript-Block: Skript-Eigenschaften im Switch, die gespeicherten
    /// Variablen über den SubVariableCollection-Satz (leerer Key) eine Ebene tiefer.
    /// </summary>
    public bool DeserializeThis(Stream stream, FragmentRecord record) {
        // Die gespeicherten Variablen sind ein eigenes Unter-Objekt (leerer Key)
        if ((ScriptNavigationType)record.LeafType == ScriptNavigationType.SubVariableCollection) { return SavedVariables.Deserialize(stream, record.Depth + 1); }

        switch ((ScriptFragmentType)record.LeafType) {
            case ScriptFragmentType.ScriptName:
                // Name steckt im Objekt; die Empfänger-Fabrik hat ihn übernommen
                break;

            case ScriptFragmentType.Script:
                Script = record.StringContent;
                break;

            case ScriptFragmentType.QuickInfo:
                QuickInfo = record.StringContent;
                break;

            case ScriptFragmentType.AdminInfo:
                AdminInfo = record.StringContent;
                break;

            case ScriptFragmentType.Image:
                Image = record.StringContent;
                break;

            case ScriptFragmentType.UserGroups:
                UserGroups = [.. record.ListContent];
                break;

            case ScriptFragmentType.FailedReason:
                FailedReason = record.StringContent;
                break;

            case ScriptFragmentType.NeedRow:
                NeedRow = record.BoolContent;
                break;

            case ScriptFragmentType.ValuesReadOnly:
                ValuesReadOnly = record.BoolContent;
                break;

            case ScriptFragmentType.Events:
                EventTypes = (ScriptEventTypes)record.IntContent;
                break;

            case ScriptFragmentType.StoppedTimeCount:
                StoppedTimeCount = record.IntContent;
                _mayAffectUser = null;
                break;

            case ScriptFragmentType.AverageRunTime:
                AverageRunTime = record.LongContent;
                break;

            default:
                // Unbekannter Blatt-Typ — Satz überspringen
                break;
        }

        return true;
    }

    /// <summary>
    /// Reicht eine Änderung der gespeicherten Variablen mit dem eigenen Pfad-Segment
    /// (SubVariableCollection, leerer Key) eine Ebene höher.
    /// </summary>
    public void ChildFragmentChanged(object? sender, FragmentChangedEventArgs e) {
        OnFragmentChanged(new FragmentChangedEventArgs(this, FragmentChangedEventArgs.Crumbs(ScriptNavigationType.SubVariableCollection, FragmentChangedEventArgs.CrumbKey(sender), e.RelativePath), e.TypeSelector, e.ContentType, e.Content) { ListContent = e.ListContent });
    }

    /// <summary>
    /// Löst das FragmentChanged-Event mit den fertigen Argumenten aus (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public void OnFragmentChanged(FragmentChangedEventArgs e) {
        if (IsDisposed) { return; }

        FragmentChanged?.Invoke(this, e);
    }

    #endregion

    #endregion
}