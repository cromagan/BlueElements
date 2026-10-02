// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using BlueControls.Editoren;
using BlueControls.EventArgs;
using BlueControls.Renderer;
using BlueScript.ScriptVariables;
using BlueTable.ClassesStatic;
using BlueTable.ColumnFormats;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SpellDictionary = BlueControls.ClassesStatic.Dictionary;

namespace BlueControls.BlueTableDialogs;

public sealed partial class TableHeadEditor : FormWithStatusBar, IHasTable, IIsEditor {

    #region Fields

    private bool _frmHeadEditorFormClosingIsin;

    private bool _writeAccessLost;

    #endregion

    #region Constructors

    public TableHeadEditor() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        Table = null;
    }

    #endregion

    #region Properties

    public Type? EditorFor => null;

    public object? InputItem {
        get => Table;
        set => Table = value as Table;
    }

    public EditorMode Mode { get; set; } = EditorMode.EditItem;
    public EditorMode SupportedModes => EditorMode.EditItem;

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            field?.Disposed -= _table_Disposed;
            field?.WriteAccessChanged -= _table_WriteAccessChanged;
            field = value;

            field?.Disposed += _table_Disposed;
            field?.WriteAccessChanged += _table_WriteAccessChanged;
        }
    }

    public bool UndoDone { get; set; }

    #endregion

    #region Methods

    public static void AddUndosToTable(TableViewWithFilters tblUndo, Table? table, float maxAgeInDays) {
        if (table is { IsDisposed: false } tb) {
            Develop.Message(ErrorType.Info, null, "?", ImageCode.Information, $"Erstelle Tabellen Ansicht des Undo-Speichers der Tabelle '{tb.Caption}'", 0);

            List<UndoItem> un = [.. tb.Undo]; // Kann und wird verändert!

            foreach (var thisUndo in un) {
                AddUndoToTable(tblUndo, thisUndo, tb, maxAgeInDays);
            }
        }
    }

    public static void AddUndoToTable(TableViewWithFilters tblUndo, UndoItem work, Table db, float maxAgeInDays) {
        if (maxAgeInDays > 0 && DateTime.UtcNow.Subtract(work.DateTimeUtc).TotalDays > maxAgeInDays) { return; }
        var r = tblUndo.Table?.Row.GenerateAndAdd(work.ParseableItems().FinishParseable(), "New Undo Item");
        if (r is null) { return; }

        r.CellSet("ColumnKey", work.ColName, string.Empty);
        r.CellSet("RowKey", work.RowKey, string.Empty);
        if (db.Column[work.ColName] is { IsDisposed: false } col) {
            r.CellSet("columnCaption", col.Caption, string.Empty);
        }
        if (db.Row.GetByKey(work.RowKey) is { IsDisposed: false } row) {
            r.CellSet("RowFirst", row.CellFirstString(), string.Empty);
        } else if (!string.IsNullOrEmpty(work.RowKey)) {
            r.CellSet("RowFirst", "[gelöscht]", string.Empty);
        }
        r.CellSet("Aenderer", work.User, string.Empty);
        r.CellSet("AenderZeit", work.DateTimeUtc, string.Empty);
        r.CellSet("Kommentar", work.Comment, string.Empty);

        r.CellSet("Table", db.Caption, string.Empty);

        if (work.Container.IsValidFilepathAndName()) {
            r.CellSet("Herkunft", work.Container.FileNameWithoutSuffix(), string.Empty);
        }

        var symb = ImageCode.Fragezeichen;
        var alt = work.PreviousValue;
        var neu = work.ChangedTo;

        switch (work.Command) {
            case TableDataType.UTF8Value_withoutSizeData:
                symb = ImageCode.Stift;
                break;

            case TableDataType.TableVariables:
                alt = "[Variablen alt]";
                neu = "[Variablen neu]";
                symb = ImageCode.Variable;
                break;

            case TableDataType.EventScript:
                alt = "[Skript alt (" + alt.Length + " Zeichen)]";
                neu = "[Skript neu (" + neu.Length + " Zeichen)]";
                symb = ImageCode.Skript;
                break;

            //case TableDataType.EventScriptEdited:
            //    alt = "[Skript alt (" + alt.Length + " Zeichen)]";
            //    neu = "[Skript neu (" + neu.Length + " Zeichen)]";
            //    symb = ImageCode.Skript;
            //    break;

            case TableDataType.Command_AddRow:
                symb = ImageCode.PlusZeichen;
                break;

            case TableDataType.ColumnArrangement:
                symb = ImageCode.Spalte;
                alt = "[Spaltenanordnung alt]";
                neu = "[Spaltenanordnung neu]";
                break;

            case TableDataType.Command_RemoveRow:
                symb = ImageCode.MinusZeichen;
                break;

            case TableDataType.Command_NewStart:
                symb = ImageCode.Abspielen;
                break;

            case TableDataType.ColumnSystemInfo:
                symb = ImageCode.Information;
                break;

            case TableDataType.TemporaryTableMasterTimeUTC:
                symb = ImageCode.Uhr;
                break;

            case TableDataType.TemporaryTableMasterUser:
                symb = ImageCode.Person;
                break;

            case TableDataType.TemporaryTableMasterMachine:
                symb = ImageCode.Monitor;
                break;

            case TableDataType.TemporaryTableMasterApp:
                symb = ImageCode.Anwendung;
                break;

            case TableDataType.TemporaryTableMasterId:
                symb = ImageCode.Formel;

                break;
        }
        r.CellSet("Aenderung", work.Command.ToString(), string.Empty);
        r.CellSet("symbol", symb + "|24", string.Empty);
        r.CellSet("Wertalt", alt, string.Empty);
        r.CellSet("Wertneu", neu, string.Empty);
    }

    public static void GenerateUndoTabelle(TableViewWithFilters tblUndo) {
        var tb = Table.Get();
        //_ = x.Column.GenerateAndAdd("hidden", "hidden", ColumnFormats.TextOneLine.Instance);
        if (tb.Column.GenerateAndAdd("ID", "ID", TextOneLineColumnFormat.Instance) is { } f) {
            f.IsFirst = true;
        }
        tb.Column.GenerateAndAdd("Table", "Tabelle", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("ColumnKey", "Spalten-<br>Name<br>(Schlüssel)", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("ColumnCaption", "Spalten-<br>Beschriftung", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("RowKey", "Zeilen-<br>Schlüssel", LongOnlyPositiveColumnFormat.Instance);
        tb.Column.GenerateAndAdd("RowFirst", "Zeile, Wert der<br>1. Spalte", TextOneLineColumnFormat.Instance);
        var az = tb.Column.GenerateAndAdd("Aenderzeit", "Änder-<br>Zeit", DateTimeColumnFormat.Instance);
        tb.Column.GenerateAndAdd("Aenderer", "Änderer", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("Symbol", "Symbol", ImageAndTextColumnFormat.Instance);
        tb.Column.GenerateAndAdd("Aenderung", "Änderung", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("WertAlt", "Wert alt", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("WertNeu", "Wert neu", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("Kommentar", "Kommentar", TextOneLineColumnFormat.Instance);
        tb.Column.GenerateAndAdd("Herkunft", "Herkunft", TextOneLineColumnFormat.Instance);
        tb.Column.DisableAllEditing();
        foreach (var thisColumn in tb.Column) {
            if (!thisColumn.IsSystemColumn()) {
                thisColumn.MultiLine = true;
                thisColumn.DefaultRenderer = TextOneLineRenderer.ClassId;
            }
        }

        if (az is { IsDisposed: false }) {
            var o = new DateTimeRenderer {
                UTCToLocal = true,
                ShowSymbol = true
            };
            az.DefaultRenderer = o.MyClassId;
            az.RendererSettings = o.ParseableItems().FinishParseable();
        }

        if (tb.Column["Symbol"] is { IsDisposed: false } c) {
            var o = new ImageAndTextRenderer {
                Text_anzeigen = false,
                Bild_anzeigen = true
            };
            c.DefaultRenderer = o.MyClassId;
            c.RendererSettings = o.ParseableItems().FinishParseable();
        }

        tb.RepairAfterParse();

        var tcvc = ColumnViewCollection.ParseAll(tb);
        tcvc[1].ShowColumns("Table", "ColumnKey", "ColumnCaption", "RowKey", "RowFirst", "Aenderzeit", "Aenderer", "Symbol", "Aenderung", "WertAlt", "WertNeu", "Kommentar", "Herkunft");

        tb.ColumnArrangements = tcvc.AsReadOnly();

        //x.SortDefinition = new RowSortDefinition(db, "Index", true);

        tblUndo.Table = tb;
        tblUndo.Arrangement = string.Empty;
        tblUndo.ColumnMoveAllowed = false;
        tblUndo.SortDefinitionTemporary = new RowSortDefinition(tb, az, true);
    }

    public object? CreateNewItem() => null;

    protected override void OnFormClosing(FormClosingEventArgs e) {
        if (_frmHeadEditorFormClosingIsin) { return; }
        _frmHeadEditorFormClosingIsin = true;
        base.OnFormClosing(e);

        if (IsDisposed || Table is not { IsDisposed: false }) { return; }

        if (!_writeAccessLost) { WriteInfosBack(); }
        Table = null;
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        PermissionGroups_NewRow.ItemClear();
        PermissionGroups_NewRow.Suggestions.Clear();
        PermissionGroups_NewRow.ItemAddRange(TableView.Permission_AllUsed(false));
        PermissionGroups_NewRow.Check(tb.PermissionGroupsNewRow, true);

        lbxTableAdmin.ItemClear();

        // GlobalShowPass ist nur bei TableFile erlaubt (Persistierung im Main-Chunk).
        var isTableFile = tb is TableFile;
        txbKennwort.Enabled = isTableFile;
        txbKennwort.Set(isTableFile ? tb.GlobalShowPass : string.Empty, () => tb.GlobalShowPass);

        rowSortDefinitionEditor.InputItem = tb.SortDefinition;

        txbTags.Set(string.Join('\r', tb.Tags), () => tb.Tags);

        txbCaption.Set(() => tb.Caption);
        txbAssetFolder.Set(() => tb.AssetFolder);
        txbSymbolFolder.Set(() => tb.SymbolFolder);
        txbStandardFormulaFile.Set(() => tb.StandardFormulaFile);
        txbZeilenQuickInfo.Set(tb.RowQuickInfo.Replace("<br>", "\r"), () => tb.RowQuickInfo);
        txbZeilenQuickInfo.SuggestionPosition = SuggestionPosition.ContextMenuOnly;
        txbZeilenQuickInfo.Suggestions = tb.Column.Where(c => !c.IsDisposed).Select(c => $"~{c.KeyName}~").ToList().AsReadOnly();

        lbxTableAdmin.Suggestions.Clear();
        lbxTableAdmin.ItemAddRange(TableView.Permission_AllUsed(false));
        lbxTableAdmin.Check(tb.TableAdmin, true);

        lbxCliRights.ItemClear();
        lbxCliRights.ItemAddRange(
        [
            ItemOf("Zeile erstellen", CliRights.AddRow, false, "Zeile erstellen"),
            ItemOf("Zeile löschen", CliRights.DeleteRow, false, "Zeile löschen"),
            ItemOf("Zeilen verschieben", CliRights.MoveRows, false, "Zeilen verschieben"),
            ItemOf("Zellwerte ändern", CliRights.ChangeCellValues, false, "Zellwerte ändern"),
            ItemOf("Zeilenlock entfernen", CliRights.RemoveRowLock, false, "Zeilenlock entfernen"),
            ItemOf("Skript ändern", CliRights.EditScript, false, "Skript ändern"),
            ItemOf("Skript ausführen", CliRights.ExecuteScript, false, "Skript ausführen"),
            ItemOf("Spalte erstellen", CliRights.AddColumn, false, "Spalte erstellen"),
            ItemOf("Spalte löschen", CliRights.DeleteColumn, false, "Spalte löschen"),
            ItemOf("Spaltenanordnung ändern", CliRights.ChangeColumnArrangement, false, "Spaltenanordnung ändern"),
            ItemOf("Tabellenkopf ändern", CliRights.EditTableHead, false, "Tabellenkopf ändern")
        ]);
        lbxCliRights.Check(tb.CliRights, true);

        variableEditor.InputItem = Table?.Variables;

        uniqueValueDefinitionEditor.Table = Table;
        lstUniqueValues.Editor = uniqueValueDefinitionEditor;
        lstUniqueValues.InputItem = Table?.UniqueValues;

        txbDictionary.Set(string.Join('\r', tb.DictionaryWords), () => tb.DictionaryWords);

        GenerateInfoText();
    }

    private static List<string> ExtractWordsFromTable(Table tb) {
        var words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in tb.Row) {
            if (row.IsDisposed) { continue; }
            foreach (var column in tb.Column) {
                if (column.IsDisposed || !column.SpellCheckingEnabled) { continue; }
                var cellText = row.CellGetString(column);
                if (string.IsNullOrEmpty(cellText)) { continue; }

                var plainText = column.TextFormatingAllowed ? cellText.HtmlToPlain() : cellText;

                foreach (Match match in WordPatternRegex().Matches(plainText)) {
                    if (match.Length > 1 && !SpellDictionary.ContainsWord(match.Value)) {
                        words.Add(match.Value);
                    }
                }
            }
        }

        return UnknownWords([.. words]);
    }

    /// <summary>
    /// Vergleicht die bestehenden Unique-Definitionen mit der neuen Arbeitskopie.
    /// </summary>
    private static bool UniquesChanged(ReadOnlyCollection<UniqueValueDefinition> oldDefinitions, List<UniqueValueDefinition> newDefinitions) {
        HashSet<string> oldKeys = [.. oldDefinitions.Select(x => x.KeyName)];
        HashSet<string> newKeys = [.. newDefinitions.Select(x => x.KeyName)];
        return !oldKeys.SetEquals(newKeys);
    }

    /// <summary>
    /// Liefert die bereinigte Arbeitskopie der Unique-Definitionen aus dem Editor.
    /// </summary>
    private List<UniqueValueDefinition> WorkingCopyUniques() => (lstUniqueValues.OutputItem ?? [])
        .Cast<UniqueValueDefinition>()
        .Where(x => x.KeyColumns.Count > 0)
        .ToList();

    private static List<string> UnknownWords(IEnumerable<string> words) {
        var result = words.Where(w => !SpellDictionary.ContainsWord(w)).ToList();
        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }

    private void _table_Disposed(object? sender, System.EventArgs e) {
        Table = null;
        Close();
    }

    private void _table_WriteAccessChanged(object? sender, WriteAccessChangedEventArgs e) {
        if (e.IsEditable || _writeAccessLost || IsDisposed) { return; }
        _writeAccessLost = true;
        Notification.Show("Tabellen-Einstellungen werden geschlossen:<br>Schreibrechte fehlen (" + e.Reason + ")", ImageCode.Warnung);
        Close();
    }

    private void btnExtractWords_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        txbDictionary.Text = string.Join('\r', ExtractWordsFromTable(tb));
    }

    private void btnFormularBearbeiten_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (string.IsNullOrWhiteSpace(txbStandardFormulaFile.Text)) {
            var newFormulaFile = IO.TempFile(tb.AssetFolderWhole(), tb.KeyName, "cfo");
            IO.WriteAllText(newFormulaFile, string.Empty, Win1252, false);
            txbStandardFormulaFile.Text = newFormulaFile;
        }

        using var x = new ConnectedFormulaEditor(txbStandardFormulaFile.Text, null);

        if (x.IsClosed || x.IsDisposed) { return; }

        x.ShowDialog();
    }

    private void btnLoadAll_Click(object sender, System.EventArgs e) {
        if (Table is not { IsDisposed: false }) { return; }
        Table.LoadTableRows(false, -1);
    }

    private void btnMasterMe_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        tb.BeSureToBeUpToDate(false);
        tb.MasterMe();
        tb.BeSureToBeUpToDate(false);
        Close();
    }

    private void btnOptimize_Click(object sender, System.EventArgs e) => Table?.Optimize();

    private void btnSkripte_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        IUniqueWindowExtension.ShowOrCreate<TableScriptEditorForm>(tb);
        //var se = new_TableScriptEditor(db);
        //_ = se.ShowDialog();
    }

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Table?.Column.GenerateOverView();

    private void btnTabellenAnsicht_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        var c = new TableViewForm(tb, false, true);
        c.ShowDialog();
    }

    private void btnUniqueAufräumen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        // Die Arbeitskopie zuerst ins Backend zurückschreiben, damit die
        // Duplikat-Suche die aktuellen (ggf. im Dialog geänderten) Definitionen nutzt.
        tb.UniqueValues = WorkingCopyUniques().AsReadOnly();

        RepairUniquesInteractive();
    }

    private void btnUnMaster_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        tb.BeSureToBeUpToDate(false);
        (tb as TableFile)?.UnMasterMe();
        tb.BeSureToBeUpToDate(false);
    }

    private void butSystemspaltenErstellen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        tb.Column.GenerateAndAddSystem();
        tb.RepairAfterParse();
    }

    private void GenerateInfoText() {
        if (IsDisposed || Table is not { IsDisposed: false } tbl) {
            capInfo.Text = "Tabellen-Fehler";
            return;
        }

        var t = "<b>Tabelle:</b> <tab>" + tbl.KeyName + "<br>";
        t += "<b>Zeilen:</b> <tab>" + (tbl.Row.Count - 1) + "<br>";
        t += $"<b>Temporärer Master:</b>  <tab>{tbl.TemporaryTableMasterTimeUtc} {tbl.TemporaryTableMasterUser} {tbl.TemporaryTableMasterMachine}<br>";

        t += "<b>Letzte Speicherung der Hauptdatei:</b> <tab>" + tbl.LastSaveMainFileUtcDate.ToString7() + " UTC<br>";

        capInfo.Text = t.TrimEnd("<br>");
    }

    private void GlobalTab_SelectedIndexChanged(object sender, System.EventArgs e) {
        if (GlobalTab.SelectedTab == tabUndo && !UndoDone) {
            UndoDone = true;
            GenerateUndoTabelle(tblUndo);
            if (tblUndo.Table is { IsDisposed: false } tb) {
                tb.SuppressEvents();
                try {
                    AddUndosToTable(tblUndo, Table, -1);
                } finally {
                    tb.ResumeEvents();
                }
            }
            tblUndo.Table?.Freeze("Nur Ansicht");
        }
    }

    private void lstUniqueValues_AddClicked(object? sender, AddItemEventArgs e) {
        // Das Form erzeugt das neue Element und übergibt es über Add direkt
        // an den Sender (den EditorForIEnumerable). Dieser übernimmt das
        // Hinzufügen zur Arbeitskopie, die Aktualisierung der Anzeige und die
        // Selektion. Das Backend wird beim Schließen über WriteInfosBack aus
        // der Arbeitskopie aktualisiert.
        if (Table is not { IsDisposed: false } tb) { return; }
        if (sender is not EditorForIEnumerable lst) { return; }

        lst.Add(new UniqueValueDefinition(tb, []));
    }

    private void OkBut_Click(object sender, System.EventArgs e) => Close();

    /// <summary>
    /// Räumt die Duplikate aller Unique-Definitionen auf — gleiche Logik wie im
    /// Zeilenbereinigungs-Dialog: Dupe-Suche per RowCollection.DuplicateGroups,
    /// dann Combine + RemoveYoungest. Bei mehr als 3 gleichen Zeilen wird
    /// nachgefragt.
    /// </summary>
    private void RepairUniquesInteractive() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        var error = string.Empty;

        // Combine/RemoveYoungest feuern pro Duplikat-Gruppe mehrere Events.
        // SuppressEvents bündelt alles, ResumeEvents macht am Ende einen
        // einzigen Aufbau. Die frühen `return` im Loop werden über finally
        // sicher wieder freigegeben.
        tb.SuppressEvents();
        try {
            foreach (var thisDef in tb.UniqueValues) {

                #region Spalten der Definition, inkl. Chunk-Spalte

                // Ohne Filter auf die Chunk-Spalte liefert die Dupe-Suche bei
                // gechunkten Tabellen keine Zeilen (siehe RowCleanUp).
                var keyColumns = thisDef.KeyColumns.Where(c => c is { IsDisposed: false }).ToList();
                if (keyColumns.Count == 0) { continue; }
                var columns = new List<ColumnItem>(keyColumns);
                if (tb.Column.ChunkValueColumn is { IsDisposed: false } chk && !columns.Contains(chk)) { columns.Add(chk); }

                #endregion

                foreach (var rows in tb.Row.DuplicateGroups(columns, null)) {
                    if (rows.Count > 5) {
                        var t = $"Der Wert <b>{string.Join("; ", keyColumns.Select(c => rows[0].CellGetString(c)))}</b> ist in der Definition <b>{thisDef.ReadableText()}</b> {rows.Count}x vorhanden.<br>Reparatur fortsetzen?";
                        if (Forms.MessageBox.Show(t, ImageCode.Warnung, "Weiter", "Abbrechen") != 0) { return; }
                    }

                    error = tb.Row.Combine(rows).FailedReason;
                    if (string.IsNullOrEmpty(error)) { error = tb.Row.RemoveYoungest(rows, true).FailedReason; }

                    if (!string.IsNullOrEmpty(error)) {
                        Forms.MessageBox.Show($"Abbruch:<br>{error}", ImageCode.Warnung, "OK");
                        return;
                    }
                }
            }
        } finally {
            tb.ResumeEvents();
        }
    }

    private void WriteInfosBack() {
        if (TableViewForm.EditableErrorMessage(Table, null) || Table is not { IsDisposed: false }) { return; }

        //eventScriptEditor.WriteScriptBack();
        // GlobalShowPass wird nur bei TableFile zurückgeschrieben (siehe TableChunk.GenerateMainChunk).
        if (Table is TableFile) { Table.GlobalShowPass = txbKennwort.Text; }
        Table.Caption = txbCaption.Text;
        //Table.UndoCount = txbUndoAnzahl.Text.IsLong() ? Math.Max(IntParse(txbUndoAnzahl.Text), 5) : 5;
        //if (txbGlobalScale.Text.IsDouble()) {
        //    Table.GlobalScale = Math.Min(FloatParse(txbGlobalScale.Text), 5);
        //    Table.GlobalScale = Math.Max(0.5f, Table.GlobalScale);
        //}
        Table.AssetFolder = txbAssetFolder.Text;
        Table.SymbolFolder = txbSymbolFolder.Text;
        Table.StandardFormulaFile = txbStandardFormulaFile.Text;
        Table.RowQuickInfo = txbZeilenQuickInfo.Text.Replace("\r", "<br>");

        Table.Tags = new(txbTags.Text.SplitAndCutByCr());

        Table.TableAdmin = new(lbxTableAdmin.Checked);

        Table.CliRights = new(lbxCliRights.Checked);

        var tmp = PermissionGroups_NewRow.Checked.ToList();
        tmp.Remove(Administrator);
        Table.PermissionGroupsNewRow = new(tmp);

        Table.SortDefinition = ((IIsEditor)rowSortDefinitionEditor).OutputItem as RowSortDefinition;

        #region UniqueValues aufräumen

        // Arbeitskopie (OutputItem) ins Backend übernehmen. Definitionen
        // ohne Schlüsselspalten werden herausgefiltert.
        var neueDefinitions = WorkingCopyUniques();

        var uniquesChanged = UniquesChanged(Table.UniqueValues, neueDefinitions);

        Table.UniqueValues = neueDefinitions.AsReadOnly();

        // Hat der Benutzer die Unique-Definitionen verändert, werden die
        // Zeilen-Duplikate aller Definitionen repariert.
        if (uniquesChanged) { RepairUniquesInteractive(); }

        #endregion

        #region Wörterbuch

        var dictWords = UnknownWords(txbDictionary.Text.SplitAndCutByCr());
        Table.DictionaryWords = new(dictWords);

        #endregion

        #region Variablen

        // Arbeitskopie (OutputItem) ins Backend übernehmen. Früher geschah das
        // bei jeder Änderung über das ItemsModified-Event — jetzt zentral beim
        // Schließen. TableScriptEditor besitzt keinen Variablen-Editor und
        // schreibt Table.Variables nicht zurück.
        if (((IIsEditor)variableEditor).OutputItem is VariableCollection vl) {
            Table.Variables = vl;
        }

        #endregion
    }

    #endregion
}