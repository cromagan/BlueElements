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

internal class InputRowOutputFilterControl : Caption, IControlAcceptSomething, IControlSendSomething {

    #region Fields

    private readonly ColumnItem? _inputcolumn;
    private readonly ColumnItem? _outputcolumn;
    private readonly FilterTypeRowInputItem _type;

    #endregion

    #region Constructors

    public InputRowOutputFilterControl(ColumnItem? inputcolumn, ColumnItem? outputcolumn, FilterTypeRowInputItem type) {
        _inputcolumn = inputcolumn;
        _outputcolumn = outputcolumn;
        _type = type;
    }

    #endregion

    #region Properties

    public List<IControlAcceptSomething> Childs { get; } = new();

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; }

    public bool FilterManualSeted { get; set; } = false;
    public FilterCollection FilterOutput { get; } = new();
    public List<IControlSendSomething> Parents { get; } = new();

    #endregion

    #region Methods

    public void FilterInput_Changed(object sender, System.EventArgs e) {
        FilterInput = this.FilterOfSender();
        Invalidate();

        var lastInputRow = FilterInput?.RowSingleOrNull;
        lastInputRow?.CheckRowDataIfNeeded();

        if (lastInputRow == null || _outputcolumn == null || _inputcolumn == null) {
            FilterOutput.Clear();
            return;
        }

        switch (_type) {
            case FilterTypeRowInputItem.Ist_GrossKleinEgal:
                FilterOutput.ChangeTo(new FilterItem(_outputcolumn, FilterType.Istgleich_GroßKleinEgal, lastInputRow.CellGetString(_inputcolumn)));
                return;

            case FilterTypeRowInputItem.Ist_genau:
                FilterOutput.ChangeTo(new FilterItem(_outputcolumn, FilterType.Istgleich, lastInputRow.CellGetString(_inputcolumn)));
                return;

            case FilterTypeRowInputItem.Ist_eines_der_Wörter_GrossKleinEgal:
                var list = lastInputRow.CellGetString(_inputcolumn).HtmlSpecialToNormalChar(false).AllWords().SortedDistinctList();
                FilterOutput.ChangeTo(new FilterItem(_outputcolumn, FilterType.Istgleich_ODER_GroßKleinEgal, list));

                //List<string> names = new();
                //names.AddRange(_outputcolumn.GetUcaseNamesSortedByLenght());

                //var tmpvalue = LastInputRow.CellGetString(_outputcolumn);

                //foreach (var thisWord in names) {
                //    var fo = tmpvalue.IndexOfWord(thisWord, 0, RegexOptions.IgnoreCase);
                //    if (fo > -1) {
                //        value.Add(thisWord);
                //    }
                //}
                return;

            default:
                Develop.DebugPrint(_type);
                FilterOutput.Clear();
                return;
        }
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { }

    #endregion
}