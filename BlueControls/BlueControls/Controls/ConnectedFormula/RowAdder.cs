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

namespace BlueControls.Controls;

public partial class RowAdder : System.Windows.Forms.UserControl, IControlAcceptFilter, IControlSendFilter, IControlUsesRow {

    #region Fields

    public List<RowAdderSingle> AdderSingle = new();
    private FilterCollection? _filterInput;

    private bool _ignoreCheckedChanged = false;

    private List<string> selectedWOAdder = [];

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

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="textkey"></param>
    /// <returns>PFAD1\\PFAD2\\PFAD3\\</returns>
    public static string RepairTextKey(string textkey, bool ucase) {
        var nt = textkey.Replace("/", "\\");
        nt = nt.Replace("\\\\\\\\", "\\");
        nt = nt.Replace("\\\\\\", "\\");
        nt = nt.Replace("\\\\", "\\");
        nt = nt.Replace("\\\\", "\\");
        nt = nt.Trim("\\") + "\\";

        if (ucase) { nt = nt.ToUpper(); }
        nt = nt.RemoveChars(Constants.Char_PfadSonderZeichen);
        return nt;
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
            lstTexte.Enabled = false;
            lstTexte.ItemClear();
            lstTexte.ItemAdd(ItemOf("Keine Wahl getroffen", BlueBasics.Enums.ImageCode.Information));
            FilterOutput.ChangeTo(new FilterItem(null, "RowCreator"));
            _ignoreCheckedChanged = false;
            return;
        }

        rowIn.CheckRowDataIfNeeded();

        var generatedentityID = rowIn.ReplaceVariables(EntityID, false, true, null);

        if (generatedentityID == EntityID) {
            lstTexte.Enabled = false;
            lstTexte.ItemClear();
            lstTexte.ItemAdd(ItemOf("Interner Fehler: EnitiyID", BlueBasics.Enums.ImageCode.Kritisch));
            FilterOutput.ChangeTo(new FilterItem(null, "RowCreator"));
            _ignoreCheckedChanged = false;
            return;
        }

        lstTexte.Enabled = true;

        FilterOutput.ChangeTo(new FilterItem(OriginIDColumn, BlueDatabase.Enums.FilterType.Istgleich, "<ID>" + generatedentityID));

        List<string> olditems = lstTexte.Items.ToListOfString().Select(s => s.ToUpper()).ToList();

        foreach (var thisIT in lstTexte.Items) {
            if (thisIT is ItemCollectionList.ReadableListItem rli && rli.Item is AdderItem ai) {
                ai.Rows.Clear();
            }
        }

        List<string> selectedFromTable = new();

        foreach (var thisR in FilterOutput.Rows) {
            var l = thisR.CellGetList(OriginIDColumn);
            if (l.Count() > 1 && l[1].StartsWith("<TK>")) {
                selectedFromTable.AddIfNotExists(l[1].ToUpper().TrimStart("<TK>"));
            }
        }

        //var selectedFromTable = TextKeyColumn.Contents(FilterOutput, null).Select(s => s.ToUpper()).ToList();

        var selected = new List<string>();
        RepearSelectedWOAdder(selectedFromTable);
        selected.AddRange(selectedWOAdder);
        selected.AddRange(selectedFromTable);

        selected = selected.SortedDistinctList();

