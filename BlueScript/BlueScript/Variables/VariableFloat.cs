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
using BlueScript.Structures;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;

namespace BlueScript.Variables;

public class VariableFloat : Variable {

    #region Fields

    private double _double;

    #endregion

    #region Constructors

    public VariableFloat(string name, double value, bool ronly, bool system, string coment) : base(name, ronly, system, coment) => _double = value;

    public VariableFloat(double value) : this(DummyName(), value, true, false, string.Empty) { }

    /// <summary>
    /// Wichtig für: GetEnumerableOfType<Variable>("NAME");
    /// </summary>
    /// <param name="name"></param>
    public VariableFloat(string name) : this(name, 0f, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ShortName_Plain => "num";
    public static string ShortName_Variable => "*num";
    public override int CheckOrder => 1;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => false;
    public override string ReadableText => _double.ToString(Constants.Format_Float1);
    public override string ShortName => "num";
    public override bool ToStringPossible => true;

    public override string ValueForReplace => ReadableText;

    public int ValueInt => (int)_double;

    public double ValueNum {
        get => _double;
        set {
            if (Readonly) { return; }
            _double = value;
        }
    }

    #endregion

    #region Methods

    public override DoItFeedback GetValueFrom(Variable variable) {
        if (variable is not VariableFloat v) { return DoItFeedback.VerschiedeneTypen(this, variable); }
        if (Readonly) { return DoItFeedback.Schreibgschützt(); }
        ValueNum = v.ValueNum;
        return DoItFeedback.Null();
    }

    protected override bool TryParse(string txt, out Variable? succesVar, Script s) {
        succesVar = null;

        var (pos2, _) = NextText(txt, 0, Berechnung.RechenOperatoren, false, false, KlammernStd);
        if (pos2 >= 0) {
            var erg = Berechnung.Ergebnis(txt);
            if (erg == null) { return false; }
            txt = erg.ToString();
        }

        if (DoubleTryParse(txt, out var zahl)) {
            succesVar = new VariableFloat(zahl);
            return true;
        }

        return false;
    }

    #endregion
}