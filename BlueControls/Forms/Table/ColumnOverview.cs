// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using BlueTable.ColumnFormats;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System.Globalization;
using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs;

/// <summary>
/// Zeigt alle Spalten einer Tabelle mit deren Einstellungen als bearbeitbare Tabellen-Ansicht.
/// Wird systemweit eindeutig pro Tabelle geöffnet (IUniqueWindowExtension.ShowOrCreate{T}).
/// </summary>
public sealed partial class ColumnOverview : FormWithStatusBar, IUniqueWindow, IHasTable {

    #region Fields

    /// <summary>
    /// Reihenfolge und Schlüssel der Übersichtsspalten (ohne ID).
    /// Bewusst in Großbuchstaben: ColumnItem.KeyName speichert immer upper-case.
    /// </summary>
    private static readonly string[] OverviewKeys = [
        "NAME", "BESCHRIFTUNG", "UEBERSCHRIFT1", "UEBERSCHRIFT2", "UEBERSCHRIFT3", "DATENFORMAT",
        "MAXZEICHEN", "FARBE", "SCHRIFTFARBE", "QUICKINFO", "ADMININFO", "TAGS",
        "SKRIPTDATENTYP", "SORTIERUNG", "AEHNLICHGEWICHT", "ZUSATZPRUEFUNG",
        "AUTOCORRECT", "GROSSBUCHSTABEN", "SORTIERENDOPPELTE", "AUTOREPLACE", "RUNDEN",
        "ERSTSPALTE", "KEYCOLUMN", "MULTILINE", "SAVECONTENT", "IGNOREATROWFILTER", "RECHTE"
    ];

    /// <summary>Ordnet den Zeilenschlüssel der Übersichtstabelle der echten Spalte zu.</summary>
    private readonly Dictionary<string, ColumnItem> _columnByRowKey = [];

    /// <summary>true, während die Übersicht selbst Zellwerte setzt (Zurücksetzen/Auffrischen), um Rekursion zu vermeiden.</summary>
    private bool _internalCellSet;

    #endregion

    #region Constructors

    public ColumnOverview() : base() =>
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

    #endregion

    #region Properties

    /// <summary>
    /// Die Tabelle, deren Spalten angezeigt werden. Wird von
    /// IUniqueWindowExtension.ShowOrCreate{T} gesetzt.
    /// </summary>
    public object? Object {
        get => IsDisposed ? null : Table;
        set {
            var tb = value as Table;
            if (IsDisposed || (tb?.IsDisposed ?? true)) { tb = null; }

            if (Table == tb) { return; }

            Table = tb;
            Text = Table is null ? "Spaltenübersicht" : "Spaltenübersicht: " + Table.Caption;
        }
    }

