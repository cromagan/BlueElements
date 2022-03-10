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

using System.Collections.Generic;
using System.Text.RegularExpressions;
using BlueScript.Structures;
using BlueScript.Enums;
using static BlueBasics.Extensions;

namespace BlueScript.Methods {

    internal class Method_StringShortenWord : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.String_or_List };
        public override string Description => "Versucht den String zu kürzen, indem Abkürzungen verwendet werden.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.String;
        public override string StartSequence => "(";
        public override string Syntax => "StringShortenWord(String)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "stringshortenword" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }
            var TXT = ((VariableString)attvar.Attributes[0]).ValueString;
            if (string.IsNullOrEmpty(TXT)) {
                return new DoItFeedback(TXT, string.Empty);
            }
            //TXT = TXT.HTMLSpecialToNormalChar();
            TXT = TXT.Replace("Sekunden", "Sek.");
            TXT = TXT.Replace("Sekunde", "Sek.");
            TXT = TXT.Replace("Minuten", "Min.");
            TXT = TXT.Replace("Minute", "Min.");
            TXT = TXT.Replace("Stunden", "Std.");
            TXT = TXT.Replace("Stunde", "Std.");
            TXT = TXT.Replace(" und ", " & ");
            TXT = TXT.Replace(" oder ", " o. ");
            TXT = TXT.Replace("Zum Beispiel", "Z. B.");
            TXT = TXT.Replace("zum Beispiel", "z. B.");
            TXT = TXT.Replace("Keine Angaben", "K. A.");
            TXT = TXT.Replace("keine Angaben", "k. A.");
            TXT = TXT.Replace("Keine Angabe", "K. A.");
            TXT = TXT.Replace("keine Angabe", "k. A.");
            //Tx = Tx.Replace("Etwa ", "Ca. ") ' und mit etwas Glück = und mit ca. Glück :-(((
            //Tx = Tx.Replace("etwa ", "ca. ")
            TXT = TXT.Replace("Circa", "Ca.");
            TXT = TXT.Replace("circa", "ca.");
            TXT = TXT.Replace("Stücke", "St.");
            TXT = TXT.Replace("Stück", "St.");
            TXT = TXT.Replace("St.n", "St."); // Stücken
            TXT = TXT.Replace("St.chen", "St."); // Stückchen
            TXT = TXT.Replace("Kilogramm", "kg");
            //  tx = tx.Replace(" Kilo", " kg")
            TXT = TXT.Replace("Gramm", "g");
            TXT = TXT.Replace("Päckchen", "P.");
            TXT = TXT.Replace("Packung", "P.");
            TXT = TXT.Replace("Esslöffel", "EL");
            TXT = TXT.Replace("Eßlöffel", "EL");
            TXT = TXT.Replace("Teelöffel", "TL");
            TXT = TXT.Replace("Messerspitze", "Msp.");
            TXT = TXT.Replace("Portionen", "Port.");
            TXT = TXT.Replace("Portion", "Port.");
            TXT = TXT.Replace("ein halbes ", "1/2 ", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("eine halbe ", "1/2 ", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("ein halber ", "1/2 ", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("ein drittel ", "1/3 ", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("zwei drittel ", "2/3 ", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("eine drittel ", "1/3 ", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("ein achtel ", "1/8 ", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("eine achtel ", "1/8 ", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("Stufe ", "St. ");
            TXT = TXT.Replace("Liter", "l ", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("ein EL", "1 EL", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("ein TL", "1 TL", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("zwei EL", "2 EL", RegexOptions.IgnoreCase);
            TXT = TXT.Replace("zwei TL", "2 TL", RegexOptions.IgnoreCase);
            string[] A = { "es", "er", "em", "en", "e", "" };
            for (var t = 0; t <= A.GetUpperBound(0); t++) {
                TXT = TXT.Replace("gerieben" + A[t], "ger.");
                //tx = tx.Replace("groß" + A[t], "gr.");
                //tx = tx.Replace("klein" + A[t], "kl.");
                TXT = TXT.Replace("gekocht" + A[t], "gek.");
                TXT = TXT.Replace("tiefgekühlt" + A[t], "TK");
            }
            TXT = TXT.Replace("Tiefkühl", "TK-");
            //TXT = TXT.CreateHtmlCodes(true);
            return new DoItFeedback(TXT, string.Empty);
        }

        #endregion
    }
}