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

using BlueBasics;
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueScript {
    public class Method_SetError : BlueScript.Method {


        public Method_SetError(Script parent) : base(parent) { }

        public override string Syntax { get => "SetError(Nachricht, Column1, Colum2, ...);"; }


        public override List<string> Comand { get => new List<string>() { "seterror" }; }
        public override string StartSequence { get => "("; }
        public override string EndSequence { get => ");"; }
        public override bool GetCodeBlockAfter { get => false; }
        public override string Returns { get => string.Empty; }

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {

            if (string.IsNullOrEmpty(infos.AttributText)) { return new strDoItFeedback("Kein Text angekommen."); }

            var bs = SplitAttribute(infos.AttributText, s, 9999);

            if (bs == null || bs.Count < 2) { return new strDoItFeedback("'SetError' erwartet mindestens zwei Attribute: " + infos.AttributText); }


            for (var z = 1; z < bs.Count; z++) {

                var v = s.Variablen.Get(bs[z]);
                if (v == null) { return new strDoItFeedback("Spalte nicht gefunden: " + bs[z]); }

          
                var n = bs[z].ToLower() + "_error";

                var ve = s.Variablen.GetSystem(n);


                //foreach (var thisv in variablen) {
                //    if (thisv.Name == n) {
                //        ve = thisv; break;
                //    }
                //}


                if (ve == null) {
                    ve = new Variable(n, string.Empty, Skript.Enums.enVariableDataType.List, false, true);
                    s.Variablen.Add(ve);
                }

                var l = ve.ValueString.SplitByCRToList();
                l.AddIfNotExists(bs[0].Trim("\""));
                ve.ValueString = l.JoinWithCr();

            }



            return new strDoItFeedback();
        }
    }
}
