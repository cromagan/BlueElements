// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueTable.Interfaces;
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

            if (_tableView is not null) {
                _tableView.SelectedCellChanged -= SelectedCellChanged;
                _tableView.TableChanged -= TableChanged;
            }

            _tableView = newt;

            if (_tableView is not null) {
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

    /// <summary>
    /// Ermittelt die Spalten, in denen Ersetzungen durchgeführt werden sollen.
    /// Entweder nur die aktuell gewählte Spalte oder alle änderbaren Spalten.
    /// </summary>
    private List<ColumnItem> CollectTargetColumns(Table tb) {
        if (chkNurinAktuellerSpalte.Checked) {
            if (_tableView?.CursorPosColumn?.Column is { IsDisposed: false } column) {
                return [column];
            }
            return [];
        }
        return [.. tb.Column.Where(c => c?.CanBeChangedByRules() == true)];
    }

    /// <summary>
    /// Ermittelt die Zeilen, die verarbeitet werden sollen.
    /// Abgeschlossene Zeilen werden übersprungen, sofern nicht explizit eingeschlossen.
    /// </summary>
    private List<RowItem> CollectTargetRows(Table tb) {
        IEnumerable<RowItem> sourceRows = chkAktuelleFilterung.Checked && _tableView is not null
            ? _tableView.RowsVisibleUnique()
            : tb.Row;

        if (tb.Column.SysLocked is not { IsDisposed: false } sl) {
            // Keine Sperrspalte vorhanden -> alle Zeilen einbeziehen
            return [.. sourceRows];
        }

        List<RowItem> targetRows = [];
        foreach (var row in sourceRows) {
            // Überspringen wenn abgeschlossen und nicht explizit eingeschlossen
            if (!chkAbgeschlosseZellen.Checked && row.CellGetBoolean(sl)) { continue; }
            targetRows.Add(row);
        }
        return targetRows;
    }

    /// <summary>
    /// Berechnet den neuen Zellinhalt basierend auf der gewählten Ersetzungsart.
    /// </summary>
    private string ComputeReplacementText(string originalText, string suchText, string ersetzText) {
        if (optErsetzeMit.Checked) { return originalText.Replace(suchText, ersetzText); }
        if (optErsetzeKomplett.Checked) { return ersetzText; }
        if (optFügeHinzu.Checked) {
            List<string> entries = [.. originalText.SplitAndCutByCr(), ersetzText];
            return string.Join('\r', entries.SortedDistinctList());
        }
        return originalText;
    }

    private void ers_Click(object sender, System.EventArgs e) {
        if (_isWorking) { return; }
        _isWorking = true;

        // Escape-Sequenzen in den Eingabefeldern auflösen
        var suchText = txbAlt.Text.Replace("\\r", "\r").Replace("\\t", "\t");
        var ersetzText = txbNeu.Text.Replace("\\r", "\r").Replace("\\t", "\t");

        if (IsDisposed || _tableView?.Table is not { IsDisposed: false } tb) {
            _isWorking = false;
            return;
        }

        // Zielspalten sammeln
        var columns = CollectTargetColumns(tb);

        // Zielzeilen sammeln, dabei abgeschlossene Zellen berücksichtigen
        var targetRows = CollectTargetRows(tb);

        // Ersetzungen durchführen
        var replaceCount = 0;
        var progress = Progressbar.Show("Ersetze...", targetRows.Count);

        for (var i = 0; i < targetRows.Count; i++) {
            progress.Update(i + 1);

            foreach (var column in columns) {
                var originalText = targetRows[i].CellGetString(column);
                if (!MatchesSearchCriteria(originalText, suchText)) { continue; }

                var newText = ComputeReplacementText(originalText, suchText, ersetzText);
                if (newText == originalText) { continue; }

                replaceCount++;
                targetRows[i].CellSet(column, newText, "Suchen und Ersetzen");
            }
        }

        progress.Close();
        Forms.MessageBox.Show(replaceCount + " Ersetzung(en) vorgenommen.", ImageCode.Information, "OK");
        _isWorking = false;
    }

    /// <summary>
    /// Prüft, ob der Zellinhalt zur Suchbedingung passt.
    /// </summary>
    private bool MatchesSearchCriteria(string cellText, string suchText) {
        if (optSucheNach.Checked) { return cellText.Contains(suchText); }
        if (optSucheExact.Checked) { return cellText == suchText; }
        if (optInhaltEgal.Checked) { return true; }
        return false;
    }

    private void SelectedCellChanged(object? sender, CellExtEventArgs e) {
        chkNurinAktuellerSpalte.Text = e.ColumnView is null ? "Nur in der <b>aktuell gewählten Spalte</b> ersetzen."
            : "Nur in Spalte <b>'" + e.ColumnView.ReadableText() + "'</b> ersetzen.";
        Checkbuttons();
    }

    private void Something_CheckedChanged(object sender, System.EventArgs e) => Checkbuttons();

    private void TableChanged(object? sender, System.EventArgs e) => Close();

    #endregion
}