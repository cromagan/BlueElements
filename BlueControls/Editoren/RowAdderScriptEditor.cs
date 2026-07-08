// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueScript.Classes;
using BlueTable.Interfaces;
using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs;

public sealed partial class RowAdderScriptEditor : ScriptEditorGeneric, IHasTable {

    #region Fields

    private Controls.TextBox? _dropDownTarget;
    private RowAdderPadItem? _item;

    /// <summary>
    /// 1 = Before
    /// 2 = Menu
    /// 3 = After
    /// </summary>
    private int scriptNo = 2;

    #endregion

    #region Constructors

    public RowAdderScriptEditor() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        UpdateChunkUiState();
    }

    #endregion

    #region Properties

    public override object? Object {
        get => IsDisposed ? null : (object?)_item;
        set {
            if (value is not RowAdderPadItem) { value = null; }
            if (_item == value) { return; }

            WriteInfosBack();

            _item = null;

            if (value is RowAdderPadItem cpi) {
                _item = cpi;
                LastFailedReason = cpi.LastFailedReason;
                LastVariables = cpi.LastSavedVariables;
            } else {
                LastFailedReason = string.Empty;
                LastVariables = null;
            }

            ShowScript();
            UpdateChunkUiState();
            btnAnzeigen_Click(null, System.EventArgs.Empty);
        }
    }

    public Table? Table {
        get;
        set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == field) { return; }

            field?.DisposingEvent -= _table_Disposing;
            field = value;

            field?.DisposingEvent += _table_Disposing;

            UpdateChunkUiState();
        }
    }

    /// <summary>
    /// Speicherschlüssel pro Tabelle. Der Editor-Typ-Suffix stellt sicher, dass die Sets
    /// nicht mit anderen Skript-Editoren (z.B. TableScriptEditor) derselben Tabelle kollidieren.
    /// </summary>
    public override string? VariablesStorageKey => Table?.KeyName is { Length: > 0 } k ? k + "|RowAdderScript" : null;

    #endregion

    #region Methods

    public override ScriptEndedFeedback ExecuteScript(bool testmode, bool syntaxCheck) {
        if (IsDisposed || Table is not { IsDisposed: false }) {
            return new ScriptEndedFeedback("Keine Tabelle geladen.", false, false, "Allgemein");
        }

        if (_item is null) {
            return new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
        }

        WriteInfosBack();

        if (!_item.IsOk()) {
            return new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, false, "Allgemein");
        }

        if (Table.Row.Count == 0) {
            return new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.", false, false, "Allgemein");
        }
        if (string.IsNullOrEmpty(txbTestZeile.Text)) {
            txbTestZeile.Text = Table?.Row.First()?.CellFirstString() ?? string.Empty;
        }

        var r = Table?.Row[txbTestZeile.Text] ?? Table?.Row.GetByKey(txbTestZeile.Text);
        if (r is not { IsDisposed: false }) {
            return new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
        }

        var produktiv = !(testmode || syntaxCheck);

        ScriptEndedFeedback feedback;

        switch (scriptNo) {
            case 1:
                feedback = RowAdder.ExecuteScript(_item.Script_Before, produktiv, "Testmodus", _item.EntityID, r, false, syntaxCheck);
                break;

            case 3:
                feedback = RowAdder.ExecuteScript(_item.Script_After, produktiv, "Testmodus", _item.EntityID, r, false, syntaxCheck);
                break;

            default:
                feedback = RowAdder.ExecuteScript(_item.Script_MenuGeneration, produktiv, "Testmodus", _item.EntityID, r, true, syntaxCheck);
                break;
        }

        if (!syntaxCheck) {
            WriteInfosBack();
        }

        return feedback;
    }

    public override void WriteInfosBack() {
        //if (IsDisposed || TableView.ErrorMessage(Table, EditableErrorReasonType.EditNormaly) || Table is null || Table.IsDisposed) { return; }

        if (_item is not null) {
            _item.LastFailedReason = LastFailedReason;
            _item.LastSavedVariables = LastVariables;

            switch (scriptNo) {
                case 1:
                    _item.Script_Before = Script;
                    break;

                case 2:
                    _item.Script_MenuGeneration = Script;
                    break;

                case 3:
                    _item.Script_After = Script;
                    break;
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        WriteInfosBack();

        base.OnFormClosing(e);

        Object = null; // erst das Item!
    }

    /// <summary>
    /// Erzeugt ein JsonObject mit den Editor-spezifischen Werten (TestZeile, Chunk, ScriptNo) zusätzlich zu den Basis-Feldern.
    /// </summary>
    protected override JsonObject SpecialFieldsToVariables() {
        var fields = base.SpecialFieldsToVariables();
        fields[Constants.KeyTestZeile.ToUpperInvariant()] = txbTestZeile.Text ?? string.Empty;
        fields[Constants.KeyChunk.ToUpperInvariant()] = txbChunk.Text ?? string.Empty;
        fields[Constants.KeyScriptNo.ToUpperInvariant()] = scriptNo;
        return fields;
    }

    protected override void VariablesToSpecialField(JsonObject? data, bool clearMissing) {
        base.VariablesToSpecialField(data, clearMissing);

        // txbTestZeile: übernehmen, wenn ein Wert vorhanden ist UND die Zeile in der
        // aktuellen Tabelle existiert. Bei clearMissing sonst leeren, sonst unverändert lassen.
        var testZeileSet = false;
        if (data is not null
            && data.TryGetPropertyValue(Constants.KeyTestZeile.ToUpperInvariant(), out var tzNode)
            && tzNode is JsonValue tzv
            && tzv.TryGetValue(out string? tz)
            && !string.IsNullOrEmpty(tz)
            && Table is { IsDisposed: false } tb) {
            var r = tb.Row[tz] ?? tb.Row.GetByKey(tz);
            if (r is { IsDisposed: false }) {
                txbTestZeile.Text = tz;
                testZeileSet = true;
            }
        }
        if (!testZeileSet && clearMissing) {
            txbTestZeile.Text = string.Empty;
        }

        // txbChunk: übernehmen, falls vorhanden. Bei clearMissing sonst leeren, sonst unverändert lassen.
        if (data is not null
            && data.TryGetPropertyValue(Constants.KeyChunk.ToUpperInvariant(), out var chNode)
            && chNode is JsonValue chv
            && chv.TryGetValue(out string? ch)) {
            txbChunk.Text = ch ?? string.Empty;
        } else if (clearMissing) {
            txbChunk.Text = string.Empty;
        }

        // scriptNo: gültigen Wert übernehmen. Sonst unverändert lassen.
        if (data is not null
            && data.TryGetPropertyValue(Constants.KeyScriptNo.ToUpperInvariant(), out var snNode)
            && snNode is JsonValue snv
            && snv.TryGetValue(out int sn)
            && sn is 1 or 2 or 3) {
            if (scriptNo != sn) {
                WriteInfosBack();
                scriptNo = sn;
                ShowScript();
            }
        }
    }

    private void _table_Disposing(object? sender, System.EventArgs e) {
        Table = null;
        Close();
    }

    private void btnChunkDropDown_Click(object sender, System.EventArgs e) {
        if (Table is not { IsDisposed: false }) { return; }
        var items = TableScriptEditor.BuildChunkDropdownItems(Table);

        _dropDownTarget = txbChunk;
        var dropDown = TableScriptEditor.ShowScriptEditorDropDown(btnChunkDropDown, items, txbChunk.Text);
        dropDown?.ItemClicked += ScriptEditorDropDown_ItemClicked;
    }

    private void btnScriptAfter_Click(object sender, System.EventArgs e) {
        WriteInfosBack();
        scriptNo = 3;
        ShowScript();
    }

    private void btnScriptBefore_Click(object sender, System.EventArgs e) {
        WriteInfosBack();
        scriptNo = 1;
        ShowScript();
    }

    private void btnScriptMenu_Click(object sender, System.EventArgs e) {
        WriteInfosBack();
        scriptNo = 2;
        ShowScript();
    }

    private void btnTabelleKopf_Click(object sender, System.EventArgs e) => InputBoxEditor.Edit(Table, typeof(TableHeadEditor), false);

    private void btnTestZeileDropDown_Click(object sender, System.EventArgs e) {
        if (Table is not { IsDisposed: false }) { return; }
        var items = TableScriptEditor.BuildRowDropdownItems(Table, txbChunk.Text);

        _dropDownTarget = txbTestZeile;
        var dropDown = TableScriptEditor.ShowScriptEditorDropDown(btnTestZeileDropDown, items, txbTestZeile.Text);
        dropDown?.ItemClicked += ScriptEditorDropDown_ItemClicked;
    }

    private void ScriptEditorDropDown_ItemClicked(object? sender, AbstractListItemEventArgs e) {
        if (_dropDownTarget is { IsDisposed: false } tbx && e.Item is { } item) {
            tbx.Text = item.KeyName;
        }
    }

    private void ShowScript() {
        if (_item is { } cpi) {
            tbcScriptEigenschaften.Enabled = true;

            switch (scriptNo) {
                case 1:
                    Script = cpi.Script_Before;
                    break;

                case 2:
                    Script = cpi.Script_MenuGeneration;
                    break;

                case 3:
                    Script = cpi.Script_After;
                    break;

                default:
                    tbcScriptEigenschaften.Enabled = false;
                    Script = string.Empty;
                    break;
            }
        } else {
            tbcScriptEigenschaften.Enabled = false;
            Script = string.Empty;
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