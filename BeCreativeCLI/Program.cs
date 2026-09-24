// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BeCreativeCLI;

internal static class Program {

    #region Methods

    [STAThread]
    private static int Main(string[] args) {
        // UTF-8 ohne BOM, damit Ausgaben in Pipes und Dateien sauber ankommen.
        try {
            Console.OutputEncoding = new UTF8Encoding(false);
        } catch {
            // Ohne nutzbare Konsole (z. B. geerbte Handles) die Standardausgabe belassen.
        }

        // stdin wird nicht umgestellt: Die Befehle lesen keine Standardeingabe.

        StartService();
        // Die CLI ist kein Tabellen-Benutzer: Erlaubte Aktionen stehen als CLI-Rechte
        // (Texte) in der Tabelle und werden direkt als String verglichen — nicht über
        // Benutzergruppen. Der Namenspräfix dient der Zuordnung von Fragment-Dateien.
        UserName = "CLI_" + UserName;
        MessageDG += Program_MessageDG;
        // Headless: Die CLI bearbeitet in jedem Fall alle Zeilen — kein Abbruch durch Benutzeraktivität oder Zeitlimit.
        RowCollection.AllowProcessingAborts = false;

        if (args.Length == 0) {
            Console.Error.WriteLine("BeCreative (bcr) — © 2026 Christian Peter, cp33@gmx.de");
            Console.Error.WriteLine("Kommandozeilen-Werkzeug für BeCreative-Dateien — Tabellen (Befehlspräfix: table-) und der Roundtrip-Test für Layout- und Tabellendateien (roundtrip).");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Benutzung: bcr <befehl> [optionen]");
            Console.Error.WriteLine("  bcr help              Listet alle verfügbaren Befehle auf.");
            Console.Error.WriteLine("  bcr help <befehl>     Zeigt Syntax, Beschreibung, Schalter und Beispiele eines Befehls.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Quellcode und Lizenz (AGPL-3.0): https://github.com/cromagan/BlueElements");
            return 2;
        }

        var cmd = CliCommand.ByName(args[0]);

        if (cmd is null) {
            Console.Error.WriteLine($"Unbekannter Befehl: '{args[0]}' — 'bcr help' listet alle Befehle.");
            return 2;
        }

        var cliArgs = new CliArgs(args.Skip(1), cmd.Flags, cmd.Options);

        if (cliArgs.ParseError is { Length: > 0 } parseError) {
            Console.Error.WriteLine(cmd.Syntax);
            Console.Error.WriteLine(parseError);
            return 2;
        }

        return cmd.DoIt(cliArgs);
    }

    private static void Program_MessageDG(ErrorType type, object? reference, string category, ImageCode symbol, string message, int indent) {
        // Info und DevelopInfo unterdrücken, nur echte Probleme auf stderr ausgeben.
        if (type > ErrorType.Warning) { return; }

        Console.Error.WriteLine(type + ": " + message);
    }

    #endregion
}