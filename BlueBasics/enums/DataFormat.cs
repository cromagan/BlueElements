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

namespace BlueBasics.Enums;

public enum DataFormat {

    // Unbekannt = -1,
    // Nothing = 0,
    Text = 1,

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
    FarbeInteger = 45, // Color

    // Email = 46, // Spezielle Formate
    // InternetAdresse = 47, // Spezielle Formate
    // Relation = 65,
    // Event = 66,
    // Tendenz = 67
    // Einschätzung = 68,
    Schrift = 69,

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
    Button = 79,

    Verknüpfung_zu_anderer_Datenbank = 80

    // bis 999 wird geprüft
}

public static class DataFormatExtensions {


    #region Methods

    public static bool Autofilter_möglich(this DataFormat format) => format is DataFormat.Text or DataFormat.FarbeInteger or DataFormat.Schrift or DataFormat.Verknüpfung_zu_anderer_Datenbank or DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems or DataFormat.RelationText;

    public static bool CanBeChangedByRules(this DataFormat format) => format is DataFormat.Text or DataFormat.FarbeInteger or DataFormat.RelationText or DataFormat.Schrift;

    public static bool CanBeCheckedByRules(this DataFormat format) => format is DataFormat.Text or DataFormat.FarbeInteger or DataFormat.RelationText or DataFormat.Schrift or DataFormat.Verknüpfung_zu_anderer_Datenbank or DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems;

    public static bool DropdownItemsAllowed(this DataFormat format) => format is DataFormat.Text or DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems or DataFormat.RelationText;

    public static bool DropdownItemsOfOtherCellsAllowed(this DataFormat format) => format is DataFormat.Text or DataFormat.FarbeInteger or DataFormat.Schrift or DataFormat.Verknüpfung_zu_anderer_Datenbank or DataFormat.RelationText;

    public static bool DropdownUnselectAllAllowed(this DataFormat format) => format is DataFormat.Text or DataFormat.FarbeInteger or DataFormat.RelationText or DataFormat.Schrift or DataFormat.Verknüpfung_zu_anderer_Datenbank;

    public static bool ExportableForLayout(this DataFormat format) => format is DataFormat.Text or DataFormat.RelationText or DataFormat.Schrift or DataFormat.Verknüpfung_zu_anderer_Datenbank or DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems;

    public static bool MultilinePossible(this DataFormat format) => format is DataFormat.Text or DataFormat.RelationText or DataFormat.Verknüpfung_zu_anderer_Datenbank;

    public static bool NeedTargetDatabase(this DataFormat format) => format is DataFormat.Verknüpfung_zu_anderer_Datenbank or DataFormat.Werte_aus_anderer_Datenbank_als_DropDownItems;

    public static bool SpellCheckingPossible(this DataFormat format) => format is DataFormat.Text or DataFormat.RelationText or DataFormat.Verknüpfung_zu_anderer_Datenbank;




    public static bool TextboxEditPossible(this DataFormat format) => format is DataFormat.Text or DataFormat.Verknüpfung_zu_anderer_Datenbank or DataFormat.RelationText;

    #endregion
}