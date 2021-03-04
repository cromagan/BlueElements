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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;



namespace BlueScript {
    public class Script {

        /*
        IsNullOrEmpty(Var);
        Contains(List, Value);
        SetError(Text, Spalte1, Spalte2, ...);
         * 
         */
        public string Error { get; private set; }

        string _ScriptText = string.Empty;

        public static IEnumerable<Method> Comands;

        public readonly List<Variable> Variablen;

        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class {
            var objects = new List<T>();
            foreach (var type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))) {
                objects.Add((T)Activator.CreateInstance(type, constructorArgs));
            }
            //objects.Sort();
            return objects;
        }


        public Script(List<Variable> variablen) {


            Comands = GetEnumerableOfType<Method>(this);

            Variablen = variablen;
        }

        public string ScriptText {
            get {
                return _ScriptText;
            }
            set {

                if (_ScriptText == value) { return; }
                //_parsed = false;
                _ScriptText = value;
            }
        }



        private static string ReduceText(string txt) {


            var s = new System.Text.StringBuilder();

            var gänsef = false;
            var comment = false;


            for (var pos = 0; pos < txt.Length; pos++) {

                var c = txt.Substring(pos, 1);
                var addt = true;

                switch (c) {
                    case "\"":
                        if (!comment) { gänsef = !gänsef; }
                        break;

                    case "/":
                        if (!gänsef) {
                            if (pos < txt.Length - 1 && txt.Substring(pos, 2) == "//") { comment = true; }
                        }
                        break;

                    case "\r":
                        if (!gänsef) {
                            comment = false;
                            addt = false;
                        }
                        break;

                    case " ":
                    case "\n":
                    case "\t":
                        if (!gänsef) { addt = false; }
                        break;
                }

                if (!comment && addt) {
                    s.Append(c);
                }

            }

            return s.ToString();
        }


        public static string Parse(string scriptText, List<Variable> variablen) {
            var pos = 0;


            var tmptxt = ReduceText(scriptText);


            do {
                if (pos >= tmptxt.Length) { return string.Empty; }


                var f = ComandOnPosition(tmptxt, pos, variablen, string.Empty);

                if (!string.IsNullOrEmpty(f.ErrorMessage)) {
                    return f.ErrorMessage;
                }
                pos = f.Position;


            } while (true);
        }

        public bool Parse() {
            Error = Parse(_ScriptText, Variablen);
            return !string.IsNullOrEmpty(Error);
        }


        public static strDoItWithEndedPosFeedback ComandOnPosition(string txt, int pos, List<Variable> Variablen, string expectedvariablefeedback) {
            foreach (var thisC in Comands) {

                //if (!mustHaveFeedback || !thisC.ReturnsVoid) {

                var f = thisC.CanDo(txt, pos, expectedvariablefeedback);

                if (f.MustAbort) { return new strDoItWithEndedPosFeedback(f.ErrorMessage); }

                if (string.IsNullOrEmpty(f.ErrorMessage)) {
                    var fn = thisC.DoIt(f, Variablen);
                    return new strDoItWithEndedPosFeedback(fn.ErrorMessage, fn.Value, f.ContinueOrErrorPosition);
                }
                //}
            }
            return new strDoItWithEndedPosFeedback("Kann nicht geparsed werden: " + txt.Substring(pos));
        }


        public static (int pos, string witch) NextText(string txt, int startpos, List<string> searchfor, bool checkforSeparatorbefore, bool checkforSeparatorafter, bool ignoreKlammern) {

            var klammern = 0;
            var Gans = false;
            var GeschwKlammern = false;
            var EckigeKlammern = 0;

            var pos = startpos;

            var maxl = txt.Length;


            const string TR = ".,;\\?!\" ~|=<>+-(){}[]/*`´\r\n\t";


            do {

                if (pos >= maxl) { return (-1, string.Empty); ; }

                #region Klammer und " erledigen
                switch (txt.Substring(pos, 1)) {

                    // Gänsefüsschen, immer erlaubt
                    case "\"":
                        Gans = !Gans;
                        break;

                    // Ekige klammern könne in { oder ( vorkommen, immer erlaubt
                    case "[":
                        if (!Gans) {
                            EckigeKlammern++;
                        }
                        break;

                    case "]":
                        if (!Gans) {
                            if (EckigeKlammern <= 0) { return (-1, string.Empty); }
                            EckigeKlammern--;
                        }
                        break;


                    // Runde klammern können in { vorkommen
                    case "(":
                        if (!Gans && !ignoreKlammern) {
                            if (EckigeKlammern > 0) { return (-1, string.Empty); }
                            klammern++;
                        }

                        break;

                    case ")":
                        if (!Gans && !ignoreKlammern) {
                            if (EckigeKlammern > 0) { return (-1, string.Empty); }
                            if (klammern <= 0) { return (-1, string.Empty); }
                            klammern--;
                        }
                        break;


                    // Gescheifte klammern müssen immer sauber auf und zu gemacht werdrn!
                    case "{":
                        if (!Gans) {
                            if (klammern > 0) { return (-1, string.Empty); }
                            if (EckigeKlammern > 0) { return (-1, string.Empty); }
                            if (GeschwKlammern) { return (-1, string.Empty); }
                            GeschwKlammern = true;
                        }
                        break;

                    case "}":
                        if (!Gans) {
                            if (klammern > 0) { return (-1, string.Empty); }
                            if (EckigeKlammern > 0) { return (-1, string.Empty); }
                            if (!GeschwKlammern) { return (-1, string.Empty); }
                            GeschwKlammern = false;
                        }
                        break;
                }
                #endregion



                if (klammern == 0 && !Gans && !GeschwKlammern && EckigeKlammern == 0) {


                    if (!checkforSeparatorbefore || pos == 0 || TR.Contains(txt.Substring(pos - 1, 1))) {

                        foreach (var thisEnd in searchfor) {
                            if (pos + thisEnd.Length <= maxl) {

                                if (txt.Substring(pos, thisEnd.Length).ToLower() == thisEnd.ToLower()) {
                                    if (!checkforSeparatorafter || pos + thisEnd.Length >= maxl || TR.Contains(txt.Substring(pos + thisEnd.Length, 1))) {
                                        return (pos, thisEnd);
                                    }
                                }
                            }
                        }
                    }
                }

                pos++;
            } while (true);


        }














    }
}
