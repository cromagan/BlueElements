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
using BlueBasics.Enums;
using BlueBasics.MultiUserFile;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using BlueBasics.Interfaces;
using BlueBasics.EventArgs;

#nullable enable

namespace BlueControls.Forms;

public partial class ConnectedFormulaEditor : PadEditor, IIsEditor {

    #region Fields

    private ConnectedFormula.ConnectedFormula? _cFormula;

    #endregion

    #region Constructors

    public ConnectedFormulaEditor(string? filename, ReadOnlyCollection<string>? notAllowedchilds) {
        InitializeComponent();

        GenQuickInfo(btnFeldHinzu, new EditFieldPadItem(string.Empty));

        GenQuickInfo(btnButton, new ButtonPadItem(string.Empty));

        GenQuickInfo(btnRegionAdd, new RegionFormulaPadItem(string.Empty));

        GenQuickInfo(btnFileExplorer, new FileExplorerPadItem(string.Empty));

        GenQuickInfo(btnBild, new EasyPicPadItem(string.Empty));

        GenQuickInfo(btnTable, new TableViewPadItem(string.Empty));

        GenQuickInfo(btnDropdownmenu, new DropDownSelectRowPadItem(string.Empty));

        GenQuickInfo(btnFilterConverter, new FilterConverterElementPadItem(string.Empty));

        GenQuickInfo(btnTabControlAdd, new TabFormulaPadItem(string.Empty));

        GenQuickInfo(btnBenutzerFilterWahl, new OutputFilterPadItem(string.Empty));

        if (!FormulaSet(filename, notAllowedchilds)) {
            Close();
        }

        //MultiUserFile.SaveAll(false);
        //Database.ForceSaveAll();
    }

    public ConnectedFormulaEditor() : this(string.Empty, null) { }

    #endregion

    #region Properties

    public ConnectedFormula.ConnectedFormula? CFormula => _cFormula;

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

    private void _cFormula_Editing(object sender, EditingEventArgs e) {
        if (IsDisposed) { return; }
        if(!Visible) { return; }
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
        //    isr2.Datenbank_wählen();
        //}

        //if (x is ReciverSenderControlPadItem isf2) {
        //    isf2.Datenbank_wählen();
        //}

        //if (x is ReciverControlPadItem iaf2 && iaf2.Parents.Count == 0) {
        //    iaf2.Datenquellen_bearbeiten();
        //}
    }

    private void btnArbeitsbereich_Click(object sender, System.EventArgs e) {
        if (CFormula?.PadData == null) { return; }

        var oldw = CFormula.PadData.SheetSizeInPix.Width / AutosizableExtension.GridSize;

        var wi = InputBox.Show("Breite in Kästchen:", oldw.ToStringFloat1(), FormatHolder.LongPositive);

        if (string.IsNullOrEmpty(wi)) { return; }

        var oldh = CFormula.PadData.SheetSizeInPix.Height / AutosizableExtension.GridSize;

        var he = InputBox.Show("Höhe in Kästchen:", oldh.ToStringFloat1(), FormatHolder.LongPositive);

        if (string.IsNullOrEmpty(wi)) { return; }

        var op = MessageBox.Show("Vorhanden Steuerelemente:", ImageCode.Textfeld2, "Anpassen", "nix machen", "Abbruch");

        if (op == 2) { return; }

        CFormula.Resize(FloatParse(wi) * AutosizableExtension.GridSize, FloatParse(he) * AutosizableExtension.GridSize, op == 0, string.Empty);
    }

    private void btnBenutzerFilterWahl_Click(object sender, System.EventArgs e) {
        var x = new OutputFilterPadItem(string.Empty);
        AddCentered(x);
    }

    private void btnBild_Click(object sender, System.EventArgs e) {
        var x = new EasyPicPadItem(string.Empty);
        AddCentered(x);
    }

    private void btnButton_Click(object sender, System.EventArgs e) {
        var x = new ButtonPadItem(string.Empty);
        AddCentered(x);
    }

    private void btnDropdownmenu_Click(object sender, System.EventArgs e) {
        var x = new DropDownSelectRowPadItem(string.Empty);
        AddCentered(x);
    }

    private void btnFeldHinzu_Click(object sender, System.EventArgs e) {
        var x = new EditFieldPadItem(string.Empty);
        AddCentered(x);
    }

    private void btnFilterConverter_Click(object sender, System.EventArgs e) {
        var x = new FilterConverterElementPadItem(string.Empty);
        AddCentered(x);
    }

    private void btnLetzteDateien_ItemClicked(object sender, AbstractListItemEventArgs e) {
        MultiUserFile.SaveAll(true);
        FormulaSet(e.Item.KeyName, null);
    }

