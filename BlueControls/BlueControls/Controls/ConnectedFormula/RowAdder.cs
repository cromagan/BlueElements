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

using BlueBasics;
using BlueControls.Interfaces;
using BlueDatabase;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace BlueControls.Controls;

public partial class RowAdder : System.Windows.Forms.UserControl, IControlAcceptFilter, IControlSendFilter, IControlUsesRow {

    #region Fields

    private FilterCollection? _filterInput;

    #endregion

    #region Constructors

    public RowAdder() {
        InitializeComponent();
    }

    #endregion

    #region Events

    public event EventHandler? DisposingEvent;

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlAcceptFilter> Childs { get; } = [];

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput {
        get => _filterInput;
        set {
            if (_filterInput == value) { return; }
            this.UnRegisterEventsAndDispose();
            _filterInput = value;
            ((IControlAcceptFilter)this).RegisterEvents();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool FilterInputChangedHandled { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; } = new("FilterOutput 08");

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlSendFilter> Parents { get; } = [];

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<RowItem>? RowsInput { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool RowsInputChangedHandled { get; set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool RowsInputManualSeted { get; set; } = false;

    #endregion

    #region Methods

    public void FilterInput_DispodingEvent(object sender, System.EventArgs e) => this.FilterInput_DispodingEvent();

    public void FilterInput_RowsChanged(object sender, System.EventArgs e) => this.FilterInput_RowsChanged();

    public void FilterOutput_DispodingEvent(object sender, System.EventArgs e) => this.FilterOutput_DispodingEvent();

    public void FilterOutput_PropertyChanged(object sender, System.EventArgs e) => this.FilterOutput_PropertyChanged();

    public void HandleChangesNow() {
        Develop.DebugPrint_NichtImplementiert(true);
        //if (IsDisposed) { return; }
        //if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        //if (!FilterInputChangedHandled) {
        //    FilterInputChangedHandled = true;
        //    this.DoInputFilter(FilterOutput.Database, true);
        //}

        //RowsInputChangedHandled = true;

        //if (!Allinitialized) { _ = CreateSubControls(); }

        //this.DoRows();

        //#region Combobox suchen

        //ComboBox? cb = null;
        //foreach (var thiscb in Controls) {
        //    if (thiscb is ComboBox cbx) { cb = cbx; break; }
        //}

        //#endregion

        //if (cb == null) { return; }

        //var ex = cb.Items().ToList();

        //#region Zeilen erzeugen

        //if (RowsInput == null || !RowsInputChangedHandled) { return; }

        //foreach (var thisR in RowsInput) {
        //    if (cb[thisR.KeyName] == null) {
        //        var tmpQuickInfo = thisR.ReplaceVariables(_showformat, true, true, null);
        //        cb.ItemAdd(ItemOf(tmpQuickInfo, thisR.KeyName));
        //    } else {
        //        ex.Remove(thisR.KeyName);
        //    }
        //}

        //#endregion

        //#region Veraltete Zeilen entfernen

        //foreach (var thisit in ex) {
        //    cb?.Remove(thisit);
        //}

        //#endregion

        //#region Nur eine Zeile? auswählen!

        //// nicht vorher auf null setzen, um Blinki zu vermeiden
        //if (cb.ItemCount == 1) {
        //    ValueSet(cb[0].KeyName, true);
        //}

        //if (cb.ItemCount < 2) {
        //    DisabledReason = "Keine Auswahl möglich.";
        //} else {
        //    DisabledReason = string.Empty;
        //}

        //#endregion

        //#region  Prüfen ob die aktuelle Auswahl passt

        //// am Ende auf null setzen, um Blinki zu vermeiden

        //if (cb[Value] == null) {
        //    ValueSet(string.Empty, true);
        //}

        //#endregion
    }

    public void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    public void ParentFilterOutput_Changed() { }

    public void RowsInput_Changed() { }

    #endregion
}