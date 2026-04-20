using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueTableDialogs {
    public sealed partial class ImportCsv {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            btnImportieren = new Button();
            chkDoppelteTrennzeichen = new Button();
            grpTrennzeichen = new GroupBox();
            txtAndere = new TextBox();
            optTabStopp = new Button();
            optAndere = new Button();
            optSemikolon = new Button();
            optLeerzeichen = new Button();
            optKomma = new Button();
            capEinträge = new Caption();
            chkTrennzeichenAmAnfang = new Button();
            btnCancel = new Button();
            grpZeilenOptionen = new GroupBox();
            optZeilenAlle = new Button();
            optZeilenZuorden = new Button();
            capSpaltenInfo = new Caption();
            pnlStatusBar.SuspendLayout();
            grpTrennzeichen.SuspendLayout();
            grpZeilenOptionen.SuspendLayout();
            SuspendLayout();
            // 
            // capStatusBar
            // 
            capStatusBar.Size = new Size(731, 24);
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.Location = new Point(0, 284);
            pnlStatusBar.Size = new Size(731, 24);
            // 
            // btnImportieren
            // 
            btnImportieren.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnImportieren.CustomContextMenuItems = null;
            btnImportieren.ImageCode = "Textfeld|16";
            btnImportieren.Location = new Point(621, 246);
            btnImportieren.Name = "btnImportieren";
            btnImportieren.Size = new Size(104, 32);
            btnImportieren.TabIndex = 9;
            btnImportieren.Text = "Importieren";
            btnImportieren.Click += Fertig_Click;
            // 
            // chkDoppelteTrennzeichen
            // 
            chkDoppelteTrennzeichen.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            chkDoppelteTrennzeichen.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkDoppelteTrennzeichen.CustomContextMenuItems = null;
            chkDoppelteTrennzeichen.Location = new Point(8, 40);
            chkDoppelteTrennzeichen.Name = "chkDoppelteTrennzeichen";
            chkDoppelteTrennzeichen.Size = new Size(715, 16);
            chkDoppelteTrennzeichen.TabIndex = 7;
            chkDoppelteTrennzeichen.Text = "Aufeinanderfolgende Trennzeichen als ein Zeichen behandeln";
            // 
            // grpTrennzeichen
            // 
            grpTrennzeichen.BackColor = Color.FromArgb(240, 240, 240);
            grpTrennzeichen.CausesValidation = false;
            grpTrennzeichen.Controls.Add(txtAndere);
            grpTrennzeichen.Controls.Add(optTabStopp);
            grpTrennzeichen.Controls.Add(optAndere);
            grpTrennzeichen.Controls.Add(optSemikolon);
            grpTrennzeichen.Controls.Add(optLeerzeichen);
            grpTrennzeichen.Controls.Add(optKomma);
            grpTrennzeichen.Location = new Point(16, 88);
            grpTrennzeichen.Name = "grpTrennzeichen";
            grpTrennzeichen.Size = new Size(168, 152);
            grpTrennzeichen.TabIndex = 13;
            grpTrennzeichen.TabStop = false;
            grpTrennzeichen.Text = "Trennzeichen";
            // 
            // txtAndere
            // 
            txtAndere.Cursor = Cursors.IBeam;
            txtAndere.CustomContextMenuItems = null;
            txtAndere.Location = new Point(88, 116);
            txtAndere.Name = "txtAndere";
            txtAndere.Size = new Size(64, 24);
            txtAndere.TabIndex = 6;
            // 
            // optTabStopp
            // 
            optTabStopp.ButtonStyle = ButtonStyle.Optionbox_Text;
            optTabStopp.Checked = true;
            optTabStopp.CustomContextMenuItems = null;
            optTabStopp.Location = new Point(16, 24);
            optTabStopp.Name = "optTabStopp";
            optTabStopp.Size = new Size(80, 16);
            optTabStopp.TabIndex = 1;
            optTabStopp.Text = "TabStop";
            // 
            // optAndere
            // 
            optAndere.ButtonStyle = ButtonStyle.Optionbox_Text;
            optAndere.CustomContextMenuItems = null;
            optAndere.Location = new Point(16, 120);
            optAndere.Name = "optAndere";
            optAndere.Size = new Size(64, 16);
            optAndere.TabIndex = 5;
            optAndere.Text = "Andere:";
            // 
            // optSemikolon
            // 
            optSemikolon.ButtonStyle = ButtonStyle.Optionbox_Text;
            optSemikolon.CustomContextMenuItems = null;
            optSemikolon.Location = new Point(16, 72);
            optSemikolon.Name = "optSemikolon";
            optSemikolon.Size = new Size(80, 16);
            optSemikolon.TabIndex = 2;
            optSemikolon.Text = "Semikolon";
            // 
            // optLeerzeichen
            // 
            optLeerzeichen.ButtonStyle = ButtonStyle.Optionbox_Text;
            optLeerzeichen.CustomContextMenuItems = null;
            optLeerzeichen.Location = new Point(16, 48);
            optLeerzeichen.Name = "optLeerzeichen";
            optLeerzeichen.Size = new Size(88, 16);
            optLeerzeichen.TabIndex = 4;
            optLeerzeichen.Text = "Leerzeichen";
            // 
            // optKomma
            // 
            optKomma.ButtonStyle = ButtonStyle.Optionbox_Text;
            optKomma.CustomContextMenuItems = null;
            optKomma.Location = new Point(16, 96);
            optKomma.Name = "optKomma";
            optKomma.Size = new Size(64, 16);
            optKomma.TabIndex = 3;
            optKomma.Text = "Komma";
            // 
            // capEinträge
            // 
            capEinträge.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            capEinträge.CausesValidation = false;
            capEinträge.CustomContextMenuItems = null;
            capEinträge.Location = new Point(8, 8);
            capEinträge.Name = "capEinträge";
            capEinträge.Size = new Size(721, 24);
            // 
            // chkTrennzeichenAmAnfang
            // 
            chkTrennzeichenAmAnfang.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            chkTrennzeichenAmAnfang.ButtonStyle = ButtonStyle.Checkbox_Text;
            chkTrennzeichenAmAnfang.CustomContextMenuItems = null;
            chkTrennzeichenAmAnfang.Location = new Point(8, 56);
            chkTrennzeichenAmAnfang.Name = "chkTrennzeichenAmAnfang";
            chkTrennzeichenAmAnfang.Size = new Size(715, 16);
            chkTrennzeichenAmAnfang.TabIndex = 10;
            chkTrennzeichenAmAnfang.Text = "Trennzeichen am Anfang entfernen";
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.CustomContextMenuItems = null;
            btnCancel.ImageCode = "Kreuz|16";
            btnCancel.Location = new Point(509, 246);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(104, 32);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Abbrechen";
            btnCancel.Click += Cancel_Click;
            // 
            // grpZeilenOptionen
            // 
            grpZeilenOptionen.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpZeilenOptionen.BackColor = Color.FromArgb(240, 240, 240);
            grpZeilenOptionen.CausesValidation = false;
            grpZeilenOptionen.Controls.Add(optZeilenAlle);
            grpZeilenOptionen.Controls.Add(optZeilenZuorden);
            grpZeilenOptionen.Location = new Point(192, 152);
            grpZeilenOptionen.Name = "grpZeilenOptionen";
            grpZeilenOptionen.Size = new Size(532, 88);
            grpZeilenOptionen.TabIndex = 0;
            grpZeilenOptionen.TabStop = false;
            grpZeilenOptionen.Text = "Zeilen-Optionen";
            // 
            // optZeilenAlle
            // 
            optZeilenAlle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            optZeilenAlle.ButtonStyle = ButtonStyle.Optionbox_Text;
            optZeilenAlle.CustomContextMenuItems = null;
            optZeilenAlle.Location = new Point(8, 56);
            optZeilenAlle.Name = "optZeilenAlle";
            optZeilenAlle.Size = new Size(516, 24);
            optZeilenAlle.TabIndex = 1;
            optZeilenAlle.Text = "<b>Jede Zeile</b> importieren, auch wenn dadurch <b>doppelte</b> Einträge entstehen.";
            // 
            // optZeilenZuorden
            // 
            optZeilenZuorden.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            optZeilenZuorden.ButtonStyle = ButtonStyle.Optionbox_Text;
            optZeilenZuorden.Checked = true;
            optZeilenZuorden.CustomContextMenuItems = null;
            optZeilenZuorden.Location = new Point(8, 24);
            optZeilenZuorden.Name = "optZeilenZuorden";
            optZeilenZuorden.Size = new Size(516, 32);
            optZeilenZuorden.TabIndex = 0;
            optZeilenZuorden.Text = "Die <b>erste Spalte</b> des Imports soll der <b>ersten Spalte</b> der Tabelle zugeordnet werden. <br>Wenn der Eintrag nicht in der Tabelle vorhanden ist, wird eine neue Zeile erstellt.";
            // 
            // capSpaltenInfo
            // 
            capSpaltenInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            capSpaltenInfo.CausesValidation = false;
            capSpaltenInfo.CustomContextMenuItems = null;
            capSpaltenInfo.Location = new Point(192, 96);
            capSpaltenInfo.Name = "capSpaltenInfo";
            capSpaltenInfo.Size = new Size(520, 48);
            capSpaltenInfo.Text = "<b><u>Achtung:</b></u><br>Die erste Zeile muss die Reihenfolge der Spalten enthalten. Diese werden dann zugeordnet.";
            // 
            // ImportCsv
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(731, 308);
            Controls.Add(capSpaltenInfo);
            Controls.Add(grpZeilenOptionen);
            Controls.Add(grpTrennzeichen);
            Controls.Add(btnCancel);
            Controls.Add(chkTrennzeichenAmAnfang);
            Controls.Add(btnImportieren);
            Controls.Add(chkDoppelteTrennzeichen);
            Controls.Add(capEinträge);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            GlobalMenuHeight = 0;
            Name = "ImportCsv";
            ShowInTaskbar = false;
            Text = "Einträge importieren:";
            Controls.SetChildIndex(capEinträge, 0);
            Controls.SetChildIndex(chkDoppelteTrennzeichen, 0);
            Controls.SetChildIndex(btnImportieren, 0);
            Controls.SetChildIndex(chkTrennzeichenAmAnfang, 0);
            Controls.SetChildIndex(btnCancel, 0);
            Controls.SetChildIndex(grpTrennzeichen, 0);
            Controls.SetChildIndex(grpZeilenOptionen, 0);
            Controls.SetChildIndex(pnlStatusBar, 0);
            Controls.SetChildIndex(capSpaltenInfo, 0);
            pnlStatusBar.ResumeLayout(false);
            grpTrennzeichen.ResumeLayout(false);
            grpZeilenOptionen.ResumeLayout(false);
            ResumeLayout(false);

        }
        private Caption capEinträge;
        private Button optTabStopp;
        private Button optSemikolon;
        private Button optKomma;
        private Button optLeerzeichen;
        private Button optAndere;
        private GroupBox grpTrennzeichen;
        private TextBox txtAndere;
        private Button chkDoppelteTrennzeichen;
        private Button btnImportieren;
        private Button chkTrennzeichenAmAnfang;
        private Button btnCancel;
        internal GroupBox grpZeilenOptionen;
        internal Button optZeilenAlle;
        internal Button optZeilenZuorden;
        private Caption capSpaltenInfo;
    }
}