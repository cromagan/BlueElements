#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
#endregion
using System.Diagnostics;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.Forms
{
    public partial class frmTableView : Form
    {
        //Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        protected override void Dispose(bool disposing)
        {
            try
            {
                //if (disposing && components != null)
                //{
                //    components.Dispose();
                //}
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        //Wird vom Windows Form-Designer benötigt.
       // private IContainer components;
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.LoadTab = new System.Windows.Forms.OpenFileDialog();
            this.SaveTab = new System.Windows.Forms.SaveFileDialog();
            this.Formula = new BlueControls.Controls.Formula();
            this.TableView = new BlueControls.Controls.Table();
            this.cbxColumnArr = new BlueControls.Controls.ComboBox();
            this.Zei = new BlueControls.Controls.Caption();
            this.MainRibbon = new BlueControls.Controls.RibbonBar();
            this.tabDatei = new System.Windows.Forms.TabPage();
            this.grpInformation = new BlueControls.Controls.GroupBox();
            this.Copyright = new BlueControls.Controls.Caption();
            this.btnInformation = new BlueControls.Controls.Button();
            this.grpOrdner = new BlueControls.Controls.GroupBox();
            this.btnDatenbanken = new BlueControls.Controls.Button();
            this.btnTemporärenSpeicherortÖffnen = new BlueControls.Controls.Button();
            this.grpDatei = new BlueControls.Controls.GroupBox();
            this.btnLetzteDateien = new BlueControls.Controls.LastFilesCombo();
            this.btnOeffnen = new BlueControls.Controls.Button();
            this.btnSaveAs = new BlueControls.Controls.Button();
            this.btnNeuDB = new BlueControls.Controls.Button();
            this.tabStart = new System.Windows.Forms.TabPage();
            this.grpAnsicht = new BlueControls.Controls.GroupBox();
            this.SpaltAnsichtCap = new BlueControls.Controls.Caption();
            this.Ansicht3 = new BlueControls.Controls.Button();
            this.Ansicht2 = new BlueControls.Controls.Button();
            this.Ansicht1 = new BlueControls.Controls.Button();
            this.Ansicht0 = new BlueControls.Controls.Button();
            this.grpBearbeitung = new BlueControls.Controls.GroupBox();
            this.btnAlleSchließen = new BlueControls.Controls.Button();
            this.btnAlleErweitern = new BlueControls.Controls.Button();
            this.Datenüberprüfung = new BlueControls.Controls.Button();
            this.AngezeigteZeilenLöschen = new BlueControls.Controls.Button();
            this.SuchenUndErsetzen = new BlueControls.Controls.Button();
            this.grpFormularSteuerung = new BlueControls.Controls.GroupBox();
            this.SuchB = new BlueControls.Controls.Button();
            this.such = new BlueControls.Controls.TextBox();
            this.Vorwärts = new BlueControls.Controls.Button();
            this.zurück = new BlueControls.Controls.Button();
            this.grpAllgemein = new BlueControls.Controls.GroupBox();
            this.btnNeu = new BlueControls.Controls.Button();
            this.btnDrucken = new BlueControls.Controls.ComboBox();
            this.btnLoeschen = new BlueControls.Controls.Button();
            this.tabExtras = new System.Windows.Forms.TabPage();
            this.grpEditor = new BlueControls.Controls.GroupBox();
            this.AllgemeinerEditor = new BlueControls.Controls.Button();
            this.BeziehungsEditor = new BlueControls.Controls.Button();
            this.tabAdmin = new BlueControls.BlueDatabaseDialogs.tabAdministration();
            this.Filter = new BlueControls.BlueDatabaseDialogs.Filterleiste();
            this.MainRibbon.SuspendLayout();
            this.tabDatei.SuspendLayout();
            this.grpInformation.SuspendLayout();
            this.grpOrdner.SuspendLayout();
            this.grpDatei.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAnsicht.SuspendLayout();
            this.grpBearbeitung.SuspendLayout();
            this.grpFormularSteuerung.SuspendLayout();
            this.grpAllgemein.SuspendLayout();
            this.tabExtras.SuspendLayout();
            this.grpEditor.SuspendLayout();
            this.SuspendLayout();
            // 
            // LoadTab
            // 
            this.LoadTab.Filter = "*.MDB Datenbanken|*.MDB|*.* Alle Dateien|*";
            this.LoadTab.Title = "Bitte Datenbank laden!";
            this.LoadTab.FileOk += new System.ComponentModel.CancelEventHandler(this.LoadTab_FileOk);
            // 
            // SaveTab
            // 
            this.SaveTab.Filter = "*.MDB Datenbanken|*.MDB|*.* Alle Dateien|*";
            this.SaveTab.Title = "Bitte neuen Dateinamen der Datenbank wählen.";
            // 
            // Formula
            // 
            this.Formula.Location = new System.Drawing.Point(216, 184);
            this.Formula.MinimumSize = new System.Drawing.Size(320, 350);
            this.Formula.Name = "Formula";
            this.Formula.Size = new System.Drawing.Size(856, 352);
            this.Formula.TabIndex = 75;
            // 
            // TableView
            // 
            this.TableView.Dock = System.Windows.Forms.DockStyle.Left;
            this.TableView.Location = new System.Drawing.Point(0, 152);
            this.TableView.Name = "TableView";
            this.TableView.ShowWaitScreen = true;
            this.TableView.Size = new System.Drawing.Size(192, 562);
            this.TableView.TabIndex = 73;
            this.TableView.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.TableView_ContextMenu_Init);
            this.TableView.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.TableView_ContextMenuItemClicked);
            this.TableView.EditBeforeBeginEdit += new System.EventHandler<BlueDatabase.EventArgs.CellCancelEventArgs>(this.TableView_EditBeforeBeginEdit);
            this.TableView.CursorPosChanged += new System.EventHandler<BlueDatabase.EventArgs.CellEventArgs>(this.TableView_CursorPosChanged);
            this.TableView.ColumnArrangementChanged += new System.EventHandler(this.TableView_ColumnArrangementChanged);
            this.TableView.ViewChanged += new System.EventHandler(this.TableView_ViewChanged);
            this.TableView.VisibleRowsChanged += new System.EventHandler(this.TableView_RowsSorted);
            // 
            // cbxColumnArr
            // 
            this.cbxColumnArr.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxColumnArr.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxColumnArr.Location = new System.Drawing.Point(120, 46);
            this.cbxColumnArr.Name = "cbxColumnArr";
            this.cbxColumnArr.Size = new System.Drawing.Size(210, 22);
            this.cbxColumnArr.TabIndex = 86;
            this.cbxColumnArr.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.cbxColumnArr_ItemClicked);
            // 
            // Zei
            // 
            this.Zei.CausesValidation = false;
            this.Zei.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Zei.Location = new System.Drawing.Point(0, 714);
            this.Zei.Name = "Zei";
            this.Zei.Size = new System.Drawing.Size(1212, 16);
            this.Zei.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // MainRibbon
            // 
            this.MainRibbon.Controls.Add(this.tabDatei);
            this.MainRibbon.Controls.Add(this.tabStart);
            this.MainRibbon.Controls.Add(this.tabExtras);
            this.MainRibbon.Controls.Add(this.tabAdmin);
            this.MainRibbon.Dock = System.Windows.Forms.DockStyle.Top;
            this.MainRibbon.HotTrack = true;
            this.MainRibbon.Location = new System.Drawing.Point(0, 0);
            this.MainRibbon.Name = "MainRibbon";
            this.MainRibbon.SelectedIndex = 1;
            this.MainRibbon.Size = new System.Drawing.Size(1212, 110);
            this.MainRibbon.TabIndex = 92;
            // 
            // tabDatei
            // 
            this.tabDatei.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabDatei.Controls.Add(this.grpInformation);
            this.tabDatei.Controls.Add(this.grpOrdner);
            this.tabDatei.Controls.Add(this.grpDatei);
            this.tabDatei.Location = new System.Drawing.Point(4, 25);
            this.tabDatei.Name = "tabDatei";
            this.tabDatei.Size = new System.Drawing.Size(1204, 81);
            this.tabDatei.TabIndex = 0;
            this.tabDatei.Text = "Datei";
            // 
            // grpInformation
            // 
            this.grpInformation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpInformation.CausesValidation = false;
            this.grpInformation.Controls.Add(this.Copyright);
            this.grpInformation.Controls.Add(this.btnInformation);
            this.grpInformation.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpInformation.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpInformation.Location = new System.Drawing.Point(488, 0);
            this.grpInformation.Name = "grpInformation";
            this.grpInformation.Size = new System.Drawing.Size(280, 81);
            this.grpInformation.TabIndex = 0;
            this.grpInformation.TabStop = false;
            this.grpInformation.Text = "Information";
            // 
            // Copyright
            // 
            this.Copyright.CausesValidation = false;
            this.Copyright.Location = new System.Drawing.Point(96, 2);
            this.Copyright.Name = "Copyright";
            this.Copyright.Size = new System.Drawing.Size(176, 22);
            this.Copyright.Text = "(c) Christian Peter";
            this.Copyright.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // btnInformation
            // 
            this.btnInformation.ImageCode = "Information";
            this.btnInformation.Location = new System.Drawing.Point(8, 2);
            this.btnInformation.Name = "btnInformation";
            this.btnInformation.QuickInfo = "Über dieses Programm";
            this.btnInformation.Size = new System.Drawing.Size(80, 66);
            this.btnInformation.TabIndex = 27;
            this.btnInformation.Text = "Information";
            this.btnInformation.Click += new System.EventHandler(this.ÜberDiesesProgramm_Click);
            // 
            // grpOrdner
            // 
            this.grpOrdner.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpOrdner.CausesValidation = false;
            this.grpOrdner.Controls.Add(this.btnDatenbanken);
            this.grpOrdner.Controls.Add(this.btnTemporärenSpeicherortÖffnen);
            this.grpOrdner.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpOrdner.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpOrdner.Location = new System.Drawing.Point(304, 0);
            this.grpOrdner.Name = "grpOrdner";
            this.grpOrdner.Size = new System.Drawing.Size(184, 81);
            this.grpOrdner.TabIndex = 1;
            this.grpOrdner.TabStop = false;
            this.grpOrdner.Text = "Ordner";
            // 
            // btnDatenbanken
            // 
            this.btnDatenbanken.ImageCode = "Ordner";
            this.btnDatenbanken.Location = new System.Drawing.Point(8, 2);
            this.btnDatenbanken.Name = "btnDatenbanken";
            this.btnDatenbanken.QuickInfo = "Speicherort der Datenbanken öffnen";
            this.btnDatenbanken.Size = new System.Drawing.Size(88, 66);
            this.btnDatenbanken.TabIndex = 27;
            this.btnDatenbanken.Text = "Datenbanken-Pfad";
            this.btnDatenbanken.Click += new System.EventHandler(this.Ordn_Click);
            // 
            // btnTemporärenSpeicherortÖffnen
            // 
            this.btnTemporärenSpeicherortÖffnen.ImageCode = "Ordner||||0000ff||126";
            this.btnTemporärenSpeicherortÖffnen.Location = new System.Drawing.Point(96, 2);
            this.btnTemporärenSpeicherortÖffnen.Name = "btnTemporärenSpeicherortÖffnen";
            this.btnTemporärenSpeicherortÖffnen.QuickInfo = "Temporären Speicherort öffnen";
            this.btnTemporärenSpeicherortÖffnen.Size = new System.Drawing.Size(80, 66);
            this.btnTemporärenSpeicherortÖffnen.TabIndex = 26;
            this.btnTemporärenSpeicherortÖffnen.Text = "Temporärer Speicherort";
            this.btnTemporärenSpeicherortÖffnen.Click += new System.EventHandler(this.TemporärenSpeicherortÖffnen_Click);
            // 
            // grpDatei
            // 
            this.grpDatei.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpDatei.CausesValidation = false;
            this.grpDatei.Controls.Add(this.btnLetzteDateien);
            this.grpDatei.Controls.Add(this.btnOeffnen);
            this.grpDatei.Controls.Add(this.btnSaveAs);
            this.grpDatei.Controls.Add(this.btnNeuDB);
            this.grpDatei.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpDatei.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpDatei.Location = new System.Drawing.Point(0, 0);
            this.grpDatei.Name = "grpDatei";
            this.grpDatei.Size = new System.Drawing.Size(304, 81);
            this.grpDatei.TabIndex = 2;
            this.grpDatei.TabStop = false;
            this.grpDatei.Text = "Datei";
            // 
            // btnLetzteDateien
            // 
            this.btnLetzteDateien.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.btnLetzteDateien.DrawStyle = BlueControls.Enums.enComboboxStyle.RibbonBar;
            this.btnLetzteDateien.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.btnLetzteDateien.Enabled = false;
            this.btnLetzteDateien.ImageCode = "Ordner";
            this.btnLetzteDateien.Location = new System.Drawing.Point(128, 2);
            this.btnLetzteDateien.Name = "btnLetzteDateien";
            this.btnLetzteDateien.Size = new System.Drawing.Size(104, 66);
            this.btnLetzteDateien.TabIndex = 1;
            this.btnLetzteDateien.Text = "zuletzt geöffnete Dateien";
            this.btnLetzteDateien.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.LastDatabases_ItemClicked);
            // 
            // btnOeffnen
            // 
            this.btnOeffnen.ImageCode = "Ordner";
            this.btnOeffnen.Location = new System.Drawing.Point(72, 2);
            this.btnOeffnen.Name = "btnOeffnen";
            this.btnOeffnen.Size = new System.Drawing.Size(56, 66);
            this.btnOeffnen.TabIndex = 1;
            this.btnOeffnen.Text = "Öffnen";
            this.btnOeffnen.Click += new System.EventHandler(this.btnOeffnen_Click);
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.ImageCode = "Diskette";
            this.btnSaveAs.Location = new System.Drawing.Point(232, 2);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(64, 66);
            this.btnSaveAs.TabIndex = 4;
            this.btnSaveAs.Text = "Speichern unter";
            this.btnSaveAs.Click += new System.EventHandler(this.btnNeuDB_SaveAs_Click);
            // 
            // btnNeuDB
            // 
            this.btnNeuDB.ImageCode = "Datei";
            this.btnNeuDB.Location = new System.Drawing.Point(8, 2);
            this.btnNeuDB.Name = "btnNeuDB";
            this.btnNeuDB.Size = new System.Drawing.Size(56, 66);
            this.btnNeuDB.TabIndex = 0;
            this.btnNeuDB.Text = "Neu";
            this.btnNeuDB.Click += new System.EventHandler(this.btnNeuDB_SaveAs_Click);
            // 
            // tabStart
            // 
            this.tabStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabStart.Controls.Add(this.grpAnsicht);
            this.tabStart.Controls.Add(this.grpBearbeitung);
            this.tabStart.Controls.Add(this.grpFormularSteuerung);
            this.tabStart.Controls.Add(this.grpAllgemein);
            this.tabStart.Location = new System.Drawing.Point(4, 25);
            this.tabStart.Name = "tabStart";
            this.tabStart.Size = new System.Drawing.Size(1204, 81);
            this.tabStart.TabIndex = 1;
            this.tabStart.Text = "Start";
            // 
            // grpAnsicht
            // 
            this.grpAnsicht.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAnsicht.CausesValidation = false;
            this.grpAnsicht.Controls.Add(this.cbxColumnArr);
            this.grpAnsicht.Controls.Add(this.SpaltAnsichtCap);
            this.grpAnsicht.Controls.Add(this.Ansicht3);
            this.grpAnsicht.Controls.Add(this.Ansicht2);
            this.grpAnsicht.Controls.Add(this.Ansicht1);
            this.grpAnsicht.Controls.Add(this.Ansicht0);
            this.grpAnsicht.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAnsicht.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpAnsicht.Location = new System.Drawing.Point(808, 0);
            this.grpAnsicht.Name = "grpAnsicht";
            this.grpAnsicht.Size = new System.Drawing.Size(464, 81);
            this.grpAnsicht.TabIndex = 0;
            this.grpAnsicht.TabStop = false;
            this.grpAnsicht.Text = "Ansichten-Auswahl";
            // 
            // SpaltAnsichtCap
            // 
            this.SpaltAnsichtCap.CausesValidation = false;
            this.SpaltAnsichtCap.Location = new System.Drawing.Point(8, 46);
            this.SpaltAnsichtCap.Name = "SpaltAnsichtCap";
            this.SpaltAnsichtCap.Size = new System.Drawing.Size(98, 22);
            this.SpaltAnsichtCap.Text = "Spalten-Ansicht:";
            this.SpaltAnsichtCap.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // Ansicht3
            // 
            this.Ansicht3.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.Ansicht3.Location = new System.Drawing.Point(216, 24);
            this.Ansicht3.Name = "Ansicht3";
            this.Ansicht3.Size = new System.Drawing.Size(240, 22);
            this.Ansicht3.TabIndex = 15;
            this.Ansicht3.Text = "Tabelle und Formular übereinander";
            this.Ansicht3.Click += new System.EventHandler(this.Ansicht_Click);
            // 
            // Ansicht2
            // 
            this.Ansicht2.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.Ansicht2.Location = new System.Drawing.Point(216, 2);
            this.Ansicht2.Name = "Ansicht2";
            this.Ansicht2.Size = new System.Drawing.Size(240, 22);
            this.Ansicht2.TabIndex = 14;
            this.Ansicht2.Text = "Tabelle und Formular nebeneinander";
            this.Ansicht2.Click += new System.EventHandler(this.Ansicht_Click);
            // 
            // Ansicht1
            // 
            this.Ansicht1.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.Ansicht1.Location = new System.Drawing.Point(8, 24);
            this.Ansicht1.Name = "Ansicht1";
            this.Ansicht1.Size = new System.Drawing.Size(192, 22);
            this.Ansicht1.TabIndex = 13;
            this.Ansicht1.Text = "Überschriften und Formular";
            this.Ansicht1.Click += new System.EventHandler(this.Ansicht_Click);
            // 
            // Ansicht0
            // 
            this.Ansicht0.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox_Text;
            this.Ansicht0.Location = new System.Drawing.Point(8, 2);
            this.Ansicht0.Name = "Ansicht0";
            this.Ansicht0.Size = new System.Drawing.Size(104, 22);
            this.Ansicht0.TabIndex = 12;
            this.Ansicht0.Text = "Nur Tabelle";
            this.Ansicht0.Click += new System.EventHandler(this.Ansicht_Click);
            // 
            // grpBearbeitung
            // 
            this.grpBearbeitung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpBearbeitung.CausesValidation = false;
            this.grpBearbeitung.Controls.Add(this.btnAlleSchließen);
            this.grpBearbeitung.Controls.Add(this.btnAlleErweitern);
            this.grpBearbeitung.Controls.Add(this.Datenüberprüfung);
            this.grpBearbeitung.Controls.Add(this.AngezeigteZeilenLöschen);
            this.grpBearbeitung.Controls.Add(this.SuchenUndErsetzen);
            this.grpBearbeitung.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpBearbeitung.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpBearbeitung.Location = new System.Drawing.Point(448, 0);
            this.grpBearbeitung.Name = "grpBearbeitung";
            this.grpBearbeitung.Size = new System.Drawing.Size(360, 81);
            this.grpBearbeitung.TabIndex = 1;
            this.grpBearbeitung.TabStop = false;
            this.grpBearbeitung.Text = "Bearbeitung";
            // 
            // btnAlleSchließen
            // 
            this.btnAlleSchließen.Enabled = false;
            this.btnAlleSchließen.ImageCode = "Pfeil_Oben_Scrollbar|14|||||0";
            this.btnAlleSchließen.Location = new System.Drawing.Point(248, 24);
            this.btnAlleSchließen.Name = "btnAlleSchließen";
            this.btnAlleSchließen.QuickInfo = "Neuen Eintrag ergänzen";
            this.btnAlleSchließen.Size = new System.Drawing.Size(104, 22);
            this.btnAlleSchließen.TabIndex = 14;
            this.btnAlleSchließen.Text = "alle schließen";
            this.btnAlleSchließen.Click += new System.EventHandler(this.btnAlleSchließen_Click);
            // 
            // btnAlleErweitern
            // 
            this.btnAlleErweitern.Enabled = false;
            this.btnAlleErweitern.ImageCode = "Pfeil_Unten_Scrollbar|14|||ff0000||200|200";
            this.btnAlleErweitern.Location = new System.Drawing.Point(248, 2);
            this.btnAlleErweitern.Name = "btnAlleErweitern";
            this.btnAlleErweitern.QuickInfo = "Neuen Eintrag ergänzen";
            this.btnAlleErweitern.Size = new System.Drawing.Size(104, 22);
            this.btnAlleErweitern.TabIndex = 13;
            this.btnAlleErweitern.Text = "alle erweitern";
            this.btnAlleErweitern.Click += new System.EventHandler(this.btnAlleErweitern_Click);
            // 
            // Datenüberprüfung
            // 
            this.Datenüberprüfung.ImageCode = "Zeile||||||||||Häkchen";
            this.Datenüberprüfung.Location = new System.Drawing.Point(88, 2);
            this.Datenüberprüfung.Name = "Datenüberprüfung";
            this.Datenüberprüfung.QuickInfo = "Aktuell angezeigte Zeilen<br>automatisch überprüfen.";
            this.Datenüberprüfung.Size = new System.Drawing.Size(80, 66);
            this.Datenüberprüfung.TabIndex = 12;
            this.Datenüberprüfung.Text = "Daten-überprüfung";
            this.Datenüberprüfung.Click += new System.EventHandler(this.Datenüberprüfung_Click);
            // 
            // AngezeigteZeilenLöschen
            // 
            this.AngezeigteZeilenLöschen.ImageCode = "Zeile||||||||||Kreuz";
            this.AngezeigteZeilenLöschen.Location = new System.Drawing.Point(168, 2);
            this.AngezeigteZeilenLöschen.Name = "AngezeigteZeilenLöschen";
            this.AngezeigteZeilenLöschen.QuickInfo = "Angezeigte Zeilen löschen";
            this.AngezeigteZeilenLöschen.Size = new System.Drawing.Size(72, 66);
            this.AngezeigteZeilenLöschen.TabIndex = 9;
            this.AngezeigteZeilenLöschen.Text = "Zeilen löschen";
            this.AngezeigteZeilenLöschen.Click += new System.EventHandler(this.AngezeigteZeilenLöschen_Click);
            // 
            // SuchenUndErsetzen
            // 
            this.SuchenUndErsetzen.ImageCode = "Fernglas";
            this.SuchenUndErsetzen.Location = new System.Drawing.Point(8, 2);
            this.SuchenUndErsetzen.Name = "SuchenUndErsetzen";
            this.SuchenUndErsetzen.Size = new System.Drawing.Size(80, 66);
            this.SuchenUndErsetzen.TabIndex = 8;
            this.SuchenUndErsetzen.Text = "Suchen und ersetzen";
            this.SuchenUndErsetzen.Click += new System.EventHandler(this.SuchenUndErsetzen_Click);
            // 
            // grpFormularSteuerung
            // 
            this.grpFormularSteuerung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpFormularSteuerung.CausesValidation = false;
            this.grpFormularSteuerung.Controls.Add(this.SuchB);
            this.grpFormularSteuerung.Controls.Add(this.such);
            this.grpFormularSteuerung.Controls.Add(this.Vorwärts);
            this.grpFormularSteuerung.Controls.Add(this.zurück);
            this.grpFormularSteuerung.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpFormularSteuerung.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpFormularSteuerung.Location = new System.Drawing.Point(208, 0);
            this.grpFormularSteuerung.Name = "grpFormularSteuerung";
            this.grpFormularSteuerung.Size = new System.Drawing.Size(240, 81);
            this.grpFormularSteuerung.TabIndex = 2;
            this.grpFormularSteuerung.TabStop = false;
            this.grpFormularSteuerung.Text = "Formular-Ansicht-Steuerung";
            // 
            // SuchB
            // 
            this.SuchB.Enabled = false;
            this.SuchB.ImageCode = "Lupe|16";
            this.SuchB.Location = new System.Drawing.Point(112, 24);
            this.SuchB.Name = "SuchB";
            this.SuchB.QuickInfo = "Nächsten Eintrag anzeigen,<br>der obigen Text enthält";
            this.SuchB.Size = new System.Drawing.Size(120, 22);
            this.SuchB.TabIndex = 7;
            this.SuchB.Text = "Textsuche";
            this.SuchB.Click += new System.EventHandler(this.SuchB_Click);
            // 
            // such
            // 
            this.such.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.such.Location = new System.Drawing.Point(112, 2);
            this.such.Name = "such";
            this.such.Size = new System.Drawing.Size(120, 22);
            this.such.TabIndex = 6;
            this.such.TextChanged += new System.EventHandler(this.such_TextChanged);
            this.such.Enter += new System.EventHandler(this.such_Enter);
            // 
            // Vorwärts
            // 
            this.Vorwärts.ButtonStyle = BlueControls.Enums.enButtonStyle.SliderButton;
            this.Vorwärts.ImageCode = "Pfeil_Rechts";
            this.Vorwärts.Location = new System.Drawing.Point(56, 2);
            this.Vorwärts.Name = "Vorwärts";
            this.Vorwärts.QuickInfo = "Nächsten Eintrag anzeigen";
            this.Vorwärts.Size = new System.Drawing.Size(48, 66);
            this.Vorwärts.TabIndex = 5;
            this.Vorwärts.Text = "vor";
            this.Vorwärts.Click += new System.EventHandler(this.vor_Click);
            // 
            // zurück
            // 
            this.zurück.ButtonStyle = BlueControls.Enums.enButtonStyle.SliderButton;
            this.zurück.ImageCode = "Pfeil_Links";
            this.zurück.Location = new System.Drawing.Point(8, 2);
            this.zurück.Name = "zurück";
            this.zurück.QuickInfo = "Vorherigen Eintrag anzeigen";
            this.zurück.Size = new System.Drawing.Size(48, 66);
            this.zurück.TabIndex = 4;
            this.zurück.Text = "zurück";
            this.zurück.Click += new System.EventHandler(this.zurück_Click);
            // 
            // grpAllgemein
            // 
            this.grpAllgemein.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAllgemein.CausesValidation = false;
            this.grpAllgemein.Controls.Add(this.btnNeu);
            this.grpAllgemein.Controls.Add(this.btnDrucken);
            this.grpAllgemein.Controls.Add(this.btnLoeschen);
            this.grpAllgemein.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAllgemein.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpAllgemein.Location = new System.Drawing.Point(0, 0);
            this.grpAllgemein.Name = "grpAllgemein";
            this.grpAllgemein.Size = new System.Drawing.Size(208, 81);
            this.grpAllgemein.TabIndex = 4;
            this.grpAllgemein.TabStop = false;
            this.grpAllgemein.Text = "Allgemein";
            // 
            // btnNeu
            // 
            this.btnNeu.ImageCode = "PlusZeichen";
            this.btnNeu.Location = new System.Drawing.Point(8, 2);
            this.btnNeu.Name = "btnNeu";
            this.btnNeu.Size = new System.Drawing.Size(56, 66);
            this.btnNeu.TabIndex = 2;
            this.btnNeu.Text = "Neu";
            this.btnNeu.Click += new System.EventHandler(this.Neu_Click);
            // 
            // btnDrucken
            // 
            this.btnDrucken.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.btnDrucken.DrawStyle = BlueControls.Enums.enComboboxStyle.RibbonBar;
            this.btnDrucken.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.btnDrucken.ImageCode = "Drucker";
            this.btnDrucken.Location = new System.Drawing.Point(120, 2);
            this.btnDrucken.Name = "btnDrucken";
            this.btnDrucken.Size = new System.Drawing.Size(80, 66);
            this.btnDrucken.TabIndex = 12;
            this.btnDrucken.Text = "Drucken bzw. Export";
            this.btnDrucken.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.Drucken_ItemClicked);
            // 
            // btnLoeschen
            // 
            this.btnLoeschen.ImageCode = "Papierkorb";
            this.btnLoeschen.Location = new System.Drawing.Point(64, 2);
            this.btnLoeschen.Name = "btnLoeschen";
            this.btnLoeschen.Size = new System.Drawing.Size(56, 66);
            this.btnLoeschen.TabIndex = 3;
            this.btnLoeschen.Text = "löschen";
            this.btnLoeschen.Click += new System.EventHandler(this.LöscheZeile);
            // 
            // tabExtras
            // 
            this.tabExtras.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabExtras.Controls.Add(this.grpEditor);
            this.tabExtras.Location = new System.Drawing.Point(4, 25);
            this.tabExtras.Name = "tabExtras";
            this.tabExtras.Size = new System.Drawing.Size(1204, 81);
            this.tabExtras.TabIndex = 3;
            this.tabExtras.Text = "Extras";
            // 
            // grpEditor
            // 
            this.grpEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpEditor.CausesValidation = false;
            this.grpEditor.Controls.Add(this.AllgemeinerEditor);
            this.grpEditor.Controls.Add(this.BeziehungsEditor);
            this.grpEditor.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpEditor.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.RibbonBar;
            this.grpEditor.Location = new System.Drawing.Point(0, 0);
            this.grpEditor.Name = "grpEditor";
            this.grpEditor.Size = new System.Drawing.Size(152, 81);
            this.grpEditor.TabIndex = 0;
            this.grpEditor.TabStop = false;
            this.grpEditor.Text = "Editoren";
            // 
            // AllgemeinerEditor
            // 
            this.AllgemeinerEditor.ImageCode = "Stern";
            this.AllgemeinerEditor.Location = new System.Drawing.Point(8, 2);
            this.AllgemeinerEditor.Name = "AllgemeinerEditor";
            this.AllgemeinerEditor.Size = new System.Drawing.Size(64, 66);
            this.AllgemeinerEditor.TabIndex = 17;
            this.AllgemeinerEditor.Text = "Allgemein";
            this.AllgemeinerEditor.Click += new System.EventHandler(this.AllgemeinerEditor_Click);
            // 
            // BeziehungsEditor
            // 
            this.BeziehungsEditor.ImageCode = "Hierarchie";
            this.BeziehungsEditor.Location = new System.Drawing.Point(80, 2);
            this.BeziehungsEditor.Name = "BeziehungsEditor";
            this.BeziehungsEditor.Size = new System.Drawing.Size(64, 66);
            this.BeziehungsEditor.TabIndex = 16;
            this.BeziehungsEditor.Text = "Hierarchie";
            this.BeziehungsEditor.Click += new System.EventHandler(this.BeziehungsEditor_Click);
            // 
            // tabAdmin
            // 
            this.tabAdmin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabAdmin.Enabled = false;
            this.tabAdmin.Location = new System.Drawing.Point(4, 25);
            this.tabAdmin.Name = "tabAdmin";
            this.tabAdmin.Size = new System.Drawing.Size(1204, 81);
            this.tabAdmin.TabIndex = 5;
            this.tabAdmin.Text = "Administration";
            // 
            // Filter
            // 
            this.Filter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Filter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.Filter.Dock = System.Windows.Forms.DockStyle.Top;
            this.Filter.GroupBoxStyle = BlueControls.Enums.enGroupBoxStyle.Nothing;
            this.Filter.Location = new System.Drawing.Point(0, 110);
            this.Filter.Name = "Filter";
            this.Filter.Size = new System.Drawing.Size(1212, 42);
            this.Filter.TabIndex = 93;
            this.Filter.TabStop = false;
            this.Filter.Text = "Filter";
            // 
            // frmTableView
            // 
            this.ClientSize = new System.Drawing.Size(1212, 730);
            this.Controls.Add(this.Formula);
            this.Controls.Add(this.TableView);
            this.Controls.Add(this.Zei);
            this.Controls.Add(this.Filter);
            this.Controls.Add(this.MainRibbon);
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "frmTableView";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.MainRibbon.ResumeLayout(false);
            this.tabDatei.ResumeLayout(false);
            this.grpInformation.ResumeLayout(false);
            this.grpOrdner.ResumeLayout(false);
            this.grpDatei.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAnsicht.ResumeLayout(false);
            this.grpBearbeitung.ResumeLayout(false);
            this.grpFormularSteuerung.ResumeLayout(false);
            this.grpAllgemein.ResumeLayout(false);
            this.tabExtras.ResumeLayout(false);
            this.grpEditor.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private Table TableView;
        private GroupBox grpFormularSteuerung;
        private Button SuchB;
        private TextBox such;
        private Button Vorwärts;
        private Button zurück;
        private Button btnLoeschen;
        private Button btnNeu;
        private ComboBox btnDrucken;
        private Formula Formula;
        private Button btnOeffnen;
        private OpenFileDialog LoadTab;
        private Button btnNeuDB;
        private SaveFileDialog SaveTab;
        private Caption SpaltAnsichtCap;
        private ComboBox cbxColumnArr;
        private Button btnSaveAs;
        private Button SuchenUndErsetzen;
        private Button AngezeigteZeilenLöschen;
        private Caption Zei;
        private Button btnInformation;
        private Button Ansicht0;
        private Button Ansicht1;
        private Button Ansicht2;
        private Button Ansicht3;
        private Button btnTemporärenSpeicherortÖffnen;
        private Button BeziehungsEditor;
        internal  System.Windows.Forms.TabPage tabDatei;
        internal  System.Windows.Forms.TabPage tabStart;
        internal  System.Windows.Forms.TabPage tabExtras;
        internal Button btnDatenbanken;
        internal Button Datenüberprüfung;
        internal LastFilesCombo btnLetzteDateien;
        private Caption Copyright;
        private Button AllgemeinerEditor;
        private GroupBox grpAllgemein;
        private GroupBox grpInformation;
        private GroupBox grpEditor;
        private GroupBox grpAnsicht;
        private GroupBox grpBearbeitung;
        private GroupBox grpOrdner;
        private GroupBox grpDatei;
        private RibbonBar MainRibbon;
        private tabAdministration tabAdmin;
        private Filterleiste Filter;
        protected Button btnAlleSchließen;
        protected Button btnAlleErweitern;
    }
}