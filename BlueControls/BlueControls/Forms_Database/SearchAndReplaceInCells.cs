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
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.Linq;

namespace BlueControls.BlueDatabaseDialogs;

internal sealed partial class SearchAndReplaceInCells : Form {

    #region Fields

    private readonly Table _table;
    private bool _isWorking;

    #endregion

    #region Constructors

    public SearchAndReplaceInCells(Table table) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        _table = table;
        _table.SelectedCellChanged += SelectedCellChanged;
        SelectedCellChanged(_table, new CellExtEventArgs(_table.CursorPosColumn, _table.CursorPosRow));
    }

    #endregion

    #region Methods

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        base.OnFormClosing(e);
        _table.SelectedCellChanged -= SelectedCellChanged;
    }

    private void AltNeu_TextChanged(object sender, System.EventArgs e) => Checkbuttons();

    private void Checkbuttons() {
        var canDo = true;
        if (IsDisposed || _table.Database is not Database db || db.IsDisposed) { return; }
        if (!db.IsAdministrator()) { canDo = false; }
        if (optSucheNach.Checked) {
            if (string.IsNullOrEmpty(txbAlt.Text)) { canDo = false; }
            txbAlt.Enabled = true;
            optErsetzeMit.Enabled = true;
            chkNurinAktuellerSpalte.Enabled = true;
        } else if (optSucheExact.Checked) {
            txbAlt.Enabled = true;
            optErsetzeMit.Enabled = false;
            if (optErsetzeMit.Checked) { canDo = false; }
            chkNurinAktuellerSpalte.Enabled = true;
        } else if (optInhaltEgal.Checked) {
            txbAlt.Enabled = false;
            optErsetzeMit.Enabled = false;
            chkNurinAktuellerSpalte.Enabled = false; // Zu Riskant
            if (!chkNurinAktuellerSpalte.Checked) { canDo = false; }// Zu Riskant
            if (optErsetzeMit.Checked) { canDo = false; }
        } else {
            canDo = false;
        }
        if (optErsetzeMit.Checked) { } else if (optErsetzeKomplett.Checked) {
            chkNurinAktuellerSpalte.Enabled = false; // Zu Riskant
            if (!chkNurinAktuellerSpalte.Checked) { canDo = false; }// Zu Riskant
        } else if (optFügeHinzu.Checked) {
            if (string.IsNullOrEmpty(txbNeu.Text)) { canDo = false; }
        } else {
            canDo = true;
        }
        if (chkNurinAktuellerSpalte.Checked) {
            if (_table.CursorPosColumn == null) {
                canDo = false;
            } else {
                if (!_table.CursorPosColumn.Function.CanBeCheckedByRules()) { canDo = false; }
            }
        }
        if (txbAlt.Text == txbNeu.Text) {
            if (!optInhaltEgal.Checked) { canDo = false; }
            if (!optErsetzeKomplett.Checked) { canDo = false; }
        }
        btnAusfuehren.Enabled = canDo;
    }

    private void ers_Click(object sender, System.EventArgs e) {
        if (_isWorking) { return; }
        var suchText = txbAlt.Text.Replace("\\r", "\r").Replace("\\t", "\t");
        var ersetzText = txbNeu.Text.Replace("\\r", "\r").Replace("\\t", "\t");
        //db.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

        if (IsDisposed || _table.Database is not Database db || db.IsDisposed) { return; }

        List<ColumnItem> sp = [];
        List<RowItem> ro = [];
        if (chkNurinAktuellerSpalte.Checked) {
            if (_table.CursorPosColumn is ColumnItem c) { sp.Add(c); }
        } else {
            sp.AddRange(db.Column.Where(thisColumn => thisColumn != null && thisColumn.Function.CanBeChangedByRules()));
        }
        foreach (var thisRow in db.Row) {
            if (!chkAktuelleFilterung.Checked || thisRow.MatchesTo(_table.Filter.ToArray()) || _table.PinnedRows.Contains(thisRow)) {
                if (db.Column.SysLocked is ColumnItem sl) {
                    if (!chkAbgeschlosseZellen.Checked || !thisRow.CellGetBoolean(sl)) { ro.Add(thisRow); }
                }
            }
        }
        var count = 0;
        var geändeterText = string.Empty;
        var co = 0;
        var p = Progressbar.Show("Ersetze...", ro.Count);
        foreach (var thisRow in ro) {
            co++;
            p.Update(co);
            foreach (var thiscolumn in sp) {
                var trifft = false;
                var originalText = thisRow.CellGetString(thiscolumn);

                if (optSucheNach.Checked) {
                    trifft = originalText.Contains(suchText);
                } else if (optSucheExact.Checked) {
                    trifft = originalText == suchText;
                } else if (optInhaltEgal.Checked) {
                    trifft = true;
                }

                if (trifft) {
                    if (optErsetzeMit.Checked) {
                        geändeterText = originalText.Replace(suchText, ersetzText);
                    } else if (optErsetzeKomplett.Checked) {
                        geändeterText = ersetzText;
                    } else if (optFügeHinzu.Checked) {
                        List<string> tmp = [.. originalText.SplitAndCutByCr(), ersetzText];
                        geändeterText = tmp.SortedDistinctList().JoinWithCr();
                    }
                    if (geändeterText != originalText) {
                        count++;
                        thisRow.CellSet(thiscolumn, geändeterText, "Suchen und Ersetzen");
                    }
                }
            }
        }
        p.Close();

        //db.Row.ExecuteValueChangedEvent(true);

        MessageBox.Show(count + " Ersetzung(en) vorgenommen.", ImageCode.Information, "OK");
        _isWorking = false;
    }

    private void SelectedCellChanged(object sender, CellExtEventArgs e) {
        chkNurinAktuellerSpalte.Text = e.Column == null ? "Nur in der <b>aktuell gewählten Spalte</b> ersetzen."
            : "Nur in Spalte <b>'" + e.Column.ReadableText() + "'</b> ersetzen.";
        Checkbuttons();
    }

    private void Something_CheckedChanged(object sender, System.EventArgs e) => Checkbuttons();

    #endregion
}