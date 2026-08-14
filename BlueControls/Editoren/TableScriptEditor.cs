// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.Editoren;
using BlueControls.EventArgs;
using BlueScript.Classes;
using BlueScript.EventArgs;
using BlueTable.Interfaces;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.IO;

namespace BlueControls.BlueTableDialogs;

/// <summary>
/// Editor für genau ein Tabellen-Skript (<see cref="TableScriptDescription"/>).
/// Erbt die Skript-Bearbeitung von <see cref="ScriptEditor"/> und wird zusammen mit einem
/// <see cref="EditorForIEnumerable"/> im <see cref="TableScriptEditorForm"/> gehostet.
/// <para />
/// Der Editor ist ein reines Anzeige-Tool: er hält das aktuell gewählte
/// <see cref="TableScriptDescription"/> in seiner Oberfläche. Sobald der Nutzer eine
/// Eingabe tätigt, feuert er <see cref="INotifyPropertyChanged.PropertyChanged"/>
/// (jeweils für <c>OutputItem</c>). Das <see cref="EditorForIEnumerable"/>
/// holt sich daraufhin über <see cref="IIsEditor.OutputItem"/> eine frische, vom
/// Editor aus der aktuellen Oberfläche erzeugte Instanz (EditCopy-Modus), ersetzt
/// das Element in seiner Arbeitskopie und benachrichtigt das hostende
/// <see cref="TableScriptEditorForm"/>. Das Form schreibt die Collection ans Backend
/// zurück — so entstehen saubere Undo-Einträge (old≠new). Die anzuzeigenden Elemente
/// liefert die Backend-Collection <see cref="Table.EventScript"/> direkt als
/// <see cref="EditorForIEnumerable.InputItem"/>.
/// Die Tabellen-Verwaltung (Lebenszyklus, Schreibrechte) übernimmt das hostende Form.
/// </summary>
public sealed partial class TableScriptEditor : ScriptEditor, IHasTable {

    #region Fields

    private bool _allowTemporay;
    private Controls.TextBox? _dropDownTarget;
    private TableScriptDescription? _item;
    private bool _loaded;

    #endregion

    #region Constructors

