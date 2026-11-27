// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Globalization;
using System.IO;
using BlueTable.ColumnFormats;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Legt eine neue Tabellendatei mit deterministischen Testdaten an —
/// Muss-Spalten 'Erstspalte' und 'Unique', keine Chunk-Spalte; Inhalte mit
/// Sonderzeichen, Unicode, Umbrüchen und varianter Länge.
/// </summary>
public class TableCreateCliCommand : CliCommand {

    #region Fields

    /// <summary>
    /// Namen mit kritischen Zeichen: Umlaute, Anführungszeichen, Pipes, Unicode, Leerzeichen, leer.
    /// </summary>
    private static readonly string[] Names = [
        "Anna Achteinhalb",
        "Bernd Müller-Lüdenscheidt",
        "Céline Notre-Dame",
        "Dido \"Doublequote\" Build",
        "Emil Schulz",
        "Frida Kahlo García",
        "Günther von Hohenlohe-Waldenburg-Schillingsfürst",
        "何伟",
        "O'Brien",
        "a|b|c",
        "  nicht getrimmt  ",
        "Zoé ✓ 🚀",
        string.Empty,
    ];

    /// <summary>
    /// Notizen mit Zeilenumbrüchen und Sonderzeichen zum Üben der Mehrzeilen-Speicherung.
    /// </summary>
    private static readonly string[] Notes = [
        string.Empty,
        "Erste Zeile\r\nZweite Zeile\r\nDritte Zeile",
        "a\r\nb",
        "x\ny",
        "Anführung \"innen\"; Semikolon; Pipe | Tab\tEnde",
        "Mehrzeilig mit Ümläuten 🎉\r\nund Emojis 🚀\r\nEnde",
    ];

    /// <summary>
    /// Basisdatum der Zufallsdaten; die Tage kommen als Offset dazu.
    /// </summary>
    private static readonly DateTime BasisDate = new(2020, 1, 1);

    #endregion

    #region Properties

    public override string Command => "table-create";

    public override string? HelpDetails =>
            "Legt eine neue, leere Tabellendatei im Format der Ziel-Endung an und füllt sie mit deterministischen Testdaten. " +
            "Spalten: Erstspalte (erste Spalte), Unique (Unique-Definition) — beides Muss-Spalten — sowie Name, Anzahl, Preis, " +
            "Datum, Notiz und Langtext. Bewusst keine Chunk-Spalte. --rows steuert die Zeilenzahl (Standard 750, max 100000), " +
            "--seed den Daten-Zufall (Standard 1234): gleicher Seed liefert gleiche Inhalte. " +
            "Beispiel: bcr table-create Test.bdb --rows 800";

