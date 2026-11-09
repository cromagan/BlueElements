// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace CodeOptimizer.Plugins;

/// <summary>
/// Prüft alle .cs-Dateien und konvertiert sie nach UTF-8 mit BOM.
/// UTF-16- und CP1252-Dateien werden konvertiert, defekte UTF-8-Dateien werden nur gemeldet.
/// </summary>
public class ConvertEncodingPlugin : CodeOptimizerPlugin {

    #region Properties

    public override string Name => "Encoding";
    public override ImageCode Symbol => ImageCode.AZ;
    public override string QuickInfo => "Konvertiert alle .cs-Dateien nach UTF-8 mit BOM. Defekte UTF-8-Dateien werden gemeldet.";
    public override int Order => 10;

    #endregion

    #region Methods

    public override string Execute(string rootPath) {
        var counterOk = 0;
        var counterAddedBom = 0;
        var counterConverted = 0;
        var broken = new List<string>();

        foreach (var file in SourceFiles.Enumerate(rootPath)) {
            var bytes = File.ReadAllBytes(file);

            // Leere Datei
            if (bytes.Length == 0) {
                counterOk++;
                continue;
            }

            // UTF-8 mit BOM
            if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) {
                var content = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                if (content.Contains('\uFFFD')) {
                    broken.Add(file);
                } else {
                    counterOk++;
                }
                continue;
            }

            // UTF-16 LE
            if (bytes[0] == 0xFF && bytes[1] == 0xFE) {
                WriteUtf8Bom(file, Encoding.Unicode.GetString(bytes));
                counterConverted++;
                continue;
            }

            // UTF-16 BE
            if (bytes[0] == 0xFE && bytes[1] == 0xFF) {
                WriteUtf8Bom(file, Encoding.BigEndianUnicode.GetString(bytes));
                counterConverted++;
                continue;
            }

            if (IsValidUtf8(bytes)) {
                // Gültiges UTF-8 ohne BOM - BOM ergänzen
                WriteUtf8Bom(file, Encoding.UTF8.GetString(bytes));
                counterAddedBom++;
            } else {
                // Kein UTF-8 (z. B. CP1252) - konvertieren
                WriteUtf8Bom(file, Win1252.GetString(bytes));
                counterConverted++;
            }
        }

        var report = new StringBuilder();
        report.AppendLine("=== Encoding ===");
        report.AppendLine(CultureInfo.InvariantCulture, $"OK (UTF-8 mit BOM): {counterOk}");
        report.AppendLine(CultureInfo.InvariantCulture, $"BOM ergänzt:        {counterAddedBom}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Konvertiert:        {counterConverted}");
        report.AppendLine(CultureInfo.InvariantCulture, $"Defekte Dateien:    {broken.Count}");

        foreach (var file in broken) {
            report.AppendLine("  Defekt: " + file);
        }

        return report.ToString();
    }

    private static void WriteUtf8Bom(string file, string content) {
        File.WriteAllText(file, content, new UTF8Encoding(true));
    }

    private static bool IsValidUtf8(byte[] bytes) {
        var i = 0;

        while (i < bytes.Length) {
            var b = bytes[i];

            if (b <= 0x7F) {
                i++;
            } else if (b is >= 0xC2 and <= 0xDF && i + 1 < bytes.Length && bytes[i + 1] is >= 0x80 and <= 0xBF) {
                i += 2;
            } else if (b is >= 0xE0 and <= 0xEF && i + 2 < bytes.Length && bytes[i + 1] is >= 0x80 and <= 0xBF && bytes[i + 2] is >= 0x80 and <= 0xBF) {
                i += 3;
            } else if (b is >= 0xF0 and <= 0xF4 && i + 3 < bytes.Length && bytes[i + 1] is >= 0x80 and <= 0xBF && bytes[i + 2] is >= 0x80 and <= 0xBF && bytes[i + 3] is >= 0x80 and <= 0xBF) {
                i += 4;
            } else {
                return false;
            }
        }

        return true;
    }

    #endregion
}
