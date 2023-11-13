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
using System.ComponentModel;
using BlueControls.Interfaces;
using BlueDatabase;

namespace BlueControls.Controls;

internal class RowEntryControl : GenericControl, IControlAcceptSomething, IControlSendSomething {

    #region Constructors

    public RowEntryControl(DatabaseAbstract? database) : base() => FilterOutput.Database = database;

    #endregion

    #region Properties

    public List<IControlAcceptSomething> Childs { get; } = new();

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; }

    public FilterCollection FilterOutput { get; } = new();

    public List<IControlSendSomething> Parents { get; } = new();

    #endregion

    #region Methods

    public void FilterInput_Changed(object sender, System.EventArgs e) {
        FilterInput = this.FilterOfSender();
        Invalidate();

        FilterOutput.Clear();
        if (FilterInput == null || FilterOutput.Database != FilterInput.Database) { return; }

        FilterOutput.AddIfNotExists(FilterInput);
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            FilterInput?.Dispose();
            FilterOutput.Dispose();
            FilterInput = null;
        }
        base.Dispose(disposing);
    }

    #endregion
}