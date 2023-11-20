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
using BlueScript.Methods;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using CefSharp.OffScreen;
using CefSharp;

namespace BlueDatabase.AdditionalScriptMethods;

public abstract class Method_WebPage : Method {

    #region Fields

    public static readonly List<string> WebPageVal = new() { VariableWebpage.ShortName_Variable };

    #endregion

    #region Methods

    public static bool AllImagesLoaded(ChromiumWebBrowser wb) {
        JavascriptResponse response = wb.EvaluateScriptAsync("document.readyState === 'complete'").GetAwaiter().GetResult();

        if (response.Success && response.Result is bool allImagesLoaded && allImagesLoaded) {
            return true;
        }

        //var result = DoTask(wb, "document.images.length === document.images.filter(img => img.complete).length");

        return false;
    }

    public static System.Threading.Tasks.Task<JavascriptResponse> DoTask(ChromiumWebBrowser wb, string script) {
        Generic.CollectGarbage();

        var task = wb.EvaluateScriptAsync(script);

        while (!task.IsCompleted) { Generic.Pause(0.1, false); }

        return task;
    }

    public static bool WaitLoaded(ChromiumWebBrowser browser) {
        Generic.Pause(0.1, false); // Um au jeden Fall das IsLoading zu erfassen

        #region  Warten, bis der Ladevorgang gestartet ist

        var d = DateTime.UtcNow;
        while (!browser.IsLoading) {
            Develop.DoEvents();
            if (DateTime.UtcNow.Subtract(d).TotalSeconds > 10) {
                return true;
            }
        }

        #endregion

        #region  Warten, bis der Ladevorgang abgeschlossen ist

        d = DateTime.UtcNow;
        while (browser.IsLoading || !AllImagesLoaded(browser)) {
            Generic.Pause(1, true);
            if (DateTime.UtcNow.Subtract(d).TotalSeconds > 60) {
                return false;
            }
        }

        #endregion

        return true;
    }

    #endregion
}