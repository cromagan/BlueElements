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

using BlueBasics;
using BlueScript;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using BlueTable.Enums;
using System.Collections.Generic;

namespace BlueTable.Interfaces;

public interface IUseableForButton {

    #region Properties

    List<List<string>> ArgsForButton { get; }
    List<string> ArgsForButtonDescription { get; }
    ButtonArgs ClickableWhen { get; }

    string Command { get; }

    string NiceTextForUser { get; }

    #endregion

    #region Methods

    DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp);

    string TranslateButtonArgs(List<string> args, string filterarg, string rowarg);

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

        var (normalizedText, error) = Script.NormalizedText(sx);

        if (!string.IsNullOrEmpty(error)) {
            return "Fehler beim Berechnen der Attribute: " + error;
        }

        var ld = new LogData("Knopfdruck", 0);
        var cdw = new CanDoFeedback(0, normalizedText, string.Empty, ld);

        var scp = new ScriptProperties("Knopfdruck im Formular", Method.AllMethods, true, [], additionalInfo, "Button", "Button");
        ufb.DoIt(varCol, cdw, scp);

        return cdw.LogData?.Protocol.JoinWithCr() ?? string.Empty;
    }

    #endregion
}