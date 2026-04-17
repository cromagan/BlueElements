// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueTable.Classes;
using BlueTable.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Form = BlueControls.Forms.Form;

namespace BlueControls.BlueTableDialogs;

internal sealed partial class SearchAndReplaceInCells : Form, IUniqueWindow, IHasTable {

    #region Fields

    private bool _isWorking;
    private TableView? _tableView;

    #endregion

    #region Constructors

    public SearchAndReplaceInCells() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
    }

    #endregion

    #region Properties

    public object? Object {
        get => _tableView;

        set {
            var newt = value as TableView;

            if (_tableView != null) {
                _tableView.SelectedCellChanged -= SelectedCellChanged;
                _tableView.TableChanged -= TableChanged;
            }

            _tableView = newt;

            if (_tableView != null) {
                _tableView.SelectedCellChanged += SelectedCellChanged;
                _tableView.TableChanged += TableChanged;
                SelectedCellChanged(_tableView, new CellExtEventArgs(_tableView.CursorPosColumn, _tableView.CursorPosRow));
            }
        }
    }

    public Table? Table => _tableView?.Table;

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        Object = null;
    }

    private void AltNeu_TextChanged(object sender, System.EventArgs e) => Checkbuttons();

    private void Checkbuttons() {
        var canDo = true;
        if (IsDisposed || _tableView?.Table is not { IsDisposed: false } tb) { return; }

        if (!tb.IsAdministrator()) { canDo = false; }

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
            if (_tableView?.CursorPosColumn?.Column is not { IsDisposed: false } column) {
                canDo = false;
            } else {
                if (!column.CanBeCheckedByRules()) { canDo = false; }
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

        if (IsDisposed || _tableView?.Table is not { IsDisposed: false } tb) { return; }

        List<ColumnItem> sp = [];
        List<RowItem> ro = [];
        if (chkNurinAktuellerSpalte.Checked) {
            if (_tableView.CursorPosColumn?.Column is { IsDisposed: false } column) { sp.Add(column); }
        } else {
            sp.AddRange(tb.Column.Where(thisColumn => thisColumn?.CanBeChangedByRules() == true));
        }
        foreach (var thisRow in tb.Row) {
            if (!chkAktuelleFilterung.Checked || thisRow.MatchesTo([.. _tableView.FilterCombined]) || _tableView.PinnedRows.Contains(thisRow)) {
                if (tb.Column.SysLocked is { IsDisposed: false } sl) {
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
                        geändeterText = string.Join('\r', tmp.SortedDistinctList());
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

        Forms.MessageBox.Show(count + " Ersetzung(en) vorgenommen.", ImageCode.Information, "OK");
        _isWorking = false;
    }

    private void SelectedCellChanged(object? sender, CellExtEventArgs e) {
        chkNurinAktuellerSpalte.Text = e.ColumnView == null ? "Nur in der <b>aktuell gewählten Spalte</b> ersetzen."
            : "Nur in Spalte <b>'" + e.ColumnView.ReadableText() + "'</b> ersetzen.";
        Checkbuttons();
    }

    private void Something_CheckedChanged(object sender, System.EventArgs e) => Checkbuttons();

    private void TableChanged(object? sender, System.EventArgs e) => Close();

    #endregion
}