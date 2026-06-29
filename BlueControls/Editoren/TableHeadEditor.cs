// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using BlueControls.Editoren;
using BlueControls.Renderer;
using BlueScript.Variables;
using BlueTable.EventArgs;
using BlueTable.Interfaces;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SpellDictionary = BlueControls.Classes.Dictionary;

namespace BlueControls.BlueTableDialogs;

public sealed partial class TableHeadEditor : FormWithStatusBar, IHasTable, IIsEditor {

    #region Fields

    private bool _frmHeadEditorFormClosingIsin;

    private UniqueValueDefinition? _selectedUniqueValue;

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
        set {
            Table = value as Table;
        }
    }

    public EditorMode Mode { get; set; } = EditorMode.EditItem;
    public EditorMode SupportedModes => EditorMode.EditItem;

    public Table? Table {
        get;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            field?.DisposingEvent -= _table_Disposing;
            field?.WriteAccessChanged -= _table_WriteAccessChanged;
            field = value;

            field?.DisposingEvent += _table_Disposing;
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

        if (work.Container.IsFormat(FormatHolder_FilepathAndName.Instance)) {
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
        //_ = x.Column.GenerateAndAdd("hidden", "hidden", ColumnFormatHolder_TextOneLine.Instance);
        var f = tb.Column.GenerateAndAdd("ID", "ID", ColumnFormatHolder_TextOneLine.Instance);
        f.IsFirst = true;
        tb.Column.GenerateAndAdd("Table", "Tabelle", ColumnFormatHolder_TextOneLine.Instance);
        tb.Column.GenerateAndAdd("ColumnKey", "Spalten-<br>Name<br>(Schlüssel)", ColumnFormatHolder_TextOneLine.Instance);
        tb.Column.GenerateAndAdd("ColumnCaption", "Spalten-<br>Beschriftung", ColumnFormatHolder_TextOneLine.Instance);
        tb.Column.GenerateAndAdd("RowKey", "Zeilen-<br>Schlüssel", ColumnFormatHolder_LongOnlyPositive.Instance);
        tb.Column.GenerateAndAdd("RowFirst", "Zeile, Wert der<br>1. Spalte", ColumnFormatHolder_TextOneLine.Instance);
        var az = tb.Column.GenerateAndAdd("Aenderzeit", "Änder-<br>Zeit", ColumnFormatHolder_DateTime.Instance);
        tb.Column.GenerateAndAdd("Aenderer", "Änderer", ColumnFormatHolder_TextOneLine.Instance);
        tb.Column.GenerateAndAdd("Symbol", "Symbol", ColumnFormatHolder_ImageCode.Instance);
        tb.Column.GenerateAndAdd("Aenderung", "Änderung", ColumnFormatHolder_TextOneLine.Instance);
        tb.Column.GenerateAndAdd("WertAlt", "Wert alt", ColumnFormatHolder_TextOneLine.Instance);
        tb.Column.GenerateAndAdd("WertNeu", "Wert neu", ColumnFormatHolder_TextOneLine.Instance);
        tb.Column.GenerateAndAdd("Kommentar", "Kommentar", ColumnFormatHolder_TextOneLine.Instance);
        tb.Column.GenerateAndAdd("Herkunft", "Herkunft", ColumnFormatHolder_TextOneLine.Instance);
        tb.Column.DisableAllEditing();
        foreach (var thisColumn in tb.Column) {
            if (!thisColumn.IsSystemColumn()) {
                thisColumn.MultiLine = true;
                thisColumn.DefaultRenderer = Renderer_TextOneLine.ClassId;
            }
        }

        if (az is { IsDisposed: false }) {
            var o = new Renderer_DateTime {
                UTCToLocal = true,
                ShowSymbol = true
            };
            az.DefaultRenderer = o.MyClassId;
            az.RendererSettings = o.ParseableItems().FinishParseable();
        }

        if (tb.Column["Symbol"] is { IsDisposed: false } c) {
            var o = new Renderer_ImageAndText {
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
        txbKennwort.Text = isTableFile ? tb.GlobalShowPass : string.Empty;

        rowSortDefinitionEditor.InputItem = tb.SortDefinition;

        UpdateCustomSortButtons();

        txbTags.Text = string.Join('\r', tb.Tags);

        txbCaption.Text = tb.Caption;
        txbAssetFolder.Text = tb.AssetFolder;
        txbStandardFormulaFile.Text = tb.StandardFormulaFile;
        txbZeilenQuickInfo.Text = tb.RowQuickInfo.Replace("<br>", "\r");

        lbxTableAdmin.Suggestions.Clear();
        lbxTableAdmin.ItemAddRange(TableView.Permission_AllUsed(false));
        lbxTableAdmin.Check(tb.TableAdmin, true);

        variableEditor.InputItem = Table?.Variables;

        lstUniqueValues.UpdateList(tb.UniqueValues);

        txbDictionary.Text = string.Join('\r', tb.DictionaryWords);

        GenerateInfoText();
    }

    private static string CellToPlainText(string cellText, bool textFormatingAllowed) {
        if (!textFormatingAllowed) { return cellText; }
        var decoded = System.Net.WebUtility.HtmlDecode(cellText);

        return Constants.HtmlTagRegex().Replace(decoded, string.Empty);
    }

    private static List<string> ExtractWordsFromTable(Table tb) {
        var words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in tb.Row) {
            if (row.IsDisposed) { continue; }
            foreach (var column in tb.Column) {
                if (column.IsDisposed || !column.SpellCheckingEnabled) { continue; }
                var cellText = row.CellGetString(column);
                if (string.IsNullOrEmpty(cellText)) { continue; }

                var plainText = CellToPlainText(cellText, column.TextFormatingAllowed);

                foreach (Match match in Constants.WordPatternRegex().Matches(plainText)) {
                    if (match.Length > 1 && !SpellDictionary.ContainsWord(match.Value)) {
                        words.Add(match.Value);
                    }
                }
            }
        }

        return UnknownWords([.. words]);
    }

    private static List<string> UnknownWords(IEnumerable<string> words) {
        var result = words.Where(w => !SpellDictionary.ContainsWord(w)).ToList();
        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }

    private void _table_Disposing(object? sender, System.EventArgs e) {
        Table = null;
        Close();
    }

    private void _table_WriteAccessChanged(object? sender, WriteAccessChangedEventArgs e) {
        if (e.IsEditable || _writeAccessLost || IsDisposed) { return; }
        _writeAccessLost = true;
        Forms.Notification.Show("Tabellen-Einstellungen werden geschlossen:<br>Schreibrechte fehlen (" + e.Reason + ")", ImageCode.Warnung);
        Close();
    }

    private void btnCustomSortDisable_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (tb.Column.SysRowSortIndex is not { IsDisposed: false }) {
            Forms.MessageBox.Show("Die benutzerdefinierte Sortierung ist nicht aktiv.", ImageCode.Information, "OK");
            return;
        }

        if (Forms.MessageBox.Show(
            "Wenn Sie die benutzerdefinierte Sortierung löschen,<br>" +
            "wird die Systemspalte <b>Sortierung</b> entfernt.<br><br>" +
            "Konsequenzen:<br>" +
            "- Die Tabellensortierung kehrt zum Standardverhalten zurück.<br>" +
            "- Die bisherigen Sortiernummern gehen verloren.<br><br>" +
            "Fortfahren?",
            ImageCode.Warnung, "Löschen", "Abbrechen") != 0) { return; }

        tb.DisableCustomSort();

        UpdateCustomSortButtons();
    }

    private void btnCustomSortEnable_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (tb.Column.SysRowSortIndex is { IsDisposed: false }) {
            Forms.MessageBox.Show("Die benutzerdefinierte Sortierung ist bereits aktiv.", ImageCode.Information, "OK");
            return;
        }

        if (Forms.MessageBox.Show(
            "Wenn Sie die benutzerdefinierte Sortierung aktivieren,<br>" +
            "wird eine neue Systemspalte <b>Sortierung</b> erstellt.<br><br>" +
            "Konsequenzen:<br>" +
            "- Alle Zeilen erhalten eine fortlaufende Nummer.<br>" +
            "- Die bisherige Sortierung wird <b>deaktiviert</b>.<br>" +
            "- Die Tabelle wird immer nach dieser Spalte sortiert.<br>" +
            "- Neue Zeilen erscheinen am Ende mit der höchsten Nummer.<br><br>" +
            "Fortfahren?",
            ImageCode.AZ, "Aktivieren", "Abbrechen") != 0) { return; }

        tb.EnableCustomSort();

        UpdateCustomSortButtons();
    }

    private void btnExtractWords_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        txbDictionary.Text = string.Join('\r', ExtractWordsFromTable(tb));
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

        IUniqueWindowExtension.ShowOrCreate<TableScriptEditor>(tb);
        //var se = new_TableScriptEditor(db);
        //_ = se.ShowDialog();
    }

    private void btnSpaltenAnordnungen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        var tcvc = ColumnViewCollection.ParseAll(tb);
        tcvc[1].Edit();
        TableView.RepairColumnArrangements(tb);
    }

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Table?.Column.GenerateOverView();

    private void btnTabellenAnsicht_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        var c = new TableViewForm(tb, false, true);
        c.ShowDialog();
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

    private void lstUniqueValues_AddClicked(object sender, System.EventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        var newitem = new UniqueValueDefinition(tb, []);

        List<UniqueValueDefinition> l = [.. tb.UniqueValues];
        l.Add(newitem);
        tb.UniqueValues = l.AsReadOnly();

        lstUniqueValues.UpdateList(tb.UniqueValues);
        lstUniqueValues.Check(newitem.KeyName);
    }

    private void lstUniqueValues_ItemCheckedChanged(object sender, System.EventArgs e) {
        var newKeyName = string.Empty;
        if (lstUniqueValues.Checked.Count == 1) {
            if (lstUniqueValues[lstUniqueValues.Checked[0]] is ReadableListItem rli) {
                newKeyName = rli.KeyName;
            }
        }

        WriteUniqueValuesBack();

        if (Table is not { IsDisposed: false } tb) {
            _selectedUniqueValue = null;
            uniqueValueDefinitionEditor.InputItem = null;
            return;
        }

        lstUniqueValues.UpdateList(tb.UniqueValues);

        if (!string.IsNullOrEmpty(newKeyName)) {
            _selectedUniqueValue = tb.UniqueValues.FirstOrDefault(u => string.Equals(u.KeyName, newKeyName, StringComparison.OrdinalIgnoreCase));
            uniqueValueDefinitionEditor.InputItem = _selectedUniqueValue;
        } else {
            _selectedUniqueValue = null;
            uniqueValueDefinitionEditor.InputItem = null;
        }
    }

    private void lstUniqueValues_RemoveClicked(object sender, EventArgs.AbstractListItemEventArgs e) {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (e.Item is not ReadableListItem rli || rli.Item is not UniqueValueDefinition uvd) { return; }

        WriteUniqueValuesBack();

        var toRemoveKey = uvd.KeyName;
        var newList = tb.UniqueValues.Where(u => !string.Equals(u.KeyName, toRemoveKey, StringComparison.OrdinalIgnoreCase)).ToList();
        tb.UniqueValues = newList.AsReadOnly();

        _selectedUniqueValue = null;
        uniqueValueDefinitionEditor.InputItem = null;

        lstUniqueValues.UpdateList(tb.UniqueValues);
    }

    private void OkBut_Click(object sender, System.EventArgs e) => Close();

    private void UpdateCustomSortButtons() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) {
            btnCustomSortEnable.Enabled = false;
            btnCustomSortDisable.Enabled = false;
            return;
        }

        var hasSortIndex = tb.Column.SysRowSortIndex is { IsDisposed: false };
        btnCustomSortEnable.Enabled = !hasSortIndex;
        btnCustomSortDisable.Enabled = hasSortIndex;
        rowSortDefinitionEditor.Enabled = !hasSortIndex;

        if (!hasSortIndex) {
            rowSortDefinitionEditor.InputItem = tb.SortDefinition;
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
        Table.StandardFormulaFile = txbStandardFormulaFile.Text;
        Table.RowQuickInfo = txbZeilenQuickInfo.Text.Replace("\r", "<br>");

        Table.Tags = new(txbTags.Text.SplitAndCutByCr());

        Table.TableAdmin = new(lbxTableAdmin.Checked);

        var tmp = PermissionGroups_NewRow.Checked.ToList();
        tmp.Remove(Constants.Administrator);
        Table.PermissionGroupsNewRow = new(tmp);

        if (Table.Column.SysRowSortIndex is not { IsDisposed: false }) {
            Table.SortDefinition = ((IIsEditor)rowSortDefinitionEditor).OutputItem as RowSortDefinition;
        }

        WriteUniqueValuesBack();

        #region UniqueValues aufräumen

        Table.UniqueValues = Table.UniqueValues.Where(x => x.KeyColumns.Count > 0).ToList().AsReadOnly();

        #endregion

        #region Wörterbuch

        var dictWords = UnknownWords(txbDictionary.Text.SplitAndCutByCr());
        Table.DictionaryWords = new(dictWords);

        #endregion

        #region Variablen

        // Identisch in TableHeadEditor und TableScriptEditor
        if (((IIsEditor)variableEditor).OutputItem is VariableCollection vl) {
            Table.Variables = vl;
        }

        #endregion
    }

    private void WriteUniqueValuesBack() {
        if (IsDisposed || Table is not { IsDisposed: false } tb) { return; }

        if (_selectedUniqueValue is null || ((IIsEditor)uniqueValueDefinitionEditor).OutputItem is not UniqueValueDefinition edited) { return; }

        var newList = new List<UniqueValueDefinition>();
        foreach (var uv in tb.UniqueValues) {
            if (string.Equals(uv.KeyName, _selectedUniqueValue.KeyName, StringComparison.OrdinalIgnoreCase)) {
                newList.Add(edited);
            } else {
                newList.Add(uv);
            }
        }
        tb.UniqueValues = newList.AsReadOnly();
    }

    #endregion
}