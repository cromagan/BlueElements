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

using BlueControls.Interfaces;
using BlueDatabase;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlueControls.Controls;

public class GenericControlReciverSender : GenericControlReciver {

    #region Constructors

    public GenericControlReciverSender(bool doubleBuffer, bool useBackgroundBitmap) : base(doubleBuffer, useBackgroundBitmap) { }

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<GenericControlReciver> Childs { get; } = [];

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; } = new("FilterOutput");

    #endregion

    #region Methods

    public void DisconnectChildParents(List<GenericControlReciver> childs) {
        var c = new List<GenericControlReciver>();
        c.AddRange(childs);

        foreach (var thisChild in c) {
            thisChild.DisconnectChildParents(this);
        }
    }

    public void DoDefaultSettings(ConnectedFormulaView parent, IItemSendFilter source) {
        FilterOutput.Database = source.DatabaseOutput;
        base.DoDefaultSettings(parent, source);
    }

    public virtual void FilterOutput_PropertyChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }

        foreach (var thisChild in Childs) {
            thisChild.Invalidate_FilterInput();
        }
    }

    public void Invalidate_FilterOutput() => FilterOutput.Clear();

    protected override void Dispose(bool disposing) {
        if (Disposing) {
            DisconnectChildParents(Childs);

            FilterOutput.Dispose();

            Childs.Clear();
        }

        base.Dispose(disposing);
    }

    protected override void OnCreateControl() {
        FilterOutput.PropertyChanged += FilterOutput_PropertyChanged;
        FilterOutput.DisposingEvent += FilterOutput_DispodingEvent;
        base.OnCreateControl();
    }

    private void FilterOutput_DispodingEvent(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        FilterOutput.PropertyChanged -= FilterOutput_PropertyChanged;
        FilterOutput.DisposingEvent -= FilterOutput_DispodingEvent;

        if (!FilterOutput.IsDisposed) {
            FilterOutput.Database = null;
            FilterOutput.Dispose();
        }
    }

    #endregion
}