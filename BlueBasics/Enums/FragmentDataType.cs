// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Enums;

/// <summary>
/// Globales Content-Enum (valueId) des binären Fragment-Formats „Typ + Key“.
/// Beschreibt, wie der Content eines Daten-Satzes zu interpretieren ist.
/// Steuer-Sätze (valueId ≥ 231) liegen im separaten FragmentTagType bzw. als
/// klasseneigene Steuer-Werte und werden vom Reader abgefangen, bevor der
/// Satz an eine Klasse gereicht wird.
/// </summary>
public enum FragmentDataType : byte {
    None = 0,

    Bool = 1,

    String = 2,

    Int = 3,

    Double = 4,

    DateTime = 5,

    /// <summary>
    /// Farbe; Content als Hex-Code (ToHtmlCode).
    /// </summary>
    Color = 6,

    /// <summary>
    /// JSON-Knoten; Content als ToJsonString-Text.
    /// </summary>
    Json = 7,

    /// <summary>
    /// Komplette String-Liste als ListSet-Satz: Count als lesbare Dezimalzahl,
    /// dann je Eintrag Tab, 2 Längenbytes, Inhalt und 0x0D (kein len2-Feld).
    /// </summary>
    ListSet = 10,

    ListKeyObject = 11

       // 13 - 230 frei für künftige Typen
}
