// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Konvertiert Legacy-Tabellen (.bdb/.nbdb/.mbdb/.tblh) ins neue Format (.btb) —
/// eine einzelne Datei oder alle in einem Verzeichnis (rekursiv).
/// </summary>
public class TableConvertCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-convert";

    public override string? HelpDetails =>
            "Konvertiert direkt an den Pfaden. Bei .tblh werden die Row-Chunks danach einzeln nachgezogen; " +
            "bereits vorhandene Zieldateien werden übersprungen — der Lauf ist fortsetzbar. " +
            "Beispiel: bcr table-convert C:\\Tabellen";

    public override string Syntax => "bcr table-convert <tabelle-oder-verzeichnis>";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird 1 Positionsargument (<tabelle-oder-verzeichnis>), erhalten: {args.PositionalCount}.");
        }

        var path = args[0] ?? string.Empty;

        List<string> heads;

        if (FileExists(path)) {
            if (!TableFileConverter.LegacySuffixes.Contains(path.FileSuffix(), StringComparer.OrdinalIgnoreCase)) {
                Console.Error.WriteLine("Keine Legacy-Tabelle (Endung '" + path.FileSuffix() + "'): " + path);
                return 2;
            }

            heads = [path];
        } else if (DirectoryExists(path)) {
            heads = TableFileConverter.FindLegacyTables(path);

            if (heads.Count == 0) {
                Console.Out.WriteLine("Keine Legacy-Tabellen gefunden in: " + path);
                return 0;
            }
        } else {
            Console.Error.WriteLine("Pfad nicht gefunden: " + path);
            return 2;
        }

        var fehler = 0;

        foreach (var head in heads) {
            var result = TableFileConverter.ConvertTable(head, null, l => Console.Out.WriteLine("  " + l));
            Console.Out.WriteLine(head.FileNameWithSuffix() + ": " + result);

            if (result.StartsWith("Fehler", StringComparison.Ordinal)) { fehler++; }
        }

        if (fehler > 0) {
            Console.Error.WriteLine("Konvertierung mit Fehlern: " + fehler.ToString1() + " von " + heads.Count.ToString1() + " Tabellen");
            return 1;
        }

        Console.Out.WriteLine("Konvertiert: " + heads.Count.ToString1() + " Tabellen");
        return 0;
    }

    #endregion
}
