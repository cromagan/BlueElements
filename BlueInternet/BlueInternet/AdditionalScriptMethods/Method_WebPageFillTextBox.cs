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
using CefSharp.OffScreen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

//using CefSharp.WinForms;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_WebPageFillTextBox : Method_WebPage {

    #region Properties

    public override List<List<string>> Args => [WebPageVal, StringVal, StringVal];
    public override string Command => "webpagefilltextbox";
    public override string Description => "Füllt ein Textfeld in der Webpage aus.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "WebPageFillTextBox(WebPageVariable, id, value)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        if (attvar.Attributes[0] is not VariableWebpage vwb) { return new DoItFeedback(infos.Data, "Interner Fehler"); }

        if (vwb.ValueWebpage is not ChromiumWebBrowser wb) { return new DoItFeedback(infos.Data, "Keine Webseite geladen"); }
        if (wb.IsLoading) { return new DoItFeedback(infos.Data, "Ladeprozess aktiv"); }

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

            var script = @"var inputField = document.getElementById('" + attvar.ValueStringGet(1) + "');" + @"
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
                return new DoItFeedback(infos.Data, "Webseite konnte nicht neu geladen werden.");
            }

            if (!task.IsFaulted && task.Result.Success && task.Result.Result is "success") { return DoItFeedback.Null(); }

            #endregion

            #region Versuch, Textbox per Klassenname

            script = @"var inputField = document.querySelector('" + attvar.ValueStringGet(1) + "');" + @"
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
                return new DoItFeedback(infos.Data, "Webseite konnte nicht neu geladen werden.");
            }

            if (!task.IsFaulted && task.Result.Success && task.Result.Result is "success") { return DoItFeedback.Null(); }

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
            return new DoItFeedback(infos.Data, "Fehler beim Ausführen des TextBox-Befehles: " + task.Exception?.Message);
        } catch {
            return new DoItFeedback(infos.Data, "Allgemeiner Fehler beim Ausführen des TextBox-Befehles.");
        }
    }

    #endregion
}