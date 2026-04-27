// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueBasics.ClassesStatic;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal sealed class Method_DownloadWebPage : Method {

    #region Fields

    private static readonly VariableCollection Last = [];

    #endregion

    #region Properties

    public static List<List<string>> Args => [StringVal, StringVal, StringVal];
    public static string Command => "downloadwebpage";
    public static List<string> Constants => [];
    public static string Description => "Lädt die angegebene Webseite aus dem Internet.\r\nGibt niemals einen Fehler zurück, eber evtl. string.empty";

    public static int LastArgMinCount => -1;
    public static MethodType MethodLevel => MethodType.LongTime;
    public static bool MustUseReturnValue => true;
    public static string Returns => VariableString.ShortName_Variable;
    public static string StartSequence => "(";
    public static string Syntax => "DownloadWebPage(Url)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var url = attvar.ValueStringGet(0);
        var varn = "X" + url.ReduceToChars(BlueBasics.ClassesStatic.Constants.AllowedCharsVariableName);

        if (Last.GetByKey(varn) is VariableString vb) {
            return new DoItFeedback(vb.ValueString);
        }

        try {
            Generic.CollectGarbage();
            var txt = Generic.Download(url);

            Last.Add(new VariableString(varn, txt, true, string.Empty));
            return new DoItFeedback(txt);
        } catch {
            return new DoItFeedback(string.Empty);
        }
    }

    #endregion
}