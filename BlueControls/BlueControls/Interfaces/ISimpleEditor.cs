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

using BlueControls;
using BlueControls.Controls;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static System.Windows.Forms.Control;

namespace BlueBasics.Interfaces;

public interface ISimpleEditor {

    #region Properties

    public string Description { get; }

    #endregion

    #region Methods

    public List<GenericControl> GetProperties(int widthOfControl);

    #endregion
}

public static class SimpleEditorExtension {

    #region Methods

    public static void DoForm(ISimpleEditor? element, ControlCollection controls, int width) {

        #region  SideMenu leeren

        foreach (var thisControl in controls) {
            if (thisControl is IDisposable d) { d.Dispose(); }
        }
        controls.Clear();

        #endregion

        if (element is null) { return; }

        var stdWidth = width - (Skin.Padding * 4);

        var flexis = element.GetProperties(stdWidth);
        if (flexis.Count == 0) { return; }

        //Rückwärts inserten

        if (element is IErrorCheckable iec && !iec.IsOk()) {
            flexis.Insert(0, new FlexiControl("<Imagecode=Warnung|16> " + iec.ErrorReason(), stdWidth, false)); // Fehlergrund
            flexis.Insert(0, new FlexiControl("Achtung!", stdWidth, true));
        }

        if (!string.IsNullOrEmpty(element.Description)) {
            flexis.Insert(0, new FlexiControl(element.Description, stdWidth, false)); // Beschreibung
            flexis.Insert(0, new FlexiControl("Beschreibung:", stdWidth, true));
        }

        #region  SideMenu erstellen

        var top = Skin.Padding;
        foreach (var thisFlexi in flexis) {
            if (thisFlexi != null && !thisFlexi.IsDisposed) {
                controls.Add(thisFlexi);
                thisFlexi.Left = Skin.Padding;
                thisFlexi.Top = top;
                thisFlexi.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                top = top + Skin.Padding + thisFlexi.Height;
                thisFlexi.Width = stdWidth;
            }
        }

        #endregion
    }

    #endregion
}