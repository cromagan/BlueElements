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

using BlueScript.Classes;
using BlueScript.Methods;
using BlueTable.AdditionalScriptVariables;
using BlueTable.Classes;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

public abstract class Method_TableGeneric : Method {

    #region Fields

    public static readonly List<string> FilterVar = [VariableFilterItem.ShortName_Variable];

    public static readonly List<string> RowVar = [VariableRowItem.ShortName_Variable];

    public static readonly List<string> TableVar = [VariableTable.ShortName_Variable];

    #endregion

    #region Methods

    protected static ColumnItem? Column(ScriptProperties scp, SplittedAttributesFeedback attvar, int no) {
        var c = attvar.Attributes[no];
        if (c == null) { return null; }

        if (c.KeyName.StartsWith("ID_", System.StringComparison.OrdinalIgnoreCase)) {
            return MyTable(scp)?.Column[c.SearchValue];
        }

        return MyTable(scp)?.Column[c.KeyName];
    }

    protected static RowItem? MyRow(ScriptProperties scp) {
        if (scp.AdditionalInfo is RowItem r) { return r; }
        return null;
    }

    protected static Table? MyTable(ScriptProperties scp) {
        if (scp.AdditionalInfo is Table { IsDisposed: false } tb) { return tb; }
        if (scp.AdditionalInfo is RowItem r) { return r.Table; }
        return null;
    }

    #endregion
}