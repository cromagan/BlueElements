namespace TableStress;

/// <summary>
/// Zufalls-Aktionen des Stresstests; der Wert markiert den Beginn des zugehörigen Zufallsbereichs.
/// </summary>
public enum StressAction {
    NeueZeile = 0,        // 0-3: Neue Zeile anlegen
    ZeileLoeschen = 4,    // 4: Zeile löschen
    ZelleAendern = 5,     // 5-20: Zufälligen Zellwert ändern
    MasterSetzen = 21,    // 21: Master-User setzen
    Komplettierung = 22,  // 22: Komplettierung anstoßen
    ScriptErstellen = 23, // 23: Ein Script erstellen
    AlleDatenLaden = 24,  // 24-26: Alle Daten laden
}