    private void btnNeuDB_SaveAs_Click(object sender, System.EventArgs e) {
        //MultiUserFile.SaveAll(true);

        //if (sender == btnSaveAs) {
        //    if (CFormula == null) { return; }
        //}

        //if (sender == btnNeuDB) {
        //    if (CFormula != null) { FormulaSet(null as ConnectedFormula.ConnectedFormula, null); }
        //}

        //_ = SaveTab.ShowDialog();
        //if (!DirectoryExists(SaveTab.FileName.FilePath())) { return; }
        //if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

        //if (sender == btnNeuDB) {
        //    FormulaSet(new ConnectedFormula.ConnectedFormula(), null); // Ab jetzt in der Variable _Database zu finden
        //}
        //if (FileExists(SaveTab.FileName)) { _ = DeleteFile(SaveTab.FileName, true); }

        //CFormula?.SaveAsAndChangeTo(SaveTab.FileName);

        //FormulaSet(SaveTab.FileName, null);
    }

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(true);
        _ = LoadTab.ShowDialog();
    }

    private void btnPfeileAusblenden_CheckedChanged(object sender, System.EventArgs e) => btnVorschauModus.Checked = btnPfeileAusblenden.Checked;

    private void btnRegionAdd_Click(object sender, System.EventArgs e) {
        if (CFormula == null) { return; }

        var x = new RegionFormulaPadItem(string.Empty, CFormula) {
            Bei_Export_sichtbar = true
        };
        AddCentered(x);
    }

    private void btnRegisterKarte_Click(object sender, System.EventArgs e) {
        var n = InputBox.Show("Formular-Name:");
        if (string.IsNullOrEmpty(n)) { return; }

        var it = new RowEntryPadItem(string.Empty) {
            Page = n
        };
        Pad.Item?.Add(it);
        CFormula?.Repair();

        //it.Datenbank_wählen();

        //it.Bei_Export_sichtbar = false;

        //Pad.AddCentered(it);

        //ChooseDatabaseAndId(it);
    }

    private void btnSpeichern_Click(object sender, System.EventArgs e) => MultiUserFile.SaveAll(true);

    private void btnTabControlAdd_Click(object sender, System.EventArgs e) {
        if (CFormula == null) { return; }

        var x = new TabFormulaPadItem(string.Empty, CFormula) {
            Bei_Export_sichtbar = true
        };
        AddCentered(x);
    }

    private void btnTable_Click(object sender, System.EventArgs e) {
        var x = new TableViewPadItem(string.Empty);
        AddCentered(x);
    }

    private void btnVorschauModus_CheckedChanged(object sender, System.EventArgs e) => btnPfeileAusblenden.Checked = btnVorschauModus.Checked;

    private void btnWeitere_Click(object sender, System.EventArgs e) {
        if (CFormula == null) { return; }

        var l = Generic.GetInstaceOfType<IItemToControl>(string.Empty, CFormula);

        if (l.Count == 0) { return; }

        var i = new List<AbstractListItem>();

        foreach (var thisl in l) {
            i.Add(ItemOf(thisl));
        }

        var x = InputBoxListBoxStyle.Show("Hinzufügen:", i, Enums.CheckBehavior.SingleSelection, null, Enums.AddType.None);

        if (x == null || x.Count != 1) { return; }

        var toadd = i.Get(x[0]);

        if (toadd is not ReadableListItem rli) { return; }

        if (rli.Item is not AbstractPadItem api) { return; }

        //if (toadd is not AbstractPadItem api) {  return; }

        //var x = new FileExplorerPadItem(string.Empty);

        AddCentered(api);
    }

    private void CheckButtons() { }

    private bool FormulaSet(string? filename, IReadOnlyCollection<string>? notAllowedchilds) {
        FormulaSet(null as ConnectedFormula.ConnectedFormula, notAllowedchilds);

        if (filename == null || !FileExists(filename)) {
            //CheckButtons();
            return false;
        }

        btnLetzteFormulare.AddFileName(filename, string.Empty);
        LoadTab.FileName = filename;
        var tmpFormula = ConnectedFormula.ConnectedFormula.GetByFilename(filename);
        if (tmpFormula == null) { return false; }

        if (!tmpFormula.LockEditing()) { return false; }

        FormulaSet(tmpFormula, notAllowedchilds);

        return true;
    }

    private void FormulaSet(ConnectedFormula.ConnectedFormula? formular, IReadOnlyCollection<string>? notAllowedchilds) {
        if (_cFormula == formular) { return; }

        if (_cFormula != null) {
            _cFormula.Editing -= _cFormula_Editing;
            _cFormula.UnlockEditing();
        }

        _cFormula = formular;
        var editable = false;

        if (_cFormula != null) {
            _cFormula.Editing += _cFormula_Editing;
            editable = _cFormula.LockEditing();
        }

        if (notAllowedchilds != null && CFormula != null && editable) {
            var l = new List<string>();
            l.AddRange(CFormula.NotAllowedChilds);
            l.AddRange(notAllowedchilds);

            CFormula.NotAllowedChilds = l.AsReadOnly();
        }

        Pad.Item = CFormula?.PadData;

        CheckButtons();
    }

    private void GenQuickInfo(Controls.Button b, ReciverControlPadItem from) {
        var txt = "Fügt das Steuerelement des Types <b>" + b.Text.Replace("-", string.Empty) + "</b> hinzu:";

        txt += "<br><br><b><u>Beschreibung:</b></u>";
        txt = txt + "<br>" + from.Description;

        txt += "<br><br><b><u>Eigenschaften:</b></u>";

        if (from is ReciverControlPadItem ias) {
            if (ias.InputMustBeOneRow) {
                txt = txt + "<br> - Das Element kann Filter <u>empfangen</u>.<br>" +
                    "   Diese müssen als Ergebniss <u>genau eine Zeile</u> einer Datenbank ergeben,<br>" +
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
        var x = new FileExplorerPadItem(string.Empty);

        AddCentered(x);
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) => FormulaSet(LoadTab.FileName, null);

    private void Pad_GotNewItemCollection(object sender, System.EventArgs e) => Pad.CurrentPage = "Head";

    #endregion
}