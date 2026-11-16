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
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) {
                var content = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                if (content.Contains('\uFFFD')) {
                    broken.Add(file);
                } else {
                    counterOk++;
                }
                continue;
            }

            // UTF-16 LE - ohne BOM-Zeichen übernehmen
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) {
                WriteUtf8Bom(file, Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2));
                counterConverted++;
                continue;
            }

            // UTF-16 BE - ohne BOM-Zeichen übernehmen
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) {
                WriteUtf8Bom(file, Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2));
                counterConverted++;
                continue;
            }

            if (SourceFiles.IsValidUtf8(bytes)) {
                // Gültiges UTF-8 ohne BOM - BOM ergänzen
                WriteUtf8Bom(file, Encoding.UTF8.GetString(bytes));
                counterAddedBom++;
            } else if (Utf16Encoding(bytes) is { } utf16) {
                // UTF-16 ohne BOM - korrekt konvertieren, bevor ein anderes Modul die Datei als Text anfassen könnte
                WriteUtf8Bom(file, utf16.GetString(bytes));
                counterConverted++;
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

    /// <summary>
    /// Liefert das Encoding einer UTF-16-Datei ohne BOM, erkennbar am NUL-Muster der High- bzw. Low-Bytes; sonst null.
    /// </summary>
    private static Encoding? Utf16Encoding(byte[] bytes) {
        if (bytes.Length == 0 || bytes.Length % 2 != 0) { return null; }

        var nulEven = 0;
        var nulOdd = 0;

        for (var i = 0; i < bytes.Length; i += 2) {
            if (bytes[i] == 0) { nulEven++; }
            if (bytes[i + 1] == 0) { nulOdd++; }
        }

        var pairs = bytes.Length / 2;

        if (nulOdd >= pairs / 4 && nulEven == 0) { return Encoding.Unicode; }
        if (nulEven >= pairs / 4 && nulOdd == 0) { return Encoding.BigEndianUnicode; }

        return null;
    }

    private static void WriteUtf8Bom(string file, string content) {
        File.WriteAllText(file, content, new UTF8Encoding(true));
    }

    #endregion
}
