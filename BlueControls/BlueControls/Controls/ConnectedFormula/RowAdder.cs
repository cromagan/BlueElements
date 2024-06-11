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

namespace BlueControls.Controls;

public partial class RowAdder : System.Windows.Forms.UserControl, IControlAcceptFilter, IControlSendFilter, IControlUsesRow {

    #region Fields

    public List<RowAdderSingle> AdderSingle = new();
    private FilterCollection? _filterInput;

    private bool _generating = false;

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
    public ColumnItem? AdditinalTextColumn { get; internal set; }

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

    /// <summary>
    /// Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.
    /// Dadurch können verschiedene Datensätze gespeichert werden.
    /// Beispiele: Rezepetname, Personenname, Beleg-Nummer
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? EntityIDColumn { get; internal set; }

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

    /// <summary>
    /// Die Herkunft-Id, die mit Variablen der erzeugt wird.
    /// Diese Id muss für jede Zeile der eingehenden Datenbank einmalig sein.
    /// Die Struktur muss wie ein Dateipfad aufgebaut sein. z.B. Kochen\\Zutaten\\Vegetarisch\\Mehl
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? TextKey { get; internal set; }

    /// <summary>
    /// Die Herkunft-Id, die mit Variablen der erzeugt wird.
    /// Diese Id muss für jede Zeile der eingehenden Datenbank einmalig sein.
    /// Die Struktur muss wie ein Dateipfad aufgebaut sein. z.B. Kochen\\Zutaten\\Vegetarisch\\Mehl
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? TextKeyColumn { get; internal set; }

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="textkey"></param>
    /// <returns>PFAD1\\PFAD2\\PFAD3\\</returns>
    public static string RepairTextKey(string textkey, bool ucase, bool cutaterix) {
        var nt = textkey.Replace("/", "\\");
        nt = nt.Replace("\\\\\\\\", "\\");
        nt = nt.Replace("\\\\\\", "\\");
        nt = nt.Replace("\\\\", "\\");
        nt = nt.Replace("\\\\", "\\");
        nt = nt.Trim("\\") + "\\";

        if (ucase) { nt = nt.ToUpper(); }

        var l = nt.SplitBy("*");

        nt = l[0].RemoveChars(Constants.Char_PfadSonderZeichen);

        if (cutaterix || l.Count() == 1) { return nt; }

        return nt + "*" + l[1];
    }

