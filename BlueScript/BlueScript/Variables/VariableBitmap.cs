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

using System.Collections.Generic;
using System.Drawing;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;

namespace BlueScript.Variables;

public class VariableBitmap : Variable {

    #region Fields

    private Bitmap? _bmp;

    #endregion

    #region Constructors

    public VariableBitmap(string name, Bitmap? value, bool ronly, bool system, string comment) : base(name, ronly, system, comment) => _bmp = value;

    /// <summary>
    /// Wichtig für: GetEnumerableOfType<Variable>("NAME");
    /// </summary>
    /// <param name="name"></param>
    public VariableBitmap(string name) : this(name, null, true, false, string.Empty) { }

    public VariableBitmap(Bitmap? value) : this(DummyName(), value, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "bmp";
    public static string ShortName_Variable => "*bmp";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _bmp == null;
    public override string MyClassId => ClassId;
    public override bool ToStringPossible => false;

    public Bitmap? ValueBitmap {
        get => _bmp;
        set {
            if (ReadOnly) { return; }
            _bmp = value;
        }
    }

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableBitmap(Name);
        v.Parse(ToString());
        return v;
    }

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableBitmap v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        ValueBitmap = v.ValueBitmap;
        return DoItFeedback.Null();
    }

    protected override Variable? NewWithThisValue(object x, VariableCollection vs) => null;

    protected override void SetValue(object? x) { }

    protected override object? TryParse(string txt, VariableCollection? vs, MethodType allowedMethods, List<Method> lm, bool changeValues, string scriptAttributes) => null;

    #endregion
}