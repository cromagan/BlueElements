// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueDatabase;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Windows.Forms;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using System.Linq;
using BlueScript.Variables;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueBasics.Enums;
using BlueControls.ItemCollectionList;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.EventArgs;
using BlueDatabase.AdditionalScriptMethods;

namespace BlueControls.Controls;

public partial class RowAdder : GenericControlReciverSender // System.Windows.Forms.UserControl,
    {
    #region Fields

    private bool _ignoreCheckedChanged = false;

    #endregion

    #region Constructors

    public RowAdder() : base(false, false) {
        InitializeComponent();
    }

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
    /// Eine Spalte in der Ziel-Datenbank.
    /// In diese wird die generierte ID des klickbaren Elements gespeichert.
    /// Diese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.
    /// Beispiel: Zutaten#Vegetarisch/Mehl#3FFDKKJ34fJ4#1
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? OriginIDColumn { get; internal set; }

    [DefaultValue("")]
    public string Script { get; set; } = string.Empty;

    protected string GeneratedEntityId { get; private set; } = string.Empty;

    #endregion

    #region Methods

    public static ScriptEndedFeedback ExecuteScript(string scripttext, List<string> selected, string entitiId, RowItem rowIn) {
        var generatedentityID = rowIn.ReplaceVariables(entitiId, false, true, null);

        var vars = new VariableCollection();
        vars.Add(new VariableString("Application", Develop.AppName(), true, "Der Name der App, die gerade geöffnet ist."));
        vars.Add(new VariableString("User", Generic.UserName, true, "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."));
        vars.Add(new VariableString("Usergroup", Generic.UserGroup, true, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."));
        vars.Add(new VariableListString("Menu", null, false, "Diese Variable muss das Rückgabemenü enthalten."));
        vars.Add(new VariableListString("Infos", null, false, "Diese Variable kann Zusatzinfos zum Menu enthalten."));
        //vars.Add(new VariableListString("CurrentlySelected", selected, true, "Was der Benutzer aktuell angeklickt hat."));
        vars.Add(new VariableString("EntityId", generatedentityID, true, "Dies ist die Eingangsvariable."));

        var scp = new ScriptProperties("Row-Adder", MethodType.Standard | MethodType.IO | MethodType.Database | MethodType.MyDatabaseRow, true, [], rowIn, 0);

        var sc = new BlueScript.Script(vars, string.Empty, scp);
        sc.ScriptText = scripttext;
        return sc.Parse(0, "Main", null);
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
        f.ItemClear();
        f.ItemAdd(ItemOf(txt, symbol));
        FilterOutput.ChangeTo(new FilterItem(null, "RowCreator"));
        _ignoreCheckedChanged = false;
    }




    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled && !_mustUpdate) { return; }

        _mustUpdate = false;

        if (_ignoreCheckedChanged) {
            Develop.DebugPrint("Liste wird bereits erstellt!");
            return;
        }
        _ignoreCheckedChanged = true;

        if (string.IsNullOrEmpty(EntityID)) {
            Fehler("Interner Fehler: EnitiyID", BlueBasics.Enums.ImageCode.Kritisch);
            return;
        }

        if (OriginIDColumn == null) {
            Fehler("Interner Fehler: OriginIDColumn", BlueBasics.Enums.ImageCode.Kritisch);
            return;
        }


        DoInputFilter(null, true);
        DoRows();
        var rowIn = RowSingleOrNull();

        if (rowIn == null) {
            Fehler("Keine Wahl getroffen", BlueBasics.Enums.ImageCode.Information);
            return;
        }

        GeneratedEntityId = string.Empty;

        var generatedentityID = rowIn.ReplaceVariables(EntityID, false, true, null);

        if (generatedentityID == EntityID) {
            Fehler("Interner Fehler: EnitiyID", BlueBasics.Enums.ImageCode.Kritisch);
            return;
        }

        if (generatedentityID.Contains("\\")) { Fehler("Interner Fehler: Ungültiges Zeichen (\\) in  EnitiyID", BlueBasics.Enums.ImageCode.Kritisch); return; }
        if (generatedentityID.Contains("#")) { Fehler("Interner Fehler: Ungültiges Zeichen (#) in  EnitiyID", BlueBasics.Enums.ImageCode.Kritisch); return; }
        if (generatedentityID.Contains("~")) { Fehler("Interner Fehler: Ungültiges Zeichen (~) in  EnitiyID", BlueBasics.Enums.ImageCode.Kritisch); return; }
        if (generatedentityID.Contains("*")) { Fehler("Interner Fehler: Ungültiges Zeichen (*) in  EnitiyID", BlueBasics.Enums.ImageCode.Kritisch); return; }

        if (string.IsNullOrEmpty(Script)) {
            Fehler("Interner Fehler: Kein Skript vorhanden", BlueBasics.Enums.ImageCode.Kritisch);
            return;
        }

        FilterOutput.ChangeTo(new FilterItem(OriginIDColumn, BlueDatabase.Enums.FilterType.BeginntMit, generatedentityID + "\\"));

        var selected = OriginIDColumn.Contents(FilterOutput, null);
        selected = selected.Select(s => s.TrimStart(generatedentityID + "\\").Trim("\\")).ToList().SortedDistinctList();

        selected = RepairMenu(selected);

        var scf = ExecuteScript(Script, selected, EntityID, rowIn);

        if (!scf.AllOk) {
            if (Generic.UserGroup == Constants.Administrator) {
                var l = new List<string> {
                "### ACHTUNG - EINMALIGE ANZEIGE ###",
                generatedentityID,
                //"Der Fehlerspeicher wird jetzt gelöscht. Es kann u.U. länger dauern, bis der Fehler erneut auftritt.",
                //"Deswegen wäre es sinnvoll, den Fehler jetzt zu reparieren.",
                //"Datenbank: " + Database.Caption,
                " ",
                " ",
                //"Letzte Fehlermeldung, die zum Deaktivieren des Skriptes führte:",
                " ",
               scf.ProtocolText
            };
                l.WriteAllText(TempFile("", "", "txt"), Constants.Win1252, true);
            }

            Fehler("Interner Fehler: Skript fehlerhaft; " + scf.ProtocolText, BlueBasics.Enums.ImageCode.Kritisch);
            return;
        }

        RowCollection.DoAllInvalidatedRows(null);

        var menu = scf.Variables?.GetList("Menu");

        if (menu == null || menu.Count < 1) {
            Fehler("Interner Fehler: Skript gab kein Menu zurück", BlueBasics.Enums.ImageCode.Kritisch);
            return;
        }

        foreach (var item in menu) {
            if (item.Contains("*")) { Fehler("Interner Fehler: Menüpunkte dürfen kein * enthalten", BlueBasics.Enums.ImageCode.Kritisch); return; }
            if (item.Contains(";")) { Fehler("Interner Fehler: Menüpunkte dürfen kein ; enthalten", BlueBasics.Enums.ImageCode.Kritisch); return; }
            if (item.Contains("#")) { Fehler("Interner Fehler: Menüpunkte dürfen kein # enthalten", BlueBasics.Enums.ImageCode.Kritisch); return; }
            if (item.Contains("~")) { Fehler("Interner Fehler: Menüpunkte dürfen kein ~ enthalten", BlueBasics.Enums.ImageCode.Kritisch); return; }
        }

        var infos = scf.Variables?.GetList("Infos") ?? new List<string>();

        foreach (var item in infos) {
            if (item.Contains("*")) { Fehler("Interner Fehler: Infos dürfen kein * enthalten", BlueBasics.Enums.ImageCode.Kritisch); return; }
            //if (item.Contains(";")) { Fehler("Interner Fehler: Menüpunkte dürfen kein ; enthalten", BlueBasics.Enums.ImageCode.Kritisch); return; }
            if (item.Contains("#")) { Fehler("Interner Fehler: Infos dürfen kein # enthalten", BlueBasics.Enums.ImageCode.Kritisch); return; }
            if (item.Contains("~")) { Fehler("Interner Fehler: Infos dürfen kein ~ enthalten", BlueBasics.Enums.ImageCode.Kritisch); return; }
            if (item.Contains("\\")) { Fehler("Interner Fehler: Infos dürfen kein \\ enthalten", BlueBasics.Enums.ImageCode.Kritisch); return; }

            if (!string.IsNullOrEmpty(item) && AdditionalInfoColumn == null) {
                Fehler("Interner Fehler: Für Infos muss eine Zusatzspalte vorhanden sein", BlueBasics.Enums.ImageCode.Kritisch);
                return;
            }
        }

        if (AdditionalInfoColumn != null && menu.Count != infos.Count) {
            Fehler("Interner Fehler: Infos und Menuitems ungleich", BlueBasics.Enums.ImageCode.Kritisch);
            return;
        }

        Enabled = true;

        GeneratedEntityId = generatedentityID;

        List<string> olditems = f.Items.ToListOfString().Select(s => s.Trim(generatedentityID + "\\")).ToList();

        foreach (var thisIT in f.Items) {
            if (thisIT is ItemCollectionList.ReadableListItem rli && rli.Item is AdderItem ai) {
                ai.KeysAndInfo.Clear();
            }
            if (thisIT is ItemCollectionList.DropDownListItem dli) {
                dli.DDItems.Clear();
            }
        }

        menu.AddRange(selected);

        var keyAndInfo = RepairMenu(menu, infos);

        for (var z = 0; z < keyAndInfo.Count; z++) {
            var key = keyAndInfo[z].SplitBy("#")[0];

            if (!ShowMe(selected, key)) { continue; }

            var check_Item = f.Items.Get(key);

            var dd_Name = key.PathParent().Trim("\\") + "~DD~";
            var dd_BoxItem = f.Items.Get(dd_Name);
            var dd_isItem = key.EndsWith("+") && !selected.Contains(key);

            if (check_Item is ItemCollectionList.ReadableListItem rli && !dd_isItem) {

                #region Item vorhanden. Weiteren Adder hinzufügen

                if (rli.Item is AdderItem ai) {
                    ai.KeysAndInfo.Add(keyAndInfo[z]);
                }
                rli.UserDefCompareKey = z.ToStringInt10();

                rli.Enabled = !HasChildNode(selected, key);

                olditems.Remove(key);

                #endregion
            } else if (dd_BoxItem is ItemCollectionList.DropDownListItem dli && dd_isItem) {

                #region das Item ist ein Objekt unter einem Dropdown und NICHT separat gewählt.

                var vorhandenDD = dli.DDItems.Get(key);

                if (vorhandenDD is ItemCollectionList.ReadableListItem rliDD) {

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

                    dli.DDItems.Add(itDD);
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

                    it.Indent = Math.Max(keyAndInfo[z].CountString("\\"), 0);
                    f.ItemAdd(it);
                    olditems.Remove(key);
                    it.UserDefCompareKey = z.ToStringInt10();
                    it.Enabled = !HasChildNode(selected, key);

                    #endregion
                } else {

                    #region ... als Dropdownmenu hinzufügen

                    var ndli = new DropDownListItem(dd_Name, true, string.Empty);
                    ndli.DDItems.Add(it);
                    ndli.Indent = Math.Max(keyAndInfo[z].CountString("\\"), 0);
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

    private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        FloatingForm.Close(this);

        if (e.Item is ItemCollectionList.ReadableListItem rli && rli.Item is AdderItem ai) {
            AdderItem.AddRowsToDatabase(OriginIDColumn, ai.KeysAndInfo, GeneratedEntityId, AdditionalInfoColumn);
        }

        _mustUpdate = true;
        //FillListBox();
        Invalidate();
    }

    bool _mustUpdate = true;

    private void F_ItemClicked(object sender, EventArgs.AbstractListItemEventArgs e) {
        if (_ignoreCheckedChanged) { return; }

        if (e.Item is ItemCollectionList.ReadableListItem rli && rli.Item is AdderItem ai) {
            if (f.Checked.Contains(rli.KeyName)) {
                AdderItem.AddRowsToDatabase(OriginIDColumn, ai.KeysAndInfo, GeneratedEntityId, AdditionalInfoColumn);
            } else {
                AdderItem.RemoveRowsFromDatabase(OriginIDColumn, GeneratedEntityId, ai.KeyName);
            }
            //FillListBox();
            _mustUpdate = true;
            Invalidate();
        }

        if (e.Item is DropDownListItem dli) {
            var x = Cursor.Position.X - MousePos().X + dli.Pos.X + (dli.Indent * 20);
            var y = Cursor.Position.Y - MousePos().Y + dli.Pos.Bottom; //Identisch

            var dropDownMenu = FloatingInputBoxListBoxStyle.Show(dli.DDItems, CheckBehavior.SingleSelection, null, x, y, dli.Pos.Width, null, this, false, ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, true);
            dropDownMenu.Cancel += DropDownMenu_Cancel;
            dropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
        }
    }

    private bool HasChildNode(List<string> selected, string key) {
        foreach (var thisS in selected) {
            if (thisS.StartsWith(key + "\\")) { return true; }
        }
        return false;
    }

    private List<string> RepairMenu(List<string> menu) {
        var m = new List<string>();

        for (var z2 = 0; z2 < menu.Count; z2++) {
            var t = menu[z2].Trim("\\").SplitBy("\\");

            var n = string.Empty;
            foreach (var item in t) {
                n = (n + "\\" + item).Trim("\\");
                m.Add(n);
            }
        }
        return m.SortedDistinctList();
    }

    private List<string> RepairMenu(List<string> menu, List<string> infos) {
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
        if (t.CountString("\\") < 1) { return true; }
        if (Selected(selected, t)) { return true; }
        return Selected(selected, t.PathParent());
    }

    #endregion
}