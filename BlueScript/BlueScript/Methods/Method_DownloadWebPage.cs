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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_DownloadWebPage : Method {

    #region Fields

    private static readonly VariableCollection Last = new();

    #endregion

    #region Properties

    public override List<List<string>> Args => new() { StringVal, StringVal, StringVal };
    public override string Comand => "downloadwebpage";
    public override string Description => "Lädt die angegebene Webseite aus dem Internet.\r\nGibt niemals einen Fehler zurück, eber evtl. string.empty";
    public override bool EndlessArgs => false;
    
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => VariableString.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "DownloadWebPage(Url)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var url = attvar.ValueStringGet(0);
        var varn = "X" + url.ReduceToChars(Constants.AllowedCharsVariableName);

        if (Last.Get(varn) is VariableString vb) {
            return new DoItFeedback(vb.ValueString);
        }

        try {
            Generic.CollectGarbage();
            var txt = Generic.Download(url);

            Last.Add(new VariableString(varn, txt, true, false, string.Empty));
            return new DoItFeedback(txt);
        } catch {
            return new DoItFeedback(string.Empty);
        }
    }

    #endregion
}