        foreach (var thisAdder in AdderSingle) {
            if (thisAdder.Database is not Database db || db.IsDisposed) { continue; }

            var fi = ((FilterCollection)thisAdder.Filter.Clone("Adder Clone"));
            foreach (var thisFi in fi) {
                var t = rowIn.ReplaceVariables(thisFi.SearchValue.JoinWithCr(), false, false, rowIn.LastCheckedEventArgs?.Variables);
                thisFi.SearchValue = t.SplitAndCutByCrToList().AsReadOnly();
            }

            var fi2 = fi.ToArray();

            foreach (var thisRow in db.Row) {
                if (thisRow == null || thisRow.IsDisposed) { continue; }

                if (thisRow.MatchesTo(fi2)) {
                    foreach (var thisRowAdderRow in thisAdder.AdderSingleRows) {
                        var generatedTextKey = RepairTextKey(thisRow.ReplaceVariables(thisRowAdderRow.TextKey, false, true, null), false);

                        //var checkall = selected.Contains(generatedTextKey.ToUpper());

                        var stufen = generatedTextKey.TrimEnd("\\").SplitBy("\\");

                        var generatedTextKey_Stufen = string.Empty;

                        for (var z = 0; z < stufen.Length; z++) {
                            var add = (z == stufen.Length - 1);

                            generatedTextKey_Stufen = generatedTextKey_Stufen + stufen[z] + "\\";

                            //if (checkall) {
                            //    selected.AddIfNotExists(generatedTextKey_Stufen.ToUpper());
                            //    if(!add) { selectedWOAdder.AddIfNotExists(generatedTextKey_Stufen.ToUpper()); }

                            //}

                            if (!ShowMe(selected, generatedTextKey_Stufen)) { continue; }

                            olditems.Remove(generatedTextKey_Stufen.ToUpper());

                            AdderItem? adderit = null;

                            if (lstTexte.Items.Get(generatedTextKey_Stufen) is ItemCollectionList.ReadableListItem rli) {
                                if (rli.Item is AdderItem ai) { adderit = ai; }
                            } else {
                                adderit = new AdderItem(generatedentityID, OriginIDColumn, generatedTextKey_Stufen, AdditinalTextColumn);

                                lstTexte.ItemAdd(ItemOf(adderit));
                            }

                            if (adderit != null) {
                                //var generatedTextKey = RepairTextKey(thisRow.ReplaceVariables(generatedTextKey_Stufen, false, true, null), true);
                                var additionaltext = string.Empty;

                                if (add) {
                                    additionaltext = thisRow.ReplaceVariables(thisRowAdderRow.AdditionalText, false, true, null);
                                }

                                var addme = true;

                                foreach (var thisRowis in adderit.Rows) {
                                    if (thisRowis.GeneratedTextKey == generatedTextKey_Stufen.ToUpper()) {
                                        if (!thisRowis.RealAdder) {
                                            thisRowis.RealAdder = add;
                                            thisRowis.Additionaltext = additionaltext;
                                        }

                                        addme = false;
                                        break;
                                    }
                                }
                                if (addme) {
                                    var ai = new AdderItemSingle(generatedTextKey_Stufen, thisRow, thisAdder.Count + ". " + thisRowAdderRow.Count, additionaltext, add);
                                    adderit.Rows.Add(ai);
                                }

                                adderit.GeneratedEntityID = generatedentityID;
                            }
                        }
                    }
                }
            }
            fi.Dispose();
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
        selectedWOAdder.Clear();
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
            if (Selected(lstTexte.Checked, e.Item.KeyName)) {
                selectedWOAdder.AddIfNotExists(ai.KeyName);

                ai.AddRowsToDatabase();
            } else {
                selectedWOAdder.Remove(ai.KeyName);
                ai.RemoveRowsFromDatabase();
            }
        }

        FillListBox();
    }

    private void RepearSelectedWOAdder(List<string> selectedFromTable) {
        foreach (var thiss in selectedFromTable) {
            var t = thiss.ToUpper().TrimEnd("\\").SplitBy("\\");

            var n = string.Empty;
            foreach (var item in t) {
                n = n + item + "\\";
                selectedWOAdder.AddIfNotExists(n);
            }
        }
    }

    private bool Selected(ICollection<string> selected, string textkey) => selected.Contains(RepairTextKey(textkey, true), false);

    private bool ShowMe(ICollection<string> selected, string textkey) {
        var t = RepairTextKey(textkey, true);
        if (t.CountString("\\") < 2) { return true; }
        if (Selected(selected, t)) { return true; }
        return Selected(selected, t.PathParent());
    }

    #endregion
}