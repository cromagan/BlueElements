// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using ColorDialog = System.Windows.Forms.ColorDialog;
using DialogResult = System.Windows.Forms.DialogResult;

namespace BlueControls.ControlStrategies;

/// <summary>
/// Zeigt nichts an: Ein einfacher Klick in die Zelle öffnet direkt den
/// Farbdialog. Ein Doppelklick bleibt wirkungslos.
/// </summary>
public class ColorDialogControlStrategy : ControlStrategy {

    #region Properties

    public static string ClassId => "ColorDialog";

    public override bool IsInstantAction => true;

    public override string KeyName => ClassId;

    protected override System.Windows.Forms.Control? ControlCore => null;

    #endregion

    #region Methods

    /// <summary>
    /// Die Spalte darf nicht mehrzeilig sein.
    /// </summary>
    public override string ErrorReason(bool mayHaveDropDown, bool multiLine) {
        if (multiLine) { return ColumnErrorConstants.NoMultilineAllowed; }
        return string.Empty;
    }

    /// <summary>
    /// Die Farbauswahl funktioniert unabhängig von Texteditierbarkeit oder Dropdown-Items der Spalte.
    /// </summary>
    public override bool IsAllowed(bool textEditable, bool mayHaveDropdownItems) => true;

    public override string ReadableText() => "Farbauswahl";

    public override void SubscribeEvents() { }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Farbrad);

    public override void UnsubscribeEvents() { }

    protected override void ApplyStyle() { }

    protected override void CreateControlCore() { }

    /// <summary>
    /// Öffnet den Farbdialog mit der aktuellen Zellfarbe und schreibt die
    /// gewählte Farbe zurück in die Zelle.
    /// </summary>
    protected override void ExecuteInstantAction(ColumnItem column, RowItem row) {
        var aktuelleFarbe = FarbeVon(row.CellGetString(column), Color.White);

        using var colorDia = new ColorDialog {
            FullOpen = true,
            Color = aktuelleFarbe,
            CustomColors = VorhandeneFarben(column, aktuelleFarbe)
        };

        if (colorDia.ShowDialog() != DialogResult.OK) { return; }

        row.CellSet(column, colorDia.Color.ToHtmlCode(), "Farbauswahl");
    }

    protected override void ForceWriteBackValue() { }

    protected override void SetValueToControlInternal(string value) { }

    private static void AddColorRef(List<int> ergebnis, Color farbe) {
        if (farbe.A == 0) { return; }

        var cr = farbe.R | (farbe.G << 8) | (farbe.B << 16);
        if (!ergebnis.Contains(cr)) { ergebnis.Add(cr); }
    }

    /// <summary>
    /// Wandelt einen Zellwert (Hex-Code, z. B. #ff0000) in eine Farbe; bei leerem oder ungültigem Wert die Ersatzfarbe.
    /// </summary>
    private static Color FarbeVon(string value, Color ersatz)
        => ColorTryParse(value, out var farbe) ? farbe : ersatz;

    /// <summary>
    /// Sammelt die vorhandenen Farben: der aktuelle Wert plus die Farbwerte
    /// der Spalte (Dropdown-Items und, falls aktiviert, die Zellinhalte).
    /// Format COLORREF (0x00BBGGRR), maximal 16 Einträge (Dialog-Grenze).
    /// </summary>
    private static int[] VorhandeneFarben(ColumnItem column, Color aktuelleFarbe) {
        List<int> ergebnis = [];

        AddColorRef(ergebnis, aktuelleFarbe);

        List<string> farbwerte = [.. column.DropDownItems];
        if (column.ShowValuesOfOtherCellsInDropdown) { farbwerte.AddRange(column.Contents()); }

        foreach (var farbe in farbwerte) {
            AddColorRef(ergebnis, FarbeVon(farbe, Color.Transparent));
        }

        return [.. ergebnis.Take(16)];
    }

    #endregion
}