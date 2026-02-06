// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics;
using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.Renderer;
using BlueTable.Classes;
using BlueTable.Enums;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Converter;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueTableDialogs;

internal sealed partial class ColumnEditor : IIsEditor, IHasTable {

    #region Fields

    private readonly TableView? _table;

    private Renderer_Abstract? _renderer;

    #endregion

    #region Constructors

    public ColumnEditor() : this(null, null) {
    }

    public ColumnEditor(ColumnItem? column, TableView? table) : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        _table = table;

        cbxChunk.ItemAddRange(ItemsOf(typeof(ChunkType)));
        cbxRelationType.ItemAddRange(ItemsOf(typeof(RelationType)));
        cbxRandLinks.ItemAddRange(ItemsOf(typeof(ColumnLineStyle)));
        cbxRandRechts.ItemAddRange(ItemsOf(typeof(ColumnLineStyle)));
        cbxBackground.ItemAddRange(ItemsOf(typeof(ColumnBackgroundStyle)));
        cbxAlign.ItemAddRange(ItemsOf(typeof(AlignmentHorizontal)));
        cbxAdditionalCheck.ItemAddRange(ItemsOf(typeof(AdditionalCheck)));
        cbxScriptType.ItemAddRange(ItemsOf(typeof(ScriptType)));
        cbxTranslate.ItemAddRange(ItemsOf(typeof(TranslationType)));

        var l = Generic.GetInstaceOfType<Renderer_Abstract>();
        foreach (var thisr in l) {
            cbxRenderer.ItemAdd(ItemOf(thisr.ReadableText(), thisr.MyClassId, thisr.SymbolForReadableText()));
        }

        cbxSort.ItemAddRange(ItemsOf(typeof(SortierTyp)));

        foreach (var thisItem in ColumnFormatHolder.AllFormats) {
            var bli = new BitmapListItem(thisItem.SymbolForReadableText(), thisItem.KeyName, thisItem.ReadableText(), thisItem.QuickInfo) {
                Padding = 5,
                Tag = thisItem,
                QuickInfo = thisItem.QuickInfo
            };
            lstStyles.ItemAdd(bli);
        }

