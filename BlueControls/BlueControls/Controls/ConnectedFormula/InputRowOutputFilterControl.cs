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
using BlueDatabase;
using BlueDatabase.Enums;
using System.Windows.Forms;

namespace BlueControls.Controls;

internal class InputRowOutputFilterControl : GenericControlReciverSender {

    #region Fields

    private readonly string _filterwert;

    private readonly ColumnItem? _outputcolumn;

    private readonly FilterTypeRowInputItem _type;

    #endregion

    #region Constructors

    public InputRowOutputFilterControl(string filterwert, ColumnItem? outputcolumn, FilterTypeRowInputItem type) : base(false, false) {
        _filterwert = filterwert;
        _outputcolumn = outputcolumn;
        _type = type;
    }

    #endregion

    #region Methods

    public override void HandleChangesNow() {
        if (IsDisposed) { return; }

        base.HandleChangesNow();

        if (FilterInputChangedHandled) { return; }

        //if (!FilterInputChangedHandled) {
        //    FilterInputChangedHandled = true;
        //    this.DoInputFilter(null, false);
        //}

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

    public override void ParentFilterOutput_Changed() {
        base.ParentFilterOutput_Changed();
        HandleChangesNow();
    }

    protected override void OnCreateControl() {
        base.OnCreateControl();
        HandleChangesNow(); // Wenn keine Input-Rows da sind
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        Text = FilterOutput.ReadableText();
    }

    #endregion
}