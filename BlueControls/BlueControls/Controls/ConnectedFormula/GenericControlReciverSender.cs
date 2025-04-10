// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlueControls.Controls;

public class GenericControlReciverSender : GenericControlReciver {

    #region Constructors

    public GenericControlReciverSender(bool doubleBuffer, bool useBackgroundBitmap, bool mouseHighlight) : base(doubleBuffer, useBackgroundBitmap, mouseHighlight) { }

    #endregion

    #region Events

    public event EventHandler? FilterOutputPropertyChanged;

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

    public void DoDefaultSettings(ConnectedFormulaView? parentFormula, ReciverSenderControlPadItem source, string mode) {
        FilterOutput.Database = source.DatabaseOutput;
        base.DoDefaultSettings(parentFormula, source, mode);

        //if (parentFormula == null) { return; }

        //foreach (var thisKey in source.ChildIds) {
        //    var it = source.Parent?[thisKey];

        //    if (it is IItemToControl itc) {
        //        var parentCon = parentFormula.SearchOrGenerate(itc, true, mode);
        //        if (parentCon is GenericControlReciver exitingChild) {
        //            ChildIsBorn(exitingChild);
        //        }
        //    }
        //}
    }

    internal void ChildIsBorn(GenericControlReciver child) {
        if (child.RowsInputManualSeted) {
            Develop.DebugPrint(ErrorType.Error, "Manuelle Filterung kann keine Parents empfangen.");
        }

        if (child.IsDisposed || IsDisposed) { return; }

        var isnew = !child.Parents.Contains(this);
        var newFilters = FilterOutput.Count > 0;
        //var doDatabaseAfter = DatabaseInput == null;

        if (isnew) { _ = child.Parents.AddIfNotExists(this); }

        _ = Childs.AddIfNotExists(child);

        if (newFilters && isnew) { child.Invalidate_FilterInput(); }

        //if (doDatabaseAfter) { child.RegisterEvents(); }
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (Disposing) {
            Parent.Controls.Remove(this);
            FilterOutput.Dispose();

            foreach (var thisChild in Childs) {
                _ = thisChild.Parents.Remove(this);
            }

            Childs.Clear();
        }
    }

    protected void Invalidate_FilterOutput() => FilterOutput.Clear();

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

    private void FilterOutput_PropertyChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }

        foreach (var thisChild in Childs) {
            thisChild.Invalidate_FilterInput();
        }
        OnFilterOutputPropertyChanged();
    }

    private void OnFilterOutputPropertyChanged() => FilterOutputPropertyChanged?.Invoke(this, System.EventArgs.Empty);

    #endregion
}