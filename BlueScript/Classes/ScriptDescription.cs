// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using static BlueBasics.ClassesStatic.Constants;

namespace BlueScript.Classes;

public abstract class ScriptDescription : IParseable, IReadableTextWithKey, IDisposableExtended, IErrorCheckable, IComparable, INotifyPropertyChanged {

    #region Fields

    private volatile int _isDisposedFlag;

    #endregion

    #region Constructors

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

    protected ScriptDescription(string name, string script) : this(string.Empty, string.Empty, name, string.Empty, script, EmptyReadOnly, string.Empty, null) { }

    protected ScriptDescription() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, EmptyReadOnly, string.Empty, null) { }

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
        private set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public string CompareKey => KeyName;

    public string FailedReason {
        get;
        private set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public string Image {
        get;
        private set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsDisposed => _isDisposedFlag == 1;

    public string KeyName {
        get;
        private set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public string QuickInfo {
        get;
        private set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public List<Variable>? SavedVariables {
        get;
        private set {
            if (field?.ToString(true) == value?.ToString(true)) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public string Script {
        get;
        private set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public ReadOnlyCollection<string> UserGroups {
        get;
        private set {
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

    public abstract List<string> Attributes();

    public abstract int CompareTo(object obj);

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
            result.ParseableAdd("UserGroups", UserGroups, false);
            result.ParseableAdd("FailedReason", FailedReason.Replace("\r\n", "\r").TrimEnd(' '));
            result.ParseableAdd("SavedVariables", SavedVariables?.ToString(true) ?? string.Empty);

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

    #endregion
}