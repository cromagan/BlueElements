// Authors:
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

#nullable enable

using System.Collections.Generic;
using BlueBasics;
using BlueDatabase.Enums;
using BlueScript;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.Interfaces;

public interface IUseableForButton {

    #region Properties

    public List<List<string>> ArgsForButton { get; }
    public List<string> ArgsForButtonDescription { get; }
    public ButtonArgs ClickableWhen { get; }

    public string Command { get; }

    public string NiceTextForUser { get; }

    #endregion

    #region Methods

    public DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp);

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg);

    #endregion
}

public static class UseableForButton {

    #region Methods

    public static string DoIt(this IUseableForButton ufb, VariableCollection varCol, List<string> args, string filterarg, string rowarg, object? additionalInfo) {
        for (var nr = 0; nr < args.Count; nr++) {
            if (nr < ufb.ArgsForButton.Count && string.IsNullOrEmpty(args[nr])) {
                return "Zu wenig Argumente erhalten";
            }
        }

        var sx = ufb.TranslateButtonArgs(args, filterarg, rowarg);

        var t = Script.ReduceText(sx);

        if (!string.IsNullOrEmpty(t.error)) {
            return "Fehler beim Berechnen der Attribute: " + t.error;
        }

        var ld = new LogData("Knopfdruck", 0);
        var cdw = new CanDoFeedback(0, t.reducedText, string.Empty, ld);

        var scp = new ScriptProperties("Knopfdruck im Formular", Method.AllMethods, true, [], additionalInfo, "Button");
        ufb.DoIt(varCol, cdw, scp);

        return cdw.LogData.Protocol.JoinWithCr();
    }

    #endregion
}