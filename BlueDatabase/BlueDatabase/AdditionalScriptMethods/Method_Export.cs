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
using BlueDatabase.Enums;
using BlueScript.Enums;
using BlueDatabase.Interfaces;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.IO;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
internal class Method_Export : Method_Database, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal, FilterVar];

    public List<List<string>> ArgsForButton => [StringVal, StringVal, StringVal];

    public List<string> ArgsForButtonDescription => ["Dateiname", "Format", "Ansichtname"];
    public ButtonArgs ClickableWhen => ButtonArgs.Eine_oder_mehr_Zeilen;

    public override string Command => "export";

    public override string Description => "Exportiert die Datenbank im angegeben Format. Achtung, bei BDB wird immer die gesamte Datenbank exportiert und die angegebenen Attribute ingnoriert.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 1;

    public override MethodType MethodType => MethodType.Database | MethodType.IO;

    public override bool MustUseReturnValue => false;

    public string NiceTextForUser => "Die gefilterten Zeilen ins Dateisystem exportieren";

    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "Export(Filename, CSV/BDB, AnsichtName, Filter, ...);";

    #endregion

    #region Methods

   public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {

        #region  Filter ermitteln (allfi)

        using var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 3);

        if (allFi is null || allFi.Count == 0) { return new DoItFeedback(ld, "Fehler im Filter"); }

        #endregion

        #region  Datebank ermitteln (db)

        var db = MyDatabase(scp);

        if (db == null || allFi.Database != db) { return new DoItFeedback(ld, "Datenbankfehler!"); }

        #endregion

        #region  Zeilen Ermitteln (r)

        var r = allFi.Rows;

        #endregion

        #region  Ansicht ermitteln (cu)

        var cu = db.ColumnArrangements.Get(attvar.ValueStringGet(2));
        if (string.IsNullOrEmpty(attvar.ValueStringGet(2)) || cu == null) {
            cu = db.ColumnArrangements[0];
        }

        if (cu == null) { return new DoItFeedback(ld, "Ansicht-Fehler!"); }

        #endregion

        #region  Dateinamen ermitteln (filn)

        var filn = attvar.ValueStringGet(0);
        if (string.IsNullOrEmpty(filn)) { return new DoItFeedback(ld, "Dateinamen-Fehler!"); }
        if (!filn.IsFormat(FormatHolder.FilepathAndName)) { return new DoItFeedback(ld, "Dateinamen-Fehler!"); }

        var pf = filn.PathParent();
        if (string.IsNullOrEmpty(pf)) { return new DoItFeedback(ld, "Dateinamen-Fehler!"); }
        if (!Directory.Exists(pf)) { return new DoItFeedback(ld, "Verzeichniss existiert nicht"); }
        if (!IO.CanWriteInDirectory(pf)) { return new DoItFeedback(ld, "Keine Schreibrechte im Zielverzeichniss."); }

        if (File.Exists(filn)) { return new DoItFeedback(ld, "Datei existiert bereits."); }

        #endregion

        if (!scp.ProduktivPhase) { return new DoItFeedback(ld, "Export im Testmodus deaktiviert."); }

        try {
            switch (attvar.ValueStringGet(1).ToUpperInvariant()) {
                case "MDB":
                case "BDB": {
                        var bytes = Database.ToListOfByte(db, 100, db.FileStateUTCDate);

                        if (bytes == null) { return new DoItFeedback(ld, "Fehler beim Erzeugen der Daten."); }

                        using FileStream x = new(filn, FileMode.Create, FileAccess.Write, FileShare.None);
                        x.Write(bytes.ToArray(), 0, bytes.ToArray().Length);
                        x.Flush();
                        x.Close();
                        break;
                    }

                case "CSV":
                    var t = db.Export_CSV(FirstRow.ColumnInternalName, cu, r);
                    if (string.IsNullOrEmpty(t)) { return new DoItFeedback(ld, "Fehler beim Erzeugen der Daten."); }
                    if (!IO.WriteAllText(filn, t, Constants.Win1252, false)) { return new DoItFeedback(ld, "Fehler beim Erzeugen der Datei."); }
                    break;

                //case "HTML":
                //case "HTM":
                //    if (!db.Export_HTML(filn, cu, r, false)) { return new DoItFeedback(ld, "Fehler beim Erzeugen der Datei."); }
                //    break;

                default:
                    return new DoItFeedback(ld, "Export-Format unbekannt.");
            }
        } catch {
            return new DoItFeedback(ld, "Allgemeiner Fehler beim Erzeugen der Daten.");
        }

        return DoItFeedback.Null();
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => args[0] + "," + args[1] + "," + args[2] + "," + filterarg;

    #endregion
}