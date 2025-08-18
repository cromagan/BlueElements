// Authors:
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

using BlueBasics;
using BlueBasics.Enums;
using System;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;

namespace BlueScript.Variables;

public class VariableDouble : Variable {

    #region Fields

    private double _double;

    #endregion

    #region Constructors

    public VariableDouble(string name, double value, bool ronly, string comment) : base(name, ronly, comment) => _double = value;

    public VariableDouble(double value) : this(DummyName(), value, true, string.Empty) { }

    public VariableDouble() : this(string.Empty, 0f, true, string.Empty) { }

    public VariableDouble(string name) : this(name, 0f, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "num";
    public static string ShortName_Plain => "num";
    public static string ShortName_Variable => "*num";
    public override int CheckOrder => 1;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => false;

    /// <summary>
    /// Der Zahlenwert mit maximal 5 Kommastellen (0.#####)
    /// </summary>
    public override string ReadableText => _double.ToStringFloat5();

    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => true;

    /// <summary>
    /// Der Zahlenwert mit maximal 5 Kommastellen (0.#####)
    /// </summary>
    public override string ValueForReplace => ReadableText;

    public int ValueInt => (int)_double;

    public double ValueNum {
        get => _double;
        set {
            if (ReadOnly) { return; }
            _double = Math.Round(value, 5, MidpointRounding.AwayFromZero);
        }
    }

    #endregion

    #region Methods

    public override void DisposeContent() { }

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariableDouble v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        ValueNum = v.ValueNum;
        return string.Empty;
    }

    protected override void SetValue(object? x) {
        if (x is float val) {
            _double = val;
        } else if (x is double vald) {
            _double = vald;
        } else {
            Develop.DebugPrint(ErrorType.Error, "Variablenfehler!");
        }
    }

    protected override bool TryParseValue(string txt, out object? result) {
        var (pos2, _) = NextText(txt, 0, MathFormulaParser.RechenOperatoren, false, false, KlammernAlle);
        if (pos2 >= 0) {
            var erg = MathFormulaParser.Ergebnis(txt);
            if (erg == null) { result = null; return false; }
            txt = erg.ToString();
        }

        if (DoubleTryParse(txt, out var zahl)) {
            result = zahl;
            return true;
        }

        result = null;
        return false;
    }

    #endregion
}