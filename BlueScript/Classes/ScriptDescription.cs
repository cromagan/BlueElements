// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;

namespace BlueScript.Classes;

/// <summary>
/// Delegate für die Test-Ausführung eines Skripts aus einem Editor oder
/// PadItem heraus. Parameter: Skripttext, Testmodus. Rückgabe: Feedback.
/// </summary>
public delegate ScriptEndedFeedback ExecuteScriptDelegate(string script, bool testmode);

public class ScriptDescription : IParseable, IReadableTextWithKey, IDisposableExtended, IErrorCheckable, IComparable, INotifyPropertyChanged {

    #region Enums

    /// <summary>
    /// Empfänger-Typen des Skripts im binären Fragment-Format (Blatt-Werte unterhalb
    /// des Scripts-Pfades). Nur dem Skript selbst bekannt.
    /// </summary>
    public enum ScriptFragmentType : byte {

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
        ScriptName = 13,

        /// <summary>
        /// Segmenteintritt in die gespeicherten Variablen; Content = leerer Key,
        /// der Variablen-Block folgt eine Ebene tiefer.
        /// </summary>
        SubVariableCollection = 94
    }

    #endregion

    /// <summary>
    /// Verständlicher Name eines Empfänger-Typs im Skript-Pfad: Der Segmenteintritt
    /// (SubVariableCollection) leitet den Zahlraum der gespeicherten Variablen ein,
    /// sonst eigener Raum; unbekannte Werte liefern die Zahl.
    /// </summary>
    public static string GetNameOf(ReadOnlySpan<FragmentRecord> path) {
        if (path.Length == 1) { return ((ScriptFragmentType)path[0].LeafType).ToString(); }

        return path[0].LeafType == (byte)ScriptFragmentType.SubVariableCollection
            ? VariableCollection.GetNameOf(path[1..])
            : ((ScriptFragmentType)path[^1].LeafType).ToString();
    }

    #region Fields

    private volatile int _isDisposedFlag;

    /// <summary>
    /// Zähler der aktiven SuppressEvents-Aufrufe; Werte größer 0 unterdrücken FragmentChanged.
    /// </summary>
    private int _eventsSuppressCount;

    #endregion

    #region Constructors

    public ScriptDescription(string name, string script) : this(string.Empty, string.Empty, name, string.Empty, script, EmptyReadOnly, string.Empty, null) { }

