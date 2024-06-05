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

    private InputBoxEditor(IEditable? toEdit) : this(toEdit, toEdit?.Editor) { }

    private InputBoxEditor(IEditable? toEdit, Type? type) : base(false, true) {
        InitializeComponent();

        if (toEdit == null) { return; }
        if (type == null) { return; }

        toEdit.Editor = type;

        try {
            object myObject = Activator.CreateInstance(toEdit.Editor);

            if (myObject is EditorAbstract ea) {
                ea.ToEdit = toEdit;

         

                Controls.Add(ea);

                Setup(string.Empty, ea, ea.Width + Skin.Padding * 2);
            }
        } catch { }
    }

    #endregion

    #region Methods

    public static void Show(IEditable? toEdit, bool isDialog) => Show(toEdit, toEdit?.Editor, isDialog);

    public static void Show(IEditable? toEdit, Type? type) => Show(toEdit, type, true);

    public static void Show(IEditable? toEdit, Type? type, bool isDialog) {
        InputBoxEditor mb = new(toEdit, type);
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