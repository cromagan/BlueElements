// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlueBasics;

//https://stackoverflow.com/questions/9462592/best-practices-for-multi-form-applications-to-show-and-hide-forms
namespace BlueControls;

public class FormManager : ApplicationContext {

    #region Fields

    public static Type? _lastWindow;
    public static dExecuteAtEnd? ExecuteAtEnd = null;

    //public static dNewModeSelectionForm? NewModeSelectionForm = null;

    private static readonly List<Form> _forms = new();

    private static FormManager? _current;

    private Form? _lastStartForm;

    #endregion

    #region Delegates

    public delegate void dExecuteAtEnd();

    #endregion

    #region Properties

    public static bool Running { get; private set; }

    #endregion

    //public delegate Form? dNewModeSelectionForm();

    #region Methods

    public static void RegisterForm(Form frm) {
        if (_current == null) {
            Develop.DebugPrint(BlueBasics.Enums.FehlerArt.Fehler, "FormManager nicht gestartert!");
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
        if (_current != null) { Develop.DebugPrint(BlueBasics.Enums.FehlerArt.Fehler, "Doppelter Start"); }

        var tmp = new FormManager(); // temporär! Weil ansonsten startet true gilt und bei initialisieren der Fenster unerwartete Effekte auftreten können
        _lastWindow = lastWindow;
        Running = true;
        tmp._lastStartForm = CreateForm(startform, tmp);
        _current = tmp;
        return _current;
    }

    //Any form which might be the last open form in the application should be created with this
    public T CreateForm<T>() where T : Form, new() {
        var ret = new T();
        RegisterForm(ret);
        return ret;
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
    private void onFormClosed(object sender, System.EventArgs e) {
        _ = _forms.Remove((Form)sender);
        if (!_forms.Any()) {
            if (sender != _lastStartForm) {
                _lastStartForm = CreateForm(_lastWindow, _current);
                if (_lastStartForm != null) { return; }
            }

            Running = false;
            ExecuteAtEnd?.Invoke();

            var a = BlueDatabase.Database.AllFiles.Clone();
            foreach (var thisDb in a) {
                thisDb.Dispose();
            }

            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false);
            BlueBasics.MultiUserFile.MultiUserFile.SaveAll(true);

            ExitThread();
            Develop.AbortExe();
        }
    }

    private void RegisterFormInternal(Form frm) {
        if (_forms.Contains(frm)) { return; }

        frm.FormClosed += onFormClosed;
        _forms.Add(frm);
    }

    #endregion
}