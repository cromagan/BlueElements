// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes.FileSystemCaching;

namespace BlueControls.Classes;

public class FormManager : System.Windows.Forms.ApplicationContext {

    #region Fields

    public static readonly List<Form> Forms = [];

    //public static dNewModeSelectionForm? NewModeSelectionForm = null;
    private static FormManager? _current;

    private Form? _lastStartForm;

    #endregion

    #region Events

    public static event EventHandler<EventArgs.FormEventArgs>? FormAdded;

    public static event EventHandler<EventArgs.FormEventArgs>? FormRemoved;

    #endregion

    #region Properties

    public static Type? FormBeforeEnd { get; set; }
    public static bool Running { get; private set; }

    #endregion

    //public delegate Form? dNewModeSelectionForm();

    #region Methods

    public static Form? OpenLastMenu() => CreateForm(FormBeforeEnd, _current);

    public static void RegisterForm(Form frm) {
        if (_current == null) {
            Develop.DebugError("FormManager nicht gestartert!");
            return;
        }
        _current.RegisterFormInternal(frm);
    }

    public static void SaveEnd(Form? lastForm) {
        Generic.Ending = true;

        CachedFileSystem.SaveAll(false); // Sicherheitshalber, falls die Worker zu lange brauchen....

        Table.SaveAll();
        CachedFileSystem.SaveAll(true); // Nun aber

        IMultiUserCapable.RevokeWriteAccessAll();

        List<Table> allTables = [.. Table.AllFiles];
        foreach (var thisTable in allTables) {
            try {
                if (lastForm is Forms.FormWithStatusBar fws && !string.IsNullOrEmpty(thisTable.Caption)) {
                    fws.UpdateStatus(ErrorType.Info, ImageCode.Tabelle, $"Entlade '{thisTable.Caption}'...", true);
                }
            } catch { }
            thisTable.UnMasterMe();
            thisTable.Freeze("Beenden...");
        }

        CachedFileSystem.DisposeAll();
    }

    public static FormManager Starter(Type startform, Type? lastWindow) {
        if (_current != null) { throw Develop.DebugError("Doppelter Start"); }

        var tmp = new FormManager(); // temporär! Weil ansonsten startet true gilt und bei initialisieren der Fenster unerwartete Effekte auftreten können
        FormBeforeEnd = lastWindow;
        Running = true;
        tmp._lastStartForm = CreateForm(startform, tmp);
        _current = tmp;
        return _current;
    }

    private static Form? CreateForm(Type? frm, FormManager? fm) {
        if (fm == null || frm == null) { return null; }

        var f = Activator.CreateInstance(frm);

        if (f is Form fr) {
            ((System.Windows.Forms.Form)fr).Show();
            fr.BringToFront();

            fm.RegisterFormInternal(fr);
            return fr;
        }

        return null;
    }

    /// <summary>
    /// When each form closes, close the application if no other open forms
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnFormClosed(object? sender, System.EventArgs e) {
        if (Forms.Count > 0) { return; }

        Develop.TraceLogging_End();

        ExitThread();
        Develop.AbortExe(true);
    }

    /// <summary>
    /// When each form closes, close the application if no other open forms
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnFormClosing(object? sender, System.EventArgs e) {
        if (sender is not Form thisForm) { return; }

        Forms.Remove(thisForm);

        FormRemoved?.Invoke(null, new EventArgs.FormEventArgs(thisForm));

        if (Forms.Count > 0) { return; }

        if (sender != _lastStartForm) {
            _lastStartForm = OpenLastMenu();
            if (_lastStartForm != null) { return; }
        }

        thisForm.Enabled = false;
        thisForm.Refresh();

        SaveEnd(thisForm);
    }

    private void RegisterFormInternal(Form frm) {
        if (Forms.Contains(frm)) { return; }

        frm.FormClosing += OnFormClosing;
        frm.FormClosed += OnFormClosed;
        Forms.Add(frm);

        FormAdded?.Invoke(null, new EventArgs.FormEventArgs(frm));
    }

    #endregion
}