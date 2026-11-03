// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Renderer;

namespace BeCreativeCLI.CliCommands;

/// <summary>
/// Tabellen: Liest oder setzt Spaltenkopf-Eigenschaften (QuickInfo, Admin-Info, Text ersetzen); Setzen nur mit dem CLI-Recht 'Edit column head'.
/// </summary>
public class TableColumnHeadCliCommand : CliCommand {

    #region Fields

    /// <summary>
    /// Anzeige-Name der Spalten-QuickInfo.
    /// </summary>
    private const string QuickInfoProperty = "QuickInfo";

    /// <summary>
    /// Anzeige-Name der Admin-Info der Spalte.
    /// </summary>
    private const string AdminInfoProperty = "Admin-Info";

    /// <summary>
    /// Anzeige-Name der Renderer-Ersetzungen; der Wert steckt versteckt in den RendererSettings der Spalte.
    /// </summary>
    private const string TextReplaceProperty = "Text ersetzen";

    #endregion

    #region Properties

    public override string Command => "table-columnhead";
    public override List<string> Options => ["password"];
    public override string Syntax => "bcr table-columnhead <tabelle> <spalte> [quickinfo[=<text>]] [admininfo[=<text>]] [textreplace[=<text>]] [--password <kennwort>]";

    public override string? HelpDetails =>
            "Ohne weitere Angabe erscheinen alle drei Werte als 'Eigenschaft: Wert', mit einer Eigenschaft (ohne =) nur ihr Wert. " +
            "Mit =<text> wird genau diese Eigenschaft gesetzt — leerer Text leert sie; Zeilenumbrüche werden zu <br> (so legt die App die Werte selbst ab), " +
            "bei textreplace trennen <br> und echte Umbrüche die Regeln (eine pro Zeile im Format alt|neu; ungültige Regeln brechen den Befehl mit Fehler ab; nur wirksam bei Spalten im Renderer '" + ImageAndTextRenderer.ClassId + "' mit sichtbarem Text). " +
            "Alle Eigenschaften sind in einem Aufruf setzbar. Setzen speichert und braucht das CLI-Recht '" + CliRights.EditColumnHead + "'.";

    #endregion

    #region Methods

    public override int DoIt(CliArgs args) {
        if (args.PositionalCount < 2) {
            return UsageError($"Erwartet werden mindestens 2 Positionsargumente (<tabelle> <spalte> [quickinfo[=<text>]] [admininfo[=<text>]] [textreplace[=<text>]]), erhalten: {args.PositionalCount}.");
        }

        // Angaben zerlegen: Eigenschaftsname ohne = = lesen, mit = = setzen (leerer Text = leeren).
        List<(string Property, string? Value)> wanted = [];

        for (var i = 2; i < args.PositionalCount; i++) {
            var spec = args[i] ?? string.Empty;
            var eq = spec.IndexOf('=');
            var name = eq < 0 ? spec : spec[..eq];
            var property = CanonicPropertyOf(name);

            if (property.Length == 0) {
                return UsageError("Unbekannte Eigenschaft: '" + name + "' — unterstützt werden: quickinfo, admininfo, textreplace");
            }

            if (wanted.Exists(w => w.Property == property)) {
                return UsageError("Eigenschaft mehrfach angegeben: " + property);
            }

            wanted.Add((property, eq < 0 ? null : spec[(eq + 1)..]));
        }

        var tbl = LoadTable(args);

        if (tbl is null) { return 1; }

        var column = tbl.Column[args[1] ?? string.Empty];

        if (column is not { IsDisposed: false }) {
            Console.Error.WriteLine("Spalte nicht gefunden: " + args[1]);
            return 1;
        }

        var toSet = wanted.Where(w => w.Value is not null).ToList();

        if (toSet.Count == 0) { return ReadOut(column, wanted); }

        // 'Text ersetzen' steckt im Renderer und ist nur dort setzbar; das vor allen
        // anderen Prüfungen klären, damit unbrauchbare Aufrufe nichts anfassen.
        if (toSet.Exists(w => w.Property == TextReplaceProperty)) {
            var rendererProblem = TextReplaceProblem(column);

            if (rendererProblem is not null) {
                Console.Error.WriteLine(rendererProblem);
                return 1;
            }
        }

        // <br> ist das Ablage-Format für Umbrüche und trennt Regeln wie ein Zeilenumbruch;
        // ungültige Regelzeilen werden abgelehnt, statt sie beim Speichern still zu verwerfen.
        for (var i = 0; i < toSet.Count; i++) {
            if (toSet[i].Property != TextReplaceProperty) { continue; }

            var text = (toSet[i].Value ?? string.Empty).Replace("\r\n", "\r").Replace('\n', '\r').Replace("<br>", "\r", StringComparison.OrdinalIgnoreCase);

            var invalid = text.SplitByCr().Where(l => l.Length > 0 && !IsValidTextReplaceRule(l)).ToList();

            if (invalid.Count > 0) {
                Console.Error.WriteLine("Ungültige Regelzeilen für 'Text ersetzen' (eine Regel pro Zeile im Format alt|neu): " + string.Join(" ; ", invalid));
                return 1;
            }

            toSet[i] = (toSet[i].Property, text);
        }

        // Die CLI vergleicht ausschließlich die CLI-Rechte der Tabelle.
        var rightProblem = RightProblem(tbl, CliRights.EditColumnHead);

        if (rightProblem is not null) {
            Console.Error.WriteLine(rightProblem);
            return 1;
        }

        // Erst nach allen Prüfungen den Fragment-Writer öffnen: Früh gescheiterte
        // Aufrufe sollen keine leere Fortsetzung mit EOF an die Fragment-Datei hängen.
        var fragmentProblem = FragmentEditProblem(tbl);

        if (fragmentProblem is not null) {
            Console.Error.WriteLine(fragmentProblem);
            return 2;
        }

        foreach (var (property, value) in toSet) {
            var text = (value ?? string.Empty).Replace("\r\n", "\r").Replace('\n', '\r');

            switch (property) {
                case TextReplaceProperty:
                    if (ImageAndTextRendererOf(column) is { } r) { // durch die Vorprüfung ausgeschlossen
                        r.Text_ersetzen = new(text.SplitAndCutByCr());
                        column.RendererSettings = r.ParseableItems().FinishParseable();
                    }

                    break;

                case QuickInfoProperty:
                    text = text.Replace("\r", "<br>");
                    column.QuickInfo = text;
                    break;

                case AdminInfoProperty:
                    text = text.Replace("\r", "<br>");
                    column.AdminInfo = text;
                    break;
            }

            // Regel-Trenner der Renderer-Ersetzungen nicht als Umbruch ausgeben.
            var readable = property == TextReplaceProperty ? text.Replace("\r", " ; ") : text;

            Console.Out.WriteLine(readable.Length > 0
                                              ? property + " von '" + column.KeyName + "' gesetzt: " + readable
                                              : property + " von '" + column.KeyName + "' geleert.");
        }

        return SaveTable(tbl);
    }

