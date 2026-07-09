// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using BlueControls.Editoren;
using BlueControls.EventArgs;
using BlueScript.Classes;
using BlueScript.EventArgs;
using BlueScript.Variables;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.IO;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueTableDialogs;

public sealed partial class TableScriptEditor : ScriptEditorGeneric, IHasTable, IIsEditor {

    #region Fields

    private bool _allowTemporay;

    private bool _didMessage;

    private Controls.TextBox? _dropDownTarget;
    private TableScriptDescription? _item;

    private bool _loaded;

    private bool _writeAccessLost;

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

    // EditorFor intentionally null — always invoked explicitly via EditItem(table, typeof(TableScriptEditor), isDialog)
    public Type? EditorFor => null;

    public object? InputItem {
        get => Table;
        set {
            Table = value as Table;
        }
    }

    public TableScriptDescription? Item {
        get => IsDisposed || Table is not { IsDisposed: false } ? null : _item;
        set {
            if (IsDisposed || Table is not { IsDisposed: false }) { value = null; }
            if (_item == value) { return; }

            _item = null; // Um keine werte zurück zu Schreiben während des Anzeigens

            if (value is not null) {
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

                btnVerlauf.Enabled = true;
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
            } else {
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
                btnVerlauf.Enabled = false;
                capFehler.Text = string.Empty;
            }
        }
    }

    public EditorMode Mode { get; set; } = EditorMode.EditItem;

    public override object? Object {
        get => ((IIsEditor)this).OutputItem;
        set => InputItem = value;
    }

    public EditorMode SupportedModes => EditorMode.EditItem; // EditCopy nicht möglich: Skripte werden inline in Tabelle bearbeitet

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            WriteInfosBack();
            lstEventScripts.UncheckAll();

            if (field is not null) {
                field.DisposingEvent -= _table_Disposing;
                field.CanDoScript -= Table_CanDoScript;
                field.Loaded -= Table_Loaded;
                field.WriteAccessChanged -= _table_WriteAccessChanged;
            }
            field = value;

            if (field is not null) {
                field.DisposingEvent += _table_Disposing;
                field.CanDoScript += Table_CanDoScript;
                field.Loaded += Table_Loaded;
                field.WriteAccessChanged += _table_WriteAccessChanged;

                tbcScriptEigenschaften.Enabled = true;
            } else {
                tbcScriptEigenschaften.Enabled = false;
            }

