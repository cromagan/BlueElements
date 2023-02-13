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

    public DoItFeedback(CanDoFeedback? infos) {
        ErrorMessage = string.Empty;
        Variable = null;
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, string errormessage) {
        ErrorMessage = errormessage;
        Variable = null;
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, Variable variable) {
        ErrorMessage = string.Empty;
        Variable = variable;
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(string errormessage, int line) {
        ErrorMessage = errormessage;
        Variable = null;
        Line = line;
    }

    public DoItFeedback(CanDoFeedback? infos, string valueString, string errormessage) {
        ErrorMessage = errormessage;
        Variable = new VariableString(Variable.DummyName(), valueString);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, List<string>? list) {
        ErrorMessage = string.Empty;
        Variable = new VariableListString(list);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, Bitmap bmp) {
        ErrorMessage = string.Empty;
        Variable = new VariableBitmap(bmp);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, bool value) {
        ErrorMessage = string.Empty;
        Variable = new VariableBool(value);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, double value) {
        ErrorMessage = string.Empty;
        Variable = new VariableFloat((float)value);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, float value) {
        ErrorMessage = string.Empty;
        Variable = new VariableFloat(value);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, DateTime value) {
        ErrorMessage = string.Empty;
        Variable = new VariableDateTime(value);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    public DoItFeedback(CanDoFeedback? infos, string[] list) {
        ErrorMessage = string.Empty;
        Variable = new VariableListString(list);
        Line = Script.Line(infos?.CurrentReducedScriptText, infos?.ContinueOrErrorPosition);
    }

    #endregion

    #region Methods

    public static DoItFeedback AttributFehler(CanDoFeedback? infos, Method method, SplittedAttributesFeedback f) =>
        new(infos, f.ErrorMessage + " > " + method.Syntax);

    public static DoItFeedback Falsch(CanDoFeedback? infos) => new(infos, false);

    public static DoItFeedback FalscherDatentyp(CanDoFeedback? infos) => new(infos, "Falscher Datentyp.");

    public static DoItFeedback Klammerfehler(CanDoFeedback? infos) => new(infos, "Fehler bei der Klammersetzung.");

    public static DoItFeedback Null(CanDoFeedback? infos) => new(infos);

    public static DoItFeedback Schreibgschützt(CanDoFeedback? infos) => new(infos, "Variable ist schreibgeschützt.");

    public static DoItFeedback VerschiedeneTypen(CanDoFeedback? infos, Variable var1, Variable var2) =>
        new(infos, "Variable '" + var1.KeyName + "' ist nicht der erwartete Typ {" + var2.MyClassId +
                      "}, sondern {" + var1.MyClassId + "}");

    public static DoItFeedback Wahr(CanDoFeedback? infos) => new(infos, true);

    #endregion
}