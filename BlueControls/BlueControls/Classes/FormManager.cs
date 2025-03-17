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

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.MultiUserFile;
using BlueDatabase;

//https://stackoverflow.com/questions/9462592/best-practices-for-multi-form-applications-to-show-and-hide-forms
namespace BlueControls;

public class FormManager : ApplicationContext {

    #region Fields

    public static readonly List<Form> Forms = [];
    public static DExecuteAtEnd? ExecuteAtEnd = null;

    //public static dNewModeSelectionForm? NewModeSelectionForm = null;
    private static FormManager? _current;

    private static Type? _lastWindow;
    private Form? _lastStartForm;

    #endregion

    #region Delegates

    public delegate void DExecuteAtEnd();

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
        _lastWindow = lastWindow;
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

    //When each form closes, close the application if no other open forms
    private void OnFormClosed(object sender, System.EventArgs e) {
        _ = Forms.Remove((Form)sender);
        if (!Forms.Any()) {
            if (sender != _lastStartForm) {
                _lastStartForm = CreateForm(_lastWindow, _current);
                if (_lastStartForm != null) { return; }
            }

            Running = false;
            ExecuteAtEnd?.Invoke();

            Database.ForceSaveAll();
            MultiUserFile.SaveAll(true);
            MultiUserFile.UnlockAllHard();

            ExitThread();
            Develop.AbortExe();
        }
    }

    private void RegisterFormInternal(Form frm) {
        if (Forms.Contains(frm)) { return; }

        frm.FormClosed += OnFormClosed;
        Forms.Add(frm);
    }

    #endregion
}