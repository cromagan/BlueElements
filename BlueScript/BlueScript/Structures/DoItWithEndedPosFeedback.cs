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

using BlueScript.Variables;

namespace BlueScript.Structures;

public struct DoItWithEndedPosFeedback {

    #region Fields

    internal readonly string ErrorMessage;

    internal readonly int Position;

    internal readonly Variable? Variable;

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
}