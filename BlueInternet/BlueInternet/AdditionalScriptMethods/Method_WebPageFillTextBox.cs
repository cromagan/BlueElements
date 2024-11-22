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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BlueDatabase.AdditionalScriptMethods;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueScript.Variables.VariableWebpage;

//using CefSharp.WinForms;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_WebPageFillTextBox : Method_WebPage {

    #region Properties

    public override List<List<string>> Args => [WebPageVal, StringVal, StringVal];
    public override string Command => "webpagefilltextbox";
    public override List<string> Constants => [];
    public override string Description => "Füllt ein Textfeld in der Webpage aus.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "WebPageFillTextBox(WebPageVariable, id, value)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableWebpage vwb) { return DoItFeedback.InternerFehler(ld); }

        if (vwb.ValueWebpage is not { IsDisposed: false } wb) { return new DoItFeedback(ld, "Keine Webseite geladen"); }
        if (wb.IsLoading) { return new DoItFeedback(ld, "Ladeprozess aktiv"); }

        try {
            //     var script = @"
            //     var inputField = document.querySelector('.login_username input[type=""text""]');
            //     if (inputField) {
            //         inputField.value = 'Hallo';
            //  var event = new Event('input', { bubbles: true });
            //inputField.dispatchEvent(event);
            //     }
            // ";

            //     var task = DoTask(wb, script);

            //     if (!WaitLoaded(wb)) {
            //         return new DoItFeedback(infos.Data, "Webseite konnte nicht neu geladen werden.");
            //     }

            //     return DoItFeedback.Null();

            #region Versuch, Textbox per ID

            var script = "var inputField = document.getElementById('" + attvar.ValueStringGet(1) + "');" + @"
                                 if (inputField) {
                                     inputField.value = '" + attvar.ValueStringGet(2) + @"'
                                     var event = new Event('input', { bubbles: true });
                                     inputField.dispatchEvent(event);
                                     'success';
                                  } else {
                                     'error';
                                  }";

            var task = DoTask(wb, script);

            if (!WaitLoaded(wb)) {
                return new DoItFeedback(ld, "Webseite konnte nicht neu geladen werden.");
            }

            if (task is { IsFaulted: false, Result: { Success: true, Result: "success" } }) { return DoItFeedback.Null(); }

            #endregion

            #region Versuch, Textbox per Klassenname

            script = "var inputField = document.querySelector('" + attvar.ValueStringGet(1) + "');" + @"
                                 if (inputField) {
                                     inputField.value = '" + attvar.ValueStringGet(2) + @"'
                                     var event = new Event('input', { bubbles: true });
                                     inputField.dispatchEvent(event);
                                     'success';
                                  } else {
                                     'error';
                                  }";

            task = DoTask(wb, script);

            if (!WaitLoaded(wb)) {
                return new DoItFeedback(ld, "Webseite konnte nicht neu geladen werden.");
            }

            if (task is { IsFaulted: false, Result: { Success: true, Result: "success" } }) { return DoItFeedback.Null(); }

            //return new DoItFeedback(infos.Data, "Fehler: Der Button wurde nicht gefunden.");

            #endregion

            //var script = "document.getElementById('" + attvar.ValueStringGet(1) + "').value = '" + attvar.ValueStringGet(2) + "'";
            //var task = DoTask(wb, script);

            //if (!WaitLoaded(wb)) {
            //    return new DoItFeedback(infos.Data, "Webseite konnte nicht neu geladen werden.");
            //}

            //if (!task.IsFaulted) {
            //    var response = task.Result;
            //    if (!response.Success) {
            //        // Es ist ein Fehler beim Ausführen des Skripts aufgetreten
            //        return new DoItFeedback(infos.Data, "Fehler beim Befüllen des Feldes: " + response.Message);
            //    }
            //    return DoItFeedback.Null();
            //}
            //return new DoItFeedback(infos.Data, "Allgemeiner Fehler beim Ausführen des TextBox-Befehles.");
            return new DoItFeedback(ld, "Fehler beim Ausführen des TextBox-Befehles: " + task.Exception?.Message);
        } catch {
            return new DoItFeedback(ld, "Allgemeiner Fehler beim Ausführen des TextBox-Befehles.");
        }
    }

    #endregion
}