// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using System;

namespace BlueBasics.Enums {

    public enum enDataFormat {

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
        Link_To_Filesystem = 73,

        LinkedCell = 74,
        Columns_für_LinkedCellDropdown = 75,

        /// <summary>
        /// Gibt die Werte einer anderen Spalte einer
        /// anderen Datenbank als Dropdown Items wieder
        /// </summary>
        Values_für_LinkedCellDropdown = 76,

        RelationText = 77,

        // KeyForSame = 78
        Button = 79,

        Verknüpfung_zu_anderer_Datenbank = 80

        // bis 999 wird geprüft
    }
}