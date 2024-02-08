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

using System.Collections.Generic;
using BlueBasics.Interfaces;
using BlueDatabase;

namespace BlueControls.Interfaces;

public interface IControlSendFilter : IDisposableExtendedWithEvent {

    #region Properties

    public List<IControlAcceptFilter> Childs { get; }

    /// <summary>
    /// Sollte von DoOutputSettings befüllt werden.
    /// </summary>
    public FilterCollection FilterOutput { get; }

    public string Name { get; set; }

    #endregion

    #region Methods

    void FilterOutput_Changed(object sender, System.EventArgs e);

    void FilterOutput_DispodingEvent(object sender, System.EventArgs e);

    #endregion
}

public static class ControlSendSomethingExtension {

    #region Methods

    public static void DisconnectChildParents(this IControlSendFilter parent, List<IControlAcceptFilter> childs) {
        var c = new List<IControlAcceptFilter>();
        c.AddRange(childs);

        foreach (var thisChild in c) {
            thisChild.DisconnectChildParents(parent);
        }
    }

    public static void DoDispose(this IControlSendFilter parent) {
        parent.DisconnectChildParents(parent.Childs);

        parent.FilterOutput.Dispose();

        parent.Childs.Clear();
    }

    public static void DoOutputSettings(this IControlSendFilter dest, Database? db, string name) {
        dest.Name = name;
        dest.FilterOutput.Database = db;
    }

    public static void DoOutputSettings(this IControlSendFilter dest, IItemSendFilter source) => dest.DoOutputSettings(source.DatabaseOutput, source.DefaultItemToControlName());

    public static void FilterOutput_Changed(this IControlSendFilter icsf) {
        if (icsf.IsDisposed) { return; }

        foreach (var thisChild in icsf.Childs) {
            thisChild.Invalidate_FilterInput();
            thisChild.ParentFilterOutput_Changed();

            if (thisChild is IControlUsesRow icur) {
                icur.Invalidate_RowsInput();
                icur.RowsInput_Changed();
            }

            thisChild.Invalidate();
        }
    }

    public static void FilterOutput_DispodingEvent(this IControlSendFilter icsf) {
        if (icsf.IsDisposed) { return; }
        icsf.FilterOutput.Changed -= icsf.FilterOutput_Changed;
        icsf.FilterOutput.DisposingEvent -= icsf.FilterOutput_DispodingEvent;

        if (!icsf.FilterOutput.IsDisposed) {
            icsf.FilterOutput.Database = null;
            icsf.FilterOutput.Dispose();
        }
    }

    public static void Invalidate_FilterOutput(this IControlSendFilter icsf) => icsf.FilterOutput.Clear();

    public static void RegisterEvents(this IControlSendFilter dest) {
        dest.FilterOutput.Changed += dest.FilterOutput_Changed;
        dest.FilterOutput.DisposingEvent += dest.FilterOutput_DispodingEvent;
    }

    #endregion
}