// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using BlueBasics;
using BlueControls.Interfaces;
using BlueDatabase;

namespace BlueControls.Controls;

internal class ConnectedFormulaFilterButton : Button, IControlAcceptFilter, ICalculateRows {

    #region Properties

    public List<IControlSendFilter> GetFilterFrom { get; } = new();

    public DatabaseAbstract? InputDatabase { get; set; }

    /// <summary>
    /// Wird benötigt, um zu wissen, ob auf welche Zeilen das Script angewendet werden soll.
    /// </summary>
    public List<RowItem> RowsFiltered => this.RowsFiltered(ref _filteredRows, this.FilterOfSender(), InputDatabase);

    #endregion

    #region Methods
    private List<RowItem>? _filteredRows;
    public void FilterFromParentsChanged() => Invalidate_FilteredRows();

    public void Invalidate_FilteredRows() {
        if (IsDisposed) { return; }
        _filteredRows = null;
        Invalidate();
    }

    #endregion
}