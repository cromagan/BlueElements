// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueScript.Classes;

namespace BlueScript.ScriptCommands;

public class FilterScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [TableVar, StringVal, StringVal, StringVal];
    public override string Command => "filter";

    public override List<string> Constants => ["IS", "ISNOT", "INSTR", "STARTSWITH", "BETWEEN"];

    public override string Description => "Erstellt einen FilterScriptCommand, der für andere Befehle (z.B. LookupFilter) verwendet werden kann.\r\n" +
                                          "Aktuell werden nur die FilterTypen 'is', 'isnot', 'startswith', 'instr' und 'between' unterstützt.\r\n" +
                                          "Bei diesem FilterScriptCommand wird die Groß/Kleinschreibung ignoriert.\r\n" +
                                          "Bei Between müssen die Werte so Angegeben werden: 50|100";

    public override LastArgMinCountTypeScriptCommand LastArgMinCount => LastArgMinCountTypeScriptCommand.MinOnce;
    public override bool MustUseReturnValue => true;
    public override string Returns => FilterScriptVariable.ShortName_Variable;
    public override string Syntax => "FilterScriptCommand(Table, Spalte, Filtertyp, Wert)";

    #endregion

    #region Methods

    public static (FilterCollection? allFi, string failedReason, bool needsScriptFix) ObjectToFilter(IReadOnlyList<ScriptVariable> attributes, int ab, Table? sourcetable, string user, bool must) {
        var allFi = new List<FilterItem>();

        for (var z = ab; z < attributes.Count; z++) {
            if (attributes[z] is not FilterScriptVariable fi) { return (null, $"Attribut {z + 1} ist kein FilterScriptCommand.", true); } // new DoItFeedback(infos.LogData, s, "Kein FilterScriptCommand übergeben.");

            if (fi.ValueFilterItem is not { } fii) { return (null, $"Attribut {z + 1} enthält keinen FilterScriptCommand.", true); }

            if (fii.Column?.Table is { IsDisposed: false } tb) {
                fii.Column.AddSystemInfo("Value Used in Script-FilterScriptCommand", sourcetable ?? tb, user);

                if (tb.IsDisposed) { return (null, "Tabellenfehler!", false); }

                //if (tb != sourcetable && !tb.AreScriptsExecutable()) { return (null, $"In der Tabelle '{tb.Caption}' sind die Skripte defekt", false); }
            }

            if (!fii.IsOk()) { return (null, $"Der FilterScriptCommand des Attributes {z + 1} ist fehlerhaft.", true); }// new DoItFeedback(infos.LogData, s, "FilterScriptCommand fehlerhaft"); }

            if (z > ab) {
                if (fii.Table != allFi[0].Table) { return (null, "FilterScriptCommand über verschiedene Tabellen wird nicht unterstützt.", true); }// new DoItFeedback(infos.LogData, s, "FilterScriptCommand über verschiedene Tabellen wird nicht unterstützt."); }
            }

            allFi.Add(fii);
        }

        if (allFi.Count < 1) {
            if (!must) { return (null, string.Empty, false); }
            return (null, "Fehler bei der Filtererstellung.", true);
        }

        var f = new FilterCollection(allFi[0].Table, "method_filter");
        f.AddIfNotExists(allFi);

        if (!string.IsNullOrEmpty(f.ErrorReason())) { return (null, f.ErrorReason(), true); }

        return (f, string.Empty, false);
    }

    public static FilterType StringToFilterType(string type) {
        switch (type.ToLowerInvariant()) {
            case "is":
                return FilterType.Istgleich_GroßKleinEgal;

            case "isnot":
                return FilterType.Ungleich_MultiRowIgnorieren_GroßKleinEgal;

            case "instr":
                return FilterType.Instr_GroßKleinEgal;

            case "startswith":
                return FilterType.BeginntMit_GroßKleinEgal;

            case "between":
                return FilterType.Between;

            default:
                return FilterType.AlwaysFalse;
        }
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (attvar.Attributes[0] is not TableScriptVariable vtb || vtb.ValueTable is not { IsDisposed: false } tb) { return new DoItFeedback("Tabelle nicht vorhanden", true); }

        //if (tb != myDb && !tb.AreScriptsExecutable()) { return new DoItFeedback($"In der Tabelle '{attvar.ValueStringGet(0)}' sind die Skripte defekt", false); }

        #region Spalte ermitteln

        var filterColumn = tb.Column[attvar.ValueStringGet(1)];
        if (filterColumn is null) { return new DoItFeedback($"Spalte '{attvar.ValueStringGet(1)}' in Tabelle '{tb.Caption}' nicht gefunden", true); }

        #endregion

        #region Typ ermitteln

        var filtertype = StringToFilterType(attvar.ValueStringGet(2));

        if (filtertype == FilterType.AlwaysFalse) {
            return new DoItFeedback("Filtertype unbekannt: " + attvar.ValueStringGet(2), true);
        }

        #endregion

        var fii = new FilterItem(filterColumn, filtertype, attvar.ValueStringGet(3));

        if (!fii.IsOk()) {
            return new DoItFeedback("FilterScriptCommand konnte nicht erstellt werden: '" + fii.ErrorReason() + "'", true);
        }

        filterColumn.AddSystemInfo("FilterScriptCommand in Script", tb, scp.ScriptName);

        return new DoItFeedback(new FilterScriptVariable(fii));
    }

    #endregion
}