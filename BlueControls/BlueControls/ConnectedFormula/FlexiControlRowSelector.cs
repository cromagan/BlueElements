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
using System.Drawing;
using BlueBasics;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;

#nullable enable

namespace BlueControls.ConnectedFormula;

internal class FlexiControlRowSelector : FlexiControl, IControlSendSomething, IControlAcceptSomething {

    #region Fields

    private readonly string _showformat;

    #endregion

    #region Constructors

    public FlexiControlRowSelector(Database? database, string caption, string showFormat) : base() {
        CaptionPosition = CaptionPosition.Über_dem_Feld;
        EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;

        Caption = string.IsNullOrEmpty(caption) ? "Wählen:" : caption;
        _showformat = showFormat;

        if (string.IsNullOrEmpty(_showformat) && database != null && database.Column.Count > 0 && database.Column.First() is ColumnItem fc) {
            _showformat = "~" + fc.KeyName + "~";
        }
    }

    #endregion

    #region Properties

    public List<IControlAcceptSomething> Childs { get; } = [];

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; }

    public bool FilterManualSeted { get; set; } = false;

    public FilterCollection FilterOutput { get; } = new("FilterOutput");

    public List<IControlSendSomething> Parents { get; } = [];

    #endregion

    #region Methods

    public void FilterInput_Changed(object? sender, System.EventArgs e) {
        if (FilterManualSeted) { Develop.DebugPrint("Steuerelement unterstützt keine manuellen Filter"); }
        FilterInput?.Dispose();
        FilterInput = null;
        Invalidate();
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { }

    public void Parents_Added(bool hasFilter) {
        if (IsDisposed) { return; }
        if (!hasFilter) { return; }
        FilterInput_Changed(null, System.EventArgs.Empty);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            //components?.Dispose();
            FilterInput?.Dispose();
            FilterOutput.Dispose();
            FilterInput = null;
            Tag = null;
            Childs.Clear();
        }

        base.Dispose(disposing);
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (FilterInput == null) {
            UpdateMyCollection();
        }
        base.DrawControl(gr, state);
    }

    protected override void OnValueChanged() {
        base.OnValueChanged();

        var row = FilterInput?.Database?.Row.SearchByKey(Value);

        if (row == null) {
            FilterOutput.Clear();
            return;
        }

        FilterOutput.ChangeTo(new FilterItem(row));
    }

    private void UpdateMyCollection() {
        if (IsDisposed) { return; }
        if (!Allinitialized) { _ = CreateSubControls(); }

        this.DoInputFilter();

        #region Combobox suchen

        ComboBox? cb = null;
        foreach (var thiscb in Controls) {
            if (thiscb is ComboBox cbx) { cb = cbx; break; }
        }

        #endregion

        if (cb == null) { return; }

        List<AbstractListItem> ex = [.. cb.Item];

        #region Zeilen erzeugen

        //FilterInput = this.FilterOfSender();
        if (FilterInput == null) { return; }

        var f = FilterInput.Rows;
        foreach (var thisR in f) {
            if (cb?.Item?[thisR.KeyName] == null) {
                var tmpQuickInfo = thisR.ReplaceVariables(_showformat, true, true);
                _ = cb?.Item?.Add(tmpQuickInfo, thisR.KeyName);
                //cb.Item.Add(thisR, string.Empty);
            } else {
                foreach (var thisIt in ex) {
                    if (thisIt.KeyName == thisR.KeyName) {
                        _ = ex.Remove(thisIt);
                        break;
                    }
                }
            }
        }

        #endregion

        #region Veraltete Zeilen entfernen

        foreach (var thisit in ex) {
            cb?.Item?.Remove(thisit);
        }

        #endregion

        #region Nur eine Zeile? auswählen!

        // nicht vorher auf null setzen, um Blinki zu vermeiden
        if (cb?.Item != null && cb.Item.Count == 1) {
            ValueSet(cb.Item[0].KeyName, true, true);
        }

        if (cb?.Item == null || cb.Item.Count < 2) {
            DisabledReason = "Keine Auswahl möglich.";
        } else {
            DisabledReason = string.Empty;
        }

        #endregion

        #region  Prüfen ob die aktuelle Auswahl passt

        // am Ende auf null setzen, um Blinki zu vermeiden

        if (cb?.Item?[Value] == null) {
            ValueSet(string.Empty, true, true);
        }

        #endregion
    }

    #endregion
}