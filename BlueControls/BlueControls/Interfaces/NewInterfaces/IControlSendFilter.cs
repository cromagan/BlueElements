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
using BlueDatabase;
using System.Collections.Generic;

namespace BlueControls.Interfaces;

public interface IControlSendFilter : IControlSendSomething {

    #region Properties

    public FilterItem? Filter { get; }

    #endregion
}

public static class IControlSendFilterExtension {

    #region Methods

    public static void DoChilds(this IControlSendFilter item, List<IControlAcceptSomething> childs, long? rowkey) {
        //var r = db?.Row.SearchByKey(rowkey);
        //r?.CheckRowDataIfNeeded();

        //foreach (var thischild in childs) {
        //    var did = false;

        //    if (!did && thischild is IAcceptRowKey fcfc) {
        //        fcfc.SetData(db, rowkey);
        //        did = true;
        //    }

        //    if (!did && thischild is IAcceptVariableList rv) {
        //        _ = rv.ParseVariables(r?.LastCheckedEventArgs?.Variables);
        //        did = true;
        //    }

        //    if (thischild is IDisabledReason id) {
        //        if (!did) {
        //            id.DeleteValue();
        //            id.DisabledReason = "Keine Befüllmethode bekannt.";
        //        }
        //    }
        //}
    }

    public static void DoOutputSettings(this IControlSendFilter dest, ConnectedFormulaView parent, IItemSendFilter source) {
        dest.Name = source.DefaultItemToControlName();
    }

    #endregion
}

public class ControlSendFilter : ControlSendSomething {
}