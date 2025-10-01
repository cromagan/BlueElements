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
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using Button = BlueControls.Controls.Button;

namespace BlueControls.Forms;

[StandaloneInfo("Formular-Editor", ImageCode.Anwendung)]
public partial class ConnectedFormulaEditor : PadEditor, IIsEditor, IIsStandalone {

    #region Fields

    private ConnectedFormula.ConnectedFormula? _formula;

    #endregion

    #region Constructors

    public ConnectedFormulaEditor(string? filename, ReadOnlyCollection<string>? notAllowedchilds) {
        InitializeComponent();

        GenQuickInfo(btnFeldHinzu, new EditFieldPadItem());

        GenQuickInfo(btnButton, new ButtonPadItem());

        GenQuickInfo(btnRegionAdd, new RegionFormulaPadItem());

        GenQuickInfo(btnFileExplorer, new FileExplorerPadItem());

        GenQuickInfo(btnBild, new EasyPicPadItem());

        GenQuickInfo(btnTable, new TableViewPadItem());

        GenQuickInfo(btnDropdownmenu, new DropDownSelectRowPadItem());

        GenQuickInfo(btnFilterConverter, new FilterConverterElementPadItem());

        GenQuickInfo(btnTabControlAdd, new TabFormulaPadItem());

        GenQuickInfo(btnBenutzerFilterWahl, new OutputFilterPadItem());

        _ = FormulaSet(filename, notAllowedchilds);

        //MultiUserFile.SaveAll(false);
        //Table.ForceSaveAll();
    }

    public ConnectedFormulaEditor() : this(string.Empty, null) { }

    #endregion

    #region Properties

    public ConnectedFormula.ConnectedFormula? Formula {
        get => _formula;
        private set {
            if (!Generic.IsAdministrator()) { value = null; }
            if (value != null && !value.LockEditing()) { value = null; }

            if (_formula == value) { return; }

            if (_formula != null) {
                _formula.Editing -= _cFormula_Editing;
                _formula.UnlockEditing();
            }

            _formula = value;

            if (_formula != null) {
                _formula.Editing += _cFormula_Editing;
            }

            if (_formula?.Pages is { IsDisposed: false } pg) {
                foreach (var thisp in pg) {
                    if (thisp is ItemCollectionPadItem { IsDisposed: false } icpi && icpi.IsHead()) {
                        Pad.Items = icpi;
                        break;
                    }
                }
            } else {
                Pad.Items = null;
            }

            CheckButtons();
        }
    }

