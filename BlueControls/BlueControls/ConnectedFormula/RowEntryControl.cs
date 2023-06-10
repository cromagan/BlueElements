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
using BlueControls.Interfaces;
using BlueDatabase;

namespace BlueControls.Controls;

internal class RowEntryControl : GenericControl, IControlAcceptRow, IControlSendRow, IControlRowInput {

    #region Fields

    private readonly List<IControlAcceptRow> _childs = new();

    private IControlSendRow? _getRowFrom;

    #endregion

    #region Constructors

    public RowEntryControl(DatabaseAbstract? database) : base() => OutputDatabase = database;

    #endregion

    #region Properties

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

    public void ChildAdd(IControlAcceptRow c) {
        if (IsDisposed) { return; }
        _childs.AddIfNotExists(c);
        this.DoChilds(_childs, LastInputRow);
    }

    public void SetData(DatabaseAbstract? database, long? rowkey) {
        //if(OutputDatabase== null) { return; }

        if (OutputDatabase != database) {
            return;
            //            Develop.DebugPrint(FehlerArt.Fehler, "Datenbanken inkonsitent!");
        }

        var r = database?.Row.SearchByKey(rowkey);
        if (r == LastInputRow) { return; }

        LastInputRow = r;
        this.DoChilds(_childs, LastInputRow);
    }

    #endregion
}