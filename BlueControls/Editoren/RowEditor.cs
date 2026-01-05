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

using BlueBasics.Interfaces;
using BlueTable;
using BlueTable.Interfaces;

namespace BlueControls.Editoren;

public partial class RowEditor : EditorEasy, IHasTable {

    #region Constructors

    public RowEditor() => InitializeComponent();

    #endregion

    #region Properties

    public Table? Table => ToEdit is RowItem { IsDisposed: false } r ? r.Table : null;

    #endregion

    #region Methods

    public override void Clear() => formular.Page = null;

    protected override void InitializeComponentDefaultValues() { }

    protected override bool SetValuesToFormula(IEditable? toEdit) {
        RowItem? row = null;
        if (toEdit is RowItem r) { row = r; }

        formular.GetHeadPageFrom(row?.Table);
        formular.SetToRow(row);

        return true;
    }

    #endregion
}