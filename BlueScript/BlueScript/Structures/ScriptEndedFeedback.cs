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

using System.Collections.Generic;
using BlueScript.Variables;

namespace BlueScript.Structures;

public class ScriptEndedFeedback {

    #region Fields

    public readonly string ErrorCode;
    public readonly string ErrorMessage;
    public readonly bool GiveItAnotherTry;
    public readonly int LastlineNo;
    public readonly List<Variable>? Variables;

    #endregion

    #region Constructors

    public ScriptEndedFeedback(int lastlineNo, string errormessage, string errorcode, List<Variable> variables) {
        ErrorMessage = errormessage;
        ErrorCode = errorcode;
        LastlineNo = lastlineNo;
        Variables = variables;
        GiveItAnotherTry = false;
    }

    public ScriptEndedFeedback(string errormessage, bool giveitanothertry) {
        ErrorMessage = errormessage;
        ErrorCode = string.Empty;
        LastlineNo = -1;
        Variables = null;
        GiveItAnotherTry = giveitanothertry;
    }

    public ScriptEndedFeedback(List<Variable> variables, int lastlineNo) {
        ErrorMessage = string.Empty;
        ErrorCode = string.Empty;
        LastlineNo = lastlineNo;
        Variables = variables;
    }

    #endregion
}