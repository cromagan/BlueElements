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
    SchreibPruefung = 40,    // 40: Zelle ändern und Schreib-Pipeline prüfen (Rücklesen, Undo-Historie, SaveRequired)
    UniqueVerletzung = 41,   // 41: Unique-Verletzung versuchen und Verweigerung erwarten
    FilterPruefung = 42,     // 42-43: Filter-Engine nach dem Wert einer Zufallszelle suchen lassen
    ZeilenSchub = 44,        // 44: Vier Zeilen anlegen und sofort wieder löschen
    ZeileVollLesen = 45,     // 45-46: Komplette Zufallszeile zweimal lesen
    TabelleNeuLaden = 47,    // 47: Speichern, freigeben und mitten im Lauf neu laden
    ScriptAusfuehren = 48,   // 48: Eigenes Stress-Script ausführen
    SpeicherStand = 49,      // 49: Managed Memory und Working Set protokollieren
}
