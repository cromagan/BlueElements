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
using System.Drawing;
using BlueScript.Methods;
using BlueScript.Variables;

namespace BlueScript.Structures;

public struct DoItFeedback {

    #region Fields

    public readonly string ErrorMessage;

    public readonly int Line;
    public readonly Variable? Variable;

    #endregion

    #region Constructors

    public DoItFeedback(Script? s, CanDoFeedback? infos) {
        ErrorMessage = string.Empty;
        Variable = null;
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(Script? s, CanDoFeedback? infos, string errormessage) {
        ErrorMessage = errormessage;
        Variable = null;
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(Script? s, CanDoFeedback? infos, Variable variable) {
        ErrorMessage = string.Empty;
        Variable = variable;
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(string errormessage, int line) {
        ErrorMessage = errormessage;
        Variable = null;
        Line = line;
    }

    public DoItFeedback(Script? s, CanDoFeedback? infos, string valueString, string errormessage) {
        ErrorMessage = errormessage;
        Variable = new VariableString(Variable.DummyName(), valueString);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(Script? s, CanDoFeedback? infos, List<string>? list) {
        ErrorMessage = string.Empty;
        Variable = new VariableListString(list);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(Script? s, CanDoFeedback? infos, Bitmap bmp) {
        ErrorMessage = string.Empty;
        Variable = new VariableBitmap(bmp);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(Script? s, CanDoFeedback? infos, bool value) {
        ErrorMessage = string.Empty;
        Variable = new VariableBool(value);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(Script? s, CanDoFeedback? infos, double value) {
        ErrorMessage = string.Empty;
        Variable = new VariableFloat((float)value);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(Script? s, CanDoFeedback? infos, float value) {
        ErrorMessage = string.Empty;
        Variable = new VariableFloat(value);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(Script? s, CanDoFeedback? infos, DateTime value) {
        ErrorMessage = string.Empty;
        Variable = new VariableDateTime(value);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(Script? s, CanDoFeedback? infos, string[] list) {
        ErrorMessage = string.Empty;
        Variable = new VariableListString(list);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    #endregion

    #region Methods

    public static DoItFeedback AttributFehler(Script? s, CanDoFeedback? infos, Method method, SplittedAttributesFeedback f) =>
        new(s, infos, f.ErrorMessage + " > " + method.Syntax);

    public static DoItFeedback Falsch(Script s, CanDoFeedback? infos) => new(s, infos, false);

    public static DoItFeedback FalscherDatentyp(Script s, CanDoFeedback? infos) => new(s, infos, "Falscher Datentyp.");

    public static DoItFeedback Klammerfehler(Script s, CanDoFeedback? infos) => new(s, infos, "Fehler bei der Klammersetzung.");

    public static DoItFeedback Null(Script s, CanDoFeedback? infos) => new(s, infos);

    public static DoItFeedback Schreibgschützt(Script s, CanDoFeedback? infos) => new(s, infos, "Variable ist schreibgeschützt.");

    public static DoItFeedback VerschiedeneTypen(CanDoFeedback? infos, Script? s, Variable var1, Variable var2) =>
        new(s, infos, "Variable '" + var1.KeyName + "' ist nicht der erwartete Typ {" + var2.MyClassId +
                      "}, sondern {" + var1.MyClassId + "}");

    public static DoItFeedback Wahr(Script s, CanDoFeedback? infos) => new(s, infos, true);

    #endregion
}