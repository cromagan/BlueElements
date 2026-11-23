// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Editoren;
using BlueControls.Renderer;
using BlueTable.ColumnFormats;
using BlueTable.EventArgs;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs;

/// <summary>
/// Editor zur Anzeige von Fragment-Sätzen als Tabelle: Ein Satz je Zeile.
/// Als Quelle dienen ein Stream, ein Dateiname, ein IFragmentStoreable-Objekt
/// oder eine Liste von Fragment-Sätzen.
/// Der Editor ist rein lesend; ein Zurückschreiben ist nicht vorgesehen.
/// </summary>
public sealed partial class FragmentEditor : UserControl, IIsEditor {

    #region Fields

    /// <summary>
    /// Arbeitskopie der Sätze in Datei-Reihenfolge; die Spalte "Quelle" jeder Zeile
    /// hält den Index in diese Liste.
    /// </summary>
    private List<FragmentRecord> _records = [];

    #endregion

    #region Constructors

    public FragmentEditor() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        tblFragments.VisibleRowsChanged += TblFragments_VisibleRowsChanged;
    }

    #endregion

    #region Events

    /// <summary>
    /// Durchgereichtes Ereignis der eingebetteten Tabelle, z. B. für Statuszeilen der Hosts.
    /// </summary>
    public event EventHandler<TableEventArgs>? VisibleRowsChanged;

    #endregion

    #region Properties

    public Type? EditorFor => null;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object? InputItem {
        get => _records;
        set {
            switch (value) {
                case null:
                    LoadFragments([]);
                    break;

                case string filename:
                    LoadFragments(filename);
                    break;

                case Stream stream:
                    LoadFragments(stream);
                    break;

                case IEnumerable<FragmentRecord> fragments:
                    LoadFragments(fragments.ToList());
                    break;

                case IFragmentStoreable storeable:
                    LoadFragments(storeable);
                    break;
            }
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public EditorMode Mode { get; set; } = EditorMode.OnlyShow;

    public EditorMode SupportedModes => EditorMode.OnlyShow;

    #endregion

    #region Methods

    /// <summary>
    /// Der Editor ist rein lesend; es wird kein verändertes Objekt erzeugt.
    /// </summary>
    public object? CreateNewItem() => null;

    /// <summary>
    /// Löst das VisibleRowsChanged-Ereignis aus (inkl. Disposed-Prüfung).
    /// </summary>
    private void OnVisibleRowsChanged(TableEventArgs e) {
        if (IsDisposed) { return; }
        VisibleRowsChanged?.Invoke(this, e);
    }

    private static string SymbolFor(FragmentRecord record) {
        if (record.IsTag) { return ImageCode.Information + "|24"; }
        if (record.HasMeta) { return ImageCode.Stift + "|24"; }
        return ImageCode.Puzzle + "|24";
    }

    /// <summary>
    /// Typbezeichner der Zeile: Der Kette-Schluss wird über die Vorfahren benannt
    /// (empfangender Zahlraum) bzw. als roher Wert.
    /// </summary>
    private static string TypeText(ReadOnlySpan<FragmentRecord> path, bool tableRoot, string layoutTypeName) {
        // Kopf-Datei: Nur das Layout-Blatt gehört der Verwaltung.
        if (!tableRoot) { return SaveableFile.GetNameOf(path[^1].LeafType); }

        // Objekt-Raum der verwalteten Datei: Nur Tabellen-Pfade erhalten echte
        // Typ-Namen, fremde Zahlräume zeigen die Zahl.
        return layoutTypeName.Contains(nameof(Table), StringComparison.OrdinalIgnoreCase)
            ? Table.GetNameOf(path)
            : path[^1].LeafType.ToString1();
    }

    private void EnsureTable() {
        if (tblFragments.Table is { IsDisposed: false }) { return; }

        var tb = Table.Get();

        var cNr = tb.Column.GenerateAndAdd("Nr", "Nr.", LongOnlyPositiveColumnFormat.Instance);
        if (cNr is { IsDisposed: false }) { cNr.IsFirst = true; }

        _ = tb.Column.GenerateAndAdd("Tiefe", "Tiefe", LongOnlyPositiveColumnFormat.Instance);
        var cTyp = tb.Column.GenerateAndAdd("Typ", "Typ", TextOneLineColumnFormat.Instance);
        var cDatentyp = tb.Column.GenerateAndAdd("Datentyp", "Datentyp", TextOneLineColumnFormat.Instance);
        var cWert = tb.Column.GenerateAndAdd("Wert", "Wert", TextMultilineColumnFormat.Instance);
        var cBenutzer = tb.Column.GenerateAndAdd("Benutzer", "Benutzer", TextOneLineColumnFormat.Instance);
        var cZeit = tb.Column.GenerateAndAdd("Zeit", "Zeit (UTC)", DateTimeColumnFormat.Instance);
        var cKommentar = tb.Column.GenerateAndAdd("Kommentar", "Kommentar", TextOneLineColumnFormat.Instance);
        var cSymbol = tb.Column.GenerateAndAdd("Symbol", "Symbol", ImageAndTextColumnFormat.Instance);

        // "Quelle" bleibt unsichtbar und verknüpft die Zeile mit dem Arbeitssatz.
        _ = tb.Column.GenerateAndAdd("Quelle", "Quelle", LongOnlyPositiveColumnFormat.Instance);

        // Nur Ansicht: keinerlei Spalte ist veränderbar.
        tb.Column.DisableAllEditing();

        foreach (var thisColumn in tb.Column) {
            if (thisColumn.IsSystemColumn()) { continue; }
            if (thisColumn == cTyp || thisColumn == cDatentyp || thisColumn == cWert || thisColumn == cBenutzer || thisColumn == cKommentar) {
                thisColumn.DefaultRenderer = TextOneLineRenderer.ClassId;
            }
        }

        cWert?.MultiLine = true;
        cKommentar?.MultiLine = true;

        if (cZeit is { IsDisposed: false }) {
            var o = new DateTimeRenderer {
                UTCToLocal = true,
                ShowSymbol = true
            };
            cZeit.DefaultRenderer = o.MyClassId;
            cZeit.RendererSettings = o.ParseableItems().FinishParseable();
        }

        if (cSymbol is { IsDisposed: false }) {
            var o = new ImageAndTextRenderer {
                Text_anzeigen = false,
                Bild_anzeigen = true
            };
            cSymbol.DefaultRenderer = o.MyClassId;
            cSymbol.RendererSettings = o.ParseableItems().FinishParseable();
        }

        tb.RepairAfterParse();

        var tcvc = ColumnViewCollection.ParseAll(tb);
        tcvc[1].ShowColumns("Nr", "Tiefe", "Typ", "Datentyp", "Wert", "Benutzer", "Zeit", "Kommentar", "Symbol");
        tcvc[1].ScaleToFit = ScaleToFitMode.Aggressiv;
        tb.ColumnArrangements = tcvc.AsReadOnly();

        tb.RepairAfterParse();

        // Die Anzeige-Tabelle färbt jede Zeile je Tiefe-Stufe 10 % dunkler.
        tb.EventScript = new TableScriptCollection(tb, [RowColorScript(tb)]);

        tblFragments.Table = tb;
        tblFragments.Arrangement = string.Empty;
        tblFragments.ColumnMoveAllowed = false;
        tblFragments.SortDefinitionTemporary = new RowSortDefinition(tb, cNr, false);
    }

    /// <summary>
    /// Zeilen-Skript der Anzeige-Tabelle: färbt die Zeile je Tiefe-Stufe 10 % dunkler
    /// (Stufe 0 = weiß, Stufe 2 = 20 % Grau) als HexCode.
    /// </summary>
    private static TableScriptDescription RowColorScript(Table tb) {
        List<string> zeilen = [];

        for (var stufe = 0; stufe <= 10; stufe++) {
            var hell = (int)(255L * (10 - stufe) / 10);
            var hex = hell.ToString("X2", CultureInfo.InvariantCulture);
            zeilen.Add($"If(Tiefe=={stufe}) {{ RowColor=\"{hex}{hex}{hex}\"; }}");
        }

        return new TableScriptDescription(
            tb,
            "RowColor",
            string.Join("\r\n", zeilen),
            string.Empty,
            "Färbt die Zeile je Tiefe-Stufe 10 % dunkler.",
            string.Empty,
            new System.Collections.ObjectModel.ReadOnlyCollection<string>([]),
            ScriptEventTypes.prepare_formula,
            true,
            true,
            string.Empty,
            null,
            0,
            0);
    }

    private void FillRows() {
        if (tblFragments.Table is not { IsDisposed: false } tb) { return; }

        tb.SuppressEvents();
        try {
            tb.Row.Clear(string.Empty);

            // Kette der Sätze: Die Vorfahren bestimmen den Zahlraum der Typ-Spalte,
            // der Schluss ist der gerade benannte Satz. Kopf-Dateien beginnen mit
            // dem Layout-Blatt, Journal- und Chunk-Dateien direkt im
            // Tabellen-Raum.
            List<FragmentRecord> path = [];
            var tableRoot = true;
            var layoutTypeName = string.Empty;
            var rootChecked = false;

            for (var i = 0; i < _records.Count; i++) {
                var record = _records[i];
                var r = tb.Row.GenerateAndAdd((i + 1).ToString1(), string.Empty);
                if (r is not { IsDisposed: false }) { continue; }

                // Vorfahren enden am ersten Satz gleicher oder geringerer Tiefe.
                while (path.Count > 0 && path[^1].Depth >= record.Depth) {
                    path.RemoveAt(path.Count - 1);
                }

                string typeText;
                if (record.IsTag) {
                    if (record.TagType == FragmentTagType.Type) { layoutTypeName = record.StringContent; }

                    // ItemPath/ItemList/ItemListChunk benennen den Zahlraum der Folge-Sätze und bleiben in der Kette.
                    if (record.TagType is FragmentTagType.ItemPath or FragmentTagType.ItemList or FragmentTagType.ItemListChunk) { path.Add(record); }

                    typeText = record.TagType.ToString();
                } else {
                    if (!rootChecked) {
                        // Erster Daten-Satz: Das Layout-Blatt mit bekannter Layout-ClassId kennzeichnet eine Kopf-Datei.
                        rootChecked = true;
                        tableRoot = record.LeafType != (byte)SaveableFile.SaveableFileFragmentType.LayoutClassId || SaveableFile.AllFileSaveTypes[record.StringContent] is null;
                    }

                    // Daten-Sätze schließen die Kette ab und werden aus ihr benannt.
                    path.Add(record);
                    typeText = TypeText(CollectionsMarshal.AsSpan(path), tableRoot, layoutTypeName);
                }

                r.CellSet("Tiefe", record.Depth, string.Empty);
                r.CellSet("Typ", typeText, string.Empty);
                r.CellSet("Datentyp", record.DataType.ToString(), string.Empty);
                r.CellSet("Wert", record.ReadableContent(), string.Empty);
                r.CellSet("Benutzer", record.User, string.Empty);
                if (record.HasMeta) { r.CellSet("Zeit", record.DateTimeUtc, string.Empty); }
                r.CellSet("Kommentar", record.Comment, string.Empty);
                r.CellSet("Symbol", SymbolFor(record), string.Empty);
                r.CellSet("Quelle", i, string.Empty);
            }
        } finally {
            tb.ResumeEvents();
        }

        tb.RepairAfterParse();
    }

    private void LoadFragments(List<FragmentRecord> fragments) {
        _records = fragments;
        EnsureTable();
        FillRows();
    }

    private void LoadFragments(Stream stream) {
        List<FragmentRecord> fragments = [];

        try {
            while (FragmentRecord.TryReadNext(stream) is { } record) {
                fragments.Add(record);
            }
        } catch (Exception ex) {
            Notification.Show("Fragment-Daten unlesbar:<br>" + ex.Message, ImageCode.Warnung);
            return;
        }

        LoadFragments(fragments);
    }

    /// <summary>
    /// Serialisiert das Objekt ins Fragment-Format und zeigt dessen Sätze an.
    /// </summary>
    private void LoadFragments(IFragmentStoreable storeable) {
        using var ms = new MemoryStream();

        if (!storeable.Serialize(ms, 0)) {
            Notification.Show("Fragment-Daten konnten nicht erzeugt werden.", ImageCode.Warnung);
            return;
        }

        ms.Position = 0;
        LoadFragments(ms);
    }

    private void LoadFragments(string filename) {
        var data = IO.ReadAllBytes(filename, 10);

        if (data.IsFailed || data.Value is not byte[] content) {
            Notification.Show($"Fragment-Datei konnte nicht geladen werden:<br>{filename}<br><br>{data.FailedReason}", ImageCode.Warnung);
            return;
        }

        if (content.IsZipped()) { content = content.UnzipIt() ?? []; }

        using var ms = new MemoryStream(content);

        LoadFragments(ms);
    }

    private void TblFragments_VisibleRowsChanged(object? sender, TableEventArgs e) {
        if (InvokeRequired) {
            Invoke(new Action(() => TblFragments_VisibleRowsChanged(sender, e)));
            return;
        }

        OnVisibleRowsChanged(e);
    }

    #endregion
}
