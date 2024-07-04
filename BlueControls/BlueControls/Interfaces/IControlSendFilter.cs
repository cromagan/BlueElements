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

using BlueBasics.Interfaces;
using BlueDatabase;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlueControls.Interfaces;

public interface IControlSendFilter : IDisposableExtendedWithEvent {

    #region Properties

    /// <summary>
    /// Einfaches Property, muss einfach nur zur Verfügung gestellt werden.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlAcceptFilter> Childs { get; }

    /// <summary>
    /// Sollte von DoOutputSettings befüllt werden.
    /// Wird im Steuerlement nur Initialsisiert, wie z.B.   public FilterCollection FilterOutput { get; } = new("FilterOutput 08");
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; }

    public string Name { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Weiterleitung zu: this.FilterOutput_DispodingEvent();
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void FilterOutput_DispodingEvent(object sender, System.EventArgs e);

    void FilterOutput_PropertyChanged(object sender, System.EventArgs e);

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

    /// <summary>
    /// Spezielle Routine, wird von verschiedenen Stelle speziell aufgerufen.
    /// evtl. ist IHasSettings bereits besonder abgehandelt
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="db"></param>
    /// <param name="name"></param>
    public static void DoOutputSettings(this IControlSendFilter dest, Database? db, string name) {
        dest.Name = name;
        dest.FilterOutput.Database = db;
    }

    public static void DoOutputSettings(this IControlSendFilter dest, IItemSendFilter source) {
        if (dest is IHasSettings s) {
            s.SettingsManualFilename = "%homepath%\\FRM_" + source.KeyName;
        }

        dest.DoOutputSettings(source.DatabaseOutput, source.DefaultItemToControlName());
    }

    public static void FilterOutput_DispodingEvent(this IControlSendFilter icsf) {
        if (icsf.IsDisposed) { return; }
        icsf.FilterOutput.PropertyChanged -= icsf.FilterOutput_PropertyChanged;
        icsf.FilterOutput.DisposingEvent -= icsf.FilterOutput_DispodingEvent;

        if (!icsf.FilterOutput.IsDisposed) {
            icsf.FilterOutput.Database = null;
            icsf.FilterOutput.Dispose();
        }
    }

    public static void FilterOutput_PropertyChanged(this IControlSendFilter icsf) {
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

    public static void Invalidate_FilterOutput(this IControlSendFilter icsf) => icsf.FilterOutput.Clear();

    public static void RegisterEvents(this IControlSendFilter dest) {
        dest.FilterOutput.PropertyChanged += dest.FilterOutput_PropertyChanged;
        dest.FilterOutput.DisposingEvent += dest.FilterOutput_DispodingEvent;
    }

    #endregion
}