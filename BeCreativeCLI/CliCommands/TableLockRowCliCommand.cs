// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Sperrt oder entsperrt alle adressierten Zeilen über die Systemspalte SYS_LOCKED und speichert die Tabelle.
/// </summary>
public class TableLockRowCliCommand : CliCommand {

    #region Properties

    public override string Command => "table-lockrow";

    public override List<string> Flags => ["dry-run", "lock", "unlock"];

    public override string? HelpDetails => "--lock sperrt die adressierten Zeilen (CLI-Recht '" + CliRights.LockRow + "'), --unlock hebt die Sperre auf (CLI-Recht '" + CliRights.RemoveRowLock + "'); genau einer der beiden Schalter ist Pflicht. " +
            "Zeilen, die den Zielzustand bereits haben, werden übersprungen. " +
            "--dry-run zeigt nur die Keys der Zeilen, die geändert würden — ohne zu ändern und ohne zu speichern.";

    public override List<string> Options => [.. AddressingOptions, "password"];

    public override string Syntax => "bcr table-lockrow <tabelle> --lock | --unlock + Zeilenadressierung (--rowkey <key> oder --filtercolumn <spalte> --filtervalue <wert> [--filtertype <typ>]) [--dry-run] [--password <kennwort>]";

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

        var lockIt = args.Flag("lock");
        var unlockIt = args.Flag("unlock");

        // Genau einer von beiden.
        if (lockIt == unlockIt) {
            return UsageError("Genau einer der Schalter --lock oder --unlock muss angegeben werden.");
        }

        var dryRun = args.Flag("dry-run");
        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        if (tbl.Column.SysLocked is not { IsDisposed: false } lockColumn) {
            Console.Error.WriteLine("Die Tabelle '" + tbl.KeyName + "' hat keine Sperrspalte (SYS_LOCKED) — Zeilen können nicht gesperrt werden.");
            return 1;
        }

        // Die CLI vergleicht ausschließlich die CLI-Rechte der Tabelle.
        var rightProblem = RightProblem(tbl, lockIt ? CliRights.LockRow : CliRights.RemoveRowLock);

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

        var action = lockIt ? "gesperrt" : "entsperrt";

        if (dryRun) {
            var changeable = rows.Where(r => r.CellGetBoolean(lockColumn) != lockIt).ToList();

            if (changeable.Count == 0) {
                Console.Error.WriteLine("Alle getroffenen Zeilen haben den Zielzustand bereits.");
                return 1;
            }

            Console.Out.WriteLine("Trockenlauf — " + action + " würde: " + string.Join(", ", changeable.Select(r => r.KeyName)));
            Console.Out.WriteLine($"{changeable.Count.ToString1()} Zeile(n), nichts gespeichert.");
            return 0;
        }

        // Erst nach allen Prüfungen den Fragment-Writer öffnen: Früh gescheiterte
        // Aufrufe sollen keine leere Fortsetzung mit EOF an die Fragment-Datei hängen.
        var fragmentProblem = FragmentEditProblem(tbl);

        if (fragmentProblem is not null) {
            Console.Error.WriteLine(fragmentProblem);
            return 2;
        }

        var changed = 0;
        var failed = 0;
        var skipped = 0;

        foreach (var row in rows) {
            if (row.CellGetBoolean(lockColumn) == lockIt) {
                Console.Error.WriteLine($"Zeile {row.KeyName} ist bereits {action} — übersprungen.");
                skipped++;
                continue;
            }

            var failedText = row.CellSet(lockColumn, lockIt, "bcr table-lockrow");

            if (!string.IsNullOrEmpty(failedText)) {
                Console.Error.WriteLine($"Zeile {row.KeyName} konnte nicht {action} werden: {failedText}");
                failed++;
            } else {
                Console.Out.WriteLine("Zeile " + row.KeyName + " " + action + ".");
                changed++;
            }
        }

        if (skipped > 0) {
            Console.Error.WriteLine(skipped.ToString1() + " Zeile(n) im Zielzustand übersprungen.");
        }

        if (changed > 0) {
            var sr = SaveTable(tbl);

            if (sr != 0) { return sr; }
        }

        return failed > 0 || changed == 0 ? 1 : 0;
    }

    #endregion
}
