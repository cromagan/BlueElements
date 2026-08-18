// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.ControlStrategies;

/// <summary>
/// Strategie für „Spalte nicht benutzer-editierbar": erzeugt kein Control
/// und kann weder Text noch Vorschläge.
/// </summary>
public class NoneControlStrategy : ControlStrategy {

    #region Properties

    public static string ClassId => "None";

    protected override System.Windows.Forms.Control? ControlCore => null;

    public override string Description => "Deaktiviert die Bearbeitung: Der Wert kann vom Benutzer nicht geändert werden.";

    public override string KeyName => ClassId;

    #endregion

    #region Methods

    public override void CreateControl() { }

    public override string ReadableText() => "Keine Bearbeitung";

    public override void SubscribeEvents() { }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(BlueBasics.Enums.ImageCode.Kreuz);

    public override void UnsubscribeEvents() { }

    protected override void ApplyStyle() { }

    protected override void SetValueToControlInternal(string value) { }

    #endregion
}