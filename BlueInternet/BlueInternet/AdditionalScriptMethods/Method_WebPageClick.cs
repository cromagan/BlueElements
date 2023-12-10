﻿// Authors:
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

//using CefSharp.WinForms;
using CefSharp.OffScreen;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_WebPageClick : Method_WebPage {

    #region Properties

    public override List<List<string>> Args => new() { WebPageVal, StringVal };
    public override string Command => "webpageclick";
    public override string Description => "Drückt einen Button, Klasse oder Link in der Webpage und wartet, bis die Seite geladen ist.";
    public override bool EndlessArgs => false;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "WebPageClick(WebPageVariable, id)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        if (attvar.Attributes[0] is not VariableWebpage vwb) { return new DoItFeedback(infos.Data, "Interner Fehler"); }

        if (vwb.ValueWebpage is not ChromiumWebBrowser wb) { return new DoItFeedback(infos.Data, "Keine Webseite geladen"); }
        if (wb.IsLoading) { return new DoItFeedback(infos.Data, "Ladeprozess aktiv"); }

        try {

            #region Versuch, Button per ID

            var script = @"var button = document.getElementById('" + attvar.ValueStringGet(1) + "');" + @"
                                 if (button) {
                                     button.click();
                                     'success';
                                  } else {
                                     'error';
                                  }";

            var task = DoTask(wb, script);

            if (!WaitLoaded(wb)) {
                return new DoItFeedback(infos.Data, "Webseite konnte nicht neu geladen werden.");
            }

            if (!task.IsFaulted && task.Result.Success && task.Result.Result is "success") { return DoItFeedback.Null(); }

            //return new DoItFeedback(infos.Data, "Fehler: Der Button wurde nicht gefunden.");

            #endregion

            #region Versuch, Button per Angzeigten Text

            script = @"var elements = document.getElementsByTagName('button');
                    var element = null;
                    for (var i = 0; i < elements.length; i++) {
                        var buttonText = elements[i].textContent.trim();
                        if (buttonText === '" + attvar.ValueStringGet(1) + @"') {
                            element = elements[i];
                            break;
                        }
                    }
                    if (element) {
                        element.click();
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

            #region Versuch, Button per Klassenname

            script = @"var element = document.querySelector('" + attvar.ValueStringGet(1) + "');" + @"
                    if (element) {
                        element.click();
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

            return new DoItFeedback(infos.Data, "Fehler beim Klicken des Buttons: " + task.Exception?.Message);
        } catch {
            return new DoItFeedback(infos.Data, "Allgemeiner Fehler beim Ausführen des Button-Befehles.");
        }
    }

    #endregion
}