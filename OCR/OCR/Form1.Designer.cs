namespace OCR
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnDo = new BlueElements.BlueControls.Button();
            this.picLetter = new System.Windows.Forms.PictureBox();
            this.txtLetter = new BlueElements.BlueControls.TextBox();
            this.capResult = new BlueElements.BlueControls.Caption();
            this.btnTraining = new BlueElements.BlueControls.Button();
            this.button1 = new BlueElements.BlueControls.Button();
            this.brainDrawer1 = new BlueElements.NeuronalNetwork.BrainDrawer();
            this.brainDrawer2 = new BlueElements.NeuronalNetwork.BrainDrawer();
            this.brainDrawer3 = new BlueElements.NeuronalNetwork.BrainDrawer();
            this.brainDrawer4 = new BlueElements.NeuronalNetwork.BrainDrawer();
            this.btnStop = new BlueElements.BlueControls.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picLetter)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDo
            // 
            this.btnDo.Location = new System.Drawing.Point(16, 8);
            this.btnDo.Name = "btnDo";
            this.btnDo.Size = new System.Drawing.Size(72, 40);
            this.btnDo.TabIndex = 0;
            this.btnDo.Text = "Do";
            this.btnDo.Click += new System.EventHandler(this.btnDo_Click);
            // 
            // picLetter
            // 
            this.picLetter.Location = new System.Drawing.Point(168, 8);
            this.picLetter.Name = "picLetter";
            this.picLetter.Size = new System.Drawing.Size(88, 80);
            this.picLetter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picLetter.TabIndex = 1;
            this.picLetter.TabStop = false;
            // 
            // txtLetter
            // 
            this.txtLetter.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtLetter.Location = new System.Drawing.Point(96, 8);
            this.txtLetter.Name = "txtLetter";
            this.txtLetter.Size = new System.Drawing.Size(40, 24);
            this.txtLetter.TabIndex = 2;
            this.txtLetter.Text = "A";
            // 
            // capResult
            // 
            this.capResult.CausesValidation = false;
            this.capResult.Location = new System.Drawing.Point(16, 96);
            this.capResult.Name = "capResult";
            this.capResult.Size = new System.Drawing.Size(240, 160);
            this.capResult.Text = "Result";
            // 
            // btnTraining
            // 
            this.btnTraining.Location = new System.Drawing.Point(8, 296);
            this.btnTraining.Name = "btnTraining";
            this.btnTraining.Size = new System.Drawing.Size(160, 32);
            this.btnTraining.TabIndex = 3;
            this.btnTraining.Text = "Training";
            this.btnTraining.Click += new System.EventHandler(this.btnTraining_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(8, 584);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(176, 40);
            this.button1.TabIndex = 4;
            this.button1.Text = "OR-Training";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // brainDrawer1
            // 
            this.brainDrawer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.brainDrawer1.Location = new System.Drawing.Point(264, 8);
            this.brainDrawer1.Name = "brainDrawer1";
            this.brainDrawer1.Size = new System.Drawing.Size(880, 184);
            this.brainDrawer1.TabIndex = 5;
            // 
            // brainDrawer2
            // 
            this.brainDrawer2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.brainDrawer2.Location = new System.Drawing.Point(264, 200);
            this.brainDrawer2.Name = "brainDrawer2";
            this.brainDrawer2.Size = new System.Drawing.Size(880, 184);
            this.brainDrawer2.TabIndex = 6;
            // 
            // brainDrawer3
            // 
            this.brainDrawer3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.brainDrawer3.Location = new System.Drawing.Point(264, 392);
            this.brainDrawer3.Name = "brainDrawer3";
            this.brainDrawer3.Size = new System.Drawing.Size(880, 184);
            this.brainDrawer3.TabIndex = 7;
            // 
            // brainDrawer4
            // 
            this.brainDrawer4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.brainDrawer4.Location = new System.Drawing.Point(264, 584);
            this.brainDrawer4.Name = "brainDrawer4";
            this.brainDrawer4.Size = new System.Drawing.Size(880, 184);
            this.brainDrawer4.TabIndex = 8;
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(176, 296);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(72, 32);
            this.btnStop.TabIndex = 9;
            this.btnStop.Text = "Stop";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1160, 777);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.brainDrawer1);
            this.Controls.Add(this.brainDrawer4);
            this.Controls.Add(this.brainDrawer3);
            this.Controls.Add(this.brainDrawer2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnTraining);
            this.Controls.Add(this.capResult);
            this.Controls.Add(this.txtLetter);
            this.Controls.Add(this.picLetter);
            this.Controls.Add(this.btnDo);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picLetter)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BlueElements.BlueControls.Button btnDo;
        private System.Windows.Forms.PictureBox picLetter;
        private BlueElements.BlueControls.TextBox txtLetter;
        private BlueElements.BlueControls.Caption capResult;
        private BlueElements.BlueControls.Button btnTraining;
        private BlueElements.BlueControls.Button button1;
        private BlueElements.NeuronalNetwork.BrainDrawer brainDrawer1;
        private BlueElements.NeuronalNetwork.BrainDrawer brainDrawer2;
        private BlueElements.NeuronalNetwork.BrainDrawer brainDrawer3;
        private BlueElements.NeuronalNetwork.BrainDrawer brainDrawer4;
        private BlueElements.BlueControls.Button btnStop;
    }
}

