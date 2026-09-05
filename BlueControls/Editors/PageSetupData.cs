// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Drawing.Printing;

namespace BlueControls.Forms;

/// <summary>
/// Definiert die Seiteneinstellungen eines Arbeitsbereichs oder Druckerdokuments
/// ausschließlich in Millimetern. Über diese Klasse werden Eingaben im
/// PageSetupDialog ohne den Genauigkeitsverlust der Druck-API
/// (1/100 Zoll) ausgetauscht.
/// </summary>
public sealed class PageSetupData : IEditable {

    #region Properties

    /// <summary>
    /// Papierbreite in mm (kurze Seite bei Hochformat-Definition).
    /// </summary>
    public float BreiteMm { get; set; }

    public string CaptionForEditor => "Seite einrichten";

    /// <summary>
    /// Papierhöhe in mm (lange Seite bei Hochformat-Definition).
    /// </summary>
    public float HöheMm { get; set; }

    public bool Querformat { get; set; }

    public float RandLinksMm { get; set; }
    public float RandObenMm { get; set; }

    public float RandRechtsMm { get; set; }
    public float RandUntenMm { get; set; }

    /// <summary>
    /// Vom Drucker unterstützte Papierformate für die Auswahl im Dialog.
    /// Bleibt die Liste leer, bietet der Dialog nur die manuelle Eingabe an.
    /// </summary>
    public List<PaperFormat> VerfügbareFormate { get; } = [];

    #endregion

    #region Methods

    /// <summary>
    /// Erzeugt eine PageSetupData aus einem PrintDocument.
    /// Die verfügbaren Papierformate werden aus den Druckereinstellungen übernommen.
    /// </summary>
    public static PageSetupData FromPrintDocument(PrintDocument doc) {
        var data = new PageSetupData {
            Querformat = doc.DefaultPageSettings.Landscape,
            BreiteMm = PixelToMm(doc.DefaultPageSettings.PaperSize.Width),
            HöheMm = PixelToMm(doc.DefaultPageSettings.PaperSize.Height),
            RandObenMm = PixelToMm(doc.DefaultPageSettings.Margins.Top),
            RandUntenMm = PixelToMm(doc.DefaultPageSettings.Margins.Bottom),
            RandLinksMm = PixelToMm(doc.DefaultPageSettings.Margins.Left),
            RandRechtsMm = PixelToMm(doc.DefaultPageSettings.Margins.Right)
        };

        foreach (PaperSize ps in doc.PrinterSettings.PaperSizes) {
            data.VerfügbareFormate.Add(new PaperFormat(ps.PaperName, PixelToMm(ps.Width), PixelToMm(ps.Height)));
        }

        return data;
    }

    /// <summary>
    /// Schreibt die Werte zurück in ein PrintDocument.
    /// Die Druck-API arbeitet in 1/100 Zoll, daher wird hier auf ganze
    /// 1/100-Zoll-Schritte gerundet (ca. 0,25 mm Auflösung).
    /// </summary>
    public void ApplyTo(PrintDocument doc) {
        doc.DefaultPageSettings.Landscape = Querformat;
        doc.DefaultPageSettings.PaperSize = new PaperSize("Benutzerdefiniert", MmToPixel(BreiteMm), MmToPixel(HöheMm));
        doc.DefaultPageSettings.Margins.Top = MmToPixel(RandObenMm);
        doc.DefaultPageSettings.Margins.Bottom = MmToPixel(RandUntenMm);
        doc.DefaultPageSettings.Margins.Left = MmToPixel(RandLinksMm);
        doc.DefaultPageSettings.Margins.Right = MmToPixel(RandRechtsMm);
    }

    public string IsNowEditable() => string.Empty;

    private static int MmToPixel(float mm) => (int)Math.Round(mm / 0.254f);

    private static float PixelToMm(int pixel) => (float)Math.Round(pixel * 0.254f, 1, MidpointRounding.AwayFromZero);

    #endregion
}

/// <summary>
/// Ein vom Drucker unterstütztes Papierformat mit Anzeigenamen und
/// Abmessungen in Millimetern.
/// </summary>
public sealed record PaperFormat(string Name, float BreiteMm, float HöheMm);