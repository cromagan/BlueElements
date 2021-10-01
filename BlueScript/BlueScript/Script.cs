// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

namespace BlueScript {

    public class Script {

        #region Fields

        public static List<Method> Comands = null;
        public readonly List<Variable> Variablen;
        public bool EndSkript = false;
        internal readonly List<Bitmap> BitmapCache;
        internal Method_BerechneVariable _berechneVariable = null; // Paralellisierung löscht ab und zu die Variable
        private string _error;
        private string _errorCode;
        private string _ScriptText = string.Empty;

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
            BitmapCache = new();
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

        public int Schleife { get; internal set; }

        public string ScriptText {
            get => _ScriptText;
            set {
                if (_ScriptText == value) { return; }
                //_parsed = false;
                _ScriptText = value;
            }
        }

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

            foreach (var thisC in Comands) {
                var f = thisC.CanDo(txt, pos, !expectedvariablefeedback, s);
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

        public bool Parse() {
            (Error, ErrorCode) = Parse(_ScriptText, true);
            return !string.IsNullOrEmpty(Error);
        }

        internal int AddBitmapToCache(Bitmap bmp) {
            BitmapCache.Add(bmp);
            return BitmapCache.IndexOf(bmp);
        }

        internal (string, string) Parse(string scriptText, bool reduce) {
            var pos = 0;
            EndSkript = false;

            string tmpScript;
            if (reduce) {
                tmpScript = ReduceText(scriptText);
                Line = 1;
                BreakFired = false;
                Schleife = 0;
                Variablen.PrepareForScript();

                _berechneVariable = null;

                foreach (var thisC in Comands) {
                    if (thisC is Method_BerechneVariable bv) { _berechneVariable = bv; }
                }

                if (_berechneVariable == null) {
                    Develop.DebugPrint(enFehlerArt.Fehler, "Method_BerechneVariable ist nicht definiet.");
                }
            } else {
                tmpScript = scriptText;
            }

            do {
                if (pos >= tmpScript.Length || EndSkript) {
                    if (reduce) { Variablen.ScriptFinished(); }
                    return (string.Empty, string.Empty);
                }

                if (BreakFired) { return (string.Empty, string.Empty); }

                if (tmpScript.Substring(pos, 1) == "¶") {
                    Line++;
                    pos++;
                } else {
                    var f = ComandOnPosition(tmpScript, pos, this, false);
                    if (!string.IsNullOrEmpty(f.ErrorMessage)) {
                        if (reduce) { Variablen.ScriptFinished(); }
                        return (f.ErrorMessage, tmpScript.Substring(pos, Math.Min(30, tmpScript.Length - pos)));
                    }
                    pos = f.Position;
                }
            } while (true);
        }

        private static string ReduceText(string txt) {
            System.Text.StringBuilder s = new();
            var gänsef = false;
            var comment = false;
            txt = txt.Replace("\\\"", Constants.GänsefüßchenReplace); // muss am Anfang gemacht werden, weil sonst die Zählweise nicht mehr stimmt
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

        #endregion

        //public static void AddScriptComands() {
        //    Comands.Add(new Method_Add());
        //    Comands.Add(new Method_BerechneVariable());
        //    Comands.Add(new Method_Break());
        //    Comands.Add(new Method_ChangeType());
        //    Comands.Add(new Method_Contains());
        //    Comands.Add(new Method_EndsWith());
        //    Comands.Add(new Method_Exception());
        //    Comands.Add(new Method_Exists());
        //    Comands.Add(new Method_if());
        //    Comands.Add(new Method_Int());
        //    Comands.Add(new Method_IsNullOrEmpty());
        //    Comands.Add(new Method_IsType());
        //    Comands.Add(new Method_Join());
        //    Comands.Add(new Method_Max());
        //    Comands.Add(new Method_Min());
        //    Comands.Add(new Method_Number());
        //    Comands.Add(new Method_Remove());
        //    Comands.Add(new Method_Round());
        //    Comands.Add(new Method_Sort());
        //    Comands.Add(new Method_Split());
        //    Comands.Add(new Method_StartsWith());
        //    Comands.Add(new Method_String());
        //       Comands.Add(new Method_Substring());
        //    Comands.Add(new Method_Var());
        //}
    }
}