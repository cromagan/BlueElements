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

using BlueScript;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueDatabase.DatabaseAbstract;

namespace BlueDatabase.AdditionalScriptComands;

public abstract class Method_Database : Method {

    #region Fields

    public static readonly List<string> FilterVar = new() { VariableFilterItem.ShortName_Variable };

    public static readonly List<string> RowVar = new() { VariableRowItem.ShortName_Variable };

    #endregion

    #region Methods

    protected ColumnItem? Column(VariableCollection variables, SplittedAttributesFeedback attvar, int no) {
        var c = attvar.Attributes[no];
        if (c == null) { return null; }

        return MyDatabase(variables)?.Column.Exists(c.KeyName);
    }

    protected DatabaseAbstract? DatabaseOf(VariableCollection variables, string tableName) {
        if (!IsValidTableName(tableName, false)) { return null; }

        var db = MyDatabase(variables)?.ConnectionDataOfOtherTable(tableName, false);
        return GetById(db, false, string.Empty, null); // Freezed unnötog, da eh keine Scripte ausgeführt werden.
    }

    protected DatabaseAbstract? MyDatabase(VariableCollection variables) {
        var f = variables.GetSystem("Database");
        if (f is VariableDatabase db) { return db.Database; }
        return null;
    }

    protected RowItem? MyRow(VariableCollection variables) {
        var f = variables.GetSystem("RowKey");
        if (f is VariableRowItem db) { return db.RowItem; }
        return null;
    }

    #endregion
}