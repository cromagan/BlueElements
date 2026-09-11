// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using System.ComponentModel;

namespace BlueControls {
    partial class Befehlsreferenz {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components is not null)) {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            lstCommands = new ListBox();
            txbComms = new TextBox();
            grpBefehle = new GroupBox();
            btnFilterDel = new Button();
            txbFilter = new TextBox();
            grpVerwendung = new GroupBox();
            btnVerwendung = new Button();
            grpBefehle.SuspendLayout();
            grpVerwendung.SuspendLayout();
            SuspendLayout();
            // 
            // lstCommands
            // 
            lstCommands.AddAllowed = AddType.None;
            lstCommands.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lstCommands.Location = new Point(8, 56);
            lstCommands.Name = "lstCommands";
            lstCommands.Size = new Size(288, 566);
            lstCommands.TabIndex = 3;
            lstCommands.ItemClicked += lstCommands_ItemClicked;
            // 
            // txbComms
            // 
            txbComms.Cursor = System.Windows.Forms.Cursors.IBeam;
            txbComms.Dock = System.Windows.Forms.DockStyle.Fill;
            txbComms.Location = new Point(304, 32);
            txbComms.Name = "txbComms";
            txbComms.Size = new Size(496, 632);
            txbComms.TabIndex = 2;
            txbComms.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            // 
            // grpBefehle
            // 
            grpBefehle.BackColor = Color.FromArgb(240, 240, 240);
            grpBefehle.Controls.Add(btnFilterDel);
            grpBefehle.Controls.Add(txbFilter);
            grpBefehle.Controls.Add(lstCommands);
            grpBefehle.Dock = System.Windows.Forms.DockStyle.Left;
            grpBefehle.Location = new Point(0, 32);
            grpBefehle.Name = "grpBefehle";
            grpBefehle.Size = new Size(304, 632);
            grpBefehle.TabIndex = 5;
            grpBefehle.TabStop = false;
            grpBefehle.Text = "Befehle";
            // 
            // btnFilterDel
            // 
            btnFilterDel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnFilterDel.Enabled = false;
            btnFilterDel.ImageCode = "Trichter|16||1";
            btnFilterDel.Location = new Point(256, 24);
            btnFilterDel.Name = "btnFilterDel";
            btnFilterDel.QuickInfo = "Filter löschen";
            btnFilterDel.Size = new Size(40, 24);
            btnFilterDel.TabIndex = 5;
            btnFilterDel.Click += btnFilterDel_Click;
            // 
            // txbFilter
            // 
            txbFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
            txbFilter.Location = new Point(8, 24);
            txbFilter.Name = "txbFilter";
            txbFilter.QuickInfo = "Textfilter";
            txbFilter.Size = new Size(240, 24);
            txbFilter.SpellCheckingEnabled = true;
            txbFilter.TabIndex = 4;
            txbFilter.TextChanged += txbFilter_TextChanged;
            // 
            // grpVerwendung
            // 
            grpVerwendung.BackColor = Color.FromArgb(255, 255, 255);
            grpVerwendung.Controls.Add(btnVerwendung);
            grpVerwendung.Dock = System.Windows.Forms.DockStyle.Top;
            grpVerwendung.GroupBoxStyle = GroupBoxStyle.RoundRect;
            grpVerwendung.Location = new Point(0, 0);
            grpVerwendung.Name = "grpVerwendung";
            grpVerwendung.Size = new Size(800, 32);
            grpVerwendung.TabIndex = 6;
            grpVerwendung.TabStop = false;
            // 
            // btnVerwendung
            // 
            btnVerwendung.ImageCode = "Lupe|16";
            btnVerwendung.Location = new Point(8, 4);
            btnVerwendung.Name = "btnVerwendung";
            btnVerwendung.QuickInfo = "Listet für jeden Befehl alle Verwendungen in einer Textdatei auf";
            btnVerwendung.Size = new Size(120, 24);
            btnVerwendung.TabIndex = 0;
            btnVerwendung.Text = "Verwendung";
            btnVerwendung.Click += btnVerwendung_Click;
            // 
            // Befehlsreferenz
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            ClientSize = new Size(800, 664);
            Controls.Add(txbComms);
            Controls.Add(grpBefehle);
            Controls.Add(grpVerwendung);
            Name = "Befehlsreferenz";
            Text = "Befehlsübersicht";
            TopMost = true;
            grpBefehle.ResumeLayout(false);
            grpVerwendung.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private ListBox lstCommands;
        private TextBox txbComms;
        private GroupBox grpBefehle;
        private Button btnFilterDel;
        private TextBox txbFilter;
        private GroupBox grpVerwendung;
        private Button btnVerwendung;
    }
}