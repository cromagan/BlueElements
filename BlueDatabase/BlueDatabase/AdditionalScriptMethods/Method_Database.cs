// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueScript;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueDatabase.Database;

namespace BlueDatabase.AdditionalScriptMethods;

public abstract class Method_Database : Method {

    #region Fields

    public static readonly List<string> FilterVar = [VariableFilterItem.ShortName_Variable];

    public static readonly List<string> RowVar = [VariableRowItem.ShortName_Variable];

    #endregion

    #region Methods

    protected static Database? MyDatabase(ScriptProperties scp) {
        if (scp.AdditionalInfo is Database db && !db.IsDisposed) { return db; }
        if (scp.AdditionalInfo is RowItem r) { return r.Database; }
        return null;
    }

    protected static RowItem? MyRow(ScriptProperties scp) {
        if (scp.AdditionalInfo is RowItem r) { return r; }
        return null;
    }

    protected static bool SetErrorAllowed(VariableCollection varCol) {
        var see = varCol.Get("SetErrorEnabled");
        if (see is VariableBool seet) { return seet.ValueBool; }
        return false;
    }

    protected ColumnItem? Column(ScriptProperties scp, SplittedAttributesFeedback attvar, int no) {
        var c = attvar.Attributes[no];
        if (c == null) { return null; }

        return MyDatabase(scp)?.Column[c.KeyName];
    }

    protected Database? DatabaseOf(ScriptProperties scp, string tableName) {
        if (!IsValidTableName(tableName, false)) { return null; }

        var db = MyDatabase(scp)?.ConnectionDataOfOtherTable(tableName, false);
        return GetById(db, false, null, true); // Freezed unnötiog, da eh keine Scripte ausgeführt werden.
    }

    #endregion
}