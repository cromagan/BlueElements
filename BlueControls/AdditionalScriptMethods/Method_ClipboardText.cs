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

using BlueScript.Classes;
using BlueScript.Methods;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BlueControls.AdditionalScriptMethods;

internal sealed class Method_ClipboardText : Method {

    #region Properties

    public static List<List<string>> Args => [];
    public static string Command => "clipboardtext";
    public static List<string> Constants => [];
    public static string Description => "Gibt den Inhalt des Windows Clipboards als Text zurück. Falls kein Text im Clipboard enthalten ist, wird ein leerer String zurückgegeben.\r\nMit SetClipoard kann ein Wert in das Clipboard geschrieben werden.";

    public static int LastArgMinCount => -1;

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableString.ShortName_Plain;
    public static string StartSequence => "(";
    public static string Syntax => "ClipboardText()";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => Clipboard.ContainsText() ? new DoItFeedback(Clipboard.GetText()) : DoItFeedback.Null();

    #endregion
}