// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Enums;

public enum MethodType {

    /// <summary>
    /// Methoden, die eine kurze Laufzeit haben und ohne weitere Bedingen eingesetzt werden können.
    /// </summary>
    Standard = 0,

    ///// <summary>
    ///// Methoden, die längere Laufzeiten haben können. Evtl. Dateizugriffe. Aber Autonom ablaufe, ohne den Bentzer zu stören.
    ///// </summary>
    LongTime = 1,

    /// <summary>
    /// Befehle, die eine subroutine Starten.
    /// </summary>
    Sub = 2,

    /// <summary>
    /// Der Befehl stört den Benutzer aktiv. Z.B. wird das Clipboard verändert der ein Programm gestartet.
    /// Aber die Ausführung des Skriptes wird nicht unterbrochen.
    /// </summary>
    ManipulatesUser = 3,

    /// <summary>
    /// Dieser Befehl kann nur aktiv verwendet werden, wenn der Benutzer aktiv vor dem Bildschirm sitzt.
    /// Evtl. werden Skripte gestoppt und warten auf Benutzereingaben.
    /// </summary>
    GUI = 4,

    /// <summary>
    /// Sehr spezielle Befehle, die nur an einer einzigen Position erlaubt sind
    /// </summary>
    Special = 9
}