            UpdateList();
            UpdateChunkUiState();
        }
    }

    /// <summary>
    /// Speicherschlüssel pro Tabelle — alle Skripte einer Tabelle teilen sich die Variablen-Sets,
    /// sodass die Werte unabhängig vom gewählten Skript geladen werden können.
    /// Der Editor-Typ-Suffix stellt sicher, dass die Sets nicht mit anderen Skript-Editoren
    /// (z.B. RowAdderScriptEditor) derselben Tabelle kollidieren.
    /// </summary>
    public override string? VariablesStorageKey => Table?.KeyName is { Length: > 0 } k ? k + "|TableScript" : null;

    #endregion

    #region Methods

    /// <summary>
    /// Erzeugt eine Liste von Chunk-Werten für das Dropdown-Menü.
    /// Chunk-IDs (Hash-Ordnernamen) können nicht verwendet werden, da
    /// <see cref="TableChunk.BeSureRowIsLoaded(string"/> und
    /// <see cref="TableChunk.GetChunkId"/> einen Chunk-<b>Wert</b> erwarten
    /// und diesen erneut hashen würden.
    /// </summary>
    public static List<AbstractListItem> BuildChunkDropdownItems(Table? table) {
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

        var chunkItems = new List<AbstractListItem>();
        foreach (var c in chunkValues) {
            chunkItems.Add(ItemOf(c, c));
        }

        return chunkItems;
    }

    /// <summary>
    /// Erzeugt eine Liste von Zeilen für das Dropdown-Menü, gefiltert nach dem Chunk-Wert.
    /// Bei einer <see cref="TableChunk"/> werden nur die Zeilen des angegebenen Chunks geladen.
    /// </summary>
    public static List<AbstractListItem> BuildRowDropdownItems(Table? table, string chunkValue) {
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

        var items = new List<AbstractListItem>();
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
    public static FloatingInputBoxListBoxStyle? ShowScriptEditorDropDown(Control anchorControl, List<AbstractListItem> items, string currentValue) {
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

    public object? CreateNewItem() => null;

    public override ScriptEndedFeedback ExecuteScript(bool testmode) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) {
            return new ScriptEndedFeedback("Keine Tabelle geladen.", false, false, "Allgemein");
        }

        if (_item is null) {
            return new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
        }

        //if (!_item.IsOk()) {
        //    return new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, false, "Allgemein");
        //}

        WriteInfosBack();

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

    public void UpdateSelectedItem(string? keyName = null, string? quickInfo = null, string? image = null, bool? needRow = null, bool? readOnly = null, ScriptEventTypes? eventTypes = null, string? script = null, ReadOnlyCollection<string>? userGroups = null, string? adminInfo = null, string? failedReason = null, List<Variable>? savedVariables = null, bool isDisposed = false, int? stoppedtimecount = null, long? averageruntime = null) {
        if (IsDisposed || _writeAccessLost || Table is not { IsDisposed: false } tb || TableViewForm.EditableErrorMessage(tb, null)) { return; }

        if (_item is null) {
            capFehler.Text = string.Empty;
            return;
        }

        var tmpname = keyName ?? _item.KeyName;

        // Backend-Update
        tb.UpdateScript(_item.KeyName, keyName, script, image, quickInfo, adminInfo, eventTypes, needRow, userGroups, failedReason, savedVariables, isDisposed, readOnly, stoppedtimecount, averageruntime);
        UpdateList();

        Item = tb.EventScript.GetByKey(tmpname, StringComparison.OrdinalIgnoreCase);
    }

    public override void WriteInfosBack() {
        var scc = StoppedTimeCount;
        if (ScriptChangedByUser) {
            scc = Math.Min(10, StoppedTimeCount);
            ScriptChangedByUser = false;
        }

        UpdateSelectedItem(script: Script, keyName: txbName.Text, failedReason: LastFailedReason, savedVariables: LastVariables, stoppedtimecount: scc);
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        Item = null; // erst das Item!
        Table = null;
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        var im = QuickImage.Images();
        foreach (var thisIm in im) {
            cbxPic.ItemAdd(ItemOf(thisIm, thisIm, QuickImage.Get(thisIm, 16)));
        }
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

    private void _table_Disposing(object? sender, System.EventArgs e) {
        // _writeAccessLost verhindert WriteInfosBack-Zugriffe auf die
        // gerade verworfenen Tabelle während Table = null (Setter).
        _writeAccessLost = true;
        Table = null;
        // Close asynchron, damit die Disposing-Verarbeitung der Tabelle
        // nicht durch synchrone Form-Schließung unterbrochen wird.
        if (IsHandleCreated) { BeginInvoke(new Action(Close)); }
    }

    private void _table_WriteAccessChanged(object? sender, WriteAccessChangedEventArgs e) {
        if (e.IsEditable || _writeAccessLost || IsDisposed) { return; }
        _writeAccessLost = true;
        Forms.Notification.Show("Skript-Editor wird geschlossen:<br>Schreibrechte fehlen (" + e.Reason + ")", ImageCode.Warnung);
        // Close asynchron ausführen, um Re-Entrancy während der Event-
        // Verarbeitung der Tabelle (z.B. Freeze/OnWriteAccessChanged) zu
        // vermeiden. Ein synchrones Close würde Table = null im Setter
        // auslösen, während die Tabelle noch im Event-Invoke steckt.
        if (IsHandleCreated) { BeginInvoke(new Action(Close)); }
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

    private void btnVerlauf_Click(object sender, System.EventArgs e) {
        // Überprüfen, ob die Tabelle oder die Instanz selbst verworfen wurde
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        // Das ausgewählte Skript aus der Liste abrufen
        if (lstEventScripts.Checked.Count != 1) { return; }

        var selectedlstEventScripts = lstEventScripts[lstEventScripts.Checked[0]] is ReadableListItem item ? (TableScriptDescription)item.Item : null;
        var l = new List<string>();
        // Durchlaufen aller Undox-Operationen in der Tabelle

        var sortedUndoItems = tb.Undo.Where(item => item.Command is TableDataType.EventScript).OrderByDescending(item => item.DateTimeUtc);

        foreach (var thisUndo in sortedUndoItems) {
            l.Add("############################################################################");
            l.Add("############################################################################");
            l.Add("############################################################################");
            l.Add("############################################################################");
            l.Add("############################################################################");
            l.Add(thisUndo.DateTimeUtc.ToString7() + " " + thisUndo.User);

            l.Add("Art: " + thisUndo.Command);
            // Überprüfen, ob das Skript geändert wurde
            var found = false;
            foreach (var t in thisUndo.ChangedTo.SplitAndCutByCr()) {
                var s = new TableScriptDescription(tb, t);
                if (s.KeyName == selectedlstEventScripts?.KeyName && selectedlstEventScripts.Script != s.Script) {
                    l.Add(s.Script);
                    found = true;
                    break;
                }
            }
            if (!found) {
                l.Add("    -> Keine Änderung am gewählten Skript");
            }
        }
        // Schreiben der Liste in eine temporäre Datei
        l.WriteAllText(TempFile(string.Empty, "Scrip.txt"), Win1252, true);
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

    private void cbxPic_TextChanged(object sender, System.EventArgs e) => UpdateSelectedItem(image: cbxPic.Text.TrimEnd("|16"));

    private void chkAuslöser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        if (_item is null) { return; }

        ScriptEventTypes tmp = 0;
        if (chkAuslöser_newrow.Checked) { tmp |= ScriptEventTypes.InitialValues; }
        if (chkAuslöser_valuechanged.Checked) { tmp |= ScriptEventTypes.value_changed; }
        if (chkAuslöser_prepaireformula.Checked) { tmp |= ScriptEventTypes.prepare_formula; }
        if (chkAuslöser_valuechangedThread.Checked) { tmp |= ScriptEventTypes.value_changed_extra_thread; }
        if (chkAuslöser_export.Checked) { tmp |= ScriptEventTypes.export; }
        if (chkAuslöser_deletingRow.Checked) { tmp |= ScriptEventTypes.row_deleting; }

        UpdateSelectedItem(eventTypes: tmp);
    }

    private void chkReadOnly_CheckedChanged(object sender, System.EventArgs e) {
        if (_item is null) { return; }

        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        UpdateSelectedItem(readOnly: chkReadOnly.Checked);
    }

    private void chkZeile_CheckedChanged(object sender, System.EventArgs e) {
        if (_item is null) { return; }

        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (chkZeile.Checked && !Table.IsRowScriptPossible()) {
            if (!EnableScript()) { chkZeile.Checked = false; }

            return;
        }

        UpdateSelectedItem(needRow: chkZeile.Checked);
        txbTestZeile.Enabled = chkZeile.Checked;
        grpRow.Enabled = chkZeile.Checked;
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

    private void GlobalTab_SelectedIndexChanged(object sender, System.EventArgs e) => WriteInfosBack();

    private void lstEventScripts_AddClicked(object sender, AddItemEventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        var newitem = new TableScriptDescription(Table);

        if (tb.EventScript.GetByKey(newitem.KeyName) is not null) {
            QuickNote.Show(NoteSymbols.Warning, "Skript existiert bereits");
            return;
        }

        List<TableScriptDescription> l = [.. tb.EventScript];
        l.Add(newitem);
        tb.EventScript = l.AsReadOnly();

        UpdateList();
        lstEventScripts.Check(newitem.KeyName);
        e.Cancel = true;
    }

    private void lstEventScripts_ItemCheckedChanged(object sender, System.EventArgs e) {
        var newItem = string.Empty;

        if (lstEventScripts.Checked.Count == 1 && !_writeAccessLost) {
            if (TableViewForm.EditableErrorMessage(Table, null)) {
                // Chunk gesperrt — Editor schließen. _writeAccessLost verhindert
                // weitere Lock-Prüfungen (und MessageBoxen) während des Schließens.
                _writeAccessLost = true;
                if (IsHandleCreated) { BeginInvoke(new Action(Close)); }
                return;
            }

            if (lstEventScripts[lstEventScripts.Checked[0]] is ReadableListItem rli) {
                newItem = rli.KeyName;
            }
        }

        WriteInfosBack();

        Item = Table?.EventScript.GetByKey(newItem);
    }

    private void lstEventScripts_RemoveClicked(object sender, AbstractListItemEventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (e.Item is not ReadableListItem rli || rli.Item is not TableScriptDescription dsd) { return; }

        var toDelete = dsd.KeyName;

        if (dsd != _item) { WriteInfosBack(); }

        //lstEventScripts.UncheckAll();

        var toDel = tb.EventScript.GetByKey(toDelete);

        if (toDel is null) { return; }

        Table.UpdateScript(toDel, isDisposed: true);
        UpdateList();
    }

    private void lstPermissionExecute_ItemClicked(object sender, AbstractListItemEventArgs e) => UpdateSelectedItem(userGroups: lstPermissionExecute.Checked.ToList().AsReadOnly());

    private void ScriptEditorDropDown_ItemClicked(object? sender, AbstractListItemEventArgs e) {
        if (_dropDownTarget is { IsDisposed: false } tbx && e.Item is { } item) {
            tbx.Text = item.KeyName;
        }
    }

    private void Table_CanDoScript(object? sender, CanDoScriptEventArgs e) {
        if (_allowTemporay) { return; }
        e.CancelReason = "Skript-Editor geöffnet";
    }

    private void Table_Loaded(object? sender, FirstEventArgs e) {
        // Bei externen Aktualisierungen (Server-Sync, Undo/Redo) werden alle
        // TableScriptDescription-Objekte neu erstellt. Liste aktualisieren,
        // damit keine veralteten Referenzen mehr angezeigt werden.
        UpdateList();

        // Aktuell ausgewähltes Item auf das neue Objekt migrieren.
        if (_item is { IsDisposed: false } old && Table is { IsDisposed: false } tb) {
            var fresh = tb.EventScript.GetByKey(old.KeyName, StringComparison.OrdinalIgnoreCase);
            if (fresh is not null && !ReferenceEquals(fresh, old)) {
                Item = fresh;
            }
        }
    }

    private void txbChunk_TextChanged(object sender, System.EventArgs e) {
        // Bei TableChunk: Zeilen-Dropdown erst freigeben, wenn ein Chunk gewählt ist.
        if (Table is TableChunk) {
            btnTestZeileDropDown.Enabled = !string.IsNullOrEmpty(txbChunk.Text);
            txbTestZeile.Enabled = btnTestZeileDropDown.Enabled;
        }

        if (Table is not TableChunk || Table.Row.Count == 0) { return; }

        if (string.IsNullOrEmpty(txbChunk.Text)) {
            var firstRow = Table.Row.First();
            txbChunk.Text = !string.IsNullOrEmpty(firstRow?.ChunkValue) ? firstRow.ChunkValue : firstRow?.KeyName ?? string.Empty;
        }

        if (!string.IsNullOrEmpty(txbChunk.Text)) { Table.BeSureRowIsLoaded(txbChunk.Text); }
    }

    private void txbName_TextChanged(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }
        if (_item is null) { return; }

        if (!ScriptDescription.IsValidName(txbName.Text)) { return; }

        if (!string.Equals(txbName.Text, _item.KeyName, StringComparison.OrdinalIgnoreCase)) {
            if (tb.EventScript.GetByKey(txbName.Text, StringComparison.OrdinalIgnoreCase) is not null) { return; }
        }

        UpdateSelectedItem(keyName: txbName.Text);
    }

    private void txbQuickInfo_TextChanged(object sender, System.EventArgs e) => UpdateSelectedItem(quickInfo: txbQuickInfo.Text);

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

    private void UpdateList() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) {
            lstEventScripts.ItemClear();
            return;
        }

        lstEventScripts.UpdateList(tb.EventScript);

        foreach (var thisSet in tb.EventScript) {
            if (thisSet is null) { continue; }

            var cap = thisSet.EventTypes != 0 ? thisSet.EventTypes.ToString() : "Sonstige";

            if (lstEventScripts[thisSet.KeyName] is ReadableListItem rli) {
                rli.UserDefCompareKey = cap + SecondSortChar + thisSet.CompareKey;
            }

            if (lstEventScripts[cap] is null) {
                lstEventScripts.ItemAdd(ItemOf(cap, cap, true, cap + FirstSortChar));
            }

            if (!_didMessage && thisSet.NeedRow && !tb.IsRowScriptPossible()) {
                _didMessage = true;
                EnableScript();
            }
        }
    }

    #endregion
}