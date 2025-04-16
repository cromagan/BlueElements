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
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript.Enums;
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

    public override List<string> Constants => ["CSV", "BDB"];
    public override string Description => "Exportiert die Datenbank im angegeben Format. Achtung, bei BDB wird immer die gesamte Datenbank exportiert und die angegebenen Attribute ingnoriert.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 1;

    public override MethodType MethodType => MethodType.Database;

    public override bool MustUseReturnValue => false;

    public string NiceTextForUser => "Die gefilterten Zeilen ins Dateisystem exportieren";

    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "Export(Filename, CSV/BDB, AnsichtName, Filter, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var myDb = MyDatabase(scp);

        #region  Filter ermitteln (allfi)

        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 3, myDb, scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }

        #endregion

        var r = allFi.Rows;

        #region  Datenbank ermitteln (db)

        if (myDb == null || allFi.Database != myDb) {
            allFi.Dispose();
            return new DoItFeedback("Datenbankfehler!", true, ld);
        }

        allFi.Dispose();

        if (!myDb.BeSureAllDataLoaded(-1)) {
            return new DoItFeedback("Datenbank konnte nicht aktualisiert werden.", true, ld);
        }

        #endregion

        #region  Ansicht ermitteln (cu)

        var tcvc = ColumnViewCollection.ParseAll(myDb);

        var cu = tcvc.Get(attvar.ValueStringGet(2));
        if (string.IsNullOrEmpty(attvar.ValueStringGet(2)) || cu == null) {
            cu = tcvc[0];
        }

        if (cu == null) { return new DoItFeedback("Ansicht-Fehler!", true, ld); }

        #endregion

        #region  Dateinamen ermitteln (filn)

        var filn = attvar.ValueStringGet(0);
        if (string.IsNullOrEmpty(filn)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }
        if (!filn.IsFormat(FormatHolder.FilepathAndName)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }

        var pf = filn.PathParent();
        if (string.IsNullOrEmpty(pf)) { return new DoItFeedback("Dateinamen-Fehler!", true, ld); }
        if (!Directory.Exists(pf)) { return new DoItFeedback("Verzeichniss existiert nicht", true, ld); }
        if (!IO.CanWriteInDirectory(pf)) { return new DoItFeedback("Keine Schreibrechte im Zielverzeichniss.", true, ld); }

        if (File.Exists(filn)) { return new DoItFeedback("Datei existiert bereits.", true, ld); }

        #endregion

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        try {
            switch (attvar.ValueStringGet(1).ToUpperInvariant()) {
                case "MDB":
                case "BDB": {
                        var chunks = DatabaseChunk.GenerateNewChunks(myDb, 100, myDb.FileStateUtcDate, false);

                        if (chunks == null || chunks.Count != 1 || chunks[0] is not { } mainchunk) { return new DoItFeedback("Fehler beim Erzeugen der Daten.", true, ld); }
                        _ = mainchunk.Save(filn, 100);
                        break;
                    }

                case "CSV":
                    var t = myDb.Export_CSV(FirstRow.ColumnInternalName, cu.ListOfUsedColumn(), r);
                    if (string.IsNullOrEmpty(t)) { return new DoItFeedback("Fehler beim Erzeugen der Daten.", true, ld); }
                    if (!IO.WriteAllText(filn, t, BlueBasics.Constants.Win1252, false)) { return new DoItFeedback("Fehler beim Erzeugen der Datei.", true, ld); }
                    break;

                //case "HTML":
                //case "HTM":
                //    if (!db.Export_HTML(filn, cu, r, false)) { return new DoItFeedback(ld, "Fehler beim Erzeugen der Datei."); }
                //    break;

                default:
                    return new DoItFeedback("Export-Format unbekannt.", true, ld);
            }
        } catch {
            return new DoItFeedback("Allgemeiner Fehler beim Erzeugen der Daten.", true, ld);
        }

        return DoItFeedback.Null();
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => args[0] + "," + args[1] + "," + args[2] + "," + filterarg;

    #endregion
}