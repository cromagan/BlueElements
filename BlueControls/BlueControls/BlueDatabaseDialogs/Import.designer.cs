using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs
{
    public sealed partial class Import 
    {
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.Fertig = new BlueControls.Controls.Button();
            this.Aufa = new BlueControls.Controls.Button();
            this.BlueFrame1 = new BlueControls.Controls.GroupBox();
            this.aTXT = new BlueControls.Controls.TextBox();
            this.TabStopp = new BlueControls.Controls.Button();
            this.Andere = new BlueControls.Controls.Button();
            this.Semikolon = new BlueControls.Controls.Button();
            this.Leerzeichen = new BlueControls.Controls.Button();
            this.Komma = new BlueControls.Controls.Button();
            this.Eintr = new BlueControls.Controls.Caption();
            this.AnfTre = new BlueControls.Controls.Button();
            this.BlueFrame3 = new BlueControls.Controls.GroupBox();
            this.SpalteNachderReihe = new BlueControls.Controls.Button();
            this.SpalteZuordnen = new BlueControls.Controls.Button();
            this.Cancel = new BlueControls.Controls.Button();
            this.GroupBox1 = new BlueControls.Controls.GroupBox();
            this.ZeilenAlle = new BlueControls.Controls.Button();
            this.ZeilenZuorden = new BlueControls.Controls.Button();
            this.pnlStatusBar.SuspendLayout();
            this.BlueFrame1.SuspendLayout();
            this.BlueFrame3.SuspendLayout();
            this.GroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(731, 24);
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 302);
            this.pnlStatusBar.Size = new System.Drawing.Size(731, 24);
            // 
            // Fertig
            // 
            this.Fertig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Fertig.ImageCode = "Textfeld|16";
            this.Fertig.Location = new System.Drawing.Point(621, 264);
            this.Fertig.Name = "Fertig";
            this.Fertig.Size = new System.Drawing.Size(104, 32);
            this.Fertig.TabIndex = 9;
            this.Fertig.Text = "Importieren";
            this.Fertig.Click += new System.EventHandler(this.Fertig_Click);
            // 
            // Aufa
            // 
            this.Aufa.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.Aufa.Location = new System.Drawing.Point(8, 40);
            this.Aufa.Name = "Aufa";
            this.Aufa.Size = new System.Drawing.Size(376, 16);
            this.Aufa.TabIndex = 7;
            this.Aufa.Text = "Aufeinanderfolgende Trennzeichen als ein Zeichen behandeln";
            // 
            // BlueFrame1
            // 
            this.BlueFrame1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.BlueFrame1.CausesValidation = false;
            this.BlueFrame1.Controls.Add(this.aTXT);
            this.BlueFrame1.Controls.Add(this.TabStopp);
            this.BlueFrame1.Controls.Add(this.Andere);
            this.BlueFrame1.Controls.Add(this.Semikolon);
            this.BlueFrame1.Controls.Add(this.Leerzeichen);
            this.BlueFrame1.Controls.Add(this.Komma);
            this.BlueFrame1.Location = new System.Drawing.Point(16, 88);
            this.BlueFrame1.Name = "BlueFrame1";
            this.BlueFrame1.Size = new System.Drawing.Size(168, 169);
            this.BlueFrame1.TabIndex = 13;
            this.BlueFrame1.TabStop = false;
            this.BlueFrame1.Text = "Trennzeichen";
            // 
            // aTXT
            // 
            this.aTXT.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.aTXT.Location = new System.Drawing.Point(88, 116);
            this.aTXT.Name = "aTXT";
            this.aTXT.Size = new System.Drawing.Size(64, 24);
            this.aTXT.TabIndex = 6;
            // 
            // TabStopp
            // 
            this.TabStopp.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.TabStopp.Checked = true;
            this.TabStopp.Location = new System.Drawing.Point(16, 24);
            this.TabStopp.Name = "TabStopp";
            this.TabStopp.Size = new System.Drawing.Size(80, 16);
            this.TabStopp.TabIndex = 1;
            this.TabStopp.Text = "TabStop";
            // 
            // Andere
            // 
            this.Andere.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.Andere.Location = new System.Drawing.Point(16, 120);
            this.Andere.Name = "Andere";
            this.Andere.Size = new System.Drawing.Size(64, 16);
            this.Andere.TabIndex = 5;
            this.Andere.Text = "Andere:";
            // 
            // Semikolon
            // 
            this.Semikolon.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.Semikolon.Location = new System.Drawing.Point(16, 72);
            this.Semikolon.Name = "Semikolon";
            this.Semikolon.Size = new System.Drawing.Size(80, 16);
            this.Semikolon.TabIndex = 2;
            this.Semikolon.Text = "Semikolon";
            // 
            // Leerzeichen
            // 
            this.Leerzeichen.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.Leerzeichen.Location = new System.Drawing.Point(16, 48);
            this.Leerzeichen.Name = "Leerzeichen";
            this.Leerzeichen.Size = new System.Drawing.Size(88, 16);
            this.Leerzeichen.TabIndex = 4;
            this.Leerzeichen.Text = "Leerzeichen";
            // 
            // Komma
            // 
            this.Komma.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.Komma.Location = new System.Drawing.Point(16, 96);
            this.Komma.Name = "Komma";
            this.Komma.Size = new System.Drawing.Size(64, 16);
            this.Komma.TabIndex = 3;
            this.Komma.Text = "Komma";
            // 
            // Eintr
            // 
            this.Eintr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Eintr.CausesValidation = false;
            this.Eintr.Location = new System.Drawing.Point(8, 8);
            this.Eintr.Name = "Eintr";
            this.Eintr.Size = new System.Drawing.Size(721, 24);
            this.Eintr.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // AnfTre
            // 
            this.AnfTre.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.AnfTre.Location = new System.Drawing.Point(8, 56);
            this.AnfTre.Name = "AnfTre";
            this.AnfTre.Size = new System.Drawing.Size(224, 16);
            this.AnfTre.TabIndex = 10;
            this.AnfTre.Text = "Trennzeichen am Anfang entfernen";
            // 
            // BlueFrame3
            // 
            this.BlueFrame3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.BlueFrame3.CausesValidation = false;
            this.BlueFrame3.Controls.Add(this.SpalteNachderReihe);
            this.BlueFrame3.Controls.Add(this.SpalteZuordnen);
            this.BlueFrame3.Location = new System.Drawing.Point(192, 88);
            this.BlueFrame3.Name = "BlueFrame3";
            this.BlueFrame3.Size = new System.Drawing.Size(532, 72);
            this.BlueFrame3.TabIndex = 12;
            this.BlueFrame3.TabStop = false;
            this.BlueFrame3.Text = "Spalten-Optionen";
            // 
            // SpalteNachderReihe
            // 
            this.SpalteNachderReihe.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.SpalteNachderReihe.Location = new System.Drawing.Point(8, 40);
            this.SpalteNachderReihe.Name = "SpalteNachderReihe";
            this.SpalteNachderReihe.Size = new System.Drawing.Size(512, 24);
            this.SpalteNachderReihe.TabIndex = 1;
            this.SpalteNachderReihe.Text = "Alle Einträge importieren, <b>interne Spaltenreihenfolge</b> benutzen und nichts " +
    "zuordnen.";
            // 
            // SpalteZuordnen
            // 
            this.SpalteZuordnen.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.SpalteZuordnen.Checked = true;
            this.SpalteZuordnen.Location = new System.Drawing.Point(8, 16);
            this.SpalteZuordnen.Name = "SpalteZuordnen";
            this.SpalteZuordnen.Size = new System.Drawing.Size(512, 24);
            this.SpalteZuordnen.TabIndex = 0;
            this.SpalteZuordnen.Text = "Die <b>erste Zeile</b> enthält den Spaltennamen und zu diesen zuordnen.";
            // 
            // Cancel
            // 
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.ImageCode = "Kreuz|16";
            this.Cancel.Location = new System.Drawing.Point(509, 264);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(104, 32);
            this.Cancel.TabIndex = 11;
            this.Cancel.Text = "Abbrechen";
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // GroupBox1
            // 
            this.GroupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.GroupBox1.CausesValidation = false;
            this.GroupBox1.Controls.Add(this.ZeilenAlle);
            this.GroupBox1.Controls.Add(this.ZeilenZuorden);
            this.GroupBox1.Location = new System.Drawing.Point(192, 168);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(532, 88);
            this.GroupBox1.TabIndex = 0;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Zeilen-Optionen";
            // 
            // ZeilenAlle
            // 
            this.ZeilenAlle.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.ZeilenAlle.Location = new System.Drawing.Point(8, 56);
            this.ZeilenAlle.Name = "ZeilenAlle";
            this.ZeilenAlle.Size = new System.Drawing.Size(512, 24);
            this.ZeilenAlle.TabIndex = 1;
            this.ZeilenAlle.Text = "<b>Jede Zeile</b> importieren, auch wenn dadurch <b>doppelte</b> Einträge entsteh" +
    "en.";
            // 
            // ZeilenZuorden
            // 
            this.ZeilenZuorden.ButtonStyle = BlueControls.Enums.ButtonStyle.Optionbox_Text;
            this.ZeilenZuorden.Checked = true;
            this.ZeilenZuorden.Location = new System.Drawing.Point(8, 24);
            this.ZeilenZuorden.Name = "ZeilenZuorden";
            this.ZeilenZuorden.Size = new System.Drawing.Size(512, 32);
            this.ZeilenZuorden.TabIndex = 0;
            this.ZeilenZuorden.Text = "Die <b>erste Spalte</b> des Imports soll der <b>ersten Spalte</b> der Datenbank z" +
    "ugeordnet werden. <br>Wenn der Eintrag nicht in der Datenbank vorhanden ist, wir" +
    "d eine neue Zeile erstellt.";
            // 
            // Import
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 326);
            this.Controls.Add(this.GroupBox1);
            this.Controls.Add(this.BlueFrame1);
            this.Controls.Add(this.BlueFrame3);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.AnfTre);
            this.Controls.Add(this.Fertig);
            this.Controls.Add(this.Aufa);
            this.Controls.Add(this.Eintr);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Import";
            this.ShowInTaskbar = false;
            this.Text = "Einträge importieren:";
            this.Controls.SetChildIndex(this.Eintr, 0);
            this.Controls.SetChildIndex(this.Aufa, 0);
            this.Controls.SetChildIndex(this.Fertig, 0);
            this.Controls.SetChildIndex(this.AnfTre, 0);
            this.Controls.SetChildIndex(this.Cancel, 0);
            this.Controls.SetChildIndex(this.BlueFrame3, 0);
            this.Controls.SetChildIndex(this.BlueFrame1, 0);
            this.Controls.SetChildIndex(this.GroupBox1, 0);
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.BlueFrame1.ResumeLayout(false);
            this.BlueFrame3.ResumeLayout(false);
            this.GroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Caption Eintr;
        private Button TabStopp;
        private Button Semikolon;
        private Button Komma;
        private Button Leerzeichen;
        private Button Andere;
        private GroupBox BlueFrame1;
        private TextBox aTXT;
        private Button Aufa;
        private Button Fertig;
        private Button AnfTre;
        private GroupBox BlueFrame3;
        private Button Cancel;
        internal Button SpalteNachderReihe;
        private Button SpalteZuordnen;
        internal GroupBox GroupBox1;
        internal Button ZeilenAlle;
        internal Button ZeilenZuorden;
    }
}
