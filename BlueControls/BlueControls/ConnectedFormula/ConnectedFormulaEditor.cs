﻿// Authors:
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.MultiUserFile;
using BlueControls.ConnectedFormula;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.Abstract;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueDatabase;
using static BlueBasics.IO;
using static BlueBasics.Converter;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;

#nullable enable

namespace BlueControls.Forms;

public partial class ConnectedFormulaEditor : PadEditor {

    #region Fields

    private ConnectedFormula.ConnectedFormula? _cFormula;

    #endregion

    #region Constructors

    public ConnectedFormulaEditor(string? filename, ReadOnlyCollection<string>? notAllowedchilds) {
        InitializeComponent();

        GenQuickInfo(btnFeldHinzu, new EditFieldPadItem(string.Empty));

        GenQuickInfo(btnButton, new ButtonPadItem(string.Empty));

        GenQuickInfo(btnTextGenerator, new TextGeneratorPadItem(string.Empty));

        GenQuickInfo(btnFileExplorer, new FileExplorerPadItem(string.Empty));

        GenQuickInfo(btnBild, new EasyPicPadItem(string.Empty));

        GenQuickInfo(btnTable, new TableViewPadItem(string.Empty));

        GenQuickInfo(btnDropdownmenu, new DropDownSelectRowPadItem(string.Empty));

        GenQuickInfo(btnFilterConverter, new FilterConverterElementPadItem(string.Empty));

        GenQuickInfo(btnTabControlAdd, new TabFormulaPadItem(string.Empty));

        GenQuickInfo(btnBenutzerFilterWahl, new OutputFilterPadItem(string.Empty));

        FormulaSet(filename, notAllowedchilds);
    }

    public ConnectedFormulaEditor() : this(string.Empty, null) { }

    #endregion

    #region Properties

    public ConnectedFormula.ConnectedFormula? CFormula {
        get => _cFormula;
        private set {
            if (_cFormula == value) { return; }

            if (_cFormula != null) {
                _cFormula.Editing -= _cFormula_Editing;
            }

            _cFormula = value;

            if (_cFormula != null) {
                _cFormula.Editing += _cFormula_Editing;
            }
        }
    }

    #endregion

    #region Methods

    public static void OpenScriptEditor(ConnectedFormula.ConnectedFormula? f) {
        if (f == null || f.IsDisposed) { return; }

        var se = new ConnectedFormulaScriptEditor(f);
        _ = se.ShowDialog();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
            CFormula = null;
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void _cFormula_Editing(object sender, EditingEventArgs e) {
        if (IsDisposed) { return; }
        e.Editing = true;
    }

    private void AddCentered(AbstractPadItem x) {
        var l = Pad.LastClickedItem;

        Pad.AddCentered(x);

        //if (l is IItemSendSomething isr && x is IItemAcceptSomething iar) {
        //    iar.GetRowFrom = isr;
        //}

        if (l is IItemSendSomething && x is IItemAcceptFilter iaf) {
            iaf.Parents = new List<string> { l.KeyName }.AsReadOnly();
        }

        //if (x is IItemSendSomething isr2) {
        //    isr2.Datenbank_wählen();
        //}

        //if (x is IItemSendSomething isf2) {
        //    isf2.Datenbank_wählen();
        //}

        if (x is IItemAcceptFilter iaf2 && iaf2.Parents.Count == 0) {
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
        MultiUserFile.ForceLoadSaveAll();
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
        var x = new TableViewPadItem(string.Empty);
        AddCentered(x);
    }

    private void btnTextGenerator_Click(object sender, System.EventArgs e) {
        var x = new TextGeneratorPadItem(string.Empty);
        AddCentered(x);
    }

    private void btnVorschauModus_CheckedChanged(object sender, System.EventArgs e) => btnPfeileAusblenden.Checked = btnVorschauModus.Checked;

    private void btnVorschauÖffnen_Click(object sender, System.EventArgs e) {
        MultiUserFile.SaveAll(false);

        Database? db = null;

        if (CFormula?.PadData != null) {
            foreach (var thisItem in CFormula.PadData) {
                if (thisItem is RowEntryPadItem iri) {
                    db = iri.DatabaseOutput;
                }
            }
        }
        RowItem? r = null;
        if (db != null) {
            r = db.Row.First();
        }

        var c = CFormula;
        CFormula = null;
        EditBoxRow.Show("Achtung:\r\nVoll funktionsfähige Test-Ansicht", c, r, true);
        CFormula = c;
    }

    private void CheckButtons() { }

    private void FormulaSet(string? filename, IReadOnlyCollection<string>? notAllowedchilds) {
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

    private void GenQuickInfo(Controls.Button b, FakeControlPadItem from) {
        var txt = "Fügt das Steuerelement des Types <b>" + b.Text.Replace("-", string.Empty) + "</b> hinzu:";

        txt = txt + "<br><br><b><u>Beschreibung:</b></u>";
        txt = txt + "<br>" + from.Description;

        txt = txt + "<br><br><b><u>Eigenschaften:</b></u>";

        if (from is IItemAcceptFilter ias) {
            if (ias.MustBeOneRow) {
                txt = txt + "<br> - Das Element kann Filter <u>empfangen</u>.<br>" +
                    "   Diese müssen als Ergebniss <u>genau eine Zeile</u> einer Datenbank ergeben,<br>" +
                    "   da die Werte der Zeile in dem Element benutzt werden können.";
            } else {
                txt = txt + "<br> - Das Element kann Filter <u>empfangen</u> und verarbeitet diese.";
            }
        }

        if (from is IItemSendSomething) {
            txt = txt + "<br> - Das Element kann Filter an andere Elemente <u>weitergeben</u>.";
        }

        if (!from.MustBeInDrawingArea) {
            txt = txt + "<br> - Das Element dient nur zur Berechnung von Werten<br> und ist im Formular <u>nicht sichtbar</u>.";
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