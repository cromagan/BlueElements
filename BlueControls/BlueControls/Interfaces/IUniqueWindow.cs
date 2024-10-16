// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueBasics.Interfaces;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das Steuerelement sich selbst zeichnet und nicht als Container vorgesehen ist.
/// </summary>
public interface IUniqueWindow : IDisposableExtended {

    #region Properties

    public object? Object { get; set; }

    #endregion

    #region Methods

    public void BringToFront();

    public void Show();

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

    public static T ShowOrCreate<T>(object o) where T : IUniqueWindow, new() {
        Type windowType = typeof(T);

        foreach (var form in FormManager.Forms) {
            if (form.GetType() == windowType && form is T { IsDisposed: false } uniqueWindow) {
                if (uniqueWindow.Object.Equals(o)) {
                    uniqueWindow.BringToFront();
                    return uniqueWindow;
                }
            }
        }

        // Wenn kein passendes Fenster gefunden wurde, erstellen Sie ein neues
        T newWindow = new T();
        newWindow.Object = o;
        newWindow.Show();
        return newWindow;
    }

    #endregion
}