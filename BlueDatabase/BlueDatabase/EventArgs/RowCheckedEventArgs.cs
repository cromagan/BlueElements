﻿// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using System.Collections.Generic;

namespace BlueDatabase.EventArgs;

public class RowCheckedEventArgs : RowEventArgs {

    #region Fields

    public readonly string Message;

    #endregion

    #region Constructors

    public RowCheckedEventArgs(RowItem row, string message) : base(row) => Message = "<b><u>" + row.CellFirstString() + "</b></u><br><br>" + message;

    public RowCheckedEventArgs(RowItem row, List<string>? columnsWithErrors, VariableCollection? variables, string message) : base(row) {
        ColumnsWithErrors = columnsWithErrors;
        Variables = variables;
        Message = string.Empty;
        Message = "<b><u>" + row.CellFirstString() + "</b></u><br><br>" + message;
    }

    #endregion

    #region Properties

    public List<string>? ColumnsWithErrors { get; }
    public VariableCollection? Variables { get; }

    #endregion
}