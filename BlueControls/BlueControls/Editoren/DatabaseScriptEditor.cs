// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueBasics.MultiUserFile;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript.Structures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class DatabaseScriptEditor : ScriptEditorGeneric, IHasDatabase {

    #region Fields

    private bool _allowTemporay;

    private Database? _database;

    private DatabaseScriptDescription? _item;

    private bool didMessage = false;

    private bool loaded = false;

    #endregion

    #region Constructors

    public DatabaseScriptEditor() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        tbcScriptEigenschaften.Enabled = false;
    }

    #endregion

    #region Properties

    public Database? Database {
        get => _database;
        private set {
            if (IsDisposed || (value?.IsDisposed ?? true)) { value = null; }
            if (value == _database) { return; }

            if (_database != null) {
                _database.DisposingEvent -= _database_Disposing;
                _database.CanDoScript -= Database_CanDoScript;
            }
            _database = value;

            if (_database != null) {
                _database.DisposingEvent += _database_Disposing;
                _database.CanDoScript += Database_CanDoScript;

                txbNeedFix.Text = _database.NeedsScriptFix;

                tbcScriptEigenschaften.Enabled = true;
            } else {
                tbcScriptEigenschaften.Enabled = false;
            }
        }
    }

    public DatabaseScriptDescription? Item {
        get => IsDisposed || Database is not { IsDisposed: false } ? null : _item;
        set {
            if (_item == value) { return; }
            if (IsDisposed || Database is not { IsDisposed: false }) { return; }

            WriteInfosBack();

            _item = null; // Um keine werte zurück zu Schreiben werden des anzeigen

            if (value != null) {
                tbcScriptEigenschaften.Enabled = true;
                tbcScriptEigenschaften.Enabled = true;
                txbName.Text = value.KeyName;
                txbQuickInfo.Text = value.ColumnQuickInfo;

                cbxPic.Text = value.Image;

                chkZeile.Checked = value.NeedRow;
                txbTestZeile.Enabled = value.NeedRow;
                chkAuslöser_newrow.Checked = value.EventTypes.HasFlag(ScriptEventTypes.InitialValues);
                chkAuslöser_valuechanged.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed);
                chkExtendend.Visible = chkAuslöser_valuechanged.Checked;
                chkAuslöser_valuechangedThread.Checked = value.EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread);
                chkAuslöser_prepaireformula.Checked = value.EventTypes.HasFlag(ScriptEventTypes.prepare_formula);
                chkAuslöser_databaseloaded.Checked = value.EventTypes.HasFlag(ScriptEventTypes.loaded);
                chkAuslöser_export.Checked = value.EventTypes.HasFlag(ScriptEventTypes.export);
                chkAuslöser_deletingRow.Checked = value.EventTypes.HasFlag(ScriptEventTypes.row_deleting);
                //chkAuslöser_Fehlerfrei.Checked = value.EventTypes.HasFlag(ScriptEventTypes.correct_changed);
                //chkAendertWerte.Checked = value.ChangeValuesAllowed;
                Script = value.Script;

                lstPermissionExecute.ItemClear();
                var l = Table.Permission_AllUsed(false).ToList();
                _ = l.AddIfNotExists(Administrator);
                lstPermissionExecute.ItemAddRange(l);
                lstPermissionExecute.Check(value.UserGroups);
                lstPermissionExecute.Suggestions.Clear();

                _item = value;

                btnVerlauf.Enabled = _item != null;
            } else {
                tbcScriptEigenschaften.Enabled = false;
                tbcScriptEigenschaften.Enabled = false;
                txbTestZeile.Enabled = false;

                txbName.Text = string.Empty;
                cbxPic.Text = string.Empty;
                txbQuickInfo.Text = string.Empty;
                Script = string.Empty;
                chkAuslöser_newrow.Checked = false;
                chkAuslöser_valuechanged.Checked = false;
                chkAuslöser_prepaireformula.Checked = false;
                chkAuslöser_valuechangedThread.Checked = false;
                chkAuslöser_databaseloaded.Checked = false;
                chkAuslöser_export.Checked = false;
                btnVerlauf.Enabled = false;
            }

            UpdateValues();
        }
    }

    public override object? Object {
        get => Database;
        set => Database = value is Database db ? db : null;
    }

    /// <summary>
    /// Nur zum setzen der Zeile zum Testen.
    /// </summary>
    public RowItem? Row {
        set {
            txbTestZeile.Text = value?.CellFirstString() ?? string.Empty;
            Database = value?.Database;
        }
    }

    #endregion

    #region Methods

    public override ScriptEndedFeedback ExecuteScript(bool testmode) {
        if (IsDisposed || Database is not { IsDisposed: false } db) {
            return new ScriptEndedFeedback("Keine Datenbank geladen.", false, false, "Allgemein");
        }

        if (_item == null) {
            return new ScriptEndedFeedback("Kein Skript gewählt.", false, false, "Allgemein");
        }

        if (!_item.IsOk()) {
            return new ScriptEndedFeedback("Bitte zuerst den Fehler korrigieren: " + _item.ErrorReason(), false, false,
                "Allgemein");
        }

        WriteInfosBack();

        RowItem? r = null;

        if (_item.NeedRow) {
            if (db.Row.Count == 0) {
                return new ScriptEndedFeedback("Zum Test wird zumindest eine Zeile benötigt.", false, false,
                    "Allgemein");
            }

            if (string.IsNullOrEmpty(txbTestZeile.Text)) {
                txbTestZeile.Text = db.Row.First()?.CellFirstString() ?? string.Empty;
            }

            r = db.Row[txbTestZeile.Text] ?? db.Row.SearchByKey(txbTestZeile.Text);
            if (r is not { IsDisposed: false }) {
                return new ScriptEndedFeedback("Zeile nicht gefunden.", false, false, "Allgemein");
            }
        }

        if (!testmode) {
            if (MessageBox.Show("Skript ändert Werte!<br>Fortfahren?", ImageCode.Warnung, "Fortfahren", "Abbruch") != 0) {
                return new ScriptEndedFeedback("Abbruch.", false, false, "Allgemein");
            }
        }

        var ext = chkExtendend is { Checked: true, Visible: true };

        _allowTemporay = true;
        var f = db.ExecuteScript(_item, !testmode, r, null, true, ext, true);
        _allowTemporay = false;

        return f;
    }

    public void UpdateValues(string? keyName = null, string? quickInfo = null, string? image = null, bool? needRow = null, ScriptEventTypes? eventTypes = null, string? script = null, ReadOnlyCollection<string>? userGroups = null, string? adminInfo = null, Database? database = null) {
        if (_item == null) {
            capFehler.Text = string.Empty;
            return;
        }

        // Entferne das alte Item anhand der Objektreferenz, nicht nur des KeyNames
        ReadableListItem? itemToRemove = null;
        foreach (var listItem in lstEventScripts.Items) {
            if (listItem is ReadableListItem rli && ReferenceEquals(rli.Item, _item)) {
                itemToRemove = rli;
                break;
            }
        }

        if (itemToRemove != null) {
            lstEventScripts.Remove(itemToRemove);
        } else {
            // Fallback: Entferne nach KeyName (nur wenn nicht leer)
            if (!string.IsNullOrEmpty(_item.KeyName)) {
                lstEventScripts.Remove(_item.KeyName);
            }
        }

        //DatabaseScriptDescription? newItem = null;
        //if (lstEventScripts.Checked.Count == 1 &&
        //    !TableView.ErrorMessage(Database, EditableErrorReasonType.EditNormaly)) {
        //    if (lstEventScripts[lstEventScripts.Checked[0]] is ReadableListItem selectedlstEventScripts) {
        //        newItem = selectedlstEventScripts.Item as DatabaseScriptDescription;
        //    }
        //}

        //Item = newItem;

        _item = new DatabaseScriptDescription(adminInfo ?? _item.AdminInfo,
                                             image ?? _item.Image,
                                             keyName ?? _item.KeyName,
                                             quickInfo ?? _item.ColumnQuickInfo,
                                             script ?? _item.Script,
                                             userGroups ?? _item.UserGroups,
                                             database ?? _item.Database,
                                             eventTypes ?? _item.EventTypes,
                                             needRow ?? _item.NeedRow);

        AddTolist(_item);

        if (_item.IsOk()) {
            capFehler.Text = "<imagecode=Häkchen|16> Keine Skript-Konflikte.";
            return;
        }

        capFehler.Text = "<imagecode=Warnung|16> " + _item.ErrorReason();
    }

    //    switch (e.Item.ToLowerInvariant()) {
    //        case "spalteneigenschaftenbearbeiten":
    //            if (c != null && !c.IsDisposed) {
    //                TableView.OpenColumnEditor(c, null, null);
    //            }
    public override void WriteInfosBack() {
        if (IsDisposed || TableView.EditabelErrorMessage(Database) || Database is not
            {
                IsDisposed: false
            }) { return; }

        UpdateValues(script: Script);

        #region Items sicherheitshalber in die Datenbank zurück schreiben, nur so werden die gelöschten und neuen erfasst

        var t2 = new List<DatabaseScriptDescription>();

        foreach (var thisItem in lstEventScripts.Items) {
            if (thisItem is ReadableListItem { Item: DatabaseScriptDescription dbs }) { t2.Add(dbs); }
        }

        //t2.AddRange(lstEventScripts.Items.Select(thisItem => (DatabaseScriptDescription)((ReadableListItem)thisItem).Item));
        Database.EventScript = new(t2);

        #endregion
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        WriteInfosBack();

        if (Database is { IsDisposed: false } db) {
            //if (!db.IsEventScriptCheckeIn()) {
            //    if (MessageBox.Show("Es sind nicht aktiv geschaltene\r\nBearbeitungen vorhanden.", ImageCode.Information, "Ok", "Aktiv schalten") == 1) {
            //        db.EventScript = db.EventScriptEdited;
            //    }
            //}

            if (!string.IsNullOrEmpty(db.NeedsScriptFix)) {
                if (MessageBox.Show("Fehlerspeicher ist nicht geleert.", ImageCode.Warnung, "Ok", "Leeren") == 1) {
                    db.NeedsScriptFix = db.CheckScriptError();
                }
            }
        }

        Item = null; // erst das Item!
        Database = null;
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        var im = QuickImage.Images();
        foreach (var thisIm in im) {
            cbxPic.ItemAdd(ItemOf(thisIm, thisIm, QuickImage.Get(thisIm, 16)));
        }

        UpdateList();
    }

    private void _database_Disposing(object sender, System.EventArgs e) {
        Database = null;
        Close();
    }

    private void AddTolist(DatabaseScriptDescription? thisSet) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        if (thisSet != null) {
            var cap = "Sonstige";

            if (thisSet.EventTypes != 0) { cap = thisSet.EventTypes.ToString(); }

            var it = ItemOf(thisSet);
            it.UserDefCompareKey = cap + SecondSortChar + thisSet.CompareKey;

            lstEventScripts.ItemAdd(it);

            if (lstEventScripts[cap] == null) {
                lstEventScripts.ItemAdd(ItemOf(cap, cap, true, cap + FirstSortChar));
            }

            if (!didMessage && thisSet.NeedRow && !Database.IsRowScriptPossible(false)) {
                didMessage = true;
                _ = EnableScript();
            }
        }
    }

    private void btnDatenbankKopf_Click(object sender, System.EventArgs e) => InputBoxEditor.Show(Database, typeof(DatabaseHeadEditor), false);

    private void btnDeleteNeedsScriptFix_Click(object sender, System.EventArgs e) {
        if (_database == null) { return; }

        _database.NeedsScriptFix = _database.CheckScriptError();

        txbNeedFix.Text = _database.NeedsScriptFix;
        MultiUserFile.SaveAll(false);
    }

    //private void btnScriptÜbertragen_Click(object sender, System.EventArgs e) {
    //    if (_database == null) { return; }

    //    if (MessageBox.Show("Ihre Bearbeitungen werden Produktiv gesetzt!", ImageCode.Warnung, "OK", "Abbrechen") != 0) { return; }
    //    Item = null;
    //    _database.EventScript = _database.EventScriptEdited;
    //    MultiUserFile.SaveAll(false);
    //    UpdateList();
    //}

    //private void btnSkriptÄnderungVerwerfen_Click(object sender, System.EventArgs e) {
    //    if (_database == null) { return; }

    //    if (MessageBox.Show("Ihre Bearbeitungen gehen verloren!", ImageCode.Warnung, "OK", "Abbrechen") != 0) { return; }
    //    Item = null;
    //    _database.EventScriptEdited = _database.EventScript;
    //    MultiUserFile.SaveAll(false);
    //    UpdateList();
    //}

    private void btnSpaltenuebersicht_Click(object sender, System.EventArgs e) => Database?.Column.GenerateOverView();

    private void btnTest_Click(object sender, System.EventArgs e) {
        if (!loaded && _database != null && _database.Row.Count == 0) {
            loaded = true;
            _ = _database.BeSureAllDataLoaded(10);
        }
        TesteScript(true);
    }

    private void btnVerlauf_Click(object sender, System.EventArgs e) {
        // Überprüfen, ob die Datenbank oder die Instanz selbst verworfen wurde
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
        // Überprüfen, ob die Tabelle bearbeitet werden kann
        if (TableView.EditabelErrorMessage(Database)) {
            Item = null;
            btnVerlauf.Enabled = false;
            return;
        }
        // Das ausgewählte Skript aus der Liste abrufen
        var selectedlstEventScripts = lstEventScripts[lstEventScripts.Checked[0]] is ReadableListItem item ? (DatabaseScriptDescription)item.Item : null;
        var l = new List<string>();
        // Durchlaufen aller Undo-Operationen in der Datenbank
        foreach (var thisUndo in db.Undo) {
            if (thisUndo.Command is DatabaseDataType.EventScript) {
                l.Add("############################################################################");
                l.Add("############################################################################");
                l.Add("############################################################################");
                l.Add("############################################################################");
                l.Add("############################################################################");
                l.Add(thisUndo.DateTimeUtc.ToString("dd.MM.yyyy HH:mm:ss.fff") + " " + thisUndo.User);

                l.Add("Art: " + thisUndo.Command.ToString());
                // Überprüfen, ob das Skript geändert wurde
                var ai = thisUndo.ChangedTo.SplitAndCutByCr().ToList();
                var found = false;
                foreach (var t in ai) {
                    var s = new DatabaseScriptDescription(db, t);
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
        }
        // Schreiben der Liste in eine temporäre Datei
        _ = l.WriteAllText(TempFile(string.Empty, "Scrip.txt"), Win1252, true);
    }

    private void btnVersionErhöhen_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        btnVersionErhöhen.Enabled = false;

        Database.EventScriptVersion = DateTime.UtcNow;
    }

    private void btnZusatzDateien_Click(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false } db) { return; }
        _ = ExecuteFile(db.AdditionalFilesPathWhole());
    }

    private void cbxPic_TextChanged(object sender, System.EventArgs e) => UpdateValues(image: cbxPic.Text.TrimEnd("|16"));

    private void chkAuslöser_newrow_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        ScriptEventTypes tmp = 0;
        if (chkAuslöser_newrow.Checked) { tmp |= ScriptEventTypes.InitialValues; }
        if (chkAuslöser_valuechanged.Checked) { tmp |= ScriptEventTypes.value_changed; }
        if (chkAuslöser_prepaireformula.Checked) { tmp |= ScriptEventTypes.prepare_formula; }
        if (chkAuslöser_valuechangedThread.Checked) { tmp |= ScriptEventTypes.value_changed_extra_thread; }
        if (chkAuslöser_databaseloaded.Checked) { tmp |= ScriptEventTypes.loaded; }
        if (chkAuslöser_export.Checked) { tmp |= ScriptEventTypes.export; }
        if (chkAuslöser_deletingRow.Checked) { tmp |= ScriptEventTypes.row_deleting; }

        UpdateValues(eventTypes: tmp);
    }

    private void chkZeile_CheckedChanged(object sender, System.EventArgs e) {
        if (Item == null) { return; }

        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        if (chkZeile.Checked && !Database.IsRowScriptPossible(false)) {
            if (!EnableScript()) { chkZeile.Checked = false; }

            return;
        }

        UpdateValues(needRow: chkZeile.Checked);
        txbTestZeile.Enabled = chkZeile.Checked;
    }

    private void Database_CanDoScript(object sender, CancelReasonEventArgs e) {
        if (_allowTemporay) { return; }
        e.CancelReason = "Skript-Editor geöffnet";
    }

    private bool EnableScript() {
        if (IsDisposed || Database is not { IsDisposed: false }) { return false; }

        var s = MessageBox.Show("Für Zeilenskripte werden bestimmte Systemspalten benötigt.<br>Sollen diese erstellt werden?", ImageCode.Spalte, "Ja", "Nein");

        if (s == 1) { return false; }

        Database.EnableScript();

        if (!Database.IsRowScriptPossible(false)) {
            MessageBox.Show("Systemspalten konnten nicht erstellt werden.", ImageCode.Information, "Ok");
            return false;
        }
        return true;
    }

    private void GlobalTab_SelectedIndexChanged(object sender, System.EventArgs e) => WriteInfosBack();

    private void lstEventScripts_AddClicked(object sender, System.EventArgs e) {
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        // Erstelle neues Script, aber füge es NICHT direkt zur Liste hinzu
        var newScript = new DatabaseScriptDescription(Database);

        // Setze das neue Script als aktuelles Item
        _item = newScript;

        // WriteInfosBack wird das neue Script korrekt zur Liste hinzufügen
        WriteInfosBack();

        // Suche das hinzugefügte Item und wähle es aus
        foreach (var item in lstEventScripts.Items) {
            if (item is ReadableListItem rli && rli.Item == newScript) {
                lstEventScripts.Check(rli);
                break;
            }
        }
    }

    private void lstEventScripts_ItemCheckedChanged(object sender, System.EventArgs e) {
        DatabaseScriptDescription? newItem = null;
        if (lstEventScripts.Checked.Count == 1 &&
            !TableView.EditabelErrorMessage(Database)) {
            if (lstEventScripts[lstEventScripts.Checked[0]] is ReadableListItem selectedlstEventScripts) {
                newItem = selectedlstEventScripts.Item as DatabaseScriptDescription;
            }
        }

        Item = newItem;
    }

    private void lstPermissionExecute_ItemClicked(object sender, AbstractListItemEventArgs e) => UpdateValues(userGroups: lstPermissionExecute.Checked.ToList().AsReadOnly());

    private void txbName_TextChanged(object sender, System.EventArgs e) => UpdateValues(keyName: txbName.Text);

    private void txbQuickInfo_TextChanged(object sender, System.EventArgs e) => UpdateValues(quickInfo: txbQuickInfo.Text);

    private void UpdateList() {
        lstEventScripts.ItemClear();
        if (IsDisposed || Database is not { IsDisposed: false }) { return; }

        foreach (var thisSet in Database.EventScript) {
            AddTolist(thisSet);
        }
    }

    #endregion

    //private void scriptEditor_ContextMenuInit(object sender, ContextMenuInitEventArgs e) {
    //    //Todo: Implementieren
    //    if (e.HotItem is string txt) {
    //        var c = Database?.Column[txt);
    //        if (c is null) { return; }
    //       e.CurrentMenu.Add(Add(ContextMenuCommands.SpaltenEigenschaftenBearbeiten);
    //    }
    //}

    //private void scriptEditor_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
    //    //Todo: Implementieren
    //    ColumnItem? c = null;

    //    if (e.HotItem is string txt) { c = Database?.Column[txt); }
}