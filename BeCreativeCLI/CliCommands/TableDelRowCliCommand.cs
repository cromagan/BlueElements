// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Löscht alle adressierten Zeilen.
/// </summary>
public class TableDelRowCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-delrow";
    public override List<string> Flags => ["dry-run"];
    public override List<string> Options => [.. AddressingOptions, "password"];
    public override string Syntax => "bcr table-delrow <tabelle> + Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>]) [--dry-run]";

    public override string? HelpDetails => "--dry-run zeigt die Keys der Zeilen an, die gelöscht würden — ohne zu löschen und ohne zu speichern. " +
            "Abgeschlossene Zeilen (SYS_LOCKED) werden übersprungen.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 1) {
            return UsageError($"Erwartet wird genau 1 Positionsargument (<tabelle>), erhalten: {args.PositionalCount}.");
        }

        var problem = RowAddressingProblem(args);

        if (problem is not null) {
            Console.Error.WriteLine(problem);
            return 2;
        }

        var dryRun = args.Flag("dry-run");
        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        // Die CLI vergleicht ausschließlich die CLI-Rechte der Tabelle.
        var rightProblem = RightProblem(tbl, CliRights.DeleteRow);

        if (rightProblem is not null) {
            Console.Error.WriteLine(rightProblem);
            return 1;
        }

        var (rows, error) = ResolveRows(tbl, args);

        if (error is not null) {
            Console.Error.WriteLine(error);
            return 1;
        }

        if (rows.Count == 0) {
            Console.Error.WriteLine("Keine Zeile getroffen.");
            return 1;
        }

        if (dryRun) {
            var deletable = rows.Where(r => !IsLocked(null, r)).ToList();

            if (deletable.Count == 0) {
                Console.Error.WriteLine("Keine löschbare Zeile getroffen.");
                return 1;
            }

            Console.Out.WriteLine("Trockenlauf — gelöscht würden: " + string.Join(", ", deletable.Select(r => r.KeyName)));
            Console.Out.WriteLine($"{deletable.Count.ToString1()} Zeile(n), nichts gespeichert.");
            return 0;
        }

        // Erst nach allen Prüfungen den Fragment-Writer öffnen: Früh gescheiterte
        // Aufrufe sollen keine leere Fortsetzung mit EOF an die Fragment-Datei hängen.
        var fragmentProblem = FragmentEditProblem(tbl);

        if (fragmentProblem is not null) {
            Console.Error.WriteLine(fragmentProblem);
            return 2;
        }

        var deleted = 0;
        var failed = 0;
        var skipped = 0;

        foreach (var r in rows) {
            if (IsLocked(null, r)) {
                Console.Error.WriteLine($"Zeile {r.KeyName} ist abgeschlossen — übersprungen.");
                skipped++;
                continue;
            }

            var opr = RowCollection.Remove(r, "bcr table-delrow");

            if (opr.IsFailed) {
                Console.Error.WriteLine("Key: " + r.KeyName + " löschen fehlgeschlagen: " + opr.FailedReason);
                failed++;
            } else {
                Console.Out.WriteLine("Key: " + r.KeyName + " gelöscht");
                deleted++;
            }
        }

        if (skipped > 0) {
            Console.Error.WriteLine(skipped.ToString1() + " abgeschlossene Zeile(n) übersprungen.");
        }

        if (deleted > 0) {
            var sr = SaveTable(tbl);

            if (sr != 0) { return sr; }
        }

        return failed > 0 || deleted == 0 ? 1 : 0;
    }

    #endregion
}
