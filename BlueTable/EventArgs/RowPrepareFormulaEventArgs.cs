// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueScript.Structures;
using System.Collections.Generic;
using System.Drawing;

namespace BlueTable.EventArgs;

public class RowPrepareFormulaEventArgs : RowEventArgs {

    #region Fields

    public readonly string Message;

    #endregion

    #region Constructors

    public RowPrepareFormulaEventArgs(RowItem row, List<string>? columnsWithErrors, ScriptEndedFeedback prepareFormulaFeedback, string message, Brush? rowcolor) : base(row) {
        ColumnsWithErrors = columnsWithErrors;
        PrepareFormulaFeedback = prepareFormulaFeedback;
        RowColor = rowcolor;
        Message = string.Empty;
        Message = "<b><u>" + row.CellFirstString() + "</b></u><br><br>" + message;
    }

    #endregion

    #region Properties

    public List<string>? ColumnsWithErrors { get; }
    public ScriptEndedFeedback PrepareFormulaFeedback { get; }
    public Brush? RowColor { get; }

    #endregion
}