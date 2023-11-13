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
using System.Collections.Generic;

namespace BlueControls.Interfaces;

public interface IControlAcceptSomething : IDisposableExtendedWithEvent {

    #region Properties

    /// <summary>
    /// Ein Wert, der bei ParentDataChanged berechnet werden sollte.
    /// Enthält die DatabaseInput und auch den berechnete Zeile.
    /// </summary>
    public FilterCollection? FilterInput { get; set; }

    public string Name { get; set; }

    public List<IControlSendSomething> Parents { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Wird ausgelöst, wenn eine relevante Änderung an den Daten erfolgt ist.
    /// Hier können die neuen temporären Daten berechnet werden und sollten auch angezeigt werden und ein Invalidate gesetzt werden
    /// Events können gekoppelt werden
    /// </summary>
    public void FilterInput_Changed(object sender, System.EventArgs e);

    /// <summary>
    /// Wird ausgelöst, bevor eine relevante Änderung an den Daten erfolgt.
    /// Hier können Daten, die angezeigt werden, zurückgeschrieben werden. Events entkoppelt werden
    /// </summary>
    public void FilterInput_Changing(object sender, System.EventArgs e);

    #endregion
}

public static class IControlAcceptSomethingExtension {

    #region Methods

    /// <summary>
    /// Nachdem das Controll erzeugt wurde, werden hiermit die Einstellungen vom IItemAcceptSomething übernommen.
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="parent"></param>
    /// <param name="source"></param>
    public static void DoInputSettings(this IControlAcceptSomething dest, ConnectedFormulaView parent, IItemAcceptSomething source) {
        dest.Name = source.DefaultItemToControlName();

        foreach (var thisKey in source.Parents) {
            var it = source.Parent?[thisKey];

            if (it is IItemToControl itc) {
                var ff = parent.SearchOrGenerate(itc);

                if (ff is IControlSendSomething ffx) {
                    ffx.ConnectChildParents(dest);
                }
            }
        }
    }

    public static FilterCollection? FilterOfSender(this IControlAcceptSomething item) {
        if (item.FilterInput?.Database is not DatabaseAbstract db || db.IsDisposed) { return null; }

        FilterCollection? fc = null;

        foreach (var thiss in item.Parents) {
            if (thiss.FilterOutput is FilterCollection fi) {
                if (fc == null) { fc = new FilterCollection(fi.Database); }
                fc.AddIfNotExists(fi);
            }
        }

        return fc;
    }

    #endregion
}