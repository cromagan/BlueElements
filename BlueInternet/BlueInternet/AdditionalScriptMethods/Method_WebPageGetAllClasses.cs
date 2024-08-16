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

using BlueDatabase.AdditionalScriptMethods;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_WebPageGetAllClasses : Method_WebPage {

    #region Properties

    public override List<List<string>> Args => [WebPageVal];
    public override string Command => "webpagegetallclasses";
    public override List<string> Constants => [];
    public override string Description => "Gibt eine Liste aller Klassen zurück.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "WebPageGetAllClasses(WebPageVariable)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableWebpage vwb) { return new DoItFeedback(ld, "Interner Fehler"); }

        if (vwb.ValueWebpage is not { IsDisposed: false } wb) { return new DoItFeedback(ld, "Keine Webseite geladen"); }
        if (wb.IsLoading) { return new DoItFeedback(ld, "Ladeprozess aktiv"); }

        try {
            const string script = @"
                    var elements = document.getElementsByTagName('*');
                    var classes = [];
                    for (var i = 0; i < elements.length; i++) {
                        var elementClasses = elements[i].classList;
                        for (var j = 0; j < elementClasses.length; j++) {
                            var className = elementClasses[j];
                            classes.push(className);
                        }
                    }
                    classes;";

            var task = DoTask(wb, script);

            var l = new List<string>();

            if (task is { IsFaulted: false, Result: { Success: true, Result: List<object> ids } }) {
                foreach (var id in ids) {
                    l.Add(id.ToString());
                }
                return new DoItFeedback(l);
            }

            // Es ist ein Fehler beim Ausführen des Skripts aufgetreten
            return new DoItFeedback(ld, "Fehler beim Extrahieren der Links: " + task.Exception?.Message);
        } catch {
            return new DoItFeedback(ld, "Allgemeiner Fehler beim Auslesen der Links.");
        }
    }

    #endregion
}