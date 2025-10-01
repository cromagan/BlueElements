using BlueControls.Controls;
using System.ComponentModel;

namespace BeCreative {
    partial class Start {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) {
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
            this.btnDatenbank = new BlueControls.Controls.Button();
            this.btnFormular = new BlueControls.Controls.Button();
            this.btnLayout = new BlueControls.Controls.Button();
            this.btnTextEditor = new BlueControls.Controls.Button();
            this.btnBildEditor = new BlueControls.Controls.Button();
            this.btnHierachie = new BlueControls.Controls.Button();
            this.btnFormularAnsicht = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // btnDatenbank
            // 
            this.btnDatenbank.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big;
            this.btnDatenbank.ImageCode = "Tabelle|16";
            this.btnDatenbank.Location = new System.Drawing.Point(80, 16);
            this.btnDatenbank.Name = "btnDatenbank";
            this.btnDatenbank.Size = new System.Drawing.Size(64, 80);
            this.btnDatenbank.TabIndex = 0;
            this.btnDatenbank.Text = "Tabellen-Ansicht";
            this.btnDatenbank.Click += new System.EventHandler(this.btnDatenbank_Click);
            // 
            // btnFormular
            // 
            this.btnFormular.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big;
            this.btnFormular.ImageCode = "Anwendung";
            this.btnFormular.Location = new System.Drawing.Point(272, 104);
            this.btnFormular.Name = "btnFormular";
            this.btnFormular.Size = new System.Drawing.Size(64, 80);
            this.btnFormular.TabIndex = 1;
            this.btnFormular.Text = "Formular-Editor";
            this.btnFormular.Click += new System.EventHandler(this.btnFormular_Click);
            // 
            // btnLayout
            // 
            this.btnLayout.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big;
            this.btnLayout.ImageCode = "Layout|16";
            this.btnLayout.Location = new System.Drawing.Point(208, 104);
            this.btnLayout.Name = "btnLayout";
            this.btnLayout.Size = new System.Drawing.Size(64, 80);
            this.btnLayout.TabIndex = 2;
            this.btnLayout.Text = "Layout-Editor";
            this.btnLayout.Click += new System.EventHandler(this.btnLayout_Click);
            // 
            // btnTextEditor
            // 
            this.btnTextEditor.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big;
            this.btnTextEditor.ImageCode = "Textdatei|16";
            this.btnTextEditor.Location = new System.Drawing.Point(16, 104);
            this.btnTextEditor.Name = "btnTextEditor";
            this.btnTextEditor.Size = new System.Drawing.Size(64, 80);
            this.btnTextEditor.TabIndex = 3;
            this.btnTextEditor.Text = "Text-Editor";
            this.btnTextEditor.Click += new System.EventHandler(this.btnTextEditor_Click);
            // 
            // btnBildEditor
            // 
            this.btnBildEditor.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big;
            this.btnBildEditor.ImageCode = "Bild|16";
            this.btnBildEditor.Location = new System.Drawing.Point(80, 104);
            this.btnBildEditor.Name = "btnBildEditor";
            this.btnBildEditor.Size = new System.Drawing.Size(64, 80);
            this.btnBildEditor.TabIndex = 4;
            this.btnBildEditor.Text = "Bild-Editor";
            this.btnBildEditor.Click += new System.EventHandler(this.btnBildEditor_Click);
            // 
            // btnHierachie
            // 
            this.btnHierachie.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big;
            this.btnHierachie.ImageCode = "Hierarchie|16";
            this.btnHierachie.Location = new System.Drawing.Point(144, 104);
            this.btnHierachie.Name = "btnHierachie";
            this.btnHierachie.Size = new System.Drawing.Size(64, 80);
            this.btnHierachie.TabIndex = 5;
            this.btnHierachie.Text = "Hierachie-Editor";
            this.btnHierachie.Click += new System.EventHandler(this.btnHierachie_Click);
            // 
            // btnFormularAnsicht
            // 
            this.btnFormularAnsicht.AllowDrop = true;
            this.btnFormularAnsicht.ButtonStyle = BlueControls.Enums.ButtonStyle.Button_Big;
            this.btnFormularAnsicht.ImageCode = "Anwendung";
            this.btnFormularAnsicht.Location = new System.Drawing.Point(16, 16);
            this.btnFormularAnsicht.Name = "btnFormularAnsicht";
            this.btnFormularAnsicht.Size = new System.Drawing.Size(64, 80);
            this.btnFormularAnsicht.TabIndex = 6;
            this.btnFormularAnsicht.Text = "Formular-Ansicht";
            this.btnFormularAnsicht.Click += new System.EventHandler(this.btnFormularAnsicht_Click);
            // 
            // Start
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(347, 200);
            this.Controls.Add(this.btnFormularAnsicht);
            this.Controls.Add(this.btnHierachie);
            this.Controls.Add(this.btnBildEditor);
            this.Controls.Add(this.btnTextEditor);
            this.Controls.Add(this.btnLayout);
            this.Controls.Add(this.btnFormular);
            this.Controls.Add(this.btnDatenbank);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Start";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BeCreative! - (c) Christian Peter";
            this.ResumeLayout(false);

        }

        #endregion

        private Button btnDatenbank;
        private Button btnFormular;
        private Button btnLayout;
        private Button btnTextEditor;
        private Button btnBildEditor;
        private Button btnHierachie;
        private Button btnFormularAnsicht;
    }
}