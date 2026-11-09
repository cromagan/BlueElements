// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls.ConnectedFormula;
using BlueControls.PadItems.Abstract;
using BlueControls.PadItems.FunktionsItems_Formular.Abstract;
using BlueScript.Interfaces;
using System.Text.RegularExpressions;
using static BlueBasics.ClassesStatic.IO;

namespace BlueControls.Forms;

/// <summary>
/// Zeigt als schreibgeschütztes CreativePad, welche Tabellen und Formulare voneinander abhängen:
/// Standard-Formulare, Formular-Elemente, Unterformulare, LinkedCells und Skript-Verweise.
/// Systemweit nur ein Fenster (IUniqueWindow, Object = null).
/// </summary>
public sealed partial class DependencyOverviewForm : FormWithStatusBar, IUniqueWindow {

    #region Fields

    private const float AbstandFormularElement = 80;

    private const float AbstandElementTabelle = 100;

    private const float AbstandSkriptSpalte = 60;

    private const float AbstandTabelleSkript = 70;

    private const float BoxBreite = 240;

    private const float BoxHöhe = 46;

    private const float GapGruppe = 70;

    private const float GapX = 24;

    private const float RandOben = 40;

    private const float RandX = 40;

    #endregion

    #region Constructors

    public DependencyOverviewForm() : base() =>
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

    #endregion

    #region Properties

    /// <summary>
    /// Die Übersicht ist tabellenunabhängig; Object bleibt immer null,
    /// dadurch ist systemweit nur ein Fenster möglich.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object? Object {
        get => null;
        set {
            // Kein Zustand pro Objekt.
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Zeichnet den Graphen ins Pad: Je Knoten eine Box, je Abhängigkeit eine Line, die den Boxen ausweicht.
    /// </summary>
    private void Build() {
        if (IsDisposed) { return; }

        var (nodes, edges) = GraphBauen();

        Text = "Abhängigkeits-Übersicht";
        capBestand.Text = "<imagecode=Hierarchie|16> Formulare: " + nodes.Count(n => n.Art == NodeArt.Formular)
            + "   Tabellen: " + nodes.Count(n => n.Art == NodeArt.Tabelle)
            + "   Elemente: " + nodes.Count(n => n.Art == NodeArt.Element)
            + "   Skripte: " + nodes.Count(n => n.Art == NodeArt.Skript)
            + "   Spalten: " + nodes.Count(n => n.Art == NodeArt.Spalte)
            + "   Verweise: " + edges.Count;

        var page = new CollectionPadItem {
            Endless = true,
            BackColor = Color.White,

            // Raster aus: weder anzeigen noch einrasten.
            GridShow = 0,
            GridSnap = 0
        };

        var positionen = PositionenBerechnen(nodes);

        foreach (var thisNode in nodes) {
            if (positionen.TryGetValue(thisNode.Key, out var pos)) {
                page.Add(new DependencyBoxPadItem(thisNode, pos));
            }
        }

        foreach (var thisEdge in edges) {
            if (!positionen.TryGetValue(thisEdge.From, out var von) || !positionen.TryGetValue(thisEdge.To, out var nach)) { continue; }

            var (start, ziel) = Anschlusspunkte(von, nach);
            var line = new LinePadItem(string.Empty, PadStyles.Standard, start, ziel) {
                Linien_Verhalten = ConnectorStyle.AusweichenUndGerade
            };
            page.Add(line);
        }

        Pad.Items = page;
        //Pad.NotEditableReason = "Übersicht";
    }

    /// <summary>
    /// Baut den kompletten Abhängigkeits-Graphen auf: Formulare mit ihren Elementen,
    /// Tabellen mit ihren Skripten und besonderen Spalten
    /// (LinkedCell-Verweise und per Skript befüllte Spalten). Kanten zeigen vom Nutzer zum Genutzten.
    /// </summary>
    private static (List<Node> Nodes, List<Edge> Edges) GraphBauen() {
        var formulare = ScriptVerwendung.AlleFormulare();
        List<Node> nodes = [];
        List<Edge> edges = [];

        foreach (var thisCf in formulare) {
            _ = Knoten(nodes, FormularKey(thisCf.Filename), NodeArt.Formular, thisCf.Filename.FileNameWithoutSuffix(), thisCf.Pages.Count + " Seiten", string.Empty, true);
        }

        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is not { IsDisposed: false } tb) { continue; }
            _ = Knoten(nodes, TabellenKey(tb.KeyName), NodeArt.Tabelle, tb.Caption, "Tabelle", string.Empty, true);
        }

