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
using System.Linq;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.EventArgs;
using Form = BlueControls.Forms.Form;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

internal sealed partial class SearchAndReplace : Form {

    #region Fields

    private readonly Table _blueTable;
    private bool _isWorking;

    #endregion

    #region Constructors

    public SearchAndReplace(Table table) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        _blueTable = table;
        _blueTable.SelectedCellChanged += SelectedCellChanged;
        SelectedCellChanged(_blueTable, new CellExtEventArgs(_blueTable.CursorPosColumn, _blueTable.CursorPosRow));
    }

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        _blueTable.SelectedCellChanged -= SelectedCellChanged;
    }

    private void Alt_TextChange(object sender, System.EventArgs e) => Checkbuttons();

    private void Checkbuttons() {
        var canDo = true;
        if (_blueTable?.Database == null) { return; }
        if (!_blueTable.Database.IsAdministrator()) { canDo = false; }
        if (SucheNach.Checked) {
            if (string.IsNullOrEmpty(Alt.Text)) { canDo = false; }
            Alt.Enabled = true;
            ErsetzeMit.Enabled = true;
            NurinAktuellerSpalte.Enabled = true;
        } else if (SucheExact.Checked) {
            Alt.Enabled = true;
            ErsetzeMit.Enabled = false;
            if (ErsetzeMit.Checked) { canDo = false; }
            NurinAktuellerSpalte.Enabled = true;
        } else if (InhaltEgal.Checked) {
            Alt.Enabled = false;
            ErsetzeMit.Enabled = false;
            NurinAktuellerSpalte.Enabled = false; // Zu Riskant
            if (!NurinAktuellerSpalte.Checked) { canDo = false; }// Zu Riskant
            if (ErsetzeMit.Checked) { canDo = false; }
        } else {
            canDo = false;
        }
        if (ErsetzeMit.Checked) { } else if (ErsetzeKomplett.Checked) {
            NurinAktuellerSpalte.Enabled = false; // Zu Riskant
            if (!NurinAktuellerSpalte.Checked) { canDo = false; }// Zu Riskant
        } else if (FügeHinzu.Checked) {
            if (string.IsNullOrEmpty(Neu.Text)) { canDo = false; }
        } else {
            canDo = true;
        }
        if (NurinAktuellerSpalte.Checked) {
            if (_blueTable.CursorPosColumn == null) {
                canDo = false;
            } else {
                if (!_blueTable.CursorPosColumn.Format.CanBeCheckedByRules()) { canDo = false; }
            }
        }
        if (Alt.Text == Neu.Text) {
            if (!InhaltEgal.Checked) { canDo = false; }
            if (!ErsetzeKomplett.Checked) { canDo = false; }
        }
        ers.Enabled = canDo;
    }

    private void ers_Click(object sender, System.EventArgs e) {
        if (_isWorking) { return; }
        var suchText = Alt.Text.Replace(";cr;", "\r").Replace(";tab;", "\t");
        var ersetzText = Neu.Text.Replace(";cr;", "\r").Replace(";tab;", "\t");
        //db.OnConnectedControlsStopAllWorking(new MultiUserFileStopWorkingEventArgs());

        if (_blueTable?.Database is not DatabaseAbstract db) { return; }

        List<ColumnItem?> sp = new();
        List<RowItem> ro = new();
        if (NurinAktuellerSpalte.Checked) {
            sp.Add(_blueTable.CursorPosColumn);
        } else {
            sp.AddRange(db.Column.Where(thisColumn => thisColumn != null && thisColumn.Format.CanBeChangedByRules()));
        }
        foreach (var thisRow in db.Row) {
            if (!AktuelleFilterung.Checked || thisRow.MatchesTo(_blueTable.Filter)) {
                if (!AbgeschlosseZellen.Checked || !thisRow.CellGetBoolean(db.Column.SysLocked)) { ro.Add(thisRow); }
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

                if (SucheNach.Checked) {
                    trifft = originalText.Contains(suchText);
                } else if (SucheExact.Checked) {
                    trifft = originalText == suchText;
                } else if (InhaltEgal.Checked) {
                    trifft = true;
                }

                if (trifft) {
                    if (ErsetzeMit.Checked) {
                        geändeterText = originalText.Replace(suchText, ersetzText);
                    } else if (ErsetzeKomplett.Checked) {
                        geändeterText = ersetzText;
                    } else if (FügeHinzu.Checked) {
                        List<string> tmp = new(originalText.SplitAndCutByCr()) { ersetzText };
                        geändeterText = tmp.SortedDistinctList().JoinWithCr();
                    }
                    if (geändeterText != originalText) {
                        count++;
                        thisRow.CellSet(thiscolumn, geändeterText);
                    }
                }
            }
        }
        p?.Close();

        db.Row.ExecuteValueChanged();

        MessageBox.Show(count + " Ersetzung(en) vorgenommen.", ImageCode.Information, "OK");
        _isWorking = false;
    }

    private void SelectedCellChanged(object sender, CellExtEventArgs e) {
        NurinAktuellerSpalte.Text = e.Column == null ? "Nur in der <b>aktuell gewählten Spalte</b> ersetzen."
            : "Nur in Spalte <b>'" + e.Column.ReadableText() + "'</b> ersetzen.";
        Checkbuttons();
    }

    private void Something_CheckedChanged(object sender, System.EventArgs e) => Checkbuttons();

    #endregion
}