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

using System;
using BlueBasics;
using BlueScript.Structures;

namespace BlueScript.Variables;

public class VariableDateTime : Variable {

    #region Fields

    private DateTime _valueDateTime;

    #endregion

    #region Constructors

    public VariableDateTime(string name, DateTime value, bool ronly, bool system, string comment) : base(name, ronly, system, comment) => _valueDateTime = value;

    /// <summary>
    /// Wichtig für: GetEnumerableOfType<Variable>("NAME");
    /// </summary>
    /// <param name="name"></param>
    public VariableDateTime(string name) : this(name, DateTime.Now, true, false, string.Empty) { }

    public VariableDateTime(DateTime value) : this(DummyName(), value, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "dat";
    public static string ShortName_Variable => "*dat";
    public override int CheckOrder => 5;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => false;
    public override string MyClassId => ClassId;
    public override string ReadableText => _valueDateTime.ToString(Constants.Format_Date4);
    public override bool ToStringPossible => false;

    /// <summary>
    /// Der Wert ohne " am Anfang/Ende. Gleichgesetzt mit ReadableText
    /// </summary>
    public DateTime ValueDate {
        get => _valueDateTime;
        set {
            if (ReadOnly) { return; }
            _valueDateTime = value;
        }
    }

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableDateTime(Name);
        v.Parse(ToString());
        return v;
    }

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableDateTime v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        ValueDate = v.ValueDate;
        return DoItFeedback.Null();
    }

    protected override Variable? NewWithThisValue(object x, Script s) => null;

    protected override void SetValue(object? x) { }

    protected override object? TryParse(string txt, Script? s) => null;

    #endregion
}