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

using BlueBasics;
using Skript.Enums;
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueScript
{
    internal class Method_if : Method
    {

        public override string Syntax => "if (true) { Code zum ausführen }";
        public override string Description => "Nur wenn der Wert in der Klammer TRUE ist, wird der nachfolgende Codeblock ausgeführt.";
        public override List<string> Comand(Script s) { return new() { "if" }; }
        public override string StartSequence => "(";
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => true;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override List<enVariableDataType> Args => new() { enVariableDataType.Bool };
        public override bool EndlessArgs => false;


        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s)
        {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args);
            if (attvar == null) { return strDoItFeedback.AttributFehler(); }

            if (attvar[0].ValueBool)
            {
                (var err, var ermess2) = Script.Parse(infos.CodeBlockAfterText, false, s);
                if (!string.IsNullOrEmpty(err)) { return new strDoItFeedback(err); }
            }
            else
            {
                s.Line += infos.LineBreakInCodeBlock;
            }
            return new strDoItFeedback(string.Empty, string.Empty);
        }



        public static string? GetBool(string txt)
        {
            txt = txt.DeKlammere(true, false, false);

            switch (txt.ToLower())
            {
                case "true":
                    return "true";
                case "false":
                    return "false";
                case "true&&true":
                    return "true";
                case "true&&false":
                    return "false";
                case "false&&true":
                    return "false";
                case "false&&false":
                    return "false";
                case "true||true":
                    return "true";
                case "true||false":
                    return "true";
                case "false||true":
                    return "true";
                case "false||false":
                    return "false";
                case "!true":
                    return "false";
                case "!false":
                    return "true";
            }


            #region Klammern zuerst berechnen
            (var posa, var _) = Script.NextText(txt, 0, new List<string>() { "(" }, false, false);
            if (posa > -1)
            {
                (var pose, var _) = Script.NextText(txt, posa, new List<string>() { ")" }, false, false);

                if (pose < posa) { return null; }

                var tmp = GetBool(txt.Substring(posa + 1, pose - posa - 1));
                if (tmp == null) { return null; }

                return GetBool(txt.Substring(0, posa) + tmp + txt.Substring(pose + 1));

            }
            #endregion




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

            ntxt = GetBoolTMP(txt, "!");
            if (!string.IsNullOrEmpty(ntxt)) { return GetBool(ntxt); }

            ntxt = GetBoolTMP(txt, "||");
            if (!string.IsNullOrEmpty(ntxt)) { return GetBool(ntxt); }

            ntxt = GetBoolTMP(txt, "&&");
            if (!string.IsNullOrEmpty(ntxt)) { return GetBool(ntxt); }

            return null;

        }

        private static string GetBoolTMP(string txt, string check)
        {
            (var i, var _) = Script.NextText(txt, 0, new List<string>() { check }, false, false);


            if (i < 0) { return string.Empty; }

            if (i < 1 && check != "!") { return string.Empty; } // <1, weil ja mindestens ein Zeichen vorher sein MUSS!

            if (i >= txt.Length - 1) { return string.Empty; } // siehe oben

            var start = i - 1;
            var ende = i + check.Length;
            var trenn = "(!|&<>=)";

            var gans = false;

            do
            {
                if (start < 0) { break; }
                var ze = txt.Substring(start, 1);
                if (!gans && trenn.Contains(ze)) { break; }
                if (ze == "\"") { gans = !gans; }
                start--;
            } while (true);

            do
            {
                if (ende >= txt.Length) { break; }
                var ze = txt.Substring(ende, 1);
                if (!gans && trenn.Contains(ze)) { break; }
                if (ze == "\"") { gans = !gans; }
                ende++;
            } while (true);

            var s1 = txt.Substring(start + 1, i - start - 1);
            var s2 = txt.Substring(i + check.Length, ende - check.Length - i);


            if (string.IsNullOrEmpty(s1) && check != "!") { return string.Empty; }
            if (string.IsNullOrEmpty(s2)) { return string.Empty; }


            Variable v1 = null;
            if (check != "!") { v1 = new Variable("dummy", s1, null); }
            var v2 = new Variable("dummy", s2, null);


            // V2 braucht nicht gepürft werden, muss ja eh der gleiche TYpe wie V1 sein
            if (v1 != null)
            {
                if (v1.Type != v2.Type) { return string.Empty; }

                if (v1.Type != Skript.Enums.enVariableDataType.Bool &&
                    v1.Type != Skript.Enums.enVariableDataType.Number &&
                    v1.Type != Skript.Enums.enVariableDataType.String) { return string.Empty; }
            }
            else
            {
                if (v2.Type != Skript.Enums.enVariableDataType.Bool) { return string.Empty; }
            }


            var replacer = string.Empty;

            switch (check)
            {
                case "==":
                    replacer = "false";
                    if (v1.ValueString == v2.ValueString) { replacer = "true"; }

                    //if (s1.IsNumeral()) {
                    //    if (s2.IsNumeral()) {
                    //        if (FloatParse(s1) == FloatParse(s2)) { replacer = "true"; }
                    //    }
                    //    else {
                    //        replacer = string.Empty;
                    //    }
                    //}
                    break;

                case "!=":
                    replacer = "false";
                    if (v1.ValueString != v2.ValueString) { replacer = "true"; }

                    //if (s1.IsNumeral()) {
                    //    if (s2.IsNumeral()) {
                    //        if (FloatParse(s1) != FloatParse(s2)) { replacer = "true"; }
                    //    }
                    //    else {
                    //        replacer = string.Empty;
                    //    }
                    //}
                    break;

                case ">=":
                    if (v1.Type != Skript.Enums.enVariableDataType.Number) { return string.Empty; }
                    replacer = "false";
                    if (v1.ValueDouble >= v2.ValueDouble) { replacer = "true"; }
                    break;

                case "<=":
                    if (v1.Type != Skript.Enums.enVariableDataType.Number) { return string.Empty; }
                    replacer = "false";
                    if (v1.ValueDouble <= v2.ValueDouble) { replacer = "true"; }
                    break;

                case "<":
                    if (v1.Type != Skript.Enums.enVariableDataType.Number) { return string.Empty; }
                    replacer = "false";
                    if (v1.ValueDouble < v2.ValueDouble) { replacer = "true"; }
                    break;

                case ">":
                    if (v1.Type != Skript.Enums.enVariableDataType.Number) { return string.Empty; }
                    replacer = "false";
                    if (v1.ValueDouble > v2.ValueDouble) { replacer = "true"; }
                    break;

                case "||":
                    if (v1.Type != Skript.Enums.enVariableDataType.Bool) { return string.Empty; }
                    replacer = "false";
                    if (v1.ValueBool || v2.ValueBool) { replacer = "true"; }
                    break;

                case "&&":
                    if (v1.Type != Skript.Enums.enVariableDataType.Bool) { return string.Empty; }
                    replacer = "false";
                    if (v1.ValueBool && v2.ValueBool) { replacer = "true"; }
                    break;

                case "!":
                    // S1 dürfte eigentlich nie was sein: !False||!false
                    // entwederist es ganz am anfang, oder direkt nach einem Trenneichen
                    if (v2.Type != Skript.Enums.enVariableDataType.Bool) { return string.Empty; }
                    replacer = "false";
                    if (!v2.ValueBool) { replacer = "true"; }

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
