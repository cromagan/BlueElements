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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

#nullable enable

namespace BlueControls.ConnectedFormula;

public partial class FlexiControlRowSelector : GenericControlReciver, IControlSendFilter, IDisposableExtended, IHasSettings {

    #region Fields

    private readonly string _showformat;

    #endregion

    #region Constructors

    public FlexiControlRowSelector(Database? database, string caption, string showFormat) : base(false, false) {
        f.CaptionPosition = CaptionPosition.Über_dem_Feld;
        f.EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;

        f.Caption = string.IsNullOrEmpty(caption) ? "Wählen:" : caption;
        _showformat = showFormat;

        if (string.IsNullOrEmpty(_showformat) && database != null && database.Column.Count > 0 && database.Column.First() is ColumnItem fc) {
            _showformat = "~" + fc.KeyName + "~";
        }
        ((IControlSendFilter)this).RegisterEvents();
        RegisterEvents();
    }

    #endregion

    #region Properties

    public CaptionPosition CaptionPosition { get => f.CaptionPosition; internal set => f.CaptionPosition = value; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<GenericControlReciver> Childs { get; } = [];

    public EditTypeFormula EditType { get => f.EditType; internal set => f.EditType = value; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; } = new("FilterOutput 04");

    public List<string> Settings { get; } = new();

    public bool SettingsLoaded { get; set; }
    public string SettingsManualFilename { get; set; }

    #endregion

    #region Methods

    public void FilterOutput_DispodingEvent(object sender, System.EventArgs e) => this.FilterOutput_DispodingEvent();

    public void FilterOutput_PropertyChanged(object sender, System.EventArgs e) => this.FilterOutput_PropertyChanged();

    public override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        if (!FilterInputChangedHandled) {
            FilterInputChangedHandled = true;
            this.DoInputFilter(FilterOutput.Database, true);
        }

        RowsInputChangedHandled = true;

        if (!f.Allinitialized) { _ = f.CreateSubControls(); }

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
                var tmpQuickInfo = thisR.ReplaceVariables(_showformat, true, true, null);
                cb.ItemAdd(ItemOf(tmpQuickInfo, thisR.KeyName));
            } else {
                ex.Remove(thisR.KeyName);
            }
        }

        #endregion

        #region Veraltete Zeilen entfernen

        foreach (var thisit in ex) {
            cb.Remove(thisit);
        }

        #endregion

        #region Nur eine Zeile? auswählen!

        // nicht vorher auf null setzen, um Blinki zu vermeiden
        if (cb.ItemCount == 1) {
            f.ValueSet(cb[0].KeyName, true);
        } else {
            var fh = this.GetSettings(this.FilterHash());

            if (!string.IsNullOrEmpty(fh) && cb.Items().Get(fh) is AbstractListItem ali) {
                f.ValueSet(ali.KeyName, true);
            }
        }

        if (cb.ItemCount < 2) {
            f.DisabledReason = "Keine Auswahl möglich.";
        } else {
            f.DisabledReason = string.Empty;
        }

        #endregion

        #region  Prüfen ob die aktuelle Auswahl passt

        // am Ende auf null setzen, um Blinki zu vermeiden

        if (cb[f.Value] == null) {
            f.ValueSet(string.Empty, true);
        }

        #endregion
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            ((IControlSendFilter)this).DoDispose();

            Tag = null;
        }

        base.Dispose(disposing);
    }

    private void F_ValueChanged(object sender, System.EventArgs e) {
        var fh = this.FilterHash();
        var row = RowsInput?.Get(f.Value);
        this.SetSetting(fh, row?.KeyName ?? string.Empty);

        if (row == null) {
            this.Invalidate_FilterOutput();
            return;
        }

        FilterOutput.ChangeTo(new FilterItem(row));
    }

    #endregion
}