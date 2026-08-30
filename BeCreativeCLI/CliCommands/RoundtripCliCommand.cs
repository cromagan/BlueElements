// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.PadItems;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Roundtrip-Test: Lädt eine Datei, serialisiert sie in ihr Gegenstück-Format
/// (altes Format &lt;-&gt; JSON), lädt das Ergebnis erneut und vergleicht die Bytes
/// bit-genau mit dem Original. Die Ausgabe protokolliert jeden Schritt und
/// codiert Steuerzeichen als Escapes — damit das Ergebnis verlustfrei kopierbar
/// ist und Abweichungen ohne GUI analysiert werden können.
/// </summary>
public class RoundtripCliCommand : CliCommand {

    #region Properties

    public override string Command => "roundtrip";
    public override string Description => "Roundtrip-Test: lädt eine Datei (Layout .cfo/.bcr oder Tabelle .bdb/.tblj/.mbdb/.mtblj), speichert sie ins Gegenstück-Format, lädt sie zurück und vergleicht bit-genau mit dem Original.";
    public override List<string> Flags => ["full"];
    public override string Syntax => "bcr roundtrip <datei> [--full]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        var filename = args[0] ?? string.Empty;

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

            case "BDB":
            case "TBLJ":
            case "MBDB":
            case "MTBLJ":
                return ProcessTableFile(filename, suffix, origBytes, full);

