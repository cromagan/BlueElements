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
using System.Collections.ObjectModel;
using BlueBasics.Enums;
using BlueBasics;
using BlueControls.Interfaces;
using BlueDatabase;

namespace BlueControls.Controls;

internal class RowEntryControl : GenericControl, IControlAcceptRow, IControlSendRow, IControlRowInput {

    #region Fields

    private readonly List<IControlAcceptSomething> _childs = new();

    #endregion

    #region Constructors

    public RowEntryControl(DatabaseAbstract database) : base() {
        OutputDatabase = database;
    }

    #endregion

    #region Properties

    public IControlSendRow? GetRowFrom { get; set; }

    public RowItem? LastInputRow { get; private set; }
    public DatabaseAbstract? OutputDatabase { get; }

    #endregion

    #region Methods

    public void ChildAdd(IControlAcceptSomething c) {
        if (IsDisposed) { return; }
        _childs.Add(c);
        this.DoChilds(_childs, LastInputRow);
    }

    public void SetData(DatabaseAbstract? database, long? rowkey) {

        if(OutputDatabase== null) { return; }

        if (OutputDatabase != database) {
            Develop.DebugPrint(FehlerArt.Fehler, "Datenbanken inkonsitent!");
        }

        var r = database?.Row.SearchByKey(rowkey);
        if (r == LastInputRow) { return; }

        LastInputRow = r;
        this.DoChilds(_childs, LastInputRow);
    }

    #endregion
}