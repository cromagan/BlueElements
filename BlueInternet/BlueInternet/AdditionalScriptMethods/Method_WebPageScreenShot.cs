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
using System.Drawing;
using System.Threading;
using BlueBasics;
using BlueDatabase.AdditionalScriptMethods;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using CefSharp;
using static BlueScript.Variables.VariableWebpage;

//using CefSharp.WinForms;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_WebPageScreenShot : Method_WebPage {

    #region Properties

    public override List<List<string>> Args => [WebPageVal];
    public override string Command => "webpagescreenshot";
    public override List<string> Constants => [];
    public override string Description => "Gibt die aktuelle Anzeige der WebPage zurück. NULL falls irgendwas fehlschlägt";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard | MethodType.DrawOnBitmap;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBitmap.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "WebPageScreenShot(WebPageVariable)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Da es keine Möglichkeit gibt, eine Url Variable (außerhalb eines If) zu deklarieren,
        // darf diese Routine nicht fehlschlagen.

        if (attvar.Attributes[0] is not VariableWebpage vwb) { return DoItFeedback.InternerFehler(ld); }

        if (vwb.ValueWebpage is not { IsDisposed: false } wb) { return new DoItFeedback("Keine Webseite geladen", false, ld); }
        if (wb.IsLoading) { return new DoItFeedback("Ladeprozess aktiv", false, ld); }

        try {
            Generic.CollectGarbage();
            const string jsString = "Math.max(document.body.scrollHeight, " +
                                    "document.documentElement.scrollHeight, document.body.offsetHeight, " +
                                    "document.documentElement.offsetHeight, document.body.clientHeight, " +
                                    "document.documentElement.clientHeight);";

            var executedScript = wb.EvaluateScriptAsync(jsString).Result.Result;
            const int width = 1280;
            var height = Convert.ToInt32(executedScript);

            var size = new Size(width, height);

            wb.Size = size;

            Thread.Sleep(500);
            // Wait for the screenshot to be taken.
            var bitmap = wb.ScreenshotOrNull();

            return new DoItFeedback(bitmap);
        } catch {
            return new DoItFeedback(null as Bitmap);
        }
    }

    #endregion
}