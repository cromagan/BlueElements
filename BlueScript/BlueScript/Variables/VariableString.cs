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

namespace BlueScript.Variables;

public class VariableString : Variable {

    #region Fields

    private string _valueString;

    #endregion

    #region Constructors

    public VariableString(string name, string value, bool ronly, string comment) : base(name, ronly, comment) => _valueString = value.RestoreCriticalVariableChars();

    public VariableString(string name) : this(name, string.Empty, true, string.Empty) { }

    public VariableString() : this(string.Empty, string.Empty, true, string.Empty) { }

    public VariableString(string name, string value) : this(name, value, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "str";
    public static string ShortName_Plain => "str";
    public static string ShortName_Variable => "*str";
    public override int CheckOrder => 2;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => string.IsNullOrEmpty(_valueString);

    /// <summary>
    /// Der Wert ohne " am Anfang/Ende. Gleichgesetzt mit ReadableText, ValueString, SearchValue
    /// </summary>
    public override string ReadableText => _valueString;

    /// <summary>
    /// Der Wert ohne " am Anfang/Ende. Gleichgesetzt mit ReadableText, ValueString, SearchValue
    /// </summary>
    public override string SearchValue => _valueString;

    public override bool ToStringPossible => true;

    /// <summary>
    /// Der Wert mit " Anfang/Ende und entfernten kritischen Zeichen.
    /// </summary>
    public override string ValueForReplace => "\"" + _valueString.RemoveCriticalVariableChars() + "\"";

    /// <summary>
    /// Der Wert ohne " am Anfang/Ende. Gleichgesetzt mit ReadableText, ValueString, SearchValue
    /// </summary>
    public string ValueString {
        get => _valueString;
        set {
            if (ReadOnly) {
                Develop.DebugPrint(ErrorType.Warning, "Read Only Variable!"); // Wichtig für DatabaseVariables
                return;
            }
            _valueString = value.RestoreCriticalVariableChars(); // Variablen enthalten immer den richtigen Wert und es werden nur beim Ersetzen im Script die kritischen Zeichen entfernt
        }
    }

    #endregion

    #region Methods

    public override void DisposeContent() { }

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariableString v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        ValueString = v.ValueString;
        return string.Empty;
    }

    protected override void SetValue(object? x) {
        if (x is string val) {
            _valueString = val.RestoreCriticalVariableChars();
        } else {
            Develop.DebugPrint(ErrorType.Error, "Variablenfehler!");
        }
    }

    protected override bool TryParseValue(string txt, out object? result) {
        if (txt.Length > 1 && txt.StartsWith("\"") && txt.EndsWith("\"")) {
            var tmp = txt.Substring(1, txt.Length - 2); // Nicht Trimmen! Ansonsten wird sowas falsch: "X=" + "";
            tmp = tmp.Replace("\"+\"", string.Empty); // Zuvor die " entfernen! dann verketten! Ansonsten wird "+" mit nix ersetzte, anstelle einem  +
            if (tmp.Contains("\"")) { result = null; return false; } //SetError("Verkettungsfehler"); return; } // Beispiel: s ist nicht definiert und "jj" + s + "kk

            result = tmp;
            return true;
        }
        result = null;
        return false;
    }

    #endregion
}