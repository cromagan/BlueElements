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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_LoadTextFile : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "loadtextfile";
    public override List<string> Constants => ["UTF8", "WIN1252"];
    public override string Description => "Lädt die angegebene Textdatei aus dem Dateisystem.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.SpecialVariables;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "LoadTextFile(Filename, UTF8/WIN1252)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var filen = attvar.ValueStringGet(0);

        if (filen.FileType() is not FileFormat.Textdocument and not FileFormat.CSV) {
            return new DoItFeedback(ld, "Datei ist kein Textformat: " + filen);
        }

        if (!IO.FileExists(filen)) {
            return new DoItFeedback(ld, "Datei nicht gefunden: " + filen);
        }

        try {
            string importText;
            switch (attvar.ValueStringGet(1).ToUpperInvariant()) {
                case "UTF8":
                    importText = File.ReadAllText(filen, Encoding.UTF8);
                    break;

                case "WIN1252":
                    importText = File.ReadAllText(filen, BlueBasics.Constants.Win1252);
                    break;

                default:
                    return new DoItFeedback(ld, "Import-Format unbekannt.");
            }

            return new DoItFeedback(importText);
        } catch {
            return new DoItFeedback(ld, "Datei konnte nicht geladen werden: " + filen);
        }
    }

    #endregion
}