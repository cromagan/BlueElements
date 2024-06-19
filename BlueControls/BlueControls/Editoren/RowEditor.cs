﻿// Authors:
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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System.Windows.Controls;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Editoren;

public partial class RowEditor : EditorEasy, IHasDatabase {

    #region Constructors

    public RowEditor() {
        InitializeComponent();
    }

    #endregion

    #region Properties

    public Database? Database {
        get {
            if (ToEdit is not RowItem r || r.IsDisposed) { return null; }
            return r.Database;
        }
    }

    #endregion

    #region Methods

    public override void Clear() => formular.InitFormula(null, Database);

    public override bool Init(IEditable? toEdit) {
        RowItem? row = null;
        if (ToEdit is RowItem r) { row = r; }

        formular.InitFormula(null, Database);

        if (row != null && !row.IsDisposed) {
            formular.GetConnectedFormulaFromDatabase(Database);

            formular.SetToRow(row);
        }

        return true;
    }

    protected override void InitializeComponentDefaultValues() {
        //RowItem? row = null;
        //if(ToEdit is RowItem r) { row = r; }

        //formular.InitFormula(null, row?.Database);

        //if (row != null && !row.IsDisposed) {
        //    formular.GetConnectedFormulaFromDatabase(row.Database);

        //    formular.SetToRow(row);
        //}
    }

    #endregion
}