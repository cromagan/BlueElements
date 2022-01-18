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

using BlueBasics;
using Skript.Enums;
using System;
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueScript {

    internal class Method_if : Method {

        #region Fields

        public static readonly List<string> OderOder = new() { "||" };

        public static readonly List<string> UndUnd = new() { "&&" };

        /// <summary>
        /// Vergleichsopeatoren in der richtigen Rang-Reihenfolge
        // https://de.wikipedia.org/wiki/Operatorrangfolge
        /// </summary>
        public static readonly List<string> VergleichsOperatoren = new() { "==", "!=", ">=", "<=", "<", ">", "!", "&&", "||" };

        #endregion

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Bool };
        public override string Description => "Nur wenn der Wert in der Klammer TRUE ist, wird der nachfolgende Codeblock ausgeführt. Es werden IMMER alle Vergleichsoperatoren aufgelöst. Deswegen sind Verschachtelungen mit Voricht zu verwenden - z.B. mir einem Exists-Befehl.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => true;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "if (true) { Code zum Ausführen }";

        #endregion

        #region Methods

        public static string? GetBool(string txt) {
            txt = txt.DeKlammere(true, false, false, true);

            switch (txt.ToLower()) {
                case "!false":
                case "true":
                    return "true";

                case "!true":
                case "false":
                    return "false";
            }

            return null;
        }

        //    #region Klammern zuerst berechnen und auflösen

        //    (var posa, var _) = NextText(txt, 0, KlammerAuf, false, false, KlammernStd);
        //    (var pose, var _) = NextText(txt, Math.Max(posa, 0), KlammerZu, false, false, KlammernStd);
        //    if (posa > -1 || pose > -1) {
        //        if (pose < posa) { return null; }
        //        var tmp = GetBool(txt.Substring(posa + 1, pose - posa - 1));
        //        return tmp == null ? null : GetBool(txt.Substring(0, posa) + tmp + txt.Substring(pose + 1));
        //    }

        //    #endregion Klammern zuerst berechnen

        //    return null;
        //}

        public override List<string> Comand(Script s) => new() { "if" };

        //public static readonly List<string> Vorbidden = new() { "exists", "istype", "isnullorempty", "isnullorzero" };
        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }
            if (attvar.Attributes[0].ValueBool) {
                (var err, var _) = s.Parse(infos.CodeBlockAfterText, false);
                if (!string.IsNullOrEmpty(err)) { return new strDoItFeedback(err); }
            } else {
                s.Line += infos.LineBreakInCodeBlock;
            }
            return new strDoItFeedback(string.Empty);
        }

        #endregion
    }
}