    /// <summary>
    /// Normalisiert die Angabe auf den Anzeige-Namen; leer bei unbekannter Eigenschaft.
    /// </summary>
    private static string CanonicPropertyOf(string name) {
        switch (name.ToLowerInvariant()) {
            case "quickinfo":
                return QuickInfoProperty;

            case "admininfo":
                return AdminInfoProperty;

            case "textreplace":
                return TextReplaceProperty;

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Gibt die gewünschten Werte lesbar aus: genau eine Eigenschaft nur als Wert, sonst als 'Eigenschaft: Wert'.
    /// </summary>
    private static int ReadOut(ColumnItem column, List<(string Property, string? Value)> wanted) {
        var show = wanted.Count > 0 ? wanted.ConvertAll(w => w.Property) : [QuickInfoProperty, AdminInfoProperty, TextReplaceProperty];

        if (show.Count == 1) {
            var value = ReadableTextOf(ValueOf(column, show[0]));
            Console.Out.WriteLine(value.Length > 0 ? value : "(leer)");
            return 0;
        }

        foreach (var property in show) {
            var value = ReadableTextOf(ValueOf(column, property));
            Console.Out.WriteLine(property + ": " + (value.Length > 0 ? value : "(leer)"));
        }

        return 0;
    }

    /// <summary>
    /// Liefert den Ablage-Wert der Eigenschaft.
    /// </summary>
    private static string ValueOf(ColumnItem column, string property) {
        switch (property) {
            case QuickInfoProperty:
                return column.QuickInfo;

            case AdminInfoProperty:
                return column.AdminInfo;

            default:
                // Die Ersetzungen stecken versteckt im Renderer; andere Renderer liefern leer.
                return ImageAndTextRendererOf(column) is { } r ? string.Join('\r', r.Text_ersetzen) : string.Empty;
        }
    }

    /// <summary>
    /// Erzeugt den ImageAndText-Renderer der Spalte mit ihren Einstellungen; null bei anderem Renderer oder unlesbaren Einstellungen.
    /// </summary>
    private static ImageAndTextRenderer? ImageAndTextRendererOf(ColumnItem column) {
        if (!column.DefaultRenderer.Equals(ImageAndTextRenderer.ClassId, StringComparison.OrdinalIgnoreCase)) { return null; }

        var r = new ImageAndTextRenderer();

        return r.Parse(column.RendererSettings) ? r : null;
    }

    /// <summary>
    /// Liefert null, wenn 'Text ersetzen' an der Spalte wirksam setzbar ist, ansonsten die Fehlermeldung.
    /// </summary>
    private static string? TextReplaceProblem(ColumnItem column) {
        if (ImageAndTextRendererOf(column) is not { } r) {
            return "Die Spalte '" + column.KeyName + "' nutzt nicht den Renderer '" + ImageAndTextRenderer.ClassId + "' — 'Text ersetzen' gibt es nur dort.";
        }

        return r.Text_anzeigen
            ? null
            : "Die Spalte '" + column.KeyName + "' zeigt keinen Text an (ShowText=-) — 'Text ersetzen' wäre wirkungslos.";
    }

    /// <summary>
    /// Löst die &lt;br&gt;-Umbrüche der Ablage zu echten Zeilenumbrüchen auf (wie der Spalten-Editor).
    /// </summary>
    private static string ReadableTextOf(string value) =>
        value.Replace("<br>", "\r", StringComparison.OrdinalIgnoreCase).Replace("\r", "\n");

    /// <summary>
    /// Prüft, ob eine Regelzeile dem Format alt|neu mit Nicht-leerem Suchtext entspricht.
    /// </summary>
    private static bool IsValidTextReplaceRule(string line) =>
        line.SplitBy("|") is { Length: 2 } parts && parts[0].Length > 0;

    #endregion
}
