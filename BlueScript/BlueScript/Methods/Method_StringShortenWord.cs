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

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_StringShortenWord : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain]];
    public override string Command => "stringshortenword";
    public override string Description => "Versucht den String zu kürzen, indem Abkürzungen verwendet werden.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "StringShortenWord(String)";

    #endregion

    #region Methods

   public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var txt = attvar.ValueStringGet(0);
        if (string.IsNullOrEmpty(txt)) { return new DoItFeedback(txt); }
        //TXT = TXT.HTMLSpecialToNormalChar();
        txt = txt.Replace("Sekunden", "Sek.");
        txt = txt.Replace("Sekunde", "Sek.");
        txt = txt.Replace("Minuten", "Min.");
        txt = txt.Replace("Minute", "Min.");
        txt = txt.Replace("Stunden", "Std.");
        txt = txt.Replace("Stunde", "Std.");
        txt = txt.Replace(" und ", " & ");
        txt = txt.Replace(" oder ", " o. ");
        txt = txt.Replace("Zum Beispiel", "Z. B.");
        txt = txt.Replace("zum Beispiel", "z. B.");
        txt = txt.Replace("Keine Angaben", "K. A.");
        txt = txt.Replace("keine Angaben", "k. A.");
        txt = txt.Replace("Keine Angabe", "K. A.");
        txt = txt.Replace("keine Angabe", "k. A.");
        //Tx = Tx.Replace("Etwa ", "Ca. ") ' und mit etwas Glück = und mit ca. Glück :-(((
        //Tx = Tx.Replace("etwa ", "ca. ")
        txt = txt.Replace("Circa", "Ca.");
        txt = txt.Replace("circa", "ca.");
        txt = txt.Replace("Stücke", "St.");
        txt = txt.Replace("Stück", "St.");
        txt = txt.Replace("St.n", "St."); // Stücken
        txt = txt.Replace("St.chen", "St."); // Stückchen
        txt = txt.Replace("Kilogramm", "kg");
        //  tx = tx.Replace(" Kilo", " kg")
        txt = txt.Replace("Gramm", "g");
        txt = txt.Replace("Päckchen", "P.");
        txt = txt.Replace("Packung", "P.");
        txt = txt.Replace("Esslöffel", "EL");
        txt = txt.Replace("Eßlöffel", "EL");
        txt = txt.Replace("Teelöffel", "TL");
        txt = txt.Replace("Messerspitze", "Msp.");
        txt = txt.Replace("Portionen", "Port.");
        txt = txt.Replace("Portion", "Port.");
        txt = txt.Replace("ein halbes ", "1/2 ", RegexOptions.IgnoreCase);
        txt = txt.Replace("eine halbe ", "1/2 ", RegexOptions.IgnoreCase);
        txt = txt.Replace("ein halber ", "1/2 ", RegexOptions.IgnoreCase);
        txt = txt.Replace("ein drittel ", "1/3 ", RegexOptions.IgnoreCase);
        txt = txt.Replace("zwei drittel ", "2/3 ", RegexOptions.IgnoreCase);
        txt = txt.Replace("eine drittel ", "1/3 ", RegexOptions.IgnoreCase);
        txt = txt.Replace("ein achtel ", "1/8 ", RegexOptions.IgnoreCase);
        txt = txt.Replace("eine achtel ", "1/8 ", RegexOptions.IgnoreCase);
        txt = txt.Replace("Stufe ", "St. ");
        txt = txt.Replace("Liter", "l ", RegexOptions.IgnoreCase);
        txt = txt.Replace("ein EL", "1 EL", RegexOptions.IgnoreCase);
        txt = txt.Replace("ein TL", "1 TL", RegexOptions.IgnoreCase);
        txt = txt.Replace("zwei EL", "2 EL", RegexOptions.IgnoreCase);
        txt = txt.Replace("zwei TL", "2 TL", RegexOptions.IgnoreCase);
        string[] a = ["es", "er", "em", "en", "e", ""];
        for (var t = 0; t <= a.GetUpperBound(0); t++) {
            txt = txt.Replace("gerieben" + a[t], "ger.");
            //tx = tx.Replace("groß" + A[t], "gr.");
            //tx = tx.Replace("klein" + A[t], "kl.");
            txt = txt.Replace("gekocht" + a[t], "gek.");
            txt = txt.Replace("tiefgekühlt" + a[t], "TK");
        }
        txt = txt.Replace("Tiefkühl", "TK-");
        //TXT = TXT.CreateHtmlCodes(true);
        return new DoItFeedback(txt);
    }

    #endregion
}