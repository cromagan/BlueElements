﻿#region BlueElements - a collection of useful tools, database and controls
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlueBasics.modAllgemein;
using static BlueBasics.Extensions;
using BlueBasics;
using static BlueBasics.modConverter;

namespace BlueScript {
    class Method_if : Method {


        public Method_if(Script parent) : base(parent) { }


        //public Method_var(Script parent, string toParse) : base(parent, toParse) { }

        //public override string ID { get => "if"; }
        public override List<string> Comand { get => new List<string>() { "if" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ")"; }
        //public override List<string> AllowedInIDs { get => null; }
        public override bool GetCodeBlockAfter { get => true; }
        public override string Returns { get => string.Empty; }




        internal override strDoItFeedback DoIt(strCanDoFeedback infos, List<Variable> variablen) {

            if (string.IsNullOrEmpty(infos.AttributText)) { return new strDoItFeedback("Kein Text angekommen."); }

            var bs = SplitAttribute(infos.AttributText, variablen);

            if (bs == null || bs.Count != 1) { return new strDoItFeedback("Attributfehler bei " + infos.ComandText + ": " + infos.AttributText); }


            var x = GetBool(bs[0]);
            if (x == null) { return new strDoItFeedback("Attributfehler bei " + infos.ComandText + ": " + infos.AttributText); }


            if (x == "true") {
                var err = Script.Parse(infos.CodeBlockAfterText, variablen);
                if (!string.IsNullOrEmpty(err)) { return new strDoItFeedback("Fehler im If-Codeblock"); }
            }

            return new strDoItFeedback(string.Empty, string.Empty);
        }



        public string? GetBool(string txt) {

            switch (txt.ToLower()) {
                case "true": return "true";
                case "false": return "false";
                case "true&&true": return "true";
                case "true&&false": return "false";
                case "false&&true": return "false";
                case "false&&false": return "false";
                case "true||true": return "true";
                case "true||false": return "true";
                case "false||true": return "true";
                case "false||false": return "false";
                case "!true": return "false";
                case "!false": return "true";
            }



            var posa = txt.IndexOf("(");
            if (posa > -1) {
                var (pose, witche) = Script.NextText(txt, posa, new List<string>() { ")" }, false, false, false);

                if (pose < posa) { return null; }

                var tmp = GetBool(txt.Substring(posa + 1, pose - posa - 1));
                if (tmp == null) { return null; }

                return GetBool(txt.Substring(0, posa) + (string)tmp + txt.Substring(pose+1));

            }




            string ntxt;


            ntxt = GetBoolTMP(txt, "==");
            if (!string.IsNullOrEmpty(ntxt)) { return GetBool(ntxt); }

            ntxt = GetBoolTMP(txt, ">=");
            if (!string.IsNullOrEmpty(ntxt)) { return GetBool(ntxt); }

            ntxt = GetBoolTMP(txt, "<=");
            if (!string.IsNullOrEmpty(ntxt)) { return GetBool(ntxt); }

            ntxt = GetBoolTMP(txt, "!=");
            if (!string.IsNullOrEmpty(ntxt)) { return GetBool(ntxt); }

            ntxt = GetBoolTMP(txt, "<");
            if (!string.IsNullOrEmpty(ntxt)) { return GetBool(ntxt); }

            ntxt = GetBoolTMP(txt, ">");
            if (!string.IsNullOrEmpty(ntxt)) { return GetBool(ntxt); }

            ntxt = GetBoolTMP(txt, "||");
            if (!string.IsNullOrEmpty(ntxt)) { return GetBool(ntxt); }

            ntxt = GetBoolTMP(txt, "&&");
            if (!string.IsNullOrEmpty(ntxt)) { return GetBool(ntxt); }

            return null;

        }




        string GetBoolTMP(string txt, string check) {
            var i = txt.IndexOf(check);

            if (i < 1) { return string.Empty; } // <1, weil ja mindestens ein Zeichen vorher sein MUSS!

            if (i >= txt.Length - 1) { return string.Empty; } // siehe oben

            var start = i - 1;
            var ende = i + check.Length;
            var trenn = "(!|&<>=)";

            do {
                if (start < 0) { break; }
                if (trenn.Contains(txt.Substring(start, 1))) { break; }
                start--;
            } while (true);

            do {
                if (ende >= txt.Length) { break; }
                if (trenn.Contains(txt.Substring(ende, 1))) { break; }
                ende++;
            } while (true);

            var s1 = txt.Substring(start + 1, i - start - 1);
            var s2 = txt.Substring(i + check.Length, ende - check.Length - i);


            if (string.IsNullOrEmpty(s1)) { return string.Empty; }
            if (string.IsNullOrEmpty(s2)) { return string.Empty; }

            var replacer = string.Empty;

            switch (check) {
                case "==":
                    replacer = "false";
                    if (s1 == s2) { replacer = "true"; }

                    if (s1.IsNumeral()) {
                        if (s2.IsNumeral()) {
                            if (FloatParse(s1) == FloatParse(s2)) { replacer = "true"; }
                        }
                        else {
                            replacer = string.Empty;
                        }
                    }
                    break;

                case "!=":
                    replacer = "false";
                    if (s1 != s2) { replacer = "true"; }

                    if (s1.IsNumeral()) {
                        if (s2.IsNumeral()) {
                            if (FloatParse(s1) != FloatParse(s2)) { replacer = "true"; }
                        }
                        else {
                            replacer = string.Empty;
                        }
                    }
                    break;

                case ">=":
                    if (s1.IsNumeral() && s2.IsNumeral()) {
                        replacer = "false";
                        if (FloatParse(s1) >= FloatParse(s2)) { replacer = "true"; }
                    }
                    break;

                case "<=":
                    if (s1.IsNumeral() && s2.IsNumeral()) {
                        replacer = "false";
                        if (FloatParse(s1) <= FloatParse(s2)) { replacer = "true"; }
                    }
                    break;

                case "<":
                    if (s1.IsNumeral() && s2.IsNumeral()) {
                        replacer = "false";
                        if (FloatParse(s1) < FloatParse(s2)) { replacer = "true"; }
                    }
                    break;

                case ">":
                    if (s1.IsNumeral() && s2.IsNumeral()) {
                        replacer = "false";
                        if (FloatParse(s1) > FloatParse(s2)) { replacer = "true"; }
                    }
                    break;

                case "||":
                    if (s1=="true" || s1 =="false") {
                        if (s2 == "true" || s2 == "false") {
                            replacer = (string) GetBool(s1 + check + s2);
                        }
                    }
                    break;

                case "&&":
                    if (s1 == "true" || s1 == "false") {
                        if (s2 == "true" || s2 == "false") {
                            replacer = (string)GetBool(s1 + check + s2);
                        }
                    }
                    break;


                default:
                    Develop.DebugPrint_NichtImplementiert();
                    break;

            }


            if (string.IsNullOrEmpty(replacer)) { return string.Empty; }


            return txt.Substring(0, start + 1) + replacer + txt.Substring(ende);



        }

    }
}