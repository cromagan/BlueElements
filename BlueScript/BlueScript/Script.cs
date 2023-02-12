// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using BlueBasics;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Generic;

namespace BlueScript;

public class Script {

    #region Fields

    public static List<Method>? Comands;
    public static List<Variable>? VarTypes;

    /// <summary>
    /// Bei diesem Pfad können zusätzliche Dateien - wie z.B. Skript-Subroutinen enthalten sein.
    /// Der Pfad mit abschließenden \
    /// </summary>
    public readonly string AdditionalFilesPath;

    public readonly bool ChangeValues;
    public readonly List<Variable> Variables;
    public bool EndScript;

    #endregion

    ///// <summary>
    ///// Dieses Feld enthält informationen, die nach dem Skript-Lauf abgegriffen werden können.
    ///// </summary>
    //public string Feedback = string.Empty;

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
    //            Develop.DebugPrint(typeLoadException.LoaderExceptions.ToString(false));
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
    public Script(List<Variable>? variablen, string additionalFilesPath, bool changevalues) {
        Comands ??= GetInstaceOfType<Method>();
        if (VarTypes == null) {
            VarTypes = GetInstaceOfType<Variable>("NAME");
            VarTypes.Sort();
        }

        ReducedScriptText = string.Empty;
        ChangeValues = changevalues;
        Variables = variablen ?? new();

        AdditionalFilesPath = (additionalFilesPath.Trim("\\") + "\\").CheckPath();

        //BitmapCache = new List<Bitmap>();
    }

    #endregion

    #region Properties

    public bool BreakFired { get; set; }

    public string ReducedScriptText { get; private set; }
    public int Schleife { get; internal set; }

    public string ScriptText { get; set; } = string.Empty;

    public int Sub { get; set; }

    #endregion

    #region Methods

    public static DoItWithEndedPosFeedback ComandOnPosition(string txt, int pos, Script s, bool expectedvariablefeedback) {
        if (Comands == null) { return new DoItWithEndedPosFeedback("Befehle nicht initialisiert", -1); }

        foreach (var thisC in Comands) {
            var f = thisC.CanDo(txt, pos, expectedvariablefeedback, s, Line(txt, pos));
            if (f.MustAbort) { return new DoItWithEndedPosFeedback(f.ErrorMessage, Line(txt, pos)); }

            if (string.IsNullOrEmpty(f.ErrorMessage)) {
                var fn = thisC.DoIt(f, s);
                return new DoItWithEndedPosFeedback(fn.ErrorMessage, fn.Variable, f.ContinueOrErrorPosition, fn.Line);
            }
        }

        #region Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        foreach (var f in Comands.Select(thisC => thisC.CanDo(txt, pos, !expectedvariablefeedback, s, Line(txt, pos)))) {
            if (f.MustAbort) {
                return new DoItWithEndedPosFeedback(f.ErrorMessage, Line(txt, pos));
            }

            if (string.IsNullOrEmpty(f.ErrorMessage)) {
                return expectedvariablefeedback
                    ? new DoItWithEndedPosFeedback("Dieser Befehl hat keinen Rückgabewert: " + txt.Substring(pos), Line(txt, pos))
                    : new DoItWithEndedPosFeedback("Dieser Befehl hat einen Rückgabewert, der nicht verwendet wird: " + txt.Substring(pos), Line(txt, pos));
            }
        }

        #endregion Prüfen für bessere Fehlermeldung, ob der Rückgabetyp falsch gesetzt wurde

        return new DoItWithEndedPosFeedback("Kann nicht geparsed werden: " + txt.Substring(pos).TrimEnd("¶"), Line(txt, pos));
    }

    public static int Line(string? txt, int? pos) {
        if (pos == null || txt == null) { return 0; }
        return txt.Substring(0, Math.Min((int)pos, txt.Length)).Count(c => c == '¶') + 1;
    }

    public static string ReduceText(string txt) {
        StringBuilder s = new();
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
                    if (gänsef) { _ = s.Append("\";Exception(\"Fehler mit Anführungsstrichen\");"); }
                    _ = s.Append("¶");
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
                _ = s.Append(c);
            }
        }
        return s.ToString();
    }

    public ScriptEndedFeedback Parse(int lineadd) {
        ReducedScriptText = ReduceText(ScriptText);
        BreakFired = false;
        Schleife = 0;
        Sub = 0;

        return Parse(ReducedScriptText, lineadd);
    }

    //internal int AddBitmapToCache(Bitmap? bmp) {
    //    BitmapCache.Add(bmp);
    //    return BitmapCache.IndexOf(bmp);
    //}

    public ScriptEndedFeedback Parse(string redScriptText, int lineadd) {
        var pos = 0;
        EndScript = false;

        do {
            if (pos >= redScriptText.Length || EndScript) {
                return new ScriptEndedFeedback(Variables, Line(redScriptText, pos) + lineadd);
            }

            if (BreakFired) { return new ScriptEndedFeedback(Variables, Line(redScriptText, pos) + lineadd); }

            if (redScriptText.Substring(pos, 1) == "¶") {
                pos++;
            } else {
                var f = ComandOnPosition(redScriptText, pos, this, false);
                if (!string.IsNullOrEmpty(f.ErrorMessage)) {
                    return new ScriptEndedFeedback(f.Line + lineadd, f.ErrorMessage, redScriptText.Substring(pos, Math.Min(30, redScriptText.Length - pos)), Variables);
                }
                pos = f.Position;
            }
        } while (true);
    }

    #endregion
}