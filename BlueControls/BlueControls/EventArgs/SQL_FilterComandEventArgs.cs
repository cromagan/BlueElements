﻿// Authors:
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

using BlueDatabase;
using BlueDatabase.EventArgs;

#nullable enable

namespace BlueControls.EventArgs;

public class SQL_FilterComandEventArgs : SQL_FilterEventArgs {

    #region Constructors

    // string Comand, ColumnItem ThisColumn, FilterItem NewFilter
    public SQL_FilterComandEventArgs(string comand, SQL_ColumnItem? column, SQL_FilterItem newFilter) : base(newFilter) {
        Comand = comand;
        Column = column;
    }

    #endregion

    #region Properties

    public SQL_ColumnItem? Column { get; }
    public string Comand { get; }

    #endregion
}