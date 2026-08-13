// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionPad;
using BlueControls.Controls.ConnectedFormula;
using static BlueBasics.ClassesStatic.IO;

namespace BlueControls.Forms;

/// <summary>
/// Form zum Testen der JSON-Serialisierung. Lädt eine CFO- oder BCR-Datei
/// im alten Format, speichert sie als JSON, lädt das JSON wieder und speichert
/// es erneut im alten Format. Die resultierenden Bytes werden bit-genau mit
/// der Originaldatei verglichen und etwaige Abweichungen protokolliert.
/// </summary>
public sealed partial class JsonRoundtripTestForm : Form {

    #region Constructors

    public JsonRoundtripTestForm() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
    }

    #endregion

    #region Methods

    [StandaloneInfo("JSON-Roundtrip-Test", ImageCode.Kontrast, "Admin", "Testet CFO/BCR-Dateien auf Bit-Identität nach JSON-Roundtrip und protokolliert Abweichungen.", 950)]
    public static System.Windows.Forms.Form Start() => new JsonRoundtripTestForm();

    private void Append(string text) {
        if (IsDisposed || !IsHandleCreated) { return; }

        if (InvokeRequired) {
            BeginInvoke(new Action(() => Append(text)));
            return;
        }

        txbProtocol.Text += text + "\r\n";
        txbProtocol.Refresh();
    }

    private static string BytesToPrintable(byte[] bytes) {
        try {
            var s = Win1252.GetString(bytes);
            return s.EncodeControlChars();
        } catch {
            return "<binär nicht darstellbar>";
        }
    }

    private void btnSelectFile_Click(object sender, System.EventArgs e) {
        FormManager.SaveAllFiles();

        dlgOpen.ShowDialog();
        if (string.IsNullOrEmpty(dlgOpen.FileName)) { return; }

        ExecuteRoundtrip(dlgOpen.FileName);
    }

    private void ExecuteRoundtrip(string filename) {
        txbProtocol.Text = string.Empty;

        Append("=== JSON-Roundtrip-Test ===");
        Append("Datei: " + filename);
        Append(string.Empty);

        var origResult = ReadAllBytes(filename, 10);
        if (origResult.IsFailed || origResult.Value is not byte[] origBytes || origBytes.Length == 0) {
            Append("FEHLER: Originaldatei konnte nicht gelesen werden:");
            Append(origResult.FailedReason ?? "Unbekannter Fehler");
            return;
        }

        Append("Originalgröße: " + origBytes.Length + " Bytes");
        Append(string.Empty);

        var suffix = filename.FileSuffix().ToUpperInvariant();

        switch (suffix) {
            case "CFO":
            case "BCR":
                ProcessLayoutFile(filename, suffix, origBytes);
                break;

            case "BDB":
                ProcessTableFile(filename, origBytes);
                break;

            default:
                Append("Unbekannter Dateityp: '" + suffix + "'");
                Append("Unterstützt werden: .cfo, .bcr");
                break;
        }

        Append(string.Empty);
        Append("=== Test beendet ===");
    }

    private void GenerateDiffReport(byte[] original, byte[] roundtrip) {
        Append(string.Empty);
        Append("--- Diff-Analyse ---");
        Append("Längendifferenz (Roundtrip - Original): " + (roundtrip.Length - original.Length) + " Bytes");

        var minLen = Math.Min(original.Length, roundtrip.Length);
        var firstDiff = -1;
        for (var i = 0; i < minLen; i++) {
            if (original[i] != roundtrip[i]) {
                firstDiff = i;
                break;
            }
        }

        if (firstDiff < 0) {
            // Alle gemeinsamen Bytes sind identisch - Unterschied liegt in der Länge
            if (original.Length < roundtrip.Length) {
                Append("Inhalt ist im gemeinsamen Bereich (0.." + (original.Length - 1) + ") identisch.");
                Append("Roundtrip hat zusätzlichen Inhalt ab Position " + original.Length + ":");
                var extra = roundtrip[original.Length..];
                Append("Zusätzliche Bytes: " + extra.Length);
                Append("Inhalt: " + BytesToPrintable(extra));
            } else {
                Append("Inhalt ist im gemeinsamen Bereich (0.." + (roundtrip.Length - 1) + ") identisch.");
                Append("Roundtrip fehlt Inhalt ab Position " + roundtrip.Length + ":");
                var missing = original[roundtrip.Length..];
                Append("Fehlende Bytes: " + missing.Length);
                Append("Inhalt: " + BytesToPrintable(missing));
            }
            return;
        }

        Append("Erste Abweichung an Byte-Position: " + firstDiff);

        // Kontext um die erste Abweichung herum anzeigen
        var contextStart = Math.Max(0, firstDiff - 40);
        var contextEnd = Math.Min(minLen, firstDiff + 40);

        Append("Kontext Original (Bytes " + contextStart + ".." + (contextEnd - 1) + "):");
        Append(BytesToPrintable(original[contextStart..contextEnd]));
        Append("Kontext Roundtrip:");
        Append(BytesToPrintable(roundtrip[contextStart..contextEnd]));

        // Anzahl unterschiedlicher Bytes im gemeinsamen Bereich
        var diffCount = 0;
        for (var i = 0; i < minLen; i++) {
            if (original[i] != roundtrip[i]) { diffCount++; }
        }

        Append("Anzahl unterschiedlicher Bytes im gemeinsamen Bereich (0.." + (minLen - 1) + "): " + diffCount);
        Append("Größenunterschied: " + Math.Abs(original.Length - roundtrip.Length) + " Bytes");
    }

    private void ProcessLayoutFile(string filename, string suffix, byte[] origBytes) {
        var tempDir = System.IO.Path.GetTempPath();
        var baseName = filename.FileNameWithoutSuffix();
        var origSuffix = suffix.ToLowerInvariant();

        var tempJson = TempFile(tempDir, baseName + "_json", "json");
        var tempOld = TempFile(tempDir, baseName + "_rt", origSuffix);
        // Kopien von Original und JSON-Datei, damit die LiveInstanceCache-Register
        // von ConnectedFormula garantiert einen Cache-Miss haben und neu von
        // Platte laden. Der Original-Pfad könnte bereits als Live-Instanz
        // irgendwoanders in Benutzung sein — deren Inhalt darf der Test weder
        // lesen noch (via Dispose im using) zerstören.
        var origCopy = TempFile(tempDir, baseName + "_orig", origSuffix);
        var tempJsonCopy = TempFile(tempDir, baseName + "_jsoncp", "json");

        try {
            #region Schritt 0: Originaldatei kopieren

            Append("--- Schritt 0: Originaldatei in Temp-Bereich kopieren ---");
            Append("Kopiere '" + filename + "' -> '" + origCopy + "'");

            if (!FileCopy(filename, origCopy, false)) {
                Append("FEHLER: Originaldatei konnte nicht kopiert werden.");
                return;
            }

            Append("Kopie erstellt.");
            Append(string.Empty);

            #endregion

            #region Schritt 1: Altes Format laden

            Append("--- Schritt 1: Altes Format laden ---");

            JsonObject? json;

            if (suffix == "CFO") {
                var cf = ConnectedFormula.Get(origCopy);
                if (cf is null) {
                    Append("FEHLER: ConnectedFormula konnte nicht geladen werden.");
                    return;
                }

                using (cf) {
                    _ = cf.Pages; // Triggert Lazy-Parse des alten Formats
                    Append("Anzahl Pages: " + cf.Pages.Count);
                    json = cf.ParseableJson();
                }
            } else {
                // BCR: wird über den ItemCollectionPadItem-Konstruktor geladen,
                // der intern ConnectedFormula.Get + ParseableItems.Parse nutzt.
                var layout = new ItemCollectionPadItem(origCopy);
                using (layout) {
                    Append("Anzahl Items: " + layout.Count());
                    json = layout.ParseableJson();
                }
            }

            Append("Schritt 1 OK - JSON-Objekt erzeugt.");
            Append(string.Empty);

            #endregion

            #region Schritt 2: JSON in Temp-Datei speichern

            Append("--- Schritt 2: JSON in Temp-Datei speichern ---");

            var jsonText = json.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            WriteAllText(tempJson, jsonText, Win1252, false);

            Append("Temp-Datei 1 (JSON): " + tempJson);
            Append("JSON-Zeichen: " + jsonText.Length);
            Append(string.Empty);

            #endregion

            #region Schritt 3: JSON laden, wieder als altes Format serialisieren

            Append("--- Schritt 3: JSON laden, wieder als altes Format serialisieren ---");

            // tempJson kopieren, damit auch dieser Load garantiert neu von Platte
            // liest (sonst liefert bei Test-Wiederholung der LiveInstanceCache
            // die vorige ConnectedFormula-Instanz zurück).
            Append("Kopiere '" + tempJson + "' -> '" + tempJsonCopy + "'");

            if (!FileCopy(tempJson, tempJsonCopy, false)) {
                Append("FEHLER: JSON-Datei konnte nicht kopiert werden.");
                return;
            }

            Append("Kopie erstellt.");
            Append(string.Empty);

            var loadedJsonText = ReadAllText(tempJsonCopy, Win1252);
            using var doc = JsonDocument.Parse(loadedJsonText);

            string oldFormatStr;

            if (suffix == "CFO") {
                var cf2 = ConnectedFormula.Get(tempJsonCopy);
                if (cf2 is null) {
                    Append("FEHLER: ConnectedFormula für Temp-Datei konnte nicht erzeugt werden.");
                    return;
                }

                using (cf2) {
                    cf2.ParseJson(doc.RootElement); // Extension: ParseJson + ParseFinishedJson
                    oldFormatStr = cf2.ParseableItems().FinishParseable();
                }
            } else {
                var layout2 = new ItemCollectionPadItem();
                using (layout2) {
                    layout2.ParseJson(doc.RootElement); // Extension: ParseJson + ParseFinishedJson
                    oldFormatStr = layout2.ParseableItems().FinishParseable();
                }
            }

            Append("Schritt 3 OK - altes Format generiert.");
            Append("Alt-Format-Zeichen: " + oldFormatStr.Length);
            Append(string.Empty);

            #endregion

            #region Schritt 4: Altes Format in zweite Temp-Datei speichern

            Append("--- Schritt 4: Altes Format in zweite Temp-Datei speichern ---");

            var oldBytes = Win1252.GetBytes(oldFormatStr);
            WriteAllBytes(tempOld, oldBytes);

            Append("Temp-Datei 2 (alt): " + tempOld);
            Append("Alt-Format-Bytes: " + oldBytes.Length);
            Append(string.Empty);

            #endregion

            #region Schritt 5: Bit-genauer Vergleich

            Append("--- Schritt 5: Bit-Vergleich Original vs. Roundtrip ---");
            Append("Original : " + origBytes.Length + " Bytes");
            Append("Roundtrip: " + oldBytes.Length + " Bytes");

            if (origBytes.SequenceEqual(oldBytes)) {
                Append(string.Empty);
                Append("*** ERFOLG: Beide Dateien sind bit-genau identisch. ***");
            } else {
                Append(string.Empty);
                Append("*** WARNUNG: Dateien unterscheiden sich! ***");
                GenerateDiffReport(origBytes, oldBytes);
            }

            #endregion
        } finally {
            DeleteFile(tempJson, false);
            DeleteFile(tempOld, false);
            DeleteFile(origCopy, false);
            DeleteFile(tempJsonCopy, false);
        }
    }

    /// <summary>
    /// BDB-Roundtrip über den tblj-Umweg:
    /// 1. Original-BDB laden
    /// 2. Als .tblj (JSON) speichern
    /// 3. .tblj wieder laden
    /// 4. Als .bdb (binär) speichern
    /// 5. Original-Bytes mit der neu erzeugten BDB vergleichen.
    /// </summary>
    private void ProcessTableFile(string filename, byte[] origBytes) {
        var tempDir = System.IO.Path.GetTempPath();
        var baseName = filename.FileNameWithoutSuffix();
        var tempJson = TempFile(tempDir, baseName + "_json", "tblj");
        var tempBdb = TempFile(tempDir, baseName + "_rt", "bdb");
        // Kopien von Original und JSON-Datei, damit das LiveInstanceCache-Register
        // von Table garantiert einen Cache-Miss hat und neu von Platte lädt.
        // Der Original-Pfad könnte bereits als Live-Instanz in Benutzung sein —
        // deren Inhalt darf der Test weder lesen noch verfälschen.
        var origCopy = TempFile(tempDir, baseName + "_orig", "bdb");
        var tempJsonCopy = TempFile(tempDir, baseName + "_jsoncp", "tblj");

        try {
            #region Schritt 0: Originaldatei kopieren

            Append("--- Schritt 0: Originaldatei in Temp-Bereich kopieren ---");
            Append("Kopiere '" + filename + "' -> '" + origCopy + "'");

            if (!FileCopy(filename, origCopy, false)) {
                Append("FEHLER: Originaldatei konnte nicht kopiert werden.");
                return;
            }

            Append("Kopie erstellt.");
            Append(string.Empty);

            #endregion

            #region Schritt 1: BDB laden

            Append("--- Schritt 1: BDB laden ---");

            var origTable = Table.Get(origCopy);
            if (origTable is null) {
                Append("FEHLER: Tabelle konnte nicht geladen werden.");
                return;
            }

            Append("Zeilen : " + origTable.Row.Count);
            Append("Spalten: " + origTable.Column.Count);
            Append(string.Empty);

            #endregion

            #region Schritt 2: Als .tblj (JSON) speichern

            Append("--- Schritt 2: Als .tblj (JSON) speichern ---");

            var jsonTable = new TableJsonFile(tempJson, origTable);
            var saveResult = jsonTable.Save();
            if (saveResult.IsFailed) {
                jsonTable.Dispose();
                Append("FEHLER beim Speichern der tblj-Datei:");
                Append(saveResult.FailedReason ?? "Unbekannter Fehler");
                return;
            }
            jsonTable.Dispose();

            Append("Temp-Datei 1 (tblj): " + tempJson);
            Append("tblj-Größe: " + (GetFileInfo(tempJson)?.Length ?? 0) + " Bytes");
            Append(string.Empty);

            #endregion

            #region Schritt 3: .tblj laden

            Append("--- Schritt 3: .tblj (JSON) laden ---");

            // tempJson kopieren, damit auch dieser Load garantiert neu von Platte
            // liest (sonst liefert bei Test-Wiederholung der LiveInstanceCache
            // die vorige Table-Instanz zurück).
            Append("Kopiere '" + tempJson + "' -> '" + tempJsonCopy + "'");

            if (!FileCopy(tempJson, tempJsonCopy, false)) {
                Append("FEHLER: JSON-Datei konnte nicht kopiert werden.");
                return;
            }

            Append("Kopie erstellt.");
            Append(string.Empty);

            var loadedTable = Table.Get(tempJsonCopy);
            if (loadedTable is null) {
                Append("FEHLER: tblj-Datei konnte nicht geladen werden.");
                return;
            }

            Append("Geladene Zeilen : " + loadedTable.Row.Count);
            Append("Geladene Spalten: " + loadedTable.Column.Count);
            Append(string.Empty);

            #endregion

            #region Schritt 4: Als .bdb (binär) speichern

            Append("--- Schritt 4: Als .bdb (binär) speichern ---");

            var bdbTable = new TableFile(tempBdb, loadedTable);
            var bdbResult = bdbTable.Save();
            if (bdbResult.IsFailed) {
                bdbTable.Dispose();
                Append("FEHLER beim Speichern der bdb-Datei:");
                Append(bdbResult.FailedReason ?? "Unbekannter Fehler");
                return;
            }
            bdbTable.Dispose();

            Append("Temp-Datei 2 (bdb): " + tempBdb);
            Append(string.Empty);

            #endregion

            #region Schritt 5: Bit-Vergleich

            Append("--- Schritt 5: Bit-Vergleich Original vs. Roundtrip ---");

            var tempResult = ReadAllBytes(tempBdb, 10);
            if (tempResult.IsFailed || tempResult.Value is not byte[] tempBytes) {
                Append("FEHLER: Temp-BDB konnte nicht gelesen werden:");
                Append(tempResult.FailedReason ?? "Unbekannter Fehler");
                return;
            }

            Append("Original : " + origBytes.Length + " Bytes");
            Append("Roundtrip: " + tempBytes.Length + " Bytes");

            if (origBytes.SequenceEqual(tempBytes)) {
                Append(string.Empty);
                Append("*** ERFOLG: Beide Dateien sind bit-genau identisch. ***");
            } else {
                Append(string.Empty);
                Append("*** WARNUNG: Dateien unterscheiden sich! ***");
                GenerateDiffReport(origBytes, tempBytes);
            }

            #endregion
        } finally {
            DeleteFile(tempJson, false);
            DeleteFile(tempBdb, false);
            DeleteFile(origCopy, false);
            DeleteFile(tempJsonCopy, false);
        }
    }

    #endregion
}
