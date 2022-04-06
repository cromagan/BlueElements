#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2022 Christian Peter
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
    public partial class TableView_OLD 
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
            this.grpAnsichtWahl = new BlueControls.Controls.GroupBox();
            this.Ansicht2 = new BlueControls.Controls.Button();
            this.Ansicht1 = new BlueControls.Controls.Button();
            this.Ansicht0 = new BlueControls.Controls.Button();
            this.grpFormularSteuerung = new BlueControls.Controls.GroupBox();
            this.SuchB = new BlueControls.Controls.Button();
            this.such = new BlueControls.Controls.TextBox();
            this.btnVorwärts = new BlueControls.Controls.Button();
            this.btnZurück = new BlueControls.Controls.Button();
            this.btnNeu = new BlueControls.Controls.Button();
            this.btnLoeschen = new BlueControls.Controls.Button();
            this.ribMain.SuspendLayout();
            this.pnlDatabaseSelect.SuspendLayout();
            this.pnlSerachBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).BeginInit();
            this.SplitContainer1.Panel1.SuspendLayout();
            this.SplitContainer1.Panel2.SuspendLayout();
            this.SplitContainer1.SuspendLayout();
            this.tbcSidebar.SuspendLayout();
            this.tabFormula.SuspendLayout();
            this.tabAllgemein.SuspendLayout();
            this.grpAnsicht.SuspendLayout();
            this.grpAnsichtWahl.SuspendLayout();
            this.grpFormularSteuerung.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribMain
            // 
            this.ribMain.SelectedIndex = 1;
            this.ribMain.Size = new System.Drawing.Size(1311, 110);
            this.ribMain.TabDefaultOrder = new string[] {
        "Datei",
        "Allgemein",
        "Extras",
        "Administration"};
            this.ribMain.TabIndex = 92;
            // 
            // pnlDatabaseSelect
            // 
            this.pnlDatabaseSelect.Size = new System.Drawing.Size(961, 24);
            // 
            // tbcDatabaseSelector
            // 
            this.tbcDatabaseSelector.Size = new System.Drawing.Size(961, 24);
            // 
            // pnlSerachBar
            // 
            this.pnlSerachBar.Size = new System.Drawing.Size(961, 40);
            // 
            // FilterLeiste
            // 
            this.FilterLeiste.Size = new System.Drawing.Size(961, 42);
            this.FilterLeiste.TabIndex = 93;
            this.FilterLeiste.Text = "Filter";
            // 
            // Table
            // 
            this.Table.Size = new System.Drawing.Size(961, 531);
            this.Table.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.TableView_ContextMenu_Init);
            this.Table.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.TableView_ContextMenuItemClicked);
            this.Table.EditBeforeBeginEdit += new System.EventHandler<BlueDatabase.EventArgs.CellCancelEventArgs>(this.TableView_EditBeforeBeginEdit);
            // 
            // SplitContainer1
            // 
            this.SplitContainer1.Size = new System.Drawing.Size(1311, 595);
            this.SplitContainer1.SplitterDistance = 961;
            // 
            // tbcSidebar
            // 
            this.tbcSidebar.Size = new System.Drawing.Size(339, 595);
            // 
            // tabFormula
            // 
            this.tabFormula.Size = new System.Drawing.Size(331, 566);
            // 
            // capStatusbar
            // 
            this.capStatusbar.Size = new System.Drawing.Size(1007, 24);
            // 
            // Formula
            // 
            this.Formula.Size = new System.Drawing.Size(325, 560);
            // 
            // tabAllgemein
            // 
            this.tabAllgemein.Controls.Add(this.grpAnsichtWahl);
            this.tabAllgemein.Controls.Add(this.grpFormularSteuerung);
            this.tabAllgemein.Size = new System.Drawing.Size(1303, 81);
            this.tabAllgemein.Controls.SetChildIndex(this.grpAnsicht, 0);
            this.tabAllgemein.Controls.SetChildIndex(this.grpFormularSteuerung, 0);
            this.tabAllgemein.Controls.SetChildIndex(this.grpAnsichtWahl, 0);
            // 
            // grpAnsichtWahl
            // 
            this.grpAnsichtWahl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAnsichtWahl.CausesValidation = false;
            this.grpAnsichtWahl.Controls.Add(this.Ansicht2);
            this.grpAnsichtWahl.Controls.Add(this.Ansicht1);
            this.grpAnsichtWahl.Controls.Add(this.Ansicht0);
            this.grpAnsichtWahl.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpAnsichtWahl.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpAnsichtWahl.Location = new System.Drawing.Point(698, 0);
            this.grpAnsichtWahl.Name = "grpAnsichtWahl";
            this.grpAnsichtWahl.Size = new System.Drawing.Size(260, 81);
            this.grpAnsichtWahl.TabIndex = 0;
            this.grpAnsichtWahl.TabStop = false;
            this.grpAnsichtWahl.Text = "Ansichten-Auswahl";
            // 
            // Ansicht2
            // 
            this.Ansicht2.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Text)));
            this.Ansicht2.Location = new System.Drawing.Point(8, 46);
            this.Ansicht2.Name = "Ansicht2";
            this.Ansicht2.Size = new System.Drawing.Size(240, 22);
            this.Ansicht2.TabIndex = 14;
            this.Ansicht2.Text = "Tabelle und Formular nebeneinander";
            this.Ansicht2.Click += new System.EventHandler(this.Ansicht_Click);
            // 
            // Ansicht1
            // 
            this.Ansicht1.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Text)));
            this.Ansicht1.Location = new System.Drawing.Point(8, 24);
            this.Ansicht1.Name = "Ansicht1";
            this.Ansicht1.Size = new System.Drawing.Size(192, 22);
            this.Ansicht1.TabIndex = 13;
            this.Ansicht1.Text = "Überschriften und Formular";
            this.Ansicht1.Click += new System.EventHandler(this.Ansicht_Click);
            // 
            // Ansicht0
            // 
            this.Ansicht0.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Text)));
            this.Ansicht0.Location = new System.Drawing.Point(8, 2);
            this.Ansicht0.Name = "Ansicht0";
            this.Ansicht0.Size = new System.Drawing.Size(104, 22);
            this.Ansicht0.TabIndex = 12;
            this.Ansicht0.Text = "Nur Tabelle";
            this.Ansicht0.Click += new System.EventHandler(this.Ansicht_Click);
            // 
            // grpFormularSteuerung
            // 
            this.grpFormularSteuerung.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpFormularSteuerung.CausesValidation = false;
            this.grpFormularSteuerung.Controls.Add(this.btnNeu);
            this.grpFormularSteuerung.Controls.Add(this.SuchB);
            this.grpFormularSteuerung.Controls.Add(this.btnLoeschen);
            this.grpFormularSteuerung.Controls.Add(this.such);
            this.grpFormularSteuerung.Controls.Add(this.btnVorwärts);
            this.grpFormularSteuerung.Controls.Add(this.btnZurück);
            this.grpFormularSteuerung.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpFormularSteuerung.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpFormularSteuerung.Location = new System.Drawing.Point(328, 0);
            this.grpFormularSteuerung.Name = "grpFormularSteuerung";
            this.grpFormularSteuerung.Size = new System.Drawing.Size(370, 81);
            this.grpFormularSteuerung.TabIndex = 2;
            this.grpFormularSteuerung.TabStop = false;
            this.grpFormularSteuerung.Text = "Formular-Ansicht-Steuerung";
            // 
            // SuchB
            // 
            this.SuchB.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.SuchB.Enabled = false;
            this.SuchB.ImageCode = "Lupe|16";
            this.SuchB.Location = new System.Drawing.Point(240, 24);
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
            this.such.Location = new System.Drawing.Point(240, 2);
            this.such.Name = "such";
            this.such.Size = new System.Drawing.Size(120, 22);
            this.such.TabIndex = 6;
            this.such.Enter += new System.EventHandler(this.such_Enter);
            this.such.TextChanged += new System.EventHandler(this.such_TextChanged);
            // 
            // btnVorwärts
            // 
            this.btnVorwärts.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnVorwärts.ImageCode = "Pfeil_Rechts";
            this.btnVorwärts.Location = new System.Drawing.Point(184, 2);
            this.btnVorwärts.Name = "btnVorwärts";
            this.btnVorwärts.QuickInfo = "Nächsten Eintrag anzeigen";
            this.btnVorwärts.Size = new System.Drawing.Size(48, 66);
            this.btnVorwärts.TabIndex = 5;
            this.btnVorwärts.Text = "vor";
            this.btnVorwärts.Click += new System.EventHandler(this.vor_Click);
            // 
            // btnZurück
            // 
            this.btnZurück.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZurück.ImageCode = "Pfeil_Links";
            this.btnZurück.Location = new System.Drawing.Point(136, 2);
            this.btnZurück.Name = "btnZurück";
            this.btnZurück.QuickInfo = "Vorherigen Eintrag anzeigen";
            this.btnZurück.Size = new System.Drawing.Size(48, 66);
            this.btnZurück.TabIndex = 4;
            this.btnZurück.Text = "zurück";
            this.btnZurück.Click += new System.EventHandler(this.zurück_Click);
            // 
            // btnNeu
            // 
            this.btnNeu.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnNeu.ImageCode = "PlusZeichen";
            this.btnNeu.Location = new System.Drawing.Point(8, 2);
            this.btnNeu.Name = "btnNeu";
            this.btnNeu.Size = new System.Drawing.Size(56, 66);
            this.btnNeu.TabIndex = 2;
            this.btnNeu.Text = "Neu";
            this.btnNeu.Click += new System.EventHandler(this.Neu_Click);
            // 
            // btnLoeschen
            // 
            this.btnLoeschen.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnLoeschen.ImageCode = "Papierkorb";
            this.btnLoeschen.Location = new System.Drawing.Point(64, 2);
            this.btnLoeschen.Name = "btnLoeschen";
            this.btnLoeschen.Size = new System.Drawing.Size(56, 66);
            this.btnLoeschen.TabIndex = 3;
            this.btnLoeschen.Text = "löschen";
            this.btnLoeschen.Click += new System.EventHandler(this.LöscheZeile);
            // 
            // TableView_OLD
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(1311, 729);
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "TableView_OLD";
            this.ribMain.ResumeLayout(false);
            this.pnlDatabaseSelect.ResumeLayout(false);
            this.pnlSerachBar.ResumeLayout(false);
            this.SplitContainer1.Panel1.ResumeLayout(false);
            this.SplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).EndInit();
            this.SplitContainer1.ResumeLayout(false);
            this.tbcSidebar.ResumeLayout(false);
            this.tabFormula.ResumeLayout(false);
            this.tabAllgemein.ResumeLayout(false);
            this.grpAnsicht.ResumeLayout(false);
            this.grpAnsichtWahl.ResumeLayout(false);
            this.grpFormularSteuerung.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private GroupBox grpFormularSteuerung;
        private Button SuchB;
        private TextBox such;
        private Button btnVorwärts;
        private Button btnZurück;
        private Button btnLoeschen;
        private Button btnNeu;
        private Button Ansicht0;
        private Button Ansicht1;
        private Button Ansicht2;
        private GroupBox grpAnsichtWahl;
    }
}