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
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_LoadTextFile : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "loadtextfile";
    public override string Description => "Lädt die angegebene Textdatei aus dem Dateisystem.";
    public override int LastArgMinCount => -1;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "LoadTextFile(Filename, UTF8/WIN1252)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var ft = attvar.ValueStringGet(0).FileType();

        if (ft is not FileFormat.Textdocument and not FileFormat.CSV) {
            return new DoItFeedback(infos.Data, "Datei ist kein Textformat: " + attvar.ValueStringGet(0));
        }

        if (!IO.FileExists(attvar.ValueStringGet(0))) {
            return new DoItFeedback(infos.Data, "Datei nicht gefunden: " + attvar.ValueStringGet(0));
        }

        try {
            string importText;
            switch (attvar.ValueStringGet(1).ToUpper()) {
                case "UTF8":
                    importText = File.ReadAllText(attvar.ValueStringGet(0), Encoding.UTF8);
                    break;

                case "WIN1252":
                    importText = File.ReadAllText(attvar.ValueStringGet(0), Constants.Win1252);
                    break;

                default:
                    return new DoItFeedback(infos.Data, "Import-Format unbekannt.");
            }

            return new DoItFeedback(importText);
        } catch {
            return new DoItFeedback(infos.Data, "Datei konnte nicht geladen werden: " + attvar.ValueStringGet(0));
        }
    }

    #endregion
}