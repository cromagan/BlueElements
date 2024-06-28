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
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlueControls.Controls;

internal class InputRowOutputFilterControl : Caption, IControlAcceptFilter, IControlSendFilter {

    #region Fields

    private readonly string _filterwert;

    private readonly ColumnItem? _outputcolumn;

    private readonly FilterTypeRowInputItem _type;

    private FilterCollection? _filterInput;

    #endregion

    //private FlexiFilterDefaultOutput _standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;

    #region Constructors

    public InputRowOutputFilterControl(string filterwert, ColumnItem? outputcolumn, FilterTypeRowInputItem type) {
        _filterwert = filterwert;
        _outputcolumn = outputcolumn;
        _type = type;
        ((IControlSendFilter)this).RegisterEvents();
        ((IControlAcceptFilter)this).RegisterEvents();

        HandleChangesNow(); // Wenn keine Input-Rows da sind


    }

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

    public bool FilterInputChangedHandled { get; set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; } = new("FilterOutput 05");

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlSendFilter> Parents { get; } = [];

    #endregion

    //public FlexiFilterDefaultOutput Standard_bei_keiner_Eingabe {
    //    get => _standard_bei_keiner_Eingabe;
    //    set {
    //        if (IsDisposed) { return; }
    //        if (_standard_bei_keiner_Eingabe == value) { return; }
    //        _standard_bei_keiner_Eingabe = value;
    //    }
    //}

    #region Methods

    public void FilterInput_DispodingEvent(object sender, System.EventArgs e) => this.FilterInput_DispodingEvent();

    public void FilterInput_RowsChanged(object sender, System.EventArgs e) { }

    public void FilterOutput_DispodingEvent(object sender, System.EventArgs e) => this.FilterOutput_DispodingEvent();

    public void FilterOutput_PropertyChanged(object sender, System.EventArgs e) {
        this.FilterOutput_PropertyChanged();

        Text = FilterOutput.ReadableText();
    }

    public void HandleChangesNow() {
        if (IsDisposed) { return; }
        if (FilterInputChangedHandled) { return; }

        if (!FilterInputChangedHandled) {
            FilterInputChangedHandled = true;
            this.DoInputFilter(null, false);
        }

        FilterInputChangedHandled = true;

        this.DoInputFilter(null, false);
        Invalidate();

        var lastInputRow = FilterInput?.RowSingleOrNull;

        if (_outputcolumn == null) {
            //if (_standard_bei_keiner_Eingabe == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
            FilterOutput.ChangeTo(new FilterItem(FilterInput?.Database, "IO"));
            //} else {
            //    this.Invalidate_FilterOutput();
            //}
            return;
        }

        var va = string.Empty;

        if (lastInputRow != null) {
            lastInputRow.CheckRowDataIfNeeded();
            va = lastInputRow.ReplaceVariables(_filterwert, false, true, lastInputRow.LastCheckedEventArgs?.Variables);
        } else {
            if (FilterInput != null) {
                FilterOutput.ChangeTo(new FilterItem(FilterInput?.Database, "IO"));
                return;
            }
        }

        //if (string.IsNullOrEmpty(va) && _standard_bei_keiner_Eingabe == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
        //    FilterOutput.ChangeTo(new FilterItem(_outputcolumn?.Database, "IO2"));
        //    return;
        //}

        FilterItem? f;

        switch (_type) {
            case FilterTypeRowInputItem.Ist_schreibungsneutral:
                f = new FilterItem(_outputcolumn, FilterType.Istgleich_GroßKleinEgal, va);
                break;

            case FilterTypeRowInputItem.Ist_genau:
                f = new FilterItem(_outputcolumn, FilterType.Istgleich, va);
                break;

            case FilterTypeRowInputItem.Ist_eines_der_Wörter_schreibungsneutral:
                var list = va.HtmlSpecialToNormalChar(false).AllWords().SortedDistinctList();
                f = new FilterItem(_outputcolumn, FilterType.Istgleich_ODER_GroßKleinEgal, list);
                break;


            case FilterTypeRowInputItem.Ist_nicht:
                f = new FilterItem(_outputcolumn, FilterType.Ungleich_MultiRowIgnorieren, va);
                break;

            default:
                f = new FilterItem(_outputcolumn?.Database, "IO3");
                break;
        }

        FilterOutput.ChangeTo(f);
    }

    public void ParentFilterOutput_Changed() => HandleChangesNow();

    protected override void Dispose(bool disposing) {
        if (disposing) {
            ((IControlSendFilter)this).DoDispose();
            ((IControlAcceptFilter)this).DoDispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}