    public ScriptDescription() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, EmptyReadOnly, string.Empty, null) { }

    protected ScriptDescription(string adminInfo, string image, string name, string quickInfo, string script, ReadOnlyCollection<string> userGroups, string failedReason, IEnumerable<ScriptVariable>? savedVariables) {
        if (string.IsNullOrEmpty(name)) {
            name = "New script";
        }

        AdminInfo = adminInfo;
        Image = image;
        KeyName = name;
        QuickInfo = quickInfo;
        Script = script;
        UserGroups = userGroups;
        FailedReason = failedReason;
        SavedVariables.ReplaceWith(savedVariables);
    }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~ScriptDescription() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler? Disposed;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Bubbling-Event für Fragment-Änderungen (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public event EventHandler<FragmentChangedEventArgs>? FragmentChanged;

    #endregion

    #region Properties

    public string AdminInfo {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
            OnFragmentChanged(new FragmentChangedEventArgs(this, ScriptFragmentType.AdminInfo, value));
        }
    }

    public string CompareKey => KeyName;

    /// <summary>
    /// Optionaler Delegate für die Test-Ausführung des Skripts aus einem
    /// Editor heraus. Wird vom Ersteller der ScriptDescription
    /// (z.B. einem PadItem) gesetzt, wenn das Skript mit spezifischen
    /// Eingangsvariablen und Methoden getestet werden soll.
    /// Parameter: Skripttext, Testmodus. Rückgabe: Feedback der Ausführung.
    /// Wird NICHT serialisiert — reine Laufzeit-Information.
    /// </summary>
    [JsonIgnore]
    public ExecuteScriptDelegate? ExecuteScript { get; set; }

    public string FailedReason {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
            OnFragmentChanged(new FragmentChangedEventArgs(this, ScriptFragmentType.FailedReason, value));
        }
    }

    public string Image {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
            OnFragmentChanged(new FragmentChangedEventArgs(this, ScriptFragmentType.Image, value));
        }
    }

    public bool IsDisposed => _isDisposedFlag == 1;

    /// <summary>
    /// Zeigt an, ob FragmentChanged-Events aktuell unterdrückt sind.
    /// </summary>
    public bool IsEventsSuppressed => _eventsSuppressCount > 0;

    public string KeyName {
        get;
        set {
            // Eine Namensänderung meldet die Sammlung als Entfernung + Aufnahme;
            // hier bewusst kein Fragment-Blatt, der Nav-Schlüssel muss stabil bleiben.
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public string QuickInfo {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
            OnFragmentChanged(new FragmentChangedEventArgs(this, ScriptFragmentType.QuickInfo, value));
        }
    }

    /// <summary>
    /// Die beim letzten Skriptlauf gespeicherten Variablen. Die Instanz ist
    /// fest mit dem Skript verbunden; der Inhalt wird per ReplaceWith
    /// ausgetauscht, damit bestehende Referenzen stets dasselbe Objekt sehen.
    /// </summary>
    public VariableCollection SavedVariables { get; } = [];

    public string Script {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
            OnFragmentChanged(new FragmentChangedEventArgs(this, ScriptFragmentType.Script, value));
        }
    }

    public ReadOnlyCollection<string> UserGroups {
        get;
        set {
            // Inhaltsvergleich statt Referenzvergleich: gleich große Listen
            // (z. B. beim Übernehmen frischer Instanzen) erzeugen kein Fragment-Blatt.
            if (!field.IsDifferentTo(value)) { return; }
            field = value;
            OnPropertyChanged();
            OnFragmentChanged(new FragmentChangedEventArgs(this, ScriptFragmentType.UserGroups, value));
        }
    }

    #endregion

    #region Methods

    public static bool IsValidName(string name) {
        if (name.IsFormat(BlueBasics.Classes.Formats.TextFormat.Instance, false) is { Length: > 0 }) { return false; }
        if (string.Equals(name, "New script", StringComparison.OrdinalIgnoreCase)) { return false; }
        return true;
    }

    public virtual List<string> Attributes() => [];

    public virtual int CompareTo(object? obj) {
        if (obj is ScriptDescription other) {
            return string.Compare(CompareKey, other.CompareKey, StringComparison.Ordinal);
        }

        return 0;
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual string ErrorReason() {
        if (!IsValidName(KeyName)) { return "Ungültiger Name"; }

        // Aktueller Syntax-Zustand per PreCheck — FailedReason ist nur das
        // Diagnose-Snapshot der letzten Live-Ausführung und blockiert OK nicht.
        var preCheck = ScriptPreCheck.Check(Script);
        if (preCheck.HasSyntaxErrors) { return string.Join("\r\n", preCheck.SyntaxErrors); }

        if (!string.IsNullOrEmpty(FailedReason)) { return "Das Skript enthält einen gespeicherten Syntax-Fehler."; }
        return string.Empty;
    }

    public virtual List<string> ParseableItems() {
        try {
            if (IsDisposed) { return []; }
            List<string> result = [];

            result.ParseableAdd("Name", KeyName.Trim());
            result.ParseableAdd("Script", Script.Replace("\r\n", "\r").TrimEnd(' '));
            result.ParseableAdd("QuickInfo", QuickInfo.Replace("\r\n", "\r").TrimEnd(' '));
            result.ParseableAdd("AdminInfo", AdminInfo.Replace("\r\n", "\r").TrimEnd(' '));
            result.ParseableAdd("Image", Image);
            result.ParseableAdd("UserGroups", UserGroups.SortedDistinctList(), false);
            result.ParseableAdd("FailedReason", FailedReason.Replace("\r\n", "\r").TrimEnd(' '));
            result.ParseableAdd("SavedVariables", SavedVariables.SortByKeyName().ToString(true));

            return result;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return ParseableItems();
        }
    }

    public virtual bool ParseFinished() => true;

    public virtual bool ParseThis(string key, string value) {
        switch (key) {
            case "name":
                KeyName = value.FromNonCritical();
                return true;

            case "script":

                Script = value.FromNonCritical();
                return true;

            case "manualexecutable":
                //if (value.FromPlusMinus()) {
                //    UserGroups.Add(Constants.Administrator);
                //    UserGroups = UserGroups.SortedDistinctList();
                //}

                return true;

            case "quickinfo":
                QuickInfo = value.FromNonCritical();
                return true;

            case "admininfo":
                AdminInfo = value.FromNonCritical();
                return true;

            case "image":
                Image = value.FromNonCritical();
                return true;

            case "failedreason":
                FailedReason = value.FromNonCritical();
                return true;

            case "savedvariables":
                SavedVariables.ReplaceWith(VariableCollection.ParseVariable(value.FromNonCritical(), true));
                return true;

            case "usergroups":
                UserGroups = value.FromNonCritical().SplitBy("|").SortedDistinctList().AsReadOnly();
                return true;

            case "changevalues": // Todo: 08.10.2024
                //_changeValues = value.FromPlusMinus();
                return true;
        }

        return false;
    }

    public string ReadableText() => KeyName;

    public virtual QuickImage? SymbolForReadableText() {
        if (!string.IsNullOrEmpty(Image)) {
            if (UserGroups.Count > 0) {
                return QuickImage.Get(Image + "|16");
            }

            return QuickImage.Get(Image + "|16|||||170");
        }

        return null;
    }

    public override string ToString() => ParseableItems().FinishParseable();

    protected virtual void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            OnDisposed();
            Disposed = null;
            PropertyChanged = null;
            FragmentChanged = null;
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Übernimmt alle Basis-Properties von <paramref name="other"/> in diese Instanz.
    /// Wird beim Recycling (siehe <c>Table.SetValueInternal</c>, case EventScript)
    /// verwendet: Statt eine neue Instanz zu erzeugen, bleiben vorhandene Objekte
    /// erhalten und nur ihre Felder werden aktualisiert — Referenzidentität bleibt gewahrt.
    /// </summary>
    protected void UpdateBaseFrom(ScriptDescription other) {
        KeyName = other.KeyName;
        Script = other.Script;
        QuickInfo = other.QuickInfo;
        AdminInfo = other.AdminInfo;
        Image = other.Image;
        UserGroups = other.UserGroups;
        FailedReason = other.FailedReason;
        SavedVariables.ReplaceWith(other.SavedVariables);
    }

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    #region IFragmentStoreable

    /// <summary>
    /// Unterdrückt FragmentChanged-Events des Skripts und seiner gespeicherten Variablen.
    /// </summary>
    public void SuppressEvents() {
        _eventsSuppressCount++;
        SavedVariables.SuppressEvents();
    }

    /// <summary>
    /// Hebt die Unterdrückung des Skripts und seiner gespeicherten Variablen wieder auf.
    /// </summary>
    public void ResumeEvents() {
        if (_eventsSuppressCount > 0) { _eventsSuppressCount--; }
        SavedVariables.ResumeEvents();
    }

    /// <summary>
    /// Löst das FragmentChanged-Event mit den fertigen Argumenten aus.
    /// </summary>
    public virtual void OnFragmentChanged(FragmentChangedEventArgs e) {
        if (IsDisposed || IsEventsSuppressed) { return; }

        FragmentChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Reicht eine Änderung der gespeicherten Variablen eine Ebene höher und stellt
    /// dem Blatt den eigenen Navigationssatz (SubVariableCollection, Segmenteintritt)
    /// voran. Nur das Blatt liefert seinen Blatt-Satz; diese Stufe feuert mit dem
    /// SubVariableCollection-Navigationssatz (leerer Key = Segmenteintritt) weiter.
    /// </summary>
    public void ChildFragmentChanged(object? sender, FragmentChangedEventArgs e) {
        if (IsDisposed || IsEventsSuppressed) { return; }

        OnFragmentChanged(e.WithNavigation(ScriptFragmentType.SubVariableCollection, string.Empty));
    }

    #endregion

    #endregion
}