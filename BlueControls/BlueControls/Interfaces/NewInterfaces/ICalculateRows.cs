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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static BlueControls.Interfaces.HasVersionExtensions;

namespace BlueControls.Interfaces;

public interface ICalculateRows {

    #region Properties

    public List<RowItem> FilteredRows { get; }

    #endregion

    #region Methods

    public void Invalidate_FilteredRows();

    #endregion
}

public static class ICalculateRowsExtension {

    #region Methods

    public static List<RowItem> CalculateFilteredRows(this ICalculateRows item, ref List<RowItem>? precalculaeItems, FilterCollection? filter, DatabaseAbstract? database) {
        if (precalculaeItems != null) { return precalculaeItems; }
        if (database == null || database.IsDisposed) { return new List<RowItem>(); }
        precalculaeItems = database.Row.CalculateFilteredRows(filter);
        return precalculaeItems;
    }

    #endregion
}

public class CalculateRows {
}