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
using BlueBasics.Enums;
using BlueControls.Forms;
using BlueDatabase;
using System.Collections.Generic;

namespace BlueControls.BlueDatabaseDialogs;

internal sealed partial class SearchAndReplaceInDBScripts : Form {

    #region Fields

    private bool _isWorking;

    #endregion

    #region Constructors

    public SearchAndReplaceInDBScripts() =>
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

    #endregion

    // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.//_table = table;//_table.SelectedCellChanged += SelectedCellChanged;//SelectedCellChanged(_table, new CellExtEventArgs(_table.CursorPosColumn, _table.CursorPosRow));

    #region Methods

    private void AltNeu_TextChanged(object sender, System.EventArgs e) => Checkbuttons();

    private void Checkbuttons() {
        var canDo = Generic.IsAdministrator();
        if (string.IsNullOrEmpty(txbAlt.Text)) { canDo = false; }
        btnAusfuehren.Enabled = canDo;
    }

    private void ers_Click(object sender, System.EventArgs e) {
        if (_isWorking) { return; }
        //var suchText = txbAlt.Text.Replace("\\r", "\r").Replace("\\t", "\t");
        //var ersetzText = txbNeu.Text.Replace("\\r", "\r").Replace("\\t", "\t");
        ////db.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

        //if (IsDisposed || _table.Database is not Database db || db.IsDisposed) { return; }

        //List<ColumnItem> sp = [];
        //List<RowItem> ro = [];
        //if (chkNurinAktuellerSpalte.Checked) {
        //    if (_table.CursorPosColumn is ColumnItem c) { sp.Add(c); }
        //} else {
        //    sp.AddRange(db.Column.Where(thisColumn => thisColumn != null && thisColumn.CanBeChangedByRules()));
        //}
        //foreach (var thisRow in db.Row) {
        //    if (!chkAktuelleFilterung.Checked || thisRow.MatchesTo(_table.Filter.ToArray()) || _table.PinnedRows.Contains(thisRow)) {
        //        if (db.Column.SysLocked is ColumnItem sl) {
        //            if (!chkAbgeschlosseZellen.Checked || !thisRow.CellGetBoolean(sl)) { ro.Add(thisRow); }
        //        }
        //    }
        //}
        var count = 0;
        //var geändeterText = string.Empty;
        //var co = 0;
        //var p = Progressbar.Show("Ersetze...", ro.Count);
        //foreach (var thisRow in ro) {
        //    co++;
        //    p.Update(co);
        //    foreach (var thiscolumn in sp) {
        //        var trifft = false;
        //        var originalText = thisRow.CellGetString(thiscolumn);

        //        if (optSucheNach.Checked) {
        //            trifft = originalText.Contains(suchText);
        //        } else if (optSucheExact.Checked) {
        //            trifft = originalText == suchText;
        //        } else if (optInhaltEgal.Checked) {
        //            trifft = true;
        //        }

        //        if (trifft) {
        //            if (optErsetzeMit.Checked) {
        //                geändeterText = originalText.Replace(suchText, ersetzText);
        //            } else if (optErsetzeKomplett.Checked) {
        //                geändeterText = ersetzText;
        //            } else if (optFügeHinzu.Checked) {
        //                List<string> tmp = [.. originalText.SplitAndCutByCr(), ersetzText];
        //                geändeterText = tmp.SortedDistinctList().JoinWithCr();
        //            }
        //            if (geändeterText != originalText) {
        //                count++;
        //                thisRow.CellSet(thiscolumn, geändeterText, "Suchen und Ersetzen");
        //            }
        //        }
        //    }
        //}
        //p.Close();

        ////db.Row.ExecuteValueChangedEvent(true);
        ///

        foreach (var thisDb in Database.AllFiles) {
            if (thisDb is { IsDisposed: false } db && !string.IsNullOrEmpty(db.Filename) && string.IsNullOrEmpty(db.EditableErrorReason(EditableErrorReasonType.EditAcut))) {
                List<DatabaseScriptDescription> updatedScripts = [];

                foreach (var thiss in db.EventScriptEdited) {
                    var neu = thiss.Script.Replace(txbAlt.Text, txbNeu.Text);
                    if (neu != thiss.Script) {
                        count++;
                        updatedScripts.Add(new DatabaseScriptDescription(thiss.AdminInfo, thiss.Image, thiss.KeyName, thiss.ColumnQuickInfo, neu, thiss.UserGroups, thiss.Database, thiss.EventTypes, thiss.NeedRow));
                    } else {
                        updatedScripts.Add(thiss);
                    }
                }
                db.EventScriptEdited = updatedScripts.AsReadOnly();
                db.EventScript = db.EventScriptEdited;
                db.NeedsScriptFix = string.Empty;
            }
        }

        MessageBox.Show(count + " Ersetzung(en) vorgenommen.", ImageCode.Information, "OK");
        _isWorking = false;
    }

    #endregion
}