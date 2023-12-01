// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueDatabase;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.IO;

namespace BlueScript.Methods;

internal class Method_SaveText : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, StringVal, StringVal };
    public override string Command => "savetext";
    public override string Description => "Speichert den Text auf die Festplatte";
    public override bool EndlessArgs => true;

    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => string.Empty;

    public override string StartSequence => "(";
    public override string Syntax => "SaveText(Filename, UTF8/WIN1252, Text);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        #region  Dateinamen ermitteln (filn)

        var filn = attvar.ValueStringGet(0);
        if (string.IsNullOrEmpty(filn)) { return new DoItFeedback(infos.Data, "Dateinamen-Fehler!"); }

        if (!filn.IsFormat(FormatHolder.FilepathAndName)) { return new DoItFeedback(infos.Data, "Dateinamen-Fehler!"); }

        var pf = filn.PathParent();
        if (string.IsNullOrEmpty(pf)) { return new DoItFeedback(infos.Data, "Dateinamen-Fehler!"); }
        if (!Directory.Exists(pf)) { return new DoItFeedback(infos.Data, "Verzeichniss existiert nicht"); }
        if (!IO.CanWriteInDirectory(pf)) { return new DoItFeedback(infos.Data, "Keine Schreibrechte im Zielverzeichniss."); }

        if (File.Exists(filn)) { return new DoItFeedback(infos.Data, "Datei existiert bereits."); }

        #endregion

        //if (!scp.ChangeValues) { return new DoItFeedback(infos.Data, "Bild Speichern im Testmodus deaktiviert."); }

        switch (attvar.ValueStringGet(1).ToUpper()) {
            case "UTF8":
                if (!IO.WriteAllText(filn, attvar.ValueStringGet(2), System.Text.Encoding.UTF8, false)) {
                    return new DoItFeedback(infos.Data, "Fehler beim Erzeugen der Datei.");
                }

                break;

            case "WIN1252":
                if (!IO.WriteAllText(filn, attvar.ValueStringGet(2), Constants.Win1252, false)) {
                    return new DoItFeedback(infos.Data, "Fehler beim Erzeugen der Datei.");
                }
                break;

            default:
                return new DoItFeedback(infos.Data, "Export-Format unbekannt.");
        }

        return DoItFeedback.Null();
    }

    #endregion
}