    public override List<string> Options => ["rows", "seed"];
    public override string Syntax => "bcr table-create <zieldatei> [--rows <anzahl>] [--seed <zahl>]";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird 1 Positionsargument (<zieldatei>), erhalten: {args.PositionalCount}.");
        }

        var targetPath = args[0] ?? string.Empty;

        if (!targetPath.IsValidFilepathAndName()) {
            Console.Error.WriteLine("Ungültige Zieldatei: " + targetPath);
            return 2;
        }

        if (!Path.IsPathRooted(targetPath)) {
            targetPath = Path.GetFullPath(targetPath);
        }

        var rowCount = 750;
        if (args.HasOption("rows")) {
            rowCount = IntParse(args.Option("rows") ?? string.Empty);

            if (rowCount is <= 0 or > 100000) {
                Console.Error.WriteLine("--rows erwartet eine Zahl zwischen 1 und 100000.");
                return 2;
            }
        }

        var seed = 1234;
        if (args.HasOption("seed")) {
            seed = IntParse(args.Option("seed") ?? string.Empty);
        }

        // Erst die leere Datei im Zielformat anlegen, dann öffnen und füllen:
        // Das Speichern läuft über den regulären Speicherweg des Datei-Formats.
        var opr = Table.Create(targetPath, null);

        if (opr.IsFailed) {
            Console.Error.WriteLine("Anlegen fehlgeschlagen: " + opr.FailedReason);
            return 1;
        }

        var tbl = Table.Get(targetPath);

        if (tbl is not { IsDisposed: false }) {
            Console.Error.WriteLine("Neue Tabelle nicht ladbar: " + targetPath);
            return 1;
        }

        var columns = CreateColumns(tbl);

        if (columns is null) {
            Console.Error.WriteLine("Spalten konnten nicht angelegt werden.");
            tbl.Dispose();
            return 1;
        }

        var failed = FillRows(tbl, columns, rowCount, new Random(seed));

        if (failed > 0) {
            Console.Error.WriteLine($"Abgebrochen: {failed} Zeilen fehlgeschlagen.");
            tbl.Dispose();
            return 1;
        }

        Console.Out.WriteLine($"Angelegt: {targetPath.NormalizeFile()} Zeilen={tbl.Row.Count} Spalten={tbl.Column.Count} Seed={seed}");
        return SaveTable(tbl);
    }

    /// <summary>
    /// Zellwert einer Spalte für die Zeile; deterministisch über Seed und Zeilenindex.
    /// Die KeyNames werden vom System großgeschrieben.
    /// </summary>
    private static string CellValueOf(string columnKey, int index, Random random) {
        switch (columnKey.ToUpperInvariant()) {
            case "ERSTSPALTE":
                return "ERST-" + index.ToString("00000", CultureInfo.InvariantCulture);

            case "UNIQUE":
                return "UNQ-" + index.ToString("00000", CultureInfo.InvariantCulture) + "-" + random.Next(0x10000).ToString("X4", CultureInfo.InvariantCulture);

            case "NAME":
                return Names[random.Next(Names.Length)];

            case "ANZAHL":
                return random.Next(100) < 5 ? string.Empty : random.Next(-5000, 500000).ToString(CultureInfo.InvariantCulture);

            case "PREIS":
                return random.Next(100) < 5 ? string.Empty : (random.NextDouble() * 10000).ToString("F2", CultureInfo.InvariantCulture);

            case "DATUM":
                return random.Next(100) < 10 ? string.Empty : BasisDate.AddDays(random.Next(0, 2500)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            case "NOTIZ":
                return RandomNote(random);

            case "LANGTEXT":
                return RandomLongText(random);

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Legt alle Spalten samt Muss-Markierungen an: 'Erstspalte' als erste Spalte,
    /// 'Unique' in einer Unique-Definition. Bewusst keine Chunk-Spalte.
    /// Liefert null, wenn eine Spalte nicht anlegbar war.
    /// </summary>
    private static List<ColumnItem>? CreateColumns(Table tbl) {
        List<(string Key, string Caption, string Format, string QuickInfo)> definition = [
            ("Erstspalte", "Erstspalte", "TextOneLine", "Muss-Spalte: Erste Spalte"),
            ("Unique", "Unique", "TextOneLine", "Muss-Spalte: eindeutiger Wert"),
            ("Name", "Name", "TextOneLine", string.Empty),
            ("Anzahl", "Anzahl", "Long", string.Empty),
            ("Preis", "Preis", "Float", string.Empty),
            ("Datum", "Datum", "Date", string.Empty),
            ("Notiz", "Notiz", "TextMultiline", "Umbrüche, Unicode, Pipes, leer"),
            ("Langtext", "Langtext", "TextMultiline", "Texte mit varianter Länge"),
        ];

        List<ColumnItem> result = [];

        foreach (var (key, caption, formatKey, quickinfo) in definition) {
            var column = tbl.Column.GenerateAndAdd(key, caption, FormatOf(formatKey), quickinfo);

            if (column is not { IsDisposed: false }) {
                Console.Error.WriteLine("Spalte konnte nicht angelegt werden: " + key);
                return null;
            }

            result.Add(column);
        }

        if (result[0] is { IsDisposed: false } first) {
            first.IsFirst = true;
        }

        // Die Unique-Spalte in eine Unique-Definition aufnehmen: Ab dann ist sie
        // Muss-Spalte beim Zeilen-Anlegen (Unique-Prüfung inklusive).
        var unique = tbl.Column["Unique"];

        if (unique is not { IsDisposed: false }) {
            Console.Error.WriteLine("Spalte 'Unique' fehlt nach dem Anlegen.");
            return null;
        }

        tbl.UniqueValues = new UniqueValueCollection(tbl, [new UniqueValueDefinition(tbl, [unique])]);

        return result;
    }

    /// <summary>
    /// Füllt die Tabelle zeilenweise über GenerateAndAdd (Muss-Spalten im Filter).
    /// Liefert die Anzahl fehlgeschlagener Zeilen.
    /// </summary>
    private static int FillRows(Table tbl, List<ColumnItem> columns, int rowCount, Random random) {
        var failed = 0;

        for (var i = 0; i < rowCount; i++) {
            List<FilterItem> filter = [];

            foreach (var column in columns) {
                filter.Add(new FilterItem(column, FilterType.Istgleich, CellValueOf(column.KeyName, i, random)));
            }

            var opr = tbl.Row.GenerateAndAdd([.. filter], "table-create Zeile " + (i + 1).ToString(CultureInfo.InvariantCulture));

            if (opr.IsFailed) {
                failed++;
                Console.Error.WriteLine($"Zeile {i + 1} fehlgeschlagen: {opr.FailedReason}");

                if (failed > 10) { break; }
            }
        }

        return failed;
    }

    /// <summary>
    /// Löst einen Format-Key (z. B. TextOneLine) zur Spaltenformat-Instanz auf.
    /// </summary>
    private static ColumnFormat? FormatOf(string formatKey) =>
                    ColumnFormat.AllFormats.Instances.FirstOrDefault(f => string.Equals(f.KeyName, formatKey, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Notiz aus dem Vorlagen-Pool, teils mit Zufallstext-Anhang; gelegentlich leer.
    /// </summary>
    private static string RandomNote(Random random) {
        if (random.Next(100) < 10) { return string.Empty; }

        var basis = Notes[random.Next(Notes.Length)];

        return random.Next(100) < 40 ? basis + "\r\n" + RandomText(random, random.Next(10, 300)) : basis;
    }

    /// <summary>
    /// Text varianter Länge: kurz, mittel oder lang (bis ~5000 Zeichen).
    /// </summary>
    private static string RandomLongText(Random random) {
        if (random.Next(100) < 10) { return string.Empty; }

        var zufall = random.Next(100);
        int length;

        if (zufall < 30) {
            length = random.Next(0, 60);
        } else if (zufall < 70) {
            length = random.Next(60, 600);
        } else {
            length = random.Next(600, 5000);
        }

        return RandomText(random, length);
    }

    /// <summary>
    /// Zufallstext mit Umlauten und gelegentlichen Umbrüchen.
    /// </summary>
    private static string RandomText(Random random, int length) {
        if (length <= 0) { return string.Empty; }

        const string Zeichen = "äöüßÄÖÜabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ,.;:!?-()\"'|_/";
        var sb = new StringBuilder(length);

        for (var i = 0; i < length; i++) {
            if (random.Next(50) == 0) {
                sb.Append("\r\n");
                continue;
            }

            sb.Append(Zeichen[random.Next(Zeichen.Length)]);
        }

        return sb.ToString();
    }

    #endregion
}
