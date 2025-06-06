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

namespace BlueScript.Variables;

public class VariableUnknown : Variable {

    #region Fields

    private string _value = string.Empty;

    #endregion

    #region Constructors

    public VariableUnknown(string name, bool ronly, string comment) : base(name, ronly, comment) { }

    public VariableUnknown() : this(string.Empty, true, string.Empty) { }

    public VariableUnknown(string name) : this(name, true, string.Empty) { }

    public VariableUnknown(string name, string value) : this(name, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "ukn";
    public static string ShortName_Plain => "ukn";
    public static string ShortName_Variable => "*ukn";
    public override int CheckOrder => 100;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => false;

    /// <summary>
    /// Gleichgesetzt mit ValueString
    /// </summary>
    public override string ReadableText => "[unknown]";

    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => false;

    /// <summary>
    /// Der Wert ohne " am Anfang/Ende. Gleichgesetzt mit ReadableText
    /// </summary>
    public string Value {
        get => _value;
        set {
            if (ReadOnly) {
                Develop.DebugPrint(ErrorType.Warning, "Read Only Variable!"); // Wichtig für DatabaseVariables
                return;
            }
            _value = value.RestoreCriticalVariableChars(); // Variablen enthalten immer den richtigen Wert und es werden nur beim Ersetzen im Script die kritischen Zeichen entfernt
        }
    }

    public override string ValueForReplace => ReadableText;

    #endregion

    #region Methods

    public override void DisposeContent() { }

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableUnknown) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        return DoItFeedback.Null();
    }

    protected override void SetValue(object? x) {
        if (x is string val) {
            _value = val.RestoreCriticalVariableChars();
        } else {
            Develop.DebugPrint(ErrorType.Error, "Variablenfehler!");
        }
    }

    protected override (bool cando, object? result) TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) {
        if (scp != null) {
            Develop.Message?.Invoke(ErrorType.Info, null, scp.MainInfo, ImageCode.Formel, "Unbekannte Variable erstellt (ukn): " + txt, scp.Stufe);
        }
        return (true, txt);
    }

    #endregion
}