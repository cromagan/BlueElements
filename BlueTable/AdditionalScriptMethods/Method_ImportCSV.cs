// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueTable.AdditionalScriptMethods;

internal class Method_ImportCsv : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "importcsv";
    public override string Description => "Importiert den Inhalt, der als CSV vorliegen muss, in die Tabelle.";
    public override MethodType MethodLevel => MethodType.LongTime;
    public override string Syntax => "ImportCSV(CSVText, Separator);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (MyTable(scp) is not { IsDisposed: false } tb) { return new DoItFeedback($"Import nur aus einer Datenbank heraus möglich.", true, ld); }

        if (BlockedRow(scp) is not null) { return new DoItFeedback($"Import in einem Zeilenskript nicht möglich.", false, ld); }

        var txt = attvar.ValueStringGet(0);
        var sep = attvar.ValueStringGet(1);

        var f = tb.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return new DoItFeedback($"Tabellensperre: {f}", false, ld); }

        

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        var sx = tb.ImportCsv(txt, true, sep, false, false);

        return string.IsNullOrEmpty(sx) ? DoItFeedback.Null() : new DoItFeedback(sx, true, ld);
    }

    #endregion
}