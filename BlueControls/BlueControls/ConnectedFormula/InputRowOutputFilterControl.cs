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
using System.ComponentModel;
using BlueBasics;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.Controls;

internal class InputRowOutputFilterControl : Caption, IControlUsesFilter, IControlSendFilter {

    #region Fields

    private readonly ColumnItem? _inputcolumn;

    private readonly ColumnItem? _outputcolumn;

    private readonly FilterTypeRowInputItem _type;

    private FlexiFilterDefaultOutput _standard_bei_keiner_Eingabe = FlexiFilterDefaultOutput.Alles_Anzeigen;

    #endregion

    #region Constructors

    public InputRowOutputFilterControl(ColumnItem? inputcolumn, ColumnItem? outputcolumn, FilterTypeRowInputItem type) {
        _inputcolumn = inputcolumn;
        _outputcolumn = outputcolumn;
        _type = type;
        ((IControlSendFilter)this).RegisterEvents();
        ((IControlUsesFilter)this).RegisterEvents();
    }

    #endregion

    #region Properties

    public List<IControlAcceptFilter> Childs { get; } = [];

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterInput { get; } = new("FilterInput 05");

    public bool FilterInputChangedHandled { get; set; } = false;

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; } = new("FilterOutput 05");

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlSendFilter> Parents { get; } = [];

    public FlexiFilterDefaultOutput Standard_bei_keiner_Eingabe {
        get => _standard_bei_keiner_Eingabe;
        set {
            if (IsDisposed) { return; }
            if (_standard_bei_keiner_Eingabe == value) { return; }
            _standard_bei_keiner_Eingabe = value;
        }
    }

    #endregion

    #region Methods

    public void FilterInput_DispodingEvent(object sender, System.EventArgs e) => this.FilterInput_DispodingEvent();

    public void FilterInput_RowsChanged(object sender, System.EventArgs e) { }

    public void FilterOutput_Changed(object sender, System.EventArgs e) => this.FilterOutput_Changed();

    public void FilterOutput_DispodingEvent(object sender, System.EventArgs e) => this.FilterOutput_DispodingEvent();

    public void HandleFilterInputNow() {
        if (FilterInputChangedHandled) { return; }
        FilterInputChangedHandled = true;
        if (IsDisposed) { return; }

        this.DoInputFilter(null, false);
        Invalidate();

        var lastInputRow = FilterInput?.RowSingleOrNull;

        if (lastInputRow == null || _outputcolumn == null || _inputcolumn == null) {
            if (_standard_bei_keiner_Eingabe == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
                FilterOutput.ChangeTo(new FilterItem(FilterInput?.Database, "IO"));
            } else {
                this.Invalidate_FilterOutput();
            }
            return;
        }

        FilterItem? f;
        var va = lastInputRow.CellGetString(_inputcolumn);

        if (string.IsNullOrEmpty(va) && _standard_bei_keiner_Eingabe == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
            FilterOutput.ChangeTo(new FilterItem(_outputcolumn?.Database, "IO2"));
            return;
        }

        switch (_type) {
            case FilterTypeRowInputItem.Ist_GrossKleinEgal:
                f = new FilterItem(_outputcolumn, FilterType.Istgleich_GroßKleinEgal, va);
                break;

            case FilterTypeRowInputItem.Ist_genau:
                f = new FilterItem(_outputcolumn, FilterType.Istgleich, va);
                break;

            case FilterTypeRowInputItem.Ist_eines_der_Wörter_GrossKleinEgal:
                var list = va.HtmlSpecialToNormalChar(false).AllWords().SortedDistinctList();
                f = new FilterItem(_outputcolumn, FilterType.Istgleich_ODER_GroßKleinEgal, list);
                break;

            default:
                f = new FilterItem(_outputcolumn?.Database, "IO3");
                break;
        }

        FilterOutput.ChangeTo(f);
    }

    public void ParentFilterOutput_Changed() => HandleFilterInputNow();

    protected override void Dispose(bool disposing) {
        if (disposing) {
            ((IControlSendFilter)this).DoDispose();
            ((IControlUsesFilter)this).DoDispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}