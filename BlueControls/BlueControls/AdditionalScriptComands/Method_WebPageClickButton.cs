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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using CefSharp;

//using CefSharp.WinForms;
using CefSharp.OffScreen;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_WebPageClickButton : Method {

    #region Properties

    public override List<List<string>> Args => new() { new() { VariableWebpage.ShortName_Variable }, StringVal };
    public override string Description => "Drückt einen Button in der Webpage und wartet, bis die Seite geladen ist.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "WebPageClickButton(WebPageVariable, id)";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "webpageclickbutton" };

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        if (attvar.Attributes[0] is not VariableWebpage vwb) { return new DoItFeedback(infos.Data, "Interner Fehler"); }

        if (vwb.ValueWebpage is not ChromiumWebBrowser wb) { return new DoItFeedback(infos.Data, "Keine Webseite geladen"); }
        if (wb.IsLoading) { return new DoItFeedback(infos.Data, "Ladeprozess aktiv"); }

        try {
            Generic.CollectGarbage();

            var script = @"var button = document.getElementById('" +  attvar.ValueStringGet(1)  + "');" + @"
                                 if (button) {
                                     button.click();
                                     'success';
                                  } else {
                                     'error';
                                  }";

            var task = wb.EvaluateScriptAsync(script);

            while (!task.IsCompleted) { Generic.Pause(0.1, false); }

            if (!Method_LoadUrl.WaitLoaded(wb)) {
                return new DoItFeedback("Webseite konnte nicht neu geladen werden.");
            }

            if (!task.IsFaulted && task.Result.Success && task.Result.Result is string result) {
                if (result == "success") { return DoItFeedback.Null(); }
                return new DoItFeedback("Fehler: Der Button wurde nicht gefunden.");
            }

            return new DoItFeedback("Fehler beim Klicken des Buttons: " + task.Exception?.Message);
        } catch {
            return new DoItFeedback("Allgemeiner Fehler beim Ausführen des TextBox-Befehles.");
        }
    }

    #endregion
}