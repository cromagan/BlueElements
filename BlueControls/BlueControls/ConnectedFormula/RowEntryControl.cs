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
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;

namespace BlueControls.Controls;

internal class RowEntryControl : GenericControl, IControlUsesRow, IControlSendFilter {

    #region Constructors

    public RowEntryControl(Database? database) : base() {
        this.DoOutputSettings(database, Name);
        ((IControlSendFilter)this).RegisterEvents();
    }

    #endregion

    #region Properties

    public List<IControlAcceptFilter> Childs { get; } = [];

    public FilterCollection FilterOutput { get; } = new("FilterOutput 06");
    public List<IControlSendFilter> Parents { get; } = [];
    public bool RowChangedHandled { get; set; } = false;
    public bool RowManualSeted { get; set; } = false;
    public List<RowItem>? RowsInput { get; set; }

    #endregion

    #region Methods

    public void FilterOutput_Changed(object sender, System.EventArgs e) => this.FilterOutput_Changed();

    public void FilterOutput_Changing(object sender, System.EventArgs e) => this.FilterOutput_Changing();

    public void FilterOutput_DispodingEvent(object sender, System.EventArgs e) => this.FilterOutput_DispodingEvent();

    public void HandleRowsNow() {
        RowChangedHandled = true;
        this.DoRows(FilterOutput.Database, true);

        if (this.RowSingleOrNull() is RowItem r) {
            FilterOutput.ChangeTo(new FilterItem(r));
        } else {
            FilterOutput.ChangeTo(new FilterItem(FilterOutput.Database, FilterType.AlwaysFalse, string.Empty));
        }
    }

    public void Rows_Changed() => HandleRowsNow();

    public void Rows_Changing() { }

    public void RowsExternal_Added(object sender, RowChangedEventArgs e) => this.RowsExternal_Changed();

    public void RowsExternal_Removed(object sender, System.EventArgs e) => this.RowsExternal_Changed();

    protected override void Dispose(bool disposing) {
        if (disposing) {
            ((IControlSendFilter)this).DoDispose();
            ((IControlUsesRow)this).DoDispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}