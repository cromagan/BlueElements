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

using System.Collections.Generic;
using BlueScript.Enums;
using BlueScript.Variables;

namespace BlueScript.Structures {

    public struct SplittedAttributesFeedback {

        #region Fields

        public readonly List<Variable> Attributes;

        public readonly string ErrorMessage;

        public readonly ScriptIssueType FehlerTyp;

        #endregion

        #region Constructors

        public SplittedAttributesFeedback(List<Variable> atts) {
            Attributes = atts;
            ErrorMessage = string.Empty;
            FehlerTyp = ScriptIssueType.ohne;
        }

        public SplittedAttributesFeedback(ScriptIssueType type, string error) {
            Attributes = new List<Variable>();
            ErrorMessage = error;
            FehlerTyp = type;
        }

        #endregion
    }
}