// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.MultiUserFile;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using static BlueBasics.IO;
using static BlueBasics.Converter;

namespace BlueControls.Forms;

public partial class ConnectedFormulaEditor : PadEditor {

    #region Constructors

    public ConnectedFormulaEditor(string? filename, ReadOnlyCollection<string>? notAllowedchilds) {
        InitializeComponent();

        FormulaSet(filename, notAllowedchilds);
    }

    public ConnectedFormulaEditor() : this(string.Empty, null) { }

    #endregion

    #region Properties

    public ConnectedFormula.ConnectedFormula? CFormula { get; private set; }

    #endregion

    #region Methods

    private void AddCentered(FakeControlPadItem x) {
        var l = Pad.LastClickedItem;

        Pad.AddCentered(x);

        if (l is IItemSendRow isr && x is IItemAcceptRow iar) {
            iar.GetRowFrom = isr;
        }

        if (l is IItemSendFilter isf && x is IItemAcceptFilter iaf) {
            iaf.GetFilterFrom = new List<string> { l.KeyName }.AsReadOnly();
        }

        if (x is IItemAcceptRow iar2 && iar2.GetRowFrom == null) {
            iar2.Datenquelle_wählen();
        }

        if (x is IItemSendRow isr2) {
            isr2.Datenbank_wählen();
        }

        if (x is IItemSendFilter isf2) {
            isf2.Datenbank_wählen();
        }

        if (x is IItemAcceptFilter iaf2 && iaf2.GetFilterFrom.Count == 0) {
            iaf2.Datenquellen_bearbeiten();
        }
    }

    private void btnArbeitsbereich_Click(object sender, System.EventArgs e) {
        if (CFormula?.PadData == null) { return; }

        var oldw = CFormula.PadData.SheetSizeInPix.Width / IAutosizableExtension.GridSize;

        var wi = InputBox.Show("Breite in Kästchen:", oldw.ToString(Constants.Format_Float1), FormatHolder.IntegerPositive);

        if (string.IsNullOrEmpty(wi)) { return; }

        var oldh = CFormula.PadData.SheetSizeInPix.Height / IAutosizableExtension.GridSize;

        var he = InputBox.Show("Höhe in Kästchen:", oldh.ToString(Constants.Format_Float1), FormatHolder.IntegerPositive);

        if (string.IsNullOrEmpty(wi)) { return; }

        var op = MessageBox.Show("Vorhanden Steuerelemente:", ImageCode.Textfeld2, "Anpassen", "nix machen", "Abbruch");

        if (op == 2) { return; }

        CFormula.Resize(FloatParse(wi) * IAutosizableExtension.GridSize, FloatParse(he) * IAutosizableExtension.GridSize, op == 0);
    }

    private void btnBenutzerFilterWahl_Click(object sender, System.EventArgs e) {
        var x = new InputFilterOutputFilterPadItem(string.Empty);
        AddCentered(x);
    }

    private void btnBild_Click(object sender, System.EventArgs e) {
        var x = new EasyPicPadItem(string.Empty);
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
        var x = new ScriptChangeFilterPadItem(string.Empty);
        AddCentered(x);
    }

    private void btnLetzteDateien_ItemClicked(object sender, AbstractListItemEventArgs e) {
        MultiUserFile.ForceLoadSaveAll();

        if (e?.Item == null) { return; }
        FormulaSet(e.Item.KeyName, null);
    }

    private void btnNeuDB_SaveAs_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(true);

        if (sender == btnSaveAs) {
            if (CFormula == null) { return; }
        }

        if (sender == btnNeuDB) {
            if (CFormula != null) { FormulaSet(null as ConnectedFormula.ConnectedFormula, null); }
        }

        _ = SaveTab.ShowDialog();
        if (!DirectoryExists(SaveTab.FileName.FilePath())) { return; }
        if (string.IsNullOrEmpty(SaveTab.FileName)) { return; }

        if (sender == btnNeuDB) {
            FormulaSet(new ConnectedFormula.ConnectedFormula(), null); // Ab jetzt in der Variable _Database zu finden
        }
        if (FileExists(SaveTab.FileName)) { _ = DeleteFile(SaveTab.FileName, true); }

        CFormula?.SaveAsAndChangeTo(SaveTab.FileName);

