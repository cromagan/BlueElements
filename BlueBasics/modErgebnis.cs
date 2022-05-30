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
using System.Collections.Generic;
using static BlueBasics.Converter;

namespace BlueBasics;

public static class Berechnung {

    #region Fields

    public static readonly List<string> RechenOperatoren = new() { "*", "/", "+", "-" };

    #endregion

    #region Methods

    public static double? Ergebnis(string formel) {
        formel = formel.Replace(" ", string.Empty);
        return string.IsNullOrEmpty(formel)
            ? null
            : formel != formel.ReduceToChars(Constants.Char_Numerals + ".,()+-/*") ? null : ErgebnisCore(formel);
    }

    public static int LastMinusIndex(string formel) {
        if (!formel.Contains("-")) { return -1; }
        var lastMin = 1;
        var okMin = -1;
        while (true) {
            lastMin = formel.IndexOf("-", lastMin);
            if (lastMin < 1) { break; }
            var vorZ = formel.Substring(lastMin - 1, 1);
            if (vorZ.IsNumeral()) { okMin = lastMin; }
            lastMin++;
        }
        return okMin;
    }

    private static double? ErgebnisCore(string formel) {
        // var TMP = 0;
        formel = formel.DeKlammere(true, false, false, true);
        // Das alles kann nur möglich sein, WENN eine Klammer vorhanden ist
        if (formel.Contains("(")) {
            // --------------------------------------------------------------------------------------------------------------------------------
            // --- Eine Klammer auflösen, im Formelstring ersetzen und         mittels Rekursivität die nun einfachere Formel berechnen.
            // --------------------------------------------------------------------------------------------------------------------------------
            var a = formel.LastIndexOf("(");
            var e = formel.IndexOf(")", a);
            if (a >= e) { return null; }
            // if (a > 2 && Formel.IndexOf(",", a) > a && Formel.IndexOf(",", a) < e)
            // {
            // // --------------------------------------------------------------------------------------------------------------------------------
            //    // --- Ok, Funktion kommt, erkannt an den beinhaltenden Kommas.
            //    // --- Es ist die Letzte Klammer, deswegen KANN in der Funktion keine mehr sein.
            //    // --- Und die Kommas stellen den Seperator dar.
            //    // --------------------------------------------------------------------------------------------------------------------------------
            //    var Att = Formel.Substring(a + 1, e - a - 1).SplitAndCutBy(",");
            //    var Att2 = new double?[Att.Length];
            // for (TMP = 0; TMP < Att.Length; TMP++)
            //    {
            //        var qq = ErgebnisCore(Att[TMP]);
            //        if (qq == null) { return null; }
            //        Att2[TMP] = qq;
            //    }
            // Replacer = Att2[0];
            // switch (Formel.Substring(a - 3, 4))
            //    {
            //        case "MIN(":
            //            for (TMP = 1; TMP < Att2.Length; TMP++)
            //            {
            //                if (Att2[TMP] < Replacer) { Replacer = Att2[TMP]; }
            //            }
            // break;
            //        case "MAX(":
            //            for (TMP = 1; TMP < Att2.Length; TMP++)
            //            {
            //                if (Att2[TMP] > Replacer) { Replacer = Att2[TMP]; }
            //            }
            // break;
            //        case "XNT(":
            //            if (Att2.Length != 2) { return null; }
            //            Replacer = Math.Floor((double)Att2[1]);
            // break;
            //        case "BTW(": // Between. Format: BTW(IsValue, MinValue, MaxValue)
            //            if (Att2.Length != 3) { return null; }
            //            if (Att2[0] >= Att2[1] && Att2[0] <= Att2[2])
            //            {
            //                Replacer = -1;
            //            }
            //            else
            //            {
            //                Replacer = 0;
            //            }
            // break;
            //        case "IFF(":
            //            if (Att2.Length != 3) { return null; }
            // if (Att2[0] == -1)
            //            {
            //                Replacer = Att2[1];
            //            }
            //            else
            //            {
            //                Replacer = Att2[2];
            //            }
            // break;
            // case "XND(":
            //            if (Att2.Length != 2) { return null; }
            //            Replacer = Constants.GlobalRND.NextDouble();
            //            break;
            // default:
            //            return null;
            //    }
            // a -= 3;
            // }
            // else // Es ist KEINE Funktion, also den Inhalt der Klammer normal berechnen
            // {
            var replacer = ErgebnisCore(formel.Substring(a + 1, e - a - 1));
            // }
            if (replacer == null) { return null; }
            formel = formel.Replace(formel.Substring(a, e - a + 1), ((double)replacer).ToString("F99").TrimEnd('0').TrimEnd(',').Replace(",", "."));
            return ErgebnisCore(formel);
        } // Ende Klammer Vorhanden-----------------------------------------------------------
        // --------------------------------------------------------------------------------------------------------------------------------
        // --- Prüfen, ob überhaupt eine Berechnung nötig ist. Z.B. wenn unnötige Klammern aufgelöst wurden. ------------------------------
        // --------------------------------------------------------------------------------------------------------------------------------
        if (formel.Replace(".", ",").IsNumeral()) { return DoubleParse(formel.Replace(".", ",")); }
        // TMP = Math.Max(Math.Max(-1, Formel.LastIndexOf("=")), Math.Max(Formel.LastIndexOf("<"), Formel.LastIndexOf(">")));
        var tmp = Math.Max(formel.LastIndexOf("+"), LastMinusIndex(formel));
        if (tmp < 0) { tmp = Math.Max(formel.LastIndexOf("/"), formel.LastIndexOf("*")); }
        if (tmp < 1) { return null; }
        // --------------------------------------------------------------------------------------------------------------------------------
        // --- Berechnung nötig, String Splitten berechnen und das Ergebnis zurückgeben
        // --------------------------------------------------------------------------------------------------------------------------------
        var seperator = formel.Substring(tmp, 1);
        // if (Seperator == "<" || Seperator == ">" || Seperator == "=")
        // {
        //    if (TMP < 1 || TMP > Formel.Length - 2) { return null; }
        // var sep2 = Formel.Substring(TMP - 1, 1);
        //    if (sep2 == "<" || sep2 == ">" || sep2 == "=") { TMP--; }
        //    sep2 = Formel.Substring(TMP + 1, 1);
        //    if (sep2 == "<" || sep2 == ">" || sep2 == "=") { Seperator = Formel.Substring(TMP, 2); }
        // }
        var w1 = ErgebnisCore(formel.Substring(0, tmp));
        if (w1 == null) { return null; }
        var w2 = ErgebnisCore(formel.Substring(tmp + seperator.Length));
        if (w2 == null) { return null; }
        switch (seperator) {
            case "/":
                if (w2 == 0) { return null; }
                return w1 / w2;

            case "*":
                return w1 * w2;

            case "-":
                return w1 - w2;

            case "+":
                return w1 + w2;
            // case ">":
            //    if (w1 > w2) { return -1; }
            //    return 0;
            // case ">=":
            // case "=>":
            //    if (w1 >= w2) { return -1; }
            //    return 0;
            // case "<":
            //    if (w1 < w2) { return -1; }
            //    return 0;
            // case "<=":
            // case "=<":
            //    if (w1 <= w2) { return -1; }
            //    return 0;
            // case "=":
            //    if (w1 == w2) { return -1; }
            //    return 0;
            // case "<>":
            // case "><":
            //    if (w1 != w2) { return -1; }
            //    return 0;
        }
        return null;
    }

    #endregion
}