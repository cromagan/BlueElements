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

using System.Collections.Generic;
using System.Drawing;

namespace BlueScript.Variables;

public class VariableBitmap : Variable {

    #region Fields

    public static readonly List<string> BmpVar = [ShortName_Variable];
    private Bitmap? _bmp;

    #endregion

    #region Constructors

    public VariableBitmap(string name, Bitmap? value, bool ronly, string comment) : base(name, ronly, comment) => _bmp = value;

    public VariableBitmap() : this(string.Empty, null, true, string.Empty) { }

    public VariableBitmap(string name) : this(name, null, true, string.Empty) { }

    public VariableBitmap(Bitmap? value) : this(DummyName(), value, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "bmp";
    public static string ShortName_Variable => "*bmp";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _bmp == null;
    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => false;

    public Bitmap? ValueBitmap {
        get => _bmp;
        set {
            if (ReadOnly) { return; }
            _bmp = value;
        }
    }

    public override string ValueForCell => string.Empty;

    #endregion

    #region Methods

    public override void DisposeContent() => _bmp?.Dispose();

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariableBitmap v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        ValueBitmap = v.ValueBitmap;
        return string.Empty;
    }

    protected override void SetValue(object? x) { }

    protected override bool TryParseValue(string txt, out object? result) {
        result = null;
        return false;
    }

    #endregion
}