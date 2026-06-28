// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Enums;

public enum AddType {

    /// <summary>
    /// Hinzu-Button wird nicht angezeigt.
    /// </summary>
    None = 0,

    /// <summary>
    /// Hinzu-Button wird angezeigt, und auf einen Klick dessen wird ein Item mittels einer Input-Box erstellt.
    /// </summary>
    Text = 1,

    ///// <summary>
    ///// Hinzu-Button wird angezeigt, und auf einen Klick dessen wird  ein Item mittels einer File-Select-Box erstellt.
    /////Die Original-Dateien werden nicht verändert.
    ///// </summary>
    //BinarysFromFileSystem = 2,

    /// <summary>
    /// Hinzu-Button wird angezeigt, und auf einen Klick dessen wird ein Item mittels einer List-Box erstellt.
    /// </summary>
    OnlySuggests = 3,

    /// <summary>
    /// Hinzu-Button wird angezeigt, und auf einen Klick dessen der Delegate AddMethode ausgeführt.
    /// </summary>
    UserDef = 4,

    /// <summary>
    /// Hinzu-Button wird als vollflächige Schaltfläche "Hinzufügen" ohne Texteingabe angezeigt.
    /// Bei Klick wird der Delegate AddMethod direkt (mit leerem Text) ausgeführt.
    /// Sind Suggestions vorhanden, öffnet sich stattdessen ein Floating-Menü zur Auswahl;
    /// nach der Auswahl wird AddMethod mit dem Schlüssel des gewählten Vorschlags aufgerufen.
    /// </summary>
    UserDef_NoText = 5
}