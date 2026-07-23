// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BlueScript.Classes;

public class ScriptDescription : IParseable, IReadableTextWithKey, IDisposableExtended, IErrorCheckable, IComparable, INotifyPropertyChanged {

    #region Fields

    private volatile int _isDisposedFlag;

    #endregion


    #region Constructors

    public ScriptDescription(string name, string script) : this(string.Empty, string.Empty, name, string.Empty, script, EmptyReadOnly, string.Empty, null) { }

    public ScriptDescription() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, EmptyReadOnly, string.Empty, null) { }

    protected ScriptDescription(string adminInfo, string image, string name, string quickInfo, string script, ReadOnlyCollection<string> userGroups, string failedReason, List<Variable>? savedVariables) {
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
        SavedVariables = savedVariables;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public string AdminInfo {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public string CompareKey => KeyName;

    public string FailedReason {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public string Image {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsDisposed => _isDisposedFlag == 1;

    public string KeyName {
        get;
        set {
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
        }
    }

    public List<Variable>? SavedVariables {
        get;
        set {
            if (field?.SortByKeyName().ToString(true) == value?.SortByKeyName().ToString(true)) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public string Script {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public ReadOnlyCollection<string> UserGroups {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public static bool IsValidName(string name) {
        if (!name.IsFormat(FormatHolder_Text.Instance)) { return false; }
        if (string.Equals(name, "New script", StringComparison.OrdinalIgnoreCase)) { return false; }
        return true;
    }

    public virtual List<string> Attributes() => [];

    public virtual int CompareTo(object obj) {
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
        if (!string.IsNullOrEmpty(FailedReason)) { return "Das Skript enthält Syntax-Fehler."; }
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
            result.ParseableAdd("SavedVariables", SavedVariables?.SortByKeyName().ToString(true) ?? string.Empty);

            return result;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return ParseableItems();
        }
    }

    public virtual void ParseFinished(string parsed) { }

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
                SavedVariables = VariableCollection.ParseVariable(value.FromNonCritical(), true);
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
            PropertyChanged = null;
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
        SavedVariables = other.SavedVariables;
    }

    #endregion
}