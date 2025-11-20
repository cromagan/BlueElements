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
using BlueControls.BlueTableDialogs;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using BlueTable;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using static BlueScript.Script;

namespace BlueControls.Controls;

public partial class RowAdder : GenericControlReciverSender, IOpenScriptEditor // System.Windows.Forms.UserControl//,
    {
    #region Fields

    private bool _ignoreCheckedChanged;

    private List<string>? _infos;

    private string _lastGeneratedEntityId = string.Empty;

    private RowItem? _lastRow;

    private List<string>? _menu;

    private bool _mustUpdate = true;

    #endregion

    #region Constructors

    public RowAdder() : base(false, false, false) => InitializeComponent();

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? AdditionalInfoColumn { get; internal set; }

    /// <summary>
    /// Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.
    /// Dadurch können verschiedene Datensätze gespeichert werden.
    /// Beispiele: Rezepetname, Personenname, Beleg-Nummer
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string EntityID { get; internal set; } = string.Empty;

    /// <summary>
    /// Eine Spalte in der Ziel-Tabelle.
    /// In diese wird die generierte ID des klickbaren Elements gespeichert.
    /// Diese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.
    /// Beispiel: Zutaten#Vegetarisch/Mehl#3FFDKKJ34fJ4#1
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? OriginIDColumn { get; internal set; }

    [DefaultValue("")]
    public string Script_After { get; set; } = string.Empty;

    [DefaultValue("")]
    public string Script_Before { get; set; } = string.Empty;

    [DefaultValue("")]
    public string Script_MenuGeneration { get; set; } = string.Empty;

    #endregion

    #region Methods

    public static ScriptEndedFeedback ExecuteScript(string scripttext, string mode, string entitiId, RowItem rowIn, bool isMenuGeneration, string info) {
        var generatedentityID = rowIn.ReplaceVariables(entitiId, true, null);

        VariableCollection vars =
        [
            new VariableString("Application", Develop.AppName(), true, "Der Name der App, die gerade geöffnet ist."),
            new VariableString("User", Generic.UserName, true,
                "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."),

            new VariableString("Usergroup", Generic.UserGroup, true,
                "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."),
            //vars.Add(new VariableListString("CurrentlySelected", selected, true, "Was der Benutzer aktuell angeklickt hat."));
            new VariableString("EntityId", generatedentityID, true, "Dies ist die Eingangsvariable."),
            new VariableString("Mode", mode, true, "In welchem Modus die Formulare angezeigt werden.")
        ];

        if (isMenuGeneration) {
            vars.Add(new VariableListString("Menu", null, false, "Diese Variable muss das Rückgabemenü enthalten."));
            vars.Add(new VariableListString("Infos", null, false, "Diese Variable kann Zusatzinfos zum Menu enthalten."));
        }

        var m = Method.GetMethods(MethodType.LongTime); // Ja, Longtime... andere Tabellen müssen geladen werden.

        var scp = new ScriptProperties("Row-Adder", m, true, [], rowIn, "Row-Adder", "Row-Adder");

        var sc = new Script(vars, scp) {
            ScriptText = scripttext
        };

        AbortReason? abr = rowIn.Table is { } tbl ? tbl.ExternalAbortScriptReason : null;
        var scf = sc.Parse(0, "Main", null, abr);

        if (scf.Failed) {
            if (Generic.UserGroup == Constants.Administrator) {
                List<string> l =
                [
                    "### ACHTUNG - EINMALIGE ANZEIGE ###",
                    info,
                    generatedentityID,
                    //"Der Fehlerspeicher wird jetzt gelöscht. Es kann u.U. länger dauern, bis der Fehler erneut auftritt.",
                    //"Deswegen wäre es sinnvoll, den Fehler jetzt zu reparieren.",
                    //"Tabelle: " + Table.Caption,
                    " ",
                    " ",
                    //"Letzte Fehlermeldung, die zum Deaktivieren des Skriptes führte:",
                    " ",
                    scf.ProtocolText
                ];
                l.WriteAllText(TempFile(string.Empty, string.Empty, "txt"), Constants.Win1252, true);
            }
        }

        return scf;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="textkey"></param>
    /// <returns>PFAD1\\PFAD2\\PFAD3\\</returns>
    public static string RepairTextKey(string textkey) {
        var nt = textkey.Replace("/", "\\");
        nt = nt.Replace("\\\\\\\\", "\\");
        nt = nt.Replace("\\\\\\", "\\");
        nt = nt.Replace("\\\\", "\\");
        nt = nt.Replace("\\\\", "\\");
        nt = nt.Trim("\\");

        //if (ucase) { nt = nt.ToUpper(); }
        nt = nt.RemoveChars(Constants.Char_PfadSonderZeichen);
        return nt;
    }

    public void Fehler(string txt, ImageCode symbol) {
        Enabled = false;
        _lastGeneratedEntityId = string.Empty;
        _menu = null;
        f.ItemClear();
        f.ItemAdd(ItemOf(txt, symbol));
        FilterOutput.ChangeTo(new FilterItem(null, "RowCreator"));
        _ignoreCheckedChanged = false;
    }

    public void OpenScriptEditor() {
        if (IsDisposed || GeneratedFrom is not RowAdderPadItem { IsDisposed: false } it) { return; }

        var se = IUniqueWindowExtension.ShowOrCreate<RowAdderScriptEditor>(it);
        se.Row = _lastRow;
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled && !_mustUpdate) { return; }

        _mustUpdate = false;

        DoInputFilter(null, true);
        RowsInputChangedHandled = true;

        if (_ignoreCheckedChanged) {
            Develop.DebugPrint("Liste wird bereits erstellt!");
            return;
        }
        _ignoreCheckedChanged = true;

        if (string.IsNullOrEmpty(EntityID)) {
            Fehler("Interner Fehler: EnitiyID", ImageCode.Kritisch);
            return;
        }

        if (OriginIDColumn == null) {
            Fehler("Interner Fehler: OriginIDColumn", ImageCode.Kritisch);
            return;
        }

        var rowIn = RowSingleOrNull();

        if (rowIn != _lastRow) {
            _menu = null;
            _lastRow = rowIn;
            _lastGeneratedEntityId = string.Empty;
        }

        if (rowIn == null) {
            Fehler("Keine Wahl getroffen", ImageCode.Information);
            return;
        }

        RowCollection.InvalidatedRowsManager.DoAllInvalidatedRows(_lastRow, true, null);

        var nowGeneratedId = GenerateEntityID(rowIn);

        if (!string.IsNullOrEmpty(nowGeneratedId.msg)) { Fehler(nowGeneratedId.msg, ImageCode.Kritisch); return; }

        if (string.IsNullOrEmpty(Script_MenuGeneration)) {
            Fehler("Interner Fehler: Kein Skript vorhanden", ImageCode.Kritisch);
            return;
        }

        FilterOutput.ChangeTo(new FilterItem(OriginIDColumn, FilterType.BeginntMit, nowGeneratedId.newid + "\\"));

        var selected = OriginIDColumn.Contents(FilterOutput, null);
        selected = selected.Select(s => s.TrimStart(nowGeneratedId.newid + "\\").Trim("\\")).ToList().SortedDistinctList();

        selected = RepairMenu(selected);

        var msg = GenerateMenuItems(rowIn);

        if (!string.IsNullOrEmpty(msg)) { Fehler(msg, ImageCode.Kritisch); return; }

        Enabled = true;

        _lastGeneratedEntityId = nowGeneratedId.newid;

        var olditems = f.Items.ToListOfString().Select(s => s.Trim(_lastGeneratedEntityId + "\\")).ToList();

        foreach (var thisIT in f.Items) {
            if (thisIT is ReadableListItem { Item: AdderItem ai }) {
                ai.KeysAndInfo.Clear();
            }
            if (thisIT is DropDownListItem dli) {
                dli.DropDownItems.Clear();
            }
        }

        var keyAndInfo = RepairMenu([.. _menu ?? [], .. selected], _infos);

        for (var z = 0; z < keyAndInfo.Count; z++) {
            var key = keyAndInfo[z].SplitBy("#")[0];

            if (!ShowMe(selected, key)) { continue; }

            var check_Item = f.Items.GetByKey(key);

            var dd_Name = key.PathParent().Trim("\\") + "~DD~";
            var dd_BoxItem = f.Items.GetByKey(dd_Name);
            var dd_isItem = key.EndsWith("+") && !selected.Contains(key);

            if (check_Item is ReadableListItem rli && !dd_isItem) {

                #region Item vorhanden. Weiteren Adder hinzufügen

                if (rli.Item is AdderItem ai) {
                    ai.KeysAndInfo.Add(keyAndInfo[z]);
                }
                rli.UserDefCompareKey = z.ToStringInt10();

                rli.Enabled = !HasChildNode(selected, key);

                olditems.Remove(key);

                #endregion
            } else if (dd_BoxItem is DropDownListItem dli && dd_isItem) {

                #region das Item ist ein Objekt unter einem Dropdown und NICHT separat gewählt.

                var vorhandenDD = dli.DropDownItems.GetByKey(key);

                if (vorhandenDD is ReadableListItem rliDD) {

                    #region DD-Item vorhanden. Weiteren Adder hinzufügen

                    if (rliDD.Item is AdderItem aiDD) {
                        aiDD.KeysAndInfo.Add(keyAndInfo[z]);
                    }
                    dli.UserDefCompareKey = z.ToStringInt10();
                    dli.Enabled = !HasChildNode(selected, key);

                    #endregion
                } else {
                    var naiDD = new AdderItem(key);
                    naiDD.KeysAndInfo.Add(keyAndInfo[z]);
                    var itDD = ItemOf(naiDD);

                    dli.DropDownItems.Add(itDD);
                    dli.Enabled = !HasChildNode(selected, key);
                }
                olditems.Remove(dd_Name);

                #endregion
            } else {

                #region Das Item ist neu. Einen einen Listen-Eintrag erstellen

                #region Item allgemein erstellen

                var nai = new AdderItem(key);
                nai.KeysAndInfo.Add(keyAndInfo[z]);
                var it = ItemOf(nai);

                #endregion

                if (!dd_isItem) {

                    #region ....normal hinzufügen

                    it.Indent = Math.Max(keyAndInfo[z].CountChar('\\'), 0);
                    f.ItemAdd(it);
                    olditems.Remove(key);
                    it.UserDefCompareKey = z.ToStringInt10();
                    it.Enabled = !HasChildNode(selected, key);

                    #endregion
                } else {

                    #region ... als Dropdownmenu hinzufügen

                    var ndli = new DropDownListItem(dd_Name, true, string.Empty);
                    ndli.DropDownItems.Add(it);
                    ndli.Indent = Math.Max(keyAndInfo[z].CountChar('\\'), 0);
                    f.ItemAdd(ndli);
                    olditems.Remove(dd_Name);
                    ndli.UserDefCompareKey = z.ToStringInt10();
                    ndli.Enabled = !HasChildNode(selected, key);

                    #endregion
                }

                #endregion
            }
        }

        foreach (var thisit in olditems) {
            f.Remove(thisit);
        }

        f.UncheckAll();
        f.Check(selected);

        _ignoreCheckedChanged = false;
    }

    private void DropDownMenu_Cancel(object sender, System.EventArgs e) {
        //FillListBox();
        _mustUpdate = true;
        Invalidate();
    }

    private void DropDownMenu_ItemClicked(object sender, AbstractListItemEventArgs e) {
        FloatingForm.Close(this);

        if (e.Item is ReadableListItem { Item: AdderItem ai }) {
            AdderItem.AddRowsToTable(OriginIDColumn, ai.KeysAndInfo, _lastGeneratedEntityId, AdditionalInfoColumn);
        }

        _mustUpdate = true;
        //FillListBox();
        Invalidate();
    }

    private void F_ItemClicked(object sender, AbstractListItemEventArgs e) {
        if (_ignoreCheckedChanged) { return; }

        if (RowSingleOrNull() is not { IsDisposed: false } rowIn) { return; }

        //        var scf = ExecuteScript(Script_MenuGeneration, Mode, EntityID, rowIn, true, "MenuGeneration");

        var scf = ExecuteScript(Script_Before, Mode, EntityID, rowIn, false, "Before");
        if (scf.Failed) {
            Fehler("Interner Fehler: Skript BEFORE fehlerhaft", ImageCode.Kritisch);
            return;
        }

        if (e.Item is ReadableListItem { Item: AdderItem ai } rli) {
            if (f.Checked.Contains(rli.KeyName)) {
                AdderItem.AddRowsToTable(OriginIDColumn, ai.KeysAndInfo, _lastGeneratedEntityId, AdditionalInfoColumn);
            } else {
                AdderItem.RemoveRowsFromTable(OriginIDColumn, _lastGeneratedEntityId, ai.KeyName);
            }
            //FillListBox();
            _mustUpdate = true;
            Invalidate();
        }

        if (e.Item is DropDownListItem dli) {
            var x = Cursor.Position.X - MousePos().X + dli.Position.X + (dli.Indent * 20);
            var y = Cursor.Position.Y - MousePos().Y + dli.Position.Bottom; //Identisch

            var dropDownMenu = FloatingInputBoxListBoxStyle.Show(dli.DropDownItems, CheckBehavior.SingleSelection, null, x, y, dli.Position.Width, null, this, false, ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, true);
            dropDownMenu.Cancel += DropDownMenu_Cancel;
            dropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
        }

        scf = ExecuteScript(Script_After, Mode, EntityID, rowIn, false, "After");
        if (scf.Failed) {
            Fehler("Interner Fehler: Skript AFTER fehlerhaft", ImageCode.Kritisch);
            return;
        }
    }

    private (string msg, string newid) GenerateEntityID(RowItem rowIn) {
        if (!string.IsNullOrEmpty(_lastGeneratedEntityId)) { return (string.Empty, _lastGeneratedEntityId); }

        var generatedentityID = rowIn.ReplaceVariables(EntityID, true, null);

        if (generatedentityID == EntityID || string.IsNullOrEmpty(generatedentityID)) {
            return ("Interner Fehler: EnitiyID", string.Empty);
        }

        if (generatedentityID.Contains("\\")) { return ("Interner Fehler: Ungültiges Zeichen (\\) in  EnitiyID", string.Empty); }
        if (generatedentityID.Contains("#")) { return ("Interner Fehler: Ungültiges Zeichen (#) in  EnitiyID", string.Empty); }
        if (generatedentityID.Contains("~")) { return ("Interner Fehler: Ungültiges Zeichen (~) in  EnitiyID", string.Empty); }
        if (generatedentityID.Contains("*")) { return ("Interner Fehler: Ungültiges Zeichen (*) in  EnitiyID", string.Empty); }

        return (string.Empty, generatedentityID);
    }

    private string GenerateMenuItems(RowItem rowIn) {
        if (_menu != null) { return string.Empty; }

        _infos = [];

        var scf = ExecuteScript(Script_MenuGeneration, Mode, EntityID, rowIn, true, "MenuGeneration");

        if (scf.Failed) {
            return "Interner Fehler: Skript Menu Generation fehlerhaft; " + scf.ProtocolText;
        }

        var menu = scf.Variables?.GetList("Menu");

        if (menu is not { Count: >= 1 }) {
            return "Interner Fehler: Skript gab kein Menu zurück";
        }

        foreach (var item in menu) {
            if (item.Contains("*")) { return "Interner Fehler: Menüpunkte dürfen kein * enthalten"; }
            if (item.Contains(";")) { return "Interner Fehler: Menüpunkte dürfen kein ; enthalten"; }
            if (item.Contains("#")) { return "Interner Fehler: Menüpunkte dürfen kein # enthalten"; }
            if (item.Contains("~")) { return "Interner Fehler: Menüpunkte dürfen kein ~ enthalten"; }
        }

        var infos = scf.Variables?.GetList("Infos") ?? [];

        foreach (var item in infos) {
            if (item.Contains("*")) { return "Interner Fehler: Infos dürfen kein * enthalten"; }
            //if (item.Contains(";")) { return "Interner Fehler: Menüpunkte dürfen kein ; enthalten"; }
            if (item.Contains("#")) { return "Interner Fehler: Infos dürfen kein # enthalten"; }
            if (item.Contains("~")) { return "Interner Fehler: Infos dürfen kein ~ enthalten"; }
            if (item.Contains("\\")) { return "Interner Fehler: Infos dürfen kein \\ enthalten"; }

            if (!string.IsNullOrEmpty(item) && AdditionalInfoColumn == null) {
                return "Interner Fehler: Für Infos muss eine Zusatzspalte vorhanden sein";
            }
        }

        if (AdditionalInfoColumn != null && menu.Count != infos.Count) {
            return "Interner Fehler: Infos und Menuitems ungleich";
        }

        _menu = menu;
        _infos = infos;

        return string.Empty;
    }

    private bool HasChildNode(List<string> selected, string key) {
        foreach (var thisS in selected) {
            if (thisS.StartsWith(key + "\\")) { return true; }
        }
        return false;
    }

    private List<string> RepairMenu(List<string> menu) {
        var m = new List<string>();

        foreach (var thisMenu in menu) {
            var t = thisMenu.Trim("\\").SplitBy("\\");

            var n = string.Empty;
            foreach (var item in t) {
                n = (n + "\\" + item).Trim("\\");
                m.Add(n);
            }
        }
        return m.SortedDistinctList();
    }

    private List<string> RepairMenu(List<string> menu, List<string>? infos) {
        infos ??= [];

        while (infos.Count < menu.Count) { infos.Add(string.Empty); }

        var m = new List<string>();

        for (var z = 0; z < menu.Count; z++) {
            menu[z] = menu[z].Trim("\\");
        }

        for (var z2 = 0; z2 < menu.Count; z2++) {
            var t = menu[z2].SplitBy("\\");

            var n = string.Empty;
            foreach (var item in t) {
                n = (n + "\\" + item).Trim("\\");

                if (!menu.Contains(n)) {
                    // Nur Fehlende aufnehmen. Die existenten werden am Schluß eh hinzugefügt
                    m.Add(n + "#");
                }
            }

            m.Add(menu[z2] + "#" + infos[z2]);
        }

        return m.SortedDistinctList();
    }

    private bool Selected(ICollection<string> selected, string textkey) => selected.Contains(RepairTextKey(textkey));

    private bool ShowMe(ICollection<string> selected, string textkey) {
        var t = RepairTextKey(textkey);
        return t.CountChar('\\') < 1 || Selected(selected, t) || Selected(selected, t.PathParent());
    }

    #endregion
}