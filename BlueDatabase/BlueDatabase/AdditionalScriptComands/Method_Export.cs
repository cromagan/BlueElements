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

using System.Collections.Generic;
using System.IO;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

internal class Method_Export : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, StringVal, StringVal, FilterVar };
    public override string Description => "Exportiert die Datenbank im angegeben Format. Achtung, bei MDB wird immer die gesamte Datenbank exportiert und die angegebenen Attribute ingnoriert.";
    public override bool EndlessArgs => true;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Database | MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => string.Empty;

    public override string StartSequence => "(";
    public override string Syntax => "Export(Filename, HTML/CSV/MDB, AnsichtName, Filter, ...);";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "export" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        #region  Filter ermitteln (allfi)

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 3);

        if (allFi is null || allFi.Count == 0) { return new DoItFeedback(infos.Data, "Fehler im Filter"); }

        #endregion

        #region  Datebank ermitteln (db)

        var db = MyDatabase(s.Variables);

        if (db == null || allFi[0].Database != db) { return new DoItFeedback(infos.Data, "Datenbankfehler!"); }

        #endregion

        #region  Zeilen Ermitteln (r)

        var r = db.Row.CalculateSortedRows(allFi, null, null, null);

        #endregion

        #region  Ansicht ermitteln (cu)

        var cu = db.ColumnArrangements.Get(((VariableString)attvar.Attributes[2]).ValueString);
        if (string.IsNullOrEmpty(((VariableString)attvar.Attributes[2]).ValueString) || cu == null) {
            cu = db.ColumnArrangements?[0];
        }

        if (cu == null) { return new DoItFeedback(infos.Data, "Ansicht-Fehler!"); }

        #endregion

        #region  Dateinamen ermitteln (filn)

        var filn = ((VariableString)attvar.Attributes[0]).ValueString;
        if (string.IsNullOrEmpty(filn)) { return new DoItFeedback(infos.Data, "Dateinamen-Fehler!"); }
        if (!filn.IsFormat(FormatHolder.FilepathAndName)) { return new DoItFeedback(infos.Data, "Dateinamen-Fehler!"); }

        var pf = filn.PathParent();
        if (string.IsNullOrEmpty(pf)) { return new DoItFeedback(infos.Data, "Dateinamen-Fehler!"); }
        if (!Directory.Exists(pf)) { return new DoItFeedback(infos.Data, "Verzeichniss existiert nicht"); }
        if (!IO.CanWriteInDirectory(pf)) { return new DoItFeedback(infos.Data, "Keine Schreibrechte im Zielverzeichniss."); }

        if (File.Exists(filn)) { return new DoItFeedback(infos.Data, "Datei existiert bereits."); }

        #endregion

        if (!s.ChangeValues) { return new DoItFeedback(infos.Data, "Export im Testmodus deaktiviert."); }

        switch (((VariableString)attvar.Attributes[1]).ValueString.ToUpper()) {
            case "MDB": {
                    var bytes = Database.ToListOfByte(db, null, 100);

                    if (bytes == null) {
                        return new DoItFeedback(infos.Data, "Fehler beim Erzeugen der Daten.");
                    }

                    using FileStream x = new(filn, FileMode.Create, FileAccess.Write, FileShare.None);
                    x.Write(bytes.ToArray(), 0, bytes.ToArray().Length);
                    x.Flush();
                    x.Close();
                    break;
                }

            case "CSV":
                var t = db.Export_CSV(FirstRow.ColumnInternalName, cu, r);
                IO.WriteAllText(filn, t, Constants.Win1252, false);
                break;

            case "HTML":
            case "HTM":
                db.Export_HTML(filn, cu, r, false);
                break;

            default:
                return new DoItFeedback(infos.Data, "Export-Format unbekannt.");
        }

        return DoItFeedback.Null();
    }

    #endregion
}