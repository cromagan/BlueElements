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
using System.Windows.Forms;
using BlueBasics;

//https://stackoverflow.com/questions/9462592/best-practices-for-multi-form-applications-to-show-and-hide-forms
namespace BlueControls;

public class FormManager : ApplicationContext {

    #region Fields

    public static dExecuteAtEnd? ExecuteAtEnd = null;
    public static bool First = true;
    public static bool FirstWindowShown;
    public static List<Form> Forms = new();

    public static dNewModeSelectionForm? NewModeSelectionForm = null;
    public static Form? StartForm;

    //I'm using Lazy here, because an exception is thrown if any Forms have been
    //created before calling Application.SetCompatibleTextRenderingDefault(false)
    //in the Program class
    private static readonly Lazy<FormManager> _current = new();

    private Form? _lastStartForm;

    #endregion

    #region Constructors

    //Startup forms should be created and shown in the constructor
    public FormManager() {
        if (!First || StartForm == null) { return; }
        First = false;

        _lastStartForm = StartForm;
        StartForm.Show();
        RegisterForm(StartForm);
        StartForm.BringToFront();

        StartForm = null;

        FirstWindowShown = true;
    }

    #endregion

    #region Delegates

    public delegate void dExecuteAtEnd();

    public delegate Form? dNewModeSelectionForm();

    #endregion

    #region Properties

    public static FormManager Current => _current.Value;

    #endregion

    #region Methods

    //Any form which might be the last open form in the application should be created with this
    public T CreateForm<T>() where T : Form, new() {
        var ret = new T();
        RegisterForm(ret);
        return ret;
    }

    public void RegisterForm(Form frm) {
        if (Forms.Contains(frm)) { return; }

        frm.FormClosed += onFormClosed;
        Forms.Add(frm);
    }

    //When each form closes, close the application if no other open forms
    private void onFormClosed(object sender, System.EventArgs e) {
        _ = Forms.Remove((Form)sender);
        if (FirstWindowShown && !Forms.Any()) {
            if (sender != _lastStartForm) {
                // Delegate muss auf null geprüt werden!
                _lastStartForm = NewModeSelectionForm?.Invoke();

                if (_lastStartForm != null) {
                    Current.RegisterForm(_lastStartForm);
                    _lastStartForm.Show();
                    return;
                }
            }

            ExecuteAtEnd();
            ExitThread();
            Develop.AbortExe();
        }
    }

    #endregion
}