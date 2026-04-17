// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics.Interfaces;
using BlueControls.Classes;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn ein einzigartiges Fenster eines Objektes angezeigt werden soll.
/// Typischerweiße bei einem Editor.
/// Wichtig! Die Form muss einen Parameterlosen Konstruktor haben.
/// Wenn Object Null ist, ist Systemweit nur ein Fenster möglich.
/// Aufrufbeispiel: IUniqueWindowExtension.ShowOrCreate&lt;TableScriptEditor&gt;(tb);
/// </summary>
public interface IUniqueWindow : IDisposableExtended {

    #region Properties

    /// <summary>
    /// Wenn Object Null ist, ist Systemweit nur ein Fenster möglich.
    /// </summary>
    object? Object { get; set; }

    #endregion

    #region Methods

    void BringToFront();

    void Show();

    #endregion
}

public static class IUniqueWindowExtension {
    //public void ShowOrCreate<T>(T window, object o ) where T : IUniqueWindow {
    //    foreach(var thisT in FormManager.Forms) {
    //        if (thisT is typeof(window){ IsDisposed: false } form) {
    //            if(o == form.Object) {
    //                form.SendToBack();
    //                return;
    //            }

    //        }

    //    }

    //}

    //public void ShowOrCreate<T>(T window, object o) where T : IUniqueWindow {
    //    foreach (var form in FormManager.Forms.OfType<T>()) {
    //        if (!form.IsDisposed && form.Object.Equals(o)) {
    //            form.SendToBack();
    //            return;
    //        }
    //    }

    //    // Wenn kein passendes Fenster gefunden wurde, erstellen Sie ein neues
    //    T newWindow = (T)Activator.CreateInstance(typeof(T), o);
    //    newWindow.Show();
    //}

    //public static void ShowOrCreate<T>(Type windowType, object o) where T : IUniqueWindow {
    //    foreach (var form in FormManager.Forms) {
    //        if (form.GetType() == windowType && form is T typedForm && !typedForm.IsDisposed) {
    //            if (typedForm.Object.Equals(o)) {
    //                typedForm.SendToBack();
    //                return;
    //            }
    //        }
    //    }

    //    // Wenn kein passendes Fenster gefunden wurde, erstellen Sie ein neues
    //    if (Activator.CreateInstance(windowType) is T newWindow) {
    //        newWindow.Object = o;
    //        newWindow.Show();
    //    }
    //}

    #region Methods

    public static T ShowOrCreate<T>(object? o) where T : System.Windows.Forms.Form, IUniqueWindow, new() {
        var windowType = typeof(T);

        foreach (var form in FormManager.Forms) {
            if (form.GetType() == windowType && form is T { IsDisposed: false } uniqueWindow) {
                if (uniqueWindow.Object == o) {
                    uniqueWindow.BringToFront();
                    return uniqueWindow;
                }
            }
        }

        // Wenn kein passendes Fenster gefunden wurde, erstellen Sie ein neues
        var newWindow = new T {
            Object = o
        };
        FormManager.RegisterForm(newWindow);
        newWindow.Show();
        return newWindow;
    }

    #endregion
}