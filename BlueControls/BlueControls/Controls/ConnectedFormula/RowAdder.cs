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
using BlueControls.Interfaces;
using BlueDatabase;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using System.Windows.Forms;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using System.Linq;
using System.Windows.Media.Animation;
using BlueScript.Variables;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using BlueScript.Enums;
using BlueScript.Structures;
using System.Runtime.InteropServices;
using BlueBasics.Enums;

namespace BlueControls.Controls;

public partial class RowAdder : System.Windows.Forms.UserControl, IControlAcceptFilter, IControlSendFilter, IControlUsesRow {

    #region Fields

    private FilterCollection? _filterInput;
    private bool _ignoreCheckedChanged = false;

    #endregion

    #region Constructors

    public RowAdder() {
        InitializeComponent();
        ((IControlSendFilter)this).RegisterEvents();
        ((IControlAcceptFilter)this).RegisterEvents();
    }

    #endregion

    #region Events

    public event EventHandler? DisposingEvent;

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? AdditionalInfoColumn { get; internal set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlAcceptFilter> Childs { get; } = [];

    /// <summary>
    /// Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.
    /// Dadurch können verschiedene Datensätze gespeichert werden.
    /// Beispiele: Rezepetname, Personenname, Beleg-Nummer
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string EntityID { get; internal set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput {
        get => _filterInput;
        set {
            if (_filterInput == value) { return; }
            this.UnRegisterEventsAndDispose();
            _filterInput = value;
            ((IControlAcceptFilter)this).RegisterEvents();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool FilterInputChangedHandled { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; } = new("FilterOutput 08");

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

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlSendFilter> Parents { get; } = [];

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<RowItem>? RowsInput { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool RowsInputChangedHandled { get; set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool RowsInputManualSeted { get; set; } = false;

    public string Script { get; set; }

    /// <summary>
    /// Die Herkunft-Id, die mit Variablen der erzeugt wird.
    /// Diese Id muss für jede Zeile der eingehenden Datenbank einmalig sein.
    /// Die Struktur muss wie ein Dateipfad aufgebaut sein. z.B. Kochen\\Zutaten\\Vegetarisch\\Mehl
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? TextKey { get; internal set; }

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
        vars.Add(new VariableListString("CurrentlySelected", selected, true, "Was der Benutzer aktuell angeklickt hat."));
        vars.Add(new VariableString("EntityId", generatedentityID, true, "Dies ist die Eingangsvariable."));

        var scp = new ScriptProperties("Row-Adder", MethodType.Standard | MethodType.IO | MethodType.Database, true, [], null);

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
        nt = nt.Trim("\\") + "\\";

        //if (ucase) { nt = nt.ToUpper(); }
        nt = nt.RemoveChars(Constants.Char_PfadSonderZeichen);
        return nt;
    }

    public void Fehler(string txt, ImageCode symbol) {
        lstTexte.Enabled = false;
        lstTexte.ItemClear();
        lstTexte.ItemAdd(ItemOf(txt, symbol));
        FilterOutput.ChangeTo(new FilterItem(null, "RowCreator"));
        _ignoreCheckedChanged = false;
    }

    public void FillListBox() {
        if (_ignoreCheckedChanged) {
            Develop.DebugPrint("Liste wird bereits erstellt!");
            return;
        }
        _ignoreCheckedChanged = true;

        if (string.IsNullOrEmpty(EntityID)) {
            lstTexte.Enabled = false;
            lstTexte.ItemClear();
            lstTexte.ItemAdd(ItemOf("Interner Fehler: EnitiyID", BlueBasics.Enums.ImageCode.Kritisch));
            FilterOutput.ChangeTo(new FilterItem(null, "RowCreator"));
            _ignoreCheckedChanged = false;
            return;
        }

        if (OriginIDColumn == null) {
            lstTexte.Enabled = false;
            lstTexte.ItemClear();
            lstTexte.ItemAdd(ItemOf("Interner Fehler: OriginIDColumn", BlueBasics.Enums.ImageCode.Kritisch));
            FilterOutput.ChangeTo(new FilterItem(null, "RowCreator"));
            _ignoreCheckedChanged = false;
            return;
        }

        if (!FilterInputChangedHandled) {
            FilterInputChangedHandled = true;
            this.DoInputFilter(FilterInput?.Database, true);
        }

        RowsInputChangedHandled = true;

        this.DoRows();

        var rowIn = this.RowSingleOrNull();

        if (rowIn == null) {
            Fehler("Keine Wahl getroffen", BlueBasics.Enums.ImageCode.Information);
            return;
        }

        rowIn.CheckRowDataIfNeeded();

        var generatedentityID = rowIn.ReplaceVariables(EntityID, false, true, null);

        if (generatedentityID == EntityID) {
            Fehler("Interner Fehler: EnitiyID", BlueBasics.Enums.ImageCode.Kritisch);
            return;
        }

        if (generatedentityID.Contains("\\")) {
            Fehler("Interner Fehler: Ungültiges Zeichen in  EnitiyID", BlueBasics.Enums.ImageCode.Kritisch);
            return;
        }

        if (string.IsNullOrEmpty(Script)) {
            Fehler("Interner Fehler: Kein Skript vorhanden", BlueBasics.Enums.ImageCode.Kritisch);
            return;
        }

        FilterOutput.ChangeTo(new FilterItem(OriginIDColumn, BlueDatabase.Enums.FilterType.BeginntMit, generatedentityID + "\\"));

        var selected = OriginIDColumn.Contents(FilterOutput, null);
        selected = selected.SortedDistinctList().Select(s => s.TrimStart(generatedentityID + "\\")).ToList();

        var scf = ExecuteScript(Script, selected, EntityID, rowIn);

        if (!scf.AllOk) {
            Fehler("Interner Fehler: Skript fehlerhaft; " + scf.ProtocolText, BlueBasics.Enums.ImageCode.Kritisch);
            return;
        }

        var menu = scf.Variables?.GetList("Menu");

        if (menu == null || menu.Count < 1) {
            Fehler("Interner Fehler: Skript gab kein Menu zurück", BlueBasics.Enums.ImageCode.Kritisch);
            return;
        }

        var infos = scf.Variables?.GetList("Infos") ?? new List<string>();

        while (infos.Count < menu.Count) { infos.Add(string.Empty); }

        lstTexte.Enabled = true;

        List<string> olditems = lstTexte.Items.ToListOfString().Select(s => s.TrimStart(generatedentityID + "\\")).ToList();

        foreach (var thisIT in lstTexte.Items) {
            if (thisIT is ItemCollectionList.ReadableListItem rli && rli.Item is AdderItem ai) {
                ai.Keys.Clear();
                ai.Infos.Clear();
            }
        }

        menu = RepairMenu(menu);

        for (var z = 0; z < menu.Count; z++) {
            var key = menu[z];

            if (!ShowMe(selected, key)) { continue; }

            olditems.Remove(key);

            AdderItem? adderit = null;

            if (lstTexte.Items.Get(key) is ItemCollectionList.ReadableListItem rli) {
                if (rli.Item is AdderItem ai) { adderit = ai; }
            } else {
                adderit = new AdderItem(generatedentityID, OriginIDColumn, AdditionalInfoColumn, key);
                lstTexte.ItemAdd(ItemOf(adderit));
            }

            if (adderit != null) {
                adderit.Keys.Add(key);
                adderit.Infos.Add(infos[z]);

                adderit.GeneratedEntityID = generatedentityID;
            }
        }

        foreach (var thisit in olditems) {
            lstTexte.Remove(thisit);
        }

        lstTexte.UncheckAll();
        lstTexte.Check(selected);

        _ignoreCheckedChanged = false;
    }

    public void FilterInput_DispodingEvent(object sender, System.EventArgs e) => this.FilterInput_DispodingEvent();

    public void FilterInput_RowsChanged(object sender, System.EventArgs e) => this.FilterInput_RowsChanged();

    public void FilterOutput_DispodingEvent(object sender, System.EventArgs e) => this.FilterOutput_DispodingEvent();

    public void FilterOutput_PropertyChanged(object sender, System.EventArgs e) => this.FilterOutput_PropertyChanged();

    public void HandleChangesNow() {
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }
        FillListBox();
    }

    public void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    public void ParentFilterOutput_Changed() { }

    public void RowsInput_Changed() { }

    protected override void OnPaint(PaintEventArgs e) {
        HandleChangesNow();
        base.OnPaint(e);
    }

    private void lstTexte_ItemClicked(object sender, EventArgs.AbstractListItemEventArgs e) {
        if (_ignoreCheckedChanged) { return; }

        if (e.Item is ItemCollectionList.ReadableListItem rli && rli.Item is AdderItem ai) {
            if (lstTexte.Checked.Contains(rli.KeyName)) {
                ai.AddRowsToDatabase();
            } else {
                ai.RemoveRowsFromDatabase();
            }
        }

        FillListBox();
    }

    private List<string> RepairMenu(List<string> menu) {
        var m = new List<string>();

        foreach (var thiss in menu) {
            var t = thiss.TrimEnd("\\").SplitBy("\\");

            var n = string.Empty;
            foreach (var item in t) {
                n = n + item + "\\";
                m.Add(n);
            }
        }

        return m.SortedDistinctList();
    }

    private bool Selected(ICollection<string> selected, string textkey) => selected.Contains(RepairTextKey(textkey));

    private bool ShowMe(ICollection<string> selected, string textkey) {
        var t = RepairTextKey(textkey);
        if (t.CountString("\\") < 2) { return true; }
        if (Selected(selected, t)) { return true; }
        return Selected(selected, t.PathParent());
    }

    #endregion
}