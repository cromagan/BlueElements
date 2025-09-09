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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BlueDatabase.AdditionalScriptMethods;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueScript.Variables.VariableWebpage;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_WebPageGetAllIds : Method_WebPage {

    #region Properties

    public override List<List<string>> Args => [WebPageVal];
    public override string Command => "webpagegetallids";
    public override List<string> Constants => [];
    public override string Description => "Gibt eine Liste aller IDS zurück, mit denen interagiert werden kann.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "WebPageGetAllIds(WebPageVariable)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableWebpage vwb) { return DoItFeedback.InternerFehler(ld); }

        if (vwb.ValueWebpage is not { IsDisposed: false } wb) { return new DoItFeedback("Keine Webseite geladen", false, ld); }
        if (wb.IsLoading) { return new DoItFeedback("Ladeprozess aktiv", false, ld); }

        try {
            //// Führen Sie den JavaScript-Code aus, um alle IDs auf der Seite zu extrahieren
            ////var script = @"var elements = document.querySelectorAll('*');
            ////                var ids = [];
            ////                for (var i = 0; i < elements.length; i++) {
            ////                    if (elements[i].id) {
            ////                        ids.push(elements[i].id);
            ////                    }
            ////                }
            ////                ids;";

            const string script = @"var inputs = document.getElementsByTagName('input');
                            var ids = [];
                            for (var i = 0; i < inputs.length; i++) {
                                if (inputs[i].id) {
                                    ids.push(inputs[i].id);
                                }
                            }
                            ids;";

            var task = DoTask(wb, script);

            var l = new List<string>();

            if (task is { IsFaulted: false, Result: { Success: true, Result: List<object> ids } }) {
                foreach (var id in ids) {
                    l.Add(id.ToString());
                }
                return new DoItFeedback(l);
            }

            // Es ist ein Fehler beim Ausführen des Skripts aufgetreten
            return new DoItFeedback("Fehler beim Extrahieren der IDs: " + task.Exception?.Message, false, ld);
        } catch {
            return new DoItFeedback("Allgemeiner Fehler beim Auslesen der IDs.", false, ld);
        }
    }

    #endregion
}