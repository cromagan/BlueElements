// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.ControlStrategies;

/// <summary>
/// Strategie für Spalten, deren Werte sich automatisch durch Verschieben der
/// Zeilen ergeben (z. B. SysRowSortIndex). Erzeugt kein Control.
/// </summary>
public class DragDropControlStrategy : ControlStrategy {

    #region Properties

    public static string ClassId => "DragDrop";

    protected override System.Windows.Forms.Control? ControlCore => null;

    public override string Description => "Der Wert ergibt sich automatisch durch Verschieben der Zeilen, z. B. der Sortierindex.";

    public override bool IsSpecial => true;

    public override string KeyName => ClassId;

    public override string NotEditableReason => "Werte ändern sich automatisch durch\r\nVerschieben der Zeilen.";

    #endregion

    #region Methods

    protected override void CreateControlCore() { }

    protected override void ForceWriteBackValue() { }

    public override string ReadableText() => "Automatisch durch Verschieben";

    public override void SubscribeEvents() { }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Pfeil_Unten);

    public override void UnsubscribeEvents() { }

    protected override void ApplyStyle() { }

    protected override void SetValueToControlInternal(string value) { }

    #endregion
}