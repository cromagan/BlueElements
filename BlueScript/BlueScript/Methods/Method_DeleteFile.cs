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
using BlueBasics.Interfaces;
using BlueScript.Enums;
using BlueScript.EventArgs;
using BlueScript.Interfaces;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_DeleteFile : Method, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [StringVal];

    public List<List<string>> ArgsForButton => Args;
    public List<string> ArgsForButtonDescription => ["Datei"];
    public ButtonArgs ClickableWhen => ButtonArgs.Genau_eine_Zeile;

    public override string Command => "deletefile";

    public override string Description => "Löscht die Datei aus dem Dateisystem. Gibt TRUE zurück, wenn die Datei nicht (mehr) existiert.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => -1;

    public override MethodType MethodType => MethodType.IO ;

    public override bool MustUseReturnValue => false;

    public string NiceTextForUser => "Eine Datei aus dem Dateisystem löschen";

    public override string Returns => VariableBool.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "DeleteFile(Filename)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var filn = attvar.ValueStringGet(0);

        if (!filn.IsFormat(FormatHolder.FilepathAndName)) { return new DoItFeedback(infos.Data, "Dateinamen-Fehler!"); }

        if (!IO.FileExists(filn)) {
            return DoItFeedback.Wahr();
        }

        if (!scp.ProduktivPhase) { return new DoItFeedback(infos.Data, "Löschen im Testmodus deaktiviert."); }

        try {
            return new DoItFeedback(IO.DeleteFile(filn, false));
        } catch {
            return new DoItFeedback(infos.Data, "Fehler beim Löschen: " + filn);
        }
    }

    public string TranslateButtonArgs(string arg1, string arg2, string arg3, string arg4, string filterarg, string rowarg) => arg1;

    #endregion
}