    public TableScriptEditor() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        tbcScriptEigenschaften.Enabled = false;
        UpdateChunkUiState();
    }

    #endregion

    #region Properties

    public override Type? EditorFor => typeof(TableScriptDescription);

    public override EditorMode SupportedModes => EditorMode.EditCopy;

    public Table? Table {
        get;
        set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            // Vor dem Wechseln/Clearing des Tables das aktuelle OutputItem
            // anfordern — falls ein EditorForIEnumerable den Editor hostet,
            // fängt dieser das Event ein und übernimmt ggf. gepufferte
            // Eingaben (z.B. den Skript-Text) ins Backend.
            if (field is not null) { OnPropertyChanged("Table"); }

            field?.CanDoScript -= Table_CanDoScript;
            field = value;
            field?.CanDoScript += Table_CanDoScript;

            UpdateChunkUiState();
        }
    }

    /// <summary>
    /// Speicherschlüssel pro Tabelle — alle Skripte einer Tabelle teilen sich die Variablen-Sets,
    /// sodass die Werte unabhängig vom gewählten Skript geladen werden können.
    /// Der Editor-Typ-Suffix stellt sicher, dass die Sets nicht mit anderen Editoren
    /// derselben Tabelle kollidieren.
    /// </summary>
    public override string? VariablesStorageKey => Table?.KeyName is { Length: > 0 } k ? k + "|TableScript" : null;

    #endregion

    #region Methods

    /// <summary>
    /// Erzeugt eine Liste von Chunk-Werten für das Dropdown-Menü.
    /// Chunk-IDs (Hash-Ordnernamen) können nicht verwendet werden, da
    /// <see cref="TableChunk.BeSureRowIsLoaded(string)"/> und
    /// <see cref="TableChunk.GetChunkId"/> einen Chunk-<b>Wert</b> erwarten
    /// und diesen erneut hashen würden.
    /// </summary>
    public static List<ListItem> BuildChunkDropdownItems(Table? table) {
        if (table is not TableChunk { IsDisposed: false } tc) { return []; }

        if (tc.Row.Count < 5) {
            tc.LoadTableRows(false, 10);
        }

        var chunkValues = tc.Row
            .Where(r => r is { IsDisposed: false } && !string.IsNullOrEmpty(r.ChunkValue))
            .Select(r => r.ChunkValue)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        var chunkItems = new List<ListItem>();
        foreach (var c in chunkValues) {
            chunkItems.Add(ItemOf(c, c));
        }

        return chunkItems;
    }

    /// <summary>
    /// Erzeugt eine Liste von Zeilen für das Dropdown-Menü, gefiltert nach dem Chunk-Wert.
    /// Bei einer <see cref="TableChunk"/> werden nur die Zeilen des angegebenen Chunks geladen.
    /// </summary>
    public static List<ListItem> BuildRowDropdownItems(Table? table, string chunkValue) {
        if (table is not { IsDisposed: false }) { return []; }

        IEnumerable<RowItem> rows = table.Row.Where(r => r is { IsDisposed: false });

        if (table is TableChunk tc && !string.IsNullOrEmpty(chunkValue)) {
            var chunkId = TableChunk.GetChunkId(tc, TableDataType.UTF8Value_withoutSizeData, chunkValue);
            rows = TableChunk.RowsOfChunk(tc, chunkId);
        }

        var rowList = rows.OrderBy(r => r.ReadableText()).ToList();

        var uni = false;

        if (table.Column.First is { } colum) {
            RowCollection.GetUniques(colum, table.Row.ToList(), out _, out var nichteinzigartig);
            uni = nichteinzigartig.Count == 0;
        }

        var items = new List<ListItem>();
        foreach (var r in rowList) {
            var caption = r.ReadableText();

            if (uni) {
                items.Add(ItemOf(caption, r.CellFirstString(), ImageCode.Zeile));
            } else {
                items.Add(ItemOf(caption, r.KeyName, ImageCode.Zeile));
            }
        }

        return items;
    }

    /// <summary>
    /// Zeigt ein Dropdown-Menü neben dem übergebenen Control an.
    /// </summary>
    public static FloatingInputBoxListBoxStyle? ShowScriptEditorDropDown(Control anchorControl, List<ListItem> items, string currentValue) {
        if (items.Count == 0) { return null; }

        items.Add(ItemOf("... und weitere", true));

        var screenPoint = anchorControl.PointToScreen(new Point(0, anchorControl.Height));
        var x = screenPoint.X;
        var y = screenPoint.Y;

        return FloatingInputBoxListBoxStyle.Show(items, CheckBehavior.SingleSelection,
             string.IsNullOrEmpty(currentValue) ? null : [currentValue],
             x, y, anchorControl.Width, anchorControl, false,
             ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, false);
    }

    public override void Clear() {
        _item = null;
        tbcScriptEigenschaften.Enabled = false;
        txbTestZeile.Enabled = false;
        chkReadOnly.Enabled = false;
        grpRow.Enabled = false;
        txbName.Text = string.Empty;
        cbxPic.Text = string.Empty;
        txbQuickInfo.Text = string.Empty;
        Script = string.Empty;
        StoppedTimeCount = 0;
        LastFailedReason = string.Empty;
        LastVariables = null;
        chkAuslöser_newrow.Checked = false;
        chkAuslöser_valuechanged.Checked = false;
        chkAuslöser_prepaireformula.Checked = false;
        chkAuslöser_valuechangedThread.Checked = false;
        chkAuslöser_export.Checked = false;
        chkAuslöser_deletingRow.Checked = false;
        capFehler.Text = string.Empty;
    }

    /// <summary>
    /// Erzeugt aus dem aktuellen UI-Zustand eine neue <see cref="TableScriptDescription"/>.
    /// Wird vom <see cref="EditorForIEnumerable"/> über
    /// <see cref="IIsEditor.OutputItem"/> abgefragt, sobald der Editor
    /// <c>OutputItem</c>-Änderung signalisiert. Nicht editierte Backend-Werte
    /// (AdminInfo, AverageRunTime, Table) werden vom geladenen <see cref="_item"/>
    /// übernommen.
    /// </summary>
    public override object? CreateNewItem() {
        if (_item is null || Table is not { IsDisposed: false } tb) { return null; }

        var scc = ScriptChangedByUser ? Math.Min(10, StoppedTimeCount) : StoppedTimeCount;
        ScriptChangedByUser = false;

        return new TableScriptDescription(
            tb,
            txbName.Text,
            Script,
            cbxPic.Text.TrimEnd("|16"),
            txbQuickInfo.Text,
            _item.AdminInfo,
            lstPermissionExecute.Checked.ToList().AsReadOnly(),
            ComputeEventTypes(),
            chkZeile.Checked,
            chkReadOnly.Checked,
            LastFailedReason,
            LastVariables,
            scc,
            _item.AverageRunTime
        );
    }

    /// <summary>
    /// Befüllt die ComboBox mit allen verfügbaren Bildern. Wird einmalig beim ersten Laden ausgeführt.
    /// </summary>
    protected override void InitializeComponentDefaultValues() {
        var im = QuickImage.Images();
        foreach (var thisIm in im) {
            cbxPic.ItemAdd(ItemOf(thisIm, thisIm, QuickImage.Get(thisIm, 16)));
        }
    }

    /// <summary>
    /// Befüllt die UI aus dem übergebenen <see cref="TableScriptDescription"/>-Skript.
    /// </summary>
    protected override bool SetValuesToFormula(object? toEdit) {
        if (toEdit is not TableScriptDescription value) { return true; }
        if (IsDisposed || Table is not { IsDisposed: false }) { return true; }

        // Eigene Test-Logik an den Editor binden (Basis-Delegate ignorieren,
        // TableScriptEditor kennt Tabellenkontext und Zeile).
        ExecuteScript = ExecuteScriptCore;

        tbcScriptEigenschaften.Enabled = true;
        txbName.Text = value.KeyName;
        txbQuickInfo.Text = value.QuickInfo;

        cbxPic.Text = value.Image;

        chkZeile.Checked = value.NeedRow;
        txbTestZeile.Enabled = value.NeedRow;
        grpRow.Enabled = value.NeedRow;
        chkReadOnly.Checked = value.ValuesReadOnly || TableScriptDescription.MustBeReadonly(value.EventTypes);
        chkReadOnly.Enabled = !TableScriptDescription.MustBeReadonly(value.EventTypes);
        chkAuslöser_newrow.Checked = value.EventTypes.HasFlag(ScriptEventTypes.InitialValues);
        chkAuslöser_valuechanged.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed);
        chkExtendend.Enabled = value.EventTypes.HasFlag(ScriptEventTypes.value_changed) || value.EventTypes == ScriptEventTypes.Ohne_Auslöser;
        chkAuslöser_valuechangedThread.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread);
        chkAuslöser_prepaireformula.Checked = value.EventTypes.HasFlag(ScriptEventTypes.prepare_formula);
        chkAuslöser_export.Checked = value.EventTypes.HasFlag(ScriptEventTypes.export);
        chkAuslöser_deletingRow.Checked = value.EventTypes.HasFlag(ScriptEventTypes.row_deleting);
        Script = value.Script;
        LastFailedReason = value.FailedReason;
        LastVariables = value.SavedVariables;
        StoppedTimeCount = value.StoppedTimeCount;
        lstPermissionExecute.ItemClear();
        var l = TableView.Permission_AllUsed(false).ToList();
        l.AddIfNotExists(Administrator);
        lstPermissionExecute.ItemAddRange(l);
        lstPermissionExecute.Check(value.UserGroups, true);
        lstPermissionExecute.Suggestions.Clear();

        _item = value;

        btnAnzeigen_Click(null, System.EventArgs.Empty);
        if (value.IsOk()) {
            capFehler.Text = "<imagecode=Häkchen|16> Keine Skript-Konflikte.";
        } else {
            capFehler.Text = "<imagecode=Warnung|16> " + value.ErrorReason();
        }

        if (value.StoppedTimeCount > 20) {
            capLaufzeit.Text = $"Geschätzte Laufzeit:  {Math.Round(value.AverageRunTime / 1000f, 2)} Sekunden";
        } else {
            capLaufzeit.Text = string.Empty;
        }

        return true;
    }

    /// <summary>
    /// Schreibt die Tabellen-spezifischen Werte (TestZeile, Chunk, Erweiterte Ausführung)
    /// zusätzlich zu den Basis-Feldern in das übergebene JsonObject.
    /// </summary>
    protected override JsonObject SpecialFieldsToVariables() {
        var fields = base.SpecialFieldsToVariables();
        fields[KeyTestZeile.ToUpperInvariant()] = txbTestZeile.Text ?? string.Empty;
        fields[KeyChunk.ToUpperInvariant()] = txbChunk.Text ?? string.Empty;
        fields[KeyExtendend.ToUpperInvariant()] = chkExtendend.Checked.ToPlusMinus();
        return fields;
    }

    /// <summary>
    /// Lädt die Tabellen-spezifischen Werte zusätzlich zu den Basis-Feldern aus dem
    /// übergebenen JsonObject. Fehlt ein Wert in <paramref name="data"/> oder passt er
    /// nicht zum aktuellen Skript-Kontext, wird das entsprechende Feld geleert.
    /// </summary>
    protected override void VariablesToSpecialField(JsonObject? data, bool clearMissing) {
        base.VariablesToSpecialField(data, clearMissing);

        // txbTestZeile: übernehmen, wenn ein Wert vorhanden ist UND die Zeile in der
        // aktuellen Tabelle existiert. Bei clearMissing sonst leeren, sonst unverändert lassen.
        var testZeileSet = false;
        if (data is not null && Table is { IsDisposed: false } tb) {
            // Versuche zuerst KeyTestZeile, falls nicht vorhanden oder leer, versuche keyRowKey
            if (!(data.TryGetPropertyValue(KeyTestZeile.ToUpperInvariant(), out var tzNode) && tzNode is JsonValue tzv && tzv.TryGetValue(out string? tz) && !string.IsNullOrEmpty(tz))) {
                data.TryGetPropertyValue(KeyInputRowKey.ToUpperInvariant(), out tzNode);
            }

            if (tzNode is JsonValue finalTzv && finalTzv.TryGetValue(out string? finalTz) && !string.IsNullOrEmpty(finalTz)) {
                var r = tb.Row[finalTz] ?? tb.Row.GetByKey(finalTz);
                if (r is { IsDisposed: false }) {
                    txbTestZeile.Text = finalTz;
                    testZeileSet = true;
                }
            }
        }
        if (!testZeileSet && clearMissing) {
            txbTestZeile.Text = string.Empty;
        }

        // txbChunk: übernehmen, falls vorhanden. Bei clearMissing sonst leeren, sonst unverändert lassen.
        if (data is not null
            && data.TryGetPropertyValue(KeyChunk.ToUpperInvariant(), out var chNode)
            && chNode is JsonValue chv
            && chv.TryGetValue(out string? ch)) {
            txbChunk.Text = ch ?? string.Empty;
        } else if (clearMissing) {
            txbChunk.Text = string.Empty;
        }

        // chkExtendend: nur behandeln, wenn das Feld im aktuellen Skript-Kontext aktiviert ist.
        if (chkExtendend.Enabled) {
            if (data is not null
                && data.TryGetPropertyValue(KeyExtendend.ToUpperInvariant(), out var exNode)
                && exNode is JsonValue exv
                && exv.TryGetValue(out string? ex)) {
                chkExtendend.Checked = ex?.FromPlusMinus() ?? false;
            }
        }
    }

    private void btnChunkDropDown_Click(object sender, System.EventArgs e) {
        if (Table is not { IsDisposed: false }) { return; }
        var items = BuildChunkDropdownItems(Table);

        _dropDownTarget = txbChunk;
        var dropDown = ShowScriptEditorDropDown(btnChunkDropDown, items, txbChunk.Text);
        dropDown?.ItemClicked += ScriptEditorDropDown_ItemClicked;
    }

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Table?.Column.GenerateOverView();

    private void btnTabelleKopf_Click(object sender, System.EventArgs e) => InputBoxEditor.Edit(Table, typeof(TableHeadEditor), false);

    private void btnTest_Click(object sender, System.EventArgs e) {
        if (!_loaded && Table is { Row.Count: 0 }) {
            _loaded = true;
            if (Table is TableChunk && !string.IsNullOrEmpty(txbChunk.Text)) {
                Table.BeSureRowIsLoaded(txbChunk.Text);
            } else {
                Table.LoadTableRows(false, 5);
            }
        }

        TesteScript(true);
    }

    private void btnTestZeileDropDown_Click(object sender, System.EventArgs e) {
        if (Table is not { IsDisposed: false }) { return; }
        var items = BuildRowDropdownItems(Table, txbChunk.Text);

        _dropDownTarget = txbTestZeile;
        var dropDown = ShowScriptEditorDropDown(btnTestZeileDropDown, items, txbTestZeile.Text);
        dropDown?.ItemClicked += ScriptEditorDropDown_ItemClicked;
    }

    private void btnVersionErhöhen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        btnVersionErhöhen.Enabled = false;

        Table.EventScriptVersion = DateTime.UtcNow;
    }

    private void btnZusatzDateien_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        ExecuteFile(tb.AssetFolderWhole());
    }

    private void cbxPic_TextChanged(object sender, System.EventArgs e) {
        if (_item is null) { return; }
        OnPropertyChanged("Picture");
    }

    private void chkAuslöser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        if (_item is null) { return; }
        OnPropertyChanged("Trigger");
    }

    private void chkReadOnly_CheckedChanged(object sender, System.EventArgs e) {
        if (_item is null) { return; }
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        OnPropertyChanged("ReadOnly");
    }

    private void chkZeile_CheckedChanged(object sender, System.EventArgs e) {
        if (_item is null) { return; }
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (chkZeile.Checked && !Table.IsRowScriptPossible()) {
            if (!EnableScript()) { chkZeile.Checked = false; }

            return;
        }

        OnPropertyChanged("RowScript");
        txbTestZeile.Enabled = chkZeile.Checked;
        grpRow.Enabled = chkZeile.Checked;
    }

    private ScriptEventTypes ComputeEventTypes() {
        ScriptEventTypes tmp = 0;
        if (chkAuslöser_newrow.Checked) { tmp |= ScriptEventTypes.InitialValues; }
        if (chkAuslöser_valuechanged.Checked) { tmp |= ScriptEventTypes.value_changed; }
        if (chkAuslöser_prepaireformula.Checked) { tmp |= ScriptEventTypes.prepare_formula; }
        if (chkAuslöser_valuechangedThread.Checked) { tmp |= ScriptEventTypes.value_changed_extra_thread; }
        if (chkAuslöser_export.Checked) { tmp |= ScriptEventTypes.export; }
        if (chkAuslöser_deletingRow.Checked) { tmp |= ScriptEventTypes.row_deleting; }
        return tmp;
    }

    private bool EnableScript() {
        if (IsDisposed || Table is not { IsDisposed: false }) { return false; }

        var s = Forms.MessageBox.Show("Für Zeilenskripte werden bestimmte Systemspalten benötigt.<br>Sollen diese erstellt werden?", ImageCode.Spalte, "Ja", "Nein");

        if (s == 1) { return false; }

        Table.EnableScript();

        if (!Table.IsRowScriptPossible()) {
            Forms.MessageBox.Show("Systemspalten konnten nicht erstellt werden.", ImageCode.Information, "Ok");
            return false;
        }
        return true;
    }

    private ScriptEndedFeedback ExecuteScriptCore(string script, bool testmode) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) {
            return new ScriptEndedFeedback("Keine Tabelle geladen.", false, false, "Allgemein");
        }

        if (_item is null) {
            return new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
        }

        //if (!_item.IsOk()) {
        //    return new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, false, "Allgemein");
        //}

        // Flush (OnPropertyChanged(OutputItem)) und Host-Benachrichtigung (Executing) werden
        // bereits in der Basis (ScriptEditor.TesteScript) vor diesem Aufruf
        // erledigt: Das hostende Form hat OutputItem ins Backend geschrieben und
        // _item auf die frische Backend-Instanz aktualisiert.

        RowItem? r = null;

        if (_item.NeedRow) {
            if (tb.Row.Count == 0) {
                return new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.", false, false, "Allgemein");
            }

            if (string.IsNullOrEmpty(txbTestZeile.Text)) {
                txbTestZeile.Text = tb.Row.First()?.CellFirstString() ?? string.Empty;
            }

            if (string.IsNullOrEmpty(txbTestZeile.Text)) {
                txbTestZeile.Text = tb.Row.First()?.KeyName ?? string.Empty;
            }

            r = tb.Row[txbTestZeile.Text] ?? tb.Row.GetByKey(txbTestZeile.Text);
            if (r is not { IsDisposed: false }) {
                return new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
            }
        }
        var produktiv = !testmode;

        if (produktiv) {
            if (Forms.MessageBox.Show("Skript ändert Werte!<br>Fortfahren?", ImageCode.Warnung, "Fortfahren", "Abbruch") != 0) {
                return new ScriptEndedFeedback("Abbruch.", false, false, "Allgemein");
            }
        }

        var ext = chkExtendend is { Checked: true, Enabled: true };

        _allowTemporay = true;
        var f = tb.ExecuteScript(_item, produktiv, r, GetParseArgs(), true, ext, true);
        _allowTemporay = false;

        return f;
    }

    private void lstPermissionExecute_ItemClicked(object sender, ListItemEventArgs e) {
        if (_item is null) { return; }
        OnPropertyChanged("Permission");
    }

    private void ScriptEditorDropDown_ItemClicked(object? sender, ListItemEventArgs e) {
        if (_dropDownTarget is { IsDisposed: false } tbx && e.Item is { } item) {
            tbx.Text = item.KeyName;
        }
    }

    private void Table_CanDoScript(object? sender, CanDoScriptEventArgs e) {
        // Während der Editor geöffnet ist, werden Skripte der gleichen Tabelle
        // blockiert - außer bei der Test-Ausführung durch den Editor selbst.
        if (_allowTemporay) { return; }
        e.CancelReason = "Skript-Editor geöffnet";
    }

    private void txbChunk_TextChanged(object sender, System.EventArgs e) {
        // Bei TableChunk: Zeilen-Dropdown erst freigeben, wenn ein Chunk gewählt ist.
        if (Table is TableChunk tc) {
            btnTestZeileDropDown.Enabled = !string.IsNullOrEmpty(txbChunk.Text);
            txbTestZeile.Enabled = btnTestZeileDropDown.Enabled;

            if (tc.Row.Count == 0) { return; }
        } else {
            return;
        }

        if (string.IsNullOrEmpty(txbChunk.Text)) {
            var firstRow = tc.Row.First();
            txbChunk.Text = !string.IsNullOrEmpty(firstRow?.ChunkValue) ? firstRow.ChunkValue : firstRow?.KeyName ?? string.Empty;
        }

        if (!string.IsNullOrEmpty(txbChunk.Text)) { tc.BeSureRowIsLoaded(txbChunk.Text); }
    }

    private void txbName_TextChanged(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false }) { return; }
        if (_item is null) { return; }
        if (!ScriptDescription.IsValidName(txbName.Text)) { return; }
        if (string.Equals(txbName.Text, _item.KeyName, StringComparison.OrdinalIgnoreCase)) { return; }
        if (Table.EventScript.GetByKey(txbName.Text, StringComparison.OrdinalIgnoreCase) is not null) { return; }

        OnPropertyChanged("Name");
    }

    private void txbQuickInfo_TextChanged(object sender, System.EventArgs e) {
        if (_item is null) { return; }
        OnPropertyChanged("QuickInfo");
    }

    private void UpdateChunkUiState() {
        var isChunk = Table is TableChunk;
        txbChunk.Enabled = isChunk;
        capChunk.Enabled = isChunk;
        btnChunkDropDown.Enabled = isChunk;

        // Bei TableChunk muss zwingend erst ein Chunk gewählt werden,
        // bevor das Zeilen-Dropdown aktiv wird.
        btnTestZeileDropDown.Enabled = !isChunk || !string.IsNullOrEmpty(txbChunk.Text);
        txbTestZeile.Enabled = btnTestZeileDropDown.Enabled;
    }

    #endregion
}