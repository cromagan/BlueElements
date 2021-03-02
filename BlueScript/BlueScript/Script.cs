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




        public strParseFeedback Parse() {

            var pos = 0;


            do {
                if (pos > _ScriptText.Length) { return new strParseFeedback(); }


                var f = ComandOnPosition(_ScriptText, pos, Variablen, false);

                if (!string.IsNullOrEmpty(f.ErrorMessage)) { return new strParseFeedback(pos, f.ErrorMessage); }
                pos = f.Position;




            } while (true);





        }


        public static strDoItWithEndedPosFeedback ComandOnPosition(string txt, int pos, List<Variable> Variablen, bool mustHaveFeedback) {
            foreach (var thisC in Comands) {

                if (!mustHaveFeedback || !thisC.ReturnsVoid) {

                    var f = thisC.CanDo(txt, pos, null);

                    if (f.MustAbort) { return new strDoItWithEndedPosFeedback(f.ErrorMessage); }

                    if (string.IsNullOrEmpty(f.ErrorMessage)) {
                        var fn = thisC.DoIt(f, Variablen, null);
                        return new strDoItWithEndedPosFeedback(fn.ErrorMessage, fn.Value, f.ContinueOrErrorPosition);
                    }
                }
            }
            return new strDoItWithEndedPosFeedback("Kann nicht geparsed werden.");
        }


        public static (int pos, string witch) NextText(string scriptText, int startpos, List<string> searchfor) {

            var klammern = 0;
            var Gans = false;
            var GeschwKlammern = false;
            var EckigeKlammern = 0;

            var pos = startpos - 1; // Letztes Zeichen noch berücksichtigen, könnte ja Klammer auf oder zu sein

            var maxl = scriptText.Length;

            do {

                if (pos > maxl) { return (-1, string.Empty); ; }

                #region Klammer und " erledigen
                switch (scriptText.Substring(pos)) {

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
                        if (!Gans) {
                            if (EckigeKlammern > 0) { return (-1, string.Empty); }
                            klammern++;
                        }

                        break;

                    case ")":
                        if (!Gans) {
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

                    foreach (var thisEnd in searchfor) {


                        if (pos + thisEnd.Length <= maxl) {

                            if (scriptText.Substring(pos, thisEnd.Length).ToLower() == thisEnd.ToLower()) {

                                return (pos, thisEnd);
                            }

                        }

                    }



                }

                pos++;
            } while (true);


        }














    }
}
