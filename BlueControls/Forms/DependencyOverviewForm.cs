// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.BlueTableDialogs;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.PadItems.Abstract;
using BlueControls.PadItems.FunktionsItems_Formular.Abstract;
using BlueScript.Interfaces;
using System.Text.RegularExpressions;
using static BlueBasics.ClassesStatic.IO;

namespace BlueControls.Forms;

/// <summary>
/// Zeigt als CreativePad die Abhängigkeiten einer Spalte und ihrer Tabelle:
/// die Tabelle mit der gewählten Spalte in der Mitte, eingehende Verweise links, ausgehende rechts.
/// Klick auf eine Box öffnet den passenden Editor bzw. die Spalten-Auswahl.
/// Ein Fenster je Spalte (IUniqueWindow, Object = ColumnItem).
/// </summary>
public sealed partial class DependencyOverviewForm : FormWithStatusBar, IUniqueWindow {

    #region Fields

    private const float BoxBreite = 240;

    private const float BoxHöhe = 46;

    private const float GapGruppe = 70;

    private const float GapY = 16;

    private const float RandOben = 40;

    private const float RandX = 40;

    /// <summary>
    /// Die Spalte, deren Abhängigkeiten angezeigt werden.
    /// </summary>
    private ColumnItem? _spalte;

    /// <summary>
    /// Die Einstiegs-Spalte (Tabellen-Schlüssel, Spalten-Schlüssel).
    /// </summary>
    private (string TableName, string ColumnKey)? _einstieg;

    /// <summary>
    /// Spalten, die per Klick auf die Tabellen-Box zusätzlich angezeigt werden (Tabellen-Schlüssel, Spalten-Schlüssel).
    /// </summary>
    private readonly List<(string TableName, string ColumnKey)> _manuelleSpalten = [];

    /// <summary>
    /// Die Schlüssel der Spalten, die in der mittleren Tabellen-Gruppe angezeigt werden.
    /// </summary>
    private readonly List<string> _spaltenImPad = [];

    /// <summary>
    /// Die Positionen der letzten Darstellung; unveränderte Knoten behalten so ihren Platz.
    /// </summary>
    private readonly Dictionary<string, RectangleF> _positionen = [];

    #endregion

    #region Constructors

