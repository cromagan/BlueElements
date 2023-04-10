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

using BlueControls.Controls;

namespace BlueControls.Interfaces;

public interface IControlAcceptRow : IControlAcceptSomething {
    //public IControlSendRow GetRowFrom { get; set; }
}

public static class IControlAcceptRowExtension {

    #region Methods

    public static void DoInputSettings(this IControlAcceptRow dest, ConnectedFormulaView parent, IItemAcceptRow source) {
        dest.Name = source.DefaultItemToControlName();

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

    #endregion
}

public class ControlAcceptRow : ControlAcceptSomething {
}