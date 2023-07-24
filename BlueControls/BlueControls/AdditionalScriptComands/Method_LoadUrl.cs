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

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_LoadUrl : Method {

    #region Fields

    private static bool didSettings = false;

    #endregion

    #region Properties

    public override List<List<string>> Args => new() { StringVal };
    public override string Description => "Lädt die angebenen Internet-Adresse.\r\nDiese Routine wird keinen Fehler auslösen.\r\nFalls etwas schief läuft, enthält die Variable ein Webpage des Wertes NULL.\r\n\r\nAlle Befehle, die auf die Url zugreifen können, beginnen mit WebPage.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => VariableWebpage.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "LoadUrl(Url)";

    #endregion

    #region Methods

    public static bool WaitLoaded(ChromiumWebBrowser browser) {
        Generic.Pause(0.1, false); // Um au jeden Fall das IsLoading zu erfassen

        #region  Warten, bis der Ladevorgang gestartet ist

        var d = DateTime.Now;
        while (!browser.IsLoading) {
            Develop.DoEvents();
            if (DateTime.Now.Subtract(d).TotalSeconds > 10) {
                return true;
            }
        }

        #endregion

        #region  Warten, bis der Ladevorgang abgeschlossen ist

        d = DateTime.Now;
        while (browser.IsLoading) {
            Generic.Pause(1, true);
            if (DateTime.Now.Subtract(d).TotalSeconds > 30) {
                return false;
            }
        }

        #endregion

        return true;
    }

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "loadurl" };

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
        if (didSettings) { return; }
        didSettings = true;

        CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

        var settings = new CefSettings() {
            CachePath = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
        };
        Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
    }

    #endregion

    //private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e) {
    //    var browser = (ChromiumWebBrowser)(sender);

    //    var bitmap = new Bitmap(browser.Width, browser.Height);
    //    browser.DrawToBitmap(bitmap, new Rectangle(0, 0, browser.Width, browser.Height));

    //    bitmap.Save("D:\\Test111.png", ImageFormat.Png);
    //}
}