
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
            this.btnDatenbankhinzu = new BlueControls.Controls.Button();
            this.btnFreiesFeld = new BlueControls.Controls.Button();
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
            this.tabEditor.Name = "tabEditor";
            this.tabEditor.Padding = new System.Windows.Forms.Padding(3);
            this.tabEditor.Size = new System.Drawing.Size(966, 81);
            this.tabEditor.TabIndex = 4;
            this.tabEditor.Text = "Editor";
            // 
            // grpFelder
            // 
            this.grpFelder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.grpFelder.Controls.Add(this.btnFreiesFeld);
            this.grpFelder.Controls.Add(this.btnDatenbankhinzu);
            this.grpFelder.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpFelder.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.RibbonBar;
            this.grpFelder.Location = new System.Drawing.Point(3, 3);
            this.grpFelder.Name = "grpFelder";
            this.grpFelder.Size = new System.Drawing.Size(293, 75);
            this.grpFelder.TabIndex = 0;
            this.grpFelder.TabStop = false;
            this.grpFelder.Text = "Felder";
            // 
            // btnDatenbankhinzu
            // 
            this.btnDatenbankhinzu.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnDatenbankhinzu.ImageCode = "Datenbank|16|||||||||PlusZeichen";
            this.btnDatenbankhinzu.Location = new System.Drawing.Point(8, 2);
            this.btnDatenbankhinzu.Name = "btnDatenbankhinzu";
            this.btnDatenbankhinzu.QuickInfo = "Fügt eine Datenbank hinzu,\r\ndie geladen wird und mit der\r\ninteragiert werden kann" +
    ".";
            this.btnDatenbankhinzu.Size = new System.Drawing.Size(64, 66);
            this.btnDatenbankhinzu.TabIndex = 0;
            this.btnDatenbankhinzu.Text = "Datenbank hinzufügen";
            this.btnDatenbankhinzu.Click += new System.EventHandler(this.btnDatenbankhinzu_Click);
            // 
            // btnFreiesFeld
            // 
            this.btnFreiesFeld.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnFreiesFeld.ImageCode = "Stift|16|||||||||PlusZeichen";
            this.btnFreiesFeld.Location = new System.Drawing.Point(80, 2);
            this.btnFreiesFeld.Name = "btnFreiesFeld";
            this.btnFreiesFeld.QuickInfo = "Fügt ein freies Eingabe-Feld hinzu.\r\nDieses kann datenbank unabhäng\r\nbefüllt werd" +
    "en.\r\nDatenbanken können diesen Wert\r\naber zur Abfrage benutzen.";
            this.btnFreiesFeld.Size = new System.Drawing.Size(64, 66);
            this.btnFreiesFeld.TabIndex = 1;
            this.btnFreiesFeld.Text = "Freies Feld hinzufügen";
            this.btnFreiesFeld.Click += new System.EventHandler(this.btnFreiesFeld_Click);
            // 
            // ConnectedFormula
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(974, 499);
            this.Name = "ConnectedFormula";
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
        private Controls.Button btnDatenbankhinzu;
        private Controls.Button btnFreiesFeld;
    }
}