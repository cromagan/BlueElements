using System.ComponentModel;
using System.Windows.Forms;
using BlueControls.Enums;
using ListBox = BlueControls.Controls.ListBox;

namespace BlueControls.Forms
{
    partial class FloatingInputBoxListBoxStyle
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lstbx = new BlueControls.Controls.ListBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // lstbx
            // 
            this.lstbx.AddAllowed = AddType.None;
            this.lstbx.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstbx.Location = new System.Drawing.Point(8, 8);
            this.lstbx.Name = "lstbx";
            this.lstbx.Size = new System.Drawing.Size(212, 233);
            this.lstbx.TabIndex = 0;
            this.lstbx.TabStop = false;
            this.lstbx.Text = "lstbx";
            this.lstbx.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.ListBox1_ItemClicked);
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // SelectionBoxSoft
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(228, 249);
            this.Controls.Add(this.lstbx);
            this.Name = "SelectionBoxSoft";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "SelectionBoxSoft";
            this.ResumeLayout(false);
        }
        #endregion

        private ListBox lstbx;
        private Timer timer1;
    }
}