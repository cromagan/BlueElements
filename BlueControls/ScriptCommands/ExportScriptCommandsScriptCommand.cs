// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;
using BlueTable.ClassesStatic;
using static BlueBasics.ClassesStatic.IO;

namespace BlueScript.ScriptCommands;

internal class ExportScriptCommandsScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal, FilterVar];

    public override string Command => "export";

    public override List<string> Constants => ["CSV"];
    public override string Description => "Exportiert die Tabelle im angegeben Format.";

    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;

    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.LongTime;

    public override string Syntax => "Export(Filename, CSV/BDB, AnsichtName, FilterScriptCommand, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (MyTable(scp) is not { IsDisposed: false } myTb) { return DoItFeedback.InternerFehler(); }

        #region  FilterScriptCommand ermitteln (allfi)

        var (allFi, failedReason, needsScriptFix) = FilterScriptCommand.ObjectToFilter(attvar.Attributes, 3, myTb, scp.ScriptName, true);
        if (allFi is null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"FilterScriptCommand-Fehler: {failedReason}", needsScriptFix); }

        #endregion

        var r = allFi.Rows;

        #region  Tabelle prüfen

        if (allFi.Table != myTb) {
            allFi.Dispose();
            return new DoItFeedback("Tabellenfehler!", true);
        }

        allFi.Dispose();

        if (!myTb.LoadTableRows(false, -1)) {
            return new DoItFeedback("Tabelle konnte nicht aktualisiert werden.", true);
        }

        #endregion

        #region  Ansicht ermitteln (cu)

        var tcvc = ColumnViewCollection.ParseAll(myTb);

        var cu = tcvc.GetByKey(attvar.ValueStringGet(2));
        if (string.IsNullOrEmpty(attvar.ValueStringGet(2)) || cu is null) {
            cu = tcvc[0];
        }

        if (cu is null) { return new DoItFeedback("Ansicht-Fehler!", true); }

        #endregion

        #region  Dateinamen ermitteln (filn)

        var filn = attvar.ValueStringGet(0);
        if (!filn.IsValidFilepathAndName()) { return new DoItFeedback("Dateinamen-Fehler!", true); }

        var pf = filn.PathParent();
        var opr = CanWriteInDirectory(pf);
        if (opr.IsFailed) { return new DoItFeedback(opr.FailedReason, true); }

        if (FileExists(filn)) { return new DoItFeedback("Datei existiert bereits.", true); }

        #endregion

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        try {
            switch (attvar.ValueStringGet(1).ToUpperInvariant()) {
                //case "MDB":
                //case "BDB": {
                //        if (myTb is not TableFile tbf) {
                //            return new DoItFeedback("nur bei Dateibasierten Tabellen möglich.", true);
                //        }

                //        var chunks = TableChunk.GenerateNewChunks(tbf, 100, DateTime.UtcNow, false);

                //        if (chunks?.Count != 1 || chunks[0] is not { } mainchunk) { return new DoItFeedback("Fehler beim Erzeugen der Daten.", true); }
                //        mainchunk.Save(filn);
                //        break;
                //    }

                case "CSV":
                    var t = CsvHelper.ExportCsv(myTb, FirstRow.ColumnInternalName, cu.ListOfUsedColumn(), r);
                    if (string.IsNullOrEmpty(t)) { return new DoItFeedback("Fehler beim Erzeugen der Daten.", true); }
                    if (WriteAllText(filn, t, Win1252, false).IsFailed) { return new DoItFeedback("Fehler beim Erzeugen der Datei.", true); }
                    break;

                //case "HTML":
                //case "HTM":
                //    if (!db.Export_HTML(filn, cu, r, false)) { return new DoItFeedback(ld, "Fehler beim Erzeugen der Datei."); }
                //    break;

                default:
                    return new DoItFeedback("Export-Format unbekannt.", true);
            }
        } catch {
            return new DoItFeedback("Allgemeiner Fehler beim Erzeugen der Daten.", true);
        }

        return DoItFeedback.Null();
    }

    #endregion
}