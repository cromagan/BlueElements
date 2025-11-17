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
using BlueBasics.Interfaces;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Text;
using static BlueBasics.IO;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
internal class Method_SaveText : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public override string Command => "savetext";
    public override List<string> Constants => ["UTF8", "WIN1252"];
    public override string Description => "Speichert den Text auf die Festplatte";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "SaveText(Filename, UTF8/WIN1252, Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {

        #region  Dateinamen ermitteln (filn)

        var filn = attvar.ValueStringGet(0);
        if (string.IsNullOrEmpty(filn)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }

        if (!filn.IsFormat(FormatHolder.FilepathAndName)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }

        var pf = filn.PathParent();
        if (string.IsNullOrEmpty(pf)) { return new DoItFeedback($"Dateinamen-Fehler: '{pf}'", true, ld); }
        if (!DirectoryExists(pf)) { return new DoItFeedback($"Verzeichnis '{pf}' existiert nicht.", true, ld); }
        if (!CanWriteInDirectory(pf)) { return new DoItFeedback($"Keine Schreibrechte im ZielVerzeichnis '{pf}'.", true, ld); }

        if (FileExists(filn)) { return new DoItFeedback("Datei existiert bereits.", true, ld); }

        #endregion

        //if (!scp.ChangeValues) { return new DoItFeedback(ld, "Bild Speichern im Testmodus deaktiviert."); }

        switch (attvar.ValueStringGet(1).ToUpperInvariant()) {
            case "UTF8":
                if (!WriteAllText(filn, attvar.ValueStringGet(2), Encoding.UTF8, false)) {
                    return new DoItFeedback("Fehler beim Erzeugen der Datei.", true, ld);
                }

                break;

            case "WIN1252":
                if (!WriteAllText(filn, attvar.ValueStringGet(2), BlueBasics.Constants.Win1252, false)) {
                    return new DoItFeedback("Fehler beim Erzeugen der Datei.", true, ld);
                }
                break;

            default:
                return new DoItFeedback("Export-Format unbekannt.", true, ld);
        }

        return DoItFeedback.Null();
    }

    #endregion
}