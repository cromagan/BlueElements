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

using BlueScript.Enums;
using BlueScript.EventArgs;
using BlueScript.Interfaces;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using static BlueBasics.Extensions;
using static BlueBasics.IO;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_DirectoryCreate : Method, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [StringVal];

    public List<List<string>> ArgsForButton => Args;
    public List<string> ArgsForButtonDescription => ["Verzeichnissname"];
    public ButtonArgs ClickableWhen => ButtonArgs.Genau_eine_Zeile;

    public override string Command => "directorycreate";

    public override string Description => "Erstellt ein Verzeichnis, falls dieses nicht existert. Gibt TRUE zurück, erstellt wurde oder bereits existierte.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => -1;

    public override MethodType MethodType => MethodType.IO ;

    public override bool MustUseReturnValue => false;

    public string NiceTextForUser => "Ein Verzeichnis im Dateisystem erstellen";

    public override string Returns => VariableBool.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "DirectoryCreate(Path)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var p = attvar.ValueStringGet(0).TrimEnd("\\");

        if (DirectoryExists(p)) { return DoItFeedback.Wahr(); }

        try {
            _ = Directory.CreateDirectory(p);
        } catch { }

        return !DirectoryExists(p) ? DoItFeedback.Falsch() : DoItFeedback.Wahr();
    }

    public string TranslateButtonArgs(string arg1, string arg2, string arg3, string arg4, string filterarg, string rowarg) => arg1;

    #endregion
}