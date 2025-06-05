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

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Editoren;
using BlueControls.Interfaces;
using System;
using System.Windows.Forms;

namespace BlueControls.Forms;

public partial class InputBoxEditor : DialogWithOkAndCancel {

    #region Constructors

    private InputBoxEditor() : this(null, false) { }

    private InputBoxEditor(Control? centerControl, bool supportsCancel) : base(supportsCancel, true) {
        InitializeComponent();

        //if (toEdit == null) { return; }
        if (centerControl == null) { return; }

        Controls.Add(centerControl);

        Setup(string.Empty, centerControl, centerControl.Width + (Skin.Padding * 2));
    }

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="isDialog"></param>
    public static void Show(ISimpleEditor? toEdit, bool isDialog) {
        if (toEdit is null or IDisposableExtended { IsDisposed: true }) { return; }

        var mb = new InputBoxEditor(toEdit.GetControl(400), false);

        if (isDialog) {
            _ = mb.ShowDialog();
            mb.Dispose();
        } else {
            mb.Show();
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="isDialog"></param>
    /// <param name="supportsCancel"></param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Show(IEditable? toEdit, bool isDialog, bool supportsCancel) => Show(toEdit, toEdit?.Editor, isDialog, supportsCancel);

    /// <summary>
    ///
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="editortype"></param>
    /// <param name="supportsCancel"></param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Show(IEditable? toEdit, Type? editortype, bool supportsCancel) => Show(toEdit, editortype, true, supportsCancel);

    /// <summary>
    ///
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="editortype"></param>
    /// <param name="isDialog"></param>
    /// <param name="supportsCancel"></param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Show(IEditable? toEdit, Type? editortype, bool isDialog, bool supportsCancel) {
        if (editortype == null) { return false; }

        if (toEdit is null or IDisposableExtended { IsDisposed: true }) { return false; }

        var m = toEdit.IsNowEditable();
        if (!string.IsNullOrEmpty(m)) {
            MessageBox.Show($"<b>Bearbeitung aktuell nicht möglich:</b><br>{m}", ImageCode.Information, "Ok");
            return false;
        }

        if (!isDialog) { supportsCancel = false; }

        toEdit.Editor = editortype;

        Form? mb = null;

        try {
            var myObject = Activator.CreateInstance(toEdit.Editor);

            if (myObject is IIsEditor ie) {
                ie.ToEdit = toEdit;
                if (ie is EditorEasy ea) {
                    ea.ToEdit = toEdit;
                    mb = new InputBoxEditor(ea, supportsCancel);
                } else if (ie is Form frm) {
                    mb = frm;
                }
            }
        } catch { }

        if (mb == null) { return false; }

        var ok = true;

        if (isDialog) {
            _ = mb.ShowDialog();

            if (toEdit is IErrorCheckable iec && !iec.IsOk()) { ok = false; }

            if (mb is DialogWithOkAndCancel { Canceled: true }) { ok = false; }

            mb.Dispose();
            return ok;
        }

        mb.Show();
        return true;
    }

    protected override bool SetValue() {
        if (Canceled) { return true; }

        foreach (var thisc in Controls) {
            if (thisc is EditorEasy { ToEdit: IErrorCheckable ec } ee) {
                if (ec.IsOk()) { return true; }

                var b = MessageBox.Show($"<b><u>{ee.ToEdit.CaptionForEditor} enthält noch Fehler:</u></b>\r\n\r\n{ec.ErrorReason()}\r\n\r\nMöchten sie diese beheben?", ImageCode.Warnung, "Beheben", "Verwerfen");

                if (b == 0) { return false; }
                Canceled = true;
            }
        }

        return true;
    }

    #endregion
}

public static class InputBoxEditorExtension {

    #region Methods

    /// <summary>
    /// Routine für allgemeine Elemente, wenn nicht bekannt ist, welcher Form zuständig ist
    /// </summary>
    /// <param name="toEdit"></param>
    public static void Edit(this IEditable? toEdit) {
        if (toEdit == null) { return; }

        _ = InputBoxEditor.Show(toEdit, true, false);
    }

    /// <summary>
    /// Routine für allgemeine Elemente, wenn nicht bekannt ist, welcher Form zuständig ist
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="isDialog"></param>
    /// <param name="supportsCancel"></param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Edit(this IEditable? toEdit, bool isDialog, bool supportsCancel) => toEdit != null && InputBoxEditor.Show(toEdit, isDialog, supportsCancel);

    /// <summary>
    /// Erweitert, setzt auch gleich den Bearbeitungs-Modus.
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="type"></param>
    ///     /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Edit(this IEditable? toEdit, Type? type) => toEdit != null && type != null && InputBoxEditor.Show(toEdit, type, true);

    /// <summary>
    /// Erweitert, setzt auch gleich den Bearbeitungs-Modus.
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="type"></param>
    /// <param name="isDialog"></param>
    public static void Edit(this IEditable? toEdit, Type? type, bool isDialog) {
        if (toEdit == null || type == null) { return; }

        _ = InputBoxEditor.Show(toEdit, type, isDialog);
    }

    #endregion
}