            default:
                Console.Error.WriteLine("Unbekannter Dateityp: '" + suffix + "'. Unterstützt: .cfo, .bcr, .bdb, .tblj, .mbdb, .mtblj");
                return 2;
        }
    }

    private static string BytesToHex(byte[] bytes) {
        var sb = new StringBuilder(bytes.Length * 3);

        foreach (var b in bytes) {
            if (sb.Length > 0) { sb.Append(' '); }
            sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Wandelt Bytes in Win1252-Text um und escaped Steuerzeichen, damit die
    /// Ausgabe verlustfrei kopierbar ist.
    /// </summary>
    private static string BytesToPrintable(byte[] bytes) {
        try {
            return Win1252.GetString(bytes).EncodeControlChars();
        } catch {
            return "<binär nicht darstellbar>";
        }
    }

    /// <summary>
    /// Vergleicht Original und Roundtrip. Liefert 0 bei Identität, ansonsten 1.
    /// ZIP-Container (bdb/mbdb) werden entpackt verglichen, da ihre Eintrags-
    /// Zeitstempel und sonstigen ZIP-Metadaten keine Tabellendaten sind.
    /// </summary>
    private static int Compare(byte[] original, byte[] roundtrip, bool full) {
        if (original.SequenceEqual(roundtrip)) {
            Out(string.Empty);
            Out("*** ERFOLG: Beide Dateien sind bit-genau identisch. ***");
            return 0;
        }

        if (original.IsZipped() && roundtrip.IsZipped()
            && original.UnzipIt() is { } origMain
            && roundtrip.UnzipIt() is { } rtMain) {

            if (origMain.SequenceEqual(rtMain)) {
                Out(string.Empty);
                Out("*** ERFOLG: Der entpackte Inhalt (Main.bin) ist identisch. ***");
                Out("Die Dateien unterscheiden sich nur in den ZIP-Metadaten (z. B. Eintrags-Zeitstempel).");
                return 0;
            }

            Out(string.Empty);
            Out("*** WARNUNG: Der entpackte Inhalt (Main.bin) UNTERSCHIEDET SICH: ***");
            GenerateDiffReport(origMain, rtMain);

            if (full) {
                DumpFull("Original", original);
                DumpFull("Roundtrip", roundtrip);
            }
            return 1;
        }

        Out(string.Empty);
        Out("*** WARNUNG: Dateien unterscheiden sich! ***");
        GenerateDiffReport(original, roundtrip);

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
    /// Erzeugt eine TableFile-Instanz passend zum Suffix, gefüllt mit den Daten
    /// der Quelltabelle. Liefert null für unbekannte Suffixe.
    /// </summary>
    private static TableFile? CreateTableFile(string filename, string suffix, Table source) {
        switch (suffix.ToUpperInvariant()) {
            case "BDB":
                return new TableFile(filename, source);

            case "MBDB":
                return new TableFragments(filename, source);

            case "MTBLJ":
                return new TableJsonFragments(filename, source);

            case "TBLJ":
                return new TableJsonFile(filename, source);

            default:
                return null;
        }
    }

    /// <summary>
    /// Gibt den kompletten Inhalt aus, zerlegt an CR. Alle Steuerzeichen sind
    /// escaped (\r, \n, \t, \0). ZIP-Container werden zusätzlich entpackt ausgegeben.
    /// </summary>
    private static void DumpFull(string caption, byte[] bytes) {
        Out(string.Empty);
        Out("--- Kompletter Inhalt: " + caption + " (" + bytes.Length + " Bytes) ---");

        DumpLines(bytes);

        if (bytes.IsZipped() && bytes.UnzipIt() is { } main) {
            Out(string.Empty);
            Out("--- Kompletter Inhalt: " + caption + ", entpackt (Main.bin, " + main.Length + " Bytes) ---");
            DumpLines(main);
        }
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

    private static void GenerateDiffReport(byte[] original, byte[] roundtrip) {
        Out(string.Empty);
        Out("--- Diff-Analyse ---");
        Out("Längendifferenz (Roundtrip - Original): " + (roundtrip.Length - original.Length) + " Bytes");

        var minLen = Math.Min(original.Length, roundtrip.Length);
        var firstDiff = -1;

        for (var i = 0; i < minLen; i++) {
            if (original[i] != roundtrip[i]) {
                firstDiff = i;
                break;
            }
        }

        if (firstDiff < 0) {
            // Alle gemeinsamen Bytes sind identisch - der Unterschied liegt in der Länge
            if (original.Length < roundtrip.Length) {
                var extra = roundtrip[original.Length..];
                Out("Inhalt ist im gemeinsamen Bereich (0.." + (original.Length - 1) + ") identisch.");
                Out("Roundtrip hat zusätzlichen Inhalt ab Position " + original.Length + " (" + extra.Length + " Bytes):");
                Out(BytesToPrintable(extra));
            } else {
                var missing = original[roundtrip.Length..];
                Out("Inhalt ist im gemeinsamen Bereich (0.." + (roundtrip.Length - 1) + ") identisch.");
                Out("Roundtrip fehlt Inhalt ab Position " + roundtrip.Length + " (" + missing.Length + " Bytes):");
                Out(BytesToPrintable(missing));
            }
            return;
        }

        Out("Erste Abweichung an Byte-Position: " + firstDiff);

        // Kontext um die erste Abweichung: als Text (escaped) und als Hex-Dump
        var contextStart = Math.Max(0, firstDiff - 40);
        var contextEnd = Math.Min(minLen, firstDiff + 40);

        Out("Kontext Original (Bytes " + contextStart + ".." + (contextEnd - 1) + "):");
        Out("Text: " + BytesToPrintable(original[contextStart..contextEnd]));
        Out("Hex : " + BytesToHex(original[contextStart..contextEnd]));
        Out("Kontext Roundtrip:");
        Out("Text: " + BytesToPrintable(roundtrip[contextStart..contextEnd]));
        Out("Hex : " + BytesToHex(roundtrip[contextStart..contextEnd]));

        var diffCount = 0;

        for (var i = 0; i < minLen; i++) {
            if (original[i] != roundtrip[i]) { diffCount++; }
        }

        Out("Anzahl unterschiedlicher Bytes im gemeinsamen Bereich (0.." + (minLen - 1) + "): " + diffCount);
        Out("Größenunterschied: " + Math.Abs(original.Length - roundtrip.Length) + " Bytes");
    }

    /// <summary>
    /// Gegenstück-Format des Roundtrips: Binär &lt;-&gt; JSON,
    /// Fragment-Binär &lt;-&gt; Fragment-JSON.
    /// </summary>
    private static string JsonCounterpartOf(string suffix) {
        switch (suffix.ToUpperInvariant()) {
            case "MBDB":
                return "mtblj";

            case "MTBLJ":
                return "mbdb";

            case "TBLJ":
                return "bdb";

            default:
                return "tblj"; // BDB -> TBLJ
        }
    }

    private static void Out(string text) => Console.Out.WriteLine(text);

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

    /// <summary>
    /// Tabellen: Original laden, im Gegenstück-Format (JSON &lt;-&gt; Binär)
    /// speichern, neu laden, im Originalformat speichern, bit-genau vergleichen.
    /// </summary>
    private static int ProcessTableFile(string filename, string suffix, byte[] origBytes, bool full) {
        var jsonSuffix = JsonCounterpartOf(suffix);
        var baseName = filename.FileNameWithoutSuffix();
        // Windows-Temp, pro Lauf ein eigenes Unterverzeichnis: Der Tabellenname
        // folgt dem Dateinamen — würde TempFile bei Kollision einen Counter an den
        // Namen hängen, verfälscht das Key und Undo-Einträge (Schein-Diffs).
        var tempDir = Path.Combine(Path.GetTempPath(), "Roundtrip",
            DateTime.UtcNow.ToString("yyyyMMdd_HHmmssFF", CultureInfo.InvariantCulture));

        var tempJson = TempFile(tempDir, baseName + "_json", jsonSuffix);
        // Ausgabedatei mit dem ORIGINALNamen: Der Tabellenname folgt dem Dateinamen,
        // ein anderer Name würde die Undo-Einträge verfälschen und Schein-Diffs erzeugen.
        var tempOut = TempFile(tempDir, baseName, suffix.ToLowerInvariant());
        // Kopien wie beim Layout-Roundtrip: LiveInstanceCache dazu zwingen,
        // neu von Platte zu laden; Live-Instanzen unangetastet lassen.
        var origCopy = TempFile(tempDir, baseName + "_orig", suffix.ToLowerInvariant());
        var tempJsonCopy = TempFile(tempDir, baseName + "_jsoncp", jsonSuffix);

        try {
            #region Schritt 0: Originaldatei kopieren

            Out("--- Schritt 0: Originaldatei in Temp-Bereich kopieren ---");

            if (!FileCopy(filename, origCopy, false)) {
                Out("FEHLER: Originaldatei konnte nicht kopiert werden.");
                return 1;
            }

            #endregion

            #region Schritt 1 und 2: Original laden, im Gegenstück-Format speichern

            var origTable = Table.Get(origCopy);

            if (origTable is null) {
                Out("FEHLER: Tabelle konnte nicht geladen werden.");
                return 1;
            }

            using (origTable) {
                Out("--- Schritt 1: Original laden ---");
                Out("Zeilen : " + origTable.Row.Count);
                Out("Spalten: " + origTable.Column.Count);
                Out("Spalten-Ordnung: " + string.Join(", ", origTable.Column.Select(c => c.KeyName)));
                Out("Save-Ordnung   : " + string.Join(", ", origTable.ColumnsInSaveOrder().Select(c => c.KeyName)));

                Out(string.Empty);
                Out("--- Schritt 2: Als ." + jsonSuffix + " speichern ---");

                var jsonTable = CreateTableFile(tempJson, jsonSuffix, origTable);

                if (jsonTable is null) {
                    Out("FEHLER: Kein Tabellentyp für Suffix '" + jsonSuffix + "'.");
                    return 1;
                }

                using (jsonTable) {
                    Out("Ziel-Ordnung   : " + string.Join(", ", jsonTable.Column.Select(c => c.KeyName)));
                    var saveResult = jsonTable.Save();

                    if (saveResult.IsFailed) {
                        Out("FEHLER beim Speichern der " + jsonSuffix + "-Datei: " + (saveResult.FailedReason ?? "Unbekannter Fehler"));
                        return 1;
                    }
                }

                Out("Temp-Datei 1 (" + jsonSuffix + "): " + tempJson);
                Out(jsonSuffix.ToUpperInvariant() + "-Größe: " + (GetFileInfo(tempJson)?.Length ?? 0) + " Bytes");
            }

            #endregion

            #region Schritt 3 und 4: Neu laden, im Originalformat speichern

            if (!FileCopy(tempJson, tempJsonCopy, false)) {
                Out("FEHLER: Zwischendatei konnte nicht kopiert werden.");
                return 1;
            }

            var loadedTable = Table.Get(tempJsonCopy);

            if (loadedTable is null) {
                Out("FEHLER: " + jsonSuffix + "-Datei konnte nicht geladen werden.");
                return 1;
            }

            using (loadedTable) {
                Out(string.Empty);
                Out("--- Schritt 3: ." + jsonSuffix + " laden ---");
                Out("Geladene Zeilen : " + loadedTable.Row.Count);
                Out("Geladene Spalten: " + loadedTable.Column.Count);
                Out("Spalten-Ordnung: " + string.Join(", ", loadedTable.Column.Select(c => c.KeyName)));

                Out(string.Empty);
                Out("--- Schritt 4: Als ." + suffix.ToLowerInvariant() + " speichern ---");

                var outTable = CreateTableFile(tempOut, suffix, loadedTable);

                if (outTable is null) {
                    Out("FEHLER: Kein Tabellentyp für Suffix '" + suffix + "'.");
                    return 1;
                }

                using (outTable) {
                    var outResult = outTable.Save();

                    if (outResult.IsFailed) {
                        Out("FEHLER beim Speichern der Ausgabedatei: " + (outResult.FailedReason ?? "Unbekannter Fehler"));
                        return 1;
                    }
                }

                Out("Temp-Datei 2: " + tempOut);
            }

            #endregion

            #region Schritt 5: Bit-genauer Vergleich

            Out(string.Empty);
            Out("--- Schritt 5: Bit-Vergleich Original vs. Roundtrip ---");

            var tempResult = ReadAllBytes(tempOut, 10);

            if (tempResult.IsFailed || tempResult.Value is not byte[] outBytes) {
                Out("FEHLER: Ausgabedatei konnte nicht gelesen werden: " + (tempResult.FailedReason ?? "Unbekannter Fehler"));
                return 1;
            }

            Out("Original : " + origBytes.Length + " Bytes");
            Out("Roundtrip: " + outBytes.Length + " Bytes");

            var compareResult = Compare(origBytes, outBytes, full);

            Out(string.Empty);
            Out("Temp-Dateien (bleiben zur Analyse liegen):");
            Out("  JSON : " + tempJson);
            Out("  Roundtrip: " + tempOut);

            return compareResult;

            #endregion
        } finally {
            DeleteFile(origCopy, false);
            DeleteFile(tempJsonCopy, false);
        }
    }

    #endregion
}
