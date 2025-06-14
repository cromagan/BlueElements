﻿// Authors:
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

//using CefSharp.WinForms;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_WebPageClick : Method_WebPage {

    #region Properties

    public override List<List<string>> Args => [WebPageVal, StringVal];
    public override string Command => "webpageclick";
    public override List<string> Constants => [];
    public override string Description => "Fügt ein Pad-Item-Symbol einer Collection hinzu";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "WebPageClick(WebPage, id)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableWebpage vwb) { return DoItFeedback.InternerFehler(ld); }

        if (vwb.ValueWebpage is not { IsDisposed: false } wb) { return new DoItFeedback( "Keine Webseite geladen", false, ld); }
        if (wb.IsLoading) { return new DoItFeedback("Ladeprozess aktiv", false, ld); }

        try {

            #region Versuch, Button per ID

            var script = "var button = document.getElementById('" + attvar.ValueStringGet(1) + "');" + @"
                                 if (button) {
                                     button.click();
                                     'success';
                                  } else {
                                     'error';
                                  }";

            var task = DoTask(wb, script);

            if (!WaitLoaded(wb)) {
                return new DoItFeedback("Webseite konnte nicht neu geladen werden.", false, ld);
            }

            if (task is { IsFaulted: false, Result: { Success: true, Result: "success" } }) { return DoItFeedback.Null(); }

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
                return new DoItFeedback("Webseite konnte nicht neu geladen werden.", false, ld);
            }

            if (task is { IsFaulted: false, Result: { Success: true, Result: "success" } }) { return DoItFeedback.Null(); }

            //return new DoItFeedback(infos.Data, "Fehler: Der Button wurde nicht gefunden.");

            #endregion

            #region Versuch, Button per Klassenname

            script = "var element = document.querySelector('" + attvar.ValueStringGet(1) + "');" + @"
                    if (element) {
                        element.click();
                        'success';
                    } else {
                        'error';
                    }";

            task = DoTask(wb, script);

            if (!WaitLoaded(wb)) {
                return new DoItFeedback("Webseite konnte nicht neu geladen werden.", false, ld);
            }

            if (task is { IsFaulted: false, Result: { Success: true, Result: "success" } }) { return DoItFeedback.Null(); }

            //return new DoItFeedback(ld, "Fehler: Der Button wurde nicht gefunden.");

            #endregion

            return new DoItFeedback( "Fehler beim Klicken des Buttons: " + task.Exception?.Message, false, ld);
        } catch {
            return new DoItFeedback("Allgemeiner Fehler beim Ausführen des Button-Befehles.", false, ld);
        }
    }

    #endregion
}