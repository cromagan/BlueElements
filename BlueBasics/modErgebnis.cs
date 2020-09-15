#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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

namespace BlueBasics
{
    public static class modErgebnis
    {

        public static double? Ergebnis(string Formel)
        {
            Formel = Formel.ToUpper();
            Formel = Formel.Replace(" ", "");
            if (string.IsNullOrEmpty(Formel)) { return null; }
            Formel = Formel.Replace("INT(", "XNT(0,");
            Formel = Formel.Replace("RND()", "XND(0,1)");
            return ErgebnisCore(Formel);
        }



        private static double? ErgebnisCore(string Formel)
        {
            var TMP = 0;

            //Das alles kann nur möglich sein, WENN eine Klammer vorhanden ist
            if (Formel.Contains("("))
            {
                if (Formel.StartsWith("(") && Formel.IndexOf(")") == Formel.Length - 1)
                {
                    return ErgebnisCore(Formel.Substring(1, Formel.Length - 2)); // Unnötige Klammern entfernen und noch Ne Runde!!!!
                }


                // --------------------------------------------------------------------------------------------------------------------------------
                // --- Eine Klammer auflösen, im Formelstring ersetzen und         mittels Rekursivität die nun einfachere Formel berechnen.
                // --------------------------------------------------------------------------------------------------------------------------------
                var a = Formel.LastIndexOf("(");
                var e = Formel.IndexOf(")", a);

                if (a >= e) { return null; }

                double? Replacer = 0;

                if (a > 2 && Formel.IndexOf(",", a) > a && Formel.IndexOf(",", a) < e)
                {

                    // --------------------------------------------------------------------------------------------------------------------------------
                    // --- Ok, Funktion kommt, erkannt an den beinhaltenden Kommas.
                    // --- Es ist die Letzte Klammer, deswegen KANN in der Funktion keine mehr sein.
                    // --- Und die Kommas stellen den Seperator dar.
                    // --------------------------------------------------------------------------------------------------------------------------------
                    var Att = Formel.Substring(a + 1, e - a - 1).SplitBy(",");
                    var Att2 = new double?[Att.Length];

                    for (TMP = 0; TMP < Att.Length; TMP++)
                    {
                        var qq = ErgebnisCore(Att[TMP]);
                        if (qq == null) { return null; }
                        Att2[TMP] = qq;
                    }

                    Replacer = Att2[0];

                    switch (Formel.Substring(a - 3, 4))
                    {
                        case "MIN(":
                            for (TMP = 1; TMP < Att2.Length; TMP++)
                            {
                                if (Att2[TMP] < Replacer) { Replacer = Att2[TMP]; }
                            }

                            break;
                        case "MAX(":
                            for (TMP = 1; TMP < Att2.Length; TMP++)
                            {
                                if (Att2[TMP] > Replacer) { Replacer = Att2[TMP]; }
                            }

                            break;
                        case "XNT(":
                            if (Att2.Length != 2) { return null; }
                            Replacer = Math.Floor((double)Att2[1]);

                            break;
                        case "BTW(": // Between. Format: BTW(IsValue, MinValue, MaxValue)
                            if (Att2.Length != 3) { return null; }
                            if (Att2[0] >= Att2[1] && Att2[0] <= Att2[2])
                            {
                                Replacer = -1;
                            }
                            else
                            {
                                Replacer = 0;
                            }

                            break;
                        case "IFF(":
                            if (Att2.Length != 3) { return null; }

                            if (Att2[0] == -1)
                            {
                                Replacer = Att2[1];
                            }
                            else
                            {
                                Replacer = Att2[2];
                            }

                            break;

                        case "XND(":
                            if (Att2.Length != 2) { return null; }
                            Replacer = Constants.GlobalRND.NextDouble();
                            break;

                        default:
                            return null;
                    }

                    a -= 3;
                }
                else // Es ist KEINE Funktion, also den Inhalt der Klammer normal berechnen
                {
                    Replacer = ErgebnisCore(Formel.Substring(a + 1, e - a - 1));
                }

                if (Replacer == null) { return null; }


                Formel = Formel.Replace(Formel.Substring(a, e - a + 1), ((double)Replacer).ToString("F99").TrimEnd('0').TrimEnd(',').Replace(",", "."));
                return ErgebnisCore(Formel);
            } // Ende Klammer Vorhanden-----------------------------------------------------------

            // --------------------------------------------------------------------------------------------------------------------------------
            // --- Prüfen, ob überhaupt eine Berechnung nötig ist. Z.B. wenn unnötige Klammern aufgelöst wurden. ------------------------------
            // --------------------------------------------------------------------------------------------------------------------------------
            if (Formel.Replace(".", ",").IsNumeral()) { return double.Parse(Formel.Replace(".", ",")); }
            TMP = Math.Max(Math.Max(-1, Formel.LastIndexOf("=")), Math.Max(Formel.LastIndexOf("<"), Formel.LastIndexOf(">")));
            if (TMP < 0) { TMP = Math.Max(Formel.LastIndexOf("+"), LastMinusIndex(Formel)); }
            if (TMP < 0) { TMP = Math.Max(Formel.LastIndexOf("/"), Formel.LastIndexOf("*")); }
            if (TMP < 1) { return null; }


            // --------------------------------------------------------------------------------------------------------------------------------
            // --- Berechnung nötig, String Splitten berechnen und das Ergebnis zurückgeben
            // --------------------------------------------------------------------------------------------------------------------------------
            var Seperator = Formel.Substring(TMP, 1);

            if (Seperator == "<" || Seperator == ">" || Seperator == "=")
            {
                if (TMP < 1 || TMP > Formel.Length - 2) { return null; }

                var sep2 = Formel.Substring(TMP - 1, 1);
                if (sep2 == "<" || sep2 == ">" || sep2 == "=") { TMP -= 1; }
                sep2 = Formel.Substring(TMP + 1, 1);
                if (sep2 == "<" || sep2 == ">" || sep2 == "=") { Seperator = Formel.Substring(TMP, 2); }
            }

            var w1 = ErgebnisCore(Formel.Substring(0, TMP));
            if (w1 == null) { return null; }


            var w2 = ErgebnisCore(Formel.Substring(TMP + Seperator.Length));
            if (w2 == null) { return null; }





            switch (Seperator)
            {
                case "/":
                    if (w2 == 0) { return null; }
                    return w1 / w2;
                case "*":
                    return w1 * w2;
                case "-":
                    return w1 - w2;
                case "+":
                    return w1 + w2;
                case ">":
                    if (w1 > w2) { return -1; }
                    return 0;
                case ">=":
                case "=>":
                    if (w1 >= w2) { return -1; }
                    return 0;
                case "<":
                    if (w1 < w2) { return -1; }
                    return 0;
                case "<=":
                case "=<":
                    if (w1 <= w2) { return -1; }
                    return 0;
                case "=":
                    if (w1 == w2) { return -1; }
                    return 0;
                case "<>":
                case "><":
                    if (w1 != w2) { return -1; }
                    return 0;
            }

            return null;
        }


        public static int LastMinusIndex(string Formel)
        {
            if (Formel.Contains("-") == false) { return -1; }

            var LastMin = 1;
            var OkMin = -1;

            while (true)
            {
                LastMin = Formel.IndexOf("-", LastMin);
                if (LastMin < 1) { break; }

                var VorZ = Formel.Substring(LastMin - 1, 1);
                if (VorZ.IsNumeral()) { OkMin = LastMin; }
                LastMin += 1;
            }
            return OkMin;
        }



    }

}