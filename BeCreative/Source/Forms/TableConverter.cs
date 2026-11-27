// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Classes;
using BlueControls.Forms;
using BlueTable.ClassesStatic;
using System.Linq;
using static BlueBasics.ClassesStatic.Develop;
using static BlueBasics.ClassesStatic.IO;

namespace BeCreative;

/// <summary>
/// Konvertiert alle Legacy-Tabellen eines Verzeichnisses in das neue Tabellen-Format (.btb).
/// Die Konvertierung selbst erledigt <see cref="TableFileConverter"/> — fortsetzbar und
/// parallel auf mehreren Rechnern ausführbar.
/// </summary>
public sealed partial class TableConverter : Form {

    #region Fields

    /// <summary>
    /// Die Progressbar des laufenden Konvertier-Laufs; Grundlage der Abbruch-Abfrage.
    /// </summary>
    private Progressbar? _progressbar;

    #endregion

    #region Constructors

    public TableConverter() : base() => InitializeComponent();

    #endregion

    #region Methods

    [StandaloneInfo("Tabellen-Konverter", ImageCode.Tabelle, "Admin", "Legacy-Tabellen (.bdb/.nbdb/.mbdb/.tblh) ins neue Format (.btb) konvertieren — fortsetzbar und parallel ausführbar", 900)]
    public static System.Windows.Forms.Form Start() => new TableConverter();

    private void BtnVerzeichnis_Click(object sender, System.EventArgs e) {
        if (folderBrowserDialog.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) { return; }

        txtVerzeichnis.Text = folderBrowserDialog.SelectedPath;
    }

    private void BtnStart_Click(object sender, System.EventArgs e) {
        var dir = txtVerzeichnis.Text.Trim();

        if (dir is not { Length: > 0 } || !DirectoryExists(dir)) {
            Notification.Show("Bitte ein gültiges Verzeichnis wählen.", ImageCode.Warnung);
            return;
        }

        var heads = TableFileConverter.FindLegacyTables(dir);

        if (heads.Count == 0) {
            capStatus.Text = "Keine Legacy-Tabellen gefunden.";
            return;
        }

        btnStart.Enabled = false;
        txtLog.Text = string.Empty;

        var p = Progressbar.Show("Konvertiere Tabellen...", heads.Count);
        p.CancelSupported = true;
        _progressbar = p;

        var done = 0;
        var fehler = 0;

        foreach (var head in heads) {
            if (IsClosing || IsClosed || p.IsCancelRequested) { break; }

            capStatus.Text = head.FileNameWithSuffix();
            DoEvents();

            var result = TableFileConverter.ConvertTable(head, IstAbgebrochen, LogLine);

            if (result.StartsWith("Fehler", StringComparison.Ordinal)) { fehler++; }

            LogLine(head.FileNameWithSuffix() + ": " + result);

            done++;
            p.Update(done);
            DoEvents();
        }

        _progressbar = null;
        p.Close();

        capStatus.Text = "Fertig: " + done.ToString1() + " von " + heads.Count.ToString1() + " Tabellen"
            + (fehler > 0 ? ", Fehler: " + fehler.ToString1() : string.Empty)
            + (p.IsCancelRequested ? " (abgebrochen)" : string.Empty);

        btnStart.Enabled = true;
    }

    /// <summary>
    /// True, sobald der Benutzer den Abbrechen-Button der Progressbar geklickt hat.
    /// </summary>
    private bool IstAbgebrochen() => _progressbar?.IsCancelRequested == true;

    /// <summary>
    /// Hängt eine Zeile an das Protokoll an; der Bestand wird auf die letzten 400 Zeilen begrenzt.
    /// </summary>
    private void LogLine(string text) {
        if (IsDisposed || IsClosing) { return; }

        var lines = txtLog.Text is { Length: > 0 } t ? t.Split("\r\n").ToList() : [];

        lines.Add(text);
        txtLog.Text = string.Join("\r\n", lines[Math.Max(0, lines.Count - 400)..]);
        txtLog.Refresh();
    }

    #endregion
}
