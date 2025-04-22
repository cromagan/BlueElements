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
using BlueScript.Structures;
using System;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;
using static BlueBasics.Interfaces.ParseableExtension;

namespace BlueScript.Variables;

public class VariableFloat : Variable {

    #region Fields

    private double _double;

    #endregion

    #region Constructors

    public VariableFloat(string name, double value, bool ronly, string comment) : base(name, ronly, comment) => _double = value;

    public VariableFloat(double value) : this(DummyName(), value, true, string.Empty) { }

    public VariableFloat() : this(string.Empty, 0f, true, string.Empty) { }

    public VariableFloat(string name) : this(name, 0f, true, string.Empty) { }

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

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableFloat v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        ValueNum = v.ValueNum;
        return DoItFeedback.Null();
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

    protected override (bool cando, object? result) TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) {
        var (pos2, _) = NextText(txt, 0, MathFormulaParser.RechenOperatoren, false, false, KlammernAlle);
        if (pos2 >= 0) {
            var erg = MathFormulaParser.Ergebnis(txt);
            if (erg == null) { return (false, null); }
            txt = erg.ToString();
        }

        if (DoubleTryParse(txt, out var zahl)) {
            return (true, zahl);
        }

        return (false, null);
    }

    #endregion
}