        Column = column;
    }

    #endregion

    #region Properties

    public ColumnItem? Column {
        get;
        set {
            if (value == field) { return; }

            if (tblFilterliste.Table is { } tb) {
                tb.Dispose();
            }

            field = value;
            Column_DatenAuslesen();
        }
    }

    public Type? EditorFor => typeof(ColumnItem);
    public Table? Table => Column?.Table;

    public IEditable? ToEdit {
        set {
            if (value == Column) { return; }
            if (IsDisposed) { return; }

            if (Column is { IsDisposed: false }) { AllOk(); }

            Column = value as ColumnItem;
        }
    }

    #endregion

    #region Methods

    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        if (!AllOk()) {
            e.Cancel = true;
        }
        Column = null;
    }

    private static string ColumnUsage(ColumnItem? column) {
        if (column?.Table is not { IsDisposed: false } tb) { return string.Empty; }

        var t = $"<b><u>Infos zu '{column.ReadableText()}'</b></u>\r";

        if (column.LinkedTable is { } tb2) {
            t += GenQIText("Tabelle", ImageCode.Tabelle, tb2.Caption);
        } else {
            t += GenQIText("Tabelle", ImageCode.Tabelle, tb.Caption);
        }

        if (!column.IsSystemColumn()) {
            t += GenQIText("Typ", column.SymbolForReadableText(), "Normale Spalte");
        } else {
            t += GenQIText("Typ", column.SymbolForReadableText(), "System Spalte");
        }

        var editable = string.IsNullOrEmpty(CellCollection.IsCellEditable(column, tb.Row.First(), "dummy"));
        t += GenQIText("Admin bearbeitbar", ImageCode.Stift, editable);

        t += GenQIText("Für Sortierung", ImageCode.AZ, tb.SortDefinition?.UsedColumns.Contains(column) ?? false);

        var cola = new List<string>();
        var tcvc = ColumnViewCollection.ParseAll(tb);
        foreach (var thisView in tcvc) {
            if (thisView[column] is { }) { cola.Add(thisView.KeyName); }
        }
        t += GenQIText("In Spalten-Anord.", ImageCode.Spalte, cola);
        t += GenQIText("In intern. Skripten", ImageCode.Skript, column.UsedInScript());
        t += GenQIText("In Zeilen-Quick-Info", ImageCode.Zeile, tb.RowQuickInfo.ContainsIgnoreCase(column.KeyName));
        t += GenQIText("Schlüssel für", ImageCode.Schlüssel, column.Am_A_Key_For);

        if (column.SaveContent) {
            var l = column.Contents();
            t += GenQIText("Verschiedene Werte", ImageCode.Textfeld, l.Count.ToString1());
        } else {
            t += GenQIText("Verschiedene Werte", ImageCode.Textfeld, "[wird nicht gespeichert]");
        }

        if (!string.IsNullOrEmpty(column.ColumnSystemInfo)) {
            t += "\r\r<b><u>Ermittelt:</b></u>\r";
            t += GenQIText("Seit UTC", ImageCode.Uhr, "Seit UTC", column.ColumnSystemInfo);
            t += GenQIText("Bearbeitet in Tab.", ImageCode.Stift, "Edit in Table", column.ColumnSystemInfo);
            t += GenQIText("Gefiltert in Tab.", ImageCode.Trichter, "Filter Clicked", column.ColumnSystemInfo);
            t += GenQIText("Gibt Werte an", ImageCode.Tabelle, "Links to me", column.ColumnSystemInfo);
            t += GenQIText("Ext. Skript-'Filter'", ImageCode.Skript, "Filter in Script", column.ColumnSystemInfo);
            t += GenQIText("Ext. Skript-'Get'", ImageCode.Skript, "Value used in Script", column.ColumnSystemInfo);
            t += GenQIText("Ext. Skript-'Set'", ImageCode.Skript, "Edit with Script", column.ColumnSystemInfo);
            //t += column.ColumnSystemInfo;
        }

        return t;
    }

    private static string GenQIText(string name, QuickImage? quickImage, string text) => $"<b>{quickImage?.HTMLCode} {name}:</b><tab>{text}\r";

    private static string GenQIText(string name, ImageCode img, string text) => GenQIText(name, QuickImage.Get(img, 16), text);

    private static string GenQIText(string name, ImageCode img, bool state) {
        if (state) {
            return $"<b>{QuickImage.Get(img, 16).HTMLCode} {name}:</b><tab>{QuickImage.Get(ImageCode.Häkchen, 16).HTMLCode}\r";
        } else {
            return $"<b>{QuickImage.Get(img, 16).HTMLCode} {name}:</b><tab>{QuickImage.Get(ImageCode.Kreuz, 16).HTMLCode}\r";
        }
    }

    private static string GenQIText(string name, ImageCode img, string suchwert, string columnSystemInfo) {
        var t = columnSystemInfo.SplitAndCutByCrAndBr().SortedDistinctList();

        var l = t.TagGetAll(suchwert);

        return GenQIText(name, img, l);
    }

    private static string GenQIText(string name, ImageCode img, List<string> werte) {
        string t2;

        if (werte.Count == 0) {
            t2 = "-";
        } else if (werte.Count is > 0 and < 4) {
            t2 = werte.JoinWith("; ");
        } else {
            t2 = werte.Take(4).JoinWith("; ") + "; ...";
        }

        return $"<b>{QuickImage.Get(img, 16).HTMLCode} {name} </b><i>({werte.Count})</i><b>:</b><tab>{t2}\r";
    }

    private void _renderer_DoUpdateSideOptionMenu(object? sender, System.EventArgs e) => _renderer.DoForm(RendererEditor);

    private bool AllOk() {
        var feh = string.Empty;

        //// Diese Fehler sind so schwer und darf auf keinen Fall in die Umwelt gelassen werden
        if (Column is not { IsDisposed: false }) {
            feh = "Spalte verworfen!";
        } else {
            if (!txbName.Text.StartsWith("SYS_")) {
                if (!Column.ColumNameAllowed(txbName.Text)) { feh = "Spaltenname nicht erlaubt!"; }
            }
        }

        if (string.IsNullOrEmpty(feh)) {
            Column_DatenZurückschreiben();
            if (string.IsNullOrEmpty(feh)) { feh = Column!.ErrorReason(); }
        }

        if (!string.IsNullOrEmpty(feh)) {
            Forms.MessageBox.Show("<b><u>Bitte korrigieren sie zuerst folgenden Fehler:</u></b><br>" + feh, ImageCode.Warnung, "Ok");
            return false;
        }

        Column?.Repair();
        return true;
    }

    private void btnBackColor_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo ?? Color.White;
        ColorDia.ShowDialog();
        btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).KeyName;
    }

    private void btnCalculateMaxCellLength_Click(object sender, System.EventArgs e) {
        if (Column == null) { return; }

        txbMaxCellLength.Text = Column.CalculatePreveredMaxCellLength(1.2f).ToString1();
    }

    private void btnMaxTextLength_Click(object sender, System.EventArgs e) {
        if (Column == null) { return; }
        txbMaxTextLength.Text = Column.CalculatePreveredMaxTextLength(1.2f).ToString1();
    }

    private void btnOk_Click(object sender, System.EventArgs e) {
        if (!AllOk()) { return; }
        Close();
    }

    private void btnQI_Vorschau_Click(object sender, System.EventArgs e) => Notification.Show(txbQuickinfo.Text.Replace("\r", "<br>") + "<br><br><br>" + txbAdminInfo.Text.Replace("\r", "<br>"));

    private void btnSpaltenkopf_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Column?.Table is not { IsDisposed: false } tb) { return; }
        if (TableViewForm.EditabelErrorMessage(tb)) { return; }

        tb.Edit();
    }

    private void btnStandard_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Column?.Table is not { IsDisposed: false }) { return; }
        if (!AllOk()) { return; }
        Column.ResetSystemToDefault(true);
        Column_DatenAuslesen();
    }

    private void btnSystemInfo_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Column?.Table is not { IsDisposed: false } tb) { return; }

        foreach (var item in tb.Column) {
            item.SystemInfoReset(true);
        }

        capInfos.Text = ColumnUsage(Column);
    }

    private void btnTextColor_Click(object sender, System.EventArgs e) {
        ColorDia.Color = QuickImage.Get(btnTextColor.ImageCode).ChangeGreenTo ?? Color.White;
        ColorDia.ShowDialog();
        btnTextColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, ColorDia.Color).KeyName;
    }

    private void butAktuellVor_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Column?.Table is not { IsDisposed: false }) { return; }
        if (!AllOk()) { return; }
        var cu = _table?.CurrentArrangement?[Column];
        Column = _table?.CurrentArrangement?.NextVisible(cu)?.Column ?? Column;
        Column_DatenAuslesen();
    }

    private void butAktuellZurueck_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Column?.Table is not { IsDisposed: false }) { return; }
        if (!AllOk()) { return; }
        var cu = _table?.CurrentArrangement?[Column];
        Column = _table?.CurrentArrangement?.PreviousVisible(cu)?.Column ?? Column;
        Column_DatenAuslesen();
    }

    /// <summary>
    /// Kümmert sich um erlaubte Spalten für LinkedCell
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>

    private void cbxLinkedTable_TextChanged(object? sender, System.EventArgs e) {
        if (Column is not { IsDisposed: false }) { return; }

        Column.LinkedTableTableName = cbxLinkedTable.Text;

        cbxTargetColumn.ItemClear();

        if (Column.LinkedTable != null) {
            //foreach (var ThisColumn in _Column.Table.Column) {
            //    if (ThisColumn.CanBeCheckedByRules() && !ThisColumn.MultiLine && !ThisColumn.Format.NeedTargetTable()) {
            //        cbxRowKeyInColumn.ItemAdd(ThisColumn);
            //    }
            //    //if (ThisColumn.Format == DataFormat.Values_für_LinkedCellDropdownx && ThisColumn.LinkedTable() == _Column.LinkedTable()) {
            //    //    cbxRowKeyInColumn.ItemAdd(ThisColumn);
            //    //}
            //}
            foreach (var thisLinkedColumn in Column.LinkedTable.Column) {
                if (thisLinkedColumn.CanBeCheckedByRules() && thisLinkedColumn.RelationType == RelationType.None) {
                    cbxTargetColumn.ItemAdd(ItemOf(thisLinkedColumn));
                }
            }
        }
        //cbxTargetColumn.Item.Sort();
        cbxTargetColumn.Text = Column.ColumnNameOfLinkedTable;
        //SetKeyTo(cbxTargetColumn, _column.LinkedCell_ColumnKeyOfLinkedTable);
        cbxTargetColumn.Enabled = cbxTargetColumn.ItemCount > 0;
        capTargetColumn.Enabled = cbxTargetColumn.Enabled;
        if (!cbxTargetColumn.Enabled) {
            cbxTargetColumn.Text = string.Empty;
        }

        GeneratFilterListe();
    }

    private void cbxRenderer_TextChanged(object sender, System.EventArgs e) {
        if (_renderer != null) {
            _renderer.DoUpdateSideOptionMenu -= _renderer_DoUpdateSideOptionMenu;
        }

        if (Column != null && !string.IsNullOrEmpty(cbxRenderer.Text)) {
            _renderer = ParseableItem.NewByTypeName<Renderer_Abstract>(cbxRenderer.Text);
            _renderer?.Parse(Column.RendererSettings);
        } else {
            _renderer = null;
        }

        _renderer_DoUpdateSideOptionMenu(_renderer, System.EventArgs.Empty);

        if (_renderer != null) {
            _renderer.DoUpdateSideOptionMenu += _renderer_DoUpdateSideOptionMenu;
        }
    }

    private void cbxTargetColumn_TextChanged(object sender, System.EventArgs e) => GeneratFilterListe();

    private void Column_DatenAuslesen() {
        if (Column is not { IsDisposed: false }) { return; }

        capTabellenname.Text = LanguageTool.DoTranslate("<b>Tabellenname: </b>{0}", true, Column.Table?.KeyName);

        cbxLinkedTable.ItemClear();

        lbxCellEditor.Suggestions.Clear();

        lbxCellEditor.ItemAddRange(TableView.Permission_AllUsed(true));

        if (_table?.CurrentArrangement is { IsDisposed: false } cu) {
            var column = cu[Column];
            butAktuellZurueck.Enabled = cu.PreviousVisible(column) != null;
            butAktuellVor.Enabled = cu.NextVisible(column) != null;
        } else {
            butAktuellVor.Enabled = false;
            butAktuellZurueck.Enabled = false;
        }

        if (!Column.IsSystemColumn()) {
            btnStandard.Enabled = false;
            capInfo.Text = Column.SymbolForReadableText()?.HTMLCode + " Normale Spalte";
        } else {
            btnStandard.Enabled = true;
            capInfo.Text = Column.SymbolForReadableText()?.HTMLCode + " System Spalte";
        }

        txbName.Text = Column.KeyName;
        txbName.Enabled = !Column.KeyName.StartsWith("SYS_");

        txbName.AllowedChars = Constants.AllowedCharsVariableName;
        txbCaption.Text = Column.Caption;
        btnBackColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Column.BackColor).KeyName;
        btnTextColor.ImageCode = QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Column.ForeColor).KeyName;
        chkMultiline.Checked = Column.MultiLine;
        chkIsFirst.Checked = Column.IsFirst;
        chkIsKeyColumn.Checked = Column.IsKeyColumn;
        chkRelation.Checked = Column.Relationship_to_First;
        cbxRelationType.Text = ((int)Column.RelationType).ToString1();
        cbxChunk.Text = ((int)Column.Value_for_Chunk).ToString1();
        cbxRandLinks.Text = ((int)Column.LineStyleLeft).ToString1();
        cbxRandRechts.Text = ((int)Column.LineStyleRight).ToString1();
        cbxAlign.Text = ((int)Column.Align).ToString1();
        cbxBackground.Text = ((long)Column.BackgroundStyle).ToString1();
        cbxAdditionalCheck.Text = ((int)Column.AdditionalFormatCheck).ToString1();
        cbxScriptType.Text = ((int)Column.ScriptType).ToString1();
        cbxTranslate.Text = ((int)Column.DoOpticalTranslation).ToString1();
        cbxRenderer.Text = Column.DefaultRenderer;
        cbxRenderer_TextChanged(cbxRenderer, System.EventArgs.Empty);
        cbxSort.Text = ((int)Column.SortType).ToString1();
        btnAutoFilterMoeglich.Checked = Column.FilterOptions.HasFlag(FilterOptions.Enabled);
        btnAutoFilterTXTErlaubt.Checked = Column.FilterOptions.HasFlag(FilterOptions.TextFilterEnabled);
        btnAutoFilterErweitertErlaubt.Checked = Column.FilterOptions.HasFlag(FilterOptions.ExtendedFilterEnabled);
        chkFilterOnlyOr.Checked = Column.FilterOptions.HasFlag(FilterOptions.OnlyOrAllowed);
        chkFilterOnlyAND.Checked = Column.FilterOptions.HasFlag(FilterOptions.OnlyAndAllowed);
        btnZeilenFilterIgnorieren.Checked = Column.IgnoreAtRowFilter;
        btnEditableStandard.Checked = Column.EditableWithTextInput;
        btnEditableDropdown.Checked = Column.EditableWithDropdown;
        btnCanBeEmpty.Checked = Column.DropdownDeselectAllAllowed;
        btnAutoEditAutoSort.Checked = Column.AfterEditQuickSortRemoveDouble;
        txbRunden.Text = Column.AfterEditRound is > -1 and < 7 ? Column.AfterEditRound.ToString1() : string.Empty;
        txbFixedColumnWidth.Text = Column.FixedColumnWidth > 0 ? Column.FixedColumnWidth.ToString1() : string.Empty;
        btnAutoEditToUpper.Checked = Column.AfterEditDoUCase;
        btnAutoEditKleineFehler.Checked = Column.AfterEditAutoCorrect;
        txbJoker.Text = Column.AutoFilterJoker;
        txbUeberschift1.Text = Column.CaptionGroup1;
        txbUeberschift2.Text = Column.CaptionGroup2;
        txbUeberschift3.Text = Column.CaptionGroup3;
        txbSpaltenbild.Text = Column.CaptionBitmapCode;
        chkSaveContent.Checked = Column.SaveContent;
        chkFormatierungErlaubt.Checked = Column.TextFormatingAllowed;
        btnSpellChecking.Checked = Column.SpellCheckingEnabled;
        txbAuswaehlbareWerte.Text = Column.DropDownItems.JoinWithCr();
        txbAutoReplace.Text = Column.AfterEditAutoReplace.JoinWithCr();
        txbRegex.Text = Column.RegexCheck;
        txbTags.Text = Column.ColumnTags.JoinWithCr();
        lbxCellEditor.Check(Column.PermissionGroupsChangeCell, true);
        txbAllowedChars.Text = Column.AllowedChars;
        txbMaxTextLength.Text = Column.MaxTextLength.ToString1();
        txbMaxCellLength.Text = Column.MaxCellLength.ToString1();
        btnOtherValuesToo.Checked = Column.ShowValuesOfOtherCellsInDropdown;
        btnIgnoreLock.Checked = Column.EditAllowedDespiteLock;
        txbAdminInfo.Text = Column.AdminInfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);
        txbQuickinfo.Text = Column.QuickInfo.Replace("<br>", "\r", RegexOptions.IgnoreCase);
        cbxLinkedTable.Text = Column.LinkedTableTableName;
        txbAutoRemove.Text = Column.AfterEditAutoRemoveChar;
        cbxLinkedTable_TextChanged(null, System.EventArgs.Empty);
        capInfos.Text = ColumnUsage(Column);
    }

    /// <summary>
    /// Schreibt die Werte hart zurück.
    /// Diese Routine sollte nicht benutzt werden, sondern AllOk
    /// </summary>

    private void Column_DatenZurückschreiben() {
        if (TableViewForm.EditabelErrorMessage(Column?.Table)) { return; }

        if (Column is not { IsDisposed: false }) { return; }
        if (IsClosed) { return; }

        if (Column.ColumNameAllowed(txbName.Text)) {
            Column.KeyName = txbName.Text;
        }

        Column.Caption = txbCaption.Text.Replace("\r\n", "\r").Trim().Trim("\r").Trim();
        Column.QuickInfo = txbQuickinfo.Text.Replace("\r", "<br>");
        Column.AdminInfo = txbAdminInfo.Text.Replace("\r", "<br>");
        Column.BackColor = QuickImage.Get(btnBackColor.ImageCode).ChangeGreenTo ?? Color.White;
        Column.ForeColor = QuickImage.Get(btnTextColor.ImageCode).ChangeGreenTo ?? Color.White;
        Column.LineStyleLeft = (ColumnLineStyle)IntParse(cbxRandLinks.Text);
        Column.LineStyleRight = (ColumnLineStyle)IntParse(cbxRandRechts.Text);
        Column.RelationType = (RelationType)IntParse(cbxRelationType.Text);
        Column.Value_for_Chunk = (ChunkType)IntParse(cbxChunk.Text);
        Column.MultiLine = chkMultiline.Checked;
        Column.IsFirst = chkIsFirst.Checked;
        Column.IsKeyColumn = chkIsKeyColumn.Checked;
        Column.Relationship_to_First = chkRelation.Checked;
        Column.AfterEditQuickSortRemoveDouble = btnAutoEditAutoSort.Checked;
        if (txbRunden.Text.IsLong()) {
            var zahl = Math.Max(IntParse(txbRunden.Text), -1);
            zahl = Math.Min(zahl, 7);
            Column.AfterEditRound = zahl;
        } else {
            Column.AfterEditRound = -1;
        }

        if (txbFixedColumnWidth.Text.IsLong()) {
            var zahl = Math.Max(IntParse(txbFixedColumnWidth.Text), 0);
            Column.FixedColumnWidth = zahl;
        } else {
            Column.FixedColumnWidth = 0;
        }

        Column.AfterEditDoUCase = btnAutoEditToUpper.Checked;
        Column.AfterEditAutoCorrect = btnAutoEditKleineFehler.Checked;
        Column.SaveContent = chkSaveContent.Checked;
        Column.TextFormatingAllowed = chkFormatierungErlaubt.Checked;
        Column.SpellCheckingEnabled = btnSpellChecking.Checked;
        var tmpf = FilterOptions.None;
        if (btnAutoFilterMoeglich.Checked) { tmpf |= FilterOptions.Enabled; }
        if (btnAutoFilterTXTErlaubt.Checked) { tmpf |= FilterOptions.TextFilterEnabled; }
        if (btnAutoFilterErweitertErlaubt.Checked) { tmpf |= FilterOptions.ExtendedFilterEnabled; }
        if (chkFilterOnlyOr.Checked) { tmpf |= FilterOptions.OnlyOrAllowed; }
        if (chkFilterOnlyAND.Checked) { tmpf |= FilterOptions.OnlyAndAllowed; }
        //_Column.AutoFilterErlaubt = AutoFilterMöglich.Checked;
        //_Column.FilterOptions.HasFlag(enFilterOptions.TextFilterEnabled) = AutoFilterTXT.Checked;
        //_Column.FilterOptions.HasFlag(enFilterOptions.ExtendedFilterEnabled) = AutoFilterErw.Checked;
        Column.FilterOptions = tmpf;
        Column.IgnoreAtRowFilter = btnZeilenFilterIgnorieren.Checked;
        Column.PermissionGroupsChangeCell = new(lbxCellEditor.Checked);
        Column.DropDownItems = txbAuswaehlbareWerte.Text.SplitAndCutByCr().SortedDistinctList().AsReadOnly();
        Column.AfterEditAutoReplace = new(txbAutoReplace.Text.SplitAndCutByCr());

        Column.AutoFilterJoker = txbJoker.Text;
        Column.CaptionGroup1 = txbUeberschift1.Text;
        Column.CaptionGroup2 = txbUeberschift2.Text;
        Column.CaptionGroup3 = txbUeberschift3.Text;
        Column.CaptionBitmapCode = txbSpaltenbild.Text;
        Column.ColumnTags = [.. txbTags.Text.SplitAndCutByCr()];
        Column.RegexCheck = txbRegex.Text;
        Column.EditableWithTextInput = btnEditableStandard.Checked;
        Column.EditableWithDropdown = btnEditableDropdown.Checked;
        Column.DropdownDeselectAllAllowed = btnCanBeEmpty.Checked;
        Column.ShowValuesOfOtherCellsInDropdown = btnOtherValuesToo.Checked;
        Column.EditAllowedDespiteLock = btnIgnoreLock.Checked;
        Column.AllowedChars = txbAllowedChars.Text;
        Column.MaxTextLength = IntParse(txbMaxTextLength.Text);
        Column.MaxCellLength = IntParse(txbMaxCellLength.Text);
        Column.LinkedTableTableName = cbxLinkedTable.Text; // Muss vor LinkedCell_RowKey zurückgeschrieben werden
        Column.ColumnNameOfLinkedTable = cbxTargetColumn.Text; // LINKED TABLE
        Column.Align = (AlignmentHorizontal)IntParse(cbxAlign.Text);
        Column.BackgroundStyle = (ColumnBackgroundStyle)LongParse(cbxBackground.Text);
        Column.AdditionalFormatCheck = (AdditionalCheck)IntParse(cbxAdditionalCheck.Text);
        Column.ScriptType = (ScriptType)IntParse(cbxScriptType.Text);
        Column.DoOpticalTranslation = (TranslationType)IntParse(cbxTranslate.Text);
        Column.DefaultRenderer = cbxRenderer.Text;
        Column.RendererSettings = _renderer?.ParseableItems().FinishParseable() ?? string.Empty;
        Column.SortType = (SortierTyp)IntParse(cbxSort.Text);
        Column.AfterEditAutoRemoveChar = txbAutoRemove.Text;

        if (Column.RelationType == RelationType.None) {
            Column.LinkedTableTableName = string.Empty;
            Column.ColumnNameOfLinkedTable = string.Empty;
        }

        Column.Invalidate_ColumAndContent();
        GetLinkedCellFilter();

        Column.Repair();

        //cbxRenderer.Text = string.Empty;
    }

    private void GeneratFilterListe() {
        if (IsDisposed || Column?.Table is not { IsDisposed: false } tb2) { return; }

        Column.LinkedTableTableName = cbxLinkedTable.Text;

        var linkedTb = Column.LinkedTable;

        if (linkedTb == null || tblFilterliste.Table != null) { tblFilterliste.TableSet(null, string.Empty); }

        if (tblFilterliste.Table != null &&
            !string.Equals(tblFilterliste.Table.Tags.TagGet("Filename").FileNameWithoutSuffix(), linkedTb?.KeyName, StringComparison.OrdinalIgnoreCase)) {
            tblFilterliste.TableSet(null, string.Empty);
        }

        if (linkedTb == null) { return; }

        if (tblFilterliste.Table == null) {
            var tb = Table.Get();
            //tb.Column.GenerateAndAdd("count", "count", ColumnFormatHolder.IntegerPositive);
            var spn = tb.Column.GenerateAndAdd("SpalteName", "Spalte-Name", ColumnFormatHolder.Text);
            if (spn is not { IsDisposed: false }) { return; }
            spn.IsFirst = true;
            var vis = tb.Column.GenerateAndAdd("visible", "visible", ColumnFormatHolder.Bit);
            if (vis is not { IsDisposed: false }) { return; }
            var sp = tb.Column.GenerateAndAdd("Spalte", "Spalte", ColumnFormatHolder.SystemName);
            if (sp is not { IsDisposed: false }) { return; }

            var info = tb.Column.GenerateAndAdd("info", "Info", ColumnFormatHolder.BildCode);
            if (info is not { IsDisposed: false }) { return; }

            sp.Align = AlignmentHorizontal.Rechts;

            var b = tb.Column.GenerateAndAdd("Such", "Suchtext", ColumnFormatHolder.Text);
            if (b is not { IsDisposed: false }) { return; }
            b.QuickInfo = "<b>Entweder</b> ~Spaltenname~<br><b>oder</b> fester Text zum Suchen<br>Mischen wird nicht unterstützt.";
            b.MultiLine = false;
            b.EditableWithTextInput = true;
            b.DropdownDeselectAllAllowed = true;
            b.EditableWithDropdown = true;

            var dd = b.DropDownItems.Clone();
            var or = new List<string>();

            foreach (var thisColumn in tb2.Column) {
                if (thisColumn.CanBeCheckedByRules() && !thisColumn.MultiLine) {
                    dd.Add("~" + thisColumn.KeyName.ToUpperInvariant() + "~");
                    or.Add("~" + thisColumn.KeyName.ToUpperInvariant() + "~|[Spalte: " + thisColumn.ReadableText() + "]");
                }
            }

            b.DropDownItems = dd.AsReadOnly();

            b.DefaultRenderer = Renderer_ImageAndText.ClassId;

            var s = new Renderer_ImageAndText {
                Text_ersetzen = or.JoinWithCr()
            };
            b.RendererSettings = s.ReadableText();

            tb.RepairAfterParse();
            var tcvc = ColumnViewCollection.ParseAll(tb);

            tcvc[1].Add(sp);
            tcvc[1].Add(b);
            tcvc[1].Add(info);
            tb.ColumnArrangements = tcvc.ToString(false);

            tb.SortDefinition = new RowSortDefinition(tb, sp, false);
            tblFilterliste.TableSet(tb, string.Empty);
            //tblFilterliste.Arrangement = 1;

            var t = tb.Tags.Clone();
            t.TagSet("Filename", linkedTb.KeyName);
            tb.Tags = t.AsReadOnly();

            tblFilterliste.FilterFix.Add(new FilterItem(vis, FilterType.Istgleich, "+"));
        }

        linkedTb.RepairAfterParse(); // Dass ja die 0 Ansicht stimmt

        var columnFromLinkedTb = linkedTb.Column[cbxTargetColumn.Text];

        foreach (var col in linkedTb.Column) {
            var r = tblFilterliste.Table?.Row[col.KeyName] ?? tblFilterliste.Table?.Row.GenerateAndAdd(col.KeyName, "Neue Spalte");

            if (r != null) {
                r.CellSet("Spalte", col.ReadableText() + " = ", string.Empty);
                r.CellSet("SpalteName", col.KeyName, string.Empty);

                if (col != columnFromLinkedTb && col.RelationType == RelationType.None && !col.IsSystemColumn()) {
                    r.CellSet("visible", true, string.Empty);
                } else {
                    r.CellSet("visible", false, string.Empty);
                }

                if (col == col.Table?.Column.ChunkValueColumn) {
                    r.CellSet("info", "Warnung", string.Empty);
                } else {
                    r.CellSet("info", string.Empty, string.Empty);
                }
            }
        }

        SetLinkedCellFilter();
    }

    /// <summary>
    /// Holt die Werte aus tblFilterliste und schreibt sie in _Column.LinkedCellFilter
    /// Hat tblFilterliste keine Tabelle, bleibt die Variable _Column.LinkedCellFilter unverändert
    /// </summary>

    private void GetLinkedCellFilter() {
        if (IsDisposed || tblFilterliste.Table is not { IsDisposed: false } tb) { return; }
        if (Column?.Table is not { IsDisposed: false }) { return; }

        var nf = new List<string>();
        foreach (var thisr in tb.Row) {
            if (thisr.CellGetBoolean("visible") && !string.IsNullOrEmpty(thisr.CellGetString("such"))) {
                nf.Add(thisr.CellGetString("spaltename") + "|=|" + thisr.CellGetString("Such").ToNonCritical());
            }
        }

        Column.LinkedCellFilter = nf;
    }

    /// <summary>
    /// Holt die Werte aus _Column.LinkedCellFilter und schreibt sie in tblFilterliste
    ///Leer evtl. Werte aus tblFilterliste
    /// </summary>

    private void lstStyles_ItemClicked(object sender, EventArgs.AbstractListItemEventArgs e) {
        if (e.Item is not BitmapListItem bli || bli.Tag is not ColumnFormatHolder chf) { return; }

        if (!AllOk()) { return; }
        Column.GetStyleFrom(chf);
        Column_DatenAuslesen();
    }

    private void SetLinkedCellFilter() {
        if (IsDisposed || tblFilterliste.Table is not { IsDisposed: false } tb) { return; }
        if (Column is not { IsDisposed: false }) { return; }

        foreach (var thisr in tb.Row) {
            thisr.CellSet("Such", string.Empty, string.Empty);
        }

        if (tb.Column["SpalteName"] is not { IsDisposed: false } c) { return; }

        foreach (var thisFi in Column.LinkedCellFilter) {
            var x = thisFi.SplitBy("|");

            if (x.Length == 3) {
                var r = tb.Row[new FilterItem(c, FilterType.Istgleich_GroßKleinEgal, x[0])];

                if (r?.CellGetBoolean("Visible") == true) {
                    r.CellSet("Such", x[2].FromNonCritical(), string.Empty);
                }
            }
        }
    }

    private void tabControl_SelectedIndexChanged(object sender, System.EventArgs e) {
        if (IsDisposed || Column?.Table is not { IsDisposed: false } tb) { return; }

        if (tabControl.SelectedTab == tabSpaltenVerlinkung && cbxLinkedTable.ItemCount == 0) {
            var l = Table.AllAvailableTables();

            foreach (var thisString in l) {
                if (!string.Equals(thisString.FileNameWithoutSuffix(), tb.KeyName, StringComparison.OrdinalIgnoreCase)) { cbxLinkedTable.ItemAdd(ItemOf(thisString.FileNameWithoutSuffix(), thisString)); }
            }
        }
    }

    #endregion
}