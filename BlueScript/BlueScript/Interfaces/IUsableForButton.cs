// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueScript.EventArgs;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Interfaces;

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

    public string TranslateButtonArgs(string arg1, string arg2, string arg3, string arg4, string filterarg, string rowarg);

    #endregion
}

public static class UseableForButton {

    #region Methods

    public static string DoIt(this IUseableForButton ufb, VariableCollection varCol, string arg1, string arg2, string arg3, string arg4, string filterarg, string rowarg, object? additionalInfo) {
        List<string> args = [arg1, arg2, arg3, arg4];

        for (var nr = 0; nr < args.Count; nr++) {
            //if (nr >= ufb.ArgsForButton.Count && !string.IsNullOrEmpty(args[nr])) {
            //    return "Zu viele Argumente erhalten";
            //}

            if (nr < ufb.ArgsForButton.Count && string.IsNullOrEmpty(args[nr])) {
                return "Zu wenig Argumente erhalten";
            }
        }

        var s = ufb.TranslateButtonArgs(arg1, arg2, arg3, arg4, filterarg, rowarg);

        var ld = new LogData("Knopfdruck", 0);
        var cdw = new CanDoFeedback(0, s, string.Empty, ld);

        var scp = new ScriptProperties("Knopfdruck im Formular", Enums.MethodType.AllDefault, true, [], additionalInfo);

        var erg = ufb.DoIt(varCol, cdw, scp);

        if (erg.AllOk) { return string.Empty; }
        return cdw.Data.Protocol.JoinWithCr();
    }

    #endregion
}