// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace TableStress;

/// <summary>
/// Zufalls-Aktionen des Stresstests; der Wert markiert den Beginn des zugehörigen Zufallsbereichs.
/// </summary>
public enum StressAction {
    NeueZeile = 0,           // 0-3: Neue Zeile anlegen
    ZeileLoeschen = 4,       // 4: Zeile löschen
    ZelleAendern = 5,        // 5-20: Zufälligen Zellwert ändern
    MasterSetzen = 21,       // 21: Master-User setzen
    Komplettierung = 22,     // 22: Komplettierung anstoßen
    ScriptErstellen = 23,    // 23: Ein Script erstellen
    AlleDatenLaden = 24,     // 24-26: Alle Daten laden
    SpalteAnlegen = 27,      // 27-28: Stress-Spalte anlegen
    SpalteLoeschen = 29,     // 29: Älteste Stress-Spalte löschen
    ZelleSonderzeichen = 30, // 30: Zelle mit kritischen Sonderzeichen füllen
    ZeileDuplizieren = 31,   // 31: Zeile duplizieren
    SpalteMetadaten = 32,    // 32: Caption/QuickInfo einer Stress-Spalte ändern
    SortierungPruefen = 33,  // 33: Sortierung einer Zufallsspalte lesen
    DuplikatePruefen = 34,   // 34: Duplikat-Suche auf einer Zufallsspalte
    ScriptLoeschen = 35,     // 35: Eigenes Stress-Script löschen
    KonsistenzPruefen = 36,  // 36: In-Memory-Konsistenz prüfen
    CsvExportPruefen = 37,   // 37: CSV-Export ausführen
    ChunkWechsel = 38,       // 38-39: CHUNK-Zelle auf anderen Pool-Wert setzen
}