    public DependencyOverviewForm() : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        Pad.ClickedItemChanged += Pad_ClickedItemChanged;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Die Spalte, deren Abhängigkeiten angezeigt werden; ein Fenster je Spalte.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object? Object {
        get => _spalte;
        set {
            if (value is not ColumnItem { IsDisposed: false } col || col.Table is not { IsDisposed: false } tb) { return; }

            _spalte = col;
            _einstieg = (tb.KeyName, col.KeyName);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Zeichnet den Graphen ins Pad: die Fokus-Tabelle mit ihren gewählten Spalten in der Mitte,
    /// eingehende Verweise links, ausgehende rechts, je Abhängigkeit eine Line, die den Boxen ausweicht.
    /// </summary>
    private void Build() {
        if (IsDisposed) { return; }
        if (_spalte is not { IsDisposed: false } col || col.Table is not { IsDisposed: false } tb) { return; }

        var tabellenKey = TabellenKey(tb.KeyName);
        var mitteSpalten = MitteSpaltenKeys(tb);

        var (nodes, edges) = GraphBauen(_einstieg, _manuelleSpalten);
        var (mitte, links, rechts) = FokusGruppen(nodes, edges, tabellenKey, mitteSpalten);
        var positionen = FokusPositionen(mitte, links, rechts, tabellenKey);

        Text = "Abhängigkeiten: " + tb.Caption + " · " + SpaltenCaption(col);
        capBestand.Text = "<imagecode=Hierarchie|16> " + tb.Caption
            + "   Eingehend: " + links.Sum(g => g.Boxen.Count)
            + "   Ausgehend: " + rechts.Sum(g => g.Boxen.Count)
            + "   Verweise: " + edges.Count;

        var page = new CollectionPadItem {
            Endless = true,
            BackColor = Color.White,

            // Raster aus: weder anzeigen noch einrasten.
            GridShow = 0,
            GridSnap = 0
        };

        // Gemerkte Positionen behalten, damit umgeordnete Boxen bei Neuaufbau nicht springen.
        foreach (var (key, rect) in _positionen) {
            if (positionen.ContainsKey(key)) { positionen[key] = rect; }
        }

        _positionen.Clear();
        foreach (var (key, rect) in positionen) { _positionen.Add(key, rect); }

        foreach (var thisNode in nodes) {
            if (positionen.TryGetValue(thisNode.Key, out var pos)) {
                page.Add(new DependencyBoxPadItem(thisNode, pos));
            }
        }

        foreach (var thisEdge in edges) {
            var von = AnschlussKey(positionen, thisEdge.From, tabellenKey);
            var nach = AnschlussKey(positionen, thisEdge.To, tabellenKey);
            if (von.Length == 0 || nach.Length == 0 || von == nach) { continue; }

            var (start, ziel) = Anschlusspunkte(positionen[von], positionen[nach]);
            var line = new LinePadItem(string.Empty, PadStyles.Standard, start, ziel) {
                Linien_Verhalten = ConnectorStyle.AusweichenUndGerade
            };
            page.Add(line);
        }

        Pad.Items = page;

        _spaltenImPad.Clear();
        _spaltenImPad.AddRange(mitteSpalten.Where(positionen.ContainsKey));
    }

    /// <summary>
    /// Die Schlüssel der Spalten, die in der mittleren Tabellen-Gruppe angezeigt werden:
    /// die Einstiegs-Spalte plus alle manuell hinzugefügten.
    /// </summary>
    private HashSet<string> MitteSpaltenKeys(Table tb) {
        HashSet<string> keys = [];
        var tbKey = TabellenKey(tb.KeyName);

        if (_einstieg is { } e) { _ = keys.Add(SpaltenKey(tbKey, e.ColumnKey)); }

        foreach (var (tableName, columnKey) in _manuelleSpalten) {
            if (string.Equals(tableName, tb.KeyName, StringComparison.OrdinalIgnoreCase)) {
                _ = keys.Add(SpaltenKey(tbKey, columnKey));
            }
        }

        return keys;
    }

    /// <summary>
    /// Reagiert auf Klicks der Boxen: Tabelle bietet die Spalten-Auswahl, Spalte öffnet den ColumnEditor,
    /// Skript den Tabellen-Skript-Editor und Formular den Formular-Editor.
    /// </summary>
    private void Pad_ClickedItemChanged(object? sender, System.EventArgs e) {
        Develop.DebugPrint_InvokeRequired(InvokeRequired, true);

        if (Pad.LastClickedItem is not DependencyBoxPadItem box) { return; }

        switch (box.Node.Art) {
            case NodeArt.Tabelle:
                SpaltenAuswahlAnbieten(box.Node);
                break;

            case NodeArt.Spalte:
                SpalteBearbeiten(box.Node);
                break;

            case NodeArt.Skript:
                SkriptBearbeiten(box.Node);
                break;

            case NodeArt.Formular:
                FormularBearbeiten(box.Node);
                break;
        }
    }

    /// <summary>
    /// Bietet alle noch nicht angezeigten Spalten der Tabelle zur Auswahl an;
    /// die gewählte Spalte bekommt eine Box unter der Tabellen-Box.
    /// </summary>
    private void SpaltenAuswahlAnbieten(Node node) {
        if (node.Tabelle is not { IsDisposed: false } tb) { return; }

        List<ListItem> optionen = [];
        foreach (var thisColumn in tb.Column) {
            if (thisColumn is not { IsDisposed: false } col) { continue; }
            if (_spaltenImPad.Contains(SpaltenKey(TabellenKey(tb.KeyName), col.KeyName))) { continue; }

            optionen.Add(new TextListItem(SpaltenCaption(col), col.KeyName, QuickImage.Get(ImageCode.Spalte, 16), false, true, string.Empty, SpaltenCaption(col)));
        }

        if (optionen.Count == 0) {
            Notification.Show("Alle Spalten der Tabelle sind bereits angezeigt.", ImageCode.Information);
            return;
        }

        var wahl = InputBoxListBoxStyle.Show("Welche Spalte unter '" + tb.Caption + "' anzeigen?", optionen, CheckBehavior.SingleSelection, null, AddType.None);
        if (wahl?.Count != 1) { return; }

        _ = _manuelleSpalten.AddIfNotExists((tb.KeyName, wahl[0].KeyName));
        Build();
    }

    /// <summary>
    /// Öffnet die Spalte direkt im ColumnEditor (wie der Klick auf den Spaltenkopf)
    /// und aktualisiert die Ansicht, da sich Beziehungen geändert haben können.
    /// </summary>
    private void SpalteBearbeiten(Node node) {
        if (node.Spalte is not { IsDisposed: false } col) { return; }

        col.Repair();
        using var editor = new ColumnEditor(col, null);
        _ = editor.ShowDialog();
        col.Repair();
        Build();
    }

    /// <summary>
    /// Öffnet den Skript-Editor der Besitzer-Tabelle.
    /// </summary>
    private static void SkriptBearbeiten(Node node) {
        if (node.Tabelle is not { IsDisposed: false } tb) { return; }
        IUniqueWindowExtension.ShowOrCreate<TableScriptEditorForm>(tb);
    }

    /// <summary>
    /// Öffnet das Formular im Formular-Editor und aktualisiert die Ansicht.
    /// </summary>
    private void FormularBearbeiten(Node node) {
        if (node.FormularDatei is not { Length: > 0 } datei) { return; }

        using var editor = new ConnectedFormulaEditor(datei, null);
        _ = editor.ShowDialog();
        Build();
    }

    /// <summary>
    /// Baut den kompletten Abhängigkeits-Graphen auf: Tabellen mit ihren Formularen, Skripten und
    /// besonderen Spalten (LinkedCell-Verweise, per Skript befüllte, im Formular genutzte sowie
    /// die anzuzeigenden Spalten). Kanten zeigen vom Nutzer zum Genutzten.
    /// </summary>
    private static (List<Node> Nodes, List<Edge> Edges) GraphBauen((string TableName, string ColumnKey)? einstieg, List<(string TableName, string ColumnKey)> manuelleSpalten) {
        List<Node> nodes = [];
        List<Edge> edges = [];

        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is not { IsDisposed: false } tb) { continue; }
            var kn = Knoten(nodes, TabellenKey(tb.KeyName), NodeArt.Tabelle, tb.Caption, "Tabelle", string.Empty, true);
            kn.Tabelle = tb;
        }

        foreach (var thisCf in ScriptVerwendung.AlleFormulare()) {
            FormularVerweise(thisCf, nodes, edges);
        }

        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is not { IsDisposed: false } tb) { continue; }
            TabellenSkripte(tb, nodes, edges);
        }

        LinkedCells(nodes, edges);

        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is not { IsDisposed: false } tb) { continue; }
            SkriptBefüllteSpalten(tb, nodes);
        }

        AngezeigteSpalten(einstieg, manuelleSpalten, nodes);

        return (nodes, edges);
    }

    /// <summary>
    /// Legt die anzuzeigenden Spalten an: die Einstiegs-Spalte und die manuell hinzugefügten.
    /// </summary>
    private static void AngezeigteSpalten((string TableName, string ColumnKey)? einstieg, List<(string TableName, string ColumnKey)> manuelleSpalten, List<Node> nodes) {
        if (einstieg is { } e) { SpaltenKnoten(e.TableName, e.ColumnKey, "Einstieg", nodes); }

        foreach (var thisManual in manuelleSpalten) {
            SpaltenKnoten(thisManual.TableName, thisManual.ColumnKey, "manuell hinzugefügt", nodes);
        }
    }

    /// <summary>
    /// Legt den Knoten einer anzuzeigenden Spalte an; ohne geladene Tabelle oder Spalte passiert nichts.
    /// </summary>
    private static void SpaltenKnoten(string tableName, string columnKey, string info, List<Node> nodes) {
        var tb = TabelleVonName(tableName);
        if (tb is null) { return; }

        var col = SpalteDerTabelle(tb.KeyName, columnKey);
        if (col is null) { return; }

        var tbKey = TabellenKey(tb.KeyName);
        var kn = Knoten(nodes, SpaltenKey(tbKey, col.KeyName), NodeArt.Spalte, SpaltenCaption(col), info, tbKey, true);
        kn.Spalte = col;
    }

    /// <summary>
    /// Teilt den Graphen in die drei Gruppen der Ansicht: mittig die Fokus-Tabelle mit ihren gewählten
    /// Spalten, Skripten und Formularen; links die Tabellen der eingehenden Verweise, rechts die
    /// Tabellen der ausgehenden Verweise, jeweils mit Kopf-Box und den beteiligten Boxen darunter.
    /// </summary>
    private static (List<Node> Mitte, List<Gruppe> Links, List<Gruppe> Rechts) FokusGruppen(List<Node> nodes, List<Edge> edges, string tabellenKey, HashSet<string> mitteSpalten) {
        var tabelle = nodes.FirstOrDefault(n => n.Art == NodeArt.Tabelle && n.Key == tabellenKey);
        List<Node> mitte = tabelle is null ? [] : [tabelle];

        foreach (var thisNode in nodes) {
            if (thisNode.OwnerKey != tabellenKey) { continue; }
            if (thisNode.Art == NodeArt.Spalte && !mitteSpalten.Contains(thisNode.Key)) { continue; }
            if (thisNode.Art is not (NodeArt.Spalte or NodeArt.Skript or NodeArt.Formular)) { continue; }

            mitte.Add(thisNode);
        }

        // Alle Knoten der Fokus-Tabelle: die Tabellen-Box selbst und all ihre Spalten-, Skript- und Formular-Knoten.
        var eigene = new HashSet<string>(nodes.Where(n => n.Key == tabellenKey || n.OwnerKey == tabellenKey).Select(n => n.Key));

        Dictionary<string, List<Node>> links = [];
        Dictionary<string, List<Node>> rechts = [];

        foreach (var thisEdge in edges) {
            var quelle = nodes.FirstOrDefault(n => n.Key == thisEdge.From);
            var ziel = nodes.FirstOrDefault(n => n.Key == thisEdge.To);
            if (quelle is null || ziel is null) { continue; }

            // Ausgehend: Quelle gehört zur Fokus-Tabelle → Ziel-Box rechts in der Gruppe der Ziel-Tabelle.
            if (eigene.Contains(thisEdge.From) && !eigene.Contains(thisEdge.To)) {
                if (ziel.Art == NodeArt.Tabelle) {
                    if (!rechts.ContainsKey(ziel.Key)) { rechts.Add(ziel.Key, []); }
                } else if (ziel.OwnerKey is { Length: > 0 } ownerKey) {
                    if (!rechts.TryGetValue(ownerKey, out var liste)) { rechts.Add(ownerKey, liste = []); }
                    if (!liste.Contains(ziel)) { liste.Add(ziel); }
                }
            }

            // Eingehend: Ziel gehört zur Fokus-Tabelle → Quell-Box links in der Gruppe der Quell-Tabelle.
            if (eigene.Contains(thisEdge.To) && !eigene.Contains(thisEdge.From) && quelle.OwnerKey is { Length: > 0 } quellOwner) {
                if (!links.TryGetValue(quellOwner, out var liste)) { links.Add(quellOwner, liste = []); }
                if (!liste.Contains(quelle)) { liste.Add(quelle); }
            }
        }

        // Tabellen, die sowohl ein- als auch ausgehend verweisen, bekommen nur die linke Gruppe;
        // jede Tabelle-Box existiert nur einmal, ihr Kopf darf nicht doppelt positioniert werden.
        foreach (var thisKey in links.Keys) { _ = rechts.Remove(thisKey); }

        return (mitte, Gruppen(nodes, links), Gruppen(nodes, rechts));
    }

    /// <summary>
    /// Baut aus den gesammelten Boxen je Besitzer-Tabelle eine Gruppe: Kopf-Box plus ihre Boxen;
    /// die Gruppen sind nach Tabellen-Name sortiert.
    /// </summary>
    private static List<Gruppe> Gruppen(List<Node> nodes, Dictionary<string, List<Node>> boxenJeTabelle) {
        List<Gruppe> gruppen = [];

        foreach (var thisPair in boxenJeTabelle) {
            var kopf = nodes.FirstOrDefault(n => n.Art == NodeArt.Tabelle && n.Key == thisPair.Key);
            if (kopf is null) { continue; }

            gruppen.Add(new Gruppe(kopf, Stapel(thisPair.Value)));
        }

        return [.. gruppen.OrderBy(g => g.Kopf.Caption)];
    }

    /// <summary>
    /// Der Schlüssel der Box, an die eine Kante angeschlossen wird: der Knoten selbst,
    /// sonst die Tabellen-Box, wenn der Knoten eine nicht angezeigte Spalte der Fokus-Tabelle ist.
    /// </summary>
    private static string AnschlussKey(Dictionary<string, RectangleF> positionen, string knotenKey, string tabellenKey) {
        if (positionen.ContainsKey(knotenKey)) { return knotenKey; }
        if (knotenKey.StartsWith("C:" + tabellenKey + "#", StringComparison.Ordinal)) { return tabellenKey; }

        return string.Empty;
    }

    /// <summary>
    /// Legt die Formular-Box unter der Heim-Tabelle an und verbindet sie mit allen genutzten
    /// Tabellen, Unterformularen und fremden Spalten. Heim-Tabelle ist die Tabelle mit diesem
    /// Standard-Formular, sonst die erste genutzte Tabelle.
    /// </summary>
    private static void FormularVerweise(ConnectedFormula cf, List<Node> nodes, List<Edge> edges) {
        var cfKey = FormularKey(cf.Filename);
        List<ColumnItem> genutzteSpalten = [];
        HashSet<string> genutzteTabellen = [];
        HashSet<string> unterFormulare = [];

        foreach (var page in cf.Pages) {
            SeiteSammeln(cf, page, nodes, genutzteSpalten, genutzteTabellen, unterFormulare);
        }

        var homeKey = HeimTabellenKey(cf.Filename, genutzteTabellen);
        if (homeKey.Length == 0) { return; }

        var kn = Knoten(nodes, cfKey, NodeArt.Formular, cf.Filename.FileNameWithoutSuffix(), "Formular", homeKey, true);
        kn.FormularDatei = cf.Filename;

        foreach (var thisKey in genutzteTabellen.Where(k => k != homeKey)) {
            Kante(edges, cfKey, thisKey, EdgeArt.FormularTabelle, string.Empty);
        }

        foreach (var datei in unterFormulare) {
            var zielKey = FormularKey(datei);
            var zielKn = Knoten(nodes, zielKey, NodeArt.Formular, datei.FileNameWithoutSuffix(), "Unterformular", homeKey, false);
            zielKn.FormularDatei = datei;
            Kante(edges, cfKey, zielKey, EdgeArt.Unterformular, "Unterformular");
        }

        foreach (var thisCol in genutzteSpalten) {
            MarkiereSpalteImFormular(nodes, edges, thisCol, homeKey);
        }
    }

    /// <summary>
    /// Sammelt rekursiv die genutzten Tabellen, Spalten und Unterformulare einer Seite
    /// samt eingebetteter Gruppen/Unter-Seiten.
    /// </summary>
    private static void SeiteSammeln(ConnectedFormula cf, PadItem thisItem, List<Node> nodes, List<ColumnItem> genutzteSpalten, HashSet<string> genutzteTabellen, HashSet<string> unterFormulare) {
        if (thisItem is CollectionPadItem collection) {
            foreach (var child in collection) {
                if (child is not { IsDisposed: false }) { continue; }
                SeiteSammeln(cf, child, nodes, genutzteSpalten, genutzteTabellen, unterFormulare);
            }

            if (collection.ReferenceTable is { IsDisposed: false } refTb) {
                _ = genutzteTabellen.Add(ZielTabellenKey(nodes, refTb.KeyName));
            }

            return;
        }

        if (thisItem is not ReciverPadItem thisRp) { return; }

        unterFormulare.UnionWith(thisRp.ReferencedFormulaFiles().Where(d => !string.Equals(d, cf.Filename, StringComparison.OrdinalIgnoreCase)));

        if (thisItem is ReciverSenderPadItem { TableOutput: { IsDisposed: false } outTb }) {
            _ = genutzteTabellen.Add(ZielTabellenKey(nodes, outTb.KeyName));
        }

        var hasScript = thisItem as IHasScript;
        if (hasScript is not null) {
            foreach (var sd in hasScript.GetAllScripts()) {
                if (sd.Script is not { Length: > 0 } s) { continue; }
                genutzteTabellen.UnionWith(ReferenzierteTabellen(s, string.Empty));
            }
        }

        if (thisItem is EditFieldPadItem { Column: { IsDisposed: false } col }) {
            genutzteSpalten.Add(col);
        }
    }

    /// <summary>
    /// Der Schlüssel der Heim-Tabelle eines Formulars: die Tabelle mit diesem Standard-Formular,
    /// sonst die alphabetisch erste genutzte Tabelle; leer ohne jeden Tabellen-Bezug.
    /// </summary>
    private static string HeimTabellenKey(string filename, HashSet<string> genutzteTabellen) {
        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is { IsDisposed: false } tb && string.Equals(tb.FormulaFileName(), filename, StringComparison.OrdinalIgnoreCase)) {
                return TabellenKey(tb.KeyName);
            }
        }

        return genutzteTabellen.OrderBy(k => k).FirstOrDefault() ?? string.Empty;
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
    /// Liefert die geladene Tabelle zum Namen (Schlüssel oder Dateiname ohne Suffix); null, wenn nicht geladen.
    /// </summary>
    private static Table? TabelleVonName(string tableName) {
        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is not { IsDisposed: false } tb) { continue; }

            if (string.Equals(tb.KeyName, tableName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tb.KeyName, tableName.FileNameWithoutSuffix(), StringComparison.OrdinalIgnoreCase)) {
                return tb;
            }
        }

        return null;
    }

    /// <summary>
    /// Liefert die Spalte mit dem Schlüssel aus der geladenen Tabelle mit dem Namen; null, wenn nicht gefunden.
    /// </summary>
    private static ColumnItem? SpalteDerTabelle(string tableName, string columnKey) {
        var tb = TabelleVonName(tableName);
        if (tb is null) { return null; }

        foreach (var thisColumn in tb.Column) {
            if (thisColumn is { IsDisposed: false } col && string.Equals(col.KeyName, columnKey, StringComparison.OrdinalIgnoreCase)) {
                return col;
            }
        }

        return null;
    }

    /// <summary>
    /// Liefert den Schlüssel des Tabellen-Knotens für einen Tabellennamen
    /// und legt den Knoten als nicht geladen an, falls er fehlt.
    /// </summary>
    private static string ZielTabellenKey(List<Node> nodes, string tableName) {
        var tb = TabelleVonName(tableName);
        if (tb is not null) { return TabellenKey(tb.KeyName); }

        var kurz = tableName.FileNameWithoutSuffix().ToUpperInvariant();
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
    /// Legt für jede Tabelle ihre Skripte als Boxen an: Kanten Skript → beeinflusste Tabellen
    /// und Skript → im Skript genannte Spalten der eigenen Tabelle.
    /// </summary>
    private static void TabellenSkripte(Table tb, List<Node> nodes, List<Edge> edges) {
        var tbKey = TabellenKey(tb.KeyName);

        foreach (var sd in tb.GetAllScripts()) {
            if (sd.Script is not { Length: > 0 } s) { continue; }

            var skriptKey = tbKey + "#S#" + sd.KeyName;
            var kn = Knoten(nodes, skriptKey, NodeArt.Skript, sd.KeyName, "Skript", tbKey, true);
            kn.Tabelle = tb;

            foreach (var zielKey in ReferenzierteTabellen(s, tbKey)) {
                Kante(edges, skriptKey, zielKey, EdgeArt.SkriptTabelle, string.Empty);
            }

            foreach (var thisColumn in tb.Column) {
                if (thisColumn is not { IsDisposed: false } col) { continue; }
                if (s.IndexOfWord(col.KeyName, 0, RegexOptions.IgnoreCase) < 0) { continue; }

                Kante(edges, skriptKey, SpaltenKey(tbKey, col.KeyName), EdgeArt.SkriptSpalte, string.Empty);
            }
        }
    }

    /// <summary>
    /// Legt die Knoten aller LinkedCell-Spalten samt ihrer Ziele an:
    /// Spalte → verlinkte Spalte bzw. Spalte → verlinkte Tabelle.
    /// </summary>
    private static void LinkedCells(List<Node> nodes, List<Edge> edges) {
        foreach (var thisTb in Table.AllInstances()) {
            if (thisTb is not { IsDisposed: false } tb) { continue; }

            foreach (var thisColumn in tb.Column) {
                if (thisColumn is not { IsDisposed: false } col) { continue; }

                if (col.LinkedTableTableName is not { Length: > 0 } ziel ||
                    string.Equals(ziel, tb.KeyName, StringComparison.OrdinalIgnoreCase)) { continue; }

                var quellKey = SpaltenKey(TabellenKey(tb.KeyName), col.KeyName);
                var quellKn = Knoten(nodes, quellKey, NodeArt.Spalte, SpaltenCaption(col), "LinkedCell → " + ziel, TabellenKey(tb.KeyName), true);
                quellKn.Spalte = col;

                var zielTbKey = ZielTabellenKey(nodes, ziel);

                if (col.ColumnKeyOfLinkedTable is { Length: > 0 } zielSpalte) {
                    var zielKey = SpaltenKey(zielTbKey, zielSpalte);
                    var zielKn = Knoten(nodes, zielKey, NodeArt.Spalte, zielSpalte, "LinkedCell-Ziel", zielTbKey, IstGeladen(nodes, zielTbKey));
                    var zielCol = SpalteDerTabelle(ziel, zielSpalte);
                    if (zielCol is not null) { zielKn.Spalte = zielCol; }

                    Kante(edges, quellKey, zielKey, EdgeArt.LinkedCell, "LinkedCell");
                } else {
                    Kante(edges, quellKey, zielTbKey, EdgeArt.LinkedCell, "LinkedCell");
                }
            }
        }
    }

    /// <summary>
    /// Merkt sich eine im Formular genutzte Spalte als Box unter ihrer Tabelle;
    /// fremde Tabellen bekommen zusätzlich eine Nutz-Kante vom Formular.
    /// Nur relevante Spalten bekommen eine Box (siehe IstRelevanteSpalte).
    /// </summary>
    private static void MarkiereSpalteImFormular(List<Node> nodes, List<Edge> edges, ColumnItem col, string formularKey) {
        if (!IstRelevanteSpalte(col)) { return; }
        if (col.Table is not { IsDisposed: false } colTb) { return; }

        var tbKey = ZielTabellenKey(nodes, colTb.KeyName);
        var key = SpaltenKey(tbKey, col.KeyName);
        var kn = Knoten(nodes, key, NodeArt.Spalte, SpaltenCaption(col), "im Formular", tbKey, IstGeladen(nodes, tbKey));
        kn.Spalte = col;
        kn.ErgänzeInfo("im Formular");

        if (tbKey != formularKey) {
            Kante(edges, formularKey, key, EdgeArt.FormularSpalte, string.Empty);
        }
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
    /// oder Formulare bekannt sind.
    /// </summary>
    private static void SkriptBefüllteSpalten(Table tb, List<Node> nodes) {
        var tbKey = TabellenKey(tb.KeyName);

        foreach (var thisColumn in tb.Column) {
            if (thisColumn is not { IsDisposed: false } col) { continue; }
            if (!IstSkriptBefüllt(col)) { continue; }

            var kn = Knoten(nodes, SpaltenKey(tbKey, col.KeyName), NodeArt.Spalte, SpaltenCaption(col), "per Skript befüllt", tbKey, true);
            kn.Spalte = col;
        }
    }

    /// <summary>
    /// Die Anzeigename einer Spalte: Beschriftung, sonst Schlüssel.
    /// </summary>
    private static string SpaltenCaption(ColumnItem col) => col.Caption is { Length: > 0 } c ? c : col.KeyName;

    /// <summary>
    /// Liefert die Tabellen-Knoten-Schlüssel aller Tabellen, die im Skripttext benannt werden
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
    /// Ordnet die drei Gruppen der Ansicht an: links die Tabellen der eingehenden Verweise, mittig die
    /// Fokus-Tabelle mit ihren gewählten Spalten, Skripten und Formularen, rechts die Tabellen der
    /// ausgehenden Verweise — jede Gruppe mit Kopf-Box und ihren Boxen darunter.
    /// </summary>
    private static Dictionary<string, RectangleF> FokusPositionen(List<Node> mitte, List<Gruppe> links, List<Gruppe> rechts, string tabellenKey) {
        Dictionary<string, RectangleF> positionen = [];

        var xLinks = RandX;
        var xMitte = RandX + BoxBreite + GapGruppe;
        var xRechts = xMitte + BoxBreite + GapGruppe;

        var y = RandOben;
        foreach (var thisGruppe in links) {
            y = GruppePositionieren(positionen, thisGruppe, xLinks, y);
        }

        positionen.Add(tabellenKey, new RectangleF(xMitte, RandOben, BoxBreite, BoxHöhe));

        y = RandOben + BoxHöhe + GapY;
        foreach (var knoten in Stapel(mitte)) {
            if (knoten.Key == tabellenKey) { continue; } // Die Tabellen-Box sitzt bereits fest oben.

            positionen.Add(knoten.Key, new RectangleF(xMitte, y, BoxBreite, BoxHöhe));
            y += BoxHöhe + GapY;
        }

        y = RandOben;
        foreach (var thisGruppe in rechts) {
            y = GruppePositionieren(positionen, thisGruppe, xRechts, y);
        }

        return positionen;
    }

    /// <summary>
    /// Setzt eine Gruppe als Stapel ab: Kopf-Box, darunter ihre Boxen; liefert das y der nächsten Gruppe.
    /// </summary>
    private static float GruppePositionieren(Dictionary<string, RectangleF> positionen, Gruppe gruppe, float x, float yStart) {
        var y = yStart;
        positionen.Add(gruppe.Kopf.Key, new RectangleF(x, y, BoxBreite, BoxHöhe));
        y += BoxHöhe + GapY;

        foreach (var knoten in gruppe.Boxen) {
            positionen.Add(knoten.Key, new RectangleF(x, y, BoxBreite, BoxHöhe));
            y += BoxHöhe + GapY;
        }

        return y + GapGruppe - GapY;
    }

    /// <summary>
    /// Die Boxen eines Stapels: erst Spalten, dann Skripte, dann Formulare, je alphabetisch.
    /// </summary>
    private static List<Node> Stapel(List<Node> knoten) =>
        [.. knoten.OrderBy(n => Reihenfolge(n.Art)).ThenBy(n => n.Caption)];

    /// <summary>
    /// Die Reihenfolge innerhalb eines Stapels: Spalte, Skript, Formular, Tabelle.
    /// </summary>
    private static int Reihenfolge(NodeArt art) {
        switch (art) {
            case NodeArt.Spalte:
                return 0;

            case NodeArt.Skript:
                return 1;

            case NodeArt.Formular:
                return 2;

            default:
                return 3;
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
    /// Die Art eines Knotens; bestimmt zugleich die Position im Übersichts-Pad.
    /// </summary>
    public enum NodeArt {

        Formular,
        Tabelle,
        Spalte,
        Skript
    }

    /// <summary>
    /// Die Art einer Abhängigkeits-Kante.
    /// </summary>
    public enum EdgeArt {

        Unterformular,
        SkriptTabelle,
        SkriptSpalte,
        LinkedCell,
        FormularTabelle,
        FormularSpalte
    }

    /// <summary>
    /// Ein Knoten des Graphen: Formular, Tabelle, Skript oder Spalte.
    /// </summary>
    /// <param name="key">Eindeutiger Schlüssel des Knotens.</param>
    /// <param name="art">Die Art des Knotens.</param>
    /// <param name="caption">Der angezeigte Name.</param>
    /// <param name="info">Die zweite Textzeile (Art/Zusatz).</param>
    /// <param name="ownerKey">Schlüssel des übergeordneten Tabellen-Knotens (Formular/Spalte/Skript → Tabelle).</param>
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
        /// Der Schlüssel der übergeordneten Tabellen-Box; leer für Tabellen selbst.
        /// </summary>
        public string OwnerKey { get; } = ownerKey;

        /// <summary>
        /// true, wenn die Quelle aktuell geladen ist; sonst ist der Knoten ein Verweis auf Nicht-Geladenes.
        /// </summary>
        public bool Geladen { get; private set; } = geladen;

        /// <summary>
        /// Die dargestellte Spalte; null bei nicht geladenen Zielen.
        /// </summary>
        public ColumnItem? Spalte { get; internal set; }

        /// <summary>
        /// Die Tabelle des Knotens; bei Skripten die Besitzer-Tabelle.
        /// </summary>
        public Table? Tabelle { get; internal set; }

        /// <summary>
        /// Die Datei des Formulars; leer außerhalb von Formular-Knoten.
        /// </summary>
        public string FormularDatei { get; internal set; } = string.Empty;

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

    /// <summary>
    /// Eine Gruppe des Übersichts-Pads: eine Tabellen-Kopf-Box und die Boxen darunter.
    /// </summary>
    /// <param name="kopf">Die Kopf-Box der Gruppe.</param>
    /// <param name="boxen">Die Boxen unterhalb des Kopfes, bereits in Anzeige-Reihenfolge.</param>
    public sealed class Gruppe(Node kopf, List<Node> boxen) {

        #region Properties

        public Node Kopf { get; } = kopf;

        public List<Node> Boxen { get; } = boxen;

        #endregion
    }

    #endregion
}
