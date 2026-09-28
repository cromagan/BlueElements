// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Enums;

/// <summary>
/// Legt fest, wie eine Spalte innerhalb von Skripten als Variable erscheint.
/// Die Summaries werden als QuickInfo im Spalten-Editor angezeigt.
/// </summary>
public enum ScriptType {

    /// <summary>
    /// Die Spalte wird nicht als Skript-Variable bereitgestellt.
    /// </summary>
    Nicht_vorhanden = 0,

    /// <summary>
    /// Die Spalte ist im Skript ein Wahrheitswert (Ja/Nein) und kann vom Skript verändert werden.
    /// </summary>
    Bool = 1,

    /// <summary>
    /// Die Spalte ist im Skript ein Text und kann vom Skript verändert werden.
    /// </summary>
    String = 2,

    /// <summary>
    /// Die Spalte ist im Skript eine Zahl und kann vom Skript verändert werden.
    /// </summary>
    Numeral = 3,

    /// <summary>
    /// Die Spalte ist im Skript eine Liste; der Zellinhalt wird an den Zeilenumbrüchen in Elemente getrennt. Das Skript kann die Liste verändern.
    /// </summary>
    List = 4,

    /// <summary>
    /// Die Spalte ist im Skript ein Text, den das Skript nur lesen kann.
    /// </summary>
    String_Readonly = 5,

    //DateTime = 6,

    /// <summary>
    /// Die Spalte ist im Skript ein Wahrheitswert (Ja/Nein), den das Skript nur lesen kann.
    /// </summary>
    Bool_Readonly = 7,

    //Row = 8,

    /// <summary>
    /// Die Spalte ist im Skript eine Liste, die das Skript nur lesen kann.
    /// </summary>
    List_Readonly = 9,

    /// <summary>
    /// Die Spalte ist im Skript eine Zahl, die das Skript nur lesen kann.
    /// </summary>
    Numeral_Readonly = 10,

    /// <summary>
    /// Die Spalte ist ein Element einer Skript-Liste: Der Spaltenname muss mit einer Zahl enden (Format NameZahl, z. B. Test5).
    /// Alle gleich benannten Spalten (Test0, Test1, Test4, …) erscheinen im Skript als eine gemeinsame Liste Test, wobei die Zahl die nullbasierte Position des Zellwerts in der Liste ist (Test0 ist das erste Element). Rück schreiben funktioniert ebenso.
    /// </summary>
    ListElement = 11,

    /// <summary>
    /// Der Skript-Typ dieser Spalte wurde noch nicht festgelegt.
    /// </summary>
    undefiniert = 999
}
