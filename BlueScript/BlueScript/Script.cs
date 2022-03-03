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
using BlueBasics.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BlueScript {

    public class Script {

        #region Fields

        public static List<Method> Comands;
        public readonly List<Variable> Variablen;
        public bool EndSkript;

        /// <summary>
        /// Dieses Feld enthält informationen, die nach dem Skript-Lauf abgegriffen werden können.
        /// </summary>
        public string Feedback = string.Empty;

        internal readonly List<Bitmap?> BitmapCache;
        internal Method_BerechneVariable _berechneVariable; // Paralellisierung löscht ab und zu die Variable
        private string _error;
        private string _errorCode;

        #endregion

        #region Constructors

        //public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class {
        //    var objects = new List<T>();
        //    IEnumerable<Type> types = null;
        //    try {
        //        types =
        //           from a in AppDomain.CurrentDomain.GetAssemblies()
        //           from t in a.GetTypes()
        //           select t;
        //    }
        //    catch (Exception ex) {
        //        Develop.DebugPrint(ex);
        //        if (ex is System.Reflection.ReflectionTypeLoadException typeLoadException) {
        //            Develop.DebugPrint(typeLoadException.LoaderExceptions.ToString());
        //            //var loaderExceptions = typeLoadException.LoaderExceptions;
        //        }
        //        return objects;
        //    }
        //    foreach (var type in types.Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))) {
        //        objects.Add((T)Activator.CreateInstance(type, constructorArgs));
        //    }
        //    //foreach( var thisa in  AppDomain.CurrentDomain.GetAssemblies())
        //    //       {
        //    //   foreach (var type in
        //    //       Assembly.GetAssembly(typeof(T)).GetTypes()
        //    //       .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))) {
        //    //       objects.Add((T)Activator.CreateInstance(type, constructorArgs));
        //    //   }
        //    //   //objects.Sort();
        //    return objects;
        //}
        public Script(List<Variable> variablen) {
            if (Comands == null) {
                Comands = GetEnumerableOfType<Method>();
            }
            Variablen = variablen;
            BitmapCache = new List<Bitmap?>();
        }

        #endregion

        #region Properties

        public bool BreakFired { get; internal set; }

        /// <summary>
        /// Es sind keine {} in diesem Text erlaubt und werden eliminiert
        /// </summary>
        public string Error {
            get => _error;
            private set => _error = value.Replace("{", "").Replace("}", "");
        }

        /// <summary>
        /// Es sind keine {} in diesem Text erlaubt und werden eliminiert
        /// </summary>
        public string ErrorCode {
            get => _errorCode;
            private set => _errorCode = value.Replace("{", "").Replace("}", "");
        }

        public int Line { get; internal set; }

        public string ReducedScriptText { get; private set; }
        public int Schleife { get; internal set; }

        public string ScriptText { get; set; } = string.Empty;

        public int Sub { get; internal set; }

        #endregion

        #region Methods

        public static strDoItWithEndedPosFeedback ComandOnPosition(string txt, int pos, Script s, bool expectedvariablefeedback) {
            foreach (var thisC in Comands) {
                var f = thisC.CanDo(txt, pos, expectedvariablefeedback, s);
                if (f.MustAbort) { return new strDoItWithEndedPosFeedback(f.ErrorMessage); }
                if (string.IsNullOrEmpty(f.ErrorMessage)) {
                    var fn = thisC.DoIt(f, s);
                    return new strDoItWithEndedPosFeedback(fn.ErrorMessage, fn.Value, f.ContinueOrErrorPosition);
                }
            }

            #region Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

            foreach (var f in Comands.Select(thisC => thisC.CanDo(txt, pos, !expectedvariablefeedback, s))) {
                if (f.MustAbort) { return new strDoItWithEndedPosFeedback(f.ErrorMessage); }
                if (string.IsNullOrEmpty(f.ErrorMessage)) {
                    return expectedvariablefeedback
                        ? new strDoItWithEndedPosFeedback("Dieser Befehl hat keinen Rückgabewert: " + txt.Substring(pos))
                        : new strDoItWithEndedPosFeedback("Dieser Befehl hat einen Rückgabewert, der nicht verwendet wird: " + txt.Substring(pos));
                }
            }

            #endregion Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

            return new strDoItWithEndedPosFeedback("Kann nicht geparsed werden: " + txt.Substring(pos));
        }

        public static List<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class {
            List<T> l = new();
            foreach (var thisas in AppDomain.CurrentDomain.GetAssemblies()) {
                try {
                    foreach (var thist in thisas.GetTypes()) {
                        if (thist.IsClass && !thist.IsAbstract && thist.IsSubclassOf(typeof(T))) {
                            l.Add((T)Activator.CreateInstance(thist, constructorArgs));
                        }
                    }
                } catch (Exception ex) {
                    Develop.DebugPrint(enFehlerArt.Info, ex);
                }
            }
            return l;
        }

        public static string ReduceText(string txt) {
            System.Text.StringBuilder s = new();
            var gänsef = false;
            var comment = false;

            txt = txt.RemoveEscape();// muss am Anfang gemacht werden, weil sonst die Zählweise nicht mehr stimmt

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
                        if (gänsef) { s.Append("\";Exception(\"Fehler mit Anführungsstrichen\");"); }
                        s.Append("¶");
                        comment = false;
                        addt = false;
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

        public bool Parse() {
            ReducedScriptText = ReduceText(ScriptText);
            Line = 1;
            BreakFired = false;
            Schleife = 0;
            Sub = 0;
            Feedback = string.Empty;

            _berechneVariable = null;

            foreach (var thisC in Comands) {
                if (thisC is Method_BerechneVariable bv) { _berechneVariable = bv; }
            }

            if (_berechneVariable == null) {
                Develop.DebugPrint(enFehlerArt.Fehler, "Method_BerechneVariable ist nicht definiet.");
            }

            (Error, ErrorCode) = Parse(ReducedScriptText);
            return !string.IsNullOrEmpty(Error);
        }

        internal int AddBitmapToCache(Bitmap? bmp) {
            BitmapCache.Add(bmp);
            return BitmapCache.IndexOf(bmp);
        }

        internal (string, string) Parse(string redScriptText) {
            var pos = 0;
            EndSkript = false;

            do {
                if (pos >= redScriptText.Length || EndSkript) {
                    return (string.Empty, string.Empty);
                }

                if (BreakFired) { return (string.Empty, string.Empty); }

                if (redScriptText.Substring(pos, 1) == "¶") {
                    Line++;
                    pos++;
                } else {
                    var f = ComandOnPosition(redScriptText, pos, this, false);
                    if (!string.IsNullOrEmpty(f.ErrorMessage)) {
                        return (f.ErrorMessage, ReducedScriptText.Substring(pos, Math.Min(30, ReducedScriptText.Length - pos)));
                    }
                    pos = f.Position;
                }
            } while (true);
        }

        #endregion
    }
}