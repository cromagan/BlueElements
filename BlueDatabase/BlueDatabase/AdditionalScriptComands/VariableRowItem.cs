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

using BlueDatabase;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript;

public class VariableRowItem : Variable {

    #region Fields

    private RowItem? _row;

    #endregion

    #region Constructors

    public VariableRowItem(string name, RowItem? value, bool ronly, bool system, string Comment) : base(name, ronly, system, Comment) => _row = value;

    public VariableRowItem(string name) : this(name, null, true, false, string.Empty) { }

    public VariableRowItem(RowItem? value) : this(DummyName(), value, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "row";
    public static string ShortName_Variable => "*row";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _row == null;
    public override string MyClassId => ClassId;

    public RowItem? RowItem {
        get => _row;
        set {
            if (ReadOnly) { return; }
            _row = value;
        }
    }

    public override bool ToStringPossible => false;

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableRowItem(Name);
        v.Parse(ToString());
        return v;
    }

    public override DoItFeedback GetValueFrom(Variable variable) {
        if (variable is not VariableRowItem v) { return DoItFeedback.VerschiedeneTypen(this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(); }
        RowItem = v.RowItem;
        return DoItFeedback.Null();
    }

    protected override Variable? NewWithThisValue(object x, Script s) => null;

    protected override void SetValue(object? x) { }

    protected override object? TryParse(string txt, Script? s) => null;

    #endregion
}