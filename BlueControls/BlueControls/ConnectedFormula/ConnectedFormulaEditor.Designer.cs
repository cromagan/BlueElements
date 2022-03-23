
namespace BlueControls.Forms {
    partial class ConnectedFormulaEditor {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.tabEditor = new System.Windows.Forms.TabPage();
            this.grpFelder = new BlueControls.Controls.GroupBox();
            this.btnKonstante = new BlueControls.Controls.Button();
            this.btnFeldHinzu = new BlueControls.Controls.Button();
            this.btnZeileHinzu = new BlueControls.Controls.Button();
            this.grpDesign.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.grpAssistent.SuspendLayout();
            this.tabEditor.SuspendLayout();
            this.grpFelder.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabRightSide
            // 
            this.tabRightSide.Location = new System.Drawing.Point(602, 110);
            this.tabRightSide.Size = new System.Drawing.Size(372, 389);
            // 
            // Pad
            // 
            this.Pad.Size = new System.Drawing.Size(602, 389);
            this.Pad.Changed += new System.EventHandler(this.Pad_Changed);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabEditor);
            this.Ribbon.Size = new System.Drawing.Size(974, 110);
            this.Ribbon.TabDefault = this.tabEditor;
            this.Ribbon.TabDefaultOrder = new string[] {
        "Editor",
        "Start",
        "Hintergrund",
        "Export"};
            this.Ribbon.Controls.SetChildIndex(this.tabEditor, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabExport, 0);
            this.Ribbon.Controls.SetChildIndex(this.tabStart, 0);
            // 
            // tabStart
            // 
            this.tabStart.Size = new System.Drawing.Size(966, 81);
            // 
            // tabExport
            // 
            this.tabExport.Size = new System.Drawing.Size(966, 81);
            // 
            // tabEditor
            // 
            this.tabEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabEditor.Controls.Add(this.grpFelder);
            this.tabEditor.Location = new System.Drawing.Point(4, 25);
            this.tabEditor.Margin = new System.Windows.Forms.Padding(0);
            this.tabEditor.Name = "tabEditor";
            this.tabEditor.Size = new System.Drawing.Size(966, 81);
            this.tabEditor.TabIndex = 4;
            this.tabEditor.Text = "Editor";
            // 
            // grpFelder
            // 
            this.grpFelder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpFelder.Controls.Add(this.btnKonstante);
            this.grpFelder.Controls.Add(this.btnFeldHinzu);
            this.grpFelder.Controls.Add(this.btnZeileHinzu);
            this.grpFelder.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpFelder.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpFelder.Location = new System.Drawing.Point(0, 0);
            this.grpFelder.Name = "grpFelder";
            this.grpFelder.Size = new System.Drawing.Size(293, 81);
            this.grpFelder.TabIndex = 0;
            this.grpFelder.TabStop = false;
            this.grpFelder.Text = "Felder";
            // 
            // btnKonstante
            // 
            this.btnKonstante.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnKonstante.ImageCode = "Textfeld|16";
            this.btnKonstante.Location = new System.Drawing.Point(136, 2);
            this.btnKonstante.Name = "btnKonstante";
            this.btnKonstante.QuickInfo = "Fügt einen konstanten Wert hinzu,\r\nder für Filterungen benutzt werden\r\nkann.";
            this.btnKonstante.Size = new System.Drawing.Size(64, 66);
            this.btnKonstante.TabIndex = 2;
            this.btnKonstante.Text = "Konstante hinzufügen";
            this.btnKonstante.Click += new System.EventHandler(this.btnKonstante_Click);
            // 
            // btnFeldHinzu
            // 
            this.btnFeldHinzu.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnFeldHinzu.ImageCode = "Stift|16|||||||||PlusZeichen";
            this.btnFeldHinzu.Location = new System.Drawing.Point(72, 2);
            this.btnFeldHinzu.Name = "btnFeldHinzu";
            this.btnFeldHinzu.QuickInfo = "Fügt ein Feld hinzu.\r\nDieses kann entweder von einer Zeile\r\nbefüllt werden, oder " +
    "frei eingegeben werden";
            this.btnFeldHinzu.Size = new System.Drawing.Size(64, 66);
            this.btnFeldHinzu.TabIndex = 1;
            this.btnFeldHinzu.Text = "Feld hinzufügen";
            this.btnFeldHinzu.Click += new System.EventHandler(this.btnFeldHinzu_Click);
            // 
            // btnZeileHinzu
            // 
            this.btnZeileHinzu.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZeileHinzu.ImageCode = "Zeile|16|||||||||PlusZeichen";
            this.btnZeileHinzu.Location = new System.Drawing.Point(8, 2);
            this.btnZeileHinzu.Name = "btnZeileHinzu";
            this.btnZeileHinzu.QuickInfo = "Fügt eine Zeile einer  Datenbank.\r\nAus dieser könne Felder extrahiert werden.";
            this.btnZeileHinzu.Size = new System.Drawing.Size(64, 66);
            this.btnZeileHinzu.TabIndex = 0;
            this.btnZeileHinzu.Text = "Zeile hinzufügen";
            this.btnZeileHinzu.Click += new System.EventHandler(this.btnZeileHinzu_Click);
            // 
            // ConnectedFormulaEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(974, 499);
            this.Name = "ConnectedFormulaEditor";
            this.Text = "ConnectedFormula";
            this.grpDesign.ResumeLayout(false);
            this.Ribbon.ResumeLayout(false);
            this.tabStart.ResumeLayout(false);
            this.grpAssistent.ResumeLayout(false);
            this.tabEditor.ResumeLayout(false);
            this.grpFelder.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabEditor;
        private Controls.GroupBox grpFelder;
        private Controls.Button btnZeileHinzu;
        private Controls.Button btnFeldHinzu;
        private Controls.Button btnKonstante;
    }
}