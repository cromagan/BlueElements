// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueScript.Structures;
using System.Drawing;

namespace BlueScript.Variables;

public class VariableBitmap : Variable {

    #region Fields

    private Bitmap? _bmp;

    #endregion

    #region Constructors

    public VariableBitmap(string name, Bitmap? value, bool ronly, bool system, string coment) : base(name, ronly, system, coment) {
        _bmp = value;
    }

    /// <summary>
    /// Wichtig für: GetEnumerableOfType<Variable>("NAME");
    /// </summary>
    /// <param name="name"></param>
    public VariableBitmap(string name) : this(name, null, true, false, string.Empty) { }

    public VariableBitmap(Bitmap? value) : this(DummyName(), value, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ShortName_Variable => "*bmp";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _bmp == null;
    public override string ShortName => "bmp";
    public override bool ToStringPossible => false;

    public Bitmap? ValueBitmap {
        get => _bmp;
        set {
            if (Readonly) { return; }
            _bmp = value;
        }
    }

    #endregion

    #region Methods

    public override DoItFeedback GetValueFrom(Variable variable) {
        if (variable is not VariableBitmap v) { return DoItFeedback.VerschiedeneTypen(this, variable); }
        if (Readonly) { return DoItFeedback.Schreibgschützt(); }
        ValueBitmap = v.ValueBitmap;
        return DoItFeedback.Null();
    }

    protected override bool TryParse(string txt, out Variable? succesVar, Script s) {
        succesVar = null;
        return false;
    }

    #endregion
}