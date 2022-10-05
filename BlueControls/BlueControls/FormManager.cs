using BlueBasics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

//https://stackoverflow.com/questions/9462592/best-practices-for-multi-form-applications-to-show-and-hide-forms
namespace BildzeichenListe {

    public class FormManager : ApplicationContext {

        #region Fields

        public static dExecuteAtEnd ExecuteAtEnd = null;
        public static bool First = true;
        public static bool FirstWindowShown = false;
        public static List<Form> forms = new();

        public static dNewModeSelectionForm NewModeSelectionForm = null;
        public static System.Windows.Forms.Form StartForm;

        //I'm using Lazy here, because an exception is thrown if any Forms have been
        //created before calling Application.SetCompatibleTextRenderingDefault(false)
        //in the Program class
        private static readonly Lazy<FormManager> _current = new();

        private System.Windows.Forms.Form _lastStartForm = null;

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

        public delegate System.Windows.Forms.Form? dNewModeSelectionForm();

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
            frm.FormClosed += onFormClosed;
            forms.Add(frm);
        }

        //When each form closes, close the application if no other open forms
        private void onFormClosed(object sender, EventArgs e) {
            forms.Remove((Form)sender);
            if (FirstWindowShown && !forms.Any()) {
                if (sender != _lastStartForm) {


                    if (NewModeSelectionForm != null) {
                        _lastStartForm = NewModeSelectionForm();
                    } else {
                        _lastStartForm = null;
                    }

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
}