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

using BlueScript.Methods;
using BlueScript.Enums;
using System;
using System.Drawing;

namespace BlueScript.Structures {

    public struct DoItFeedback {

        #region Fields

        public string ErrorMessage;
        public Variable? Variable;

        #endregion

        #region Constructors

        public DoItFeedback(string errormessage) {
            ErrorMessage = errormessage;
            Variable = null;
        }

        public DoItFeedback(Variable variable) {
            ErrorMessage = string.Empty;
            Variable = variable;
        }

        public DoItFeedback(string valueString, string errormessage) {
            ErrorMessage = errormessage;
            Variable = new VariableString(valueString);
        }

        public DoItFeedback(bool value) {
            ErrorMessage = string.Empty;
            Variable = new VariableBool(value);
        }

        public DoItFeedback(double value) {
            ErrorMessage = string.Empty;
            Variable = new VariableFloat((float)value);
        }

        public DoItFeedback(float value) {
            ErrorMessage = string.Empty;
            Variable = new VariableFloat(value);
        }

        #endregion

        #region Properties

        [Obsolete]
        public string Value {
            get {
                if (Variable == null) { return string.Empty; }
                return Variable.ValueForReplace;
            }
        }

        #endregion

        #region Methods

        public static DoItFeedback AttributFehler(Method method, SplittedAttributesFeedback f) =>
            new(f.ErrorMessage + " > " + method.Syntax);

        public static DoItFeedback Falsch() => new(false);

        public static DoItFeedback FalscherDatentyp() => new("Falscher Datentyp.");

        public static DoItFeedback Klammerfehler() => new("Fehler bei der Klammersetzung.");

        public static DoItFeedback Null() => new();

        public static DoItFeedback Schreibgschützt() => new("Variable ist schreibgeschützt.");

        public static DoItFeedback Wahr() => new(true);

        #endregion
    }
}