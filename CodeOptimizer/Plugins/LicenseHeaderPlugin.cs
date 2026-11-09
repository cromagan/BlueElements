// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Plugins;

/// <summary>
/// Ergänzt den MIT-Lizenz-Header am Dateianfang und entfernt führende Leer- und Kommentarzeilen davor.
/// </summary>
public class LicenseHeaderPlugin : CodeOptimizerPlugin {

    #region Fields

    private const string LicenseHeader = "// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.\r\n\r\n";

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

        foreach (var file in SourceFiles.Enumerate(rootPath)) {
            var content = File.ReadAllText(file);

            if (content.StartsWith(LicenseHeader, StringComparison.Ordinal) && !content.StartsWith(LicenseHeader + "\r\n", StringComparison.Ordinal)) {
                counterOk++;
                continue;
            }

            var lines = new List<string>(content.Split(["\r\n", "\n"], StringSplitOptions.None));
            var stripped = 0;

            while (lines.Count > 0 && (string.IsNullOrWhiteSpace(lines[0]) || lines[0].TrimStart(' ', '\t').StartsWith("//", StringComparison.Ordinal))) {
                lines.RemoveAt(0);
                stripped++;
            }

            var newContent = LicenseHeader + string.Join("\r\n", lines);

            if (newContent == content) { continue; }

            File.WriteAllText(file, newContent, new UTF8Encoding(true));
            counterAdded++;

            if (stripped > 0) { counterStripped++; }
        }

        var report = new StringBuilder();
        report.AppendLine("=== Lizenz-Header ===");
        report.AppendLine(CultureInfo.InvariantCulture, $"Bereits vorhanden: {counterOk}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Ergänzt:           {counterAdded}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Zeilen entfernt:   {counterStripped}");
        return report.ToString();
    }

    #endregion
}
