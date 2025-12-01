// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueBasics.FileSystemCaching;
using BlueBasics.MultiUserFile;
using BlueTable;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

//https://stackoverflow.com/questions/9462592/best-practices-for-multi-form-applications-to-show-and-hide-forms
namespace BlueControls;

public class FormManager : ApplicationContext {

    #region Fields

    public static readonly List<Form> Forms = [];
    public static DExecuteAtEnd? ExecuteAtEnd;

    public static Type? FormBeforeEnd;

    //public static dNewModeSelectionForm? NewModeSelectionForm = null;
    private static FormManager? _current;

    private Form? _lastStartForm;

    #endregion

    #region Delegates

    public delegate void DExecuteAtEnd();

    #endregion

    #region Events

    public static event EventHandler<EventArgs.FormEventArgs>? FormAdded;

    public static event EventHandler<EventArgs.FormEventArgs>? FormRemoved;

    #endregion

    #region Properties

    public static bool Running { get; private set; }

    #endregion

    //public delegate Form? dNewModeSelectionForm();

    #region Methods

    public static void RegisterForm(Form frm) {
        if (_current == null) {
            Develop.DebugPrint(ErrorType.Error, "FormManager nicht gestartert!");
            return;
        }
        _current.RegisterFormInternal(frm);
    }

    public static void SaveEnd(Form? lastForm) {
        Generic.Ending = true;

        try {
            ExecuteAtEnd?.Invoke();
        } catch { }

        //Develop.DebugPrint(ErrorType.Info, "Schließe Programm...");
        //var p = BlueControls.Forms.Progressbar.Show("Beenden eingeleitet\r\nBitte warten, Daten werden gespeichert.");

        Table.SaveAll(false);
        MultiUserFile.SaveAll(false); // Sicherheitshalber, falls die Worker zu lange brauchen....

        Table.SaveAll(true);
        MultiUserFile.SaveAll(true); // Nun aber

        MultiUserFile.UnlockAllHard();

        List<Table> allTables = [.. Table.AllFiles];
        foreach (var thisTable in allTables) {
            try {
                if (lastForm is Forms.FormWithStatusBar fws) {
                    fws.UpdateStatus(ErrorType.Info, ImageCode.Tabelle, $"Entlade '{thisTable.Caption}'...", true);
                }
            } catch { }
            thisTable.UnMasterMe();
            thisTable.Freeze("Beenden...");
        }

        CachedFileSystem.DisposeAll();
    }

    //public static List<T> GetInstaceOfType<T>(params object[] constructorArgs) where T : class {
    //    List<T> l = new();
    //    foreach (var thisas in AppDomain.CurrentDomain.GetAssemblies()) {
    //        try {
    //            foreach (var thist in thisas.GetTypes()) {
    //                if (thist.IsClass && !thist.IsAbstract && thist.IsSubclassOf(typeof(T))) {
    //                    l.Add((T)Activator.CreateInstance(thist, constructorArgs));
    //                }
    //            }
    //        } catch { }
    //    }
    //    return l;
    //}
    public static FormManager Starter(Type startform, Type? lastWindow) {
        if (_current != null) { Develop.DebugPrint(ErrorType.Error, "Doppelter Start"); }

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
            fr.Show();
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
    private void OnFormClosed(object sender, System.EventArgs e) {
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
    private void OnFormClosing(object sender, System.EventArgs e) {
        if (sender is not Form thisForm) { return; }

        Forms.Remove(thisForm);

        FormRemoved?.Invoke(this, new EventArgs.FormEventArgs(thisForm));

        if (Forms.Count > 0) { return; }

        if (sender != _lastStartForm) {
            _lastStartForm = CreateForm(FormBeforeEnd, _current);
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

        FormAdded?.Invoke(this, new EventArgs.FormEventArgs(frm));
    }

    #endregion
}