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

namespace BlueControls.Controls;

internal class RowEntryControl : GenericControlReciver, IControlSendFilter {

    #region Constructors

    public RowEntryControl(Database? database) : base(false, false) {
        this.DoOutputSettings(database, Name);
        ((IControlSendFilter)this).RegisterEvents();
        RegisterEvents();
    }

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<GenericControlReciver> Childs { get; } = [];

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; } = new("FilterOutput 06");

    #endregion

    #region Methods

    public void FilterOutput_DispodingEvent(object sender, System.EventArgs e) => this.FilterOutput_DispodingEvent();

    public void FilterOutput_PropertyChanged(object sender, System.EventArgs e) => this.FilterOutput_PropertyChanged();

    public override void HandleChangesNow() {
        base.HandleChangesNow();

        if (IsDisposed) { return; }
        if (FilterInputChangedHandled) { return; }

        if (!FilterInputChangedHandled) {
            FilterInputChangedHandled = true;
            DoInputFilter(FilterOutput.Database, true);
        }

        FilterInputChangedHandled = true;

        DoRows();

        if (RowSingleOrNull() is RowItem r) {
            FilterOutput.ChangeTo(new FilterItem(r));
        } else {
            FilterOutput.ChangeTo(new FilterItem(FilterOutput.Database, FilterType.AlwaysFalse, string.Empty));
        }
    }

    public override void RowsInput_Changed() => HandleChangesNow();

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.DoDispose();
            base.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}