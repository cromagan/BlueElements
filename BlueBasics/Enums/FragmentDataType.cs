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

    /// <summary>
    /// 32-Bit-Ganzzahl; Content als 4 Little-Endian-Bytes.
    /// </summary>
    Int = 3,

    /// <summary>
    /// 32-Bit-IEEE-754-Float; Content als 4 Little-Endian-Bytes.
    /// </summary>
    Float = 4,

    /// <summary>
    /// Zeit; Content als 8 Tick-Bytes (Little-Endian).
    /// </summary>
    DateTime = 5,

    /// <summary>
    /// Farbe; Content als 4 ARGB-Bytes (Little-Endian).
    /// </summary>
    Color = 6,

    /// <summary>
    /// JSON-Knoten; Content als ToJsonString-Text.
    /// </summary>
    Json = 7,

    /// <summary>
    /// Komplette String-Liste als ListSet-Satz: Content enthält je Eintrag
    /// Tab, 2 Längenbytes, Inhalt und 0x0D.
    /// </summary>
    ListSet = 10,

    ListKeyObject = 11,

    /// <summary>
    /// 64-Bit-Ganzzahl; Content als 8 Little-Endian-Bytes.
    /// </summary>
    Long = 12,

    /// <summary>
    /// 64-Bit-IEEE-754-Double; Content als 8 Little-Endian-Bytes.
    /// </summary>
    Double = 13,

    /// <summary>
    /// 128-Bit-Decimal; Content als 16 Little-Endian-Bytes (GetBits: Lo, Mid, Hi, Flags).
    /// </summary>
    Decimal = 14

       // 15 - 230 frei für künftige Typen
}
