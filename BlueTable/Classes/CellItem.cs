// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.IO;

namespace BlueTable.Classes;

/// <summary>
/// Diese Klasse enthält nur das Aussehen und gibt keinerlei Fachevents ab;
/// FragmentChanged dient ausschließlich dem Bubbling des binären Fragment-Formats.
/// </summary>
public class CellItem : IFragmentStoreable {

    #region Enums

    /// <summary>
    /// Empfänger-Typen der Zelle im binären Fragment-Format; spiegelt den Zell-Bereich
    /// von TableDataType (200-206). Nur der Zelle selbst bekannt.
    /// </summary>
    private enum CellFragmentType : byte {

        /// <summary>
        /// Zellinhalt als UTF-8-Text.
        /// </summary>
        UTF8Value_withoutSizeData = 1,
    }

    #endregion

    #region Constructors

    public CellItem(string value) => Value = value;

    public CellItem() { }

    #endregion

    #region Properties


    public string Value {
        get;
        set {
            if (field == value) { return; }
            field = value;

            // FragmentChanged dient ausschließlich dem Bubbling des binären
            // Fragment-Formats; die Zelle hat keine Sub-Items, der Pfad bleibt leer.
            OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, CellFragmentType.UTF8Value_withoutSizeData, value));
        }
    } = string.Empty;

    #endregion

    #region IFragmentStoreable

    /// <summary>
    /// Bubbling-Event für Fragment-Änderungen (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public event EventHandler<FragmentChangedEventArgs>? FragmentChanged;

    /// <summary>
    /// Eine Zelle hat keine Sub-Items.
    /// </summary>
    public bool SerializeCore(Stream stream, int depth) {
        if (Value is not { Length: > 0 }) { return true; }

        return FragmentRecord.Append(stream, depth, CellFragmentType.UTF8Value_withoutSizeData, Value);
    }

    /// <summary>
    /// Verarbeitet den Zell-Block; der Inhalt ist das Blatt UTF8Value_withoutSizeData.
    /// </summary>
    public bool DeserializeThis(Stream stream, FragmentRecord record) {
        switch ((CellFragmentType)record.LeafType) {
            case CellFragmentType.UTF8Value_withoutSizeData:
                Value = record.StringContent;
                break;

            default:
                // Unbekannter Blatt-Typ — Satz überspringen
                break;
        }

        return true;
    }

    /// <summary>
    /// Löst das FragmentChanged-Event mit den fertigen Argumenten aus (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public void OnFragmentChanged(FragmentChangedEventArgs e) => FragmentChanged?.Invoke(this, e);

    #endregion
}
