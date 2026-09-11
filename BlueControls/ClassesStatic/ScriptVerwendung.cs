// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.ControlStrategies;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.PadItems.Abstract;
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
    /// Läuft rekursiv durch Seiten/Gruppen und sammelt die Skripte aller IHasScript-PadItems.
    /// </summary>
    private static void AddPadItemScripts(PadItem thisItem, string container, List<(string Ort, string Skript)> result) {
        if (thisItem is CollectionPadItem collection) {
            var pageName = collection.ReadableText();
            if (string.IsNullOrEmpty(pageName)) { pageName = collection.KeyName; }

            foreach (var child in collection) {
                if (child is not { IsDisposed: false }) { continue; }
                AddPadItemScripts(child, container + "/" + pageName, result);
            }
            return;
        }

        if (thisItem is not IHasScript hasScript) { return; }

        var typ = thisItem.GetType().Name.Replace("PadItem", string.Empty);

        foreach (var sd in hasScript.GetAllScripts()) {
            if (sd.Script is not { Length: > 0 } s) { continue; }
            result.Add((container + "/" + thisItem.KeyName + " (" + typ + ")", s));
        }
    }

    /// <summary>
    /// Sammelt Ort und Skripttext aller aktuell geladenen Skripte:
    /// Tabellen-Skripte, skriptführende Spalten-Steuerstrategien und Formular-Elemente.
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

        foreach (var thisCf in ConnectedFormula.AllInstances()) {
            var cfName = thisCf.ReadableText();
            if (string.IsNullOrEmpty(cfName)) { cfName = "(unbenannt)"; }

            foreach (var page in thisCf.Pages) {
                AddPadItemScripts(page, "Formular " + cfName, result);
            }
        }

        return result;
    }

    #endregion
}