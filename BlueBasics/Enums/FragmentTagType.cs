// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Enums;

/// <summary>
/// Standardisierte Tags (valueId 231–255) des binären Fragment-Formats — Steuer-Sätze
/// auf Datei-Ebene ohne Empfänger-Typ: genau 1 Tab, nie Meta, werden vom Reader
/// abgefangen, bevor der Satz an eine Klasse gereicht wird. Klasseneigene Enums
/// dürfen keine Steuer-Werte ≥ 231 definieren und teilen sich die Bytes mit
/// diesem Enum.
/// </summary>
public enum FragmentTagType : byte {
    None = 0,

    /// <summary>
    /// Kommentar, der in die Datei geschrieben wurde.
    /// </summary>
    Comment = 231,

    /// <summary>
    /// Version am Datei-Kopf; Content = Versionskennung der Fragment-Datei.
    /// </summary>
    SerializerVersion = 232,

    /// <summary>
    /// Benutzer, der die Datei zuletzt gespeichert hat.
    /// </summary>
    SaveUser = 233,

    /// <summary>
    /// UTC-Zeitpunkt der letzten Speicherung (ToString9).
    /// </summary>
    SaveTimeUtc = 234,

    /// <summary>
    /// Rechner, der die Binärdatei zuletzt gespeichert hat.
    /// </summary>
    Machine = 235,

    /// <summary>
    /// Copyright-Info der Datei.
    /// </summary>
    CopyrightInfo = 236,

    Type = 237,

    /// <summary>
    /// UTC-Zeitpunkt, bis zu dem der eingetragene temporäre Master erwartet wird.
    /// </summary>
    MasterTimeUtc = 238,

    /// <summary>
    /// Benutzer des aktuell eingetragenen temporären Masters.
    /// </summary>
    MasterUser = 239,

    /// <summary>
    /// Rechner des aktuell eingetragenen temporären Masters.
    /// </summary>
    MasterMachine = 240,

    /// <summary>
    /// Anwendung des aktuell eingetragenen temporären Masters.
    /// </summary>
    MasterApp = 241,

    /// <summary>
    /// Instanz-Kennung des aktuell eingetragenen temporären Masters.
    /// </summary>
    MasterId = 242,

    /// <summary>
    /// Benutzer, der die Daten erzeugt hat; bleibt über weitere Speicherungen erhalten.
    /// </summary>
    CreateUser = 243,

    /// <summary>
    /// UTC-Zeitpunkt der Erzeugung der Daten (ToString9); bleibt über weitere Speicherungen erhalten.
    /// </summary>
    CreateTimeUtc = 244,



    // 245 - 253 frei für künftige Typen

    /// <summary>
    /// Marker „Datei wird von einem anderen Prozess fortgeführt“; ersetzt den
    /// Textmarker „- CONTINUED“. Eine Datei, die darauf endet, ist in Bearbeitung.
    /// </summary>
    ContinueEof = 254,

    /// <summary>
    /// Marker „Datei sauber abgeschlossen“; ersetzt den Textmarker „- EOF“.
    /// </summary>
    Eof = 255
}
