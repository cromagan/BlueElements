// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.PadItems;
namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Intern: Lädt eine Layout-Datei, serialisiert sie ins Gegenstück-Format (alt &lt;-&gt; JSON), lädt das Ergebnis erneut und vergleicht bit-genau mit dem Original.
/// </summary>
public class RoundtripCliCommand : CliCommand {

    #region Properties

    public override string Command => "roundtrip";
    public override List<string> Flags => ["full", "dev"];
    public override bool Hidden => true;
    public override string Syntax => "bcr roundtrip <datei> [--full]";

    public override string? HelpDetails =>
        "--full gibt bei Abweichung zusätzlich den kompletten Inhalt beider Dateien aus (Steuerzeichen escaped). " +
        "Unterstützt werden Layout-Dateien (.cfo/.bcr).";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<datei>), erhalten: {args.PositionalCount}.");
        }

        var filename = args[0] ?? string.Empty;

        // Relative Pfade auflösen — IO.FileExists verlangt gültige (absolute) Pfade.
        if (!filename.IsValidFilepathAndName()) {
            try {
                filename = Path.GetFullPath(filename);
            } catch {
                Console.Error.WriteLine("Datei nicht gefunden: " + filename);
                return 1;
            }
        }

        if (!FileExists(filename)) {
            Console.Error.WriteLine("Datei nicht gefunden: " + filename);
            return 1;
        }

        var origResult = ReadAllBytes(filename, 10);

        if (origResult.IsFailed || origResult.Value is not byte[] origBytes || origBytes.Length == 0) {
            Console.Error.WriteLine("Datei konnte nicht gelesen werden: " + (origResult.FailedReason ?? "Unbekannter Fehler"));
            return 1;
        }

        Out("=== Roundtrip-Test ===");
        Out("Datei: " + filename);
        Out("Originalgröße: " + origBytes.Length + " Bytes");
        Out(string.Empty);

        var suffix = filename.FileSuffix().ToUpperInvariant();
        var full = args.Flag("full");

        switch (suffix) {
            case "CFO":
            case "BCR":
                return ProcessLayoutFile(filename, suffix, origBytes, full);

            default:
                Console.Error.WriteLine("Unbekannter Dateityp: '" + suffix + "'. Unterstützt: .cfo, .bcr");
                return 2;
        }
    }

    /// <summary>
    /// Vergleicht Original und Roundtrip. Liefert 0 bei Identität, ansonsten 1.
    /// </summary>
    private static int Compare(byte[] original, byte[] roundtrip, bool full) {
        if (original.SequenceEqual(roundtrip)) {
            Out(string.Empty);
            Out("*** ERFOLG: Beide Dateien sind bit-genau identisch. ***");
            return 0;
        }

        Out(string.Empty);
        Out("*** WARNUNG: Dateien unterscheiden sich! ***");
        TableRoundtripCliCommand.GenerateDiffReport(original, roundtrip);

        if (full) {
            DumpFull("Original", original);
            DumpFull("Roundtrip", roundtrip);
        } else {
            Out(string.Empty);
            Out("Hinweis: 'bcr roundtrip <datei> --full' gibt den kompletten Inhalt beider Dateien aus (Steuerzeichen escaped).");
        }

        return 1;
    }

    /// <summary>
    /// Gibt den kompletten Inhalt aus, zerlegt an CR. Alle Steuerzeichen sind
    /// escaped (\r, \n, \t, \0).
    /// </summary>
    private static void DumpFull(string caption, byte[] bytes) {
        Out(string.Empty);
        Out("--- Kompletter Inhalt: " + caption + " (" + bytes.Length + " Bytes) ---");

        DumpLines(bytes);
    }

    private static void DumpLines(byte[] bytes) {
        var s = Win1252.GetString(bytes);
        var start = 0;

        for (var i = 0; i <= s.Length; i++) {
            if (i < s.Length && s[i] != '\r') { continue; }

            Out(start.ToString(CultureInfo.InvariantCulture).PadLeft(6, '0') + ": " + s[start..i].EncodeControlChars());
            start = i + 1;
        }
    }

    /// <summary>
    /// CFO/BCR: altes Format laden, als JSON speichern, JSON laden, wieder als
    /// altes Format speichern, bit-genau vergleichen.
    /// </summary>
    private static int ProcessLayoutFile(string filename, string suffix, byte[] origBytes, bool full) {
        var tempDir = Path.GetTempPath();
        var baseName = filename.FileNameWithoutSuffix();
        var origSuffix = suffix.ToLowerInvariant();

        var tempJson = TempFile(tempDir, baseName + "_json", "json");
        var tempOld = TempFile(tempDir, baseName + "_rt", origSuffix);
        // Kopien von Original und JSON-Datei, damit die LiveInstanceCache-Register
        // garantiert einen Cache-Miss haben und neu von Platte laden. Live-Instanzen
        // des Original-Pfads dürfen weder gelesen noch (via Dispose) zerstört werden.
        var origCopy = TempFile(tempDir, baseName + "_orig", origSuffix);
        var tempJsonCopy = TempFile(tempDir, baseName + "_jsoncp", "json");

        try {
            #region Schritt 0: Originaldatei kopieren

            Out("--- Schritt 0: Originaldatei in Temp-Bereich kopieren ---");

            if (!FileCopy(filename, origCopy, false)) {
                Out("FEHLER: Originaldatei konnte nicht kopiert werden.");
                return 1;
            }

            #endregion

            #region Schritt 1: Altes Format laden

            Out(string.Empty);
            Out("--- Schritt 1: Altes Format laden ---");

            JsonObject json;

            if (suffix == "CFO") {
                var cf = ConnectedFormula.Get(origCopy);

                if (cf is null) {
                    Out("FEHLER: ConnectedFormula konnte nicht geladen werden.");
                    return 1;
                }

                using (cf) {
                    _ = cf.Pages; // Triggert Lazy-Parse des alten Formats
                    Out("Anzahl Pages: " + cf.Pages.Count);
                    json = cf.ParseableJson();
                }
            } else {
                // BCR: wird über den CollectionPadItem-Konstruktor geladen,
                // der intern ConnectedFormula.Get + ParseableItems.Parse nutzt.
                var layout = new CollectionPadItem(origCopy);

                using (layout) {
                    Out("Anzahl Items: " + layout.Count());
                    json = layout.ParseableJson();
                }
            }

            Out("Schritt 1 OK - JSON-Objekt erzeugt.");

            #endregion

            #region Schritt 2: JSON in Temp-Datei speichern

            Out(string.Empty);
            Out("--- Schritt 2: JSON in Temp-Datei speichern ---");

            var jsonText = json.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            WriteAllText(tempJson, jsonText, Win1252, false);

            Out("Temp-Datei 1 (JSON): " + tempJson);
            Out("JSON-Zeichen: " + jsonText.Length);

            #endregion

            #region Schritt 3: JSON laden, wieder als altes Format serialisieren

            Out(string.Empty);
            Out("--- Schritt 3: JSON laden, wieder als altes Format serialisieren ---");

            if (!FileCopy(tempJson, tempJsonCopy, false)) {
                Out("FEHLER: JSON-Datei konnte nicht kopiert werden.");
                return 1;
            }

            var loadedJsonText = ReadAllText(tempJsonCopy, Win1252);
            using var doc = JsonDocument.Parse(loadedJsonText);

            string oldFormatStr;

            if (suffix == "CFO") {
                var cf2 = ConnectedFormula.Get(tempJsonCopy);

                if (cf2 is null) {
                    Out("FEHLER: ConnectedFormula für Temp-Datei konnte nicht erzeugt werden.");
                    return 1;
                }

                using (cf2) {
                    cf2.ParseJson(doc.RootElement);
                    oldFormatStr = cf2.ParseableItems().FinishParseable();
                }
            } else {
                var layout2 = new CollectionPadItem();

                using (layout2) {
                    layout2.ParseJson(doc.RootElement);
                    oldFormatStr = layout2.ParseableItems().FinishParseable();
                }
            }

            Out("Schritt 3 OK - altes Format generiert.");
            Out("Alt-Format-Zeichen: " + oldFormatStr.Length);

            #endregion

            #region Schritt 4: Altes Format in zweite Temp-Datei speichern

            Out(string.Empty);
            Out("--- Schritt 4: Altes Format in zweite Temp-Datei speichern ---");

            var oldBytes = Win1252.GetBytes(oldFormatStr);
            WriteAllBytes(tempOld, oldBytes);

            Out("Temp-Datei 2 (alt): " + tempOld);
            Out("Alt-Format-Bytes: " + oldBytes.Length);

            #endregion

            #region Schritt 5: Bit-genauer Vergleich

            Out(string.Empty);
            Out("--- Schritt 5: Bit-Vergleich Original vs. Roundtrip ---");
            Out("Original : " + origBytes.Length + " Bytes");
            Out("Roundtrip: " + oldBytes.Length + " Bytes");

            return Compare(origBytes, oldBytes, full);

            #endregion
        } finally {
            DeleteFile(tempJson, false);
            DeleteFile(tempOld, false);
            DeleteFile(origCopy, false);
            DeleteFile(tempJsonCopy, false);
        }
    }

    #endregion
}