        foreach (var thisCf in formulare) {
            FormularVerweise(thisCf, nodes, edges);
        }

        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is not { IsDisposed: false } tb) { continue; }

            StandardFormular(tb, nodes, edges);
            TabellenSkripte(tb, nodes, edges);
        }

        LinkedCells(nodes, edges);

        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is not { IsDisposed: false } tb) { continue; }
            SkriptBefüllteSpalten(tb, nodes, edges);
        }

        return (nodes, edges);
    }

    /// <summary>
    /// Der stabile Schlüssel eines Formular-Knotens.
    /// </summary>
    private static string FormularKey(string filename) => "F:" + filename.ToLowerInvariant();

    /// <summary>
    /// Der stabile Schlüssel eines Tabellen-Knotens.
    /// </summary>
    private static string TabellenKey(string keyName) => "T:" + keyName.ToUpperInvariant();

    /// <summary>
    /// Der stabile Schlüssel eines Spalten-Knotens.
    /// </summary>
    private static string SpaltenKey(string tabellenKey, string columnKey) => "C:" + tabellenKey + "#" + columnKey.ToUpperInvariant();

    /// <summary>
    /// Kante „Tabelle → Standard-Formular", falls der Tabelle eines zugeordnet ist.
    /// </summary>
    private static void StandardFormular(Table tb, List<Node> nodes, List<Edge> edges) {
        var filename = tb.FormulaFileName();
        if (filename is not { Length: > 0 }) { return; }

        _ = Knoten(nodes, FormularKey(filename), NodeArt.Formular, filename.FileNameWithoutSuffix(), "Formular", string.Empty, false);
        Kante(edges, TabellenKey(tb.KeyName), FormularKey(filename), EdgeArt.StandardFormular, "Standard-Formular");
    }

    /// <summary>
    /// Läuft durch alle Seiten und Elemente des Formulars und legt die Element-Knoten samt
    /// Verweisen an: Besitz (Formular → Element), Tabellen-Nutzung, Unterformulare,
    /// Element-Skripte und im Formular genutzte Spalten.
    /// </summary>
    private static void FormularVerweise(ConnectedFormula cf, List<Node> nodes, List<Edge> edges) {
        var cfKey = FormularKey(cf.Filename);

        foreach (var page in cf.Pages) {
            SeiteVerweise(cf, page, cfKey, nodes, edges);
        }
    }

    /// <summary>
    /// Sammelt rekursiv die Verweise einer Seite samt eingebetteter Gruppen/Unter-Seiten.
    /// </summary>
    private static void SeiteVerweise(ConnectedFormula cf, PadItem thisItem, string cfKey, List<Node> nodes, List<Edge> edges) {
        if (thisItem is CollectionPadItem collection) {
            foreach (var child in collection) {
                if (child is not { IsDisposed: false }) { continue; }
                SeiteVerweise(cf, child, cfKey, nodes, edges);
            }

            if (collection.ReferenceTable is { IsDisposed: false } refTb) {
                Kante(edges, cfKey, TabellenKey(refTb.KeyName), EdgeArt.Referenztabelle, "Referenz");
            }

            return;
        }

        if (thisItem is not ReciverPadItem thisRp) { return; }

        var referenzen = thisRp.ReferencedFormulaFiles();
        var hasScript = thisItem as IHasScript;
        var hatSkript = hasScript is not null && hasScript.GetAllScripts().Any(s => s.Script is { Length: > 0 });
        var hatTabelle = thisItem is ReciverSenderPadItem { TableOutput: { IsDisposed: false } };
        var istEditFeld = thisItem is EditFieldPadItem;

        var elementKey = cfKey + "#" + thisItem.KeyName;

        if (referenzen.Count > 0 || hatTabelle || hatSkript || istEditFeld) {
            var typ = thisItem.GetType().Name.Replace("PadItem", string.Empty);
            _ = Knoten(nodes, elementKey, NodeArt.Element, ElementCaption(thisItem, hasScript), typ, cfKey, true);
            Kante(edges, cfKey, elementKey, EdgeArt.FormularElement, string.Empty);
        }

        foreach (var datei in referenzen) {
            if (string.Equals(datei, cf.Filename, StringComparison.OrdinalIgnoreCase)) { continue; }

            _ = Knoten(nodes, FormularKey(datei), NodeArt.Formular, datei.FileNameWithoutSuffix(), "Formular", string.Empty, false);
            Kante(edges, cfKey, FormularKey(datei), EdgeArt.Unterformular, "Unterformular");
        }

        if (thisItem is ReciverSenderPadItem { TableOutput: { IsDisposed: false } outTb }) {
            Kante(edges, elementKey, TabellenKey(outTb.KeyName), EdgeArt.ElementTabelle, string.Empty);
        }

        if (hatSkript) {
            foreach (var sd in hasScript.GetAllScripts()) {
                if (sd.Script is not { Length: > 0 } s) { continue; }

                foreach (var zielKey in ReferenzierteTabellen(s, cfKey)) {
                    Kante(edges, elementKey, zielKey, EdgeArt.ElementSkript, string.Empty);
                }
            }
        }

        if (thisItem is EditFieldPadItem { Column: { IsDisposed: false } col }) {
            MarkiereSpalteImFormular(nodes, edges, col, cfKey);
        }
    }

    /// <summary>
    /// Der Anzeigename eines Formular-Elements: bei Eingabefeldern die Spalte,
    /// sonst der erste Skript-Name, sonst der interne Schlüssel.
    /// </summary>
    private static string ElementCaption(PadItem item, IHasScript? hasScript) {
        if (item is EditFieldPadItem { ColumnKey: { Length: > 0 } ck }) { return ck; }

        var skriptName = hasScript?.GetAllScripts().FirstOrDefault(s => s.KeyName is { Length: > 0 })?.KeyName;
        if (skriptName is { Length: > 0 }) { return skriptName; }

        return item.KeyName;
    }

    /// <summary>
    /// Legt für jede Tabelle ihre Skripte als Knoten unter der Tabelle an:
    /// Kante Tabelle → Skript (Besitz) und Skript → beeinflusste Tabellen.
    /// </summary>
    private static void TabellenSkripte(Table tb, List<Node> nodes, List<Edge> edges) {
        var tbKey = TabellenKey(tb.KeyName);

        foreach (var sd in tb.GetAllScripts()) {
            if (sd.Script is not { Length: > 0 } s) { continue; }

            var skriptKey = tbKey + "#S#" + sd.KeyName;
            _ = Knoten(nodes, skriptKey, NodeArt.Skript, sd.KeyName, "Skript", tbKey, true);
            Kante(edges, tbKey, skriptKey, EdgeArt.TabellenSkript, string.Empty);

            foreach (var zielKey in ReferenzierteTabellen(s, tbKey)) {
                Kante(edges, skriptKey, zielKey, EdgeArt.SkriptTabelle, string.Empty);
            }
        }
    }

    /// <summary>
    /// Legt die Knoten aller LinkedCell-Spalten samt ihrer Ziele an:
    /// Spalte → verlinkte Spalte bzw. Spalte → verlinkte Tabelle.
    /// Zusätzlich im Formular genutzte Spalten ergänzt MarkiereSpalteImFormular.
    /// </summary>
    private static void LinkedCells(List<Node> nodes, List<Edge> edges) {
        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is not { IsDisposed: false } tb) { continue; }

            foreach (var thisColumn in tb.Column) {
                if (thisColumn is not { IsDisposed: false } col) { continue; }

                if (col.LinkedTableTableName is not { Length: > 0 } ziel ||
                    string.Equals(ziel, tb.KeyName, StringComparison.OrdinalIgnoreCase)) { continue; }

                var quellKey = SpaltenKey(TabellenKey(tb.KeyName), col.KeyName);
                _ = Knoten(nodes, quellKey, NodeArt.Spalte, SpaltenCaption(col), "LinkedCell → " + ziel, TabellenKey(tb.KeyName), true);
                Kante(edges, TabellenKey(tb.KeyName), quellKey, EdgeArt.TabellenSpalte, string.Empty);

                var zielTbKey = ZielTabellenKey(nodes, ziel);

                if (col.ColumnKeyOfLinkedTable is { Length: > 0 } zielSpalte) {
                    var zielKey = SpaltenKey(zielTbKey, zielSpalte);
                    _ = Knoten(nodes, zielKey, NodeArt.Spalte, zielSpalte, "LinkedCell-Ziel", zielTbKey, IstGeladen(nodes, zielTbKey));
                    Kante(edges, quellKey, zielKey, EdgeArt.LinkedCell, "LinkedCell");
                } else {
                    Kante(edges, quellKey, zielTbKey, EdgeArt.LinkedCell, "LinkedCell");
                }
            }
        }
    }

    /// <summary>
    /// Merkt sich eine im Formular genutzte Spalte: Knoten unter der Tabelle,
    /// Besitz-Kante Tabelle → Spalte und Nutz-Kante Spalte → Formular.
    /// Nur relevante Spalten bekommen eine Box (siehe IstRelevanteSpalte).
    /// </summary>
    private static void MarkiereSpalteImFormular(List<Node> nodes, List<Edge> edges, ColumnItem col, string cfKey) {
        if (!IstRelevanteSpalte(col)) { return; }
        if (col.Table is not { IsDisposed: false } colTb) { return; }

        var tbKey = ZielTabellenKey(nodes, colTb.KeyName);
        if (tbKey.Length == 0) { return; }

        var key = SpaltenKey(tbKey, col.KeyName);
        var kn = Knoten(nodes, key, NodeArt.Spalte, SpaltenCaption(col), "im Formular", tbKey, IstGeladen(nodes, tbKey));
        kn.ErgänzeInfo("im Formular");

        Kante(edges, tbKey, key, EdgeArt.TabellenSpalte, string.Empty);
        Kante(edges, key, cfKey, EdgeArt.SpalteFormular, string.Empty);
    }

    /// <summary>
    /// true, wenn die Spalte eine eigene Box bekommt: sie verlinkt, wird verlinkt oder wird per Skript befüllt.
    /// Die Kriterien entsprechen den Verwendungsinformationen des ColumnEditors.
    /// </summary>
    private static bool IstRelevanteSpalte(ColumnItem col) =>
        col.LinkedTableTableName is { Length: > 0 } ||                                         // verlinkt selbst
        col.ColumnSystemInfo.Contains("Links to me", StringComparison.OrdinalIgnoreCase) ||    // wird verlinkt
        IstSkriptBefüllt(col);

    /// <summary>
    /// true, wenn die Spalte per Skript befüllt/berechnet wird: intern über Event-Skripte
    /// oder extern über Skript-'Set' (ColumnEditor: „Ext. Skript-'Set'").
    /// </summary>
    private static bool IstSkriptBefüllt(ColumnItem col) =>
        col.UsedInScript() ||
        col.ColumnSystemInfo.Contains("Edit with Script", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Legt Knoten für per Skript befüllte Spalten an, die nicht ohnehin über LinkedCells
    /// oder Formulare bekannt sind: Kante Tabelle → Spalte (Besitz).
    /// </summary>
    private static void SkriptBefüllteSpalten(Table tb, List<Node> nodes, List<Edge> edges) {
        var tbKey = TabellenKey(tb.KeyName);

        foreach (var thisColumn in tb.Column) {
            if (thisColumn is not { IsDisposed: false } col) { continue; }
            if (!IstSkriptBefüllt(col)) { continue; }

            var key = SpaltenKey(tbKey, col.KeyName);
            _ = Knoten(nodes, key, NodeArt.Spalte, SpaltenCaption(col), "per Skript befüllt", tbKey, true);
            Kante(edges, tbKey, key, EdgeArt.TabellenSpalte, string.Empty);
        }
    }

    /// <summary>
    /// Die Anzeigename einer Spalte: Beschriftung, sonst Schlüssel.
    /// </summary>
    private static string SpaltenCaption(ColumnItem col) => col.Caption is { Length: > 0 } c ? c : col.KeyName;

    /// <summary>
    /// Liefert die Knoten-Schlüssel aller Tabellen, die im Skripttext benannt werden
    /// (Tabellenname als Wort oder {TBL:...}-Token), außer dem Besitzer selbst.
    /// </summary>
    private static IEnumerable<string> ReferenzierteTabellen(string skript, string besitzerKey) {
        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is not { IsDisposed: false } tb) { continue; }

            var key = TabellenKey(tb.KeyName);
            if (key == besitzerKey) { continue; }

            if (skript.IndexOfWord(tb.KeyName, 0, RegexOptions.IgnoreCase) >= 0 ||
                skript.Contains("{TBL:" + tb.KeyName + "}", StringComparison.OrdinalIgnoreCase)) {
                yield return key;
            }
        }

        // Nicht geladene Tabellen: deren Namen kennt nur das Dateisystem.
        foreach (var name in Table.AllAvailableTables()) {
            var kurz = name.FileNameWithoutSuffix().ToUpperInvariant();
            var key = TabellenKey(kurz);
            if (key == besitzerKey) { continue; }

            if (skript.IndexOfWord(kurz, 0, RegexOptions.IgnoreCase) >= 0 ||
                skript.Contains("{TBL:" + kurz + "}", StringComparison.OrdinalIgnoreCase)) {
                yield return key;
            }
        }
    }

    /// <summary>
    /// Liefert den Schlüssel des Tabellen-Knotens für einen Tabellennamen
    /// und legt den Knoten als nicht geladen an, falls er fehlt.
    /// </summary>
    private static string ZielTabellenKey(List<Node> nodes, string tableName) {
        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is { IsDisposed: false } tb && string.Equals(tb.KeyName, tableName, StringComparison.OrdinalIgnoreCase)) {
                return TabellenKey(tb.KeyName);
            }
        }

        var kurz = tableName.FileNameWithoutSuffix().ToUpperInvariant();
        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is { IsDisposed: false } tb && string.Equals(tb.KeyName, kurz, StringComparison.OrdinalIgnoreCase)) {
                return TabellenKey(tb.KeyName);
            }
        }

        var key = TabellenKey(kurz);
        _ = Knoten(nodes, key, NodeArt.Tabelle, kurz, "Tabelle (nicht geladen)", string.Empty, false);
        return key;
    }

    /// <summary>
    /// true, wenn der Tabellen-Knoten am Schlüssel existiert und geladen ist.
    /// </summary>
    private static bool IstGeladen(List<Node> nodes, string tabellenKey) => nodes.FirstOrDefault(n => n.Key == tabellenKey)?.Geladen ?? false;

    /// <summary>
    /// Sucht den Knoten am Schlüssel oder legt ihn neu an.
    /// </summary>
    private static Node Knoten(List<Node> nodes, string key, NodeArt art, string caption, string info, string ownerKey, bool geladen) {
        var ex = nodes.FirstOrDefault(n => n.Key == key);
        if (ex is not null) {
            ex.SetLoaded(geladen);
            return ex;
        }

        var n = new Node(key, art, caption, info, ownerKey, geladen);
        nodes.Add(n);
        return n;
    }

    /// <summary>
    /// Ergänzt eine Kante; Kanten desselben Paares und Typs werden zusammengefasst.
    /// Ohne Label wird keine Beschriftung geführt.
    /// </summary>
    private static void Kante(List<Edge> edges, string from, string to, EdgeArt art, string label) {
        if (from == to) { return; }

        if (edges.FirstOrDefault(e => e.From == from && e.To == to && e.Art == art) is not { } ex) {
            edges.Add(new Edge(from, to, art, label is { Length: > 0 } ? [label] : []));
            return;
        }

        if (label is { Length: > 0 } && !ex.Labels.Contains(label)) { ex.Labels.Add(label); }
    }

    /// <summary>
    /// Gruppiert die Knoten und liefert die Box-Position je Knoten-Schlüssel;
    /// Skripte liegen links unter der Tabelle, Spalten rechts.
    /// </summary>
    private static Dictionary<string, RectangleF> PositionenBerechnen(List<Node> nodes) {
        Dictionary<string, RectangleF> positionen = [];

        var yFormular = RandOben;
        var yElement = yFormular + BoxHöhe + AbstandFormularElement;
        var yTabelle = yElement + BoxHöhe + AbstandElementTabelle;
        var ySkript = yTabelle + BoxHöhe + AbstandTabelleSkript;
        var ySpalte = ySkript + BoxHöhe + AbstandSkriptSpalte;

        var formulare = nodes.Where(n => n.Art == NodeArt.Formular).OrderBy(n => n.Caption).ToList();
        var tabellen = nodes.Where(n => n.Art == NodeArt.Tabelle).OrderBy(n => n.Caption).ToList();

        var x = RandX;

        foreach (var thisCf in formulare) {
            var elemente = nodes.Where(n => n.Art == NodeArt.Element && n.OwnerKey == thisCf.Key).OrderBy(n => n.Caption).ToList();
            var breite = ReihenBreite(elemente.Count);

            positionen.Add(thisCf.Key, new RectangleF(x + (breite - BoxBreite) / 2f, yFormular, BoxBreite, BoxHöhe));
            PositioniereReihe(positionen, elemente, x, breite, yElement);

            x += breite + GapGruppe;
        }

        foreach (var thisTb in tabellen) {
            var skripte = nodes.Where(n => n.Art == NodeArt.Skript && n.OwnerKey == thisTb.Key).OrderBy(n => n.Caption).ToList();
            var spalten = nodes.Where(n => n.Art == NodeArt.Spalte && n.OwnerKey == thisTb.Key).OrderBy(n => n.Caption).ToList();
            var breite = ReihenBreite(Math.Max(skripte.Count, 1) + spalten.Count);

            positionen.Add(thisTb.Key, new RectangleF(x + (breite - BoxBreite) / 2f, yTabelle, BoxBreite, BoxHöhe));
            PositioniereReihe(positionen, skripte, x, ReihenBreite(Math.Max(skripte.Count, 1)), ySkript);

            // Spalten rechts in der Gruppe, damit die Verbindungen die Skripte nicht kreuzen.
            var spaltenBreite = ReihenBreite(Math.Max(spalten.Count, 1));
            PositioniereReihe(positionen, spalten, x + breite - spaltenBreite, spaltenBreite, ySpalte);

            x += breite + GapGruppe;
        }

        return positionen;
    }

    /// <summary>
    /// Die Breite einer Reihe mit der angegebenen Box-Anzahl (mind. eine Box).
    /// </summary>
    private static float ReihenBreite(int anzahl) {
        var n = Math.Max(anzahl, 1);
        return n * BoxBreite + (n - 1) * GapX;
    }

    /// <summary>
    /// Setzt die Boxen einer Reihe zentriert in die angegebene Gruppen-Breite.
    /// </summary>
    private static void PositioniereReihe(Dictionary<string, RectangleF> positionen, List<Node> knoten, float gruppeX, float gruppeBreite, float y) {
        var breite = ReihenBreite(knoten.Count);
        var startX = knoten.Count == 0 ? gruppeX : gruppeX + (gruppeBreite - breite) / 2f;

        for (var i = 0; i < knoten.Count; i++) {
            positionen.Add(knoten[i].Key, new RectangleF(startX + i * (BoxBreite + GapX), y, BoxBreite, BoxHöhe));
        }
    }

    /// <summary>
    /// Der Start- und Zielpunkt einer Verbindung: abwärts an Unter-/Oberkante,
    /// aufwärts an Ober-/Unterkante, auf gleicher Höhe unten herum.
    /// </summary>
    private static (PointF Start, PointF Ziel) Anschlusspunkte(RectangleF von, RectangleF nach) {
        var cxVon = von.Left + von.Width / 2f;
        var cxNach = nach.Left + nach.Width / 2f;

        if (von.Top < nach.Top) { return (new PointF(cxVon, von.Bottom + 2), new PointF(cxNach, nach.Top - 2)); }
        if (von.Top > nach.Top) { return (new PointF(cxVon, von.Top - 2), new PointF(cxNach, nach.Bottom + 2)); }

        return (new PointF(cxVon, von.Bottom + 2), new PointF(cxNach, nach.Bottom + 2));
    }

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        Build();
    }

    #endregion

    #region Nested Types

    /// <summary>
    /// Die Art eines Knotens; bestimmt zugleich die Zeile im Übersichts-Pad.
    /// </summary>
    public enum NodeArt {

        Formular,
        Element,
        Tabelle,
        Skript,
        Spalte
    }

    /// <summary>
    /// Die Art einer Abhängigkeits-Kante.
    /// </summary>
    public enum EdgeArt {

        StandardFormular,
        Referenztabelle,
        FormularElement,
        ElementTabelle,
        Unterformular,
        ElementSkript,
        TabellenSkript,
        SkriptTabelle,
        TabellenSpalte,
        SpalteFormular,
        LinkedCell
    }

    /// <summary>
    /// Ein Knoten des Graphen: Formular, Formular-Element, Tabelle, Skript oder Spalte.
    /// </summary>
    /// <param name="key">Eindeutiger Schlüssel des Knotens.</param>
    /// <param name="art">Die Art des Knotens.</param>
    /// <param name="caption">Der angezeigte Name.</param>
    /// <param name="info">Die zweite Textzeile (Art/Zusatz).</param>
    /// <param name="ownerKey">Schlüssel des übergeordneten Knotens (Element → Formular, Skript/Spalte → Tabelle).</param>
    /// <param name="geladen">true, wenn die Quelle aktuell geladen ist.</param>
    public sealed class Node(string key, NodeArt art, string caption, string info, string ownerKey, bool geladen) {

        #region Properties

        public string Key { get; } = key;

        public NodeArt Art { get; } = art;

        public string Caption { get; } = caption;

        /// <summary>
        /// Die zweite Textzeile des Knotens.
        /// </summary>
        public string Info { get; private set; } = info;

        /// <summary>
        /// Der Schlüssel des übergeordneten Knotens; leer für Formulare und Tabellen.
        /// </summary>
        public string OwnerKey { get; } = ownerKey;

        /// <summary>
        /// true, wenn die Quelle aktuell geladen ist; sonst ist der Knoten ein Verweis auf Nicht-Geladenes.
        /// </summary>
        public bool Geladen { get; private set; } = geladen;

        #endregion

        #region Methods

        /// <summary>
        /// Härtest den Lade-Zustand, wenn der Knoten zusätzlich über eine geladene Quelle bekannt wird.
        /// </summary>
        internal void SetLoaded(bool geladen) {
            if (!geladen || Geladen) { return; }

            Geladen = true;
        }

        /// <summary>
        /// Ergänzt die Info-Zeile; bekannte Einträge bleiben unverändert.
        /// </summary>
        internal void ErgänzeInfo(string info) {
            if (info is not { Length: > 0 } i || Info.Contains(i)) { return; }

            Info = Info is { Length: > 0 } ? Info + " · " + i : i;
        }

        #endregion
    }

    /// <summary>
    /// Eine gerichtete Kante vom Nutzer zum Genutzten.
    /// </summary>
    /// <param name="from">Schlüssel des nutzenden Knotens.</param>
    /// <param name="to">Schlüssel des genutzten Knotens.</param>
    /// <param name="art">Die Art des Verweises.</param>
    /// <param name="labels">Die Beschriftungen (z. B. „LinkedCell").</param>
    public sealed class Edge(string from, string to, EdgeArt art, List<string> labels) {

        #region Properties

        public string From { get; } = from;

        public string To { get; } = to;

        public EdgeArt Art { get; } = art;

        public List<string> Labels { get; } = labels;

        #endregion

        #region Methods

        // TODO: Unused
        /// <summary>
        /// Die Beschriftung der Kante als eine Zeile; ohne Label ein leerer Text.
        /// </summary>
        public string Label() {
            if (Labels.Count == 0) { return string.Empty; }
            if (Labels.Count == 1) { return Labels[0]; }

            return Labels[0] + " …";
        }

        #endregion
    }

    #endregion
}
