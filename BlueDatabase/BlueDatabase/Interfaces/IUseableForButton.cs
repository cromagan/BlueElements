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

using BlueBasics;
using BlueDatabase.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;


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

    public string TranslateButtonArgs(string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string filterarg, string rowarg);

    #endregion
}

public static class UseableForButton {

    #region Methods

    public static string DoIt(this IUseableForButton ufb, VariableCollection varCol, string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string filterarg, string rowarg, object? additionalInfo) {
        List<string> args = [arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8];

        for (var nr = 0; nr < args.Count; nr++) {
            if (nr < ufb.ArgsForButton.Count && string.IsNullOrEmpty(args[nr])) {
                return "Zu wenig Argumente erhalten";
            }
        }

        var s = ufb.TranslateButtonArgs(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, filterarg, rowarg);

        var ld = new LogData("Knopfdruck", 0);
        var cdw = new CanDoFeedback(0, s, string.Empty, ld);

        var scp = new ScriptProperties("Knopfdruck im Formular", BlueScript.Enums.MethodType.AllDefault, true, [], additionalInfo, 0);

        var erg = ufb.DoIt(varCol, cdw, scp);

        return cdw.LogData.Protocol.JoinWithCr();
    }

    #endregion
}