using System.ComponentModel;
using System.Windows.Forms;
using BlueControls.Controls;

namespace BlueControls.Forms
{
    partial class QuickInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickInfo));
            this.capTXT = new BlueControls.Controls.Caption();
            this.timQI = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // capTXT
            // 
            this.capTXT.CausesValidation = false;
            this.capTXT.Location = new System.Drawing.Point(8, 8);
            this.capTXT.Name = "capTXT";
            this.capTXT.Size = new System.Drawing.Size(16, 16);
            this.capTXT.TextAnzeigeVerhalten = BlueControls.Enums.SteuerelementVerhalten.Steuerelement_Anpassen;
            this.capTXT.Translate = false;
            // 
            // timQI
            // 
            this.timQI.Interval = 500;
            this.timQI.Tick += new System.EventHandler(this.timQI_Tick);
            // 
            // QuickInfo
            // 
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(86, 65);
            this.Controls.Add(this.capTXT);
            this.Name = "QuickInfo";
            this.Text = "QuickInfo";
            this.ResumeLayout(false);
        }
        #endregion

        private Caption capTXT;
        private Timer timQI;
    }
}