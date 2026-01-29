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
using BlueControls.Controls;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn eine Klasse bearbeitbar ist, es sie selbst die Properties ausgeben kann.
/// Ein allgemeiner Dialog, eingeschränkte Design-Möglichkeiten.
/// Mit der Extension kann mit DoForm die Properties in ein Steuerelement generiert werden.
/// </summary>
public interface ISimpleEditor {

    #region Events

    event EventHandler? DoUpdateSideOptionMenu;

    #endregion

    #region Properties

    string Description { get; }

    #endregion

    #region Methods

    List<GenericControl> GetProperties(int widthOfControl);

    #endregion
}

public static class SimpleEditorExtension {

    #region Methods

    public static void DoForm(this ISimpleEditor? element, Control control) {

        #region  SideMenu leeren

        foreach (var thisControl in control.Controls) {
            if (thisControl is IDisposable d) { d.Dispose(); }
        }
        control.Controls.Clear();

        #endregion

        if (element is null) { return; }

        //var stdWidth = width - (Skin.Padding * 4);

        var flexis = element.GetProperties(control.Width);
        if (flexis.Count == 0) { return; }

        //Rückwärts inserten

        if (element is IErrorCheckable iec && !iec.IsOk()) {
            flexis.Insert(0, new FlexiControl("<Imagecode=Warnung|16> " + iec.ErrorReason(), control.Width, false)); // Fehlergrund
            flexis.Insert(0, new FlexiControl("Achtung!", control.Width, true));
        }

        if (!string.IsNullOrEmpty(element.Description)) {
            flexis.Insert(0, new FlexiControl(element.Description, control.Width, false)); // Beschreibung
            flexis.Insert(0, new FlexiControl("Beschreibung:", control.Width, true));
        }

        #region  SideMenu erstellen

        var top = Skin.Padding;
        foreach (var thisFlexi in flexis) {
            if (thisFlexi is { IsDisposed: false }) {
                control.Controls.Add(thisFlexi);
                thisFlexi.Left = Skin.Padding;
                thisFlexi.Top = top;
                thisFlexi.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                top = top + Skin.Padding + thisFlexi.Height;
                thisFlexi.Width = control.Width - Skin.Padding * 2;
            }
        }

        #endregion
    }

    public static UserControl GetControl(this ISimpleEditor? element, int widthOfControl) {
        var l = new UserControl {
            Width = widthOfControl,
            Height = 100,
            Visible = true
        };

        element.DoForm(l);

        foreach (var control in l.Controls) {
            if (control is Control c) {
                l.Height = Math.Max(l.Height, c.Bottom);
            }
        }
        return l;
    }

    #endregion
}