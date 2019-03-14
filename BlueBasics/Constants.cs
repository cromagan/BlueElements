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

using System;


// http://www.carlosag.net/tools/codetranslator/
// http://converter.telerik.com/

namespace BlueBasics
{
    public static class Constants
    {

        //public const char beTab = (char)9;

        // <summary>
        // Macintosh(OS 9): \r, 0D, 13
        // </summary>
        //public const char beCr = (char)13;


        /// <summary>
        /// Unix & Mac OS X: \n, 0A, 10
        /// </summary>
        //public const char beLf = (char)10;
        public const char beChrW1 = (char)1;



        public static readonly string Char_NotFromClip = (char)3 + ((char)22).ToString() + (char)24 + "\n";


        /// <summary>
        /// DOS & Windows: \r\n 0D0A(hex), 13,10 (decimal)
        /// </summary>
        //public const string beCrLf = "\r\n"; // "\r" + beLf.ToString();

        public const char FirstSortChar = '-';
        public const char SecondSortChar = 'X';

        public const string Char_PfadSonderZeichen = "*?\"<>|\r\n";
        public const string Char_DateiSonderZeichen = "\\/:*?\"<>|\r\n";


        public static readonly string Char_Sonderzeichen = "'()+-=$&´`^%˜°•øØμ@\\/*<>|[]{}#~µ_„\"²³€" + (char)0 + beChrW1 + "\t";
        public static readonly string Char_KritischeZeichen = Char_Sonderzeichen + Char_DateiSonderZeichen;

        public static readonly string Char_Steuercodes = ((char)0).ToString() + beChrW1 + (char)2 + (char)3 + (char)4 +
                                               (char)5 + (char)6 + (char)7 + (char)8 + "\t" +
                                                   "\n" + (char)11 + (char)12 + "\r" + (char)14 +
                                                   (char)15 + (char)16 + (char)17 + (char)18 + (char)19 +
                                                   (char)20 + (char)21 + (char)22 + (char)23 + (char)24 +
                                                   (char)25 + (char)26 + (char)27 + (char)28 + (char)29 +
                                                   (char)30;

        public const string Char_Numerals = "0123456789";
        public const string Char_Buchstaben = "abcdefghijklmnopqrstuvwxyzäöüßáéíóúâêîôûàèìòùžñ";
        public const string Char_Satzeichen = " .!,:;?";

        //public static readonly string[] Umrechnungen = { "1000 μm = 1 mm", "10 mm = 1 cm", "10 cm = 1 dm", "10 dm = 1 m", "1000 m = 1 km", "1000 μg = 1 mg", "1000 mg = 1 g", "1000 g = 1 kg", "1000 kg = 1 t", "1 d = 24 h", "1 h = 60 min", "1 min = 60 s", "1000 ms = 1 s", "1000 μl = 1 ml", "10 ml = 1 cl", "10 cl = 1 dl", "10 dl = 1 l", "100 l = 1 hl", "1 kcal = 4,187 kJ", "1000 cal = 1 kcal", "1000 J = 1 kJ", "1 mph = 1,609344 km/h", "1 m/s = 3600 m/h", "1 m/s = 3,6 km/h", "1 € = 100 ct", "1 byte = 8 bit", "1 MB = 1024 byte", "1 GB = 1024 MB", "1 TB = 1024 GB" }

        public static Random GlobalRND = new Random();

    }
}
