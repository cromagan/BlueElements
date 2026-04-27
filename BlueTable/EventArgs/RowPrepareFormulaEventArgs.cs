// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueTable.Classes;
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