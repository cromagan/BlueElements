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

using BlueBasics;

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using static BlueControls.ItemCollectionList.ItemCollectionList;
using BlueControls.ItemCollectionList;

using BlueDatabase;
using BlueDatabase.Enums;

using System.Collections.Generic;

using System.ComponentModel;

using System.Drawing;

using static BlueBasics.Extensions;

using BlueBasics;

using BlueBasics.Enums;

using BlueDatabase.EventArgs;

using BlueScript.Variables;

using System;

using System.Collections.Specialized;

using System.Diagnostics;

using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static BlueBasics.Constants;
using static BlueBasics.Develop;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using static BlueControls.FormManager;

//using static BlueDatabase.Database;
using static BlueBasics.Converter;

using BlueControls.ItemCollectionList;
using static BlueControls.ItemCollectionList.ItemCollectionList;

#nullable enable

namespace BlueControls.ConnectedFormula;

internal class FlexiControlRowSelector : FlexiControl, IControlSendFilter, IControlUsesRow, IDisposableExtended {

    #region Fields

    private readonly string _showformat;

    private BlueDatabase.FilterCollection? _filterInput;

    #endregion

    #region Constructors

    public FlexiControlRowSelector(BlueDatabase.Database? database, string caption, string showFormat) : base() {
        CaptionPosition = CaptionPosition.Über_dem_Feld;
        EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;

        Caption = string.IsNullOrEmpty(caption) ? "Wählen:" : caption;
        _showformat = showFormat;

        if (string.IsNullOrEmpty(_showformat) && database != null && database.Column.Count > 0 && database.Column.First() is BlueDatabase.ColumnItem fc) {
            _showformat = "~" + fc.KeyName + "~";
        }
        ((IControlSendFilter)this).RegisterEvents();
        ((IControlAcceptFilter)this).RegisterEvents();
    }

    #endregion

    #region Properties

    public List<IControlAcceptFilter> Childs { get; } = [];

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BlueDatabase.FilterCollection? FilterInput {
        get => _filterInput;
        set {
            if (_filterInput == value) { return; }
            this.UnRegisterEventsAndDispose();
            _filterInput = value;
            ((IControlAcceptFilter)this).RegisterEvents();
        }
    }

    public bool FilterInputChangedHandled { get; set; }

    public BlueDatabase.FilterCollection FilterOutput { get; } = new("FilterOutput 04");

    public List<IControlSendFilter> Parents { get; } = [];

    public List<BlueDatabase.RowItem>? RowsInput { get; set; }

    public bool RowsInputChangedHandled { get; set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool RowsInputManualSeted { get; set; } = false;

    #endregion

    #region Methods

    public void FilterInput_DispodingEvent(object sender, System.EventArgs e) => this.FilterInput_DispodingEvent();

    public void FilterInput_RowsChanged(object sender, System.EventArgs e) => this.FilterInput_RowsChanged();

    public void FilterOutput_DispodingEvent(object sender, System.EventArgs e) => this.FilterOutput_DispodingEvent();

    public void FilterOutput_PropertyChanged(object sender, System.EventArgs e) => this.FilterOutput_PropertyChanged();

    public void HandleChangesNow() {
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        if (!FilterInputChangedHandled) {
            FilterInputChangedHandled = true;
            this.DoInputFilter(FilterOutput.Database, true);
        }

        RowsInputChangedHandled = true;

        if (!Allinitialized) { _ = CreateSubControls(); }

        this.DoRows();

        #region Combobox suchen

        ComboBox? cb = null;
        foreach (var thiscb in Controls) {
            if (thiscb is ComboBox cbx) { cb = cbx; break; }
        }

        #endregion

        if (cb == null) { return; }

        var ex = cb.Items().ToList();

        #region Zeilen erzeugen

        if (RowsInput == null || !RowsInputChangedHandled) { return; }

        foreach (var thisR in RowsInput) {
            if (cb[thisR.KeyName] == null) {
                var tmpQuickInfo = thisR.ReplaceVariables(_showformat, true, true);
                cb.ItemAdd(Add(tmpQuickInfo, thisR.KeyName));
            } else {
                ex.Remove(thisR.KeyName);
            }
        }

        #endregion

        #region Veraltete Zeilen entfernen

        foreach (var thisit in ex) {
            cb?.Remove(thisit);
        }

        #endregion

        #region Nur eine Zeile? auswählen!

        // nicht vorher auf null setzen, um Blinki zu vermeiden
        if (cb.ItemCount == 1) {
            ValueSet(cb[0].KeyName, true);
        }

        if (cb.ItemCount < 2) {
            DisabledReason = "Keine Auswahl möglich.";
        } else {
            DisabledReason = string.Empty;
        }

        #endregion

        #region  Prüfen ob die aktuelle Auswahl passt

        // am Ende auf null setzen, um Blinki zu vermeiden

        if (cb[Value] == null) {
            ValueSet(string.Empty, true);
        }

        #endregion
    }

    public void ParentFilterOutput_Changed() { }

    public void RowsInput_Changed() { }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            ((IControlSendFilter)this).DoDispose();
            ((IControlUsesRow)this).DoDispose();
            Tag = null;
        }

        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) {
        HandleChangesNow();
        base.DrawControl(gr, state);
    }

    protected override void OnValueChanged() {
        base.OnValueChanged();

        var row = RowsInput?.Get(Value);

        if (row == null) {
            this.Invalidate_FilterOutput();
            return;
        }

        FilterOutput.ChangeTo(new BlueDatabase.FilterItem(row));
    }

    #endregion
}