        FormulaSet(SaveTab.FileName, null);
    }

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        MultiUserFile.ForceLoadSaveAll();
        _ = LoadTab.ShowDialog();
    }

    private void btnPfeileAusblenden_CheckedChanged(object sender, System.EventArgs e) => btnVorschauModus.Checked = btnPfeileAusblenden.Checked;

    private void btnRegisterKarte_Click(object sender, System.EventArgs e) {
        var n = InputBox.Show("Formular-Name:");
        if (string.IsNullOrEmpty(n)) { return; }

        var it = new RowEntryPadItem(string.Empty);
        it.Page = n;
        Pad.Item?.Add(it);
        CFormula?.Repair();

        it.Datenbank_wählen();

        //it.Bei_Export_sichtbar = false;

        //Pad.AddCentered(it);

        //ChooseDatabaseAndId(it);
    }

    private void btnSkripteBearbeiten_Click(object sender, System.EventArgs e) => FormulaView.OpenScriptEditor(CFormula);

    private void btnSpeichern_Click(object sender, System.EventArgs e) => MultiUserFile.ForceLoadSaveAll();

    private void btnTabControlAdd_Click(object sender, System.EventArgs e) {
        if (CFormula == null) { return; }

        var x = new TabFormulaPadItem(string.Empty, CFormula) {
            Bei_Export_sichtbar = true
        };
        AddCentered(x);
    }

    private void btnTable_Click(object sender, System.EventArgs e) {
        var x = new TableSelectRowPadItem(string.Empty);
        AddCentered(x);
    }

    private void btnVariable_Click(object sender, System.EventArgs e) {
        var x = new VariableFieldPadItem(string.Empty);

        AddCentered(x);
    }

    private void btnVorschauModus_CheckedChanged(object sender, System.EventArgs e) => btnPfeileAusblenden.Checked = btnVorschauModus.Checked;

    private void btnVorschauÖffnen_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);

        DatabaseAbstract? db = null;

        if (CFormula?.PadData != null) {
            foreach (var thisItem in CFormula.PadData) {
                if (thisItem is IItemRowInput iri) {
                    db = iri.OutputDatabase;
                }
            }
        }
        RowItem? r = null;
        if (db != null) {
            r = db.Row.First();
        }

        EditBoxRow.Show("Achtung:\r\nVoll funktionsfähige Test-Ansicht", CFormula, r, true);
    }

    private void btnZeileAnlegen_Click(object sender, System.EventArgs e) {
        var x = new AddRowPaditem(string.Empty);
        AddCentered(x);
    }

    private void btnZeileZuFilter_Click(object sender, System.EventArgs e) {
        var x = new InputRowOutputFilterPadItem(string.Empty);
        AddCentered(x);
    }

    private void CheckButtons() { }

    private void ChooseDatabaseAndId(IItemSendRow? it) {
        if (CFormula == null || it == null) { return; }

        it.Datenbank_wählen();

        //var db = CommonDialogs.ChooseKnownDatabase();

        //if (db == null) { return; }

        //it.OutputDatabase = db;
        //it.Id = CFormula.NextId();
    }

    private void FormulaSet(string? filename, ReadOnlyCollection<string>? notAllowedchilds) {
        FormulaSet(null as ConnectedFormula.ConnectedFormula, notAllowedchilds);

        if (filename == null || !FileExists(filename)) {
            //CheckButtons();
            return;
        }

        btnLetzteFormulare.AddFileName(filename, string.Empty);
        LoadTab.FileName = filename;
        var tmpFormula = ConnectedFormula.ConnectedFormula.GetByFilename(filename);
        if (tmpFormula == null) { return; }
        FormulaSet(tmpFormula, notAllowedchilds);
    }

    private void FormulaSet(ConnectedFormula.ConnectedFormula? formular, IReadOnlyCollection<string>? notAllowedchilds) {
        CFormula = formular;

        if (notAllowedchilds != null && CFormula != null) {
            var l = new List<string>();
            l.AddRange(CFormula.NotAllowedChilds);
            l.AddRange(notAllowedchilds);

            CFormula.NotAllowedChilds = l.AsReadOnly();
        }

        Pad.Item = CFormula?.PadData;

        CheckButtons();
    }

    private void grpFileExplorer_Click(object sender, System.EventArgs e) {
        var x = new FileExplorerPadItem(string.Empty);

        AddCentered(x);
    }

    private void LoadTab_FileOk(object sender, CancelEventArgs e) => FormulaSet(LoadTab.FileName, null);

    private void Pad_GotNewItemCollection(object sender, System.EventArgs e) => Pad.CurrentPage = "Head";

    #endregion
}