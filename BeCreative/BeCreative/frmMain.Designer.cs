#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using BlueBasics.Enums;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueDatabase.EventArgs;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;
using Form = BlueControls.Forms.Form;
using GroupBox = BlueControls.Controls.GroupBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using TabControl = BlueControls.Controls.TabControl;
using TabPage = BlueControls.Controls.TabPage;
using TextBox = BlueControls.Controls.TextBox;


namespace BeCreative
{
    public partial class frmMain : Form
    {
        //Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        //Wird vom Windows Form-Designer benötigt.
        private IContainer components;

        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.LoadTab = new System.Windows.Forms.OpenFileDialog();
            this.SaveTab = new System.Windows.Forms.SaveFileDialog();
            this.BlueFormulax = new BlueControls.Controls.Formula();
            this.TableView = new BlueControls.Controls.Table();
            this.cbxColumnArr = new BlueControls.Controls.ComboBox();
            this.Zei = new BlueControls.Controls.Caption();
            this.MainRibbon = new BlueControls.Controls.TabControl();
            this.tabDatei = new BlueControls.Controls.TabPage();
            this.BlueFrame12 = new BlueControls.Controls.GroupBox();
            this.Copyright = new BlueControls.Controls.Caption();
            this.ÜberDiesesProgramm = new BlueControls.Controls.Button();
            this.BlueFrame2 = new BlueControls.Controls.GroupBox();
            this.Ordn = new BlueControls.Controls.Button();
            this.TemporärenSpeicherortÖffnen = new BlueControls.Controls.Button();
            this.BlueFrame1 = new BlueControls.Controls.GroupBox();
            this.LastDatabases = new BlueControls.Controls.LastFilesCombo();
            this.Öffnen = new BlueControls.Controls.Button();
            this.SaveAs = new BlueControls.Controls.Button();
            this.NeuDB = new BlueControls.Controls.Button();
            this.tabStart = new BlueControls.Controls.TabPage();
            this.BlueFrame5 = new BlueControls.Controls.GroupBox();
            this.SpaltAnsichtCap = new BlueControls.Controls.Caption();
            this.Ansicht3 = new BlueControls.Controls.Button();
            this.Ansicht2 = new BlueControls.Controls.Button();
            this.Ansicht1 = new BlueControls.Controls.Button();
            this.Ansicht0 = new BlueControls.Controls.Button();
            this.BlueFrame4 = new BlueControls.Controls.GroupBox();
            this.Datenüberprüfung = new BlueControls.Controls.Button();
            this.AngezeigteZeilenLöschen = new BlueControls.Controls.Button();
            this.SuchenUndErsetzen = new BlueControls.Controls.Button();
            this.FormulaViewBar = new BlueControls.Controls.GroupBox();
            this.SuchB = new BlueControls.Controls.Button();
            this.such = new BlueControls.Controls.TextBox();
            this.Vorwärts = new BlueControls.Controls.Button();
            this.zurück = new BlueControls.Controls.Button();
            this.TableViewBar = new BlueControls.Controls.GroupBox();
            this.btnTextLöschen = new BlueControls.Controls.Button();
            this.AlleFilterAus = new BlueControls.Controls.Button();
            this.ZeilenFilter_TextFeld = new BlueControls.Controls.TextBox();
            this.BlueFrame13 = new BlueControls.Controls.GroupBox();
            this.Neu = new BlueControls.Controls.Button();
            this.Drucken = new BlueControls.Controls.ComboBox();
            this.löschen = new BlueControls.Controls.Button();
            this.tabExtras = new BlueControls.Controls.TabPage();
            this.BlueFrame6 = new BlueControls.Controls.GroupBox();
            this.AllgemeinerEditor = new BlueControls.Controls.Button();
            this.BeziehungsEditor = new BlueControls.Controls.Button();
            this.tabAdmin = new BlueControls.BlueDatabaseDialogs.tabAdministration();
            this.MainRibbon.SuspendLayout();
            this.tabDatei.SuspendLayout();
            this.BlueFrame12.SuspendLayout();
            this.BlueFrame2.SuspendLayout();
            this.BlueFrame1.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.BlueFrame5.SuspendLayout();
            this.BlueFrame4.SuspendLayout();
            this.FormulaViewBar.SuspendLayout();
            this.TableViewBar.SuspendLayout();
            this.BlueFrame13.SuspendLayout();
            this.tabExtras.SuspendLayout();
            this.BlueFrame6.SuspendLayout();
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
            // BlueFormulax
            // 
            this.BlueFormulax.Location = new System.Drawing.Point(200, 144);
            this.BlueFormulax.MinimumSize = new System.Drawing.Size(320, 350);
            this.BlueFormulax.Name = "BlueFormulax";
            this.BlueFormulax.Size = new System.Drawing.Size(856, 352);
            this.BlueFormulax.TabIndex = 75;
            // 
            // TableView
            // 
            this.TableView.Dock = System.Windows.Forms.DockStyle.Left;
            this.TableView.Location = new System.Drawing.Point(0, 110);
            this.TableView.Name = "TableView";
            this.TableView.Size = new System.Drawing.Size(192, 604);
            this.TableView.TabIndex = 73;
            this.TableView.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.TableView_ContextMenu_Init);
            this.TableView.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.TableView_ContextMenuItemClicked);
            this.TableView.EditBeforeBeginEdit += new System.EventHandler<BlueDatabase.EventArgs.CellCancelEventArgs>(this.TableView_EditBeforeBeginEdit);
            this.TableView.CursorPosChanged += new System.EventHandler<BlueDatabase.EventArgs.CellEventArgs>(this.TableView_CursorPosChanged);
            this.TableView.ColumnArrangementChanged += new System.EventHandler(this.TableView_ColumnArrangementChanged);
            this.TableView.ViewChanged += new System.EventHandler(this.TableView_ViewChanged);
            this.TableView.RowsSorted += new System.EventHandler(this.TableView_RowsSorted);
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
            this.MainRibbon.IsRibbonBar = true;
            this.MainRibbon.Location = new System.Drawing.Point(0, 0);
            this.MainRibbon.Name = "MainRibbon";
            this.MainRibbon.Size = new System.Drawing.Size(1212, 110);
            this.MainRibbon.TabIndex = 92;
            // 
            // tabDatei
            // 
            this.tabDatei.Controls.Add(this.BlueFrame12);
            this.tabDatei.Controls.Add(this.BlueFrame2);
            this.tabDatei.Controls.Add(this.BlueFrame1);
            this.tabDatei.Location = new System.Drawing.Point(4, 25);
            this.tabDatei.Name = "tabDatei";
            this.tabDatei.Size = new System.Drawing.Size(1204, 81);
            this.tabDatei.TabIndex = 0;
            this.tabDatei.Text = "Datei";
            // 
            // BlueFrame12
            // 
            this.BlueFrame12.CausesValidation = false;
            this.BlueFrame12.Controls.Add(this.Copyright);
            this.BlueFrame12.Controls.Add(this.ÜberDiesesProgramm);
            this.BlueFrame12.Dock = System.Windows.Forms.DockStyle.Left;
            this.BlueFrame12.Location = new System.Drawing.Point(488, 0);
            this.BlueFrame12.Name = "BlueFrame12";
            this.BlueFrame12.Size = new System.Drawing.Size(280, 81);
            this.BlueFrame12.Text = "Information";
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
            // ÜberDiesesProgramm
            // 
            this.ÜberDiesesProgramm.ImageCode = "Information";
            this.ÜberDiesesProgramm.Location = new System.Drawing.Point(8, 2);
            this.ÜberDiesesProgramm.Name = "ÜberDiesesProgramm";
            this.ÜberDiesesProgramm.QuickInfo = "Über dieses Programm";
            this.ÜberDiesesProgramm.Size = new System.Drawing.Size(80, 66);
            this.ÜberDiesesProgramm.TabIndex = 27;
            this.ÜberDiesesProgramm.Text = "Information";
            this.ÜberDiesesProgramm.Click += new System.EventHandler(this.ÜberDiesesProgramm_Click);
            // 
            // BlueFrame2
            // 
            this.BlueFrame2.CausesValidation = false;
            this.BlueFrame2.Controls.Add(this.Ordn);
            this.BlueFrame2.Controls.Add(this.TemporärenSpeicherortÖffnen);
            this.BlueFrame2.Dock = System.Windows.Forms.DockStyle.Left;
            this.BlueFrame2.Location = new System.Drawing.Point(304, 0);
            this.BlueFrame2.Name = "BlueFrame2";
            this.BlueFrame2.Size = new System.Drawing.Size(184, 81);
            this.BlueFrame2.Text = "Ordner";
            // 
            // Ordn
            // 
            this.Ordn.ImageCode = "Ordner";
            this.Ordn.Location = new System.Drawing.Point(8, 2);
            this.Ordn.Name = "Ordn";
            this.Ordn.QuickInfo = "Speicherort der Datenbanken öffnen";
            this.Ordn.Size = new System.Drawing.Size(88, 66);
            this.Ordn.TabIndex = 27;
            this.Ordn.Text = "Datenbanken-Pfad";
            this.Ordn.Click += new System.EventHandler(this.Ordn_Click);
            // 
            // TemporärenSpeicherortÖffnen
            // 
            this.TemporärenSpeicherortÖffnen.ImageCode = "Ordner||||0000ff||126";
            this.TemporärenSpeicherortÖffnen.Location = new System.Drawing.Point(96, 2);
            this.TemporärenSpeicherortÖffnen.Name = "TemporärenSpeicherortÖffnen";
            this.TemporärenSpeicherortÖffnen.QuickInfo = "Temporären Speicherort öffnen";
            this.TemporärenSpeicherortÖffnen.Size = new System.Drawing.Size(80, 66);
            this.TemporärenSpeicherortÖffnen.TabIndex = 26;
            this.TemporärenSpeicherortÖffnen.Text = "Temporärer Speicherort";
            this.TemporärenSpeicherortÖffnen.Click += new System.EventHandler(this.TemporärenSpeicherortÖffnen_Click);
            // 
            // BlueFrame1
            // 
            this.BlueFrame1.CausesValidation = false;
            this.BlueFrame1.Controls.Add(this.LastDatabases);
            this.BlueFrame1.Controls.Add(this.Öffnen);
            this.BlueFrame1.Controls.Add(this.SaveAs);
            this.BlueFrame1.Controls.Add(this.NeuDB);
            this.BlueFrame1.Dock = System.Windows.Forms.DockStyle.Left;
            this.BlueFrame1.Location = new System.Drawing.Point(0, 0);
            this.BlueFrame1.Name = "BlueFrame1";
            this.BlueFrame1.Size = new System.Drawing.Size(304, 81);
            this.BlueFrame1.Text = "Datei";
            // 
            // LastDatabases
            // 
            this.LastDatabases.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.LastDatabases.DrawStyle = BlueControls.Enums.enComboboxStyle.RibbonBar;
            this.LastDatabases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LastDatabases.Enabled = false;
            this.LastDatabases.Format = BlueBasics.Enums.enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.LastDatabases.ImageCode = "Ordner";
            this.LastDatabases.Location = new System.Drawing.Point(128, 2);
            this.LastDatabases.Name = "LastDatabases";
            this.LastDatabases.Size = new System.Drawing.Size(104, 66);
            this.LastDatabases.TabIndex = 1;
            this.LastDatabases.Text = "zuletzt geöffnete Dateien";
            this.LastDatabases.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.LastDatabases_Item_Click);
            // 
            // Öffnen
            // 
            this.Öffnen.ImageCode = "Ordner";
            this.Öffnen.Location = new System.Drawing.Point(72, 2);
            this.Öffnen.Name = "Öffnen";
            this.Öffnen.Size = new System.Drawing.Size(56, 66);
            this.Öffnen.TabIndex = 1;
            this.Öffnen.Text = "Öffnen";
            this.Öffnen.Click += new System.EventHandler(this.Öffne_Click);
            // 
            // SaveAs
            // 
            this.SaveAs.ImageCode = "Diskette";
            this.SaveAs.Location = new System.Drawing.Point(232, 2);
            this.SaveAs.Name = "SaveAs";
            this.SaveAs.Size = new System.Drawing.Size(64, 66);
            this.SaveAs.TabIndex = 4;
            this.SaveAs.Text = "Speichern unter";
            this.SaveAs.Click += new System.EventHandler(this.NeuDBSaveAs_Click);
            // 
            // NeuDB
            // 
            this.NeuDB.ImageCode = "Datei";
            this.NeuDB.Location = new System.Drawing.Point(8, 2);
            this.NeuDB.Name = "NeuDB";
            this.NeuDB.Size = new System.Drawing.Size(56, 66);
            this.NeuDB.TabIndex = 0;
            this.NeuDB.Text = "Neu";
            this.NeuDB.Click += new System.EventHandler(this.NeuDBSaveAs_Click);
            // 
            // tabStart
            // 
            this.tabStart.Controls.Add(this.BlueFrame5);
            this.tabStart.Controls.Add(this.BlueFrame4);
            this.tabStart.Controls.Add(this.FormulaViewBar);
            this.tabStart.Controls.Add(this.TableViewBar);
            this.tabStart.Controls.Add(this.BlueFrame13);
            this.tabStart.Location = new System.Drawing.Point(4, 25);
            this.tabStart.Name = "tabStart";
            this.tabStart.Size = new System.Drawing.Size(1204, 81);
            this.tabStart.TabIndex = 1;
            this.tabStart.Text = "Start";
            // 
            // BlueFrame5
            // 
            this.BlueFrame5.CausesValidation = false;
            this.BlueFrame5.Controls.Add(this.cbxColumnArr);
            this.BlueFrame5.Controls.Add(this.SpaltAnsichtCap);
            this.BlueFrame5.Controls.Add(this.Ansicht3);
            this.BlueFrame5.Controls.Add(this.Ansicht2);
            this.BlueFrame5.Controls.Add(this.Ansicht1);
            this.BlueFrame5.Controls.Add(this.Ansicht0);
            this.BlueFrame5.Dock = System.Windows.Forms.DockStyle.Left;
            this.BlueFrame5.Location = new System.Drawing.Point(936, 0);
            this.BlueFrame5.Name = "BlueFrame5";
            this.BlueFrame5.Size = new System.Drawing.Size(464, 81);
            this.BlueFrame5.Text = "Ansichten-Auswahl";
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
            // BlueFrame4
            // 
            this.BlueFrame4.CausesValidation = false;
            this.BlueFrame4.Controls.Add(this.Datenüberprüfung);
            this.BlueFrame4.Controls.Add(this.AngezeigteZeilenLöschen);
            this.BlueFrame4.Controls.Add(this.SuchenUndErsetzen);
            this.BlueFrame4.Dock = System.Windows.Forms.DockStyle.Left;
            this.BlueFrame4.Location = new System.Drawing.Point(688, 0);
            this.BlueFrame4.Name = "BlueFrame4";
            this.BlueFrame4.Size = new System.Drawing.Size(248, 81);
            this.BlueFrame4.Text = "Bearbeitung";
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
            // FormulaViewBar
            // 
            this.FormulaViewBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
            this.FormulaViewBar.CausesValidation = false;
            this.FormulaViewBar.Controls.Add(this.SuchB);
            this.FormulaViewBar.Controls.Add(this.such);
            this.FormulaViewBar.Controls.Add(this.Vorwärts);
            this.FormulaViewBar.Controls.Add(this.zurück);
            this.FormulaViewBar.Dock = System.Windows.Forms.DockStyle.Left;
            this.FormulaViewBar.Location = new System.Drawing.Point(448, 0);
            this.FormulaViewBar.Name = "FormulaViewBar";
            this.FormulaViewBar.Size = new System.Drawing.Size(240, 81);
            this.FormulaViewBar.Text = "Formular-Ansicht-Steuerung";
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
            // TableViewBar
            // 
            this.TableViewBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
            this.TableViewBar.CausesValidation = false;
            this.TableViewBar.Controls.Add(this.btnTextLöschen);
            this.TableViewBar.Controls.Add(this.AlleFilterAus);
            this.TableViewBar.Controls.Add(this.ZeilenFilter_TextFeld);
            this.TableViewBar.Dock = System.Windows.Forms.DockStyle.Left;
            this.TableViewBar.Location = new System.Drawing.Point(208, 0);
            this.TableViewBar.Name = "TableViewBar";
            this.TableViewBar.Size = new System.Drawing.Size(240, 81);
            this.TableViewBar.Text = "Filter";
            // 
            // btnTextLöschen
            // 
            this.btnTextLöschen.ImageCode = "Kreuz|16";
            this.btnTextLöschen.Location = new System.Drawing.Point(144, 2);
            this.btnTextLöschen.Name = "btnTextLöschen";
            this.btnTextLöschen.Size = new System.Drawing.Size(24, 22);
            this.btnTextLöschen.TabIndex = 11;
            this.btnTextLöschen.Click += new System.EventHandler(this.btnTextLöschen_Click);
            // 
            // AlleFilterAus
            // 
            this.AlleFilterAus.ImageCode = "Trichter||||||||||Kreuz";
            this.AlleFilterAus.Location = new System.Drawing.Point(176, 2);
            this.AlleFilterAus.Name = "AlleFilterAus";
            this.AlleFilterAus.Size = new System.Drawing.Size(56, 66);
            this.AlleFilterAus.TabIndex = 9;
            this.AlleFilterAus.Text = "alle Filter aus";
            this.AlleFilterAus.Click += new System.EventHandler(this.AlleFilterAus_Click);
            // 
            // ZeilenFilter_TextFeld
            // 
            this.ZeilenFilter_TextFeld.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ZeilenFilter_TextFeld.Location = new System.Drawing.Point(8, 2);
            this.ZeilenFilter_TextFeld.Name = "ZeilenFilter_TextFeld";
            this.ZeilenFilter_TextFeld.QuickInfo = "Texte können mit <b>+</b> kombiniert werden.<br><i>Kein Leerzeichen beim +-Zeiche" +
    "n benutzen.";
            this.ZeilenFilter_TextFeld.Size = new System.Drawing.Size(136, 22);
            this.ZeilenFilter_TextFeld.TabIndex = 6;
            this.ZeilenFilter_TextFeld.TextChanged += new System.EventHandler(this.ZeilenFilter_TextFeld_TextChanged);
            this.ZeilenFilter_TextFeld.Enter += new System.EventHandler(this.ZeilenFilter_TextFeld_Enter);
            // 
            // BlueFrame13
            // 
            this.BlueFrame13.CausesValidation = false;
            this.BlueFrame13.Controls.Add(this.Neu);
            this.BlueFrame13.Controls.Add(this.Drucken);
            this.BlueFrame13.Controls.Add(this.löschen);
            this.BlueFrame13.Dock = System.Windows.Forms.DockStyle.Left;
            this.BlueFrame13.Location = new System.Drawing.Point(0, 0);
            this.BlueFrame13.Name = "BlueFrame13";
            this.BlueFrame13.Size = new System.Drawing.Size(208, 81);
            this.BlueFrame13.Text = "Allgemein";
            // 
            // Neu
            // 
            this.Neu.ImageCode = "PlusZeichen";
            this.Neu.Location = new System.Drawing.Point(8, 2);
            this.Neu.Name = "Neu";
            this.Neu.Size = new System.Drawing.Size(56, 66);
            this.Neu.TabIndex = 2;
            this.Neu.Text = "Neu";
            this.Neu.Click += new System.EventHandler(this.Neu_Click);
            // 
            // Drucken
            // 
            this.Drucken.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.Drucken.DrawStyle = BlueControls.Enums.enComboboxStyle.RibbonBar;
            this.Drucken.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Drucken.ImageCode = "Drucker";
            this.Drucken.Location = new System.Drawing.Point(120, 2);
            this.Drucken.Name = "Drucken";
            this.Drucken.Size = new System.Drawing.Size(80, 66);
            this.Drucken.TabIndex = 12;
            this.Drucken.Text = "Drucken bzw. Export";
            this.Drucken.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.Drucken_Item_Click);
            // 
            // löschen
            // 
            this.löschen.ImageCode = "Papierkorb";
            this.löschen.Location = new System.Drawing.Point(64, 2);
            this.löschen.Name = "löschen";
            this.löschen.Size = new System.Drawing.Size(56, 66);
            this.löschen.TabIndex = 3;
            this.löschen.Text = "löschen";
            this.löschen.Click += new System.EventHandler(this.LöscheZeile);
            // 
            // tabExtras
            // 
            this.tabExtras.Controls.Add(this.BlueFrame6);
            this.tabExtras.Location = new System.Drawing.Point(4, 25);
            this.tabExtras.Name = "tabExtras";
            this.tabExtras.Size = new System.Drawing.Size(1204, 81);
            this.tabExtras.TabIndex = 3;
            this.tabExtras.Text = "Extras";
            // 
            // BlueFrame6
            // 
            this.BlueFrame6.CausesValidation = false;
            this.BlueFrame6.Controls.Add(this.AllgemeinerEditor);
            this.BlueFrame6.Controls.Add(this.BeziehungsEditor);
            this.BlueFrame6.Dock = System.Windows.Forms.DockStyle.Left;
            this.BlueFrame6.Location = new System.Drawing.Point(0, 0);
            this.BlueFrame6.Name = "BlueFrame6";
            this.BlueFrame6.Size = new System.Drawing.Size(152, 81);
            this.BlueFrame6.Text = "Editoren";
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
            this.tabAdmin.Enabled = false;
            this.tabAdmin.Location = new System.Drawing.Point(4, 25);
            this.tabAdmin.Name = "tabAdmin";
            this.tabAdmin.Size = new System.Drawing.Size(1204, 81);
            this.tabAdmin.TabIndex = 5;
            this.tabAdmin.Text = "Administration";
            // 
            // frmMain
            // 
            this.ClientSize = new System.Drawing.Size(1212, 730);
            this.Controls.Add(this.BlueFormulax);
            this.Controls.Add(this.TableView);
            this.Controls.Add(this.Zei);
            this.Controls.Add(this.MainRibbon);
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "frmMain";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.MainRibbon.ResumeLayout(false);
            this.tabDatei.ResumeLayout(false);
            this.BlueFrame12.ResumeLayout(false);
            this.BlueFrame2.ResumeLayout(false);
            this.BlueFrame1.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.BlueFrame5.ResumeLayout(false);
            this.BlueFrame4.ResumeLayout(false);
            this.FormulaViewBar.ResumeLayout(false);
            this.TableViewBar.ResumeLayout(false);
            this.BlueFrame13.ResumeLayout(false);
            this.tabExtras.ResumeLayout(false);
            this.BlueFrame6.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        [STAThread]
        static void Main()
        {
            var culture = new System.Globalization.CultureInfo("de-DE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
        private Table TableView;
        private GroupBox FormulaViewBar;
        private Button SuchB;
        private TextBox such;
        private Button Vorwärts;
        private Button zurück;
        private Button löschen;
        private Button Neu;
        private TextBox ZeilenFilter_TextFeld;
        private Button AlleFilterAus;
        private GroupBox TableViewBar;
        private ComboBox Drucken;
        private Formula BlueFormulax;
        private Button Öffnen;
        private OpenFileDialog LoadTab;
        private Button NeuDB;
        private SaveFileDialog SaveTab;
        private Caption SpaltAnsichtCap;
        private ComboBox cbxColumnArr;
        private Button SaveAs;
        private Button SuchenUndErsetzen;
        private Button AngezeigteZeilenLöschen;
        private Caption Zei;
        private Button ÜberDiesesProgramm;
        private Button Ansicht0;
        private Button Ansicht1;
        private Button Ansicht2;
        private Button Ansicht3;
        private Button TemporärenSpeicherortÖffnen;
        private Button BeziehungsEditor;
        internal TabPage tabDatei;
        internal TabPage tabStart;
        internal TabPage tabExtras;
        internal Button Ordn;
        internal Button Datenüberprüfung;
        internal LastFilesCombo LastDatabases;
        private Caption Copyright;
        private Button AllgemeinerEditor;
        private GroupBox BlueFrame13;
        private GroupBox BlueFrame12;
        private GroupBox BlueFrame6;
        private GroupBox BlueFrame5;
        private GroupBox BlueFrame4;
        private GroupBox BlueFrame2;
        private GroupBox BlueFrame1;
        private TabControl MainRibbon;
        private tabAdministration tabAdmin;
        private Button btnTextLöschen;
    }

}