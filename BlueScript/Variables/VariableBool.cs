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
using static BlueBasics.Extensions;

namespace BlueScript.Variables;

public class VariableBool : Variable {

    #region Fields

    private bool _valuebool;

    #endregion

    #region Constructors

    public VariableBool(string name, bool value, bool ronly, string comment) : base(name, ronly, comment) => _valuebool = value;

    public VariableBool(string name) : this(name, false, true, string.Empty) { }

    public VariableBool() : this(string.Empty, false, true, string.Empty) { }

    public VariableBool(bool value) : this(DummyName(), value, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "bol";
    public static string ShortName_Plain => "bol";
    public static string ShortName_Variable => "*bol";
    public override int CheckOrder => 0;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => false;
    public override string ReadableText => _valuebool.ToString();

    /// <summary>
    /// Der Wert + oder -
    /// </summary>
    public override string SearchValue => _valuebool.ToPlusMinus();

    public override bool ToStringPossible => true;

    public bool ValueBool {
        get => _valuebool;
        set {
            if (ReadOnly) { return; }
            _valuebool = value; // Variablen enthalten immer den richtigen Wert und es werden nur beim Ersetzen im Script die kritischen Zeichen entfernt
        }
    }

    public override string ValueForCell => _valuebool.ToPlusMinus();
    public override string ValueForReplace => _valuebool ? "true" : "false";

    #endregion

    #region Methods

    public override void DisposeContent() { }

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariableBool v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        ValueBool = v.ValueBool;
        return string.Empty;
    }

    protected override void SetValue(object? x) {
        if (x is bool val) {
            _valuebool = val;
        } else {
            Develop.DebugPrint(ErrorType.Error, "Variablenfehler!");
        }
    }

    protected override bool TryParseValue(string txt, out object? result) {
        result = null;

        switch (txt.ToLowerInvariant()) {
            case "true":
            case "+":
            case "wahr":
                result = true;
                return true;

            case "false":
            case "-":
            case "falsch":
                result = false;
                return true;
        }

        return false;
    }

    #endregion
}