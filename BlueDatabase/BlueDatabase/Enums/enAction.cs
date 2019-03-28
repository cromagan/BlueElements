#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
#endregion

namespace BlueDatabase.Enums
{
    public enum enAction
    {

        Anmerkung = 1,

        Ist = 11,
        Ist_Nicht = 12,
        Enthält = 13,

        Enthält_Zeichenkette = 15,
        Enthält_NICHT_Zeichenkette = 16,

        Formatfehler_des_Zelleninhaltes = 17,

        Enthält_ungültige_Zeichen = 18,

        Auf_eine_existierende_Datei_verweist = 20,
        Auf_einen_existierenden_Pfad_verweist = 21,

       // Erhält_den_Focus = 22,

        Unsichtbare_Zeichen_am_Ende_Enthält = 23,


        Ist_der_Nutzer = 25,
        Berechnung_ist_True = 26,
        // Ist_Jünger_Als = 27,


        Setze_Fehlerhaft = 1001,
        Wert_Dazu = 1003,
        Wert_Weg = 1004,
        Wert_Setzen = 1005,
        //Mache_einen_Vorschlag = 1007,

        Berechne = 1009,

        Sperre_die_Zelle = 1011,

        Substring = 1017

       // LinkedCell = 1018,
        //SortiereIntelligent = 1020,
    //    KopiereAndereSpalten = 1021



    }
}