// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.Linq;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ConnectedFormula;

public partial class FlexiControlRowSelector : GenericControlReciverSender, IHasSettings {

    #region Fields

    private readonly string _showformat;

    #endregion

    #region Constructors

    public FlexiControlRowSelector(Database? database, string caption, string showFormat) : base(false, false, false) {
        InitializeComponent();
        f.CaptionPosition = CaptionPosition.Über_dem_Feld;
        f.EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;

        f.Caption = string.IsNullOrEmpty(caption) ? "Wählen:" : caption;
        _showformat = showFormat;

        if (string.IsNullOrEmpty(_showformat) && database is { Column.Count: > 0 } && database.Column.First is { IsDisposed: false } fc) {
            _showformat = "~" + fc.KeyName + "~";
        }
    }

    #endregion

    #region Properties

    public CaptionPosition CaptionPosition { get => f.CaptionPosition; internal set => f.CaptionPosition = value; }
    public EditTypeFormula EditType { get => f.EditType; internal set => f.EditType = value; }
    public List<string> Settings { get; } = [];
    public bool SettingsLoaded { get; set; }
    public string SettingsManualFilename { get; set; } = string.Empty;
    public bool UsesSettings => true;

    #endregion

    #region Methods

    protected override void Dispose(bool disposing) {
        if (disposing) {
            Tag = null;
        }

        base.Dispose(disposing);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        if (!f.Allinitialized) { f.CreateSubControls(); }

        DoInputFilter(FilterOutput.Database, true);
        DoRows();

        #region Combobox suchen

        ComboBox? cb = null;
        foreach (var thiscb in f.Controls) {
            if (thiscb is ComboBox cbx) { cb = cbx; break; }
        }

        #endregion

        if (cb == null) { return; }

        var ex = cb.Items().ToList();

        #region Zeilen erzeugen

        if (RowsInput == null || !RowsInputChangedHandled) { return; }

        foreach (var thisR in RowsInput) {
            if (cb[thisR.KeyName] == null) {
                var tmpQuickInfo = thisR.ReplaceVariables(_showformat, true, null);
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
            f.ValueSet(cb[0]?.KeyName, true);
        } else {
            var fh = this.GetSettings(FilterHash());

            if (!string.IsNullOrEmpty(fh) && cb.Items().Get(fh) is { } ali) {
                f.ValueSet(ali.KeyName, true);
            }
        }

        f.DisabledReason = cb.ItemCount < 2 ? "Keine Auswahl möglich." : string.Empty;

        #endregion

        #region  Prüfen ob die aktuelle Auswahl passt

        // am Ende auf null setzen, um Blinki zu vermeiden

        if (cb[f.Value] == null) {
            f.ValueSet(string.Empty, true);
        }

        #endregion
    }

    private void F_ValueChanged(object sender, System.EventArgs e) {
        var fh = FilterHash();
        var row = RowsInput?.Get(f.Value);
        this.SetSetting(fh, row?.KeyName ?? string.Empty);

        if (row == null) {
            Invalidate_FilterOutput();
            return;
        }
        using var nfc = new FilterCollection(row, "FlexiControlRowSelector");

        FilterOutput.ChangeTo(nfc);
    }

    #endregion
}