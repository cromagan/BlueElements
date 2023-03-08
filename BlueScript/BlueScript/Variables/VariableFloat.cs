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

using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Structures;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;

namespace BlueScript.Variables;

public class VariableFloat : Variable {

    #region Fields

    private double _double;

    #endregion

    #region Constructors

    public VariableFloat(string name, double value, bool ronly, bool system, string comment) : base(name, ronly, system, comment) => _double = value;

    public VariableFloat(double value) : this(DummyName(), value, true, false, string.Empty) { }

    /// <summary>
    /// Wichtig für: GetEnumerableOfType<Variable>("NAME");
    /// </summary>
    /// <param name="name"></param>
    public VariableFloat(string name) : this(name, 0f, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "num";
    public static string ShortName_Plain => "num";
    public static string ShortName_Variable => "*num";
    public override int CheckOrder => 1;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => false;
    public override string MyClassId => ClassId;
    public override string ReadableText => _double.ToString(Constants.Format_Float5);
    public override bool ToStringPossible => true;

    public override string ValueForReplace => ReadableText;

    public int ValueInt => (int)_double;

    public double ValueNum {
        get => _double;
        set {
            if (ReadOnly) { return; }
            _double = value;
        }
    }

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableFloat(Name);
        v.Parse(ToString());
        return v;
    }

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableFloat v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        ValueNum = v.ValueNum;
        return DoItFeedback.Null();
    }

    protected override Variable NewWithThisValue(object x, Script s) {
        var v = new VariableFloat(string.Empty);
        v.SetValue(x);
        return v;
    }

    protected override void SetValue(object? x) {
        if (x is float val) {
            _double = val;
        } else if (x is double vald) {
            _double = vald;
        } else {
            Develop.DebugPrint(FehlerArt.Fehler, "Variablenfehler!");
        }
    }

    protected override object? TryParse(string txt, Script? s) {
        var (pos2, _) = NextText(txt, 0, MathFormulaParser.RechenOperatoren, false, false, KlammernStd);
        if (pos2 >= 0) {
            var erg = MathFormulaParser.Ergebnis(txt);
            if (erg == null) { return null; }
            txt = erg.ToString();
        }

        if (DoubleTryParse(txt, out var zahl)) {
            return zahl;
        }

        return null;
    }

    #endregion
}