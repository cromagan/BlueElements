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
using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;

namespace BlueScript.Variables;

public class VariableString : Variable {

    #region Fields

    private string _valueString;

    #endregion

    #region Constructors

    public VariableString(string name, string value, bool ronly, bool system, string comment) : base(name, ronly, system, comment) => _valueString = value.RestoreCriticalVariableChars();

    /// <summary>
    /// Wichtig für: GetEnumerableOfType Variable ("NAME");
    /// </summary>
    /// <param name="name"></param>
    public VariableString(string name) : this(name, string.Empty, true, false, string.Empty) { }

    public VariableString(string name, string value) : this(name, value, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "str";
    public static string ShortName_Plain => "str";
    public static string ShortName_Variable => "*str";
    public override int CheckOrder => 2;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => string.IsNullOrEmpty(_valueString);
    public override string MyClassId => ClassId;

    /// <summary>
    /// Der Wert ohne " am Anfang/Ende. Gleichgesetzt mit ValueString
    /// </summary>
    public override string ReadableText => _valueString;

    public override bool ToStringPossible => true;

    /// <summary>
    /// Der Wert mit " Aanfang/Ende un entfernten Kritischen Zeichen.
    /// </summary>
    public override string ValueForReplace => "\"" + _valueString.RemoveCriticalVariableChars() + "\"";

    /// <summary>
    /// Der Wert ohne " am Anfang/Ende. Gleichgesetzt mit ReadableText
    /// </summary>
    public string ValueString {
        get => _valueString;
        set {
            if (ReadOnly) {
                Develop.DebugPrint(FehlerArt.Warnung, "Read Only Variable!"); // Wichtig für DatabaseVariables
                return;
            }
            _valueString = value.RestoreCriticalVariableChars(); // Variablen enthalten immer den richtigen Wert und es werden nur beim Ersetzen im Script die kritischen Zeichen entfernt
        }
    }

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableString(Name);
        v.Parse(ToString());
        return v;
    }

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableString v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        ValueString = v.ValueString;
        return DoItFeedback.Null();
    }

    protected override Variable NewWithThisValue(object x, VariableCollection vs) {
        var v = new VariableString(string.Empty);
        v.SetValue(x);
        return v;
    }

    protected override void SetValue(object? x) {
        if (x is string val) {
            _valueString = val.RestoreCriticalVariableChars();
        } else {
            Develop.DebugPrint(FehlerArt.Fehler, "Variablenfehler!");
        }
    }

    protected override object? TryParse(string txt, VariableCollection? vs, MethodType allowedMethods, List<Method> lm, bool changeValues, string scriptAttributes) {
        if (txt.Length > 1 && txt.StartsWith("\"") && txt.EndsWith("\"")) {
            var tmp = txt.Substring(1, txt.Length - 2); // Nicht Trimmen! Ansonsten wird sowas falsch: "X=" + "";
            tmp = tmp.Replace("\"+\"", string.Empty); // Zuvor die " entfernen! dann verketten! Ansonsten wird "+" mit nix ersetzte, anstelle einem  +
            if (tmp.Contains("\"")) { return null; } //SetError("Verkettungsfehler"); return; } // Beispiel: s ist nicht definiert und "jj" + s + "kk

            return tmp;
        }
        return null;
    }

    #endregion
}