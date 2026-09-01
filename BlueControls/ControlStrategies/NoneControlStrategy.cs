// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.ControlStrategies;

/// <summary>
/// Strategie für „Spalte nicht benutzer-editierbar": erzeugt kein Control
/// und kann weder Text noch Vorschläge.
/// </summary>
public class NoneControlStrategy : ControlStrategy {

    #region Properties

    public static string ClassId => "None";

    public override string Description => "Deaktiviert die Bearbeitung: Der Wert kann vom Benutzer nicht geändert werden.";
    public override string KeyName => ClassId;
    public override string NotEditableReason => "Die Inhalte dieser Spalte können nicht manuell bearbeitet werden, da keine Bearbeitungsmethode erlaubt ist.";
    public override bool SupportsValueChange => false;
    protected override System.Windows.Forms.Control? ControlCore => null;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) => [];

    public override string ReadableText() => "Keine Bearbeitung";

    public override void SubscribeEvents() { }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Kreuz);

    public override void UnsubscribeEvents() { }

    protected override void ApplyStyle() { }

    protected override void CreateControlCore() { }

    protected override void ForceWriteBackValue() { }

    protected override void SetValueToControlInternal(string value) { }

    #endregion
}