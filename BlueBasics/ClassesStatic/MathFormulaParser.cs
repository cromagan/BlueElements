// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using System.Collections.Generic;
using static BlueBasics.ClassesStatic.Constants;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueBasics.ClassesStatic;

public static class MathFormulaParser {

    #region Fields

    public static readonly List<string> RechenOperatoren = ["^", "*", "/", "+", "-"];

    #endregion

    #region Methods

    public static double? Ergebnis(string formel) {
        formel = formel.Replace(" ", string.Empty);
        return string.IsNullOrEmpty(formel)
            ? null
            : formel != formel.ReduceToChars(Char_Numerals + ".,()+-/*^") ? null : ErgebnisCore(formel);
    }

    public static int LastMinusIndex(string formel) {
        if (!formel.Contains("-")) { return -1; }
        var lastMin = 1;
        var okMin = -1;
        while (true) {
            lastMin = formel.IndexOf("-", lastMin, StringComparison.Ordinal);
            if (lastMin < 1) { break; }
            var vorZ = formel.Substring(lastMin - 1, 1);
            if (vorZ.IsNumeral() || vorZ == ")") { okMin = lastMin; } // FIX: auch schließende Klammer vor Minus zulassen
            lastMin++;
        }
        return okMin;
    }

    private static double? ErgebnisCore(string formel) {
        formel = formel.Trim();

        // Umschließende Klammern nur entfernen, wenn sie wirklich das Ganze umschließen
        while (HatUmschliessendeKlammern(formel)) {
            formel = formel.Substring(1, formel.Length - 2).Trim();
        }

        formel = NormalisiereVorzeichen(formel);

        // Führendes + entfernen
        if (formel.StartsWith("+")) { formel = formel.Substring(1); }

        // Klammern auflösen
        if (formel.Contains("(")) {
            var a = formel.LastIndexOf("(", StringComparison.Ordinal);
            var e = formel.IndexOf(")", a, StringComparison.Ordinal);
            if (a < 0 || e < 0 || a >= e) { return null; }
            var inner = formel.Substring(a + 1, e - a - 1);
            var replacer = ErgebnisCore(inner);
            if (replacer == null) { return null; }
            // Kulturunabhängig mit InvariantCulture (statt Komma/Punkt Turnerei)
            var repString = ((double)replacer).ToString("0.#############################", System.Globalization.CultureInfo.InvariantCulture);
            formel = formel.Substring(0, a) + repString + formel.Substring(e + 1);
            formel = NormalisiereVorzeichen(formel);
            return ErgebnisCore(formel);
        }

        // Numerisch?
        if (formel.Replace(".", ",").IsNumeral()) { return DoubleParse(formel.Replace(".", ",")); }

        // Operator-Suche nach Priorität: + - (letztes binäres), dann * /, dann ^ (rechtsassoziativ)
        // Ebene 1: + -
        var tmp = Math.Max(formel.LastIndexOf("+", StringComparison.Ordinal), LastMinusIndex(formel));
        if (tmp > 0) {
            var sep = formel.Substring(tmp, 1);
            var w1 = ErgebnisCore(formel.Substring(0, tmp));
            if (w1 == null) { return null; }
            var w2 = ErgebnisCore(formel.Substring(tmp + 1));
            if (w2 == null) { return null; }
            return sep == "+" ? w1 + w2 : w1 - w2;
        }

        // Ebene 2: * /
        var mul = formel.LastIndexOf("*", StringComparison.Ordinal);
        var div = formel.LastIndexOf("/", StringComparison.Ordinal);
        tmp = Math.Max(mul, div);
        if (tmp > 0) {
            var sep = formel.Substring(tmp, 1);
            var w1 = ErgebnisCore(formel.Substring(0, tmp));
            if (w1 == null) { return null; }
            var w2 = ErgebnisCore(formel.Substring(tmp + 1));
            if (w2 == null) { return null; }
            if (sep == "/") {
                if (w2 == 0) { return null; }
                return w1 / w2;
            }
            return w1 * w2;
        }

        // Ebene 3: ^ (rechtsassoziativ => wir suchen das erste von rechts, aber splitten so, dass rechts rekursiv weiter aufgelöst wird)
        var pow = formel.LastIndexOf("^", StringComparison.Ordinal);
        if (pow > 0) {
            var w1 = ErgebnisCore(formel.Substring(0, pow));
            if (w1 == null) { return null; }
            var w2 = ErgebnisCore(formel.Substring(pow + 1));
            if (w2 == null) { return null; }
            return Math.Pow((double)w1, (double)w2); // FIX
        }

        // Führendes negatives Zahlliteral? (z.B. -3)
        if (formel.StartsWith("-") &&
            DoubleTryParse(formel.Substring(1).Replace(".", ","), out var result)) { return -result; }

        return null;
    }

    private static bool HatUmschliessendeKlammern(string f) {
        if (f.Length < 2) { return false; }
        if (f[0] != '(' || f[f.Length - 1] != ')') { return false; }
        var tiefe = 0;
        for (var i = 0; i < f.Length; i++) {
            var c = f[i];
            if (c == '(') { tiefe++; } else if (c == ')') {
                tiefe--;
                if (tiefe == 0 && i < f.Length - 1) { return false; }
            }
        }
        return tiefe == 0;
    }

    private static string NormalisiereVorzeichen(string f) {
        // Wiederholen bis stabil
        while (true) {
            var alt = f;
            f = f.Replace("++", "+")
                 .Replace("--", "+")
                 .Replace("+-", "-")
                 .Replace("-+", "-");
            if (f == alt) { return f; }
        }
    }

    #endregion
}