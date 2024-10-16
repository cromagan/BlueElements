using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueScript.EventArgs;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs {
    public sealed partial class CreativePadScriptEditor {
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) {
            if (disposing) {
            }
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.eventScriptEditor = new ScriptEditor();
            this.ribMain = new RibbonBar();
            this.tabStart = new TabPage();
            this.grpAktionen = new GroupBox();
            this.btnSaveLoad = new Button();
            this.grpInfos = new GroupBox();
            this.btnDatenbankKopf = new Button();
            this.btnBefehlsUebersicht = new Button();
            this.grpAusführen = new GroupBox();
            this.txbTestZeile = new TextBox();
            this.cpZeile = new Caption();
            this.btnAusführen = new Button();
            this.cpad = new CreativePad();
            this.pnlStatusBar.SuspendLayout();
            this.ribMain.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAktionen.SuspendLayout();
            this.grpInfos.SuspendLayout();
            this.grpAusführen.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new Size(1297, 24);
            this.capStatusBar.Text = "<imagecode=Information|16> 14:41:21 Überprüfe auf Veränderungen von 2 Datenbanken" +
    " des Typs \'Database\'";
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new Point(0, 653);
            this.pnlStatusBar.Size = new Size(1297, 24);
            // 
            // eventScriptEditor
            // 
            this.eventScriptEditor.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.eventScriptEditor.Dock = DockStyle.Fill;
            this.eventScriptEditor.Location = new Point(0, 110);
            this.eventScriptEditor.Name = "eventScriptEditor";
            this.eventScriptEditor.Script = "";
            this.eventScriptEditor.Size = new Size(840, 543);
            this.eventScriptEditor.TabIndex = 6;
            this.eventScriptEditor.TabStop = false;
            this.eventScriptEditor.ExecuteScript += new EventHandler<ScriptEventArgs>(this.eventScriptEditor_ExecuteScript);
            // 
            // ribMain
            // 
            this.ribMain.Controls.Add(this.tabStart);
            this.ribMain.Dock = DockStyle.Top;
            this.ribMain.HotTrack = true;
            this.ribMain.Location = new Point(0, 0);
            this.ribMain.Name = "ribMain";
            this.ribMain.SelectedIndex = 0;
            this.ribMain.Size = new Size(1297, 110);
            this.ribMain.TabDefault = null;
            this.ribMain.TabDefaultOrder = new string[0];
            this.ribMain.TabIndex = 97;
            // 
            // tabStart
            // 
            this.tabStart.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabStart.Controls.Add(this.grpAktionen);
            this.tabStart.Controls.Add(this.grpInfos);
            this.tabStart.Controls.Add(this.grpAusführen);
            this.tabStart.Location = new Point(4, 25);
            this.tabStart.Name = "tabStart";
            this.tabStart.Padding = new Padding(3);
            this.tabStart.Size = new Size(1289, 81);
            this.tabStart.TabIndex = 0;
            this.tabStart.Text = "Start";
            // 
            // grpAktionen
            // 
            this.grpAktionen.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAktionen.Controls.Add(this.btnSaveLoad);
            this.grpAktionen.Dock = DockStyle.Left;
            this.grpAktionen.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpAktionen.Location = new Point(608, 3);
            this.grpAktionen.Name = "grpAktionen";
            this.grpAktionen.Size = new Size(64, 75);
            this.grpAktionen.TabIndex = 2;
            this.grpAktionen.TabStop = false;
            this.grpAktionen.Text = "Aktionen";
            // 
            // btnSaveLoad
            // 
            this.btnSaveLoad.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnSaveLoad.ImageCode = "Diskette|16";
            this.btnSaveLoad.Location = new Point(8, 2);
            this.btnSaveLoad.Name = "btnSaveLoad";
            this.btnSaveLoad.QuickInfo = "Aktualisiert die Datenbank-Daten. (Speichern, neu Laden)";
            this.btnSaveLoad.Size = new Size(48, 66);
            this.btnSaveLoad.TabIndex = 45;
            this.btnSaveLoad.Text = "Daten aktual.";
            this.btnSaveLoad.Click += new EventHandler(this.btnSave_Click);
            // 
            // grpInfos
            // 
            this.grpInfos.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpInfos.Controls.Add(this.btnDatenbankKopf);
            this.grpInfos.Controls.Add(this.btnBefehlsUebersicht);
            this.grpInfos.Dock = DockStyle.Left;
            this.grpInfos.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpInfos.Location = new Point(464, 3);
            this.grpInfos.Name = "grpInfos";
            this.grpInfos.Size = new Size(144, 75);
            this.grpInfos.TabIndex = 1;
            this.grpInfos.TabStop = false;
            this.grpInfos.Text = "Infos";
            // 
            // btnDatenbankKopf
            // 
            this.btnDatenbankKopf.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnDatenbankKopf.ImageCode = "Datenbank||||||||||Stift";
            this.btnDatenbankKopf.Location = new Point(8, 2);
            this.btnDatenbankKopf.Name = "btnDatenbankKopf";
            this.btnDatenbankKopf.Size = new Size(64, 66);
            this.btnDatenbankKopf.TabIndex = 46;
            this.btnDatenbankKopf.Text = "Datenbank-Kopf";
            this.btnDatenbankKopf.Click += new EventHandler(this.btnDatenbankKopf_Click);
            // 
            // btnBefehlsUebersicht
            // 
            this.btnBefehlsUebersicht.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnBefehlsUebersicht.ImageCode = "Tabelle|16";
            this.btnBefehlsUebersicht.Location = new Point(72, 2);
            this.btnBefehlsUebersicht.Name = "btnBefehlsUebersicht";
            this.btnBefehlsUebersicht.Size = new Size(64, 66);
            this.btnBefehlsUebersicht.TabIndex = 4;
            this.btnBefehlsUebersicht.Text = "Befehls-Übersicht";
            // 
            // grpAusführen
            // 
            this.grpAusführen.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpAusführen.Controls.Add(this.txbTestZeile);
            this.grpAusführen.Controls.Add(this.cpZeile);
            this.grpAusführen.Controls.Add(this.btnAusführen);
            this.grpAusführen.Dock = DockStyle.Left;
            this.grpAusführen.GroupBoxStyle = GroupBoxStyle.RibbonBar;
            this.grpAusführen.Location = new Point(3, 3);
            this.grpAusführen.Name = "grpAusführen";
            this.grpAusführen.Size = new Size(461, 75);
            this.grpAusführen.TabIndex = 0;
            this.grpAusführen.TabStop = false;
            this.grpAusführen.Text = "Ausführen";
            // 
            // txbTestZeile
            // 
            this.txbTestZeile.Cursor = Cursors.IBeam;
            this.txbTestZeile.Location = new Point(73, 24);
            this.txbTestZeile.Name = "txbTestZeile";
            this.txbTestZeile.RaiseChangeDelay = 5;
            this.txbTestZeile.Size = new Size(379, 22);
            this.txbTestZeile.TabIndex = 9;
            // 
            // cpZeile
            // 
            this.cpZeile.CausesValidation = false;
            this.cpZeile.Location = new Point(73, 2);
            this.cpZeile.Name = "cpZeile";
            this.cpZeile.Size = new Size(112, 22);
            this.cpZeile.Text = "Betreffende Zeile:";
            // 
            // btnAusführen
            // 
            this.btnAusführen.ButtonStyle = ButtonStyle.Button_Big_Borderless;
            this.btnAusführen.ImageCode = "Abspielen";
            this.btnAusführen.Location = new Point(0, 2);
            this.btnAusführen.Name = "btnAusführen";
            this.btnAusführen.Size = new Size(60, 66);
            this.btnAusführen.TabIndex = 3;
            this.btnAusführen.Text = "Aus-führen";
            this.btnAusführen.Click += new EventHandler(this.btnAusführen_Click);
            // 
            // cpad
            // 
            this.cpad.CausesValidation = false;
            this.cpad.Dock = DockStyle.Right;
            this.cpad.Location = new Point(840, 110);
            this.cpad.Name = "cpad";
            this.cpad.ShiftX = 0F;
            this.cpad.ShiftY = 0F;
            this.cpad.ShowJointPoint = true;
            this.cpad.Size = new Size(457, 543);
            this.cpad.TabIndex = 0;
            this.cpad.TabStop = false;
            this.cpad.Text = "connectedCreativePad1";
            this.cpad.Zoom = 1F;
            // 
            // CreativePadScriptEditor
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.ClientSize = new Size(1297, 677);
            this.Controls.Add(this.eventScriptEditor);
            this.Controls.Add(this.cpad);
            this.Controls.Add(this.ribMain);
            this.MinimizeBox = false;
            this.Name = "CreativePadScriptEditor";
            this.Text = "Datenbank-Eigenschaften";
            this.TopMost = true;
            this.WindowState = FormWindowState.Maximized;
            this.Controls.SetChildIndex(this.ribMain, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.cpad, 0);
            this.Controls.SetChildIndex(this.eventScriptEditor, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.ribMain.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAktionen.ResumeLayout(false);
            this.grpInfos.ResumeLayout(false);
            this.grpAusführen.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private ScriptEditor eventScriptEditor;
        private RibbonBar ribMain;
        private TabPage tabStart;
        private GroupBox grpInfos;
        private Button btnBefehlsUebersicht;
        private GroupBox grpAusführen;
        private GroupBox grpAktionen;
        private Button btnSaveLoad;
        private Button btnAusführen;
        private TextBox txbTestZeile;
        private Caption cpZeile;
        private Button btnDatenbankKopf;
        private CreativePad cpad;
    }
}
