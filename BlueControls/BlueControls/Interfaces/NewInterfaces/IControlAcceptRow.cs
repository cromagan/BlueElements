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

using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueDatabase;

namespace BlueControls.Interfaces;

public interface IControlAcceptRow : IControlAcceptSomething, IDisposableExtended {

    #region Properties

    public IControlSendRow? GetRowFrom { get; set; }

    public RowItem? LastInputRow { get; }

    #endregion

    #region Methods

    public void SetData(DatabaseAbstract? database, long? rowkey);

    #endregion
}

public static class IControlAcceptRowExtension {

    #region Methods

    public static void DoInputSettings(this IControlAcceptRow dest, ConnectedFormulaView parent, IItemAcceptRow source) {
        dest.Name = source.DefaultItemToControlName();

        if (source.GetRowFrom != null) {
            var it = source.Parent?[source.GetRowFrom.KeyName];

            if (it is IItemToControl itc) {
                var ff = parent.SearchOrGenerate(itc);

                if (ff is IControlSendRow ffx) {
                    dest.GetRowFrom = ffx;
                }
            }
        }

        //if (GetRowFrom is ICalculateRowsItemLevel rfw2) {
        //    var ff = parent.SearchOrGenerate(rfw2);
        //    if (ff is ICalculateRowsControlLevel cc) { cc.ChildAdd(con); }
        //}

        //if (ff is ICalculateRowsControlLevel cc) { cc.ChildAdd(con); }

        //if (GetRowFrom is ICalculateRowsItemLevel rfw2) {
        //    var ff = parent.SearchOrGenerate(rfw2);
        //    if (ff is ICalculateRowsControlLevel cc) { cc.ChildAdd(con); }
        //}

        //if (GetRowFrom is ICalculateRowsItemLevel rfw2) {
        //    var ff = parent.SearchOrGenerate(rfw2);

        //    if (ff is ICalculateRowsControlLevel cc) { cc.ChildAdd(con); }
        //    con.DisabledReason = "Dieser Wert ist nur eine Anzeige.";
        //} else {
        //    con.DisabledReason = "Keine gültige Verknüpfung";
        //}
    }

    public static DatabaseAbstract? InputDatabase(this IControlAcceptRow item) {
        if (item.GetRowFrom == null || item.GetRowFrom.IsDisposed) { return null; }
        return item.GetRowFrom.OutputDatabase;
    }

    #endregion
}

public class ControlAcceptRow : ControlAcceptSomething {
}