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

using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using CefSharp;
using CefSharp.OffScreen;
using System;
using System.Collections.Generic;
using System.IO;
using BlueDatabase.AdditionalScriptMethods;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_LoadUrl : Method_WebPage {

    #region Fields

    private static bool _didSettings;

    #endregion

    #region Properties

    public override List<List<string>> Args => new() { StringVal };
    public override string Command => "loadurl";
    public override string Description => "Lädt die angebene Internet-Adresse.\r\nDiese Routine wird keinen Fehler auslösen.\r\nFalls etwas schief läuft, enthält die Variable ein Webpage des Wertes NULL.\r\n\r\nAlle Befehle, die auf die Url zugreifen können, beginnen mit WebPage.";
    public override bool EndlessArgs => false;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => VariableWebpage.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "LoadUrl(Url)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        //https://keyholesoftware.com/2019/02/11/create-your-own-web-bots-in-net-with-cefsharp/

        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        // Da es keine Möglichkeit gibt, eine Url Variable (außerhalb eines If) zu deklarieren,
        // darf diese Routine nicht fehlschlagen.

        //if (attvar.ValueString(0).FileType() != FileFormat.Image) {
        //    return new DoItFeedback(infos.Data, "Datei ist kein Bildformat: " + attvar.ValueString(0));
        //}

        //if (!IO.FileExists(attvar.ValueString(0))) {
        //    return new DoItFeedback(infos.Data, "Datei nicht gefunden: " + attvar.ValueString(0));
        //}

        try {
            Generic.CollectGarbage();

            DoSettings();

            var browser = new ChromiumWebBrowser(attvar.ValueStringGet(0));

            if (!WaitLoaded(browser)) {
                return new DoItFeedback(new VariableWebpage(null as ChromiumWebBrowser));
            }
            return new DoItFeedback(new VariableWebpage(browser));
        } catch {
            return new DoItFeedback(new VariableWebpage(null as ChromiumWebBrowser));
        }
    }

    private void DoSettings() {
        if (_didSettings) { return; }
        _didSettings = true;

        CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

        var settings = new CefSettings() {
            CachePath = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
        };
        Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
    }

    #endregion
}