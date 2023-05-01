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

using BlueScript.Enums;
using BlueScript.Variables;

namespace BlueScript.Structures;

public readonly struct SplittedAttributesFeedback {

    #region Constructors

    public SplittedAttributesFeedback(VariableCollection atts) {
        Attributes = atts;
        ErrorMessage = string.Empty;
        FehlerTyp = ScriptIssueType.ohne;
    }

    public SplittedAttributesFeedback(ScriptIssueType type, string error) {
        Attributes = new VariableCollection();
        ErrorMessage = error;
        FehlerTyp = type;
    }

    #endregion

    #region Properties

    public VariableCollection Attributes { get; }

    public string ErrorMessage { get; }

    public ScriptIssueType FehlerTyp { get; }

    #endregion
}