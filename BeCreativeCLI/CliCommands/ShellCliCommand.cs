// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BeCreativeCLI.CliCommands;

public class ShellCliCommand : CliCommand {

    #region Properties

    public override string Command => "shell";
    public override string Description => "Liest Befehle zeilenweise von der Konsole (oder einer Pipe). Geladene Dateien — aktuell Tabellen — bleiben in der Session offen: eine Fragment-Datei pro Tabelle statt pro Befehl.";
    public override string Syntax => "bcr shell";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount != 0) {
            Console.Error.WriteLine(Syntax);
            return 2;
        }

        var interactive = !Console.IsInputRedirected;
        SessionActive = true;
        var exitCode = 0;

        try {
            if (interactive) {
                Console.Error.WriteLine("Session gestartet. 'exit' oder 'quit' beendet, '#' kommentiert eine Zeile aus.");
            }

            while (true) {
                if (interactive) { Console.Error.Write("bcr> "); }

                var line = Console.ReadLine();

                if (line is null) { break; }

                line = line.Trim();

                if (line.Length == 0 || line.StartsWith('#')) { continue; }

                if (string.Equals(line, "exit", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(line, "quit", StringComparison.OrdinalIgnoreCase)) { break; }

                var (tokens, error) = Tokenize(line);

                if (error is { Length: > 0 } err) {
                    Console.Error.WriteLine(err);
                    continue;
                }

                if (tokens.Count == 0) { continue; }

                if (string.Equals(tokens[0], "shell", StringComparison.OrdinalIgnoreCase)) {
                    Console.Error.WriteLine("Eine Session kann nicht verschachtelt werden.");
                    continue;
                }

                var cmd = ByName(tokens[0]);

                if (cmd is null) {
                    Console.Error.WriteLine($"Unbekannter Befehl: '{tokens[0]}' — 'help' listet alle Befehle.");
                    continue;
                }

                var cmdArgs = new CliArgs(tokens.Skip(1), cmd.Flags);

                if (cmdArgs.ParseError is { Length: > 0 } parseError) {
                    Console.Error.WriteLine(cmd.Syntax);
                    Console.Error.WriteLine(parseError);
                    continue;
                }

                var code = cmd.DoIt(cmdArgs);

                if (code != 0 && exitCode == 0) { exitCode = code; }
            }
        } finally {
            SessionActive = false;
        }

        // Datenüberprüfung aller geänderten Zeilen — vor dem Entladen der in der Session offenen Tabellen.
        RowCollection.ExecuteValueChangedEvent();
        RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(null, true, null);

        foreach (var tbl in Table.AllInstances()) {
            if (tbl is { IsDisposed: false } live) { live.Dispose(); }
        }

        return exitCode;
    }

    /// <summary>
    /// Zerlegt eine Eingabezeile in Token. Anführungszeichen gruppieren Werte,
    /// ein doppeltes Anführungszeichen ("") ergibt ein einzelnes im Wert.
    /// </summary>
    private static (List<string> Tokens, string? Error) Tokenize(string line) {
        List<string> tokens = [];
        var sb = new StringBuilder();
        var inQuotes = false;
        var hasToken = false;

        for (var i = 0; i < line.Length; i++) {
            var c = line[i];

            if (inQuotes) {
                if (c == '"') {
                    if (i + 1 < line.Length && line[i + 1] == '"') {
                        sb.Append('"');
                        i++;
                    } else {
                        inQuotes = false;
                    }
                } else {
                    sb.Append(c);
                }
                continue;
            }

            if (c == '"') {
                inQuotes = true;
                hasToken = true;
                continue;
            }

            if (char.IsWhiteSpace(c)) {
                if (hasToken) {
                    tokens.Add(sb.ToString());
                    sb.Clear();
                    hasToken = false;
                }
                continue;
            }

            sb.Append(c);
            hasToken = true;
        }

        if (inQuotes) { return ([], "Nicht geschlossenes Anführungszeichen."); }

        if (hasToken) { tokens.Add(sb.ToString()); }

        return (tokens, null);
    }

    #endregion
}