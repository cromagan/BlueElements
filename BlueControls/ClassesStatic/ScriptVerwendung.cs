// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.ControlStrategies;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.PadItems.Abstract;
using BlueControls.PadItems.FunktionsItems_Formular.Abstract;
using BlueScript.Interfaces;
using System.Text;
using System.Text.RegularExpressions;
using static BlueBasics.ClassesStatic.IO;

namespace BlueControls.ClassesStatic;

/// <summary>
/// Erstellt einen Bericht, der für jeden Skript-Befehl alle Verwendungen
/// (Tabellen-Skripte, Spalten-Skripte, Formular-Elemente) auflistet.
/// </summary>
public static class ScriptVerwendung {

    #region Methods

    /// <summary>
    /// Listet jeden Befehl mit allen gefundenen Verwendungen auf;
    /// ohne Verwendung erhält der Befehl den Eintrag „Keine Verwendung".
    /// </summary>
    public static string Report() {
        var quellen = CollectScripts();
        var sb = new StringBuilder();
        var t = new List<string>();

        foreach (var thisc in ScriptCommand.AllMethods.Instances) {
            sb.AppendLine(thisc.KeyName);

            var gefunden = false;
            foreach (var (ort, skript) in quellen) {
                if (skript.IndexOfWord(thisc.KeyName, 0, RegexOptions.IgnoreCase) < 0) { continue; }
                sb.AppendLine(" - " + ort);
                gefunden = true;
            }

            if (!gefunden) {
                t.Add(thisc.KeyName);
                sb.AppendLine(" - Keine Verwendung");
            }
            sb.AppendLine();
        }

        sb.Append("\r#################################");
        sb.Append("\r#################################");
        sb.Append("\r#################################");
        sb.Append("\rBefehle ohne Verwendung:\r");
        sb.Append(string.Join('\r',t));

        return sb.ToString();
    }

    /// <summary>
    /// Schreibt den Bericht in eine temporäre Textdatei und öffnet diese.
    /// </summary>
    public static void Show() {
        var datei = TempFile(string.Empty, "BefehlsVerwendung", "txt");
        _ = WriteAllText(datei, Report(), Win1252, true);
    }

    /// <summary>
    /// Ermittelt das Skript einer Spalte über deren Steuerstrategie,
    /// sodass jede IHasScript-Strategie automatisch erfasst wird.
    /// </summary>
    private static void AddColumnScripts(Table tb, ColumnItem col, List<(string Ort, string Skript)> result) {
        var strategy = ControlStrategy.CreateNew(col.ControlStrategy);
        strategy.ControlStrategyParameter = col.ControlStrategyParameter;

        if (strategy is IHasScript hasScript) {
            foreach (var sd in hasScript.GetAllScripts()) {
                if (sd.Script is not { Length: > 0 } s) { continue; }
                result.Add(("Tabelle " + tb.Caption + "/Spalte " + col.Caption + " (" + strategy.ReadableText() + ")", s));
            }
        }

        strategy.Dispose();
    }

    /// <summary>
    /// Läuft rekursiv durch Seiten/Gruppen, sammelt die Skripte aller IHasScript-PadItems
    /// und stellt referenzierte Formulardateien von ReciverPadItems zur Verarbeitung.
    /// </summary>
    private static void AddPadItemScripts(PadItem thisItem, string container, Queue<ConnectedFormula> offen, HashSet<string> verarbeitet, List<(string Ort, string Skript)> result) {
        if (thisItem is CollectionPadItem collection) {
            var pageName = collection.ReadableText();
            if (string.IsNullOrEmpty(pageName)) { pageName = collection.KeyName; }

            foreach (var child in collection) {
                if (child is not { IsDisposed: false }) { continue; }
                AddPadItemScripts(child, container + "/" + pageName, offen, verarbeitet, result);
            }
            return;
        }

        if (thisItem is ReciverPadItem thisRp) {
            foreach (var datei in thisRp.ReferencedFormulaFiles()) {
                EnqueueFormula(datei, offen, verarbeitet);
            }
        }

        if (thisItem is not IHasScript hasScript) { return; }

        var typ = thisItem.GetType().Name.Replace("PadItem", string.Empty);

        foreach (var sd in hasScript.GetAllScripts()) {
            if (sd.Script is not { Length: > 0 } s) { continue; }
            result.Add((container + "/" + thisItem.KeyName + " (" + typ + ")", s));
        }
    }

