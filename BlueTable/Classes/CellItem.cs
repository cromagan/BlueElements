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
    public enum CellFragmentType : byte {

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


    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Setzt den Wert und meldet die Änderung als Fragment-Event; der Änderungs-Kommentar
    /// geht in den Meta-Stempel des Undo-Eintrags ein.
    /// </summary>
    public void SetValue(string value, string comment) {
        if (Value == value) { return; }
        Value = value;

        // FragmentChanged dient ausschließlich dem Bubbling des binären
        // Fragment-Formats; die Zelle hat keine Sub-Items, die Kette bleibt beim Blatt.
        OnFragmentChanged(new FragmentChangedEventArgs(this, CellFragmentType.UTF8Value_withoutSizeData, value) { Comment = comment });
    }

    #endregion

    #region IFragmentStoreable

    /// <summary>
    /// Zähler der aktiven SuppressEvents-Aufrufe; Werte größer 0 unterdrücken FragmentChanged.
    /// </summary>
    private int _eventsSuppressCount;

    /// <summary>
    /// Zeigt an, ob FragmentChanged-Events aktuell unterdrückt sind.
    /// </summary>
    public bool IsEventsSuppressed => _eventsSuppressCount > 0;

    /// <summary>
    /// Unterdrückt FragmentChanged-Events; zählbasiert, also verschachtelbar — jedes SuppressEvents braucht ein ResumeEvents.
    /// </summary>
    public void SuppressEvents() => _eventsSuppressCount++;

    /// <summary>
    /// Hebt die Unterdrückung von SuppressEvents wieder auf.
    /// </summary>
    public void ResumeEvents() {
        if (_eventsSuppressCount > 0) { _eventsSuppressCount--; }
    }

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
    /// Verständlicher Name eines Empfänger-Typs im eigenen Zahlraum; unbekannte
    /// Werte liefern die Zahl.
    /// </summary>
    public static string GetNameOf(ReadOnlySpan<FragmentRecord> path) => ((CellFragmentType)path[^1].LeafType).ToString();

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
    /// Übernimmt den Zellinhalt; der Value-Setter meldet die Änderung selbst.
    /// </summary>
    public string Update(IFragmentStoreable source) {
        if (source is not CellItem sourceCell) { return "Quelltyp ungültig"; }

        Value = sourceCell.Value;
        return string.Empty;
    }

    /// <summary>
    /// Löst das FragmentChanged-Event mit den fertigen Argumenten aus (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public void OnFragmentChanged(FragmentChangedEventArgs e) {
        if (IsEventsSuppressed) { return; }

        FragmentChanged?.Invoke(this, e);
    }

    #endregion
}
