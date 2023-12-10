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

#nullable enable

using System.Collections.Generic;
using BlueDatabase.AdditionalScriptMethods;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using CefSharp.OffScreen;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_WebPageGetAllLinks : Method_WebPage {

    #region Properties

    public override List<List<string>> Args => new() { WebPageVal };
    public override string Command => "webpagegetalllinks";
    public override string Description => "Gibt eine Liste aller Links zurück.";
    public override bool EndlessArgs => false;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "WebPageGetAllLinks(WebPageVariable)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        if (attvar.Attributes[0] is not VariableWebpage vwb) { return new DoItFeedback(infos.Data, "Interner Fehler"); }

        if (vwb.ValueWebpage is not ChromiumWebBrowser wb) { return new DoItFeedback(infos.Data, "Keine Webseite geladen"); }
        if (wb.IsLoading) { return new DoItFeedback(infos.Data, "Ladeprozess aktiv"); }

        try {
            const string script = @"var inputs = document.getElementsByTagName('a');
                            var ids = [];
                            for (var i = 0; i < inputs.length; i++) {
                                if (inputs[i].id) {
                                    ids.push(inputs[i].href);
                                }
                            }
                            ids;";

            var task = DoTask(wb, script);

            var l = new List<string>();

            if (!task.IsFaulted && task.Result.Success && task.Result.Result is List<object> ids) {
                foreach (var id in ids) {
                    l.Add(id.ToString());
                }
                return new DoItFeedback(l);
            }

            // Es ist ein Fehler beim Ausführen des Skripts aufgetreten
            return new DoItFeedback(infos.Data, "Fehler beim Extrahieren der Links: " + task.Exception?.Message);
        } catch {
            return new DoItFeedback(infos.Data, "Allgemeiner Fehler beim Auslesen der Links.");
        }
    }

    #endregion
}