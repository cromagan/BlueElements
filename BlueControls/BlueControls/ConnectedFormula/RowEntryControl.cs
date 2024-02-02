﻿// Authors:
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

namespace BlueControls.Controls;

internal class RowEntryControl : GenericControl, IControlAcceptSomething, IControlSendSomething {

    #region Constructors

    public RowEntryControl(Database? database) : base() => FilterOutput.Database = database;

    #endregion

    #region Properties

    public List<IControlAcceptSomething> Childs { get; } = [];

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; }

    public bool FilterManualSeted { get; set; } = false;

    public FilterCollection FilterOutput { get; } = new("FilterIput 3");

    public List<IControlSendSomething> Parents { get; } = [];

    #endregion

    #region Methods

    public void FilterInput_Changed(object? sender, System.EventArgs e) {
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { }

    public void FilterInput_RowChanged(object? sender, System.EventArgs e) {
        this.DoInputFilter(FilterOutput.Database, false);
        FilterOutput.ChangeTo(FilterInput);
    }

    public void Parents_Added(bool hasFilter) {
        if (IsDisposed) { return; }
        if (!hasFilter) { return; }
        FilterInput_RowChanged(null, System.EventArgs.Empty);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.Invalidate_FilterInput(false);
            FilterOutput.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}