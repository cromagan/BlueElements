// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

namespace BlueDatabase.Enums;

public enum ColumnFunction {
    // Unbekannt = -1,
    // Nothing = 0,

    /// <summary>
    /// Ohne besondere Funktion
    /// </summary>
    Normal = 1,

    //Bit = 2,

    //Ganzzahl = 3,
    //Gleitkommazahl = 6,

    //BildCode = 13,

    // Date_GermanFormat = 15
    //Datum_und_Uhrzeit = 16,

    // Binärdaten_Bild = 19,
    // Passwort = 20, // String
    //  Text_Ohne_Kritische_Zeichen = 21,
    // Binärdaten = 39,
    // Link_To_BlueDataSystem = 42
    // Telefonnummer = 43, // Spezielle Formate
    //FarbeInteger = 45, // Color

    // Email = 46, // Spezielle Formate
    // InternetAdresse = 47, // Spezielle Formate
    // Relation = 65,
    // Event = 66,
    // Tendenz = 67
    // Einschätzung = 68,
    //Schrift = 69,

    //Text_mit_Formatierung = 70,

    // TextmitFormatierungUndLinkToAnotherDatabase = 71
    // Relation_And_Event_Mixed = 72,
    //Link_To_Filesystem = 73,

    /// <summary>
    /// Gibt die Werte einer anderen Spalte einer
    /// anderen Datenbank als Dropdown Items wieder
    /// </summary>
    Werte_aus_anderer_Datenbank_als_DropDownItems = 76,

    /// <summary>
    /// Legt automatisch Verknüpfungen zu anderen Zellen an / hält diese Gleich
    /// </summary>
    RelationText = 77,

    // KeyForSame = 78
    //Button = 79,

    //Verknüpfung_zu_anderer_Datenbank = 80,

    /// <summary>
    /// Besodere Spalte, löst eine extended Changed aus bei Wertänderung
    /// </summary>
    Schlüsselspalte = 81,

    /// <summary>
    /// Zeigt eine Zelle einer anderen Datenbank an
    /// </summary>
    Verknüpfung_zu_anderer_Datenbank = 82,

    ///// <summary>
    ///// Werte werden in echtzeit - evtl. für jeden Benutzer anderes - berechnet. Wird nicht gespeichert.
    ///// </summary>
    //Virtuelle_Spalte = 83,

    ///// <summary>
    ///// Hat den Wert einer anderen Spalte, für schnelle Zugriffe
    ///// </summary>
    //Zeile = 84,

    /// <summary>
    /// Dieser Wert ist er Hauptwert der Zeile. Nur einmal pro datenbank erlaubt
    /// </summary>
    First = 85,

    /// <summary>
    /// Dieser Spalte kann zum Aufsplitten der Datenbank benutzt werden. Nur einmal pro Datenbank erlaubt
    /// </summary>

    Split_Name = 90,
    Split_Medium = 95,
    Split_Large = 96,

    // bis 999 wird geprüft
}

public static class ColumnFunctionExtensions {

    #region Methods

    public static bool Autofilter_möglich(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                    or ColumnFunction.Verknüpfung_zu_anderer_Datenbank
                                                                                    or ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems
                                                                                    or ColumnFunction.RelationText
                                                                                    or ColumnFunction.Schlüsselspalte
                                                                                    or ColumnFunction.Split_Medium
                                                                                    or ColumnFunction.Split_Large
                                                                                    or ColumnFunction.Split_Name
                                                                                    or ColumnFunction.First;

    public static bool CanBeChangedByRules(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                     or ColumnFunction.RelationText
                                                                                     or ColumnFunction.Schlüsselspalte
                                                                                     or ColumnFunction.First;

    public static bool CanBeCheckedByRules(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                     or ColumnFunction.RelationText
                                                                                     or ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems
                                                                                     or ColumnFunction.Schlüsselspalte
                                                                                     or ColumnFunction.Split_Medium
                                                                                     or ColumnFunction.Split_Large
                                                                                     or ColumnFunction.Split_Name
                                                                                     or ColumnFunction.First;

    public static bool CopyAble(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                 or ColumnFunction.RelationText
                                                                                 or ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems
                                                                                 or ColumnFunction.Schlüsselspalte
                                                                                 or ColumnFunction.Split_Medium
                                                                                 or ColumnFunction.Split_Large
                                                                                 or ColumnFunction.Split_Name
                                                                                 or ColumnFunction.First
                                                                                 or ColumnFunction.Verknüpfung_zu_anderer_Datenbank;

    public static bool DropdownItemsAllowed(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                      or ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems
                                                                                      or ColumnFunction.RelationText
                                                                                      or ColumnFunction.First;

    public static bool DropdownItemsOfOtherCellsAllowed(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                                  or ColumnFunction.Verknüpfung_zu_anderer_Datenbank
                                                                                                  or ColumnFunction.RelationText
                                                                                                  or ColumnFunction.First;

    public static bool DropdownUnselectAllAllowed(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                            or ColumnFunction.RelationText
                                                                                            or ColumnFunction.Verknüpfung_zu_anderer_Datenbank;

    public static bool MultilinePossible(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                   or ColumnFunction.RelationText
                                                                                   or ColumnFunction.Verknüpfung_zu_anderer_Datenbank
                                                                                   or ColumnFunction.Schlüsselspalte;

    public static bool NeedTargetDatabase(this ColumnFunction function) => function is ColumnFunction.Verknüpfung_zu_anderer_Datenbank
                                                                                    or ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems;

    public static bool SpellCheckingPossible(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                       or ColumnFunction.RelationText
                                                                                       or ColumnFunction.Verknüpfung_zu_anderer_Datenbank
                                                                                       or ColumnFunction.Schlüsselspalte;

    public static bool TextboxEditPossible(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                     or ColumnFunction.Verknüpfung_zu_anderer_Datenbank
                                                                                     or ColumnFunction.RelationText;

    #endregion
}