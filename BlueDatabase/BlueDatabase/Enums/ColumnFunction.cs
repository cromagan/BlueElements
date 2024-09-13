// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
    Normal = 1,

    //Bit = 2,

    //Ganzzahl = 3,
    //Gleitkommazahl = 6,

    //BildCode = 13,

    // Date_GermanFormat = 15
    //Datum_und_Uhrzeit = 16,

    // Bin�rdaten_Bild = 19,
    // Passwort = 20, // String
    //  Text_Ohne_Kritische_Zeichen = 21,
    // Bin�rdaten = 39,
    // Link_To_BlueDataSystem = 42
    // Telefonnummer = 43, // Spezielle Formate
    //FarbeInteger = 45, // Color

    // Email = 46, // Spezielle Formate
    // InternetAdresse = 47, // Spezielle Formate
    // Relation = 65,
    // Event = 66,
    // Tendenz = 67
    // Einsch�tzung = 68,
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

    RelationText = 77,

    // KeyForSame = 78
    //Button = 79,

    Verkn�pfung_zu_anderer_Datenbank = 80,

    Schl�sselspalte = 81,

    Verkn�pfung_zu_anderer_Datenbank2 = 82,

    Virtuelle_Spalte = 83,

    Zeile = 84

    // bis 999 wird gepr�ft
}

public static class ColumnFunctionExtensions {

    #region Methods

    public static bool Autofilter_m�glich(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                    or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank
                                                                                    or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2
                                                                                    or ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems
                                                                                    or ColumnFunction.RelationText
                                                                                    or ColumnFunction.Schl�sselspalte
                                                                                    or ColumnFunction.Virtuelle_Spalte
                                                                                    or ColumnFunction.Zeile;

    public static bool CanBeChangedByRules(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                     or ColumnFunction.RelationText
                                                                                     or ColumnFunction.Virtuelle_Spalte
                                                                                     or ColumnFunction.Zeile;

    public static bool CanBeCheckedByRules(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                     or ColumnFunction.RelationText
                                                                                     or ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems
                                                                                     or ColumnFunction.Schl�sselspalte
                                                                                     or ColumnFunction.Virtuelle_Spalte
                                                                                     or ColumnFunction.Zeile;

    public static bool DropdownItemsAllowed(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                      or ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems
                                                                                      or ColumnFunction.RelationText;

    public static bool DropdownItemsOfOtherCellsAllowed(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                                  or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank
                                                                                                  or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2
                                                                                                  or ColumnFunction.RelationText;

    public static bool DropdownUnselectAllAllowed(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                            or ColumnFunction.RelationText
                                                                                            or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank
                                                                                            or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2;

    public static bool MultilinePossible(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                   or ColumnFunction.RelationText
                                                                                   or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank
                                                                                   or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2
                                                                                   or ColumnFunction.Schl�sselspalte
                                                                                   or ColumnFunction.Virtuelle_Spalte;

    public static bool NeedTargetDatabase(this ColumnFunction function) => function is ColumnFunction.Verkn�pfung_zu_anderer_Datenbank
                                                                                    or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2
                                                                                    or ColumnFunction.Werte_aus_anderer_Datenbank_als_DropDownItems;

    public static bool SpellCheckingPossible(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                       or ColumnFunction.RelationText
                                                                                       or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank
                                                                                       or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2
                                                                                       or ColumnFunction.Schl�sselspalte;

    public static bool TextboxEditPossible(this ColumnFunction function) => function is ColumnFunction.Normal
                                                                                     or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank
                                                                                     or ColumnFunction.Verkn�pfung_zu_anderer_Datenbank2
                                                                                     or ColumnFunction.RelationText
                                                                                     or ColumnFunction.Virtuelle_Spalte;

    #endregion
}