    /// <summary>
    /// Baut die Verarbeitungswarteschlange aller Formulare auf:
    /// bereits geladene Instanzen plus *.cfo-Dateien aus den Formular-Ordnern der geladenen Tabellen.
    /// </summary>
    private static Queue<ConnectedFormula> CollectFormulas(HashSet<string> verarbeitet) {
        var offen = new Queue<ConnectedFormula>();

        foreach (var thisCf in ConnectedFormula.AllInstances()) {
            if (thisCf is not { IsDisposed: false }) { continue; }
            if (thisCf.Filename.Length > 0 && !verarbeitet.Add(thisCf.Filename)) { continue; }
            offen.Enqueue(thisCf);
        }

        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is not { IsDisposed: false } tb) { continue; }

            EnqueueFormula(tb.FormulaFileName(), offen, verarbeitet);
            EnqueueFolder(tb.AssetFolderWhole(), offen, verarbeitet);
            EnqueueFolder(tb.DefaultFormulaPath(), offen, verarbeitet);
        }

        return offen;
    }

    /// <summary>
    /// Sammelt Ort und Skripttext aller aktuell geladenen Skripte:
    /// Tabellen-Skripte, skriptführende Spalten-Steuerstrategien sowie alle Formulare —
    /// geladene, Dateien der Tabellen-Ordner und deren verschachtelte Unterformulare.
    /// </summary>
    private static List<(string Ort, string Skript)> CollectScripts() {
        List<(string Ort, string Skript)> result = [];

        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is not { IsDisposed: false } tb) { continue; }

            foreach (var sd in tb.GetAllScripts()) {
                if (sd.Script is not { Length: > 0 } s) { continue; }
                result.Add(("Tabelle " + tb.Caption + "/" + sd.KeyName, s));
            }

            foreach (var thisColumn in tb.Column) {
                if (thisColumn is not { IsDisposed: false } col) { continue; }
                AddColumnScripts(tb, col, result);
            }
        }

        var verarbeitet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var offen = CollectFormulas(verarbeitet);

        while (offen.Count > 0) {
            var thisCf = offen.Dequeue();
            if (thisCf is not { IsDisposed: false }) { continue; }

            var cfName = thisCf.ReadableText();
            if (string.IsNullOrEmpty(cfName)) { cfName = "(unbenannt)"; }

            foreach (var page in thisCf.Pages) {
                AddPadItemScripts(page, "Formular " + cfName, offen, verarbeitet, result);
            }
        }

        return result;
    }

    /// <summary>
    /// Lädt eine Formulardatei und stellt sie zur Verarbeitung, falls sie existiert und neu ist.
    /// </summary>
    private static void EnqueueFormula(string? datei, Queue<ConnectedFormula> offen, HashSet<string> verarbeitet) {
        if (datei is not { Length: > 0 }) { return; }
        if (!FileExists(datei)) { return; }

        var cf = ConnectedFormula.Get(datei);
        if (cf is not { IsDisposed: false }) { return; }
        if (cf.Filename.Length > 0 && !verarbeitet.Add(cf.Filename)) { return; }

        offen.Enqueue(cf);
    }

    /// <summary>
    /// Stellt alle *.cfo-Dateien eines Ordners zur Verarbeitung.
    /// </summary>
    private static void EnqueueFolder(string? ordner, Queue<ConnectedFormula> offen, HashSet<string> verarbeitet) {
        if (string.IsNullOrEmpty(ordner)) { return; }
        foreach (var datei in GetFiles(ordner, "*.cfo", System.IO.SearchOption.TopDirectoryOnly)) {
            EnqueueFormula(datei, offen, verarbeitet);
        }
    }

    #endregion
}