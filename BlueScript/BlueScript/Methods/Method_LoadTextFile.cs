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
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
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
            return new DoItFeedback("Datei ist kein Textformat: " + filen, true, ld);
        }

        if (!IO.FileExists(filen)) {
            return new DoItFeedback("Datei nicht gefunden: " + filen, true, ld);
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
                    return new DoItFeedback("Import-Format unbekannt.", true, ld);
            }

            return new DoItFeedback(importText);
        } catch {
            return new DoItFeedback("Datei konnte nicht geladen werden: " + filen, true, ld);
        }
    }

    #endregion
}