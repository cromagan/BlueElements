// Draft: Soll als ScriptCommand (BildzeichenListe\ScriptCommands) implementiert werden.
// ImportBOM: CS03 öffnen, Stückliste als Text exportieren und in die Spalten der Tabelle schreiben.

using BildzeichenListe.Classes;
using BlueBasics;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptCommands;
using BlueScript.ScriptVariables;
using System.Collections.Generic;

namespace BildzeichenListe.AdditionalScriptMethods;

public class ImportBOMScriptCommand : TableGenericScriptCommand {

    #region Fields

    /// <summary>
    /// Überschrift im CS03-Text-Export → Schlüssel zum Auslesen
    /// </summary>
    private static readonly Dictionary<string, string> Cs03Spalten = new(StringComparer.OrdinalIgnoreCase) {
        { "Pos.", "POSNR" },
        { "Sortbg.", "SORTF" },
        { "PosT", "POSTP" },
        { "Materialnummer", "IDNRK" },
        { "Benennung", "BENENNUNG" },
        { "Menge", "MENGE" },
        { "ME", "MEINS" }
    };

    #endregion

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal, StringVal, StringVal, StringVal, StringVal, StringVal];
    public override string Command => "importbom";

    public override string Description => "Öffnet SAP - falls möglich - und holt die Materialstückliste (CS03).\r\n" +
                                          "Die Positionen werden in die angegebenen Spalten der Tabelle geschrieben.\r\n" +
                                          "Failed wird gesetzt, wenn die Routine scheitert.";

    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.ManipulatesUser;

    public override string Syntax => "ImportBOM(Materialnummer, Werk, Pos, Sortbegr, PTp, Komponente, Menge, Einheit)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        var nummer = attvar.ValueStringGet(0);
        if (!nummer.IsKronesNummer()) {
            return new DoItFeedback($"'{nummer}' ist keine Krones-Materialnummer", true);
        }

        var werk = attvar.ValueStringGet(1);

        var col_pos = Column(scp, attvar, 2);
        if (col_pos is not { IsDisposed: false }) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.Name(2), true); }

        var col_sortbegr = Column(scp, attvar, 3);
        if (col_sortbegr is not { IsDisposed: false }) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.Name(3), true); }

        var col_ptp = Column(scp, attvar, 4);
        if (col_ptp is not { IsDisposed: false }) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.Name(4), true); }

        var col_komponente = Column(scp, attvar, 5);
        if (col_komponente is not { IsDisposed: false }) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.Name(5), true); }

        var col_menge = Column(scp, attvar, 6);
        if (col_menge is not { IsDisposed: false }) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.Name(6), true); }

        var col_einheit = Column(scp, attvar, 7);
        if (col_einheit is not { IsDisposed: false }) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.Name(7), true); }

        if (MyTable(scp) is not { IsDisposed: false } tb) { return new DoItFeedback($"Import nur aus einer Datenbank heraus möglich.", true); }

        if (BlockedRow(scp) is not null) { return new DoItFeedback($"Import in einem Zeilenskript nicht möglich.", false); }

        var f = tb.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) { return new DoItFeedback($"Tabellensperre: {f}", false); }

        #region CS03 öffnen und als Text exportieren

        var sap = SAPBZ.GetSAP(false);
        if (!sap.ConnectionOk) { return DoItFeedback.Null(); }

        _ = sap.StartTransaktion("CS03");
        sap.TextBox("ctxtRC29L-MATNR").Text = nummer;
        sap.TextBox("ctxtRC29L-WERKS").Text = werk;
        sap.TextBox("ctxtRC29L-STLAN").Text = "1"; // TODO: Verwendung ggf. als Parameter
        sap.TextBox("ctxtRC29L-STALT").Text = string.Empty;
        sap.TextBox("ctxtRC29L-AENNR").Text = string.Empty;
        sap.TextBox("txtRC29L-EMENG").Text = string.Empty;
        _ = sap.ButtonOkOrWeiter();

        // TODO: Popup bei mehreren Alternativen abfangen
        // TODO: Menüpfad für Listen-Ausgabe am realen System verifizieren
        sap.MenuClick("menu[0]/menu[1]/menu[2]");
        var txt = sap.ExportOverFilesystem();

        if (string.IsNullOrEmpty(txt)) {
            return new DoItFeedback("Stückliste konnte von " + nummer + " nicht gelesen werden!", true);
        }

        #endregion

        var zeilen = Cs03TextZeilen(txt);

        if (zeilen.Count == 0) {
            return new DoItFeedback("Stückliste konnte von " + nummer + " nicht gelesen werden!", true);
        }

        // Import erzeugt viele GenerateAndAdd- und CellSet-Aufrufe; ohne
        // Suppression feuert jeder sofort Events und baut die UI mehrfach auf.
        tb.SuppressEvents();
        try {
            foreach (var werte in zeilen) {
                // TODO: Zeilen ohne Key anlegen - ggf. andere GenerateAndAdd-Überladung nutzen
                if (tb.Row.GenerateAndAdd(string.Empty, "ImportBOM") is not { IsDisposed: false } r) {
                    return new DoItFeedback("Zeile konnte nicht erstellt werden", true);
                }

                if (werte.TryGetValue("POSNR", out var v)) { r.CellSet(col_pos, v, "ImportBOM"); }
                if (werte.TryGetValue("SORTF", out v)) { r.CellSet(col_sortbegr, v, "ImportBOM"); }
                if (werte.TryGetValue("POSTP", out v)) { r.CellSet(col_ptp, v, "ImportBOM"); }
                if (werte.TryGetValue("IDNRK", out v)) { r.CellSet(col_komponente, v, "ImportBOM"); }
                if (werte.TryGetValue("MENGE", out v)) { r.CellSet(col_menge, v, "ImportBOM"); }
                if (werte.TryGetValue("MEINS", out v)) { r.CellSet(col_einheit, v, "ImportBOM"); }
            }
        } finally {
            tb.ResumeEvents();
        }

        return DoItFeedback.Null();
    }

    /// <summary>
    /// Zerlegt den CS03-Text-Export (feste Spaltenbreiten) in Zeilen: Schlüssel → Wert.
    /// </summary>
    private static List<Dictionary<string, string>> Cs03TextZeilen(string txt) {
        var erg = new List<Dictionary<string, string>>();
        var lines = txt.SplitAndCutByCr();

        #region Trennzeile suchen und Spaltenbereiche aus den '-'-Blöcken ermitteln

        List<(int Start, int Ende)> bereiche = [];
        var trenn = -1;

        for (var z = 0; z < lines.Length; z++) {
            if (lines[z].Length < 5 || !NurStriche(lines[z])) { continue; }

            bereiche.Clear();
            var inStrich = false;
            var start = 0;

            for (var p = 0; p < lines[z].Length; p++) {
                var isStrich = lines[z][p] == '-';
                if (isStrich && !inStrich) {
                    start = p;
                    inStrich = true;
                } else if (!isStrich && inStrich) {
                    bereiche.Add((start, p));
                    inStrich = false;
                }
            }
            if (inStrich) { bereiche.Add((start, lines[z].Length)); }

            if (bereiche.Count > 2) {
                trenn = z;
                break;
            }
        }

        if (trenn < 1 || bereiche.Count == 0) { return erg; }

        #endregion

        #region Überschriften den Schlüsseln zuordnen

        var kopf = lines[trenn - 1];
        var spalten = new string?[bereiche.Count];

        for (var i = 0; i < bereiche.Count; i++) {
            var (start, ende) = bereiche[i];
            if (kopf.Length <= start) { continue; }
            var t = kopf[start..Math.Min(ende, kopf.Length)].Trim();
            if (t.Length > 0 && Cs03Spalten.TryGetValue(t, out var sname)) { spalten[i] = sname; }
        }

        #endregion

        for (var z = trenn + 1; z < lines.Length; z++) {
            var zeile = lines[z];
            if (string.IsNullOrWhiteSpace(zeile)) { continue; }
            if (NurStriche(zeile)) { break; } // Abschluss-Trennzeile

            Dictionary<string, string>? werte = null;

            for (var i = 0; i < bereiche.Count; i++) {
                var sname = spalten[i];
                if (sname == null) { continue; }

                var (start, ende) = bereiche[i];
                if (zeile.Length <= start) { break; }

                var v = zeile[start..Math.Min(ende, zeile.Length)].Trim();
                if (v.Length > 0) {
                    werte ??= [];
                    werte[sname] = v;
                }
            }

            if (werte != null) { erg.Add(werte); }
        }

        return erg;
    }

    private static bool NurStriche(string zeile) {
        foreach (var c in zeile) {
            if (c is not ('-' or ' ')) { return false; }
        }
        return true;
    }

    #endregion
}