    public IEditable? ToEdit {
        set {
            if (value is ConnectedFormula.ConnectedFormula cf) {
                FormulaSet(cf, null);
            } else {
                FormulaSet(null as ConnectedFormula.ConnectedFormula, null);
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            FormulaSet(null as ConnectedFormula.ConnectedFormula, null);
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        FormulaSet(null as ConnectedFormula.ConnectedFormula, null);
        base.OnFormClosing(e);
    }

    protected override void Pad_GotNewItemCollection(object sender, System.EventArgs e) {
        base.Pad_GotNewItemCollection(sender, e);
        DoPages();
    }

    private void _cFormula_Editing(object sender, EditingEventArgs e) {
        if (IsDisposed) { return; }
        if (!Visible) { return; }
        e.Editing = true;
    }

    private void AddCentered(AbstractPadItem x) {
        var l = Pad.LastClickedItem;

        Pad.AddCentered(x);

        //if (l is ReciverSenderControlPadItem isr && x is ReciverControlPadItem iar) {
        //    iar.GetRowFrom = isr;
        //}

        if (l is ReciverSenderControlPadItem && x is ReciverControlPadItem iaf) {
            iaf.Parents = new List<string> { l.KeyName }.AsReadOnly();
        }

        //if (x is ReciverSenderControlPadItem isr2) {
        //    isr2.Tabelle_wählen();
        //}

        //if (x is ReciverSenderControlPadItem isf2) {
        //    isf2.Tabelle_wählen();
        //}

        //if (x is ReciverControlPadItem iaf2 && iaf2.Parents.Count == 0) {
        //    iaf2.Datenquellen_bearbeiten();
        //}
    }

    private void btnArbeitsbereich_Click(object sender, System.EventArgs e) {
        if (Pad?.Items is not { IsDisposed: false } pi) { return; }

        var oldw = pi.UsedArea.Width / AutosizableExtension.GridSize;

        var wi = InputBox.Show("Breite in Kästchen:", oldw.ToStringFloat1(), FormatHolder.LongPositive);

        if (string.IsNullOrEmpty(wi)) { return; }

        var oldh = pi.UsedArea.Height / AutosizableExtension.GridSize;

        var he = InputBox.Show("Höhe in Kästchen:", oldh.ToStringFloat1(), FormatHolder.LongPositive);

        if (string.IsNullOrEmpty(wi)) { return; }

        var op = MessageBox.Show("Vorhanden Steuerelemente:", ImageCode.Textfeld2, "Anpassen", "nix machen", "Abbruch");

        if (op == 2) { return; }

        pi.Resize(FloatParse(wi) * AutosizableExtension.GridSize, FloatParse(he) * AutosizableExtension.GridSize, op == 0, string.Empty);
    }

    private void btnBenutzerFilterWahl_Click(object sender, System.EventArgs e) {
        var x = new OutputFilterPadItem();
        AddCentered(x);
    }

    private void btnBild_Click(object sender, System.EventArgs e) {
        var x = new EasyPicPadItem();
        AddCentered(x);
    }

    private void btnButton_Click(object sender, System.EventArgs e) {
        var x = new ButtonPadItem();
        AddCentered(x);
    }

    private void btnDropdownmenu_Click(object sender, System.EventArgs e) {
        var x = new DropDownSelectRowPadItem();
        AddCentered(x);
    }

    private void btnFeldHinzu_Click(object sender, System.EventArgs e) {
        var x = new EditFieldPadItem();
        AddCentered(x);
    }

    private void btnFilterConverter_Click(object sender, System.EventArgs e) {
        var x = new FilterConverterElementPadItem();
        AddCentered(x);
    }

    private void btnLetzteDateien_ItemClicked(object sender, AbstractListItemEventArgs e) {
        MultiUserFile.SaveAll(true);
        _ = FormulaSet(e.Item.KeyName, null);
    }

    private void btnNeuDB_SaveAs_Click(object sender, System.EventArgs e) {
        //MultiUserFile.SaveAll(true);

        //if (sender == btnSaveAs) {
        //    if (Formula == null) { return; }
        //}

        //if (sender == btnNeuDB) {
        //    if (Formula != null) { FormulaSet(null as ConnectedFormula.ConnectedFormula, null); }
        //}

        //_ = SaveTab.ShowDialog();
        //if (!DirectoryExists(SaveTab.FileName.FilePath())) { return; }
        //if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

        //if (sender == btnNeuDB) {
        //    FormulaSet(new ConnectedFormula.ConnectedFormula(), null); // Ab jetzt in der Variable _Table zu finden
        //}
        //if (FileExists(SaveTab.FileName)) { _ = DeleteFile(SaveTab.FileName, true); }

        //Formula?.SaveAsAndChangeTo(SaveTab.FileName);

        //FormulaSet(SaveTab.FileName, null);
    }

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(true);
        _ = LoadTab.ShowDialog();
    }

    private void btnPfeileAusblenden_CheckedChanged(object sender, System.EventArgs e) => btnVorschauModus.Checked = btnPfeileAusblenden.Checked;

    private void btnRegionAdd_Click(object sender, System.EventArgs e) {
        if (Formula == null) { return; }

        var x = new RegionFormulaPadItem(string.Empty, Formula) {
            Bei_Export_sichtbar = true
        };
        AddCentered(x);
    }

    private void btnRegisterKarte_Click(object sender, System.EventArgs e) {
        if (Formula is not { IsDisposed: false } cf) { return; }
        if (cf.Pages is not { IsDisposed: false }) { return; }

        var n = InputBox.Show("Formular-Name:");
        if (string.IsNullOrEmpty(n)) { return; }

        var p = new ItemCollectionPadItem {
            Caption = n,
            Breite = 100,
            Höhe = 100
        };

        var it = new RowEntryPadItem();
        p.Add(it);

        cf.Pages.Add(p);

        Pad.Items = p;
        Formula?.Repair();

        //it.Tabelle_wählen();

        //it.Bei_Export_sichtbar = false;

        //Pad.AddCentered(it);

        //ChooseTableAndId(it);
    }

    private void btnSpeichern_Click(object sender, System.EventArgs e) => MultiUserFile.SaveAll(true);

    private void btnSymbolLaden_Click(object sender, System.EventArgs e) {
        if (!string.IsNullOrEmpty(LastFilePath)) { LoadSymbol.InitialDirectory = LastFilePath; }

        _ = LoadSymbol.ShowDialog();
    }

    private void btnTabControlAdd_Click(object sender, System.EventArgs e) {
        if (Formula == null) { return; }

        var x = new TabFormulaPadItem(string.Empty, Formula) {
            Bei_Export_sichtbar = true
        };
        AddCentered(x);
    }

    private void btnTable_Click(object sender, System.EventArgs e) {
        var x = new TableViewPadItem();
        AddCentered(x);
    }

    private void btnVorschauModus_CheckedChanged(object sender, System.EventArgs e) => btnPfeileAusblenden.Checked = btnVorschauModus.Checked;

    private void btnWeitereCF_Click(object sender, System.EventArgs e) {
        if (Formula == null) { return; }

        var l = Generic.GetInstaceOfType<IItemToControl>(string.Empty, Formula);

        if (l.Count == 0) { return; }

        var i = new List<AbstractListItem>();

        foreach (var thisl in l) {
            i.Add(ItemOf(thisl));
        }

        var x = InputBoxListBoxStyle.Show("Hinzufügen:", i, CheckBehavior.SingleSelection, null, AddType.None);

        if (x is not { Count: 1 }) { return; }

        var toadd = i.Get(x[0]);

        if (toadd is not ReadableListItem { Item: AbstractPadItem api }) { return; }

        //if (toadd is not AbstractPadItem api) {  return; }

        //var x = new FileExplorerPadItem(string.Empty);

        AddCentered(api);
    }

    private void CheckButtons() { }

    private void DoPages() {
        if (InvokeRequired) {
            _ = Invoke(new Action(DoPages));
            return;
        }

        try {
            if (IsDisposed) { return; }

            var x = _formula?.AllPages() ?? [];

            TabPage? later = null;

            if (x.Count == 1 && string.IsNullOrEmpty(x[0])) { x.Clear(); }

            if (x.Count > 0) {
                tabSeiten.Visible = true;

                foreach (var thisTab in tabSeiten.TabPages) {
                    var tb = (TabPage)thisTab;

                    if (!x.Contains(tb.Text)) {
                        tabSeiten.TabPages.Remove(tb);
                        DoPages();
                        return;
                    }

                    _ = x.Remove(tb.Text);
                    if (Pad?.Items != null && tb.Text == Pad.Items.Caption) { later = tb; }
                }

                foreach (var thisn in x) {
                    var t = new TabPage(thisn) {
                        Name = "Seite_" + thisn
                    };
                    tabSeiten.TabPages.Add(t);

                    if (Pad?.Items != null && t.Text == Pad.Items.Caption) { later = t; }
                }
            } else {
                tabSeiten.Visible = false;
                if (tabSeiten.TabPages.Count > 0) {
                    tabSeiten.TabPages.Clear();
                }
            }

            tabSeiten.SelectedTab = later;
        } catch { }
    }

    private bool FormulaSet(string? filename, IReadOnlyCollection<string>? notAllowedchilds) {
        FormulaSet(null as ConnectedFormula.ConnectedFormula, notAllowedchilds);

        if (!Generic.IsAdministrator()) { return false; }

        if (filename == null || !FileExists(filename)) {
            //CheckButtons();
            return false;
        }

        btnLetzteFormulare.AddFileName(filename, string.Empty);
        LoadTab.FileName = filename;
        var tmpFormula = ConnectedFormula.ConnectedFormula.GetByFilename(filename);
        if (tmpFormula == null) { return false; }

        FormulaSet(tmpFormula, notAllowedchilds);

        return true;
    }

    private void FormulaSet(ConnectedFormula.ConnectedFormula? formular, IReadOnlyCollection<string>? notAllowedchilds) {
        Formula = formular;

        //if (notAllowedchilds != null && Formula != null && editable) {
        //    var l = new List<string>();
        //    l.AddRange(Formula.NotAllowedChilds);
        //    l.AddRange(notAllowedchilds);

        //    Formula.NotAllowedChilds = l.AsReadOnly();
        //}

        if (_formula?.Pages is { IsDisposed: false } pg) {
            foreach (var thisp in pg) {
                if (thisp is ItemCollectionPadItem { IsDisposed: false } icpi && icpi.IsHead()) {
                    Pad.Items = icpi;
                    break;
                }
            }
        } else {
            Pad.Items = null;
        }

        CheckButtons();
    }

    private void GenQuickInfo(Button b, ReciverControlPadItem from) {
        var txt = "Fügt das Steuerelement des Types <b>" + b.Text.Replace("-", string.Empty) + "</b> hinzu:";

        txt += "<br><br><b><u>Beschreibung:</b></u>";
        txt = txt + "<br>" + from.Description;

        txt += "<br><br><b><u>Eigenschaften:</b></u>";

        if (from is { IsDisposed: false } ias) {
            if (ias.InputMustBeOneRow) {
                txt = txt + "<br> - Das Element kann Filter <u>empfangen</u>.<br>" +
                    "   Diese müssen als Ergebniss <u>genau eine Zeile</u> einer Tabelle ergeben,<br>" +
                    "   da die Werte der Zeile in dem Element benutzt werden können.";
            } else {
                txt += "<br> - Das Element kann Filter <u>empfangen</u> und verarbeitet diese.";
            }
        }

        if (from is ReciverSenderControlPadItem) {
            txt += "<br> - Das Element kann Filter an andere Elemente <u>weitergeben</u>.";
        }

        if (!from.MustBeInDrawingArea) {
            txt += "<br> - Das Element dient nur zur Berechnung von Werten<br> und ist im Formular <u>nicht sichtbar</u>.";
        }

        b.QuickInfo = txt;
    }

    private void grpFileExplorer_Click(object sender, System.EventArgs e) {
        var x = new FileExplorerPadItem();

        AddCentered(x);
    }

    private void LoadSymbol_FileOk(object sender, CancelEventArgs e) {
        if (Pad.Items == null) { return; }

        if (string.IsNullOrEmpty(LoadSymbol.FileName)) { return; }
        var toparse = ReadAllText(LoadSymbol.FileName, Constants.Win1252);
        LastFilePath = LoadSymbol.FileName.FilePath();

        var i = ParseableItem.NewByParsing<ReciverControlPadItem>(toparse);
        if (i is not { IsDisposed: false } api) { return; }

        api.GetNewIdsForEverything();

        Pad.Items.Add(api);
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) => FormulaSet(LoadTab.FileName, null);

    private void tabSeiten_Selected(object sender, TabControlEventArgs e) {
        var s = string.Empty;

        if (tabSeiten.SelectedTab != null) {
            s = tabSeiten.SelectedTab.Text;
        }

        if (_formula?.Pages is { IsDisposed: false } pg) {
            foreach (var thisp in pg) {
                if (thisp is ItemCollectionPadItem { IsDisposed: false } icp) {
                    if (s == icp.Caption) {
                        Pad.Items = icp;
                        break;
                    }
                }
            }
        } else {
            Pad.Items = null;
        }
    }

    #endregion
}