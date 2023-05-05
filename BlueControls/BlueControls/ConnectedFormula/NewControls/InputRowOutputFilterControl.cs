// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;

namespace BlueControls.Controls;

internal class InputRowOutputFilterControl : GenericControl, IControlAcceptRow, IControlSendFilter {

    #region Fields

    private readonly List<IControlAcceptSomething> _childs = new();

    private readonly ColumnItem? _inputcolumn = null;
    private readonly ColumnItem? _outputcolumn = null;
    private readonly FilterTypeRowInputItem _type;
    private FilterItem? _filter = null;
    private IControlSendRow? _getRowFrom = null;

    #endregion

    #region Constructors

    public InputRowOutputFilterControl(ColumnItem? inputcolumn, ColumnItem? outputcolumn, FilterTypeRowInputItem type) {
        _inputcolumn = inputcolumn;
        _outputcolumn = outputcolumn;
        _type = type;
    }

    #endregion

    #region Properties

    public FilterItem? Filter {
        get => _filter;
        set {
            if (_filter?.Equals(value) ?? false) { return; }
            _filter = value;
            this.DoChilds(_childs);
        }
    }

    public IControlSendRow? GetRowFrom {
        get => _getRowFrom;
        set {
            if (_getRowFrom == value) { return; }
            if (_getRowFrom != null) {
                Develop.DebugPrint(BlueBasics.Enums.FehlerArt.Fehler, "Änderung nicht erlaubt");
            }

            _getRowFrom = value;
            if (_getRowFrom != null) { _getRowFrom.ChildAdd(this); }
        }
    }

    public RowItem? LastInputRow { get; private set; }
    public DatabaseAbstract? OutputDatabase { get; set; }

    #endregion

    #region Methods

    public void ChildAdd(IControlAcceptSomething c) {
        if (IsDisposed) { return; }
        _childs.Add(c);
        this.DoChilds(_childs);
    }

    public void SetData(DatabaseAbstract? database, long? rowkey) {
        //if (OutputDatabase != database) {
        //    Develop.DebugPrint(FehlerArt.Fehler, "Datenbanken inkonsitent!");
        //}

        //Develop.DebugPrint_NichtImplementiert();

        var nr = database?.Row.SearchByKey(rowkey);
        if (nr == LastInputRow) { return; }

        LastInputRow = nr;
        LastInputRow?.CheckRowDataIfNeeded();

        if (LastInputRow == null || _inputcolumn == null || _outputcolumn == null) {
            Filter = new FilterItem(_outputcolumn, BlueDatabase.Enums.FilterType.Fehler, string.Empty);
        }

        q
        Filter = null;
    }

    #endregion
}