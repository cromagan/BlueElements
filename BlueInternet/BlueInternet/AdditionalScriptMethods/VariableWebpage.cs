// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

//using CefSharp.WinForms;
using System;
using BlueScript.Structures;
using CefSharp.OffScreen;
using static BlueBasics.Interfaces.ParseableExtension;

namespace BlueScript.Variables;

public class VariableWebpage : Variable, IDisposable {

    #region Fields

    private ChromiumWebBrowser? _browser;

    #endregion

    #region Constructors

    public VariableWebpage(string name, ChromiumWebBrowser? value, bool ronly, bool system, string comment) : base(name, ronly, system, comment) => _browser = value;

    /// <summary>
    /// Wichtig für: GetEnumerableOfType<Variable>("NAME");
    /// </summary>
    /// <param name="name"></param>
    public VariableWebpage(string name) : this(name, null, true, false, string.Empty) { }

    public VariableWebpage(ChromiumWebBrowser? value) : this(DummyName(), value, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "web";
    public static string ShortName_Variable => "*web";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _browser == null;
    public override bool MustDispose => true;
    public override string MyClassId => ClassId;
    public override bool ToStringPossible => false;

    public ChromiumWebBrowser? ValueWebpage {
        get => _browser;
        set {
            if (ReadOnly) { return; }
            _browser = value;
        }
    }

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableWebpage(KeyName);
        v.Parse(ToString());
        return v;
    }

    public void Dispose() {
        _browser?.Dispose();
    }

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableWebpage v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        ValueWebpage = v.ValueWebpage;
        return DoItFeedback.Null();
    }

    protected override Variable? NewWithThisValue(object x) => null;

    protected override void SetValue(object? x) { }

    protected override object? TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) => null;

    #endregion
}