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

using BlueBasics.Interfaces;
using BlueControls.Editoren;
using BlueControls.Interfaces;
using BlueDatabase;
using System;

namespace BlueControls.Forms;

public partial class InputBoxEditor : DialogWithOkAndCancel {

    #region Constructors

    private InputBoxEditor() : this(null, null) { }

    private InputBoxEditor(IEditable? toEdit, EditorEasy? ea) : base(false, true) {
        InitializeComponent();

        if (toEdit == null) { return; }
        if (ea == null) { return; }

        Controls.Add(ea);

        Setup(string.Empty, ea, ea.Width + (Skin.Padding * 2));
    }

    #endregion

    #region Methods

    public static void Show(IEditable? toEdit, bool isDialog) => Show(toEdit, toEdit?.Editor, isDialog);

    public static void Show(IEditable? toEdit, Type? type) => Show(toEdit, type, true);

    public static void Show(IEditable? toEdit, Type? type, bool isDialog) {
        if (type == null && toEdit is ISimpleEditor) { type = typeof(EditorEasy); }

        if (toEdit == null || type == null) { return; }

        if (type == null && toEdit is not ISimpleEditor) { return; }

        if (toEdit is IDisposableExtended id && id.IsDisposed) { return; }

        toEdit.Editor = type;

        Form? mb = null;

        try {
            object myObject = Activator.CreateInstance(toEdit.Editor);

            if (myObject is IIsEditor ie) {
                ie.ToEdit = toEdit;
                if (ie is EditorEasy ea) {
                    mb = new InputBoxEditor(toEdit, ea);
                } else if (ie is Form frm) {
                    mb = frm;
                }
            }
        } catch { }

        if (mb == null) { return; }

        if (isDialog) {
            _ = mb.ShowDialog();
            mb.Dispose();
        } else {
            mb.Show();
        }
    }

    protected override void SetValue(bool canceled) { }

    #endregion

    // Nix zu tun
}

public static class InputBoxEditorExtension {

    #region Methods

    /// <summary>
    /// Routine für allgemeine Elemente, wenn nicht bekannt ist, welcher Form zuständig ist
    /// </summary>
    /// <param name="toEdit"></param>
    public static void Edit(this IEditable? toEdit) {
        if (toEdit == null) { return; }

        InputBoxEditor.Show(toEdit, true);
    }

    /// <summary>
    /// Erweitert, setzt auch gleich den Bearbeitungs-Modus.
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="type"></param>
    public static void Edit(this IEditable? toEdit, Type? type) {
        if (toEdit == null || type == null) { return; }

        InputBoxEditor.Show(toEdit, type, true);
    }

    /// <summary>
    /// Erweitert, setzt auch gleich den Bearbeitungs-Modus.
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="type"></param>
    public static void Edit(this IEditable? toEdit, Type? type, bool isDialog) {
        if (toEdit == null || type == null) { return; }

        InputBoxEditor.Show(toEdit, type, isDialog);
    }

    #endregion
}