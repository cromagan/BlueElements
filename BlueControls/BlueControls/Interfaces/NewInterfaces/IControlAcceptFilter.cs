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
using BlueControls.Controls;
using BlueDatabase;
using System.Collections.ObjectModel;
using BlueBasics;

namespace BlueControls.Interfaces;

public interface IControlAcceptFilter : IControlAcceptSomething {

    #region Properties

    public List<IControlSendFilter> GetFilterFrom { get; }

    public DatabaseAbstract? InputDatabase { get; set; }

    #endregion

    #region Methods

    public void FilterFromParentsChanged();

    #endregion
}

public static class IControlAcceptFilterExtension {

    #region Methods

    public static void AddGetFilterFrom(this IControlAcceptFilter mz, IControlSendFilter item) {
        mz.GetFilterFrom.AddIfNotExists(item);
        mz.FilterFromParentsChanged();
        item.ChildAdd(mz);
    }

    public static void DoInputSettings(this IControlAcceptFilter dest, ConnectedFormulaView parent, IItemAcceptFilter source) {
        dest.Name = source.DefaultItemToControlName();

        dest.InputDatabase = source.InputDatabase;

        foreach (var thisKey in source.GetFilterFrom) {
            var it = source.Parent?[thisKey];

            if (it is IItemToControl itc) {
                var ff = parent.SearchOrGenerate(itc);

                if (ff is IControlSendFilter ffx) {
                    dest.AddGetFilterFrom(ffx);
                }
            }
        }

        dest.FilterFromParentsChanged();
    }

    public static FilterCollection? FilterOfSender(this IControlAcceptFilter item) {
        if (item.InputDatabase is not DatabaseAbstract db) { return null; }

        var x = new FilterCollection(db);

        foreach (var thiss in item.GetFilterFrom) {
            if (thiss.Filter is FilterItem f) {
                x.Add(f);
            }
        }

        return x;
    }

    #endregion

    //public static DatabaseAbstract? InputDatabase(this IControlAcceptFilter item) {
    //    if (item.GetFilterFrom == null || item.GetFilterFrom.Count == 0) { return null; }
    //    return item.GetFilterFrom[0].OutputDatabase;
    //}
}

public class ControlAcceptFilter : ControlAcceptSomething {
}