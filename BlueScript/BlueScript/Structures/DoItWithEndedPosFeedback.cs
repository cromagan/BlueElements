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

#nullable enable

using System;
using BlueScript.Variables;

namespace BlueScript.Structures {

    public struct DoItWithEndedPosFeedback {

        #region Fields

        public string ErrorMessage;

        public int Position;

        public Variable? Variable;

        #endregion

        #region Constructors

        public DoItWithEndedPosFeedback(string errormessage, Variable variable, int endpos) {
            ErrorMessage = errormessage;
            Variable = variable;
            Position = endpos;
        }

        public DoItWithEndedPosFeedback(string errormessage) {
            Position = -1;
            ErrorMessage = errormessage;
            Variable = null;
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
    }
}