    public void FillListBox() {
        if (_generating) {
            Develop.DebugPrint("Liste wird bereits erstellt!");
            return;
        }
        _generating = true;

        if (string.IsNullOrEmpty(EntityID) || EntityIDColumn == null) {
            lstTexte.Enabled = false;
            lstTexte.ItemClear();
            lstTexte.ItemAdd(ItemOf("Interner Fehler: EnitiyID", BlueBasics.Enums.ImageCode.Kritisch));
            FilterOutput.ChangeTo(new FilterItem(null, "RowCreator"));
            _generating = false;
            return;
        }

        if (TextKeyColumn == null) {
            lstTexte.Enabled = false;
            lstTexte.ItemClear();
            lstTexte.ItemAdd(ItemOf("Interner Fehler: TextKey", BlueBasics.Enums.ImageCode.Kritisch));
            FilterOutput.ChangeTo(new FilterItem(null, "RowCreator"));
            _generating = false;
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
            lstTexte.Enabled = false;
            lstTexte.ItemClear();
            lstTexte.ItemAdd(ItemOf("Keine Wahl getroffen", BlueBasics.Enums.ImageCode.Information));
            FilterOutput.ChangeTo(new FilterItem(null, "RowCreator"));
            _generating = false;
            return;
        }

        var generatedentityID = rowIn.ReplaceVariables(EntityID, false, true, null);

        if (generatedentityID == EntityID) {
            lstTexte.Enabled = false;
            lstTexte.ItemClear();
            lstTexte.ItemAdd(ItemOf("Interner Fehler: EnitiyID", BlueBasics.Enums.ImageCode.Kritisch));
            FilterOutput.ChangeTo(new FilterItem(null, "RowCreator"));
            _generating = false;
            return;
        }

        lstTexte.Enabled = true;

        FilterOutput.ChangeTo(new FilterItem(EntityIDColumn, BlueDatabase.Enums.FilterType.Istgleich_GroßKleinEgal, generatedentityID));

        List<string> olditems = lstTexte.Items.ToListOfString().Select(s => s.ToUpper()).ToList();

        foreach (var thisIT in lstTexte.Items) {
            if (thisIT is ItemCollectionList.ReadableListItem rli && rli.Item is AdderItem ai) {
                ai.Rows.Clear();
            }
        }

        var selected = TextKeyColumn.Contents(FilterOutput, null).Select(s => s.ToUpper()).ToList();

        foreach (var thisAdder in AdderSingle) {
            if (thisAdder.Database is not Database db || db.IsDisposed) { continue; }

            foreach (var thisRow in db.Row) {
                if (thisRow == null || thisRow.IsDisposed) { continue; }

                var generatedTextKeyWOAsterix = RepairTextKey(thisRow.ReplaceVariables(thisAdder.TextKey, false, true, null), false, true);
                if (!ShowMe(selected, generatedTextKeyWOAsterix)) { continue; }

                olditems.Remove(generatedTextKeyWOAsterix.ToUpper());

                AdderItem? adderit = null;

                if (lstTexte.Items.Get(generatedTextKeyWOAsterix) is ItemCollectionList.ReadableListItem rli) {
                    if (rli.Item is AdderItem ai) { adderit = ai; }
                } else {
                    var it = new AdderItem(EntityIDColumn, generatedentityID, OriginIDColumn, TextKeyColumn, generatedTextKeyWOAsterix, AdditinalTextColumn);

                    lstTexte.ItemAdd(ItemOf(it));
                }

                if (adderit != null) {
                    var generatedTextKey = RepairTextKey(thisRow.ReplaceVariables(thisAdder.TextKey, false, true, null), true, false);
                    var additionaltext = thisRow.ReplaceVariables(thisAdder.AdditionalText, false, true, null);

                    var ai = new AdderItemSingle(generatedTextKey, thisRow, thisAdder.Count, additionaltext);
                    adderit.Rows.Add(ai);

                    adderit.GeneratedentityID = generatedentityID;
                }
            }
        }

        foreach (var thisit in olditems) {
            lstTexte.Remove(thisit);
        }

        lstTexte.UncheckAll();
        lstTexte.Check(selected);

        _generating = false;

        //#region Combobox suchen

        //ComboBox? cb = null;
        //foreach (var thiscb in Controls) {
        //    if (thiscb is ComboBox cbx) { cb = cbx; break; }
        //}

        //#endregion

        //if (cb == null) { return; }

        //var ex = cb.Items().ToList();

        //#region Zeilen erzeugen

        //if (RowsInput == null || !RowsInputChangedHandled) { return; }

        //foreach (var thisR in RowsInput) {
        //    if (cb[thisR.KeyName] == null) {
        //        var tmpQuickInfo = thisR.ReplaceVariables(_showformat, true, true, null);
        //        cb.ItemAdd(ItemOf(tmpQuickInfo, thisR.KeyName));
        //    } else {
        //        ex.Remove(thisR.KeyName);
        //    }
        //}

        //#endregion

        //#region Veraltete Zeilen entfernen

        //foreach (var thisit in ex) {
        //    cb?.Remove(thisit);
        //}

        //#endregion

        //#region Nur eine Zeile? auswählen!

        //// nicht vorher auf null setzen, um Blinki zu vermeiden
        //if (cb.ItemCount == 1) {
        //    ValueSet(cb[0].KeyName, true);
        //}

        //if (cb.ItemCount < 2) {
        //    DisabledReason = "Keine Auswahl möglich.";
        //} else {
        //    DisabledReason = string.Empty;
        //}

        //#endregion

        //#region  Prüfen ob die aktuelle Auswahl passt

        //// am Ende auf null setzen, um Blinki zu vermeiden

        //if (cb[Value] == null) {
        //    ValueSet(string.Empty, true);
        //}

        //#endregion
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
        if (_generating) { return; }

        if (e.Item is ItemCollectionList.ReadableListItem rli && rli.Item is AdderItem ai) {
            if (Selected(lstTexte.Checked, e.Item.KeyName)) {
                ai.AddRowsToDatabase();
            } else {
                ai.RemoveRowsFromDatabase();
            }
        }

        FillListBox();
    }

    private bool Selected(ICollection<string> selected, string textkey) => selected.Contains(RepairTextKey(textkey, true, true), false);

    private bool ShowMe(ICollection<string> selected, string textkey) {
        var t = RepairTextKey(textkey, true, true);
        if (t.CountString("\\") < 2) { return true; }
        if (Selected(selected, t)) { return true; }
        return Selected(selected, t.PathParent());
    }

    #endregion
}