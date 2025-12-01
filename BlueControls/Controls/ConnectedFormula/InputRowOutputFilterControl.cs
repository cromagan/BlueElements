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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueTable;
using BlueTable.Enums;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls;

internal class InputRowOutputFilterControl : GenericControlReciverSender {

    #region Fields

    private readonly string _filterwert;

    private readonly ColumnItem? _outputcolumn;

    private readonly FilterTypeRowInputItem _type;

    #endregion

    #region Constructors

    public InputRowOutputFilterControl(string filterwert, ColumnItem? outputcolumn, FilterTypeRowInputItem type) : base(false, false, false) {
        _filterwert = filterwert;
        _outputcolumn = outputcolumn;
        _type = type;
    }

    #endregion

    #region Properties

    public string ErrorText { get; set; } = string.Empty;

    #endregion

    #region Methods

    public override void Invalidate_FilterInput() {
        base.Invalidate_FilterInput();
        HandleChangesNow();
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);

        string txt;

        var qi = QuickImage.Get(ImageCode.Trichter);

        if (FilterOutput.Count == 0) {
            txt = string.IsNullOrEmpty(ErrorText) ? ErrorText : "Kein Filter";

            qi = null;
        } else if (!FilterOutput.IsOk()) {
            txt = FilterOutput.ErrorReason();

            qi = QuickImage.Get(ImageCode.Warnung);
        } else {
            if (!string.IsNullOrEmpty(ErrorText) && FilterOutput.HasAlwaysFalse()) {
                txt = ErrorText;
                qi = null;
            } else {
                txt = FilterOutput.ReadableText();
            }
        }

        Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);
        Skin.Draw_FormatedText(gr, txt, qi, Alignment.Top_Left, DisplayRectangle, Design.Caption, States.Standard, null, false, false);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);
        RowsInputChangedHandled = true;
        Invalidate();

        var lastInputRow = FilterInput?.RowSingleOrNull;

        if (_outputcolumn == null) {
            //if (_standard_bei_keiner_Eingabe == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
            FilterOutput.ChangeTo(new FilterItem(FilterInput?.Table, "IO"));
            //} else {
            //    this.Invalidate_FilterOutput();
            //}
            return;
        }

        string? va;
        if (lastInputRow != null) {
            va = lastInputRow.ReplaceVariables(_filterwert, true, lastInputRow.CheckRow()?.Feedback.Variables);
        } else {
            if (FilterInput != null) {
                FilterOutput.ChangeTo(new FilterItem(_outputcolumn.Table, "IO"));
                return;
            }
            va = _filterwert;
        }

        if (va.Contains("~")) {
            FilterOutput.ChangeTo(new FilterItem(_outputcolumn.Table, "IO2"));
            return;
        }

        //if (string.IsNullOrEmpty(va) && _standard_bei_keiner_Eingabe == FlexiFilterDefaultOutput.Nichts_Anzeigen) {
        //    FilterOutput.ChangeTo(new FilterItem(_outputcolumn?.Table, "IO2"));
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
                var list = va.AllWords().SortedDistinctList();
                f = new FilterItem(_outputcolumn, FilterType.Istgleich_ODER_GroßKleinEgal, list);
                break;

            case FilterTypeRowInputItem.Ist_nicht:
                f = new FilterItem(_outputcolumn, FilterType.Ungleich_MultiRowIgnorieren, va);
                break;

            default:
                f = new FilterItem(_outputcolumn?.Table, "IO3");
                break;
        }

        FilterOutput.ChangeTo(f);
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