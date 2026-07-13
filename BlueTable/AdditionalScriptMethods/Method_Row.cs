// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.AdditionalScriptVariables;
using System.Diagnostics;

namespace BlueTable.AdditionalScriptMethods;

public static class Method_Row_Extension {

    #region Methods

    public static RowItem? ValueRowGet(this SplittedAttributesFeedback attvar, int varno) {
        if (varno < 0 || varno >= attvar.Attributes.Count) { return null; }

        return attvar.Attributes[varno] is VariableRowItem vro ? vro.RowItem : null;
    }

    #endregion
}

public class Method_Row : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [FloatVal, FilterVar];

    public override string Command => "row";

    public override string Description => "Sucht eine Zeile mittels dem gegebenen Filter.\r\n" +
                                              "Wird keine Zeile gefunden, wird eine neue Zeile erstellt.\r\n" +
                                          "Ist sie bereits mehrfach vorhanden, werden diese zusammengefasst (maximal 5!).\r\n" +
                                          "Kann keine neue Zeile erstellt werden, wird das Programm unterbrochen.\r\n" +
        "Mit AgeInDay kann angebeben werden, ab welchen Alter eine gefundene Zeile invalidiert werden soll.";

    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;

    // Manipulates User deswegen, weil eine neue Zeile evtl. andere Rechte hat und dann stören kann.
    public override MethodType MethodLevel => MethodType.Sub;

    public override string Returns => VariableRowItem.ShortName_Variable;

    public override string Syntax => "Row(AgeInDays, Filter, ...)";

    #endregion

    #region Methods

    public static DoItFeedback RowToObjectFeedback(RowItem? row) => new(new VariableRowItem(row));

    public static DoItFeedback UniqueRow(FilterCollection fic, double invalidateinDays, string coment, ScriptProperties scp) {
        if (invalidateinDays < 0.01) { return new DoItFeedback("Intervall zu kurz.", true); }

        if (fic.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Filter", true); }
        if (tb.Column.SysRowState is not { IsDisposed: false } srs) { return new DoItFeedback($"Zeilen-Status-Spalte in '{tb.KeyName}' nicht gefunden", true); }

        foreach (var thisFi in fic) {
            if (thisFi.Column is not { IsDisposed: false } c) {
                return new DoItFeedback("Fehler im Filter, Spalte ungültig", true);
            }

            //if (thisFi.FilterType is not FilterType.Istgleich and not FilterType.Istgleich_GroßKleinEgal) {
            //    return new DoItFeedback("Fehler im Filter, nur 'is' ist erlaubt", true);
            //}

            //if (thisFi.SearchValue.Count != 1) {
            //    return new DoItFeedback("Fehler im Filter, ein einzelner Suchwert wird benötigt", true);
            //}

            if (FilterCollection.InitValue(c, true, false, [.. fic]) is not { } l) {
                return new DoItFeedback("Fehler im Filter, dieser Filtertyp kann nicht initialisiert werden.", true);
            }

            if (thisFi.SearchValue[0] != l) {
                return new DoItFeedback($"Fehler im Filter:\r\nWert '{thisFi.SearchValue[0]}' kann nicht gesetzt werden.\r\nVorgeschlager Wert: '{l}'\r\nSpalte: {thisFi.Column.Caption}", true);
            }
        }

        Develop.Message(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\Row-Befehl: {fic.ReadableText()}", scp.Stufe);

        RowItem? newrow = null;

        if (scp.ProduktivPhase) {
            var t = Stopwatch.StartNew();
            string message;
            do {
                var nr = RowCollection.UniqueRow(fic, coment);
                message = nr.FailedReason;
                newrow = nr.Value as RowItem;
                if (!nr.IsRetryable) { break; }
                if (t.Elapsed.TotalMinutes > 5) { break; }

                Generic.Pause(5, false);
            } while (true);

            t.Stop();
            if (!string.IsNullOrEmpty(message)) { return new DoItFeedback(message, false); }
        } else {
            if (fic.Rows.Count != 1) { return DoItFeedback.TestModusInaktiv(); }
            newrow = fic.Rows[0];
        }

        if (newrow is { IsDisposed: false } r) {
            var v = r.CellGetDateTime(srs);
            if (DateTime.UtcNow.Subtract(v).TotalDays >= invalidateinDays) {
                if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }
                var f = Table.IsCellEditable(srs, r, fic.ChunkVal, false);
                if (!string.IsNullOrEmpty(f)) { return new DoItFeedback($"Tabellensperre: {f}", false); }
                r.InvalidateRowState(coment);
            } else {
                Develop.Message(ErrorType.DevelopInfo, null, scp.MainInfo, ImageCode.Skript, $"Parsen: {scp.Chain}\\Kein Zeilenupdate ({r.ReadableText()}, {r.Table?.Caption ?? "?"}), da Zeile aktuell ist.", scp.Stufe);
            }
        } else {
            return new DoItFeedback("Zeile konnte nicht angelegt werden", false);
        }

        return RowToObjectFeedback(newrow);
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var myTb = MyTable(scp);
        var cap = myTb?.Caption ?? "Unbekannt";

        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 1, myTb, scp.ScriptName, true);
        if (allFi is null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix); }

        var d = attvar.ValueNumGet(0);

        var fb = UniqueRow(allFi, d, $"Skript-Befehl: 'Row' der Tabelle {cap}, Skript {scp.ScriptName}", scp);
        allFi.Dispose();

        return fb;
    }

    #endregion
}