    public Table? Table { get; private set; }

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        if (tblSpalten.Table is { IsDisposed: false } t) {
            t.CellValueChanged -= SpaltenTabelle_CellValueChanged;
            t.Dispose();
        }
        if (Table is { IsDisposed: false } rt) { rt.RepairAfterParse(); }
        _columnByRowKey.Clear();
        Table = null;
    }

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        GenerateSpaltenTabelle(tblSpalten);
        FillSpalten(tblSpalten, Table);
        if (tblSpalten.Table is { IsDisposed: false } t) { t.CellValueChanged += SpaltenTabelle_CellValueChanged; }
        capSpalten.Text = "<imagecode=Information|16> Spalten: " + (tblSpalten.Table?.Row.Count ?? 0);
    }

    /// <summary>
    /// Schreibt den neuen Zellwert in die echte Spalte.
    /// Liefert false, wenn die Eingabe unzulässig ist und die Zelle zurückgesetzt werden muss.
    /// </summary>
    private static bool ApplyEdit(ColumnItem c, string key, string value) {
        if (c.Table is not { IsDisposed: false } rt) { return false; }

        if (rt.IsFreezed) {
            Notification.Show("Die Tabelle ist gesperrt:<br>" + rt.FreezedReason, ImageCode.Warnung);
            return false;
        }

        switch (key) {
            case "NAME":
                var newKey = value.ToUpperInvariant();
                if (newKey == c.KeyName) { return true; }
                if (!c.ColumNameAllowed(newKey)) {
                    Forms.MessageBox.Show("Der Spaltenname <b>" + value + "</b> ist nicht erlaubt<br>oder in der Tabelle bereits vorhanden.", ImageCode.Information, "Ok");
                    return false;
                }
                c.KeyName = newKey;
                return true;

            case "BESCHRIFTUNG":
                c.Caption = value;
                return true;

            case "UEBERSCHRIFT1":
                c.CaptionGroup1 = value;
                return true;

            case "UEBERSCHRIFT2":
                c.CaptionGroup2 = value;
                return true;

            case "UEBERSCHRIFT3":
                c.CaptionGroup3 = value;
                return true;

            case "MAXZEICHEN":
                if (!int.TryParse(value, out var maxLen) || maxLen < 1) { RejectValue(value); return false; }
                c.MaxCellLength = maxLen;
                return true;

            case "FARBE":
                if (!Converter.ColorTryParse(value, out var back)) { RejectValue(value); return false; }
                c.BackColor = back;
                return true;

            case "SCHRIFTFARBE":
                if (!Converter.ColorTryParse(value, out var fore)) { RejectValue(value); return false; }
                c.ForeColor = fore;
                return true;

            case "SKRIPTDATENTYP":
                if (!Enum.TryParse(value, out ScriptType scriptType)) { RejectValue(value); return false; }
                c.ScriptType = scriptType;
                return true;

            case "SORTIERUNG":
                if (!Enum.TryParse(value, out SortierTyp sortType)) { RejectValue(value); return false; }
                c.SortType = sortType;
                return true;

            case "AEHNLICHGEWICHT":
                var weight = ParseWeight(value);
                if (weight is null) { RejectValue(value); return false; }
                c.SimilarityWeight = weight.Value;
                return true;

            case "ZUSATZPRUEFUNG":
                if (!Enum.TryParse(value, out AdditionalCheck additionalCheck)) { RejectValue(value); return false; }
                c.AdditionalFormatCheck = additionalCheck;
                return true;

            case "AUTOCORRECT":
                c.AfterEditAutoCorrect = value == "+";
                return true;

            case "GROSSBUCHSTABEN":
                c.AfterEditDoUCase = value == "+";
                return true;

            case "SORTIERENDOPPELTE":
                c.AfterEditQuickSortRemoveDouble = value == "+";
                return true;

            case "AUTOREPLACE":
                c.AfterEditAutoReplace = new(value.SplitAndCutByCr());
                return true;

            case "RUNDEN":
                if (!int.TryParse(value, out var round) || round < -1) { RejectValue(value); return false; }
                c.AfterEditRound = round;
                return true;

            case "ERSTSPALTE":
                c.IsFirst = value == "+";
                return true;

            case "KEYCOLUMN":
                c.IsKeyColumn = value == "+";
                return true;

            case "MULTILINE":
                c.MultiLine = value == "+";
                return true;

            case "SAVECONTENT":
                c.SaveContent = value == "+";
                return true;

            case "IGNOREATROWFILTER":
                c.IgnoreAtRowFilter = value == "+";
                return true;

            case "RECHTE":
                c.PermissionGroupsChangeCell = new(value.SplitAndCutByCr());
                return true;

            default:
                return false;
        }
    }

    /// <summary>Liefert den Namen des zum Spaltenkern passenden Formats.</summary>
    private static string FormatNameOf(ColumnItem c) {
        var formatName = string.Empty;
        foreach (var thisFormat in ColumnFormat.AllFormats.Instances) {
            if (thisFormat.IsFormatIdenticalSoft(c)) { formatName = thisFormat.KeyName; }
        }
        return formatName;
    }

    /// <summary>Erzeugt eine Auswahlspalte für die Werte eines Enums.</summary>
    private static void GenerateEnumSpalte(Table tb, string keyName, string caption, Type enumType) {
        var c = tb.Column.GenerateAndAdd(keyName, caption, TextOneLineColumnFormat.Instance);
        if (c is not { IsDisposed: false } col) { return; }

        col.ControlStrategy = "Combobox";
        col.EditableWithTextInput = false;
        col.ShowValuesOfOtherCellsInDropdown = false;
        col.DropDownItems = new(Enum.GetNames(enumType));
    }

    private static void GenerateSpaltenTabelle(TableViewWithFilters tblColumns) {
        var tb = Table.Get();

        var cId = tb.Column.GenerateAndAdd("ID", "#", LongOnlyPositiveColumnFormat.Instance);
        if (cId is { IsDisposed: false } id) {
            id.IsFirst = true;
            id.DisableAllEditing();
        }

        tb.Column.GenerateAndAdd("NAME", "Name", TextOneLineColumnFormat.Instance);
        var cCap = tb.Column.GenerateAndAdd("BESCHRIFTUNG", "Beschriftung", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("UEBERSCHRIFT1", "Überschrift 1", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("UEBERSCHRIFT2", "Überschrift 2", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("UEBERSCHRIFT3", "Überschrift 3", TextOneLineColumnFormat.Instance);

        var cFormat = tb.Column.GenerateAndAdd("DATENFORMAT", "Datenformat", TextOneLineColumnFormat.Instance);
        if (cFormat is { IsDisposed: false } format) { format.DisableAllEditing(); }

        tb.Column.GenerateAndAdd("MAXZEICHEN", "Max.<br>Zeichen", LongOnlyPositiveColumnFormat.Instance);
        tb.Column.GenerateAndAdd("FARBE", "Farbe", ColorColumnFormat.Instance);
        tb.Column.GenerateAndAdd("SCHRIFTFARBE", "Schrift-<br>farbe", ColorColumnFormat.Instance);
        tb.Column.GenerateAndAdd("QUICKINFO", "Quickinfo", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("ADMININFO", "Admin-Info", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("TAGS", "Tags", TextOneLineColumnFormat.Instance);

        GenerateEnumSpalte(tb, "SKRIPTDATENTYP", "Skript-<br>Datentyp", typeof(ScriptType));
        GenerateEnumSpalte(tb, "SORTIERUNG", "Sortierung", typeof(SortierTyp));
        tb.Column.GenerateAndAdd("AEHNLICHGEWICHT", "Ähnlich-<br>Gewicht", FloatColumnFormat.Instance);
        GenerateEnumSpalte(tb, "ZUSATZPRUEFUNG", "Zusatz-<br>Prüfung", typeof(AdditionalCheck));

        tb.Column.GenerateAndAdd("AUTOCORRECT", "Auto-<br>Correct", BitColumnFormat.Instance);
        tb.Column.GenerateAndAdd("GROSSBUCHSTABEN", "Groß-<br>buchstaben", BitColumnFormat.Instance);
        tb.Column.GenerateAndAdd("SORTIERENDOPPELTE", "Sortieren +<br>Doppelte", BitColumnFormat.Instance);
        tb.Column.GenerateAndAdd("AUTOREPLACE", "Auto-<br>Replace", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("RUNDEN", "Runden", LongColumnFormat.Instance);
        tb.Column.GenerateAndAdd("ERSTSPALTE", "Erst-<br>spalte", BitColumnFormat.Instance);
        tb.Column.GenerateAndAdd("KEYCOLUMN", "Schlüssel-<br>spalte", BitColumnFormat.Instance);
        tb.Column.GenerateAndAdd("MULTILINE", "Multi-<br>Line", BitColumnFormat.Instance);
        tb.Column.GenerateAndAdd("SAVECONTENT", "Speichern", BitColumnFormat.Instance);
        tb.Column.GenerateAndAdd("IGNOREATROWFILTER", "Ohne<br>Filter", BitColumnFormat.Instance);
        tb.Column.GenerateAndAdd("RECHTE", "Änderungs-<br>Rechte", TextOneLineColumnFormat.Instance);

        foreach (var thisColumn in tb.Column) {
            if (thisColumn.IsSystemColumn()) { continue; }

            switch (thisColumn.KeyName) {
                case "BESCHRIFTUNG":
                case "UEBERSCHRIFT1":
                case "UEBERSCHRIFT2":
                case "UEBERSCHRIFT3":
                case "QUICKINFO":
                case "ADMININFO":
                case "TAGS":
                case "AUTOREPLACE":
                case "RECHTE":
                    thisColumn.MultiLine = true;
                    break;
            }
        }

        tb.RepairAfterParse();

        var tcvc = ColumnViewCollection.ParseAll(tb);
        tcvc[1].ShowColumns(["ID", .. OverviewKeys]);

        // Erste drei Spalten (Nummer, Key, Beschriftung) als Wiederholungsspalten fixieren
        string[] wiederholungsspalten = ["ID", "NAME", "BESCHRIFTUNG"];
        foreach (var key in wiederholungsspalten) {
            if (tcvc[1][key] is { IsDisposed: false } vi) { vi.Permanent = true; }
        }

        tb.ColumnArrangements = tcvc.AsReadOnly();
        tb.SortDefinition = new RowSortDefinition(tb, cCap, false);
        tblColumns.Table = tb;

        tblColumns.Arrangement = string.Empty;
        tblColumns.ColumnMoveAllowed = false;
    }

    /// <summary>Liefert den Anzeigewert einer Übersichtsspalte aus der echten Spalte.</summary>
    private static string OverviewValue(ColumnItem c, string key) {
        switch (key) {
            case "NAME":
                return c.KeyName;

            case "BESCHRIFTUNG":
                return c.Caption;

            case "UEBERSCHRIFT1":
                return c.CaptionGroup1;

            case "UEBERSCHRIFT2":
                return c.CaptionGroup2;

            case "UEBERSCHRIFT3":
                return c.CaptionGroup3;

            case "DATENFORMAT":
                return FormatNameOf(c);

            case "MAXZEICHEN":
                return c.MaxCellLength.ToString1();

            case "FARBE":
                return c.BackColor.ToHtmlCode();

            case "SCHRIFTFARBE":
                return c.ForeColor.ToHtmlCode();

            case "QUICKINFO":
                return c.QuickInfo;

            case "ADMININFO":
                return c.AdminInfo;

            case "TAGS":
                return c.ColumnTags;

            case "SKRIPTDATENTYP":
                return c.ScriptType.ToString();

            case "SORTIERUNG":
                return c.SortType.ToString();

            case "AEHNLICHGEWICHT":
                return c.SimilarityWeight.ToString1_5();

            case "ZUSATZPRUEFUNG":
                return c.AdditionalFormatCheck.ToString();

            case "AUTOCORRECT":
                return c.AfterEditAutoCorrect.ToPlusMinus();

            case "GROSSBUCHSTABEN":
                return c.AfterEditDoUCase.ToPlusMinus();

            case "SORTIERENDOPPELTE":
                return c.AfterEditQuickSortRemoveDouble.ToPlusMinus();

            case "AUTOREPLACE":
                return string.Join('\r', c.AfterEditAutoReplace);

            case "RUNDEN":
                return c.AfterEditRound.ToString1();

            case "ERSTSPALTE":
                return c.IsFirst.ToPlusMinus();

            case "KEYCOLUMN":
                return c.IsKeyColumn.ToPlusMinus();

            case "MULTILINE":
                return c.MultiLine.ToPlusMinus();

            case "SAVECONTENT":
                return c.SaveContent.ToPlusMinus();

            case "IGNOREATROWFILTER":
                return c.IgnoreAtRowFilter.ToPlusMinus();

            case "RECHTE":
                return string.Join('\r', c.PermissionGroupsChangeCell);

            default:
                return string.Empty;
        }
    }

    /// <summary>Parst die Ähnlichkeits-Gewichtung (Trennzeichen Punkt oder Komma).</summary>
    private static float? ParseWeight(string value) {
        var v = value.Trim().Replace(',', '.');
        return float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var w) ? w : null;
    }

    private static void RejectValue(string value) => Forms.MessageBox.Show("Der Wert <b>" + value + "</b> ist für diese Spalte ungültig.", ImageCode.Information, "Ok");

    private void FillSpalten(TableViewWithFilters tblColumns, Table? source) {
        if (source is not { IsDisposed: false } src) { return; }
        if (tblColumns.Table is not { IsDisposed: false } ot) { return; }

        _columnByRowKey.Clear();

        var lfdn = 0;
        foreach (var thisColumn in src.Column) {
            if (thisColumn is not { IsDisposed: false }) { continue; }

            lfdn++;
            var r = ot.Row.GenerateAndAdd("Spalte_" + lfdn, string.Empty);
            if (r is null) { continue; }

            _columnByRowKey.Add(r.KeyName, thisColumn);

            r.CellSet("ID", lfdn.ToString1(), string.Empty);
            foreach (var key in OverviewKeys) {
                if (ot.Column[key] is not { IsDisposed: false } oc) { continue; }
                r.CellSet(oc, OverviewValue(thisColumn, key), string.Empty);
            }
        }
    }

    /// <summary>Öffnet den Spalten-Editor zur Korrektur und frischt die Übersichtszeile anschließend auf.</summary>
    private void RepairWithEditor(ColumnItem realColumn, RowItem row) {
        if (realColumn.Table is not { IsDisposed: false } rt) { return; }

        if (rt.IsFreezed) {
            Notification.Show("Die Spalte <b>" + realColumn.KeyName + "</b> ist fehlerhaft:<br>" + realColumn.ErrorReason(), ImageCode.Warnung);
            return;
        }

        Forms.MessageBox.Show("Die Spalte <b>" + realColumn.KeyName + "</b> ist mit dieser Änderung fehlerhaft:<br><b>" + realColumn.ErrorReason() + "</b><br><br>Bitte im Spalten-Editor korrigieren.", ImageCode.Warnung, "Ok");

        //realColumn.Repair();
        using var editor = new ColumnEditor(realColumn, null);
        editor.ShowDialog();
        realColumn.Repair();

        _internalCellSet = true;
        try {
            var ot = row.Table;
            foreach (var key in OverviewKeys) {
                if (ot?.Column[key] is not { IsDisposed: false } oc) { continue; }
                row.CellSet(oc, OverviewValue(realColumn, key), "Spaltenübersicht: Zeile aufgefrischt");
            }
        } finally {
            _internalCellSet = false;
        }
    }

    /// <summary>
    /// Schreibt bearbeitete Zellwerte in die echte Spalte; weist ungültige Eingaben zurück
    /// und öffnet bei fehlerhafter Spalte den Spalten-Editor.
    /// </summary>
    private void SpaltenTabelle_CellValueChanged(object? sender, CellEventArgs e) {
        if (_internalCellSet) { return; }
        if (!_columnByRowKey.TryGetValue(e.Row.KeyName, out var realColumn) || realColumn is not { IsDisposed: false }) { return; }

        var newValue = e.Row.CellGetString(e.Column);
        var key = e.Column.KeyName;

        _internalCellSet = true;
        try {
            if (!ApplyEdit(realColumn, key, newValue)) {
                e.Row.CellSet(e.Column, OverviewValue(realColumn, key), "Spaltenübersicht: Eingabe zurückgewiesen");
                return;
            }

            // Normalisiertes Ergebnis (z. B. Großschreibung des KeyNamens) in die Zelle nachziehen
            e.Row.CellSet(e.Column, OverviewValue(realColumn, key), "Spaltenübersicht: Wert übernommen");
        } finally {
            _internalCellSet = false;
        }

        if (!realColumn.IsOk()) { RepairWithEditor(realColumn, e.Row); }
    }

    #endregion
}