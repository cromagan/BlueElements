// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Plugins;

/// <summary>
/// Ergänzt den MIT-Lizenz-Header am Dateianfang und entfernt führende Leer- und Kommentarzeilen davor.
/// </summary>
public class LicenseHeaderPlugin : CodeOptimizerPlugin {

    #region Fields

    private const string LicenseHeaderText = "// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.";

    private const string LicenseHeader = LicenseHeaderText + "\r\n\r\n";

    #endregion

    #region Properties

    public override string Name => "Lizenz-Header";
    public override ImageCode Symbol => ImageCode.Schild;
    public override string QuickInfo => "Ergänzt den MIT-Lizenz-Header am Dateianfang und entfernt führende Leer- und Kommentarzeilen.";
    public override int Order => 20;

    #endregion

    #region Methods

    public override string Execute(string rootPath) {
        var counterOk = 0;
        var counterAdded = 0;
        var counterStripped = 0;
        var skipped = new List<string>();

        foreach (var file in SourceFiles.Enumerate(rootPath)) {
            // Dateien ohne gültiges UTF-8 dürfen nicht umgeschrieben werden - das übernimmt das Encoding-Modul
            if (!SourceFiles.IsUtf8(file)) {
                skipped.Add(file);
                continue;
            }

            var content = File.ReadAllText(file).TrimStart('\uFEFF');

            // Leere Dateien bleiben leer
            if (content.Length == 0) { continue; }

            var lines = new List<string>(content.Split(["\r\n", "\n"], StringSplitOptions.None));
            string newContent;
            var stripped = 0;

            if (lines.Count > 0 && lines[0].Trim() == LicenseHeaderText) {
                // Header vorhanden: Kommentare dahinter bleiben erhalten, nur überzählige Leerzeilen direkt danach entfernen
                var rest = lines.Skip(1).ToList();

                while (rest.Count > 0 && rest[0].Length == 0) {
                    rest.RemoveAt(0);
                    stripped++;
                }

                newContent = LicenseHeader + string.Join("\r\n", rest);
            } else {
                // Kein Header: führende Leer- und Kommentarzeilen (alter Header) entfernen und Header ergänzen
                while (lines.Count > 0 && (string.IsNullOrWhiteSpace(lines[0]) || lines[0].TrimStart(' ', '\t').StartsWith("//", StringComparison.Ordinal))) {
                    lines.RemoveAt(0);
                    stripped++;
                }

                newContent = LicenseHeader + string.Join("\r\n", lines);
            }

            if (newContent == content) {
                counterOk++;
                continue;
            }

            File.WriteAllText(file, newContent, new UTF8Encoding(true));
            counterAdded++;
            counterStripped += stripped;
        }

        var report = new StringBuilder();
        report.AppendLine("=== Lizenz-Header ===");
        report.AppendLine(CultureInfo.InvariantCulture, $"Bereits vorhanden: {counterOk}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Ergänzt:           {counterAdded}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Zeilen entfernt:   {counterStripped}");
        AppendSkipped(report, skipped);
        return report.ToString();
    }

    #endregion
}
