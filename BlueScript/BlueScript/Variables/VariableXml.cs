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

using System.Xml.Linq;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript;

public class VariableXml : Variable {

    #region Fields

    private XDocument? _xml;

    #endregion

    #region Constructors

    public VariableXml(string name, XDocument value, bool ronly, bool system, string comment) : base(name, ronly, system, comment) => _xml = value;

    public VariableXml(string name) : this(name, null!, true, false, string.Empty) { }

    public VariableXml(XDocument value) : this(DummyName(), value, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "xml";
    public static string ShortName_Variable => "*xml";
    public override int CheckOrder => 99;

    public override bool GetFromStringPossible => false;

    public override bool IsNullOrEmpty => _xml == null;

    public override string MyClassId => ClassId;

    public override bool ToStringPossible => false;

    public XDocument? XML {
        get => _xml;
        set {
            if (ReadOnly) { return; }
            _xml = value;
        }
    }

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableXml(Name);
        v.Parse(ToString());
        return v;
    }

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableXml v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        XML = v._xml;
        return DoItFeedback.Null();
    }

    protected override Variable? NewWithThisValue(object x) => null;

    protected override void SetValue(object? x) { }

    protected override object? TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) => null;

    #endregion
}