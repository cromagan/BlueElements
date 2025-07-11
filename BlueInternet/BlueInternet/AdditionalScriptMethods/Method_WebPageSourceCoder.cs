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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BlueBasics;
using BlueDatabase.AdditionalScriptMethods;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueScript.Variables.VariableWebpage;

//using CefSharp.WinForms;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_WebPageSourceCode : Method_WebPage {

    #region Properties

    public override List<List<string>> Args => [WebPageVal];
    public override string Command => "webpagesourcecode";
    public override List<string> Constants => [];
    public override string Description => "Gibt den Quell-Code-Text der Webpage zurück.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "WebPageSourceCode(WebPageVariable)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableWebpage vwb) { return DoItFeedback.InternerFehler(ld); }

        if (vwb.ValueWebpage is not { IsDisposed: false } wb) { return new DoItFeedback("Keine Webseite geladen", false, ld); }
        if (wb.IsLoading) { return new DoItFeedback("Ladeprozess aktiv", false, ld); }

        try {
            Generic.CollectGarbage();

            const string script = "document.documentElement.outerHTML";

            var task = DoTask(wb, script);

            if (!WaitLoaded(wb)) {
                return new DoItFeedback("Webseite konnte nicht neu geladen werden.", false, ld);
            }

            if (task is { IsFaulted: false, Result: { Success: true, Result: string result } }) {
                return new DoItFeedback(result);
            }

            //var task = wb.GetSourceAsync();

            //while (!task.IsCompleted) { Generic.Pause(0.1, false); }

            //var mainFrameHtmlSource = wb.GetMainFrame().GetSourceAsync();
            //while (!mainFrameHtmlSource.IsCompleted) { Generic.Pause(0.1, false); }
            //Console.WriteLine(mainFrameHtmlSource.Result);

            ////// Rufen Sie alle Unterframes ab
            //var frameIdentifiers = wb.GetBrowser().GetFrameIdentifiers();

            ////// Durchlaufen Sie alle Unterframes und rufen Sie den HTML-Quellcode ab
            //foreach (var frameIdentifier in frameIdentifiers) {
            //    var frame = wb.GetBrowser().GetFrame(frameIdentifier);
            //    var frameHtml = frame.GetSourceAsync();
            //    while (!frameHtml.IsCompleted) { Generic.Pause(0.1, false); }
            //    Console.WriteLine(frameHtml.Result);
            //}

            return new DoItFeedback("Quellcode konnte nicht gelesen werden.", false, ld);
        } catch (Exception ex) {
            return new DoItFeedback("Quellcode konnte nicht gelesen werden: " + ex, false, ld);
        }
    }

    #endregion
}