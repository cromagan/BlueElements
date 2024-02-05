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

    public RowEntryControl(Database? database) : base() => FilterOutput.Database = database;

    #endregion

    #region Properties

    public List<IControlAcceptFilter> Childs { get; } = [];

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; }

    public FilterCollection FilterOutput { get; } = new("FilterIput 3");
    public List<IControlSendFilter> Parents { get; } = [];
    public bool RowManualSeted { get; set; } = false;
    public List<RowItem>? RowsInput { get; set; }

    #endregion

    #region Methods

    public void FilterOutput_Changed(object sender, System.EventArgs e) => this.FilterOutput_Changed();

    public void FilterOutput_Changing(object sender, System.EventArgs e) => this.FilterOutput_Changing();

    public void FilterOutput_DispodingEvent(object sender, System.EventArgs e) => this.FilterOutput_DispodingEvent();

    public void Rows_Changed() {
        if (this.RowSingleOrNull() is RowItem r) {
            FilterOutput.ChangeTo(new FilterItem(r));
        } else {
            FilterOutput.ChangeTo(new FilterItem(null as Database, FilterType.AlwaysFalse, string.Empty));
        }
    }

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