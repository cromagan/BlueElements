// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueScript.Interfaces;

namespace BlueControls.BlueTableDialogs;

internal sealed partial class SearchAndReplaceInTbScripts : Form, IUniqueWindow {

    #region Fields

    private bool _isWorking;

    #endregion

    #region Constructors

    public SearchAndReplaceInTbScripts() =>
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

    #endregion

    #region Properties

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object? Object { get; set; }

    #endregion

    // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.//_table = table;//_table.SelectedCellChanged += SelectedCellChanged;//SelectedCellChanged(_table, new CellExtEventArgs(_table.CursorPosColumn, _table.CursorPosRow));

    #region Methods

    private void AltNeu_TextChanged(object sender, System.EventArgs e) => Checkbuttons();

    private void btnErsetzen_Click(object sender, System.EventArgs e) {
        if (_isWorking) { return; }
        _isWorking = true;

        var count = 0;
        var suchText = txbAlt.Text.DecodeControlChars();
        var ersetzText = txbNeu.Text.DecodeControlChars();

        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is { IsDisposed: false } tb && !TableViewForm.EditableErrorMessage(tb, null)) {
                count += tb.ReplaceInScripts(suchText, ersetzText);
            }
        }

        foreach (var (tb, col, strategie) in ScriptVerwendung.SpaltenSkripte()) {
            if (strategie is not IHasScript hasScript) { continue; }
            if (tb is not { IsDisposed: false } || !TableViewForm.EditableErrorMessage(tb, null)) { continue; }

            var changed = hasScript.ReplaceInScripts(suchText, ersetzText);
            if (changed <= 0) { continue; }

            // Geänderten Strategie-Zustand zur Spalte zurückschreiben (löst ChangeData aus)
            col.ControlStrategyParameter = strategie.ControlStrategyParameter;
            count += changed;
        }

        foreach (var (formular, _, element) in ScriptVerwendung.FormularSkripte()) {
            if (formular.BlockerMessage() is { Length: > 0 }) { continue; }
            count += element.ReplaceInScripts(suchText, ersetzText);
        }

        MessageBox.Show(count + " Skript(e) bearbeitet.", ImageCode.Information, "OK");
        _isWorking = false;
    }

    private void btnFehler_Click(object sender, System.EventArgs e) {
        if (_isWorking) { return; }
        _isWorking = true;

        var count = 0;

        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is { IsDisposed: false } tb && !TableViewForm.EditableErrorMessage(tb, null)) {
                foreach (var script in tb.EventScript.ToList()) { // ToList() für sichere Iteration
                    if (!string.IsNullOrEmpty(script.FailedReason)) {
                        count++;
                        Table.UpdateScript(script, failedReason: string.Empty, stoppedtimecount: Math.Min(20, script.StoppedTimeCount));
                    }
                }
            }
        }

        MessageBox.Show(count + " Skript(e) bearbeitet.", ImageCode.Information, "OK");
        _isWorking = false;
    }

    private void btnVerwendung_Click(object sender, System.EventArgs e) => ScriptVerwendung.Show();

    private void btnSuche_Click(object sender, System.EventArgs e) {
        if (_isWorking) { return; }
        _isWorking = true;

        var txt = new List<string>();
        var suchText = txbAlt.Text.DecodeControlChars();

        foreach (var (ort, skript) in ScriptVerwendung.CollectScripts()) {
            if (!skript.Contains(suchText)) { continue; }

            // Fundort zur Liste hinzufügen
            txt.Add(string.Empty);
            txt.Add(string.Empty);
            txt.Add("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            txt.Add(ort);

            // Alle Zeilen durchgehen und die mit dem Suchtext zur Liste hinzufügen
            var lines = skript.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                if (line.Contains(suchText)) {
                    txt.Add(line.Trim());
                }
            }
        }

        var tv = new TextView(string.Join("\r\n", txt), "Skript-Suche");
        tv.Show();
        _isWorking = false;
    }

    private void Checkbuttons() {
        var canDo = IsAdministrator();
        if (string.IsNullOrEmpty(txbAlt.Text)) { canDo = false; }
        btnErsetzen.Enabled = canDo;
        btnSuche.Enabled = canDo;
    }

    #endregion
}
