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
using Anthropic.SDK;
using BlueScript.Structures;
using static BlueBasics.Interfaces.ParseableExtension;

namespace BlueScript.Variables;

public class VariableAi : Variable {

    #region Fields

    public static readonly List<string> AiVal = [ShortName_Variable];

    private AnthropicClient? _client;

    #endregion

    #region Constructors

    public VariableAi(string name, AnthropicClient? value, bool ronly, string comment) : base(name, ronly, comment) => _client = value;

    /// <summary>
    /// Wichtig für: GetEnumerableOfType<Variable>("NAME");
    /// </summary>
    /// <param name="name"></param>
    public VariableAi(string name) : this(name, null, true, string.Empty) { }

    public VariableAi(AnthropicClient? value) : this(DummyName(), value, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "ari";
    public static string ShortName_Variable => "*ari";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _client == null;

    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => false;

    public AnthropicClient? ValueClient {
        get => _client;
        set {
            if (ReadOnly) { return; }
            _client = value;
        }
    }

    #endregion

    #region Methods

    public override void DisposeContent() => _client?.Dispose();

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableAi v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        ValueClient = v.ValueClient;
        return DoItFeedback.Null();
    }

    protected override void SetValue(object? x) { }

    protected override (bool cando, object? result) TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) => (false, null);

    #endregion
}